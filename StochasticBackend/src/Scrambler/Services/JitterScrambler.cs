using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace StochasticBackend.src.Scrambler.Services
{
    public class JitterScrambler/*: IScrambler*/
    {
        private const int TotalFrames = 12;

        public static void PoisonImage(string inputPath, string outputPath, int jitterBlockSize = 2)
        {
            // 1. Load original image safely
            using var sourceImage = Image.Load<Rgb24>(inputPath);

            // 2. Create the base multi-frame GIF container
            using var gifOutput = new Image<Rgb24>(sourceImage.Width, sourceImage.Height);
            gifOutput.Metadata.GetGifMetadata().RepeatCount = 0; // Infinite loop

            var random = new Random();

            // 3. Loop to generate 12 completely distinct, shifting frames
            for (int frameIndex = 0; frameIndex < TotalFrames; frameIndex++)
            {
                // Clone the original to manipulate a fresh copy for this frame
                var currentFrame = sourceImage.Clone();

                // LAYER 1: Macro-Block Spatial Jitter (Dynamic per frame using index)
                ApplyDynamicMacroJitter(currentFrame, jitterBlockSize, frameIndex);

                // LAYER 2 & 3: Visible Chrominance Tear + Heavy Retro Static
                currentFrame.ProcessPixelRows(accessor =>
                {
                    for (int y = 1; y < accessor.Height - 1; y++)
                    {
                        Span<Rgb24> prevRow = accessor.GetRowSpan(y - 1);
                        Span<Rgb24> currentRow = accessor.GetRowSpan(y);
                        Span<Rgb24> nextRow = accessor.GetRowSpan(y + 1);

                        for (int x = 1; x < currentRow.Length - 1; x++)
                        {
                            double r = currentRow[x].R;
                            double g = currentRow[x].G;
                            double b = currentRow[x].B;

                            // Convert to YCbCr space
                            double yChan = 0.299 * r + 0.587 * g + 0.114 * b;
                            double cb = -0.168736 * r - 0.331264 * g + 0.5 * b;
                            double cr = 0.5 * r - 0.418688 * g - 0.081312 * b;

                            // --- UPGRADED CHROMA SHIFT: Animating Color Glitch Effect ---
                            // Incorporating frameIndex ensures the color distortions move organically
                            cb += Math.Sin(frameIndex + (x * 0.1)) * 30.0; // Noticeable color bleed
                            cr += Math.Cos(frameIndex + (y * 0.1)) * 30.0;

                            // Revert back to RGB space
                            int baseR = (int)(yChan + 1.402 * cr);
                            int baseG = (int)(yChan - 0.344136 * cb - 0.714136 * cr);
                            int baseB = (int)(yChan + 1.772 * cb);

                            // --- UPGRADED RANDOM STATIC: Visible TV Snow ---
                            // We remove the texture masking because we WANT the static visible everywhere
                            // 15% chance of intense black/white static dots mapping to old TV signals
                            int staticNoise = 0;
                            if (random.NextDouble() < 0.15)
                            {
                                staticNoise = random.Next(-65, 65);
                            }

                            // Combine layers and clamp to safe byte bounds
                            byte finalR = (byte)Math.Clamp(baseR + staticNoise, 0, 255);
                            byte finalG = (byte)Math.Clamp(baseG + staticNoise, 0, 255);
                            byte finalB = (byte)Math.Clamp(baseB + staticNoise, 0, 255);

                            currentRow[x] = new Rgb24(finalR, finalG, finalB);
                        }
                    }
                });

                // Set a crunchy frame rate (approx 70ms per frame) for visible heavy animation
                currentFrame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = 7;

                // Strip metadata tracking on every frame
                currentFrame.Metadata.ExifProfile = null;
                currentFrame.Metadata.IptcProfile = null;
                currentFrame.Metadata.XmpProfile = null;

                // Push the processed frame into our final animated compilation
                gifOutput.Frames.AddFrame(currentFrame.Frames.RootFrame);
            }

            // Remove initial blank canvas frame and save out file
            gifOutput.Frames.RemoveFrame(0);
            gifOutput.SaveAsGif(outputPath);
        }

        private static void ApplyDynamicMacroJitter(Image<Rgb24> image, int blockSize, int frameIndex)
        {
            int step = blockSize * 2;
            // Offsetting the loop starting point by frameIndex causes the macro-blocks 
            // to dance/vibrate rather than standing completely still across frames.
            int offset = frameIndex % blockSize;

            for (int y = offset; y < image.Height - step; y += step)
            {
                for (int x = offset; x < image.Width - step; x += step)
                {
                    for (int row = 0; row < blockSize; row++)
                    {
                        for (int col = 0; col < blockSize; col++)
                        {
                            Rgb24 p1 = image[x + col, y + row];
                            Rgb24 p2 = image[x + blockSize + col, y + blockSize + row];

                            image[x + col, y + row] = p2;
                            image[x + blockSize + col, y + blockSize + row] = p1;
                        }
                    }
                }
            }
        }
    }
}
