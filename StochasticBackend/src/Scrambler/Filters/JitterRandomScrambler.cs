using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StochasticBackend.src.Scrambler.Configuration;

namespace StochasticBackend.src.Scrambler.Filters
{
    public class JitterRandomScrambler: IScrambler
    {
        private const int TOTAL_FRAMES = 12;

        public async Task PoisonImageAsync(string inputPath, string outputPath)
        {
            await Task.Run(() => PoisonImage(inputPath, outputPath));
        }

        public void PoisonImage(string inputPath, string outputPath)
        {
            // 1. Load original image safely into memory
            using var sourceImage = Image.Load<Rgb24>(inputPath);

            // 2. Create the base multi-frame GIF container
            using var gifOutput = new Image<Rgb24>(sourceImage.Width, sourceImage.Height);
            gifOutput.Metadata.GetGifMetadata().RepeatCount = 0; // Infinite loop

            // 3. Generate 12 distinctly chaotic frames
            for (int frameIndex = 0; frameIndex < TOTAL_FRAMES; frameIndex++)
            {
                // Clone the original to manipulate a fresh copy for this frame
                var currentFrame = sourceImage.Clone();

                // CRITICAL: Seed the Random object with the frame index!
                // This ensures Frame 1 always generates the exact same 'Chaos 1' pattern,
                // giving the GIF smooth, crisp temporal playback instead of complete blur.
                Random frameRandom = new Random(frameIndex);

                // LAYER 1: Your Chaotic Macro-Block Jitter with non-uniform step sizes
                int dynamicBlockSize = frameRandom.Next(2, 5);
                ApplyChaoticJitterPerFrame(currentFrame, dynamicBlockSize, frameRandom);

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
                            double cb = 128 - 0.168736 * r - 0.331264 * g + 0.5 * b;
                            double cr = 128 + 0.5 * r - 0.418688 * g - 0.081312 * b;

                            // --- AMPLIFIED CHAOTIC CHROMA SHIFT ---
                            // Combines your dynamic amplitude shift with a moving wave
                            double baseAmplitude = 20.0 + (frameRandom.NextDouble() * 25.0);
                            cb += Math.Sin(frameIndex + (x * 0.1)) * baseAmplitude;
                            cr += Math.Cos(frameIndex + (y * 0.1)) * baseAmplitude;

                            // Revert back to RGB space
                            int baseR = (int)(yChan + 1.402 * cr);
                            int baseG = (int)(yChan - 0.344136 * cb - 0.714136 * cr);
                            int baseB = (int)(yChan + 1.772 * cb);

                            // --- HEAVY TV SNOW STATIC LAYER ---
                            // 15% chance of intense black/white static dots mapping to old TV signals
                            int staticNoise = 0;
                            if (frameRandom.NextDouble() < 0.15)
                            {
                                staticNoise = frameRandom.Next(-65, 65);
                            }

                            // Combine layers and clamp to safe byte bounds
                            byte finalR = (byte)Math.Clamp(baseR + staticNoise, 0, 255);
                            byte finalG = (byte)Math.Clamp(baseG + staticNoise, 0, 255);
                            byte finalB = (byte)Math.Clamp(baseB + staticNoise, 0, 255);

                            currentRow[x] = new Rgb24(finalR, finalG, finalB);
                        }
                    }
                });

                // Set a crunchy frame rate delay (approx 70ms) for highly visible heavy animation
                currentFrame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = 7;

                // Strip metadata tracking profiles
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

        private static void ApplyChaoticJitterPerFrame(Image<Rgb24> image, int baseBlockSize, Random frameRandom)
        {
            int y = 0;
            while (y < image.Height - 10)
            {
                int stepY = baseBlockSize * 2 + frameRandom.Next(-1, 2);
                stepY = Math.Max(2, stepY);

                int x = 0;
                while (x < image.Width - 10)
                {
                    int stepX = baseBlockSize * 2 + frameRandom.Next(-1, 2);
                    stepX = Math.Max(2, stepX);

                    for (int row = 0; row < baseBlockSize && (y + baseBlockSize + row) < image.Height; row++)
                    {
                        for (int col = 0; col < baseBlockSize && (x + baseBlockSize + col) < image.Width; col++)
                        {
                            Rgb24 p1 = image[x + col, y + row];
                            Rgb24 p2 = image[x + baseBlockSize + col, y + baseBlockSize + row];

                            image[x + col, y + row] = p2;
                            image[x + baseBlockSize + col, y + baseBlockSize + row] = p1;
                        }
                    }
                    x += stepX;
                }
                y += stepY;
            }
        }
    }
}
