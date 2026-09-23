using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Forms;

namespace John_Audio_Vision_FromsApp_
{
    public partial class Form1 : Form
    {
        private bool isEditing = false;

        // Everything for this app lives in a folder called JohnAudioVision
        // inside the user's Documents folder. This is where jobs.json,
        // autosaves, and backup tracking files all go.
        private readonly string autoSaveFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "JohnAudioVision");

        private readonly string filePath;
        private readonly string lastBackupFilePath;
        private readonly string lastPopupFilePath;

        // This list holds every repair job while the app is running.
        // Whatever is in here is what gets shown in the grid and saved to disk.
        private List<RepairJob> Jobs = new List<RepairJob>();

        // The undo restore feature works by remembering where the "pre-restore"
        // snapshot was saved. If the user does something after restoring,
        // undoAvailable flips to false and the undo is no longer allowed.
        private string? preRestorePath = null;
        private bool undoAvailable = false;

        // The bottom banner is the yellow/red/green strip that warns the user
        // about backups. It also has its own buttons for backing up or dismissing.
        private Panel bannerPanel = null!;
        private Label bannerLabel = null!;
        private Button bannerBackupButton = null!;
        private Button bannerDismissButton = null!;
        private Button undoButton = null!;

        private bool bannerDismissedThisSession = false;

        // When looking for a USB drive, we can either find one, get cancelled
        // by the user, or find none at all. This enum keeps track of which.
        private enum DriveLookupResult { Found, Cancelled, NoneAvailable }

        public Form1()
        {
            InitializeComponent();

            // Set up the three main file paths the app uses.
            filePath = Path.Combine(autoSaveFolder, "jobs.json");
            lastBackupFilePath = Path.Combine(autoSaveFolder, "lastBackup.txt");
            lastPopupFilePath = Path.Combine(autoSaveFolder, "lastPopup.txt");

            // Make sure the folder exists before we try to read or write anything.
            if (!Directory.Exists(autoSaveFolder)) Directory.CreateDirectory(autoSaveFolder);

            // Build the custom UI pieces that aren't in the designer.
            BuildBannerUI();
            BuildUndoUI();

            this.Shown += Form1_Shown;
            this.FormClosing += Form1_FormClosing;

            // Load whatever was saved from the last session.
            LoadJobs();
            RefreshGrid();
        }

