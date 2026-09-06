using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using StochasticBackend.src.Scrambler.Configuration;

namespace StochasticBackend.src.Scrambler.Filters
{
    public class JitterRandomWaveScrambler: IScrambler
    {
        private const int TOTAL_FRAMES = 12;

        public async Task PoisonImageAsync(string inputPath, string outputPath)
        {
            await Task.Run(() => PoisonImage(inputPath, outputPath));
        }

        public void PoisonImage(string inputPath, string outputPath)
        {
            using var sourceImage = Image.Load<Rgb24>(inputPath);

            using var gifOutput = new Image<Rgb24>(sourceImage.Width, sourceImage.Height);
            gifOutput.Metadata.GetGifMetadata().RepeatCount = 0;

            for (int frameIndex = 0; frameIndex < TOTAL_FRAMES; frameIndex++)
            {
                var currentFrame = sourceImage.Clone();

                Random frameRandom = new Random(frameIndex);

                int dynamicBlockSize = frameRandom.Next(2, 5);
                ApplyChaoticJitterPerFrame(currentFrame, dynamicBlockSize, frameRandom);

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

                            // Convert to YCbCr
                            double yChan = 0.299 * r + 0.587 * g + 0.114 * b;
                            double cb = -0.168736 * r - 0.331264 * g + 0.5 * b;
                            double cr = 0.5 * r - 0.418688 * g - 0.081312 * b;

                            // Chroma shift
                            double baseAmplitude = 20.0 + (frameRandom.NextDouble() * 25.0);
                            cb += Math.Sin(frameIndex + (x * 0.1)) * baseAmplitude;
                            cr += Math.Cos(frameIndex + (y * 0.1)) * baseAmplitude;

                            // Diagonal shift
                            double diagonalAxis = (x * 0.08) + (y * 0.08);

                            // Secondary perpendicular warp wave (this bends the straight lines into curves)
                            // Modifying the 0.1 changes the frequency of the curves, and the 4.0 changes how deep the bends are.
                            double curveWarp = Math.Sin((x * 0.05) - (y * 0.05) + frameIndex) * 4.0;

                            double finalCurvyPosition = diagonalAxis + curveWarp;

                            cb += Math.Sin(finalCurvyPosition + frameIndex) * baseAmplitude;
                            cr += Math.Cos(finalCurvyPosition - frameIndex) * baseAmplitude;

                            int baseR = (int)(yChan + 1.402 * cr);
                            int baseG = (int)(yChan - 0.344136 * cb - 0.714136 * cr);
                            int baseB = (int)(yChan + 1.772 * cb);

                            int localContrast = Math.Abs(currentRow[x].R - currentRow[x - 1].R) +
                                                Math.Abs(currentRow[x].R - currentRow[x + 1].R) +
                                                Math.Abs(currentRow[x].R - prevRow[x].R) +
                                                Math.Abs(currentRow[x].R - nextRow[x].R);

                            // If it's a flat area (like sky), apply normal heavy static (limit = 45).
                            // If it's a high-detail area (like hair/textures), CRANK UP the static (limit = 85)
                            int dynamicLimit = (localContrast < 15) ? 45 : 85;

                            int staticNoise = 0;
                            if (frameRandom.NextDouble() < 0.05)
                            {
                                staticNoise = frameRandom.Next(-dynamicLimit, dynamicLimit + 1);
                            }

                            byte finalR = (byte)Math.Clamp(baseR + staticNoise, 0, 255);
                            byte finalG = (byte)Math.Clamp(baseG + staticNoise, 0, 255);
                            byte finalB = (byte)Math.Clamp(baseB + staticNoise, 0, 255);

                            currentRow[x] = new Rgb24(finalR, finalG, finalB);
                        }
                    }
                });

                currentFrame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = 9;

                currentFrame.Metadata.ExifProfile = null;
                currentFrame.Metadata.IptcProfile = null;
                currentFrame.Metadata.XmpProfile = null;

                gifOutput.Frames.AddFrame(currentFrame.Frames.RootFrame);
            }

            gifOutput.Frames.RemoveFrame(0);

            var gifEncoder = new GifEncoder
            {
                Quantizer = new WuQuantizer(new QuantizerOptions
                {
                    MaxColors = 128,
                    Dither = null,
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
