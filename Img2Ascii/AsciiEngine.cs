using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Text;

namespace Img2Ascii
{
    public class AsciiEngine
    {
        // Default ASCII ramps
        public static readonly string RampStandard = "@%#*+=-:. ";
        public static readonly string RampComplex = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\\|()1{}[]?-_+~<>i!lI;:,\"^`'. ";
        public static readonly string RampMinimal = "#@. ";

        /// <summary>
        /// Converts an image from file path to ASCII string.
        /// </summary>
        public static string Convert(string filePath, int width, string ramp, bool invert = false)
        {
            if (!File.Exists(filePath)) return "Error: File not found.";

            try
            {
                using (Image<Rgba32> image = Image.Load<Rgba32>(filePath))
                {
                    // Calculate height to maintain aspect ratio, accounting for font height (~2:1 ratio)
                    // Fonts are usually taller than wide, so we divide height by 2 roughly.
                    double aspectRatio = (double)image.Height / image.Width;
                    int height = (int)(width * aspectRatio * 0.55); // 0.55 is a magic number for common monospaced fonts

                    // Resize image
                    image.Mutate(x => x.Resize(width, height));

                    StringBuilder sb = new StringBuilder();

                    for (int y = 0; y < image.Height; y++)
                    {
                        for (int x = 0; x < image.Width; x++)
                        {
                            Rgba32 pixel = image[x, y];
                            
                            // Grayscale conversion using luminance formula
                            // Y = 0.2126R + 0.7152G + 0.0722B
                            float brightness = (0.2126f * pixel.R) + (0.7152f * pixel.G) + (0.0722f * pixel.B);
                            
                            // Map brightness to character index
                            // Normalize brightness (0-255) to ramp index
                            int index = (int)((brightness / 255.0f) * (ramp.Length - 1));

                            if (invert)
                            {
                                index = (ramp.Length - 1) - index;
                            }

                            // Clamp index just in case
                            index = Math.Max(0, Math.Min(index, ramp.Length - 1));

                            sb.Append(ramp[index]);
                        }
                        sb.AppendLine();
                    }

                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"Error processing image: {ex.Message}";
            }
        }
    }
}