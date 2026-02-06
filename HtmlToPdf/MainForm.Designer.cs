namespace HtmlToPdf
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            panelLeft = new Panel();
            grpInputs = new GroupBox();
            flowInputs = new FlowLayoutPanel();
            grpExport = new GroupBox();
            label4 = new Label();
            cmbFormat = new ComboBox();
            label3 = new Label();
            cmbPaperSize = new ComboBox();
            numRepeat = new NumericUpDown();
            chkRepeat = new CheckBox();
            grpTemplate = new GroupBox();
            btnExport = new Button();
            btnRefresh = new Button();
            label1 = new Label();
            cmbTemplates = new ComboBox();
            panelPreview = new Panel();
            lblPreview = new Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panelLeft.SuspendLayout();
            grpInputs.SuspendLayout();
            grpExport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numRepeat).BeginInit();
            grpTemplate.SuspendLayout();
            panelPreview.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.BackColor = Color.WhiteSmoke;
            splitContainer1.Panel1.Controls.Add(panelLeft);
            splitContainer1.Panel1.Padding = new Padding(10);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.BackColor = Color.DarkGray;
            splitContainer1.Panel2.Controls.Add(panelPreview);
            splitContainer1.Size = new Size(1184, 749);
            splitContainer1.SplitterDistance = 350;
            splitContainer1.TabIndex = 0;
            // 
            // panelLeft
            // 
            panelLeft.AutoScroll = true;
            panelLeft.Controls.Add(grpInputs);
            panelLeft.Controls.Add(grpExport);
            panelLeft.Controls.Add(grpTemplate);
            panelLeft.Dock = DockStyle.Fill;
            panelLeft.Location = new Point(10, 10);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(330, 729);
            panelLeft.TabIndex = 0;
            // 
            // grpInputs
            // 
            grpInputs.Controls.Add(flowInputs);
            grpInputs.Dock = DockStyle.Fill;
            grpInputs.Location = new Point(0, 140);
            grpInputs.Name = "grpInputs";
            grpInputs.Size = new Size(330, 439);
            grpInputs.TabIndex = 1;
            grpInputs.TabStop = false;
            grpInputs.Text = "Form Data";
            // 
            // flowInputs
            // 
            flowInputs.AutoScroll = true;
            flowInputs.Dock = DockStyle.Fill;
            flowInputs.FlowDirection = FlowDirection.TopDown;
            flowInputs.Location = new Point(3, 19);
            flowInputs.Name = "flowInputs";
            flowInputs.Size = new Size(324, 417);
            flowInputs.TabIndex = 0;
            flowInputs.WrapContents = false;
            // 
            // grpExport
            // 
            grpExport.Controls.Add(label4);
            grpExport.Controls.Add(cmbFormat);
            grpExport.Controls.Add(label3);
            grpExport.Controls.Add(cmbPaperSize);
            grpExport.Controls.Add(numRepeat);
            grpExport.Controls.Add(chkRepeat);
            grpExport.Dock = DockStyle.Bottom;
            grpExport.Location = new Point(0, 579);
            grpExport.Name = "grpExport";
            grpExport.Size = new Size(330, 150);
            grpExport.TabIndex = 2;
            grpExport.TabStop = false;
            grpExport.Text = "Export Settings";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(16, 83);
            label4.Name = "label4";
            label4.Size = new Size(45, 15);
            label4.TabIndex = 5;
            label4.Text = "Format";
            // 
            // cmbFormat
            // 
            cmbFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFormat.FormattingEnabled = true;
            cmbFormat.Items.AddRange(new object[] { "PDF", "HTML", "Word (.doc)" });
            cmbFormat.Location = new Point(16, 101);
            cmbFormat.Name = "cmbFormat";
            cmbFormat.Size = new Size(296, 23);
            cmbFormat.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(16, 29);
            label3.Name = "label3";
            label3.Size = new Size(60, 15);
            label3.TabIndex = 3;
            label3.Text = "Paper Size";
            // 
            // cmbPaperSize
            // 
            cmbPaperSize.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaperSize.FormattingEnabled = true;
            cmbPaperSize.Items.AddRange(new object[] { "A4", "Letter", "Legal", "Tabloid", "A3", "A5" });
            cmbPaperSize.Location = new Point(16, 47);
            cmbPaperSize.Name = "cmbPaperSize";
            cmbPaperSize.Size = new Size(296, 23);
            cmbPaperSize.TabIndex = 2;
            // 
            // numRepeat
            // 
            numRepeat.Location = new Point(232, 0);
            numRepeat.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRepeat.Name = "numRepeat";
            numRepeat.Size = new Size(56, 23);
            numRepeat.TabIndex = 1;
            numRepeat.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numRepeat.Visible = false;
            // 
            // chkRepeat
            // 
            chkRepeat.AutoSize = true;
            chkRepeat.Location = new Point(173, 0);
            chkRepeat.Name = "chkRepeat";
            chkRepeat.Size = new Size(62, 19);
            chkRepeat.TabIndex = 0;
            chkRepeat.Text = "Repeat";
            chkRepeat.UseVisualStyleBackColor = true;
            chkRepeat.Visible = false;
            // 
            // grpTemplate
            // 
            grpTemplate.Controls.Add(btnExport);
            grpTemplate.Controls.Add(btnRefresh);
            grpTemplate.Controls.Add(label1);
            grpTemplate.Controls.Add(cmbTemplates);
            grpTemplate.Dock = DockStyle.Top;
            grpTemplate.Location = new Point(0, 0);
            grpTemplate.Name = "grpTemplate";
            grpTemplate.Size = new Size(330, 140);
            grpTemplate.TabIndex = 0;
            grpTemplate.TabStop = false;
            grpTemplate.Text = "Template";
            // 
            // btnExport
            // 
            btnExport.BackColor = Color.DodgerBlue;
            btnExport.Cursor = Cursors.Hand;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnExport.ForeColor = Color.White;
            btnExport.Location = new Point(13, 80);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(299, 45);
            btnExport.TabIndex = 6;
            btnExport.Text = "EXPORT";
            btnExport.UseVisualStyleBackColor = false;
            btnExport.Click += BtnExport_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(253, 37);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(62, 23);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "Reload";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 22);
            label1.Name = "label1";
            label1.Size = new Size(90, 15);
            label1.TabIndex = 1;
            label1.Text = "Select Template";
            // 
            // cmbTemplates
            // 
            cmbTemplates.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTemplates.FormattingEnabled = true;
            cmbTemplates.Location = new Point(13, 38);
            cmbTemplates.Name = "cmbTemplates";
            cmbTemplates.Size = new Size(234, 23);
            cmbTemplates.TabIndex = 0;
            cmbTemplates.SelectedIndexChanged += CmbTemplates_SelectedIndexChanged;
            // 
            // panelPreview
            // 
            panelPreview.Controls.Add(lblPreview);
            panelPreview.Dock = DockStyle.Fill;
            panelPreview.Location = new Point(0, 0);
            panelPreview.Name = "panelPreview";
            panelPreview.Size = new Size(830, 749);
            panelPreview.TabIndex = 0;
            // 
            // lblPreview
            // 
            lblPreview.Dock = DockStyle.Fill;
            lblPreview.Font = new Font("Segoe UI", 15F);
            lblPreview.ForeColor = Color.White;
            lblPreview.Location = new Point(0, 0);
            lblPreview.Name = "lblPreview";
            lblPreview.Size = new Size(830, 749);
            lblPreview.TabIndex = 0;
            lblPreview.Text = "Loading Preview Engine...";
            lblPreview.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 749);
            Controls.Add(splitContainer1);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HtmlToPdf Converter";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panelLeft.ResumeLayout(false);
            grpInputs.ResumeLayout(false);
            grpExport.ResumeLayout(false);
            grpExport.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numRepeat).EndInit();
            grpTemplate.ResumeLayout(false);
            grpTemplate.PerformLayout();
            panelPreview.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.GroupBox grpTemplate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbTemplates;
        private System.Windows.Forms.GroupBox grpInputs;
        private System.Windows.Forms.FlowLayoutPanel flowInputs;
        private System.Windows.Forms.GroupBox grpExport;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbPaperSize;
        private System.Windows.Forms.NumericUpDown numRepeat;
        private System.Windows.Forms.CheckBox chkRepeat;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbFormat;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.Button btnRefresh;
    }
}