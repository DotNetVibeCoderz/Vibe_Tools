using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using CsvHelper;
using FacialAttendance.Helpers;

namespace FacialAttendance.Forms
{
    public partial class ReportForm : Form
    {
        public ReportForm()
        {
            InitializeComponent();
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var logs = DatabaseHelper.GetAttendanceLogs();
                dgvData.DataSource = logs;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV File|*.csv";
                sfd.FileName = "Attendance_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var logs = DatabaseHelper.GetAttendanceLogs();
                        using (var writer = new StreamWriter(sfd.FileName))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(logs);
                        }
                        MessageBox.Show("Export Successful!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting: " + ex.Message);
                    }
                }
            }
        }
    }
}