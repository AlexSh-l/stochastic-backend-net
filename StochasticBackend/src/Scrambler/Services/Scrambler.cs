using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace StochasticBackend.src.Scrambler.Services
{
    public class ScramblerHeavy/*: IScrambler*/
    {
        private const int TotalFrames = 12;

        public static void PoisonImage(string inputImagePath, string outputGifPath)
        {
            // 1. Load the original image (supports PNG, JPEG, etc.)
            using Image<Rgba32> sourceImage = Image.Load<Rgba32>(inputImagePath);

            // 2. Create the base animated GIF container using the dimensions of the source
            using Image<Rgba32> gifOutput = new Image<Rgba32>(sourceImage.Width, sourceImage.Height);

            // Set GIF metadata to loop infinitely
            var gifMetadata = gifOutput.Metadata.GetGifMetadata();
            gifMetadata.RepeatCount = 0;

            Random random = new Random();

            // 3. Generate 12 distinct frames
            for (int frameIndex = 0; frameIndex < TotalFrames; frameIndex++)
            {
                // Clone the original image to mutate for this specific frame
                Image<Rgba32> currentFrame = sourceImage.Clone();

                // Process pixels row by row using high-performance Spans
                currentFrame.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                        for (int x = 0; x < accessor.Width; x++)
                        {
                            Rgba32 pixel = pixelRow[x];

                            // Convert RGB to YCbCr math manually
                            double yComponent = 0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B;
                            double cbComponent = 128 - 0.168736 * pixel.R - 0.331264 * pixel.G + 0.5 * pixel.B;
                            double crComponent = 128 + 0.5 * pixel.R - 0.418688 * pixel.G - 0.081312 * pixel.B;

                            // 1. HEAVY FREQUENCY STATIC (Aggressive wave patterns)
                            // Decreasing the multiplier inside Math.Sin makes the static blocks larger and chunkier
                            double frequencyFactor = Math.Sin(x * 0.2 + frameIndex) * Math.Cos(y * 0.2 - frameIndex);

                            // 2. HEAVY SALT & PEPPER SPIKES (15% of pixels will be stark black or white dots)
                            double staticSpike = 0;
                            if (random.NextDouble() < 0.15) // Up from 5% to 15%
                            {
                                // Generates intense dark/light glitches
                                staticSpike = random.Next(-70, 70);
                            }

                            // Inject the heavy wave texture and aggressive spikes into the brightness
                            yComponent += (frequencyFactor * 45.0) + staticSpike; // Up from 20.0 to 45.0
                            yComponent = Math.Clamp(yComponent, 0, 255);

                            // 3. VISIBLE CHROMATIC SHIMMER (Intentionally tearing the colors apart)
                            // This creates a noticeable color-bleeding/glitch effect that dances across frames
                            cbComponent += Math.Sin(frameIndex + (x * 0.1)) * 35.0; // Up from 8.0 to 35.0
                            crComponent += Math.Cos(frameIndex + (y * 0.1)) * 35.0; // Up from 8.0 to 35.0
                            cbComponent = Math.Clamp(cbComponent, 0, 255);
                            crComponent = Math.Clamp(crComponent, 0, 255);

                            // Convert YCbCr back to RGB
                            int r = (int)(yComponent + 1.402 * (crComponent - 128));
                            int g = (int)(yComponent - 0.344136 * (cbComponent - 128) - 0.714136 * (crComponent - 128));
                            int b = (int)(yComponent + 1.772 * (cbComponent - 128));

                            // Reassign pixel values safely clamped
                            pixelRow[x] = new Rgba32(
                                (byte)Math.Clamp(r, 0, 255),
                                (byte)Math.Clamp(g, 0, 255),
                                (byte)Math.Clamp(b, 0, 255),
                                pixel.A // Keep original transparency intact
                            );
                        }
                    }
                });

                // Set the metadata delay for the frame (30ms = fast shimmering effect)
                var frameMetadata = currentFrame.Frames.RootFrame.Metadata.GetGifMetadata();
                //frameMetadata.FrameDelay = 3;
                frameMetadata.FrameDelay = 7;

                // Add the mutated frame into our final GIF container
                gifOutput.Frames.AddFrame(currentFrame.Frames.RootFrame);
            }

            // 4. Remove the very first empty frame placeholder created during initialization
            gifOutput.Frames.RemoveFrame(0);

            // 5. Save the final file as an animated GIF
            gifOutput.SaveAsGif(outputGifPath);
        }
    }
}
