using Microsoft.VisualBasic;
using System.IO;// added for saving and loading data
using System.Linq; // added since i am using LINQ for filtering
using System.Media;
using System.Net.Sockets;
using System.Text.Json;  // added for saving and loading data
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;


namespace John_Audio_Vision_FromsApp_
{
    public partial class Form1 : Form

    {
        private bool isEditing = false; // variable to control the state of my buttons



        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"JohnAudioVision","jobs.json");//global variable for storing our data

        //saving our data
        private void SaveJobs()
        {
            try //save data to jobs.json file 
            {
                string json = JsonSerializer.Serialize(Jobs, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex) // give user feedback if there is something that happen / or they did something that stops the process.It should not just crash 
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show(
                    "Error saving data.\n\n" + ex.Message,
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        List<RepairJob> Jobs = new List<RepairJob>();  //Global variable placed outside of contructor so it can be accessed by other methods .Where all records are stored

        public Form1() // our constructor to create our object based on our blueprint (RepairJob class) and call load method 
        {
            InitializeComponent();

            // CREATE DIRECTORY HERE - RIGHT AFTER InitializeComponent()
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            LoadJobs();
            RefreshGrid();
        }
        //Loading our data
        private void LoadJobs()
        {
            try // tries to read and load the data 1st then show feedback if there is any issue but the system will not crash and i new list is made
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);

                    Jobs = JsonSerializer.Deserialize<List<RepairJob>>(json)
                           ?? new List<RepairJob>();
                }
                else
                {
                    Jobs = new List<RepairJob>();
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show(
                    "Error loading saved data.\n\nA new empty list will be created.\n\n" + ex.Message,
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                Jobs = new List<RepairJob>();
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                File.Copy(filePath, "backup_jobs.json", true);// try to copy data from jobs.json to backup_jobs.json

                MessageBox.Show(
                    "Backup created successfully.",
                    "Backup Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show(
                    "Backup failed.\n\n" + ex.Message,
                    "Backup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists("backup_jobs.json")) // check if the backup_jobs.json file is not found the user will get feedback 
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show(
                        "No backup file found.",
                        "Restore Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                File.Copy("backup_jobs.json", filePath, true);

                LoadJobs();
                RefreshGrid();

                MessageBox.Show(
                    "Data restored successfully.",
                    "Restore Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show(
                    "Restore failed.\n\n" + ex.Message,
                    "Restore Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            HighlightFinishedJobs();

        }


        private void HighlightFinishedJobs()
        {
            foreach (DataGridViewRow row in RecordsGrid.Rows)
            {
                RepairJob job = row.DataBoundItem as RepairJob;

                if (job != null && job.Status == "Finished")
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                else
                    row.DefaultCellStyle.BackColor = Color.White;
            }
        }





        private void RefreshGrid() // clears the grid and displays data after pressing the enter button , also checks if the repair job is finished then colours it green if done 
        {

            RecordsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // makes more room for text
            RecordsGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True; // text wrapping so i dont need to hover anymore to see full text 
            RecordsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;// cells grow to fit text when wrapping
            RecordsGrid.MultiSelect = false; //only can select one record at a time
            RecordsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // select the whole row not indivial rows 
            RecordsGrid.RowHeadersVisible = false;
            // prevent user from making any changes directly from the grid and make to display only
            RecordsGrid.ReadOnly = true;
            RecordsGrid.AllowUserToAddRows = false;
            RecordsGrid.AllowUserToDeleteRows = false;
            RecordsGrid.EditMode = DataGridViewEditMode.EditProgrammatically;
            //where data is gotten from          
            RecordsGrid.DataSource = null;
            RecordsGrid.DataSource = Jobs.OrderByDescending(j => j.Date).ToList();// Orders the records by date .(NB:RecordsGrid.DataSource is always expecting a List  ) 


            HighlightFinishedJobs();


        }
        private void ClearFields() // clears the fields after a record is entered 
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
        private bool IsValidPhoneNumber(string phoneNumber)//method to make sure the user only puts numbers , spaces, dashes, parentheses, and plus sign for in the contact textfield

        {

            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^[\d\s\-\(\)\+]+$");
        }

        private void Enterbutton_Click(object sender, EventArgs e) // Event handler for when the user press the enter button
        {
            if (isEditing) // if the editing mode is on and the user tries to click the button then theyll get a message
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Finish editing first.", "Editing Mode",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (string.IsNullOrWhiteSpace(Clientnames.Text)) // A condition for if the client name textbox is empty
            {
                SystemSounds.Exclamation.Play();// sound for messageBox pop up
                MessageBox.Show("Please enter client name!!!", "Missing Information" /*What shows up at the messageBox header*/, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }
            if (!radFinished.Checked && !radNotStarted.Checked)// if the user has not selected a job status ,they will be shown a message telling them to be pick one
            {
                SystemSounds.Exclamation.Play();// sound for messageBox pop up
                MessageBox.Show("Please select job status !!!", "Missing Information" /*What shows up at the messageBox header*/, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }
            if (string.IsNullOrWhiteSpace(txtContact.Text))
            {
                SystemSounds.Exclamation.Play();// sound for messageBox pop up
                MessageBox.Show("Enter contact number !!!", "Missing Information" /*What shows up at the messageBox header*/, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!IsValidPhoneNumber(txtContact.Text)) // error mesaage to pop up if there is an invalid inputs in the contact textfield (input is not numbers , spaces, dashes, parentheses, and plus sign)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Please enter a valid contact number (digits, spaces, dashes only)",
                    "Invalid Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (string.IsNullOrWhiteSpace(txtItem.Text))
            {
                SystemSounds.Exclamation.Play();// sound for messageBox pop up
                MessageBox.Show("Enter item name !!!", "Missing Information" /*What shows up at the messageBox header*/, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Fprice.Value < 0) // valiadtion for the price, so price can never be negative
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Price cannot be negative!", "Invalid Price",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string status = "";

            if (radFinished.Checked)
                status = "Finished";
            else
                status = "Not Started";

            RepairJob job = new RepairJob() // this is for inputting the values for each variable/property 
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

            Jobs.Add(job); // a new record is inserted into the list 

            RefreshGrid();
            ClearFields();
            SaveJobs();
        }

        private void RecordsGrid_SelectionChanged(object sender, EventArgs e)
        {
            // Don't fill fields if currently editing
            if (isEditing)
                return;

            if (RecordsGrid.SelectedRows.Count == 0)
            {
                ClearFields();
                return;
            }

            RepairJob job = RecordsGrid.SelectedRows[0].DataBoundItem as RepairJob;

            if (job == null)
                return;

            // Fill fields (where to get values)
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


        private void EnableEditMode()
        {
            isEditing = true;

            // Disable grid to prevent row switching
            RecordsGrid.Enabled = false;

            // Disable other buttons
            Enterbutton.Enabled = false;
            btnDelete.Enabled = false;

            // Visual feedback
            Enterbutton.BackColor = Color.LightGray;
            btnDelete.BackColor = Color.LightGray;

            // Change Edit button to Save button
            edit_button.Text = "Save Changes";
            edit_button.BackColor = Color.LightGreen;
        }

        private void DisableEditMode()
        {
            isEditing = false;

            // Re-enable grid
            RecordsGrid.Enabled = true;

            // Re-enable buttons
            Enterbutton.Enabled = true;
            btnDelete.Enabled = true;

            // Reset colors
            Enterbutton.BackColor = Color.Green;
            btnDelete.BackColor = Color.Red;
            edit_button.BackColor = Color.Orange;

            // Change back to Edit button
            edit_button.Text = "EDIT";
            edit_button.BackColor = Color.Orange;

            RecordsGrid.ClearSelection();
        }





        private void edit_button_Click(object sender, EventArgs e)
        {
            if (isEditing)
            {
                //in editmode and User is finishing their edit

                // Validation
                if (string.IsNullOrWhiteSpace(Clientnames.Text))
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("Client name cannot be empty!", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!radFinished.Checked && !radNotStarted.Checked)
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("Please select job status!", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtContact.Text))
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("Contact number cannot be empty!", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!IsValidPhoneNumber(txtContact.Text))
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("Please enter a valid contact number!",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtItem.Text))
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("Item name cannot be empty!", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (Fprice.Value < 0)
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("Price cannot be negative!", "Invalid Price",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the original job and update
                RepairJob originalJob = RecordsGrid.SelectedRows[0].DataBoundItem as RepairJob;

                if (originalJob != null)
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

                    MessageBox.Show("Job updated successfully!", "Update Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                //when clicked edit button

                if (RecordsGrid.SelectedRows.Count == 0)
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show("Select a record to edit", "No Record Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                RepairJob jobToEdit = RecordsGrid.SelectedRows[0].DataBoundItem as RepairJob;

                if (jobToEdit != null)
                {
                    // Fill fields when edit starts
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


        private void finButton_Click(object sender, EventArgs e) //Event handler for when the user filters jobs based on job status
        {
            var finishedJobs = Jobs.Where(j => j.Status == "Finished").ToList(); // if the job status is "Finished" , the job is added to the list of FinishedJobs

            RecordsGrid.DataSource = null;
            RecordsGrid.DataSource = finishedJobs;
            foreach (DataGridViewRow row in RecordsGrid.Rows) // colours the finished jobs green
            {
                RepairJob job = row.DataBoundItem as RepairJob;

                if (job != null && job.Status == "Finished")
                {
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }
            }

            HighlightFinishedJobs();

        }

        private void AllButton_Click(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.ToLower();// to store user input for
                                                         // what to search

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                RefreshGrid(); // Show all if search is empty
                return;
            }

            var searchResults = Jobs.Where(j =>
                j.ClientName.ToLower().Contains(searchTerm) ||
                j.Contact.ToLower().Contains(searchTerm) ||
                j.ItemName.ToLower().Contains(searchTerm)
            ).ToList(); // all records that contain the string stored in the searchTerm string will be put in a list which is searchResults 

            RecordsGrid.DataSource = null;// refreshes the grid
            RecordsGrid.DataSource = searchResults;// shows all likelly results

            foreach (DataGridViewRow row in RecordsGrid.Rows)
            {
                RepairJob job = row.DataBoundItem as RepairJob;
                if (job != null)
                {
                    row.DefaultCellStyle.BackColor = job.Status == "Finished" ? Color.LightGreen : Color.White;
                }
            }

            if (searchResults.Count == 0)
            {
                SystemSounds.Exclamation.Play();// sound for messageBox pop up

                MessageBox.Show("No jobs found matching your search.", "No match " /*What shows up at the messageBox header*/, MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshGrid();
            }
            HighlightFinishedJobs();

        }

        private void btnDelete_Click(object sender, EventArgs e) // delete event handler
        {
            if (isEditing) // if the editing mode is on and the user tries to click the button then theyll get a message
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Finish editing first.", "Editing Mode",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (RecordsGrid.SelectedRows.Count == 0) // error message if the user clicks the delete button without selecting a record first  
            {
                SystemSounds.Exclamation.Play();// sound for messageBox pop up
                MessageBox.Show("Select a record first", "No record selected " /*What shows up at the messageBox header*/, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirm deletion
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this job? ",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (result == DialogResult.Yes)
            {
                RepairJob job = RecordsGrid.SelectedRows[0].DataBoundItem as RepairJob; Jobs.Remove(job);

                SaveJobs();
                RefreshGrid();
            }
        }

        private void btnCancelEdit_Click(object sender, EventArgs e)
        {
            if (isEditing)
            {
                DialogResult result = MessageBox.Show(
                    "Cancel editing? Any unsaved changes will be lost.",
                    "Confirm Cancel",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    DisableEditMode();
                    ClearFields();
                }
            }
        }
    }
}