        // BANNER CODE
        // The banner sits at the bottom of the window and warns about backups.
        // It's hidden by default and only shows when the app decides it's time.
        private void BuildBannerUI()
        {
            // Make the window slightly taller so the banner has room at the bottom.
            this.ClientSize = new Size(this.ClientSize.Width, this.ClientSize.Height + 45);

            bannerPanel = new Panel
            {
                Left = 0,
                Top = this.ClientSize.Height - 45,
                Width = this.ClientSize.Width,
                Height = 45,
                BackColor = Color.LightYellow,
                Visible = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            bannerLabel = new Label
            {
                Left = 12,
                Top = 6,
                Width = bannerPanel.Width - 320,
                Height = 33,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = false
            };

            // The "Backup now" button on the banner triggers a manual backup.
            bannerBackupButton = new Button
            {
                Text = "Backup now",
                Width = 130,
                Height = 33,
                Top = 6,
                Left = bannerPanel.Width - 240,
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            bannerBackupButton.Click += (s, e) => btnBackup_Click(s, e);

            // Dismiss hides the banner for the rest of this session.
            // It comes back the next time the app is opened.
            bannerDismissButton = new Button
            {
                Text = "Dismiss",
                Width = 90,
                Height = 33,
                Top = 6,
                Left = bannerPanel.Width - 100,
                Font = new Font("Segoe UI", 9F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            bannerDismissButton.Click += (s, e) =>
            {
                bannerPanel.Visible = false;
                bannerDismissedThisSession = true;
            };

            bannerPanel.Controls.Add(bannerLabel);
            bannerPanel.Controls.Add(bannerBackupButton);
            bannerPanel.Controls.Add(bannerDismissButton);

            this.Controls.Add(bannerPanel);
        }
        // BANNER CODE ENDS

        // UNDO BUTTON CODE
        // This button only appears after a restore. If the user edits anything
        // afterwards, it turns grey and its text changes to say it's disabled.
        private void BuildUndoUI()
        {
            undoButton = new Button
            {
                Text = "Undo Restore",
                Left = 786,
                Top = 12,
                Width = 140,
                Height = 44,
                BackColor = Color.LightGreen,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Visible = false
            };
            undoButton.Click += UndoRestore_Click;

            this.Controls.Add(undoButton);
            undoButton.BringToFront();
        }
        // UNDO BUTTON CODE ENDS

        // LIFECYCLE CODE
        // These methods run when the form opens and closes. On open we
        // refresh the grid, write an autosave, and run the launch checks
        // that decide whether to nag the user about backups.
        private void Form1_Shown(object sender, EventArgs e)
        {
            RefreshGrid();
            WriteAutosaveSnapshot();
            RunLaunchChecks();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteAutosaveSnapshot();
        }

        private void WriteAutosaveSnapshot()
        {
            try
            {
                if (!Directory.Exists(autoSaveFolder)) Directory.CreateDirectory(autoSaveFolder);

                string json = JsonSerializer.Serialize(Jobs, new JsonSerializerOptions { WriteIndented = true });

                // Save to the main jobs file.
                File.WriteAllText(filePath, json);

                // Also keep a timestamped copy in case the user needs to go back.
                string autoSavePath = Path.Combine(autoSaveFolder, BuildTimestampedName("AUTOSAVES"));
                File.WriteAllText(autoSavePath, json);

                // Don't let the autosaves pile up forever.
                PruneAutosaves();
            }
            catch { /* never block startup or shutdown because of a save error */ }
        }
        // LIFECYCLE CODE ENDS

        // FILE HELPERS
        // Small utilities for building filenames, comparing files,
        // counting jobs in a file, and reading timestamps.
        private static string BuildTimestampedName(string prefix)
            => $"{prefix}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";

        private static string ComputeSha256(string path)
        {
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(File.ReadAllBytes(path));
            return Convert.ToHexString(hash);
        }

        // Comparing raw file bytes is too strict. Two files can hold the same
        // data but differ by a newline or a space. So instead, we parse both
        // files into job lists, serialize them back in a standard format,
        // and compare those strings. If they match, the data is truly identical.
        private static bool JsonFilesAreIdentical(string path1, string path2)
        {
            try
            {
                if (!File.Exists(path1) || !File.Exists(path2)) return false;

                string json1 = File.ReadAllText(path1);
                string json2 = File.ReadAllText(path2);

                var list1 = JsonSerializer.Deserialize<List<RepairJob>>(json1);
                var list2 = JsonSerializer.Deserialize<List<RepairJob>>(json2);

                if (list1 == null || list2 == null) return false;

                var options = new JsonSerializerOptions { WriteIndented = true };
                string normalized1 = JsonSerializer.Serialize(list1, options);
                string normalized2 = JsonSerializer.Serialize(list2, options);

                return normalized1 == normalized2;
            }
            catch { return false; }
        }

        private static int CountJobsInFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var jobs = JsonSerializer.Deserialize<List<RepairJob>>(json);
                return jobs?.Count ?? 0;
            }
            catch { return -1; }
        }

        private static DateTime GetFileTimestamp(string path)
        {
            try { return File.GetLastWriteTime(path); }
            catch { return DateTime.MinValue; }
        }

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
                MessageBox.Show(
                    $"Error loading saved data.\n\nA new empty list will be created.\n\n{ex.Message}",
                    "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Jobs = new List<RepairJob>();
            }
        }

        // Keep only the 30 most recent autosaves. Older ones get deleted.
        private void PruneAutosaves()
        {
            try
            {
                var autosaves = Directory.GetFiles(autoSaveFolder, "AUTOSAVES_*.json")
                    .Select(p => new FileInfo(p))
                    .OrderByDescending(fi => fi.LastWriteTime)
                    .Skip(30)
                    .ToList();

                foreach (var old in autosaves)
                {
                    try { old.Delete(); } catch { }
                }
            }
            catch { }
        }
        // FILE HELPERS END

        // BACKUP TRACKING CODE
        // These methods handle the "last backup" and "last popup" timestamps
        // so we know when to nudge the user about backing up.
        private int GetDaysSinceLastBackup()
        {
            try
            {
                if (!File.Exists(lastBackupFilePath)) return int.MaxValue;
                string content = File.ReadAllText(lastBackupFilePath).Trim();
                if (DateTime.TryParse(content, out var last))
                    return (int)(DateTime.Now - last).TotalDays;
            }
            catch { }
            return int.MaxValue;
        }

        private void RecordBackupDone()
        {
            try { File.WriteAllText(lastBackupFilePath, DateTime.Now.ToString("O")); }
            catch { }
        }

        private bool ShouldShowLaunchPopup()
        {
            try
            {
                if (!File.Exists(lastPopupFilePath)) return true;
                string content = File.ReadAllText(lastPopupFilePath).Trim();
                if (DateTime.TryParse(content, out var lastShown))
                    return (DateTime.Now - lastShown).TotalHours >= 24;
            }
            catch { }
            return true;
        }

        private void RecordPopupShown()
        {
            try { File.WriteAllText(lastPopupFilePath, DateTime.Now.ToString("O")); }
            catch { }
        }
        // BACKUP TRACKING CODE ENDS

        // DRIVE DETECTION CODE
        // Anything USB-like is what we're looking for here. E: is preferred
        // if it exists, otherwise the user gets asked to confirm or pick one.
        private static bool IsUsableRemovable(DriveInfo d)
        {
            try { return d.DriveType == DriveType.Removable && d.IsReady; }
            catch { return false; }
        }

        private string? FindDriveForAutoBackup()
        {
            try
            {
                var eDrive = new DriveInfo("E");
                if (IsUsableRemovable(eDrive)) return eDrive.Name;
            }
            catch { }

            var removable = DriveInfo.GetDrives().Where(IsUsableRemovable).ToList();

            // Only auto-backup if there's exactly one removable drive plugged in.
            // If there are several, it's safer to ask the user rather than guess.
            if (removable.Count == 1) return removable[0].Name;
            return null;
        }

        private (DriveLookupResult result, string? drive) FindDriveForManualBackup()
        {
            try
            {
                var eDrive = new DriveInfo("E");
                if (IsUsableRemovable(eDrive))
                {
                    string? confirmed = ConfirmSingleDrive(eDrive.Name);
                    return confirmed != null
                        ? (DriveLookupResult.Found, confirmed)
                        : (DriveLookupResult.Cancelled, null);
                }
            }
            catch { }

            var removable = DriveInfo.GetDrives()
                .Where(IsUsableRemovable)
                .Select(d => d.Name)
                .ToList();

            if (removable.Count == 0) return (DriveLookupResult.NoneAvailable, null);

            // Exactly one drive? Ask the user to confirm it.
            if (removable.Count == 1)
            {
                string? confirmed = ConfirmSingleDrive(removable[0]);
                return confirmed != null
                    ? (DriveLookupResult.Found, confirmed)
                    : (DriveLookupResult.Cancelled, null);
            }

            // More than one drive? Let the user pick.
            string? picked = PromptForDrive(removable);
            return picked != null
                ? (DriveLookupResult.Found, picked)
                : (DriveLookupResult.Cancelled, null);
        }

        private string? ConfirmSingleDrive(string drive)
        {
            var result = MessageBox.Show(
                $"Backup will be saved to:\n\n{drive}John's Audio Vision FILES\\\n\nProceed?",
                "Confirm Backup Location",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question);

            return result == DialogResult.OK ? drive : null;
        }

        private string? PromptForDrive(System.Collections.Generic.List<string> drives)
        {
            using var form = new Form
            {
                Text = "Choose Backup Drive",
                Width = 400,
                Height = 220,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = SystemColors.Control
            };

            var lbl = new Label
            {
                Text = "Multiple removable drives detected. Choose which one to back up to:",
                Left = 15,
                Top = 15,
                Width = 355,
                Height = 30
            };

            var list = new ListBox { Left = 15, Top = 50, Width = 355, Height = 90 };
            list.Items.AddRange(drives.Cast<object>().ToArray());
            list.SelectedIndex = 0;

            var ok = new Button { Text = "Backup", DialogResult = DialogResult.OK, Left = 210, Top = 150, Width = 75 };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 295, Top = 150, Width = 75 };

            form.Controls.Add(lbl);
            form.Controls.Add(list);
            form.Controls.Add(ok);
            form.Controls.Add(cancel);
            form.AcceptButton = ok;
            form.CancelButton = cancel;

            return form.ShowDialog(this) == DialogResult.OK && list.SelectedItem is string s ? s : null;
        }

