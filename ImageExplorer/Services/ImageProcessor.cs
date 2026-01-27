using Avalonia.Media.Imaging;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using ImageExplorer.Models;
using System;

namespace ImageExplorer.Services
{
    public static class ImageProcessor
    {
        // ... (Existing filter methods) ...

        public static Bitmap? ApplyAnnotations(Bitmap source, IEnumerable<AnnotationItem> annotations)
        {
            if (source == null) return null;
            
            // Convert Avalonia Bitmap to SKBitmap
            using var skBitmap = ToSKBitmap(source);
            if (skBitmap == null) return source; // Or null

            // Create a surface to draw on
            using var surface = SKSurface.Create(new SKImageInfo(skBitmap.Width, skBitmap.Height));
            using var canvas = surface.Canvas;
            
            // Draw original image first
            canvas.DrawBitmap(skBitmap, 0, 0);

            // Draw annotations
            foreach (var item in annotations)
            {
                using var paint = new SKPaint
                {
                    Color = new SKColor(item.Color.R, item.Color.G, item.Color.B, item.Color.A),
                    StrokeWidth = (float)item.Thickness,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke // Default to Stroke
                };

                switch (item.Type)
                {
                    case AnnotationType.Line:
                        canvas.DrawLine((float)item.X, (float)item.Y, (float)item.EndX, (float)item.EndY, paint);
                        break;
                    case AnnotationType.Rectangle:
                        canvas.DrawRect(SKRect.Create((float)item.X, (float)item.Y, (float)item.Width, (float)item.Height), paint);
                        break;
                    case AnnotationType.Ellipse:
                         canvas.DrawOval(SKRect.Create((float)item.X, (float)item.Y, (float)item.Width, (float)item.Height), paint);
                        break;
                    case AnnotationType.Point:
                        paint.Style = SKPaintStyle.Fill;
                        canvas.DrawCircle((float)item.X, (float)item.Y, (float)item.Thickness / 2, paint);
                        break;
                    case AnnotationType.Pen:
                        if (item.Points != null && item.Points.Count > 0)
                        {
                            using var path = new SKPath();
                            // Move to start
                            path.MoveTo((float)item.Points[0].X, (float)item.Points[0].Y);
                            // Connect lines
                            for (int i = 1; i < item.Points.Count; i++)
                            {
                                path.LineTo((float)item.Points[i].X, (float)item.Points[i].Y);
                            }
                            
                            paint.Style = SKPaintStyle.Stroke;
                            paint.StrokeCap = SKStrokeCap.Round;
                            paint.StrokeJoin = SKStrokeJoin.Round;
                            canvas.DrawPath(path, paint);
                        }
                        break;
                }
            }
            
            // Return resulting Avalonia Bitmap
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream();
            data.SaveTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            
            return new Bitmap(ms);
        }

        public static Bitmap? Rotate(Bitmap source, float degrees)
        {
            using var skBitmap = ToSKBitmap(source);
            if (skBitmap == null) return null;

            // Simplified rotation logic using Matrix mapping to find new bounds
            var matrix = SKMatrix.CreateRotationDegrees(degrees, skBitmap.Width / 2f, skBitmap.Height / 2f);
            var mapRect = matrix.MapRect(SKRect.Create(skBitmap.Width, skBitmap.Height));
            
            var newWidth = (int)Math.Ceiling(mapRect.Width);
            var newHeight = (int)Math.Ceiling(mapRect.Height);
            
            using var result = new SKBitmap(newWidth, newHeight);
            using var canvas = new SKCanvas(result);
            
            canvas.Clear(SKColors.Transparent);
            
            // Move canvas to center so we can rotate around origin, then move back
            canvas.Translate(-mapRect.Left, -mapRect.Top);
            
            // Draw rotated
            canvas.SetMatrix(canvas.TotalMatrix.PostConcat(matrix));
            canvas.DrawBitmap(skBitmap, 0, 0);

            return ToAvaloniaBitmap(result);
        }

        public static Bitmap? Resize(Bitmap source, int width, int height)
        {
            using var skBitmap = ToSKBitmap(source);
            if (skBitmap == null) return null;

            var info = new SKImageInfo(width, height);
            using var resized = skBitmap.Resize(info, SKSamplingOptions.Default);
            if (resized == null) return null;
            
            return ToAvaloniaBitmap(resized);
        }

        public static Bitmap? ApplyFilter(Bitmap source, string filterName)
        {
            using var skBitmap = ToSKBitmap(source);
            if (skBitmap == null) return null;

            SKBitmap resultBitmap;

            // Simple switch for filters
            switch (filterName)
            {
                 case "Grayscale":
                    resultBitmap = ApplyColorMatrix(skBitmap, new float[]
                    {
                        0.21f, 0.72f, 0.07f, 0, 0,
                        0.21f, 0.72f, 0.07f, 0, 0,
                        0.21f, 0.72f, 0.07f, 0, 0,
                        0,     0,     0,     1, 0
                    });
                    break;
                case "Sepia":
                     resultBitmap = ApplyColorMatrix(skBitmap, new float[]
                    {
                        0.393f, 0.769f, 0.189f, 0, 0,
                        0.349f, 0.686f, 0.168f, 0, 0,
                        0.272f, 0.534f, 0.131f, 0, 0,
                        0,      0,      0,      1, 0
                    });
                    break;
                 case "Invert":
                    resultBitmap = ApplyColorMatrix(skBitmap, new float[]
                    {
                        -1,  0,  0, 0, 255,
                         0, -1,  0, 0, 255,
                         0,  0, -1, 0, 255,
                         0,  0,  0, 1, 0
                    });
                    break;
                 case "Blur":
                    resultBitmap = ApplyBlur(skBitmap);
                    break;
                 default:
                    return source;
            }
            
            var res = ToAvaloniaBitmap(resultBitmap);
            resultBitmap.Dispose();
            return res;
        }

        private static SKBitmap ApplyColorMatrix(SKBitmap original, float[] matrix)
        {
            var result = new SKBitmap(original.Info);
            using (var canvas = new SKCanvas(result))
            using (var paint = new SKPaint())
            {
                paint.ColorFilter = SKColorFilter.CreateColorMatrix(matrix);
                canvas.DrawBitmap(original, 0, 0, paint);
            }
            return result;
        }

        private static SKBitmap ApplyBlur(SKBitmap original)
        {
            var result = new SKBitmap(original.Info);
            using (var canvas = new SKCanvas(result))
            using (var paint = new SKPaint())
            {
                paint.ImageFilter = SKImageFilter.CreateBlur(5, 5);
                canvas.DrawBitmap(original, 0, 0, paint);
            }
            return result;
        }

        private static SKBitmap? ToSKBitmap(Bitmap avaloniaBitmap)
        {
            try
            {
                using var stream = new MemoryStream();
                avaloniaBitmap.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return SKBitmap.Decode(stream);
            }
            catch { return null; }
        }

        private static Bitmap ToAvaloniaBitmap(SKBitmap skBitmap)
        {
            using var image = SKImage.FromBitmap(skBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = new MemoryStream();
            data.SaveTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(stream);
        }
    }
}