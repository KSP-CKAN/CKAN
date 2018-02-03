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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.NewRepoButton = new System.Windows.Forms.Button();
            this.RepositoryGroupBox = new System.Windows.Forms.GroupBox();
            this.DownRepoButton = new System.Windows.Forms.Button();
            this.UpRepoButton = new System.Windows.Forms.Button();
            this.DeleteRepoButton = new System.Windows.Forms.Button();
            this.ReposListBox = new System.Windows.Forms.ListBox();
            this.AuthTokensGroupBox = new System.Windows.Forms.GroupBox();
            this.AuthTokensListBox = new System.Windows.Forms.ListBox();
            this.NewAuthTokenButton = new System.Windows.Forms.Button();
            this.DeleteAuthTokenButton = new System.Windows.Forms.Button();
            this.CacheGroupBox = new System.Windows.Forms.GroupBox();
            this.ClearCKANCacheButton = new System.Windows.Forms.Button();
            this.CKANCacheLabel = new System.Windows.Forms.Label();
            this.AutoUpdateGroupBox = new System.Windows.Forms.GroupBox();
            this.HideEpochsCheckbox = new System.Windows.Forms.CheckBox();
            this.RefreshOnStartupCheckbox = new System.Windows.Forms.CheckBox();
            this.CheckUpdateOnLaunchCheckbox = new System.Windows.Forms.CheckBox();
            this.InstallUpdateButton = new System.Windows.Forms.Button();
            this.LatestVersionLabel = new System.Windows.Forms.Label();
            this.LatestVersionLabelLabel = new System.Windows.Forms.Label();
            this.LocalVersionLabel = new System.Windows.Forms.Label();
            this.LocalVersionLabelLabel = new System.Windows.Forms.Label();
            this.CheckForUpdatesButton = new System.Windows.Forms.Button();
            this.RepositoryGroupBox.SuspendLayout();
            this.AuthTokensGroupBox.SuspendLayout();
            this.CacheGroupBox.SuspendLayout();
            this.AutoUpdateGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // NewRepoButton
            //
            this.NewRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NewRepoButton.Location = new System.Drawing.Point(6, 94);
            this.NewRepoButton.Name = "NewRepoButton";
            this.NewRepoButton.Size = new System.Drawing.Size(56, 23);
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
            this.RepositoryGroupBox.Size = new System.Drawing.Size(476, 127);
            this.RepositoryGroupBox.TabIndex = 12;
            this.RepositoryGroupBox.TabStop = false;
            this.RepositoryGroupBox.Text = "Metadata Repositories";
            //
            // DownRepoButton
            //
            this.DownRepoButton.Enabled = false;
            this.DownRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DownRepoButton.Location = new System.Drawing.Point(130, 94);
            this.DownRepoButton.Name = "DownRepoButton";
            this.DownRepoButton.Size = new System.Drawing.Size(56, 23);
            this.DownRepoButton.TabIndex = 11;
            this.DownRepoButton.Text = "Down";
            this.DownRepoButton.UseVisualStyleBackColor = true;
            this.DownRepoButton.Click += new System.EventHandler(this.DownRepoButton_Click);
            //
            // UpRepoButton
            //
            this.UpRepoButton.Enabled = false;
            this.UpRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UpRepoButton.Location = new System.Drawing.Point(68, 94);
            this.UpRepoButton.Name = "UpRepoButton";
            this.UpRepoButton.Size = new System.Drawing.Size(56, 23);
            this.UpRepoButton.TabIndex = 10;
            this.UpRepoButton.Text = "Up";
            this.UpRepoButton.UseVisualStyleBackColor = true;
            this.UpRepoButton.Click += new System.EventHandler(this.UpRepoButton_Click);
            //
            // DeleteRepoButton
            //
            this.DeleteRepoButton.Enabled = false;
            this.DeleteRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteRepoButton.Location = new System.Drawing.Point(414, 94);
            this.DeleteRepoButton.Name = "DeleteRepoButton";
            this.DeleteRepoButton.Size = new System.Drawing.Size(56, 23);
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
            this.ReposListBox.Size = new System.Drawing.Size(464, 72);
            this.ReposListBox.TabIndex = 9;
            this.ReposListBox.SelectedIndexChanged += new System.EventHandler(this.ReposListBox_SelectedIndexChanged);
            //
            // AuthTokensGroupBox
            //
            this.AuthTokensGroupBox.Controls.Add(this.AuthTokensListBox);
            this.AuthTokensGroupBox.Controls.Add(this.NewAuthTokenButton);
            this.AuthTokensGroupBox.Controls.Add(this.DeleteAuthTokenButton);
            this.AuthTokensGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AuthTokensGroupBox.Location = new System.Drawing.Point(12, 145);
            this.AuthTokensGroupBox.Name = "AuthTokensGroupBox";
            this.AuthTokensGroupBox.Size = new System.Drawing.Size(476, 127);
            this.AuthTokensGroupBox.TabIndex = 13;
            this.AuthTokensGroupBox.TabStop = false;
            this.AuthTokensGroupBox.Text = "Authentication Tokens";
            //
            // AuthTokensListBox
            //
            this.AuthTokensListBox.FormattingEnabled = true;
            this.AuthTokensListBox.Location = new System.Drawing.Point(6, 19);
            this.AuthTokensListBox.Name = "AuthTokensListBox";
            this.AuthTokensListBox.Size = new System.Drawing.Size(464, 72);
            this.AuthTokensListBox.TabIndex = 14;
            this.AuthTokensListBox.SelectedIndexChanged += new System.EventHandler(this.AuthTokensListBox_SelectedIndexChanged);
            //
            // NewAuthTokenButton
            //
            this.NewAuthTokenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NewAuthTokenButton.Location = new System.Drawing.Point(6, 94);
            this.NewAuthTokenButton.Name = "NewAuthTokenButton";
            this.NewAuthTokenButton.Size = new System.Drawing.Size(56, 23);
            this.NewAuthTokenButton.TabIndex = 15;
            this.NewAuthTokenButton.Text = "New";
            this.NewAuthTokenButton.UseVisualStyleBackColor = true;
            this.NewAuthTokenButton.Click += new System.EventHandler(this.NewAuthTokenButton_Click);
            //
            // DeleteAuthTokenButton
            //
            this.DeleteAuthTokenButton.Enabled = false;
            this.DeleteAuthTokenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteAuthTokenButton.Location = new System.Drawing.Point(414, 94);
            this.DeleteAuthTokenButton.Name = "DeleteAuthTokenButton";
            this.DeleteAuthTokenButton.Size = new System.Drawing.Size(56, 23);
            this.DeleteAuthTokenButton.TabIndex = 16;
            this.DeleteAuthTokenButton.Text = "Delete";
            this.DeleteAuthTokenButton.UseVisualStyleBackColor = true;
            this.DeleteAuthTokenButton.Click += new System.EventHandler(this.DeleteAuthTokenButton_Click);
            //
            // CacheGroupBox
            //
            this.CacheGroupBox.Controls.Add(this.ClearCKANCacheButton);
            this.CacheGroupBox.Controls.Add(this.CKANCacheLabel);
            this.CacheGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CacheGroupBox.Location = new System.Drawing.Point(12, 279);
            this.CacheGroupBox.Name = "CacheGroupBox";
            this.CacheGroupBox.Size = new System.Drawing.Size(476, 49);
            this.CacheGroupBox.TabIndex = 10;
            this.CacheGroupBox.TabStop = false;
            this.CacheGroupBox.Text = "Cache";
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
            this.CKANCacheLabel.Size = new System.Drawing.Size(271, 13);
            this.CKANCacheLabel.TabIndex = 0;
            this.CKANCacheLabel.Text = "There are currently N files in the cache, taking up M MB";
            //
            // AutoUpdateGroupBox
            //
            this.AutoUpdateGroupBox.Controls.Add(this.HideEpochsCheckbox);
            this.AutoUpdateGroupBox.Controls.Add(this.RefreshOnStartupCheckbox);
            if (AutoUpdate.CanUpdate)
            {
                this.AutoUpdateGroupBox.Controls.Add(this.CheckUpdateOnLaunchCheckbox);
                this.AutoUpdateGroupBox.Controls.Add(this.InstallUpdateButton);
                this.AutoUpdateGroupBox.Controls.Add(this.LatestVersionLabel);
                this.AutoUpdateGroupBox.Controls.Add(this.LatestVersionLabelLabel);
                this.AutoUpdateGroupBox.Controls.Add(this.LocalVersionLabel);
                this.AutoUpdateGroupBox.Controls.Add(this.LocalVersionLabelLabel);
                this.AutoUpdateGroupBox.Controls.Add(this.CheckForUpdatesButton);
            }
            this.AutoUpdateGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AutoUpdateGroupBox.Location = new System.Drawing.Point(12, 334);
            this.AutoUpdateGroupBox.Name = "AutoUpdateGroupBox";
            this.AutoUpdateGroupBox.Size = new System.Drawing.Size(476, 105);
            this.AutoUpdateGroupBox.TabIndex = 11;
            this.AutoUpdateGroupBox.TabStop = false;
            this.AutoUpdateGroupBox.Text = "Auto-Updates";
            //
            // HideEpochsCheckbox
            //
            this.HideEpochsCheckbox.AutoSize = true;
            this.HideEpochsCheckbox.Location = new System.Drawing.Point(275, 70);
            this.HideEpochsCheckbox.Name = "HideEpochsCheckbox";
            this.HideEpochsCheckbox.Size = new System.Drawing.Size(195, 17);
            this.HideEpochsCheckbox.TabIndex = 8;
            this.HideEpochsCheckbox.Text = "Hide epoch numbers in mod list\r\n(Requires Restart)";
            this.HideEpochsCheckbox.UseVisualStyleBackColor = true;
            this.HideEpochsCheckbox.CheckedChanged += new System.EventHandler(this.HideEpochsCheckbox_CheckedChanged);
            //
            // RefreshOnStartupCheckbox
            //
            this.RefreshOnStartupCheckbox.AutoSize = true;
            this.RefreshOnStartupCheckbox.Location = new System.Drawing.Point(275, 43);
            this.RefreshOnStartupCheckbox.Name = "RefreshOnStartupCheckbox";
            this.RefreshOnStartupCheckbox.Size = new System.Drawing.Size(167, 17);
            this.RefreshOnStartupCheckbox.TabIndex = 7;
            this.RefreshOnStartupCheckbox.Text = "Update repositories on launch";
            this.RefreshOnStartupCheckbox.UseVisualStyleBackColor = true;
            this.RefreshOnStartupCheckbox.CheckedChanged += new System.EventHandler(this.RefreshOnStartupCheckbox_CheckedChanged);
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
            // LatestVersionLabelLabel
            //
            this.LatestVersionLabelLabel.AutoSize = true;
            this.LatestVersionLabelLabel.Location = new System.Drawing.Point(7, 43);
            this.LatestVersionLabelLabel.Name = "LatestVersionLabelLabel";
            this.LatestVersionLabelLabel.Size = new System.Drawing.Size(76, 13);
            this.LatestVersionLabelLabel.TabIndex = 3;
            this.LatestVersionLabelLabel.Text = "Latest version:";
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
            // LocalVersionLabelLabel
            //
            this.LocalVersionLabelLabel.AutoSize = true;
            this.LocalVersionLabelLabel.Location = new System.Drawing.Point(7, 20);
            this.LocalVersionLabelLabel.Name = "LocalVersionLabelLabel";
            this.LocalVersionLabelLabel.Size = new System.Drawing.Size(73, 13);
            this.LocalVersionLabelLabel.TabIndex = 1;
            this.LocalVersionLabelLabel.Text = "Local version:";
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
            this.ClientSize = new System.Drawing.Size(495, 442);
            this.Controls.Add(this.AutoUpdateGroupBox);
            this.Controls.Add(this.CacheGroupBox);
            this.Controls.Add(this.RepositoryGroupBox);
            this.Controls.Add(this.AuthTokensGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsDialog";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsDialog_Load);
            this.RepositoryGroupBox.ResumeLayout(false);
            this.AuthTokensGroupBox.ResumeLayout(false);
            this.CacheGroupBox.ResumeLayout(false);
            this.CacheGroupBox.PerformLayout();
            this.AutoUpdateGroupBox.ResumeLayout(false);
            this.AutoUpdateGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button NewRepoButton;
        private System.Windows.Forms.GroupBox RepositoryGroupBox;
        private System.Windows.Forms.GroupBox CacheGroupBox;
        private System.Windows.Forms.Label CKANCacheLabel;
        private System.Windows.Forms.Button ClearCKANCacheButton;
        private System.Windows.Forms.Button DeleteRepoButton;
        private System.Windows.Forms.ListBox ReposListBox;
        private System.Windows.Forms.Button UpRepoButton;
        private System.Windows.Forms.Button DownRepoButton;
        private System.Windows.Forms.GroupBox AuthTokensGroupBox;
        private System.Windows.Forms.ListBox AuthTokensListBox;
        private System.Windows.Forms.Button NewAuthTokenButton;
        private System.Windows.Forms.Button DeleteAuthTokenButton;
        private System.Windows.Forms.GroupBox AutoUpdateGroupBox;
        private System.Windows.Forms.Label LatestVersionLabel;
        private System.Windows.Forms.Label LatestVersionLabelLabel;
        private System.Windows.Forms.Label LocalVersionLabel;
        private System.Windows.Forms.Label LocalVersionLabelLabel;
        private System.Windows.Forms.Button CheckForUpdatesButton;
        private System.Windows.Forms.Button InstallUpdateButton;
        private System.Windows.Forms.CheckBox CheckUpdateOnLaunchCheckbox;
        private System.Windows.Forms.CheckBox RefreshOnStartupCheckbox;
        private System.Windows.Forms.CheckBox HideEpochsCheckbox;
    }
}
