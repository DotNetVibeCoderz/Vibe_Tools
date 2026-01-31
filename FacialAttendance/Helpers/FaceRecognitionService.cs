using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Face;

namespace FacialAttendance.Helpers
{
    public class FaceRecognitionService : IDisposable
    {
        private VideoCapture _capture;
        private CascadeClassifier _faceCascade;
        private LBPHFaceRecognizer _recognizer;
        private bool _isTrained = false;
        private string _modelPath = "Data/trained_model.xml";
        private string _haarcascadePath = "haarcascade_frontalface_default.xml";

        public bool IsCameraRunning => _capture != null && _capture.IsOpened();

        public FaceRecognitionService()
        {
            if (File.Exists(_haarcascadePath))
            {
                _faceCascade = new CascadeClassifier(_haarcascadePath);
            }
            else
            {
                throw new FileNotFoundException("Haar cascade file not found.");
            }

            _recognizer = LBPHFaceRecognizer.Create(1, 8, 8, 8, 100); // Radius 1, Neighbors 8, Grid 8x8, Threshold 100 (Adjust as needed)
            
            LoadModel();
        }

        public void StartCamera(int index = 0)
        {
            if (_capture == null)
            {
                _capture = new VideoCapture(index);
            }
            else if (!_capture.IsOpened())
            {
                _capture.Open(index);
            }
        }

        public void StopCamera()
        {
            if (_capture != null && _capture.IsOpened())
            {
                _capture.Release();
                _capture.Dispose();
                _capture = null;
            }
        }

        public Mat GetFrame()
        {
            if (_capture != null && _capture.IsOpened())
            {
                Mat frame = new Mat();
                _capture.Read(frame);
                return frame;
            }
            return null;
        }

        public Rect[] DetectFaces(Mat frame)
        {
            if (frame.Empty()) return new Rect[0];
            
            using (var gray = new Mat())
            {
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.EqualizeHist(gray, gray);
                
                // ScaleFactor 1.1, MinNeighbors 5
                return _faceCascade.DetectMultiScale(gray, 1.1, 5, HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(30, 30));
            }
        }

        public void SaveTrainingImage(Mat frame, Rect faceRect, int userId)
        {
            // Crop the face
            Mat face = new Mat(frame, faceRect);
            
            // Convert to Gray
            Mat grayFace = new Mat();
            Cv2.CvtColor(face, grayFace, ColorConversionCodes.BGR2GRAY);
            Cv2.Resize(grayFace, grayFace, new OpenCvSharp.Size(100, 100));

            // Create folder
            string userFolder = Path.Combine("Faces", userId.ToString());
            if (!Directory.Exists(userFolder)) Directory.CreateDirectory(userFolder);

            // Save
            string fileName = Path.Combine(userFolder, $"{Guid.NewGuid()}.jpg");
            grayFace.SaveImage(fileName);
        }

        public void Train()
        {
            var images = new List<Mat>();
            var labels = new List<int>();

            string facesDir = "Faces";
            if (!Directory.Exists(facesDir)) return;

            var userDirs = Directory.GetDirectories(facesDir);
            foreach (var userDir in userDirs)
            {
                int userId;
                if (int.TryParse(Path.GetFileName(userDir), out userId))
                {
                    var files = Directory.GetFiles(userDir, "*.jpg");
                    foreach (var file in files)
                    {
                        Mat img = Cv2.ImRead(file, ImreadModes.Grayscale);
                        Cv2.Resize(img, img, new OpenCvSharp.Size(100, 100)); // Ensure consistent size
                        images.Add(img);
                        labels.Add(userId);
                    }
                }
            }

            if (images.Count > 0)
            {
                _recognizer.Train(images, labels);
                _recognizer.Save(_modelPath);
                _isTrained = true;
            }
        }

        private void LoadModel()
        {
            if (File.Exists(_modelPath))
            {
                try 
                {
                    _recognizer.Read(_modelPath);
                    _isTrained = true;
                }
                catch
                {
                    _isTrained = false;
                }
            }
        }

        public int Predict(Mat frame, Rect faceRect, out double confidence)
        {
            confidence = 0;
            if (!_isTrained) return -1;

            Mat face = new Mat(frame, faceRect);
            Mat grayFace = new Mat();
            Cv2.CvtColor(face, grayFace, ColorConversionCodes.BGR2GRAY);
            Cv2.Resize(grayFace, grayFace, new OpenCvSharp.Size(100, 100));

            int label = -1;
            _recognizer.Predict(grayFace, out label, out confidence);

            // Lower confidence value means better match in LBPH (distance)
            // Typically < 50 is a good match, > 80 is likely unknown.
            // Adjust threshold logic in calling code or here.
            return label;
        }

        public void Dispose()
        {
            StopCamera();
            _faceCascade?.Dispose();
            _recognizer?.Dispose();
        }
    }
}