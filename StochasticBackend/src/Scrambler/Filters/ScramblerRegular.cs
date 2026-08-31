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
            // 1. Load the original image (supports PNG, JPEG, etc.)
            using Image<Rgba32> sourceImage = Image.Load<Rgba32>(inputPath);

            // 2. Create the base animated GIF container using the dimensions of the source
            using Image<Rgba32> gifOutput = new Image<Rgba32>(sourceImage.Width, sourceImage.Height);

            // Set GIF metadata to loop infinitely
            var gifMetadata = gifOutput.Metadata.GetGifMetadata();
            gifMetadata.RepeatCount = 0;

            Random random = new Random();

            // 3. Generate 12 distinct frames
            for (int frameIndex = 0; frameIndex < TOTAL_FRAMES; frameIndex++)
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

                            // Convert RGB to YCbCr math manually (Fast math, no AI needed)
                            // Y is Luminance (brightness), Cb/Cr are chroma (color) components
                            double yComponent = 0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B;
                            double cbComponent = 128 - 0.168736 * pixel.R - 0.331264 * pixel.G + 0.5 * pixel.B;
                            double crComponent = 128 + 0.5 * pixel.R - 0.418688 * pixel.G - 0.081312 * pixel.B;

                            // --- FREQUENCY STATIC MATH ---
                            // Generates high-frequency waves based on X/Y grids and the frame index
                            double frequencyFactor = Math.Sin(x * 0.5 + frameIndex) * Math.Cos(y * 0.5 - frameIndex);

                            // --- SALT & PEPPER STATIC SHIFT ---
                            // 5% chance per pixel to create sharp, unpredictable static spikes
                            double staticSpike = 0;
                            if (random.NextDouble() < 0.05)
                            {
                                staticSpike = random.Next(-35, 35);
                            }

                            // Combine frequency waves and random spikes into the Luminance channel
                            // This attacks how AI feature maps detect edges and lighting gradients
                            yComponent += (frequencyFactor * 20.0) + staticSpike;

                            // Clamp Y value to valid byte range (0-255)
                            yComponent = Math.Clamp(yComponent, 0, 255);

                            // --- COLOR CHANNEL SHIFTING ---
                            // Subtly desynchronize color channels based on frame phase
                            cbComponent += Math.Sin(frameIndex + x) * 8.0;
                            crComponent += Math.Cos(frameIndex + y) * 8.0;
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
                frameMetadata.FrameDelay = 3;

                // Add the mutated frame into our final GIF container
                gifOutput.Frames.AddFrame(currentFrame.Frames.RootFrame);
            }

            // 4. Remove the very first empty frame placeholder created during initialization
            gifOutput.Frames.RemoveFrame(0);

            // 5. Save the final file as an animated GIF
            gifOutput.SaveAsGif(outputPath);
        }
    }
}
