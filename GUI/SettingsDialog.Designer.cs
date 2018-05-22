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
            this.RefreshOnStartupCheckbox = new System.Windows.Forms.CheckBox();
            this.CheckUpdateOnLaunchCheckbox = new System.Windows.Forms.CheckBox();
            this.InstallUpdateButton = new System.Windows.Forms.Button();
            this.LatestVersionLabel = new System.Windows.Forms.Label();
            this.LatestVersionLabelLabel = new System.Windows.Forms.Label();
            this.LocalVersionLabel = new System.Windows.Forms.Label();
            this.LocalVersionLabelLabel = new System.Windows.Forms.Label();
            this.CheckForUpdatesButton = new System.Windows.Forms.Button();
            this.HideEpochsCheckbox = new System.Windows.Forms.CheckBox();
            this.MoreSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.AutoSortUpdateCheckBox = new System.Windows.Forms.CheckBox();
            this.HideVCheckbox = new System.Windows.Forms.CheckBox();
            this.RepositoryGroupBox.SuspendLayout();
            this.AuthTokensGroupBox.SuspendLayout();
            this.CacheGroupBox.SuspendLayout();
            this.AutoUpdateGroupBox.SuspendLayout();
            this.MoreSettingsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // NewRepoButton
            // 
            this.NewRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NewRepoButton.Location = new System.Drawing.Point(8, 116);
            this.NewRepoButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.NewRepoButton.Name = "NewRepoButton";
            this.NewRepoButton.Size = new System.Drawing.Size(75, 28);
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
            this.RepositoryGroupBox.Location = new System.Drawing.Point(16, 15);
            this.RepositoryGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RepositoryGroupBox.Name = "RepositoryGroupBox";
            this.RepositoryGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RepositoryGroupBox.Size = new System.Drawing.Size(635, 156);
            this.RepositoryGroupBox.TabIndex = 12;
            this.RepositoryGroupBox.TabStop = false;
            this.RepositoryGroupBox.Text = "Metadata Repositories";
            // 
            // DownRepoButton
            // 
            this.DownRepoButton.Enabled = false;
            this.DownRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DownRepoButton.Location = new System.Drawing.Point(173, 116);
            this.DownRepoButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DownRepoButton.Name = "DownRepoButton";
            this.DownRepoButton.Size = new System.Drawing.Size(75, 28);
            this.DownRepoButton.TabIndex = 11;
            this.DownRepoButton.Text = "Down";
            this.DownRepoButton.UseVisualStyleBackColor = true;
            this.DownRepoButton.Click += new System.EventHandler(this.DownRepoButton_Click);
            // 
            // UpRepoButton
            // 
            this.UpRepoButton.Enabled = false;
            this.UpRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UpRepoButton.Location = new System.Drawing.Point(91, 116);
            this.UpRepoButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.UpRepoButton.Name = "UpRepoButton";
            this.UpRepoButton.Size = new System.Drawing.Size(75, 28);
            this.UpRepoButton.TabIndex = 10;
            this.UpRepoButton.Text = "Up";
            this.UpRepoButton.UseVisualStyleBackColor = true;
            this.UpRepoButton.Click += new System.EventHandler(this.UpRepoButton_Click);
            // 
            // DeleteRepoButton
            // 
            this.DeleteRepoButton.Enabled = false;
            this.DeleteRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteRepoButton.Location = new System.Drawing.Point(552, 116);
            this.DeleteRepoButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DeleteRepoButton.Name = "DeleteRepoButton";
            this.DeleteRepoButton.Size = new System.Drawing.Size(75, 28);
            this.DeleteRepoButton.TabIndex = 9;
            this.DeleteRepoButton.Text = "Delete";
            this.DeleteRepoButton.UseVisualStyleBackColor = true;
            this.DeleteRepoButton.Click += new System.EventHandler(this.DeleteRepoButton_Click);
            // 
            // ReposListBox
            // 
            this.ReposListBox.FormattingEnabled = true;
            this.ReposListBox.ItemHeight = 16;
            this.ReposListBox.Location = new System.Drawing.Point(8, 23);
            this.ReposListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ReposListBox.Name = "ReposListBox";
            this.ReposListBox.Size = new System.Drawing.Size(617, 84);
            this.ReposListBox.TabIndex = 9;
            this.ReposListBox.SelectedIndexChanged += new System.EventHandler(this.ReposListBox_SelectedIndexChanged);
            // 
            // AuthTokensGroupBox
            // 
            this.AuthTokensGroupBox.Controls.Add(this.AuthTokensListBox);
            this.AuthTokensGroupBox.Controls.Add(this.NewAuthTokenButton);
            this.AuthTokensGroupBox.Controls.Add(this.DeleteAuthTokenButton);
            this.AuthTokensGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AuthTokensGroupBox.Location = new System.Drawing.Point(16, 178);
            this.AuthTokensGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AuthTokensGroupBox.Name = "AuthTokensGroupBox";
            this.AuthTokensGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AuthTokensGroupBox.Size = new System.Drawing.Size(635, 156);
            this.AuthTokensGroupBox.TabIndex = 13;
            this.AuthTokensGroupBox.TabStop = false;
            this.AuthTokensGroupBox.Text = "Authentication Tokens";
            // 
            // AuthTokensListBox
            // 
            this.AuthTokensListBox.FormattingEnabled = true;
            this.AuthTokensListBox.ItemHeight = 16;
            this.AuthTokensListBox.Location = new System.Drawing.Point(8, 23);
            this.AuthTokensListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AuthTokensListBox.Name = "AuthTokensListBox";
            this.AuthTokensListBox.Size = new System.Drawing.Size(617, 84);
            this.AuthTokensListBox.TabIndex = 14;
            this.AuthTokensListBox.SelectedIndexChanged += new System.EventHandler(this.AuthTokensListBox_SelectedIndexChanged);
            // 
            // NewAuthTokenButton
            // 
            this.NewAuthTokenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NewAuthTokenButton.Location = new System.Drawing.Point(8, 116);
            this.NewAuthTokenButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.NewAuthTokenButton.Name = "NewAuthTokenButton";
            this.NewAuthTokenButton.Size = new System.Drawing.Size(75, 28);
            this.NewAuthTokenButton.TabIndex = 15;
            this.NewAuthTokenButton.Text = "New";
            this.NewAuthTokenButton.UseVisualStyleBackColor = true;
            this.NewAuthTokenButton.Click += new System.EventHandler(this.NewAuthTokenButton_Click);
            // 
            // DeleteAuthTokenButton
            // 
            this.DeleteAuthTokenButton.Enabled = false;
            this.DeleteAuthTokenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteAuthTokenButton.Location = new System.Drawing.Point(552, 116);
            this.DeleteAuthTokenButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DeleteAuthTokenButton.Name = "DeleteAuthTokenButton";
            this.DeleteAuthTokenButton.Size = new System.Drawing.Size(75, 28);
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
            this.CacheGroupBox.Location = new System.Drawing.Point(16, 343);
            this.CacheGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.CacheGroupBox.Name = "CacheGroupBox";
            this.CacheGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.CacheGroupBox.Size = new System.Drawing.Size(635, 60);
            this.CacheGroupBox.TabIndex = 10;
            this.CacheGroupBox.TabStop = false;
            this.CacheGroupBox.Text = "Cache";
            // 
            // ClearCKANCacheButton
            // 
            this.ClearCKANCacheButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ClearCKANCacheButton.Location = new System.Drawing.Point(455, 20);
            this.ClearCKANCacheButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ClearCKANCacheButton.Name = "ClearCKANCacheButton";
            this.ClearCKANCacheButton.Size = new System.Drawing.Size(172, 28);
            this.ClearCKANCacheButton.TabIndex = 1;
            this.ClearCKANCacheButton.Text = "Clear cache";
            this.ClearCKANCacheButton.UseVisualStyleBackColor = true;
            this.ClearCKANCacheButton.Click += new System.EventHandler(this.ClearCKANCacheButton_Click);
            // 
            // CKANCacheLabel
            // 
            this.CKANCacheLabel.AutoSize = true;
            this.CKANCacheLabel.Location = new System.Drawing.Point(4, 26);
            this.CKANCacheLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CKANCacheLabel.Name = "CKANCacheLabel";
            this.CKANCacheLabel.Size = new System.Drawing.Size(359, 17);
            this.CKANCacheLabel.TabIndex = 0;
            this.CKANCacheLabel.Text = "There are currently N files in the cache, taking up M MB";
            // 
            // AutoUpdateGroupBox
            // 
            this.AutoUpdateGroupBox.Controls.Add(this.RefreshOnStartupCheckbox);
            this.AutoUpdateGroupBox.Controls.Add(this.CheckUpdateOnLaunchCheckbox);
            this.AutoUpdateGroupBox.Controls.Add(this.InstallUpdateButton);
            this.AutoUpdateGroupBox.Controls.Add(this.LatestVersionLabel);
            this.AutoUpdateGroupBox.Controls.Add(this.LatestVersionLabelLabel);
            this.AutoUpdateGroupBox.Controls.Add(this.LocalVersionLabel);
            this.AutoUpdateGroupBox.Controls.Add(this.LocalVersionLabelLabel);
            this.AutoUpdateGroupBox.Controls.Add(this.CheckForUpdatesButton);
            this.AutoUpdateGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AutoUpdateGroupBox.Location = new System.Drawing.Point(16, 411);
            this.AutoUpdateGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AutoUpdateGroupBox.Name = "AutoUpdateGroupBox";
            this.AutoUpdateGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AutoUpdateGroupBox.Size = new System.Drawing.Size(635, 129);
            this.AutoUpdateGroupBox.TabIndex = 11;
            this.AutoUpdateGroupBox.TabStop = false;
            this.AutoUpdateGroupBox.Text = "Auto-Updates";
            // 
            // RefreshOnStartupCheckbox
            // 
            this.RefreshOnStartupCheckbox.AutoSize = true;
            this.RefreshOnStartupCheckbox.Location = new System.Drawing.Point(367, 53);
            this.RefreshOnStartupCheckbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RefreshOnStartupCheckbox.Name = "RefreshOnStartupCheckbox";
            this.RefreshOnStartupCheckbox.Size = new System.Drawing.Size(220, 21);
            this.RefreshOnStartupCheckbox.TabIndex = 7;
            this.RefreshOnStartupCheckbox.Text = "Update repositories on launch";
            this.RefreshOnStartupCheckbox.UseVisualStyleBackColor = true;
            this.RefreshOnStartupCheckbox.CheckedChanged += new System.EventHandler(this.RefreshOnStartupCheckbox_CheckedChanged);
            // 
            // CheckUpdateOnLaunchCheckbox
            // 
            this.CheckUpdateOnLaunchCheckbox.AutoSize = true;
            this.CheckUpdateOnLaunchCheckbox.Location = new System.Drawing.Point(367, 20);
            this.CheckUpdateOnLaunchCheckbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.CheckUpdateOnLaunchCheckbox.Name = "CheckUpdateOnLaunchCheckbox";
            this.CheckUpdateOnLaunchCheckbox.Size = new System.Drawing.Size(252, 21);
            this.CheckUpdateOnLaunchCheckbox.TabIndex = 6;
            this.CheckUpdateOnLaunchCheckbox.Text = "Check for CKAN updates on launch";
            this.CheckUpdateOnLaunchCheckbox.UseVisualStyleBackColor = true;
            this.CheckUpdateOnLaunchCheckbox.CheckedChanged += new System.EventHandler(this.CheckUpdateOnLaunchCheckbox_CheckedChanged);
            // 
            // InstallUpdateButton
            // 
            this.InstallUpdateButton.Enabled = false;
            this.InstallUpdateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.InstallUpdateButton.Location = new System.Drawing.Point(155, 94);
            this.InstallUpdateButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.InstallUpdateButton.Name = "InstallUpdateButton";
            this.InstallUpdateButton.Size = new System.Drawing.Size(139, 28);
            this.InstallUpdateButton.TabIndex = 5;
            this.InstallUpdateButton.Text = "Install update";
            this.InstallUpdateButton.UseVisualStyleBackColor = true;
            this.InstallUpdateButton.Click += new System.EventHandler(this.InstallUpdateButton_Click);
            // 
            // LatestVersionLabel
            // 
            this.LatestVersionLabel.AutoSize = true;
            this.LatestVersionLabel.Location = new System.Drawing.Point(115, 53);
            this.LatestVersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LatestVersionLabel.Name = "LatestVersionLabel";
            this.LatestVersionLabel.Size = new System.Drawing.Size(32, 17);
            this.LatestVersionLabel.TabIndex = 4;
            this.LatestVersionLabel.Text = "???";
            // 
            // LatestVersionLabelLabel
            // 
            this.LatestVersionLabelLabel.AutoSize = true;
            this.LatestVersionLabelLabel.Location = new System.Drawing.Point(9, 53);
            this.LatestVersionLabelLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LatestVersionLabelLabel.Name = "LatestVersionLabelLabel";
            this.LatestVersionLabelLabel.Size = new System.Drawing.Size(101, 17);
            this.LatestVersionLabelLabel.TabIndex = 3;
            this.LatestVersionLabelLabel.Text = "Latest version:";
            // 
            // LocalVersionLabel
            // 
            this.LocalVersionLabel.AutoSize = true;
            this.LocalVersionLabel.Location = new System.Drawing.Point(115, 25);
            this.LocalVersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LocalVersionLabel.Name = "LocalVersionLabel";
            this.LocalVersionLabel.Size = new System.Drawing.Size(47, 17);
            this.LocalVersionLabel.TabIndex = 2;
            this.LocalVersionLabel.Text = "v0.0.0";
            // 
            // LocalVersionLabelLabel
            // 
            this.LocalVersionLabelLabel.AutoSize = true;
            this.LocalVersionLabelLabel.Location = new System.Drawing.Point(9, 25);
            this.LocalVersionLabelLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LocalVersionLabelLabel.Name = "LocalVersionLabelLabel";
            this.LocalVersionLabelLabel.Size = new System.Drawing.Size(96, 17);
            this.LocalVersionLabelLabel.TabIndex = 1;
            this.LocalVersionLabelLabel.Text = "Local version:";
            // 
            // CheckForUpdatesButton
            // 
            this.CheckForUpdatesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckForUpdatesButton.Location = new System.Drawing.Point(8, 94);
            this.CheckForUpdatesButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.CheckForUpdatesButton.Name = "CheckForUpdatesButton";
            this.CheckForUpdatesButton.Size = new System.Drawing.Size(139, 28);
            this.CheckForUpdatesButton.TabIndex = 0;
            this.CheckForUpdatesButton.Text = "Check for updates";
            this.CheckForUpdatesButton.UseVisualStyleBackColor = true;
            this.CheckForUpdatesButton.Click += new System.EventHandler(this.CheckForUpdatesButton_Click);
            // 
            // HideEpochsCheckbox
            // 
            this.HideEpochsCheckbox.AutoSize = true;
            this.HideEpochsCheckbox.Location = new System.Drawing.Point(8, 52);
            this.HideEpochsCheckbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.HideEpochsCheckbox.Name = "HideEpochsCheckbox";
            this.HideEpochsCheckbox.Size = new System.Drawing.Size(349, 21);
            this.HideEpochsCheckbox.TabIndex = 8;
            this.HideEpochsCheckbox.Text = "Hide epoch numbers in mod list (Requires Restart)";
            this.HideEpochsCheckbox.UseVisualStyleBackColor = true;
            this.HideEpochsCheckbox.CheckedChanged += new System.EventHandler(this.HideEpochsCheckbox_CheckedChanged);
            // 
            // MoreSettingsGroupBox
            // 
            this.MoreSettingsGroupBox.Controls.Add(this.HideVCheckbox);
            this.MoreSettingsGroupBox.Controls.Add(this.HideEpochsCheckbox);
            this.MoreSettingsGroupBox.Controls.Add(this.AutoSortUpdateCheckBox);
            this.MoreSettingsGroupBox.Location = new System.Drawing.Point(16, 549);
            this.MoreSettingsGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MoreSettingsGroupBox.Name = "MoreSettingsGroupBox";
            this.MoreSettingsGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MoreSettingsGroupBox.Size = new System.Drawing.Size(635, 113);
            this.MoreSettingsGroupBox.TabIndex = 14;
            this.MoreSettingsGroupBox.TabStop = false;
            this.MoreSettingsGroupBox.Text = "More Settings";
            // 
            // AutoSortUpdateCheckBox
            // 
            this.AutoSortUpdateCheckBox.AutoSize = true;
            this.AutoSortUpdateCheckBox.Location = new System.Drawing.Point(8, 23);
            this.AutoSortUpdateCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AutoSortUpdateCheckBox.Name = "AutoSortUpdateCheckBox";
            this.AutoSortUpdateCheckBox.Size = new System.Drawing.Size(511, 21);
            this.AutoSortUpdateCheckBox.TabIndex = 0;
            this.AutoSortUpdateCheckBox.Text = "Automatically sort by \"Update\"-column when clicking \"Add available updates\"";
            this.AutoSortUpdateCheckBox.UseVisualStyleBackColor = true;
            this.AutoSortUpdateCheckBox.CheckedChanged += new System.EventHandler(this.AutoSortUpdateCheckBox_CheckedChanged);
            // 
            // HideVCheckbox
            // 
            this.HideVCheckbox.AutoSize = true;
            this.HideVCheckbox.Location = new System.Drawing.Point(8, 80);
            this.HideVCheckbox.Margin = new System.Windows.Forms.Padding(4);
            this.HideVCheckbox.Name = "HideVCheckbox";
            this.HideVCheckbox.Size = new System.Drawing.Size(268, 21);
            this.HideVCheckbox.TabIndex = 9;
            this.HideVCheckbox.Text = "Hide \"v\" in mod list (Requires Restart)";
            this.HideVCheckbox.UseVisualStyleBackColor = true;
            this.HideVCheckbox.CheckedChanged += new System.EventHandler(this.HideVCheckbox_CheckedChanged);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 675);
            this.Controls.Add(this.MoreSettingsGroupBox);
            this.Controls.Add(this.AutoUpdateGroupBox);
            this.Controls.Add(this.CacheGroupBox);
            this.Controls.Add(this.RepositoryGroupBox);
            this.Controls.Add(this.AuthTokensGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SettingsDialog";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsDialog_Load);
            this.RepositoryGroupBox.ResumeLayout(false);
            this.AuthTokensGroupBox.ResumeLayout(false);
            this.CacheGroupBox.ResumeLayout(false);
            this.CacheGroupBox.PerformLayout();
            this.AutoUpdateGroupBox.ResumeLayout(false);
            this.AutoUpdateGroupBox.PerformLayout();
            this.MoreSettingsGroupBox.ResumeLayout(false);
            this.MoreSettingsGroupBox.PerformLayout();
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
        private System.Windows.Forms.GroupBox MoreSettingsGroupBox;
        private System.Windows.Forms.CheckBox AutoSortUpdateCheckBox;
        private System.Windows.Forms.CheckBox HideVCheckbox;
    }
}
