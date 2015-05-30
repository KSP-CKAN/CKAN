namespace CKAN
{
    partial class SettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.NewRepoButton = new System.Windows.Forms.Button();
            this.RepositoryGroupBox = new System.Windows.Forms.GroupBox();
            this.DownRepoButton = new System.Windows.Forms.Button();
            this.UpRepoButton = new System.Windows.Forms.Button();
            this.DeleteRepoButton = new System.Windows.Forms.Button();
            this.ReposListBox = new System.Windows.Forms.ListBox();
            this.CacheGroupBox = new System.Windows.Forms.GroupBox();
            this.ClearCKANCacheButton = new System.Windows.Forms.Button();
            this.CKANCacheLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.KSPInstallPathLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ResetAutoStartChoice = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.CheckUpdateOnLaunchCheckbox = new System.Windows.Forms.CheckBox();
            this.InstallUpdateButton = new System.Windows.Forms.Button();
            this.LatestVersionLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.LocalVersionLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.CheckForUpdatesButton = new System.Windows.Forms.Button();
            this.RepositoryGroupBox.SuspendLayout();
            this.CacheGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // NewRepoButton
            // 
            this.NewRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NewRepoButton.Location = new System.Drawing.Point(6, 224);
            this.NewRepoButton.Name = "NewRepoButton";
            this.NewRepoButton.Size = new System.Drawing.Size(56, 26);
            this.NewRepoButton.TabIndex = 6;
            this.NewRepoButton.Text = "New";
            this.NewRepoButton.UseVisualStyleBackColor = true;
            this.NewRepoButton.Click += new System.EventHandler(this.NewRepoButton_Click);
            // 
            // RepositoryGroupBox
            // 
            this.RepositoryGroupBox.Controls.Add(this.DownRepoButton);
            this.RepositoryGroupBox.Controls.Add(this.UpRepoButton);
            this.RepositoryGroupBox.Controls.Add(this.DeleteRepoButton);
            this.RepositoryGroupBox.Controls.Add(this.ReposListBox);
            this.RepositoryGroupBox.Controls.Add(this.NewRepoButton);
            this.RepositoryGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RepositoryGroupBox.Location = new System.Drawing.Point(12, 12);
            this.RepositoryGroupBox.Name = "RepositoryGroupBox";
            this.RepositoryGroupBox.Size = new System.Drawing.Size(476, 261);
            this.RepositoryGroupBox.TabIndex = 8;
            this.RepositoryGroupBox.TabStop = false;
            this.RepositoryGroupBox.Text = "Metadata repositories";
            // 
            // DownRepoButton
            // 
            this.DownRepoButton.Enabled = false;
            this.DownRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DownRepoButton.Location = new System.Drawing.Point(130, 224);
            this.DownRepoButton.Name = "DownRepoButton";
            this.DownRepoButton.Size = new System.Drawing.Size(56, 26);
            this.DownRepoButton.TabIndex = 11;
            this.DownRepoButton.Text = "Down";
            this.DownRepoButton.UseVisualStyleBackColor = true;
            this.DownRepoButton.Click += new System.EventHandler(this.DownRepoButton_Click);
            // 
            // UpRepoButton
            // 
            this.UpRepoButton.Enabled = false;
            this.UpRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UpRepoButton.Location = new System.Drawing.Point(68, 224);
            this.UpRepoButton.Name = "UpRepoButton";
            this.UpRepoButton.Size = new System.Drawing.Size(56, 26);
            this.UpRepoButton.TabIndex = 10;
            this.UpRepoButton.Text = "Up";
            this.UpRepoButton.UseVisualStyleBackColor = true;
            this.UpRepoButton.Click += new System.EventHandler(this.UpRepoButton_Click);
            // 
            // DeleteRepoButton
            // 
            this.DeleteRepoButton.Enabled = false;
            this.DeleteRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteRepoButton.Location = new System.Drawing.Point(414, 224);
            this.DeleteRepoButton.Name = "DeleteRepoButton";
            this.DeleteRepoButton.Size = new System.Drawing.Size(56, 26);
            this.DeleteRepoButton.TabIndex = 9;
            this.DeleteRepoButton.Text = "Delete";
            this.DeleteRepoButton.UseVisualStyleBackColor = true;
            this.DeleteRepoButton.Click += new System.EventHandler(this.DeleteRepoButton_Click);
            // 
            // ReposListBox
            // 
            this.ReposListBox.FormattingEnabled = true;
            this.ReposListBox.Location = new System.Drawing.Point(6, 19);
            this.ReposListBox.Name = "ReposListBox";
            this.ReposListBox.Size = new System.Drawing.Size(464, 199);
            this.ReposListBox.TabIndex = 8;
            this.ReposListBox.SelectedIndexChanged += new System.EventHandler(this.ReposListBox_SelectedIndexChanged);
            // 
            // CacheGroupBox
            // 
            this.CacheGroupBox.Controls.Add(this.ClearCKANCacheButton);
            this.CacheGroupBox.Controls.Add(this.CKANCacheLabel);
            this.CacheGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CacheGroupBox.Location = new System.Drawing.Point(12, 279);
            this.CacheGroupBox.Name = "CacheGroupBox";
            this.CacheGroupBox.Size = new System.Drawing.Size(476, 49);
            this.CacheGroupBox.TabIndex = 9;
            this.CacheGroupBox.TabStop = false;
            this.CacheGroupBox.Text = "CKAN Cache";
            // 
            // ClearCKANCacheButton
            // 
            this.ClearCKANCacheButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ClearCKANCacheButton.Location = new System.Drawing.Point(341, 16);
            this.ClearCKANCacheButton.Name = "ClearCKANCacheButton";
            this.ClearCKANCacheButton.Size = new System.Drawing.Size(129, 23);
            this.ClearCKANCacheButton.TabIndex = 1;
            this.ClearCKANCacheButton.Text = "Clear cache";
            this.ClearCKANCacheButton.UseVisualStyleBackColor = true;
            this.ClearCKANCacheButton.Click += new System.EventHandler(this.ClearCKANCacheButton_Click);
            // 
            // CKANCacheLabel
            // 
            this.CKANCacheLabel.AutoSize = true;
            this.CKANCacheLabel.Location = new System.Drawing.Point(3, 21);
            this.CKANCacheLabel.Name = "CKANCacheLabel";
            this.CKANCacheLabel.Size = new System.Drawing.Size(273, 13);
            this.CKANCacheLabel.TabIndex = 0;
            this.CKANCacheLabel.Text = "There are currently N files in the cache, taking up M MiB";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.KSPInstallPathLabel);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.ResetAutoStartChoice);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(12, 334);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(476, 93);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "KSP Install";
            // 
            // KSPInstallPathLabel
            // 
            this.KSPInstallPathLabel.Location = new System.Drawing.Point(137, 20);
            this.KSPInstallPathLabel.Name = "KSPInstallPathLabel";
            this.KSPInstallPathLabel.Size = new System.Drawing.Size(320, 41);
            this.KSPInstallPathLabel.TabIndex = 2;
            this.KSPInstallPathLabel.Text = "label2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Current KSP install path: ";
            // 
            // ResetAutoStartChoice
            // 
            this.ResetAutoStartChoice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ResetAutoStartChoice.Location = new System.Drawing.Point(309, 64);
            this.ResetAutoStartChoice.Name = "ResetAutoStartChoice";
            this.ResetAutoStartChoice.Size = new System.Drawing.Size(161, 23);
            this.ResetAutoStartChoice.TabIndex = 0;
            this.ResetAutoStartChoice.Text = "Choose another KSP install";
            this.ResetAutoStartChoice.UseVisualStyleBackColor = true;
            this.ResetAutoStartChoice.Click += new System.EventHandler(this.ResetAutoStartChoice_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.CheckUpdateOnLaunchCheckbox);
            this.groupBox2.Controls.Add(this.InstallUpdateButton);
            this.groupBox2.Controls.Add(this.LatestVersionLabel);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.LocalVersionLabel);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.CheckForUpdatesButton);
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox2.Location = new System.Drawing.Point(12, 433);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(476, 105);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "CKAN auto-update";
            // 
            // CheckUpdateOnLaunchCheckbox
            // 
            this.CheckUpdateOnLaunchCheckbox.AutoSize = true;
            this.CheckUpdateOnLaunchCheckbox.Location = new System.Drawing.Point(275, 16);
            this.CheckUpdateOnLaunchCheckbox.Name = "CheckUpdateOnLaunchCheckbox";
            this.CheckUpdateOnLaunchCheckbox.Size = new System.Drawing.Size(195, 17);
            this.CheckUpdateOnLaunchCheckbox.TabIndex = 6;
            this.CheckUpdateOnLaunchCheckbox.Text = "Check for CKAN updates on launch";
            this.CheckUpdateOnLaunchCheckbox.UseVisualStyleBackColor = true;
            this.CheckUpdateOnLaunchCheckbox.CheckedChanged += new System.EventHandler(this.CheckUpdateOnLaunchCheckbox_CheckedChanged);
            // 
            // InstallUpdateButton
            // 
            this.InstallUpdateButton.Enabled = false;
            this.InstallUpdateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.InstallUpdateButton.Location = new System.Drawing.Point(116, 76);
            this.InstallUpdateButton.Name = "InstallUpdateButton";
            this.InstallUpdateButton.Size = new System.Drawing.Size(104, 23);
            this.InstallUpdateButton.TabIndex = 5;
            this.InstallUpdateButton.Text = "Install update";
            this.InstallUpdateButton.UseVisualStyleBackColor = true;
            this.InstallUpdateButton.Click += new System.EventHandler(this.InstallUpdateButton_Click);
            // 
            // LatestVersionLabel
            // 
            this.LatestVersionLabel.AutoSize = true;
            this.LatestVersionLabel.Location = new System.Drawing.Point(86, 43);
            this.LatestVersionLabel.Name = "LatestVersionLabel";
            this.LatestVersionLabel.Size = new System.Drawing.Size(25, 13);
            this.LatestVersionLabel.TabIndex = 4;
            this.LatestVersionLabel.Text = "???";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Latest version:";
            // 
            // LocalVersionLabel
            // 
            this.LocalVersionLabel.AutoSize = true;
            this.LocalVersionLabel.Location = new System.Drawing.Point(86, 20);
            this.LocalVersionLabel.Name = "LocalVersionLabel";
            this.LocalVersionLabel.Size = new System.Drawing.Size(37, 13);
            this.LocalVersionLabel.TabIndex = 2;
            this.LocalVersionLabel.Text = "v0.0.0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Local version:";
            // 
            // CheckForUpdatesButton
            // 
            this.CheckForUpdatesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckForUpdatesButton.Location = new System.Drawing.Point(6, 76);
            this.CheckForUpdatesButton.Name = "CheckForUpdatesButton";
            this.CheckForUpdatesButton.Size = new System.Drawing.Size(104, 23);
            this.CheckForUpdatesButton.TabIndex = 0;
            this.CheckForUpdatesButton.Text = "Check for updates";
            this.CheckForUpdatesButton.UseVisualStyleBackColor = true;
            this.CheckForUpdatesButton.Click += new System.EventHandler(this.CheckForUpdatesButton_Click);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 542);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CacheGroupBox);
            this.Controls.Add(this.RepositoryGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SettingsDialog";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsDialog_Load);
            this.RepositoryGroupBox.ResumeLayout(false);
            this.CacheGroupBox.ResumeLayout(false);
            this.CacheGroupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button NewRepoButton;
        private System.Windows.Forms.GroupBox RepositoryGroupBox;
        private System.Windows.Forms.GroupBox CacheGroupBox;
        private System.Windows.Forms.Label CKANCacheLabel;
        private System.Windows.Forms.Button ClearCKANCacheButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button ResetAutoStartChoice;
        private System.Windows.Forms.Label KSPInstallPathLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button DeleteRepoButton;
        private System.Windows.Forms.ListBox ReposListBox;
        private System.Windows.Forms.Button UpRepoButton;
        private System.Windows.Forms.Button DownRepoButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label LatestVersionLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label LocalVersionLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button CheckForUpdatesButton;
        private System.Windows.Forms.Button InstallUpdateButton;
        private System.Windows.Forms.CheckBox CheckUpdateOnLaunchCheckbox;
    }
}
