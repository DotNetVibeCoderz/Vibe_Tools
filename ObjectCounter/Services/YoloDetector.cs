using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using ObjectCounter.Models;

namespace ObjectCounter.Services
{
    public class YoloDetector : IDisposable
    {
        private InferenceSession? _session;
        private string[]? _outputNames;
        private readonly float _confidenceThreshold = 0.45f;
        private readonly float _iouThreshold = 0.5f;

        private readonly string[] _labels = new string[]
        {
            "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light",
            "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow",
            "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee",
            "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard",
            "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
            "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch",
            "potted plant", "bed", "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard", "cell phone",
            "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear",
            "hair drier", "toothbrush"
        };

        public YoloDetector(string modelPath)
        {
            if (!File.Exists(modelPath)) return;

            var options = new SessionOptions();
            try
            {
                // options.AppendExecutionProvider_CUDA(); 
            }
            catch { }

            _session = new InferenceSession(modelPath, options);
            _outputNames = _session.OutputMetadata.Keys.ToArray();
        }

        public List<DetectionResult> Detect(Mat image)
        {
            var detections = new List<DetectionResult>();
            if (_session == null) return detections;

            int targetWidth = 640;
            int targetHeight = 640;
            
            using var resized = new Mat();
            Cv2.Resize(image, resized, new Size(targetWidth, targetHeight));

            using var blob = CvDnn.BlobFromImage(resized, 1/255.0, new Size(targetWidth, targetHeight), new Scalar(0,0,0), true, false);
            
            float[] data = new float[1 * 3 * targetHeight * targetWidth];
            if (blob.IsContinuous())
            {
                System.Runtime.InteropServices.Marshal.Copy(blob.Data, data, 0, data.Length);
            }

            var tensor = new DenseTensor<float>(data, new[] { 1, 3, targetHeight, targetWidth });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", tensor)
            };

            using var results = _session.Run(inputs);
            var output = results.First().AsTensor<float>(); 

            int anchors = 8400;
            int dimensions = 84; 

            float xScale = (float)image.Width / targetWidth;
            float yScale = (float)image.Height / targetHeight;

            var boxes = new List<Rect2d>();
            var confidences = new List<float>();
            var classIds = new List<int>();

            // output[0, c, a] access via indexer
            for (int a = 0; a < anchors; a++)
            {
                float maxScore = -1;
                int maxClassId = -1;

                for (int c = 4; c < dimensions; c++)
                {
                    float score = output[0, c, a];
                    if (score > maxScore)
                    {
                        maxScore = score;
                        maxClassId = c - 4;
                    }
                }

                if (maxScore > _confidenceThreshold)
                {
                    float cx = output[0, 0, a];
                    float cy = output[0, 1, a];
                    float w = output[0, 2, a];
                    float h = output[0, 3, a];

                    float x = (cx - w / 2) * xScale;
                    float y = (cy - h / 2) * yScale;
                    float width = w * xScale;
                    float height = h * yScale;

                    boxes.Add(new Rect2d(x, y, width, height));
                    confidences.Add(maxScore);
                    classIds.Add(maxClassId);
                }
            }

            if (boxes.Count > 0)
            {
                CvDnn.NMSBoxes(boxes, confidences, _confidenceThreshold, _iouThreshold, out int[] indices);

                foreach (var i in indices)
                {
                    var box = boxes[i];
                    detections.Add(new DetectionResult
                    {
                        Id = i,
                        Label = _labels[classIds[i]],
                        Confidence = confidences[i],
                        X = (float)box.X,
                        Y = (float)box.Y,
                        Width = (float)box.Width,
                        Height = (float)box.Height
                    });
                }
            }

            return detections;
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
