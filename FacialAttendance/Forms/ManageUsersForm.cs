using System;
using System.Windows.Forms;
using FacialAttendance.Helpers;
using FacialAttendance.Models;
using Microsoft.VisualBasic; // For InputBox if needed or custom dialog

namespace FacialAttendance.Forms
{
    public partial class ManageUsersForm : Form
    {
        public ManageUsersForm()
        {
            InitializeComponent();
        }

        private void ManageUsersForm_Load(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var users = DatabaseHelper.GetUsers();
                dgvUsers.DataSource = users;
                
                // Optional: Hide ID column if desired, or reorder
                // dgvUsers.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0) return;

            var row = dgvUsers.SelectedRows[0];
            int id = Convert.ToInt32(row.Cells["Id"].Value);
            string name = row.Cells["Name"].Value.ToString();

            if (MessageBox.Show($"Are you sure you want to delete user '{name}'? This will also delete their attendance history and photos.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.DeleteUser(id);
                    LoadUsers();
                    MessageBox.Show("User deleted.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting user: " + ex.Message);
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0) return;

            var row = dgvUsers.SelectedRows[0];
            int id = Convert.ToInt32(row.Cells["Id"].Value);
            string currentName = row.Cells["Name"].Value.ToString();

            // Simple InputBox using VisualBasic interaction or just a small form. 
            // Since we can't easily add VB ref without project file edit, let's make a quick helper or assume we add the ref.
            // Actually, let's just make a simple custom input dialog on the fly or just use a standard way.
            // I'll create a tiny inner class form for input to avoid external dependencies.
            
            using (var form = new Form())
            {
                form.Text = "Edit Name";
                form.Size = new System.Drawing.Size(300, 150);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var txt = new TextBox() { Left = 20, Top = 20, Width = 240, Text = currentName };
                var btnOk = new Button() { Text = "OK", Left = 100, Width = 80, Top = 60, DialogResult = DialogResult.OK };
                var btnCancel = new Button() { Text = "Cancel", Left = 190, Width = 80, Top = 60, DialogResult = DialogResult.Cancel };
                
                form.Controls.Add(txt);
                form.Controls.Add(btnOk);
                form.Controls.Add(btnCancel);
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txt.Text))
                {
                    try
                    {
                        DatabaseHelper.UpdateUser(id, txt.Text.Trim());
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating user: " + ex.Message);
                    }
                }
            }
        }
    }
}