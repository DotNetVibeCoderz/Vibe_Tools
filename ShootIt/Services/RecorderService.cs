using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices; // Added for Marshal
using System.Threading;
using SharpAvi;
using SharpAvi.Output;

namespace ShootIt.Services
{
    public class RecorderService
    {
        private AviWriter _writer;
        private IAviVideoStream _stream;
        private bool _isRecording;
        private Thread _recordThread;
        private readonly CaptureService _captureService;
        private int _width = 1920;
        private int _height = 1080;

        public RecorderService()
        {
            _captureService = new CaptureService();
        }

        public string StartRecording()
        {
            if (_isRecording) return null;

            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recordings");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string filename = Path.Combine(folder, $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.avi");

            // Setup AviWriter
            _writer = new AviWriter(filename)
            {
                FramesPerSecond = 10,
                EmitIndex1 = true
            };

            // Create UNCOMPRESSED video stream (Safe fallback for MVP without external codecs)
            // Note: This produces large files. In a real app, use MotionJpegVideoEncoderLame or similar.
            _stream = _writer.AddVideoStream(_width, _height, BitsPerPixel.Bpp32);
            _stream.Name = "ShootIt_ScreenRecording";
            
            _isRecording = true;
            _recordThread = new Thread(RecordLoop);
            _recordThread.Start();

            return filename;
        }

        public void StopRecording()
        {
            _isRecording = false;
            if (_recordThread != null && _recordThread.IsAlive)
            {
                _recordThread.Join();
            }

            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }
        }

        private void RecordLoop()
        {
            // Buffer size: Width * Height * 4 bytes (32 bpp)
            var buffer = new byte[_width * _height * 4]; 

            while (_isRecording)
            {
                try
                {
                    using (var bmp = _captureService.CaptureFullScreenBitmap())
                    {
                        if (bmp != null)
                        {
                            // Fix: Flip vertically because AVI expects bottom-up bitmap data
                            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                            // Lock bits to get raw data
                            // Note: GDI+ usually is BGR order, SharpAvi uncompressed usually expects BGR too for Bpp32
                            var bits = bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                            
                            // Copy data from native memory to managed buffer
                            Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
                            
                            bmp.UnlockBits(bits);
                            
                            // Write sync to stream (Async in loop can be tricky without await)
                            // Since we are in a dedicated thread, sync write is fine.
                            _stream.WriteFrame(true, buffer, 0, buffer.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Frame drop: " + ex.Message);
                }

                // Throttle to match target FPS roughly
                Thread.Sleep(100); 
            }
        }
    }
}