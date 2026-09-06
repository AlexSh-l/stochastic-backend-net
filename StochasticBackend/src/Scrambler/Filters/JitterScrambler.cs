using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StochasticBackend.src.Scrambler.Configuration;

namespace StochasticBackend.src.Scrambler.Filters
{
    public class JitterScrambler: IScrambler
    {
        private const int TOTAL_FRAMES = 12;
        private const int JITTER_BLOCK_SIZE = 2;

        public async Task PoisonImageAsync(string inputPath, string outputPath)
        {
            await Task.Run(() => PoisonImage(inputPath, outputPath));
        }

        public void PoisonImage(string inputPath, string outputPath)
        {
            using var sourceImage = Image.Load<Rgb24>(inputPath);

            using var gifOutput = new Image<Rgb24>(sourceImage.Width, sourceImage.Height);
            gifOutput.Metadata.GetGifMetadata().RepeatCount = 0;

            var random = new Random();

            for (int frameIndex = 0; frameIndex < TOTAL_FRAMES; frameIndex++)
            {
                var currentFrame = sourceImage.Clone();

                ApplyDynamicMacroJitter(currentFrame, JITTER_BLOCK_SIZE, frameIndex);

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

                            // Chroma shift glitch effect
                            cb += Math.Sin(frameIndex + (x * 0.1)) * 30.0;
                            cr += Math.Cos(frameIndex + (y * 0.1)) * 30.0;

                            int baseR = (int)(yChan + 1.402 * cr);
                            int baseG = (int)(yChan - 0.344136 * cb - 0.714136 * cr);
                            int baseB = (int)(yChan + 1.772 * cb);

                            // Random static
                            int staticNoise = 0;
                            if (random.NextDouble() < 0.15)
                            {
                                staticNoise = random.Next(-65, 65);
                            }

                            byte finalR = (byte)Math.Clamp(baseR + staticNoise, 0, 255);
                            byte finalG = (byte)Math.Clamp(baseG + staticNoise, 0, 255);
                            byte finalB = (byte)Math.Clamp(baseB + staticNoise, 0, 255);

                            currentRow[x] = new Rgb24(finalR, finalG, finalB);
                        }
                    }
                });

                currentFrame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = 7;

                currentFrame.Metadata.ExifProfile = null;
                currentFrame.Metadata.IptcProfile = null;
                currentFrame.Metadata.XmpProfile = null;

                gifOutput.Frames.AddFrame(currentFrame.Frames.RootFrame);
            }

            gifOutput.Frames.RemoveFrame(0);
            gifOutput.SaveAsGif(outputPath);
        }

        private static void ApplyDynamicMacroJitter(Image<Rgb24> image, int blockSize, int frameIndex)
        {
            int step = blockSize * 2;

            // Offset the loop starting point by frameIndex for vibration across frames
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
