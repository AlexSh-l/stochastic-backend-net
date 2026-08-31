using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using StochasticBackend.src.Scrambler.Configuration;

namespace StochasticBackend.src.Scrambler.Filters
{
    public class JitterRandomAreaScrambler: IScrambler
    {
        private const int TotalFrames = 12;

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
            for (int frameIndex = 0; frameIndex < TotalFrames; frameIndex++)
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
                            // 1. Calculate the curvy diagonal wave path
                            double diagonalAxis = (x * 0.08) + (y * 0.08);
                            double curveWarp = Math.Sin((x * 0.05) - (y * 0.05) + frameIndex) * 4.0;

                            // This value smoothly bounces between -1.0 and 1.0
                            double waveValue = Math.Sin(diagonalAxis + curveWarp + frameIndex);

                            // 2. THE THRESHOLD CHECK (Creates the clean spaces)
                            if (waveValue < 0.3)
                            {
                                continue; // Skip entirely, leaving the original image untouched
                            }

                            // 3. Load original RGB components
                            double r = currentRow[x].R;
                            double g = currentRow[x].G;
                            double b = currentRow[x].B;

                            // --- TRANSLUCENT DARKENING (OPACITY LENS) ---
                            // We scale down the original RGB values to darken them.
                            // 0.70 means the pixel keeps 70% of its original color (effectively a 30% dark shadow).
                            // Lower this to 0.50 to make the bands darker; raise it to 0.85 to make them more transparent.
                            double darkenFactor = 0.70;

                            int baseR = (int)(r * darkenFactor);
                            int baseG = (int)(g * darkenFactor);
                            int baseB = (int)(b * darkenFactor);

                            // 4. SMART MONOCHROME NOISE AMPLIFICATION (Within the bands only)
                            int localContrast = Math.Abs(currentRow[x].R - currentRow[x - 1].R) +
                                                Math.Abs(currentRow[x].R - currentRow[x + 1].R) +
                                                Math.Abs(currentRow[x].R - prevRow[x].R) +
                                                Math.Abs(currentRow[x].R - nextRow[x].R);

                            int dynamicLimit = (localContrast < 15) ? 35 : 65;

                            int staticNoise = 0;
                            if (frameRandom.NextDouble() < 0.12) // 12% chance of a static dot inside the dark bands
                            {
                                int rawNoise = frameRandom.Next(-dynamicLimit, dynamicLimit + 1);
                                staticNoise = (rawNoise / 15) * 15; // Kept your compression step optimization!
                            }

                            // 5. Combine the darkened colors and the monochrome static noise, then clamp safely
                            byte finalR = (byte)Math.Clamp(baseR + staticNoise, 0, 255);
                            byte finalG = (byte)Math.Clamp(baseG + staticNoise, 0, 255);
                            byte finalB = (byte)Math.Clamp(baseB + staticNoise, 0, 255);

                            currentRow[x] = new Rgb24(finalR, finalG, finalB);
                        }
                    }
                });

                // Set a crunchy frame rate delay (approx 70ms) for highly visible heavy animation
                currentFrame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = 9;

                // Strip metadata tracking profiles
                currentFrame.Metadata.ExifProfile = null;
                currentFrame.Metadata.IptcProfile = null;
                currentFrame.Metadata.XmpProfile = null;

                // Push the processed frame into our final animated compilation
                gifOutput.Frames.AddFrame(currentFrame.Frames.RootFrame);
            }

            // Remove initial blank canvas frame and save out file
            gifOutput.Frames.RemoveFrame(0);

            // --- UPGRADED COMPRESSION ENCODER ---
            var gifEncoder = new GifEncoder
            {
                // WuQuantizer is the standard modern choice for palette quantization.
                // Reducing MaxColors to 128 merges similar pixels to compress the GIF size.
                Quantizer = new WuQuantizer(new QuantizerOptions
                {
                    MaxColors = 128,             // Cuts file size in half compared to 256 colors
                    Dither = null,               // Disabling dithering ensures clean compression streams
                    TransparentColorMode = TransparentColorMode.Preserve
                }),
            };

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
