using SkiaSharp;
using System;
using System.IO;

namespace ImageExplorer.Services
{
    public static class BatchImageProcessor
    {
        public enum BatchAction
        {
            Rotate,
            Convert,
            Resize
        }

        public static void ProcessImage(string sourcePath, string destFolder, BatchAction action, object parameter)
        {
            using var stream = File.OpenRead(sourcePath);
            using var skBitmap = SKBitmap.Decode(stream);

            if (skBitmap == null) return;

            SKBitmap resultBitmap = skBitmap;
            bool shouldDisposeResult = false; // Track if we created a new bitmap that needs disposal

            try
            {
                // Apply Transformation
                switch (action)
                {
                    case BatchAction.Rotate:
                        if (parameter is float degrees)
                        {
                            resultBitmap = Rotate(skBitmap, degrees);
                            shouldDisposeResult = true;
                        }
                        break;
                    case BatchAction.Resize:
                        if (parameter is Tuple<int, int> size)
                        {
                            var info = new SKImageInfo(size.Item1, size.Item2);
                            resultBitmap = skBitmap.Resize(info, SKFilterQuality.High);
                            shouldDisposeResult = true;
                        }
                        break;
                    case BatchAction.Convert:
                        // No bitmap transformation needed, just encoding change
                        break;
                }

                // Determine Output Format
                var extension = Path.GetExtension(sourcePath).ToLower();
                var format = SKEncodedImageFormat.Png;

                if (action == BatchAction.Convert && parameter is string targetExt)
                {
                    extension = targetExt.StartsWith(".") ? targetExt : "." + targetExt;
                }

                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        format = SKEncodedImageFormat.Jpeg;
                        break;
                    case ".webp":
                        format = SKEncodedImageFormat.Webp;
                        break;
                    case ".bmp":
                        format = SKEncodedImageFormat.Bmp;
                        break;
                    case ".png":
                    default:
                        format = SKEncodedImageFormat.Png;
                        break;
                }

                // Save
                var fileName = Path.GetFileNameWithoutExtension(sourcePath);
                var destPath = Path.Combine(destFolder, fileName + extension);

                using (var image = SKImage.FromBitmap(resultBitmap))
                using (var data = image.Encode(format, 90))
                using (var destStream = File.OpenWrite(destPath))
                {
                    data.SaveTo(destStream);
                }
            }
            finally
            {
                if (shouldDisposeResult && resultBitmap != skBitmap)
                {
                    resultBitmap.Dispose();
                }
            }
        }

        private static SKBitmap Rotate(SKBitmap source, float degrees)
        {
            var matrix = SKMatrix.CreateRotationDegrees(degrees, source.Width / 2f, source.Height / 2f);
            var mapRect = new SKRect(0, 0, source.Width, source.Height);
            var mapped = matrix.MapRect(mapRect);

            var newWidth = (int)Math.Ceiling(mapped.Width);
            var newHeight = (int)Math.Ceiling(mapped.Height);

            var result = new SKBitmap(newWidth, newHeight);
            using (var canvas = new SKCanvas(result))
            {
                canvas.Clear(SKColors.Transparent);
                canvas.Translate(-mapped.Left, -mapped.Top);
                canvas.RotateDegrees(degrees, source.Width / 2f, source.Height / 2f);
                canvas.DrawBitmap(source, 0, 0);
            }
            return result;
        }
    }
}