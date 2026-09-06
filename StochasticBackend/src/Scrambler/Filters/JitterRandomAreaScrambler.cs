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
            using var sourceImage = Image.Load<Rgb24>(inputPath);

            using var gifOutput = new Image<Rgb24>(sourceImage.Width, sourceImage.Height);
            gifOutput.Metadata.GetGifMetadata().RepeatCount = 0;

            for (int frameIndex = 0; frameIndex < TotalFrames; frameIndex++)
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
                            // Diagonal wave
                            double diagonalAxis = (x * 0.08) + (y * 0.08);
                            double curveWarp = Math.Sin((x * 0.05) - (y * 0.05) + frameIndex) * 4.0;

                            double waveValue = Math.Sin(diagonalAxis + curveWarp + frameIndex);

                            if (waveValue < 0.3)
                            {
                                continue;
                            }

                            double r = currentRow[x].R;
                            double g = currentRow[x].G;
                            double b = currentRow[x].B;

                            // Darkening
                            // Lower this to 0.50 to make the bands darker; raise it to 0.85 to make them more transparent.
                            double darkenFactor = 0.70;

                            int baseR = (int)(r * darkenFactor);
                            int baseG = (int)(g * darkenFactor);
                            int baseB = (int)(b * darkenFactor);

                            int localContrast = Math.Abs(currentRow[x].R - currentRow[x - 1].R) +
                                                Math.Abs(currentRow[x].R - currentRow[x + 1].R) +
                                                Math.Abs(currentRow[x].R - prevRow[x].R) +
                                                Math.Abs(currentRow[x].R - nextRow[x].R);

                            int dynamicLimit = (localContrast < 15) ? 35 : 65;

                            int staticNoise = 0;
                            if (frameRandom.NextDouble() < 0.12)
                            {
                                int rawNoise = frameRandom.Next(-dynamicLimit, dynamicLimit + 1);
                                staticNoise = (rawNoise / 15) * 15;
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
