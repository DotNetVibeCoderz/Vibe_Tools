# ObjectCounter

A Computer Vision based object counting application using C#, Avalonia UI, and YOLOv8.

## Features
- Real-time Object Detection (YOLOv8)
- Object Classification & Tracking
- SQLite Data Logging
- Statistics Dashboard
- API Integration (Mini HTTP Server)

## Prerequisites
- .NET 8.0 SDK or later
- A webcam or video file

## Setup
1. Clone or download this repository.
2. **IMPORTANT:** You must download the `yolov8n.onnx` model file and place it in the `ObjectCounter` project folder (where the .csproj file is).
   - Download link: [https://github.com/microsoft/onnxruntime-inference-examples/raw/main/c_sharp/ObjectDetection/yolov8/yolov8n.onnx](https://github.com/microsoft/onnxruntime-inference-examples/raw/main/c_sharp/ObjectDetection/yolov8/yolov8n.onnx)
   - Or convert your own YOLOv8 model to ONNX.
3. Build and Run the project.

## Usage
- **Source Input:** Select Webcam, Video File, or IP Camera URL. Click Start.
- **Statistics:** View logged detections.
- **API:** Access `http://localhost:5000/api/logs` for JSON data.

## Note from Jacky
This application uses `OpenCvSharp4` and `Microsoft.ML.OnnxRuntime`. 
If you encounter GPU errors, ensure you have CUDA installed or rely on CPU inference (default).

Happy Coding!
-- Jacky The Code Bender
