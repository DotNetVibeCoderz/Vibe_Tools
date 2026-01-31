using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FacialAttendance.Helpers;
using FacialAttendance.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace FacialAttendance.Forms
{
    public partial class MainForm : Form
    {
        private FaceRecognitionService _faceService;
        private List<User> _users;
        private bool _isDetecting = true;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                DatabaseHelper.Initialize();
                RefreshUsers();

                _faceService = new FaceRecognitionService();
                _faceService.StartCamera();
                timerCamera.Start();
                lblStatus.Text = "System Running... Camera Active";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing: " + ex.Message);
            }
        }

        public void RefreshUsers()
        {
            _users = DatabaseHelper.GetUsers();
        }

        private void timerCamera_Tick(object sender, EventArgs e)
        {
            if (!_isDetecting) return;

            var frame = _faceService.GetFrame();
            if (frame != null && !frame.Empty())
            {
                var faces = _faceService.DetectFaces(frame);
                
                foreach (var faceRect in faces)
                {
                    // Draw rectangle
                    Cv2.Rectangle(frame, faceRect, new Scalar(0, 255, 0), 2);

                    // Recognize
                    double confidence;
                    int label = _faceService.Predict(frame, faceRect, out confidence);

                    string name = "Unknown";
                    // Confidence: Lower is better for LBPH. < 50-60 is usually good match.
                    if (label != -1 && confidence < 70) 
                    {
                        var user = _users.FirstOrDefault(u => u.Id == label);
                        if (user != null)
                        {
                            name = $"{user.Name} ({confidence:0.0})";
                            // Log Attendance
                            DatabaseHelper.LogAttendance(user.Id);
                            lblStatus.Text = $"Detected: {user.Name} at {DateTime.Now.ToShortTimeString()}";
                        }
                    }

                    Cv2.PutText(frame, name, new OpenCvSharp.Point(faceRect.X, faceRect.Y - 5), 
                        HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 255, 0), 2);
                }

                // Convert to Bitmap for PictureBox
                var bmp = BitmapConverter.ToBitmap(frame);
                var old = pbCamera.Image;
                pbCamera.Image = bmp;
                old?.Dispose();
                frame.Dispose();
            }
        }

        private void registerUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PauseCamera();
            using (var frm = new RegisterForm())
            {
                frm.ShowDialog();
                RefreshUsers();
                ReloadModel();
            }
            ResumeCamera();
        }

        private void manageUsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PauseCamera();
            using (var frm = new ManageUsersForm())
            {
                frm.ShowDialog();
                RefreshUsers();
                ReloadModel(); // Reload because users might be deleted
            }
            ResumeCamera();
        }

        private void reportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Report form doesn't need camera pause strictly, but safer
            using (var frm = new ReportForm())
            {
                frm.ShowDialog();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new AboutForm())
            {
                frm.ShowDialog();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerCamera.Stop();
            _faceService?.Dispose();
        }

        private void PauseCamera()
        {
            timerCamera.Stop();
            _faceService.StopCamera();
            lblStatus.Text = "Camera Paused";
        }

        private void ResumeCamera()
        {
            _faceService.StartCamera();
            timerCamera.Start();
            lblStatus.Text = "System Running... Camera Active";
        }

        private void ReloadModel()
        {
            _faceService.Dispose(); 
            _faceService = new FaceRecognitionService();
        }
    }
}