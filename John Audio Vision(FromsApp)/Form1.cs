using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.Json;
using System.Windows.Forms;

namespace John_Audio_Vision_FromsApp_
{
    public partial class Form1 : Form
    {
        private bool isEditing = false;
        private string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "JohnAudioVision", "jobs.json");
        private List<RepairJob> Jobs = new List<RepairJob>();

        public Form1()
        {
            InitializeComponent();
            this.Shown += Form1_Shown;
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            LoadJobs();
            RefreshGrid();
        }

        private void Form1_Shown(object sender, EventArgs e) => RefreshGrid();

        private void SaveJobs()
        {
            try
            {
                string json = JsonSerializer.Serialize(Jobs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show($"Error saving data.\n\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadJobs()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    Jobs = JsonSerializer.Deserialize<List<RepairJob>>(json) ?? new List<RepairJob>();
                }
                else
                {
                    Jobs = new List<RepairJob>();
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show($"Error loading saved data.\n\nA new empty list will be created.\n\n{ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Jobs = new List<RepairJob>();
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                File.Copy(filePath, "backup_jobs.json", true);
                MessageBox.Show("Backup created successfully.", "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show($"Backup failed.\n\n{ex.Message}", "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists("backup_jobs.json"))
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("No backup file found.", "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                File.Copy("backup_jobs.json", filePath, true);
                LoadJobs();
                RefreshGrid();
                MessageBox.Show("Data restored successfully.", "Restore Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show($"Restore failed.\n\n{ex.Message}", "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            HighlightFinishedJobs();
        }

        private void HighlightFinishedJobs()
        {
            foreach (DataGridViewRow row in RecordsGrid.Rows)
            {
                if (row.DataBoundItem is RepairJob job)
                    row.DefaultCellStyle.BackColor = job.Status == "Finished" ? Color.LightGreen : Color.White;
            }
        }

        private void RefreshGrid()
        {
            RecordsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            RecordsGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            RecordsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            RecordsGrid.MultiSelect = false;
            RecordsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            RecordsGrid.RowHeadersVisible = false;
            RecordsGrid.ReadOnly = true;
            RecordsGrid.AllowUserToAddRows = false;
            RecordsGrid.AllowUserToDeleteRows = false;
            RecordsGrid.EditMode = DataGridViewEditMode.EditProgrammatically;
            RecordsGrid.ScrollBars = ScrollBars.Vertical;

            // Smooth scrolling via double buffering
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, RecordsGrid, new object[] { true });

            RecordsGrid.DataSource = null;
            RecordsGrid.DataSource = Jobs.OrderByDescending(j => j.Date).ToList();
            RecordsGrid.Refresh();
            Application.DoEvents();
            HighlightFinishedJobs();
        }

        private void ClearFields()
        {
            Clientnames.Clear();
            txtContact.Clear();
            txtAddress.Clear();
            txtItem.Clear();
            txtIssue.Clear();
            txtJob.Clear();
            radFinished.Checked = false;
            radNotStarted.Checked = false;
            Fprice.Value = 0;
            dateTimePicker1.Value = DateTime.Now;
        }

        private bool IsValidPhoneNumber(string phoneNumber)
            => System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^[\d\s\-\(\)\+]+$");

        private void Enterbutton_Click(object sender, EventArgs e)
        {
            if (isEditing)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Finish editing first.", "Editing Mode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validations
            if (string.IsNullOrWhiteSpace(Clientnames.Text))
            { SystemSounds.Exclamation.Play(); MessageBox.Show("Please enter client name!!!", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!radFinished.Checked && !radNotStarted.Checked)
            { SystemSounds.Exclamation.Play(); MessageBox.Show("Please select job status !!!", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtContact.Text))
            { SystemSounds.Exclamation.Play(); MessageBox.Show("Enter contact number !!!", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!IsValidPhoneNumber(txtContact.Text))
            { SystemSounds.Exclamation.Play(); MessageBox.Show("Please enter a valid contact number (digits, spaces, dashes only)", "Invalid Format", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtItem.Text))
            { SystemSounds.Exclamation.Play(); MessageBox.Show("Enter item name !!!", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (Fprice.Value < 0)
            { SystemSounds.Exclamation.Play(); MessageBox.Show("Price cannot be negative!", "Invalid Price", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string status = radFinished.Checked ? "Finished" : "Not Started";
            RepairJob job = new RepairJob
            {
                ClientName = Clientnames.Text,
                Contact = txtContact.Text,
                Address = txtAddress.Text,
                ItemName = txtItem.Text,
                Issue = txtIssue.Text,
                JobDone = txtJob.Text,
                Price = Fprice.Value,
                Date = dateTimePicker1.Value,
                Status = status
            };
            Jobs.Add(job);
            RefreshGrid();
            ClearFields();
            SaveJobs();
        }

        private void RecordsGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (isEditing || RecordsGrid.SelectedRows.Count == 0) return;
            if (RecordsGrid.SelectedRows[0].DataBoundItem is RepairJob job)
            {
                Clientnames.Text = job.ClientName;
                txtContact.Text = job.Contact;
                txtAddress.Text = job.Address;
                txtItem.Text = job.ItemName;
                txtIssue.Text = job.Issue;
                txtJob.Text = job.JobDone;
                Fprice.Value = job.Price;
                dateTimePicker1.Value = job.Date;
                radFinished.Checked = job.Status == "Finished";
                radNotStarted.Checked = job.Status == "Not Started";
            }
        }

        private void EnableEditMode()
        {
            isEditing = true;
            RecordsGrid.Enabled = false;
            Enterbutton.Enabled = false;
            btnDelete.Enabled = false;
            Enterbutton.BackColor = Color.LightGray;
            btnDelete.BackColor = Color.LightGray;
            edit_button.Text = "Save Changes";
            edit_button.BackColor = Color.LightGreen;
        }

        private void DisableEditMode()
        {
            isEditing = false;
            RecordsGrid.Enabled = true;
            Enterbutton.Enabled = true;
            btnDelete.Enabled = true;
            Enterbutton.BackColor = Color.Green;
            btnDelete.BackColor = Color.Red;
            edit_button.Text = "EDIT";
            edit_button.BackColor = Color.Orange;
            RecordsGrid.ClearSelection();
        }

        private void edit_button_Click(object sender, EventArgs e)
        {
            if (isEditing)
            {
                // Validate before saving edits
                if (string.IsNullOrWhiteSpace(Clientnames.Text) ||
                    (!radFinished.Checked && !radNotStarted.Checked) ||
                    string.IsNullOrWhiteSpace(txtContact.Text) ||
                    !IsValidPhoneNumber(txtContact.Text) ||
                    string.IsNullOrWhiteSpace(txtItem.Text) ||
                    Fprice.Value < 0)
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("Please correct the highlighted fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (RecordsGrid.SelectedRows[0].DataBoundItem is RepairJob originalJob)
                {
                    originalJob.ClientName = Clientnames.Text;
                    originalJob.Contact = txtContact.Text;
                    originalJob.Address = txtAddress.Text;
                    originalJob.ItemName = txtItem.Text;
                    originalJob.Issue = txtIssue.Text;
                    originalJob.JobDone = txtJob.Text;
                    originalJob.Price = Fprice.Value;
                    originalJob.Date = dateTimePicker1.Value;
                    originalJob.Status = radFinished.Checked ? "Finished" : "Not Started";

                    SaveJobs();
                    RefreshGrid();
                    ClearFields();
                    DisableEditMode();
                    MessageBox.Show("Job updated successfully!", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else // Enter edit mode
            {
                if (RecordsGrid.SelectedRows.Count == 0)
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("Select a record to edit", "No Record Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (RecordsGrid.SelectedRows[0].DataBoundItem is RepairJob jobToEdit)
                {
                    Clientnames.Text = jobToEdit.ClientName;
                    txtContact.Text = jobToEdit.Contact;
                    txtAddress.Text = jobToEdit.Address;
                    txtItem.Text = jobToEdit.ItemName;
                    txtIssue.Text = jobToEdit.Issue;
                    txtJob.Text = jobToEdit.JobDone;
                    Fprice.Value = jobToEdit.Price;
                    dateTimePicker1.Value = jobToEdit.Date;
                    radFinished.Checked = jobToEdit.Status == "Finished";
                    radNotStarted.Checked = jobToEdit.Status == "Not Started";
                    EnableEditMode();
                }
            }
        }

        private void finButton_Click(object sender, EventArgs e)
        {
            var finishedJobs = Jobs.Where(j => j.Status == "Finished").ToList();
            RecordsGrid.DataSource = null;
            RecordsGrid.DataSource = finishedJobs;
            HighlightFinishedJobs();
        }

        private void AllButton_Click(object sender, EventArgs e) => RefreshGrid();

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                RefreshGrid();
                return;
            }
            var results = Jobs.Where(j =>
                j.ClientName.ToLower().Contains(searchTerm) ||
                j.Contact.ToLower().Contains(searchTerm) ||
                j.ItemName.ToLower().Contains(searchTerm)).ToList();
            RecordsGrid.DataSource = null;
            RecordsGrid.DataSource = results;
            HighlightFinishedJobs();
            if (results.Count == 0)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("No jobs found matching your search.", "No match", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshGrid();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (isEditing)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Finish editing first.", "Editing Mode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (RecordsGrid.SelectedRows.Count == 0)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Select a record first", "No record selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("Are you sure you want to delete this job?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (RecordsGrid.SelectedRows[0].DataBoundItem is RepairJob job)
                    Jobs.Remove(job);
                SaveJobs();
                RefreshGrid();
            }
        }

        private void btnCancelEdit_Click(object sender, EventArgs e)
        {
            if (isEditing && MessageBox.Show("Cancel editing? Any unsaved changes will be lost.", "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DisableEditMode();
                ClearFields();
            }
        }
    }
}