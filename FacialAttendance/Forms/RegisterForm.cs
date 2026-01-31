using System;
using System.Drawing;
using System.Windows.Forms;
using FacialAttendance.Helpers;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace FacialAttendance.Forms
{
    public partial class RegisterForm : Form
    {
        private FaceRecognitionService _faceService;
        private bool _isCapturing = false;
        private int _samplesTaken = 0;
        private int _targetSamples = 5;
        private int _currentUserId = 0;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {
            _faceService = new FaceRecognitionService();
            _faceService.StartCamera();
            timerPreview.Start();
        }

        private void timerPreview_Tick(object sender, EventArgs e)
        {
            var frame = _faceService.GetFrame();
            if (frame != null && !frame.Empty())
            {
                var faces = _faceService.DetectFaces(frame);
                
                // Draw rectangles
                foreach (var face in faces)
                {
                    Cv2.Rectangle(frame, face, new Scalar(255, 0, 0), 2);
                }

                // Capture logic
                if (_isCapturing && faces.Length == 1)
                {
                    // Slight delay or check to ensure we don't take same frame? 
                    // 30ms interval is fine, maybe we want slight variation.
                    // For simplicity, just take it.
                    
                    _faceService.SaveTrainingImage(frame, faces[0], _currentUserId);
                    _samplesTaken++;
                    lblCount.Text = $"Samples: {_samplesTaken}/{_targetSamples}";

                    if (_samplesTaken >= _targetSamples)
                    {
                        _isCapturing = false;
                        MessageBox.Show("Samples captured! Click 'Save & Train' to finish.");
                        btnSave.Enabled = true;
                        btnCapture.Enabled = false;
                    }
                }

                // Show in PictureBox
                var bmp = BitmapConverter.ToBitmap(frame);
                var old = pbCapture.Image;
                pbCapture.Image = bmp;
                old?.Dispose();
                frame.Dispose();
            }
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a name.");
                return;
            }

            try 
            {
                _currentUserId = DatabaseHelper.AddUser(txtName.Text);
                _samplesTaken = 0;
                _isCapturing = true;
                btnCapture.Enabled = false; // Prevent double click
                txtName.Enabled = false;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error creating user: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            lblCount.Text = "Training model... Please wait.";
            Application.DoEvents();
            
            try 
            {
                _faceService.Train();
                MessageBox.Show("Training Complete! User Registered.");
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error training: " + ex.Message);
            }
        }

        private void RegisterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerPreview.Stop();
            _faceService?.Dispose();
        }
    }
}