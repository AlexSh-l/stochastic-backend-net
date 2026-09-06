using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StochasticBackend.src.Scrambler.Configuration;

namespace StochasticBackend.src.Scrambler.Filters
{
    public class ScramblerRegular: IScrambler
    {
        private const int TOTAL_FRAMES = 12;

        public async Task PoisonImageAsync(string inputPath, string outputPath)
        {
            await Task.Run(() => PoisonImage(inputPath, outputPath));
        }

        public void PoisonImage(string inputPath, string outputPath)
        {
            using Image<Rgba32> sourceImage = Image.Load<Rgba32>(inputPath);

            using Image<Rgba32> gifOutput = new Image<Rgba32>(sourceImage.Width, sourceImage.Height);

            var gifMetadata = gifOutput.Metadata.GetGifMetadata();
            gifMetadata.RepeatCount = 0;

            Random random = new Random();

            for (int frameIndex = 0; frameIndex < TOTAL_FRAMES; frameIndex++)
            {
                Image<Rgba32> currentFrame = sourceImage.Clone();

                currentFrame.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                        for (int x = 0; x < accessor.Width; x++)
                        {
                            Rgba32 pixel = pixelRow[x];

                            // Convert RGB to YCbCr
                            double yComponent = 0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B;
                            double cbComponent = 128 - 0.168736 * pixel.R - 0.331264 * pixel.G + 0.5 * pixel.B;
                            double crComponent = 128 + 0.5 * pixel.R - 0.418688 * pixel.G - 0.081312 * pixel.B;

                            // Static waves
                            double frequencyFactor = Math.Sin(x * 0.5 + frameIndex) * Math.Cos(y * 0.5 - frameIndex);

                            // Salt and Pepper (5%)
                            double staticSpike = 0;
                            if (random.NextDouble() < 0.05)
                            {
                                staticSpike = random.Next(-35, 35);
                            }

                            
                            yComponent += (frequencyFactor * 20.0) + staticSpike;
                            yComponent = Math.Clamp(yComponent, 0, 255);

                            // Color shift
                            cbComponent += Math.Sin(frameIndex + x) * 8.0;
                            crComponent += Math.Cos(frameIndex + y) * 8.0;
                            cbComponent = Math.Clamp(cbComponent, 0, 255);
                            crComponent = Math.Clamp(crComponent, 0, 255);

                            // Convert YCbCr back to RGB
                            int r = (int)(yComponent + 1.402 * (crComponent - 128));
                            int g = (int)(yComponent - 0.344136 * (cbComponent - 128) - 0.714136 * (crComponent - 128));
                            int b = (int)(yComponent + 1.772 * (cbComponent - 128));

                            pixelRow[x] = new Rgba32(
                                (byte)Math.Clamp(r, 0, 255),
                                (byte)Math.Clamp(g, 0, 255),
                                (byte)Math.Clamp(b, 0, 255),
                                pixel.A
                            );
                        }
                    }
                });

                var frameMetadata = currentFrame.Frames.RootFrame.Metadata.GetGifMetadata();
                frameMetadata.FrameDelay = 3;

                currentFrame.Metadata.ExifProfile = null;
                currentFrame.Metadata.IptcProfile = null;
                currentFrame.Metadata.XmpProfile = null;

                gifOutput.Frames.AddFrame(currentFrame.Frames.RootFrame);
            }

            gifOutput.Frames.RemoveFrame(0);

            gifOutput.SaveAsGif(outputPath);
        }
    }
}
