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
                        // 1. We kept prevRow and nextRow because we are going to use them now!
                        Span<Rgb24> prevRow = accessor.GetRowSpan(y - 1);
                        Span<Rgb24> currentRow = accessor.GetRowSpan(y);
                        Span<Rgb24> nextRow = accessor.GetRowSpan(y + 1);

                        for (int x = 1; x < currentRow.Length - 1; x++)
                        {
                            // 1. Calculate the curvy diagonal wave path
            //double diagonalAxis = (x * 0.08) + (y * 0.08);
            //double curveWarp = Math.Sin((x * 0.05) - (y * 0.05) + frameIndex) * 4.0;
            
            //// This value smoothly bounces between -1.0 and 1.0
            //double waveValue = Math.Sin(diagonalAxis + curveWarp + frameIndex);

            //// 2. THE THRESHOLD CHECK (Creates the clean spaces)
            //if (waveValue < 0.3)
            //{
            //    continue; // Skip static entirely for this pixel ribbon
            //}


                            double r = currentRow[x].R;
                            double g = currentRow[x].G;
                            double b = currentRow[x].B;

                            // 2. Convert to YCbCr space properly
                            double yChan = 0.299 * r + 0.587 * g + 0.114 * b;
                            double cb = -0.168736 * r - 0.331264 * g + 0.5 * b;
                            double cr = 0.5 * r - 0.418688 * g - 0.081312 * b;


                            // --- FIX FOR THE GREEN TINT: ATTACK THE LUMINANCE CHANNEL ---
                            // Instead of driving the color components wild, we drive the brightness wild.
                            // This forces the static to stay gray/white/dark instead of green/pink.
                            //double baseAmplitude = 35.0 + (frameRandom.NextDouble() * 25.0);
                            //yChan += waveValue * baseAmplitude;
                            //yChan = Math.Clamp(yChan, 0, 255);



                            // 3. AMPLIFIED CHAOTIC CHROMA SHIFT
                            double baseAmplitude = 20.0 + (frameRandom.NextDouble() * 25.0);
                            cb += Math.Sin(frameIndex + (x * 0.1)) * baseAmplitude;
                            cr += Math.Cos(frameIndex + (y * 0.1)) * baseAmplitude;




                            // --- DIAGONALLY ALIGNED CHROMA SHIFT (Bottom-Left to Top-Right) ---
                            // By adding X and Y together inside the trigonometric function, we tilt the wave axis.
                            // Adjust the 0.1 multipliers to change the thickness/density of the diagonal bands.
                            //double diagonalPosition = (x * 0.1) + (y * 0.1);

                            // Adding +frameIndex to one and -frameIndex to the other makes the diagonal lines 
                            // slide across the screen in opposing patterns for an active shimmering feel.
                            //cb += Math.Sin(diagonalPosition + frameIndex) * baseAmplitude;
                            //cr += Math.Cos(diagonalPosition - frameIndex) * baseAmplitude;


                            // --- CURVY DIAGONAL SHIFT ---
                            // 1. Calculate a base diagonal vector
                            double diagonalAxis = (x * 0.08) + (y * 0.08);

                            // 2. Introduce a secondary perpendicular warp wave (this bends the straight lines into curves)
                            // Modifying the 0.1 changes the frequency of the curves, and the 4.0 changes how deep the bends are.
                            double curveWarp = Math.Sin((x * 0.05) - (y * 0.05) + frameIndex) * 4.0;

                            // 3. Combine them to get the final curvy position
                            double finalCurvyPosition = diagonalAxis + curveWarp;

                            // 4. Apply the shifting math to the color components
                            // Adding or subtracting frameIndex makes these curved bands actively ripple like liquid static
                            cb += Math.Sin(finalCurvyPosition + frameIndex) * baseAmplitude;
                            cr += Math.Cos(finalCurvyPosition - frameIndex) * baseAmplitude;




                            // 4. FIXED RGB CONVERSION (Brought back the 128 offsets)
                            int baseR = (int)(yChan + 1.402 * cr);
                            int baseG = (int)(yChan - 0.344136 * cb - 0.714136 * cr);
                            int baseB = (int)(yChan + 1.772 * cb);

                            // 5. SMART NOISE AMPLIFICATION (Using the surrounding rows)
                            // We calculate local contrast using the pixels above, below, left, and right
                            int localContrast = Math.Abs(currentRow[x].R - currentRow[x - 1].R) +
                                                Math.Abs(currentRow[x].R - currentRow[x + 1].R) +
                                                Math.Abs(currentRow[x].R - prevRow[x].R) +
                                                Math.Abs(currentRow[x].R - nextRow[x].R);

                            // If it's a flat area (like sky), apply normal heavy static (limit = 45).
                            // If it's a high-detail area (like hair/textures), CRANK UP the static (limit = 85)
                            // to aggressively shred edge detection where AI looks closest!
                            int dynamicLimit = (localContrast < 15) ? 45 : 85;

                            int staticNoise = 0;
                            if (frameRandom.NextDouble() < 0.05) // 15% static chance
                            {
                                staticNoise = frameRandom.Next(-dynamicLimit, dynamicLimit + 1);
                            }

                            // 6. Combine layers and clamp to safe byte bounds
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