        // When the user opens the file picker to restore, we'd like it to
        // start in the USB backup folder if one is plugged in. Otherwise
        // we fall back to the documents folder.
        private string GetBackupFolderInitialPath()
        {
            try
            {
                foreach (var d in DriveInfo.GetDrives().Where(IsUsableRemovable))
                {
                    string folder = Path.Combine(d.Name, "John's Audio Vision FILES");
                    if (Directory.Exists(folder)) return folder;
                }
            }
            catch { }

            return autoSaveFolder;
        }
        // DRIVE DETECTION CODE ENDS

        // LAUNCH CHECKS CODE
        // Every time the app starts, we look at when the last backup was.
        // If it's been a while, we try to back up silently. If we can't,
        // we show the banner and maybe a popup reminder.
        private void RunLaunchChecks()
        {
            int days = GetDaysSinceLastBackup();

            // Fresh install: don't nag. Just record today as the starting point.
            if (days == int.MaxValue)
            {
                RecordBackupDone();
                return;
            }

            if (days < 5) return;

            // Try a silent auto-backup if a single USB drive is available.
            string? drive = FindDriveForAutoBackup();
            if (!string.IsNullOrEmpty(drive))
            {
                if (DoBackupToDrive(drive, silent: true))
                {
                    return;
                }
            }

            UpdateBanner(days);

            if (ShouldShowLaunchPopup())
            {
                ShowLaunchPopup(days);
                RecordPopupShown();
            }
        }

