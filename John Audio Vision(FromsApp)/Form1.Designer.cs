namespace John_Audio_Vision_FromsApp_
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            textBox1 = new TextBox();
            Clientnames = new TextBox();
            label2 = new Label();
            dateTimePicker1 = new DateTimePicker();
            label3 = new Label();
            txtContact = new TextBox();
            label4 = new Label();
            txtAddress = new TextBox();
            txtItem = new TextBox();
            label5 = new Label();
            txtIssue = new RichTextBox();
            label6 = new Label();
            label7 = new Label();
            txtJob = new RichTextBox();
            label8 = new Label();
            Fprice = new NumericUpDown();
            RecordsGrid = new DataGridView();
            Enterbutton = new Button();
            edit_button = new Button();
            grpStatus = new GroupBox();
            radFinished = new RadioButton();
            radNotStarted = new RadioButton();
            finButton = new Button();
            AllButton = new Button();
            txtSearch = new TextBox();
            btnSearch = new Button();
            btnDelete = new Button();
            btnBackup = new Button();
            btnRestore = new Button();
            btnCancelEdit = new Button();
            ((System.ComponentModel.ISupportInitialize)Fprice).BeginInit();
            ((System.ComponentModel.ISupportInitialize)RecordsGrid).BeginInit();
            grpStatus.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 9);
            label1.Name = "label1";
            label1.Size = new Size(44, 15);
            label1.TabIndex = 0;
            label1.Text = "Client :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(0, 0);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 0;
            // 
            // Clientnames
            // 
            Clientnames.Location = new Point(57, 6);
            Clientnames.Name = "Clientnames";
            Clientnames.Size = new Size(205, 23);
            Clientnames.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 112);
            label2.Name = "label2";
            label2.Size = new Size(37, 15);
            label2.TabIndex = 2;
            label2.Text = "Date :";
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(57, 106);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(136, 23);
            dateTimePicker1.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(4, 41);
            label3.Name = "label3";
            label3.Size = new Size(121, 15);
            label3.TabIndex = 4;
            label3.Text = "Contact information :";
            // 
            // txtContact
            // 
            txtContact.Location = new Point(134, 41);
            txtContact.Name = "txtContact";
            txtContact.Size = new Size(128, 23);
            txtContact.TabIndex = 5;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(7, 75);
            label4.Name = "label4";
            label4.Size = new Size(94, 15);
            label4.TabIndex = 6;
            label4.Text = "Clients Address :";
            // 
            // txtAddress
            // 
            txtAddress.Location = new Point(107, 72);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(188, 23);
            txtAddress.TabIndex = 7;
            // 
            // txtItem
            // 
            txtItem.Location = new Point(99, 132);
            txtItem.Name = "txtItem";
            txtItem.Size = new Size(163, 23);
            txtItem.TabIndex = 8;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(7, 140);
            label5.Name = "label5";
            label5.Size = new Size(86, 15);
            label5.TabIndex = 9;
            label5.Text = "Name of Item :";
            // 
            // txtIssue
            // 
            txtIssue.Location = new Point(79, 175);
            txtIssue.Name = "txtIssue";
            txtIssue.Size = new Size(314, 59);
            txtIssue.TabIndex = 10;
            txtIssue.Text = "";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(10, 195);
            label6.Name = "label6";
            label6.Size = new Size(39, 15);
            label6.TabIndex = 11;
            label6.Text = "Issue :";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(10, 269);
            label7.Name = "label7";
            label7.Size = new Size(62, 15);
            label7.TabIndex = 13;
            label7.Text = "Job Done :";
            // 
            // txtJob
            // 
            txtJob.Location = new Point(79, 249);
            txtJob.Name = "txtJob";
            txtJob.Size = new Size(183, 59);
            txtJob.TabIndex = 12;
            txtJob.Text = "";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(10, 330);
            label8.Name = "label8";
            label8.Size = new Size(67, 15);
            label8.TabIndex = 14;
            label8.Text = "Final Price :";
            // 
            // Fprice
            // 
            Fprice.Location = new Point(79, 324);
            Fprice.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            Fprice.Name = "Fprice";
            Fprice.Size = new Size(120, 23);
            Fprice.TabIndex = 15;
            // 
            // RecordsGrid
            // 
            RecordsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            RecordsGrid.BackgroundColor = SystemColors.AppWorkspace;
            RecordsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            RecordsGrid.Location = new Point(399, 75);
            RecordsGrid.Name = "RecordsGrid";
            RecordsGrid.Size = new Size(939, 610);
            RecordsGrid.TabIndex = 16;
            // 
            // Enterbutton
            // 
            Enterbutton.BackColor = Color.Green;
            Enterbutton.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Enterbutton.ForeColor = SystemColors.ControlLightLight;
            Enterbutton.Location = new Point(12, 362);
            Enterbutton.Name = "Enterbutton";
            Enterbutton.Size = new Size(381, 42);
            Enterbutton.TabIndex = 17;
            Enterbutton.Text = "ENTER";
            Enterbutton.UseVisualStyleBackColor = false;
            Enterbutton.Click += Enterbutton_Click;
            // 
            // edit_button
            // 
            edit_button.BackColor = Color.Orange;
            edit_button.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            edit_button.ForeColor = SystemColors.ControlLightLight;
            edit_button.Location = new Point(12, 410);
            edit_button.Name = "edit_button";
            edit_button.Size = new Size(283, 42);
            edit_button.TabIndex = 21;
            edit_button.Text = " EDIT";
            edit_button.UseVisualStyleBackColor = false;
            edit_button.Click += edit_button_Click;
            // 
            // grpStatus
            // 
            grpStatus.Controls.Add(radFinished);
            grpStatus.Controls.Add(radNotStarted);
            grpStatus.Location = new Point(268, 249);
            grpStatus.Name = "grpStatus";
            grpStatus.Size = new Size(125, 100);
            grpStatus.TabIndex = 22;
            grpStatus.TabStop = false;
            grpStatus.Text = "Job Status";
            // 
            // radFinished
            // 
            radFinished.AutoSize = true;
            radFinished.Location = new Point(18, 58);
            radFinished.Name = "radFinished";
            radFinished.Size = new Size(74, 19);
            radFinished.TabIndex = 1;
            radFinished.TabStop = true;
            radFinished.Text = "Job Done";
            radFinished.UseVisualStyleBackColor = true;
            // 
            // radNotStarted
            // 
            radNotStarted.AutoSize = true;
            radNotStarted.Location = new Point(17, 22);
            radNotStarted.Name = "radNotStarted";
            radNotStarted.Size = new Size(85, 19);
            radNotStarted.TabIndex = 0;
            radNotStarted.TabStop = true;
            radNotStarted.Text = "Not Started";
            radNotStarted.UseVisualStyleBackColor = true;
            // 
            // finButton
            // 
            finButton.BackColor = SystemColors.ControlLightLight;
            finButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            finButton.ForeColor = Color.Black;
            finButton.Location = new Point(268, 549);
            finButton.Name = "finButton";
            finButton.Size = new Size(79, 46);
            finButton.TabIndex = 23;
            finButton.Text = "All finished Jobs";
            finButton.UseVisualStyleBackColor = false;
            finButton.Click += finButton_Click;
            // 
            // AllButton
            // 
            AllButton.BackColor = SystemColors.ControlLightLight;
            AllButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            AllButton.ForeColor = Color.Black;
            AllButton.Location = new Point(349, 549);
            AllButton.Name = "AllButton";
            AllButton.Size = new Size(44, 46);
            AllButton.TabIndex = 24;
            AllButton.Text = "All Jobs";
            AllButton.UseVisualStyleBackColor = false;
            AllButton.Click += AllButton_Click;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(12, 549);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(229, 23);
            txtSearch.TabIndex = 25;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(12, 578);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 23);
            btnSearch.TabIndex = 26;
            btnSearch.Text = "🔍Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.Red;
            btnDelete.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnDelete.ForeColor = SystemColors.ControlLightLight;
            btnDelete.Location = new Point(12, 458);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(381, 42);
            btnDelete.TabIndex = 27;
            btnDelete.Text = "DELETE";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnBackup
            // 
            btnBackup.FlatStyle = FlatStyle.Flat;
            btnBackup.ForeColor = Color.OrangeRed;
            btnBackup.Location = new Point(401, 12);
            btnBackup.Name = "btnBackup";
            btnBackup.Size = new Size(150, 44);
            btnBackup.TabIndex = 28;
            btnBackup.Text = "BackUp Data";
            btnBackup.UseVisualStyleBackColor = true;
            btnBackup.Click += btnBackup_Click;
            // 
            // btnRestore
            // 
            btnRestore.FlatStyle = FlatStyle.Flat;
            btnRestore.ForeColor = Color.Green;
            btnRestore.Location = new Point(632, 12);
            btnRestore.Name = "btnRestore";
            btnRestore.Size = new Size(150, 44);
            btnRestore.TabIndex = 29;
            btnRestore.Text = "Restore Data";
            btnRestore.UseVisualStyleBackColor = true;
            btnRestore.Click += btnRestore_Click;
            // 
            // btnCancelEdit
            // 
            btnCancelEdit.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCancelEdit.Location = new Point(301, 410);
            btnCancelEdit.Name = "btnCancelEdit";
            btnCancelEdit.Size = new Size(82, 42);
            btnCancelEdit.TabIndex = 30;
            btnCancelEdit.Text = "Cancel";
            btnCancelEdit.UseVisualStyleBackColor = true;
            btnCancelEdit.Click += btnCancelEdit_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(1264, 681);
            Controls.Add(btnCancelEdit);
            Controls.Add(btnRestore);
            Controls.Add(btnBackup);
            Controls.Add(btnDelete);
            Controls.Add(btnSearch);
            Controls.Add(txtSearch);
            Controls.Add(AllButton);
            Controls.Add(finButton);
            Controls.Add(grpStatus);
            Controls.Add(edit_button);
            Controls.Add(Enterbutton);
            Controls.Add(RecordsGrid);
            Controls.Add(Fprice);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(txtJob);
            Controls.Add(label6);
            Controls.Add(txtIssue);
            Controls.Add(label5);
            Controls.Add(txtItem);
            Controls.Add(txtAddress);
            Controls.Add(label4);
            Controls.Add(txtContact);
            Controls.Add(label3);
            Controls.Add(dateTimePicker1);
            Controls.Add(label2);
            Controls.Add(Clientnames);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Client Records";
            ((System.ComponentModel.ISupportInitialize)Fprice).EndInit();
            ((System.ComponentModel.ISupportInitialize)RecordsGrid).EndInit();
            grpStatus.ResumeLayout(false);
            grpStatus.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private TextBox Clientnames;
        private Label label2;
        private DateTimePicker dateTimePicker1;
        private Label label3;
        private TextBox txtContact;
        private Label label4;
        private TextBox txtAddress;
        private TextBox txtItem;
        private Label label5;
        private RichTextBox txtIssue;
        private Label label6;
        private Label label7;
        private RichTextBox txtJob;
        private Label label8;
        private NumericUpDown Fprice;
        private DataGridView RecordsGrid;
        private Button Enterbutton;
        private Button edit_button;
        private GroupBox grpStatus;
        private RadioButton radFinished;
        private RadioButton radNotStarted;
        private Button finButton;
        private Button AllButton;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnDelete;
        private Button btnBackup;
        private Button btnRestore;
        private Button btnCancelEdit;
    }
}
