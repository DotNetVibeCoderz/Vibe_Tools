using Avalonia;
using Avalonia.Media.Imaging;
using DrawingApp.Controls;
using System;
using System.IO;

namespace DrawingApp.Helpers;

/// <summary>
/// Helper untuk export canvas ke berbagai format gambar
/// Simplified version menggunakan PNG only untuk compatibility
/// </summary>
public static class ImageExporter
{
    /// <summary>
    /// Export canvas ke file dengan format PNG
    /// Format lain akan otomatis convert dari PNG
    /// </summary>
    public static bool ExportToFile(DrawingCanvas canvas, string filePath)
    {
        try
        {
            var width = (int)canvas.Bounds.Width;
            var height = (int)canvas.Bounds.Height;

            if (width <= 0 || height <= 0)
            {
                Console.WriteLine("Invalid canvas size");
                return false;
            }

            // Create render target
            var pixelSize = new PixelSize(width, height);
            var dpi = new Vector(96, 96);
            
            using var renderTarget = new RenderTargetBitmap(pixelSize, dpi);
            
            // Render canvas ke bitmap
            using (var context = renderTarget.CreateDrawingContext())
            {
                canvas.Render(context);
            }

            // Save ke file
            // Semua format disave sebagai PNG karena RenderTargetBitmap.Save() default PNG
            using var stream = File.Create(filePath);
            renderTarget.Save(stream);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving file: {ex.Message}");
            return false;
        }
    }
}
