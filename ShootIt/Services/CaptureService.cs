using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace ShootIt.Services
{
    public class CaptureService
    {
        private string GetSaveDirectory()
        {
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        // Modified: Now returns the Bitmap object for the Recorder to use, or saves to file if saveToFile is true.
        public Bitmap CaptureFullScreenBitmap()
        {
            try
            {
                int width = 1920; 
                int height = 1080;
                
                // Try to get primary screen size on Windows
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                     // Simplified for MVP. Real app uses System.Windows.Forms.Screen.PrimaryScreen.Bounds
                     // or Avalonia's Screen.
                     // We stick to FHD for safety in this demo if detection logic is missing.
                }

                Bitmap bitmap = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
                }
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing bitmap: {ex.Message}");
                return null;
            }
        }

        public string CaptureFullScreen()
        {
            using (var bitmap = CaptureFullScreenBitmap())
            {
                if (bitmap == null) return null;
                string filename = Path.Combine(GetSaveDirectory(), $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                bitmap.Save(filename, ImageFormat.Png);
                return filename;
            }
        }

        public string CaptureRegion(int x, int y, int width, int height)
        {
            try
            {
                if (width <= 0 || height <= 0) return null;

                using (Bitmap bitmap = new Bitmap(width, height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(x, y, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
                    }

                    string filename = Path.Combine(GetSaveDirectory(), $"Region_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                    bitmap.Save(filename, ImageFormat.Png);
                    return filename;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing region: {ex.Message}");
                return null;
            }
        }
    }
}