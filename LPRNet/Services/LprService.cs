using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LPRNet.Models;
using OpenCvSharp;
using Tesseract;

// Alias to resolve ambiguity
using Rect = OpenCvSharp.Rect;

namespace LPRNet.Services
{
    public interface ILprService
    {
        Task<PlateRecord?> ProcessImageAsync(string imagePath);
    }

    /// <summary>
    /// Service for License Plate Recognition using OpenCvSharp (Computer Vision) and Tesseract OCR.
    /// This implementation performs real OCR on images.
    /// </summary>
    public class SimpleLprService : ILprService
    {
		private readonly Random _random = new Random();

		private readonly string _tessDataPath;
        private const string Language = "eng";

        public SimpleLprService()
        {
            // Ensure we look for tessdata in the application directory
            _tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
        }

        public async Task<PlateRecord?> ProcessImageAsync(string imagePath)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(imagePath)) return null;

                try
                {
					// Check if file is empty (dummy file from ViewModel)
					var info = new FileInfo(imagePath);
					if (info.Length == 0)
					{
						// Create a synthetic image for demonstration if file is empty
						using var synthetic = new Mat(480, 640, MatType.CV_8UC3, new Scalar(50, 50, 50));
						Cv2.PutText(synthetic, "LPR SIMULATION", new Point(50, 50), HersheyFonts.HersheySimplex, 1.0, Scalar.White, 2);

						// Draw a fake plate
						var plateRect = new Rect(200, 300, 240, 60);
						Cv2.Rectangle(synthetic, plateRect, Scalar.White, -1);
						Cv2.PutText(synthetic, GenerateMockPlate(), new Point(220, 345), HersheyFonts.HersheySimplex, 1.0, Scalar.Black, 2);

						synthetic.SaveImage(imagePath);
					}
					
                    // 1. Read Image
                    using var src = Cv2.ImRead(imagePath);
                    if (src.Empty()) return null;

                    string bestText = "";
                    float bestConfidence = 0.0f;
                    string processingNotes = "Processing started.";

                    // Initialize Tesseract Engine
                    using var engine = new TesseractEngine(_tessDataPath, Language, EngineMode.Default);
                    
                    // Configure for uppercase alphanumeric (common for plates)
                    // We allow space as well
                    engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ");
                    engine.SetVariable("tessedit_pageseg_mode", "7"); // Treat the image as a single text line

                    // 2. Pre-processing for Contour Detection
                    using var gray = new Mat();
                    Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
                    
                    using var blur = new Mat();
                    Cv2.GaussianBlur(gray, blur, new Size(5, 5), 0);

                    using var binary = new Mat();
                    // Adaptive threshold to handle different lighting conditions
                    Cv2.AdaptiveThreshold(blur, binary, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 19, 9);

                    // 3. Find Candidates (Contours)
                    Point[][] contours;
                    HierarchyIndex[] hierarchy;
                    Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

                    // Filter contours based on area and aspect ratio typical for license plates
                    var potentialPlates = new List<Rect>();
                    foreach (var contour in contours)
                    {
                        var rect = Cv2.BoundingRect(contour);
                        double aspect = (double)rect.Width / rect.Height;
                        double area = rect.Width * rect.Height;

                        // Typical plate aspect ratio ~ 2.0 to 5.0
                        // Min area to filter noise
                        if (aspect > 2.0 && aspect < 6.0 && area > 1000) 
                        {
                            potentialPlates.Add(rect);
                        }
                    }

                    // Sort candidates by area descending to process largest (most likely) first
                    potentialPlates = potentialPlates.OrderByDescending(r => r.Width * r.Height).Take(5).ToList();

                    bool foundValidPlate = false;

                    // 4. Process Candidates
                    foreach (var rect in potentialPlates)
                    {
                        // Crop the potential plate area
                        // Explicitly using OpenCvSharp.Rect is handled by the alias
                        using var cropped = new Mat(src, rect);
                        
                        // Convert to grayscale for OCR
                        using var croppedGray = new Mat();
                        Cv2.CvtColor(cropped, croppedGray, ColorConversionCodes.BGR2GRAY);

                        // Thresholding for cleaner OCR input
                        using var croppedThresh = new Mat();
                        Cv2.Threshold(croppedGray, croppedThresh, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

                        // Convert OpenCv Mat to Tesseract Pix via memory
                        // Note: encoding as bmp/png to memory stream
                        byte[] imageBytes = croppedThresh.ToBytes(".bmp");
                        using var img = Pix.LoadFromMemory(imageBytes);
                        using var page = engine.Process(img);
                        
                        string text = page.GetText().Trim();
                        float confidence = page.GetMeanConfidence();

                        // Clean text
                        string cleanedText = Regex.Replace(text, "[^A-Z0-9]", "");

                        // Heuristic check: length >= 4
                        if (cleanedText.Length >= 4 && confidence > bestConfidence)
                        {
                            bestText = FormatPlate(cleanedText);
                            bestConfidence = confidence;
                            foundValidPlate = true;
                            processingNotes = "Plate detected from contours.";
                        }
                    }

                    // 5. Fallback: Full Image OCR if no plate found via contours
                    if (!foundValidPlate)
                    {
                        processingNotes = "Fallback to full image OCR.";
                        using var fullGray = new Mat();
                        Cv2.CvtColor(src, fullGray, ColorConversionCodes.BGR2GRAY);
                        
                        byte[] fullBytes = fullGray.ToBytes(".bmp");
                        using var fullPix = Pix.LoadFromMemory(fullBytes);
                        
                        // Use a different segmentation mode for full page
                        engine.SetVariable("tessedit_pageseg_mode", "3"); // Auto segmentation
                        using var page = engine.Process(fullPix);
                        
                        string rawText = page.GetText().Trim();
                        if (!string.IsNullOrWhiteSpace(rawText))
                        {
                             // Extract potential plate pattern from full text using Regex
                             // Pattern: 1-2 letters, 1-4 numbers, 1-3 letters
                             var match = Regex.Match(rawText, @"([A-Z]{1,2})\s*(\d{1,4})\s*([A-Z]{1,3})");
                             if (match.Success)
                             {
                                 bestText = $"{match.Groups[1].Value} {match.Groups[2].Value} {match.Groups[3].Value}";
                                 bestConfidence = page.GetMeanConfidence();
                             }
                             else
                             {
                                 // Just take the cleanest alphanumeric string
                                 string cleaned = Regex.Replace(rawText, "[^A-Z0-9]", "");
                                 if (cleaned.Length > 3)
                                 {
                                     bestText = cleaned;
                                     bestConfidence = page.GetMeanConfidence();
                                 }
                             }
                        }
                    }

                    return new PlateRecord
                    {
                        PlateNumber = string.IsNullOrEmpty(bestText) ? "UNREADABLE" : bestText,
                        Timestamp = DateTime.Now,
                        ImagePath = imagePath,
                        Confidence = bestConfidence,
                        Country = "ID",
                        IsBlacklisted = false, 
                        Notes = processingNotes
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing: {ex.Message}");
                    return new PlateRecord
                    {
                        PlateNumber = "ERROR",
                        Timestamp = DateTime.Now,
                        ImagePath = imagePath,
                        Notes = $"Error: {ex.Message}"
                    };
                }
            });
        }

		private string GenerateMockPlate()
		{
			string[] prefixes = { "B", "D", "F", "AB", "AD", "L", "N" };
			string prefix = prefixes[_random.Next(prefixes.Length)];
			int number = _random.Next(1000, 9999);
			string[] suffixes = { "XY", "ABC", "JK", "RF", "BD" };
			string suffix = suffixes[_random.Next(suffixes.Length)];
			return $"{prefix} {number} {suffix}";
		}
        private string FormatPlate(string text)
        {
            // Simple heuristic to insert spaces if they are missing
            // E.g. B1234CD -> B 1234 CD
            
            // If already contains spaces, return as is (normalized)
            if (text.Contains(" ")) return text;

            // Find the numeric part
            var match = Regex.Match(text, @"^([A-Z]{1,2})(\d{1,4})([A-Z]{1,3})$");
            if (match.Success)
            {
                return $"{match.Groups[1].Value} {match.Groups[2].Value} {match.Groups[3].Value}";
            }

            return text;
        }
    }
}