        private void UpdateBanner(int days)
        {
            if (bannerDismissedThisSession) return;

            if (days >= 8)
            {
                bannerPanel.BackColor = Color.FromArgb(255, 205, 205);
                bannerLabel.Text = $"⚠ Your last backup was over {days} days ago. Back up now?";
                bannerBackupButton.Visible = true;
                bannerBackupButton.BackColor = Color.Red;
                bannerBackupButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                bannerBackupButton.Width = 150;
                bannerBackupButton.Left = bannerPanel.Width - 260;
                bannerDismissButton.Text = "Dismiss";
                bannerPanel.Visible = true;
            }
            else if (days >= 5)
            {
                bannerPanel.BackColor = Color.LightYellow;
                bannerLabel.Text = $"⚠ Last USB backup: {days} days ago. Consider backing up soon.";
                bannerBackupButton.Visible = true;
                bannerBackupButton.BackColor = Color.Orange;
                bannerBackupButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                bannerBackupButton.Width = 130;
                bannerBackupButton.Left = bannerPanel.Width - 240;
                bannerDismissButton.Text = "Dismiss";
                bannerPanel.Visible = true;
            }
            else
            {
                bannerPanel.Visible = false;
            }
        }

        private void ShowLaunchPopup(int days)
        {
            using var form = new Form
            {
                Text = "Backup Reminder",
                Width = 480,
                Height = 200,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = SystemColors.Control
            };

            var lbl = new Label
            {
                Text = $"No backup in {days} days.\n\nPlease insert a USB / SD card when convenient.",
                Left = 20,
                Top = 20,
                Width = 430,
                Height = 80,
                Font = new Font("Segoe UI", 10F)
            };

            var btnOk = new Button
            {
                Text = "OK, I'll do it",
                DialogResult = DialogResult.OK,
                Left = 200,
                Top = 115,
                Width = 120
            };

            var btnBackup = new Button
            {
                Text = "Back up now",
                Left = 330,
                Top = 115,
                Width = 120,
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnBackup.Click += (s, e) =>
            {
                form.DialogResult = DialogResult.Yes;
                form.Close();
            };

            form.Controls.Add(lbl);
            form.Controls.Add(btnOk);
            form.Controls.Add(btnBackup);
            form.AcceptButton = btnOk;

            var result = form.ShowDialog(this);

            if (result == DialogResult.Yes)
            {
                btnBackup_Click(this, EventArgs.Empty);
            }
        }
        // LAUNCH CHECKS CODE ENDS

        // BACKUP AND RESTORE CODE
        // This is the meat of the backup system. It writes the current jobs
        // to a USB drive, and it can also read a backup back in and replace
        // the current data (with a pre-restore snapshot kept in case of undo).
        private bool DoBackupToDrive(string drive, bool silent)
        {
            try
            {
                string backupFolder = Path.Combine(drive, "John's Audio Vision FILES");
                Directory.CreateDirectory(backupFolder);

                if (!File.Exists(filePath)) SaveJobs();

                string backupFileName = BuildTimestampedName("backupSAVED");
                string backupPath = Path.Combine(backupFolder, backupFileName);

                File.Copy(filePath, backupPath, true);

                RecordBackupDone();

                if (silent)
                {
                    // Silent backups just show a green success banner.
                    bannerPanel.BackColor = Color.FromArgb(205, 235, 205);
                    bannerLabel.Text = $"✓ Automatic backup saved to {drive}John's Audio Vision FILES\\";
                    bannerBackupButton.Visible = false;
                    bannerDismissButton.Text = "OK";
                    bannerPanel.Visible = true;
                    bannerDismissedThisSession = false;
                }
                else
                {
                    // Manual backups show a proper popup so the user knows it worked.
                    MessageBox.Show(
                        $"Backup created successfully.\n\nSaved to:\n{backupPath}",
                        "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    bannerPanel.Visible = false;
                    bannerDismissedThisSession = false;
                }

                return true;
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show(
                        $"Backup failed.\n\n{ex.Message}",
                        "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            var (result, drive) = FindDriveForManualBackup();

            switch (result)
            {
                case DriveLookupResult.Cancelled:
                    return;

                case DriveLookupResult.NoneAvailable:
                    SystemSounds.Exclamation.Play();
                    MessageBox.Show(
                        "No removable drive detected.\n\nInsert a USB or SD card and try again.",
                        "No Drive",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;

                case DriveLookupResult.Found:
                    DoBackupToDrive(drive!, silent: false);
                    return;
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                using var dialog = new OpenFileDialog
                {
                    Title = "Select a backup file to restore",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    InitialDirectory = GetBackupFolderInitialPath()
                };

                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                string selected = dialog.FileName;

                // First thing to check: is this backup identical to what we
                // already have? If so, there's nothing to do. Tell the user
                // and bail out before any confirmation dialogs show up.
                if (JsonFilesAreIdentical(selected, filePath))
                {
                    MessageBox.Show(
                        "This file is already identical to your current data.\n\nNothing to restore.",
                        "No Change",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                DateTime backupTime = GetFileTimestamp(selected);
                DateTime currentTime = GetFileTimestamp(filePath);
                bool isOlder = backupTime < currentTime;

                int backupCount = CountJobsInFile(selected);
                int currentCount = Jobs.Count;

                string warning = "";
                if (isOlder)
                {
                    warning =
                        "\n\nWARNING: This backup is OLDER than your current data.\n" +
                        $"Backup date:  {backupTime:yyyy-MM-dd HH:mm}\n" +
                        $"Current data: {currentTime:yyyy-MM-dd HH:mm}\n\n" +
                        "Anything you added or edited since the backup will be LOST.";
                }

                var confirm = MessageBox.Show(
                    $"You are about to replace your current data with:\n\n" +
                    $"File: {Path.GetFileName(selected)}\n" +
                    $"Date: {backupTime:yyyy-MM-dd HH:mm}\n" +
                    $"Jobs in file: {backupCount}\n\n" +
                    $"Your current data has {currentCount} jobs.\n\n" +
                    "A snapshot of your current data will be saved to PRERESTORE first." +
                    warning + "\n\nProceed?",
                    "Confirm Restore",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                // Take a snapshot of the current data before we overwrite it.
                // This is what the Undo Restore button will use.
                preRestorePath = Path.Combine(autoSaveFolder, BuildTimestampedName("PRERESTORE"));
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, preRestorePath, true);
                }

                // Overwrite the current jobs file with the chosen backup.
                File.Copy(selected, filePath, true);
                LoadJobs();
                RefreshGrid();

                // Now undo is available until the user makes a change.
                undoAvailable = true;
                undoButton.Visible = true;
                undoButton.Enabled = true;
                undoButton.BackColor = Color.LightGreen;
                undoButton.Text = "Undo Restore";

                MessageBox.Show(
                    $"Data restored successfully.\n\n" +
                    $"Your previous data was saved to:\n{preRestorePath}\n\n" +
                    "You can undo this restore until you close the app or make a change.",
                    "Restore Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show(
                    $"Restore failed.\n\n{ex.Message}",
                    "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // BACKUP AND RESTORE CODE ENDS

        // UNDO RESTORE CODE
        // If the user clicks Undo Restore, we copy the PRERESTORE snapshot
        // back over the jobs file. After that, undo is gone for good.
        private void UndoRestore_Click(object? sender, EventArgs e)
        {
            if (!undoAvailable || string.IsNullOrEmpty(preRestorePath))
            {
                MessageBox.Show(
                    "Undo is no longer available — you've made changes since the restore.",
                    "Undo Unavailable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(preRestorePath))
            {
                MessageBox.Show(
                    "The PRERESTORE file is missing. Cannot undo.",
                    "Undo Unavailable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                undoAvailable = false;
                undoButton.Visible = false;
                return;
            }

            try
            {
                File.Copy(preRestorePath, filePath, true);
                LoadJobs();
                RefreshGrid();

                undoAvailable = false;
                undoButton.Visible = false;

                MessageBox.Show(
                    "Previous data restored successfully.",
                    "Undo Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show(
                    $"Undo failed.\n\n{ex.Message}",
                    "Undo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Called when the user does something that invalidates the undo.
        // The button stays clickable so the user can find out why it's greyed out.
        private void DisableUndoIfActive()
        {
            if (!undoAvailable) return;

            undoAvailable = false;
            undoButton.BackColor = Color.LightGray;
            undoButton.Text = "Undo Restore\n(disabled)";
        }
        // UNDO RESTORE CODE ENDS

        // GRID CODE
        // Refreshing the grid means clearing it, re-binding the job list
        // sorted by date, and painting finished rows green.
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
            // Set up the grid so it looks clean and wraps text properly.
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

            // Turn on double buffering so the grid doesn't flicker when it refreshes.
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, RecordsGrid, new object[] { true });

            // Bind the list of jobs, newest first.
            RecordsGrid.DataSource = null;
            RecordsGrid.DataSource = Jobs.OrderByDescending(j => j.Date).ToList();
            RecordsGrid.Refresh();
            Application.DoEvents();
            HighlightFinishedJobs();
        }
        // GRID CODE ENDS

        // INPUT HANDLING CODE
        // Clearing the form fields and checking that a phone number
        // looks vaguely like a phone number before we save it.
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
        // INPUT HANDLING CODE ENDS

        // ADD / EDIT / DELETE CODE
        // The main CRUD operations. Anything that changes the job list
        // also kills the undo restore, since undo only works if nothing
        // has changed since the restore happened.
        private void Enterbutton_Click(object sender, EventArgs e)
        {
            if (isEditing)
            {
                SystemSounds.Exclamation.Play();
                MessageBox.Show("Finish editing first.", "Editing Mode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Basic validation of the form fields.
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

            DisableUndoIfActive();

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

        // When the user selects a row in the grid, fill the form with that job's data.
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
                // Re-validate the form before saving the edit.
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
                    DisableUndoIfActive();

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
            else
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

        // Show only jobs with a "Finished" status.
        private void finButton_Click(object sender, EventArgs e)
        {
            var finishedJobs = Jobs.Where(j => j.Status == "Finished").ToList();
            RecordsGrid.DataSource = null;
            RecordsGrid.DataSource = finishedJobs;
            HighlightFinishedJobs();
        }

        // Show every job again.
        private void AllButton_Click(object sender, EventArgs e) => RefreshGrid();

        // Search across client name, contact, and item name.
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
                DisableUndoIfActive();

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
        // ADD / EDIT / DELETE CODE ENDS
    }
}