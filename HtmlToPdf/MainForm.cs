using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace HtmlToPdf
{
    public partial class MainForm : Form
    {
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private Dictionary<string, TextBox> inputControls = new Dictionary<string, TextBox>();
        private string currentTemplateHtml = "";
        private string templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        private bool isBrowserDownloaded = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeWebView();
            SetupCustomControls();
            
            this.Load += MainForm_Load;
        }

        private void SetupCustomControls()
        {
            // Adjust layout for Repeat controls
            // GrpExport is now at the bottom.
            // Items: PaperSize (29, 47), Format (83, 101).
            
            // Let's position Repeat controls below Format.
            chkRepeat.Location = new System.Drawing.Point(16, 140);
            chkRepeat.Visible = true;
            chkRepeat.Text = "Repeat x times";
            
            numRepeat.Location = new System.Drawing.Point(130, 140);
            numRepeat.Visible = true;
            numRepeat.Enabled = false;

            // Increase height of grpExport to fit them
            grpExport.Height = 180;

            chkRepeat.CheckedChanged += (s, e) => numRepeat.Enabled = chkRepeat.Checked;
        }

        private async void InitializeWebView()
        {
            try
            {
                webView = new Microsoft.Web.WebView2.WinForms.WebView2();
                webView.Dock = DockStyle.Fill;
                panelPreview.Controls.Add(webView);
                panelPreview.Controls.SetChildIndex(lblPreview, 0); // Keep label on top until loaded
                
                await webView.EnsureCoreWebView2Async(null);
                lblPreview.Visible = false;
            }
            catch (Exception ex)
            {
                lblPreview.Text = "Error initializing WebView2.\nMake sure WebView2 Runtime is installed.\n" + ex.Message;
            }
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(templatesPath))
            {
                Directory.CreateDirectory(templatesPath);
            }

            LoadTemplates();
            cmbPaperSize.SelectedIndex = 0; // A4
            cmbFormat.SelectedIndex = 0; // PDF

            // Background download of browser
            await DownloadBrowserAsync();
        }

        private async Task DownloadBrowserAsync()
        {
            var browserFetcher = new BrowserFetcher();
            
            lblPreview.Text = "Downloading PDF Engine (Chromium)... Please wait.";
            lblPreview.Visible = true;
            
            try 
            {
                await browserFetcher.DownloadAsync();
                isBrowserDownloaded = true;
                lblPreview.Visible = false;
                UpdatePreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to download browser for PDF generation: " + ex.Message);
            }
        }

        private void LoadTemplates()
        {
            cmbTemplates.Items.Clear();
            var files = Directory.GetFiles(templatesPath, "*.html");
            foreach (var file in files)
            {
                cmbTemplates.Items.Add(Path.GetFileName(file));
            }

            if (cmbTemplates.Items.Count > 0)
                cmbTemplates.SelectedIndex = 0;
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadTemplates();
        }

        private void CmbTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTemplates.SelectedItem == null) return;

            string filename = cmbTemplates.SelectedItem.ToString();
            string path = Path.Combine(templatesPath, filename);

            try
            {
                currentTemplateHtml = File.ReadAllText(path);
                GenerateInputFields(currentTemplateHtml);
                UpdatePreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading template: {ex.Message}");
            }
        }

        private void GenerateInputFields(string html)
        {
            flowInputs.Controls.Clear();
            inputControls.Clear();

            var fields = TemplateProcessor.ParseFields(html);

            foreach (var field in fields)
            {
                Label lbl = new Label();
                lbl.Text = field;
                lbl.AutoSize = true;
                lbl.Margin = new Padding(3, 10, 3, 0);
                lbl.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);

                TextBox txt = new TextBox();
                txt.Name = field;
                txt.Width = 300;
                txt.Tag = field;
                txt.TextChanged += (s, e) => UpdatePreview();

                // Add some default values for better UX (optional)
                if (field.Contains("Name")) txt.Text = "John Doe";
                else if (field.Contains("Company")) txt.Text = "Acme Corp";
                else if (field.Contains("Date")) txt.Text = DateTime.Now.ToShortDateString();
                else txt.Text = $"Sample {field}";

                inputControls.Add(field, txt);

                flowInputs.Controls.Add(lbl);
                flowInputs.Controls.Add(txt);
            }
        }

        private Dictionary<string, string> GetCurrentValues()
        {
            var values = new Dictionary<string, string>();
            foreach (var kvp in inputControls)
            {
                values.Add(kvp.Key, kvp.Value.Text);
            }
            return values;
        }

        private void UpdatePreview()
        {
            if (webView == null || webView.CoreWebView2 == null) return;

            var values = GetCurrentValues();
            string finalHtml = TemplateProcessor.Process(currentTemplateHtml, values);
            
            // If repeat is checked, preview it repeated? 
            // Usually preview shows single item to be fast, but user might want to see grid.
            // Let's respect the checkbox for preview too if possible, or just keep single for preview.
            // Keeping single for preview is safer for performance, but user wants to see layout.
            // Let's use single for preview unless they really want to see it. 
            // The prompt didn't specify preview behavior, only "export button".
            // I'll leave preview as single item for simplicity and speed.
            
            webView.NavigateToString(finalHtml);
        }

        private async void BtnExport_Click(object sender, EventArgs e)
        {
            if (!isBrowserDownloaded && cmbFormat.SelectedIndex == 0)
            {
                MessageBox.Show("Browser is still downloading or failed. Cannot generate PDF yet.");
                return;
            }

            var values = GetCurrentValues();
            string processedHtml = TemplateProcessor.Process(currentTemplateHtml, values);

            // Handle Repeat
            if (chkRepeat.Checked)
            {
                int count = (int)numRepeat.Value;
                processedHtml = TemplateProcessor.RepeatTemplate(processedHtml, count);
            }
            else
            {
                // Ensure single item is also wrapped or handled correctly if needed.
                // But RepeatTemplate(html, 1) does return html raw.
                // If we want consistent styling (like grid-item class), we might want to wrap even 1 item?
                // No, usually 1 item is a full page document (like Invoice).
                // Grid is useful for small items (Business Cards).
                // So if Repeat is OFF, we assume standard behavior.
            }

            string format = cmbFormat.SelectedItem.ToString();
            
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                if (format == "PDF") sfd.Filter = "PDF Files|*.pdf";
                else if (format == "HTML") sfd.Filter = "HTML Files|*.html";
                else sfd.Filter = "Word Document|*.doc";

                sfd.FileName = "ExportedDocument";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (format == "PDF")
                        {
                            await ExportToPdf(processedHtml, sfd.FileName);
                        }
                        else
                        {
                            // HTML or Word (Fake HTML)
                            File.WriteAllText(sfd.FileName, processedHtml);
                            MessageBox.Show("File exported successfully!");
                            
                            // Try to open it
                            new System.Diagnostics.Process
                            {
                                StartInfo = new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true }
                            }.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}");
                    }
                }
            }
        }

        private async Task ExportToPdf(string html, string filePath)
        {
            btnExport.Text = "EXPORTING...";
            btnExport.Enabled = false;

            try
            {
                using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
                using (var page = await browser.NewPageAsync())
                {
                    await page.SetContentAsync(html);

                    var pdfOptions = new PdfOptions();
                    
                    // Paper Size
                    string selectedSize = cmbPaperSize.SelectedItem.ToString();
                    if (selectedSize == "A4") pdfOptions.Format = PaperFormat.A4;
                    else if (selectedSize == "Letter") pdfOptions.Format = PaperFormat.Letter;
                    else if (selectedSize == "Legal") pdfOptions.Format = PaperFormat.Legal;
                    else if (selectedSize == "Tabloid") pdfOptions.Format = PaperFormat.Tabloid;
                    else if (selectedSize == "A3") pdfOptions.Format = PaperFormat.A3;
                    else if (selectedSize == "A5") pdfOptions.Format = PaperFormat.A5;
                    else pdfOptions.Format = PaperFormat.A4;

                    pdfOptions.PrintBackground = true;
                    
                    await page.PdfAsync(filePath, pdfOptions);
                }

                MessageBox.Show("PDF Created Successfully!");
                
                // Open file
                 new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true }
                }.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Puppeteer Error: {ex.Message}");
            }
            finally
            {
                btnExport.Text = "EXPORT";
                btnExport.Enabled = true;
            }
        }
    }
}