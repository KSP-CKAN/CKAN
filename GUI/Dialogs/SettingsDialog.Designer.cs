namespace CKAN.GUI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(SettingsDialog));
            this.RepositoryGroupBox = new System.Windows.Forms.GroupBox();
            this.ReposListBox = new ThemedListView();
            this.RepoNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RepoURLHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NewRepoButton = new System.Windows.Forms.Button();
            this.UpRepoButton = new System.Windows.Forms.Button();
            this.DownRepoButton = new System.Windows.Forms.Button();
            this.DeleteRepoButton = new System.Windows.Forms.Button();
            this.AuthTokensGroupBox = new System.Windows.Forms.GroupBox();
            this.AuthTokensListBox = new ThemedListView();
            this.AuthHostHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AuthTokenHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NewAuthTokenButton = new System.Windows.Forms.Button();
            this.DeleteAuthTokenButton = new System.Windows.Forms.Button();
            this.CacheGroupBox = new System.Windows.Forms.GroupBox();
            this.CachePath = new System.Windows.Forms.TextBox();
            this.CacheSummary = new System.Windows.Forms.Label();
            this.CacheLimitPreLabel = new System.Windows.Forms.Label();
            this.CacheLimit = new System.Windows.Forms.TextBox();
            this.CacheLimitPostLabel = new System.Windows.Forms.Label();
            this.ChangeCacheButton = new System.Windows.Forms.Button();
            this.ClearCacheButton = new CKAN.GUI.DropdownMenuButton();
            this.ClearCacheMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.PurgeToLimitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PurgeAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ResetCacheButton = new System.Windows.Forms.Button();
            this.OpenCacheButton = new System.Windows.Forms.Button();
            this.AutoUpdateGroupBox = new System.Windows.Forms.GroupBox();
            this.LocalVersionPreLabel = new System.Windows.Forms.Label();
            this.LocalVersionLabel = new System.Windows.Forms.Label();
            this.LatestVersionPreLabel = new System.Windows.Forms.Label();
            this.LatestVersionLabel = new System.Windows.Forms.Label();
            this.CheckUpdateOnLaunchCheckbox = new System.Windows.Forms.CheckBox();
            this.DevBuildsCheckbox = new System.Windows.Forms.CheckBox();
            this.CheckForUpdatesButton = new System.Windows.Forms.Button();
            this.InstallUpdateButton = new System.Windows.Forms.Button();
            this.BehaviourGroupBox = new System.Windows.Forms.GroupBox();
            this.EnableTrayIconCheckBox = new System.Windows.Forms.CheckBox();
            this.MinimizeToTrayCheckBox = new System.Windows.Forms.CheckBox();
            this.RefreshPreLabel = new System.Windows.Forms.Label();
            this.RefreshTextBox = new System.Windows.Forms.TextBox();
            this.RefreshPostLabel = new System.Windows.Forms.Label();
            this.PauseRefreshCheckBox = new System.Windows.Forms.CheckBox();
            this.MoreSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.LanguageSelectionLabel = new System.Windows.Forms.Label();
            this.LanguageSelectionComboBox = new System.Windows.Forms.ComboBox();
            this.RefreshOnStartupCheckbox = new System.Windows.Forms.CheckBox();
            this.HideEpochsCheckbox = new System.Windows.Forms.CheckBox();
            this.HideVCheckbox = new System.Windows.Forms.CheckBox();
            this.AutoSortUpdateCheckBox = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.RepositoryGroupBox.SuspendLayout();
            this.AuthTokensGroupBox.SuspendLayout();
            this.CacheGroupBox.SuspendLayout();
            this.ClearCacheMenu.SuspendLayout();
            this.AutoUpdateGroupBox.SuspendLayout();
            this.BehaviourGroupBox.SuspendLayout();
            this.MoreSettingsGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // RepositoryGroupBox
            //
            this.RepositoryGroupBox.Controls.Add(this.ReposListBox);
            this.RepositoryGroupBox.Controls.Add(this.NewRepoButton);
            this.RepositoryGroupBox.Controls.Add(this.UpRepoButton);
            this.RepositoryGroupBox.Controls.Add(this.DownRepoButton);
            this.RepositoryGroupBox.Controls.Add(this.DeleteRepoButton);
            this.RepositoryGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.RepositoryGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RepositoryGroupBox.Location = new System.Drawing.Point(12, 6);
            this.RepositoryGroupBox.Name = "RepositoryGroupBox";
            this.RepositoryGroupBox.Size = new System.Drawing.Size(488, 128);
            this.RepositoryGroupBox.TabIndex = 0;
            this.RepositoryGroupBox.TabStop = false;
            resources.ApplyResources(this.RepositoryGroupBox, "RepositoryGroupBox");
            //
            // ReposListBox
            //
            this.ReposListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ReposListBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.RepoNameHeader,
            this.RepoURLHeader});
            this.ReposListBox.Location = new System.Drawing.Point(12, 18);
            this.ReposListBox.FullRowSelect = true;
            this.ReposListBox.MultiSelect = false;
            this.ReposListBox.Name = "ReposListBox";
            this.ReposListBox.Size = new System.Drawing.Size(464, 67);
            this.ReposListBox.TabIndex = 0;
            this.ReposListBox.View = System.Windows.Forms.View.Details;
            this.ReposListBox.SelectedIndexChanged += new System.EventHandler(this.ReposListBox_SelectedIndexChanged);
            //
            // RepoNameHeader
            //
            this.RepoNameHeader.Width = 120;
            resources.ApplyResources(this.RepoNameHeader, "RepoNameHeader");
            //
            // RepoURLHeader
            //
            this.RepoURLHeader.Width = 380;
            resources.ApplyResources(this.RepoURLHeader, "RepoURLHeader");
            //
            // NewRepoButton
            //
            this.NewRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NewRepoButton.Location = new System.Drawing.Point(12, 93);
            this.NewRepoButton.Name = "NewRepoButton";
            this.NewRepoButton.Size = new System.Drawing.Size(73, 25);
            this.NewRepoButton.TabIndex = 1;
            this.NewRepoButton.Click += new System.EventHandler(this.NewRepoButton_Click);
            resources.ApplyResources(this.NewRepoButton, "NewRepoButton");
            //
            // UpRepoButton
            //
            this.UpRepoButton.Enabled = false;
            this.UpRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UpRepoButton.Location = new System.Drawing.Point(91, 93);
            this.UpRepoButton.Name = "UpRepoButton";
            this.UpRepoButton.Size = new System.Drawing.Size(73, 25);
            this.UpRepoButton.TabIndex = 2;
            this.UpRepoButton.Click += new System.EventHandler(this.UpRepoButton_Click);
            resources.ApplyResources(this.UpRepoButton, "UpRepoButton");
            //
            // DownRepoButton
            //
            this.DownRepoButton.Enabled = false;
            this.DownRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DownRepoButton.Location = new System.Drawing.Point(170, 93);
            this.DownRepoButton.Name = "DownRepoButton";
            this.DownRepoButton.Size = new System.Drawing.Size(73, 25);
            this.DownRepoButton.TabIndex = 3;
            this.DownRepoButton.Click += new System.EventHandler(this.DownRepoButton_Click);
            resources.ApplyResources(this.DownRepoButton, "DownRepoButton");
            //
            // DeleteRepoButton
            //
            this.DeleteRepoButton.Enabled = false;
            this.DeleteRepoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteRepoButton.Location = new System.Drawing.Point(406, 93);
            this.DeleteRepoButton.Name = "DeleteRepoButton";
            this.DeleteRepoButton.Size = new System.Drawing.Size(70, 25);
            this.DeleteRepoButton.TabIndex = 4;
            this.DeleteRepoButton.Click += new System.EventHandler(this.DeleteRepoButton_Click);
            resources.ApplyResources(this.DeleteRepoButton, "DeleteRepoButton");
            //
            // AuthTokensGroupBox
            //
            this.AuthTokensGroupBox.Controls.Add(this.AuthTokensListBox);
            this.AuthTokensGroupBox.Controls.Add(this.NewAuthTokenButton);
            this.AuthTokensGroupBox.Controls.Add(this.DeleteAuthTokenButton);
            this.AuthTokensGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.AuthTokensGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AuthTokensGroupBox.Location = new System.Drawing.Point(512, 6);
            this.AuthTokensGroupBox.Name = "AuthTokensGroupBox";
            this.AuthTokensGroupBox.Size = new System.Drawing.Size(244, 128);
            this.AuthTokensGroupBox.TabIndex = 1;
            this.AuthTokensGroupBox.TabStop = false;
            resources.ApplyResources(this.AuthTokensGroupBox, "AuthTokensGroupBox");
            //
            // AuthTokensListBox
            //
            this.AuthTokensListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AuthTokensListBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.AuthHostHeader,
            this.AuthTokenHeader});
            this.AuthTokensListBox.FullRowSelect = true;
            this.AuthTokensListBox.MultiSelect = false;
            this.AuthTokensListBox.View = System.Windows.Forms.View.Details;
            this.AuthTokensListBox.Location = new System.Drawing.Point(12, 18);
            this.AuthTokensListBox.Name = "AuthTokensListBox";
            this.AuthTokensListBox.Size = new System.Drawing.Size(220, 67);
            this.AuthTokensListBox.TabIndex = 0;
            this.AuthTokensListBox.SelectedIndexChanged += new System.EventHandler(this.AuthTokensListBox_SelectedIndexChanged);
            //
            // AuthHostHeader
            //
            this.AuthHostHeader.Width = 100;
            resources.ApplyResources(this.AuthHostHeader, "AuthHostHeader");
            //
            // AuthTokenHeader
            //
            this.AuthTokenHeader.Width = 380;
            resources.ApplyResources(this.AuthTokenHeader, "AuthTokenHeader");
            //
            // NewAuthTokenButton
            //
            this.NewAuthTokenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NewAuthTokenButton.Location = new System.Drawing.Point(12, 93);
            this.NewAuthTokenButton.Name = "NewAuthTokenButton";
            this.NewAuthTokenButton.Size = new System.Drawing.Size(73, 25);
            this.NewAuthTokenButton.TabIndex = 1;
            this.NewAuthTokenButton.Click += new System.EventHandler(this.NewAuthTokenButton_Click);
            resources.ApplyResources(this.NewAuthTokenButton, "NewAuthTokenButton");
            //
            // DeleteAuthTokenButton
            //
            this.DeleteAuthTokenButton.Enabled = false;
            this.DeleteAuthTokenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteAuthTokenButton.Location = new System.Drawing.Point(164, 93);
            this.DeleteAuthTokenButton.Name = "DeleteAuthTokenButton";
            this.DeleteAuthTokenButton.Size = new System.Drawing.Size(70, 25);
            this.DeleteAuthTokenButton.TabIndex = 2;
            this.DeleteAuthTokenButton.Click += new System.EventHandler(this.DeleteAuthTokenButton_Click);
            resources.ApplyResources(this.DeleteAuthTokenButton, "DeleteAuthTokenButton");
            //
            // AutoUpdateGroupBox
            //
            this.AutoUpdateGroupBox.Controls.Add(this.LocalVersionPreLabel);
            this.AutoUpdateGroupBox.Controls.Add(this.LocalVersionLabel);
            this.AutoUpdateGroupBox.Controls.Add(this.LatestVersionPreLabel);
            this.AutoUpdateGroupBox.Controls.Add(this.LatestVersionLabel);
            this.AutoUpdateGroupBox.Controls.Add(this.CheckUpdateOnLaunchCheckbox);
            this.AutoUpdateGroupBox.Controls.Add(this.DevBuildsCheckbox);
            this.AutoUpdateGroupBox.Controls.Add(this.CheckForUpdatesButton);
            this.AutoUpdateGroupBox.Controls.Add(this.InstallUpdateButton);
            this.AutoUpdateGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.AutoUpdateGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AutoUpdateGroupBox.Location = new System.Drawing.Point(12, 144);
            this.AutoUpdateGroupBox.Name = "AutoUpdateGroupBox";
            this.AutoUpdateGroupBox.Size = new System.Drawing.Size(254, 156);
            this.AutoUpdateGroupBox.TabIndex = 2;
            this.AutoUpdateGroupBox.TabStop = false;
            resources.ApplyResources(this.AutoUpdateGroupBox, "AutoUpdateGroupBox");
            //
            // LocalVersionPreLabel
            //
            this.LocalVersionPreLabel.AutoSize = true;
            this.LocalVersionPreLabel.Location = new System.Drawing.Point(9, 18);
            this.LocalVersionPreLabel.Name = "LocalVersionPreLabel";
            this.LocalVersionPreLabel.Size = new System.Drawing.Size(73, 13);
            this.LocalVersionPreLabel.TabIndex = 0;
            resources.ApplyResources(this.LocalVersionPreLabel, "LocalVersionPreLabel");
            //
            // LocalVersionLabel
            //
            this.LocalVersionLabel.AutoSize = true;
            this.LocalVersionLabel.Location = new System.Drawing.Point(95, 18);
            this.LocalVersionLabel.Name = "LocalVersionLabel";
            this.LocalVersionLabel.Size = new System.Drawing.Size(37, 13);
            this.LocalVersionLabel.TabIndex = 1;
            resources.ApplyResources(this.LocalVersionLabel, "LocalVersionLabel");
            //
            // LatestVersionPreLabel
            //
            this.LatestVersionPreLabel.AutoSize = true;
            this.LatestVersionPreLabel.Location = new System.Drawing.Point(9, 39);
            this.LatestVersionPreLabel.Name = "LatestVersionPreLabel";
            this.LatestVersionPreLabel.Size = new System.Drawing.Size(76, 13);
            this.LatestVersionPreLabel.TabIndex = 2;
            resources.ApplyResources(this.LatestVersionPreLabel, "LatestVersionPreLabel");
            //
            // LatestVersionLabel
            //
            this.LatestVersionLabel.AutoSize = true;
            this.LatestVersionLabel.Location = new System.Drawing.Point(95, 39);
            this.LatestVersionLabel.Name = "LatestVersionLabel";
            this.LatestVersionLabel.Size = new System.Drawing.Size(25, 13);
            this.LatestVersionLabel.TabIndex = 3;
            resources.ApplyResources(this.LatestVersionLabel, "LatestVersionLabel");
            //
            // CheckUpdateOnLaunchCheckbox
            //
            this.CheckUpdateOnLaunchCheckbox.AutoSize = true;
            this.CheckUpdateOnLaunchCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckUpdateOnLaunchCheckbox.Location = new System.Drawing.Point(12, 61);
            this.CheckUpdateOnLaunchCheckbox.Name = "CheckUpdateOnLaunchCheckbox";
            this.CheckUpdateOnLaunchCheckbox.Size = new System.Drawing.Size(195, 17);
            this.CheckUpdateOnLaunchCheckbox.TabIndex = 4;
            this.CheckUpdateOnLaunchCheckbox.CheckedChanged += new System.EventHandler(this.CheckUpdateOnLaunchCheckbox_CheckedChanged);
            resources.ApplyResources(this.CheckUpdateOnLaunchCheckbox, "CheckUpdateOnLaunchCheckbox");
            //
            // DevBuildsCheckbox
            //
            this.DevBuildsCheckbox.AutoSize = true;
            this.DevBuildsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DevBuildsCheckbox.Location = new System.Drawing.Point(12, 85);
            this.DevBuildsCheckbox.Name = "DevBuildsCheckbox";
            this.DevBuildsCheckbox.Size = new System.Drawing.Size(195, 17);
            this.DevBuildsCheckbox.TabIndex = 4;
            this.DevBuildsCheckbox.CheckedChanged += new System.EventHandler(this.DevBuildsCheckbox_CheckedChanged);
            resources.ApplyResources(this.DevBuildsCheckbox, "DevBuildsCheckbox");
            //
            // CheckForUpdatesButton
            //
            this.CheckForUpdatesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CheckForUpdatesButton.Location = new System.Drawing.Point(12, 108);
            this.CheckForUpdatesButton.Name = "CheckForUpdatesButton";
            this.CheckForUpdatesButton.Size = new System.Drawing.Size(112, 38);
            this.CheckForUpdatesButton.TabIndex = 5;
            this.CheckForUpdatesButton.Click += new System.EventHandler(this.CheckForUpdatesButton_Click);
            resources.ApplyResources(this.CheckForUpdatesButton, "CheckForUpdatesButton");
            //
            // InstallUpdateButton
            //
            this.InstallUpdateButton.Enabled = false;
            this.InstallUpdateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.InstallUpdateButton.Location = new System.Drawing.Point(130, 108);
            this.InstallUpdateButton.Name = "InstallUpdateButton";
            this.InstallUpdateButton.Size = new System.Drawing.Size(112, 38);
            this.InstallUpdateButton.TabIndex = 6;
            this.InstallUpdateButton.Click += new System.EventHandler(this.InstallUpdateButton_Click);
            resources.ApplyResources(this.InstallUpdateButton, "InstallUpdateButton");
            //
            // CacheGroupBox
            //
            this.CacheGroupBox.Controls.Add(this.CachePath);
            this.CacheGroupBox.Controls.Add(this.CacheSummary);
            this.CacheGroupBox.Controls.Add(this.CacheLimitPreLabel);
            this.CacheGroupBox.Controls.Add(this.CacheLimit);
            this.CacheGroupBox.Controls.Add(this.CacheLimitPostLabel);
            this.CacheGroupBox.Controls.Add(this.ChangeCacheButton);
            this.CacheGroupBox.Controls.Add(this.ClearCacheButton);
            this.CacheGroupBox.Controls.Add(this.ResetCacheButton);
            this.CacheGroupBox.Controls.Add(this.OpenCacheButton);
            this.CacheGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.CacheGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CacheGroupBox.Location = new System.Drawing.Point(280, 144);
            this.CacheGroupBox.Name = "CacheGroupBox";
            this.CacheGroupBox.Size = new System.Drawing.Size(476, 156);
            this.CacheGroupBox.TabIndex = 3;
            this.CacheGroupBox.TabStop = false;
            resources.ApplyResources(this.CacheGroupBox, "CacheGroupBox");
            //
            // CachePath
            //
            this.CachePath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CachePath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.CachePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CachePath.Location = new System.Drawing.Point(12, 18);
            this.CachePath.Margin = new System.Windows.Forms.Padding(2);
            this.CachePath.Name = "CachePath";
            this.CachePath.Size = new System.Drawing.Size(452, 20);
            this.CachePath.TabIndex = 0;
            this.CachePath.TextChanged += new System.EventHandler(this.CachePath_TextChanged);
            //
            // CacheSummary
            //
            this.CacheSummary.AutoSize = true;
            this.CacheSummary.Location = new System.Drawing.Point(9, 44);
            this.CacheSummary.Name = "CacheSummary";
            this.CacheSummary.Size = new System.Drawing.Size(70, 13);
            this.CacheSummary.TabIndex = 1;
            resources.ApplyResources(this.CacheSummary, "CacheSummary");
            //
            // CacheLimitPreLabel
            //
            this.CacheLimitPreLabel.AutoSize = true;
            this.CacheLimitPreLabel.Location = new System.Drawing.Point(9, 65);
            this.CacheLimitPreLabel.Name = "CacheLimitPreLabel";
            this.CacheLimitPreLabel.Size = new System.Drawing.Size(108, 13);
            this.CacheLimitPreLabel.TabIndex = 2;
            resources.ApplyResources(this.CacheLimitPreLabel, "CacheLimitPreLabel");
            //
            // CacheLimit
            //
            this.CacheLimit.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CacheLimit.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.CacheLimit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CacheLimit.Location = new System.Drawing.Point(117, 63);
            this.CacheLimit.Margin = new System.Windows.Forms.Padding(2);
            this.CacheLimit.Name = "CacheLimit";
            this.CacheLimit.Size = new System.Drawing.Size(50, 20);
            this.CacheLimit.TabIndex = 3;
            this.CacheLimit.TextChanged += new System.EventHandler(this.CacheLimit_TextChanged);
            this.CacheLimit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CacheLimit_KeyPress);
            //
            // CacheLimitPostLabel
            //
            this.CacheLimitPostLabel.AutoSize = true;
            this.CacheLimitPostLabel.Location = new System.Drawing.Point(167, 65);
            this.CacheLimitPostLabel.Name = "CacheLimitPostLabel";
            this.CacheLimitPostLabel.Size = new System.Drawing.Size(119, 13);
            this.CacheLimitPostLabel.TabIndex = 4;
            resources.ApplyResources(this.CacheLimitPostLabel, "CacheLimitPostLabel");
            //
            // ChangeCacheButton
            //
            this.ChangeCacheButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeCacheButton.Location = new System.Drawing.Point(12, 89);
            this.ChangeCacheButton.Name = "ChangeCacheButton";
            this.ChangeCacheButton.Size = new System.Drawing.Size(75, 25);
            this.ChangeCacheButton.TabIndex = 5;
            this.ChangeCacheButton.Click += new System.EventHandler(this.ChangeCacheButton_Click);
            resources.ApplyResources(this.ChangeCacheButton, "ChangeCacheButton");
            //
            // ClearCacheButton
            //
            this.ClearCacheButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ClearCacheButton.Location = new System.Drawing.Point(93, 89);
            this.ClearCacheButton.Menu = this.ClearCacheMenu;
            this.ClearCacheButton.Name = "ClearCacheButton";
            this.ClearCacheButton.Size = new System.Drawing.Size(75, 25);
            this.ClearCacheButton.TabIndex = 6;
            resources.ApplyResources(this.ClearCacheButton, "ClearCacheButton");
            //
            // ClearCacheMenu
            //
            this.ClearCacheMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PurgeToLimitMenuItem,
            this.PurgeAllMenuItem});
            this.ClearCacheMenu.Name = "ClearCacheMenu";
            this.ClearCacheMenu.Size = new System.Drawing.Size(147, 48);
            //
            // PurgeToLimitMenuItem
            //
            this.PurgeToLimitMenuItem.Name = "PurgeToLimitMenuItem";
            this.PurgeToLimitMenuItem.Size = new System.Drawing.Size(146, 22);
            this.PurgeToLimitMenuItem.Click += new System.EventHandler(this.PurgeToLimitMenuItem_Click);
            resources.ApplyResources(this.PurgeToLimitMenuItem, "PurgeToLimitMenuItem");
            //
            // PurgeAllMenuItem
            //
            this.PurgeAllMenuItem.Name = "PurgeAllMenuItem";
            this.PurgeAllMenuItem.Size = new System.Drawing.Size(146, 22);
            this.PurgeAllMenuItem.Click += new System.EventHandler(this.PurgeAllMenuItem_Click);
            resources.ApplyResources(this.PurgeAllMenuItem, "PurgeAllMenuItem");
            //
            // ResetCacheButton
            //
            this.ResetCacheButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ResetCacheButton.Location = new System.Drawing.Point(174, 89);
            this.ResetCacheButton.Name = "ResetCacheButton";
            this.ResetCacheButton.Size = new System.Drawing.Size(75, 25);
            this.ResetCacheButton.TabIndex = 7;
            this.ResetCacheButton.Click += new System.EventHandler(this.ResetCacheButton_Click);
            resources.ApplyResources(this.ResetCacheButton, "ResetCacheButton");
            //
            // OpenCacheButton
            //
            this.OpenCacheButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OpenCacheButton.Location = new System.Drawing.Point(255, 89);
            this.OpenCacheButton.Name = "OpenCacheButton";
            this.OpenCacheButton.Size = new System.Drawing.Size(75, 25);
            this.OpenCacheButton.TabIndex = 8;
            this.OpenCacheButton.Click += new System.EventHandler(this.OpenCacheButton_Click);
            resources.ApplyResources(this.OpenCacheButton, "OpenCacheButton");
            //
            // BehaviourGroupBox
            //
            this.BehaviourGroupBox.Controls.Add(this.EnableTrayIconCheckBox);
            this.BehaviourGroupBox.Controls.Add(this.MinimizeToTrayCheckBox);
            this.BehaviourGroupBox.Controls.Add(this.RefreshPreLabel);
            this.BehaviourGroupBox.Controls.Add(this.RefreshTextBox);
            this.BehaviourGroupBox.Controls.Add(this.RefreshPostLabel);
            this.BehaviourGroupBox.Controls.Add(this.PauseRefreshCheckBox);
            this.BehaviourGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.BehaviourGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BehaviourGroupBox.Location = new System.Drawing.Point(12, 310);
            this.BehaviourGroupBox.Name = "BehaviourGroupBox";
            this.BehaviourGroupBox.Size = new System.Drawing.Size(254, 150);
            this.BehaviourGroupBox.TabIndex = 4;
            this.BehaviourGroupBox.TabStop = false;
            resources.ApplyResources(this.BehaviourGroupBox, "BehaviourGroupBox");
            //
            // EnableTrayIconCheckBox
            //
            this.EnableTrayIconCheckBox.AutoSize = true;
            this.EnableTrayIconCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.EnableTrayIconCheckBox.Location = new System.Drawing.Point(12, 18);
            this.EnableTrayIconCheckBox.Name = "EnableTrayIconCheckBox";
            this.EnableTrayIconCheckBox.Size = new System.Drawing.Size(102, 17);
            this.EnableTrayIconCheckBox.TabIndex = 0;
            this.EnableTrayIconCheckBox.CheckedChanged += new System.EventHandler(this.EnableTrayIconCheckBox_CheckedChanged);
            resources.ApplyResources(this.EnableTrayIconCheckBox, "EnableTrayIconCheckBox");
            //
            // MinimizeToTrayCheckBox
            //
            this.MinimizeToTrayCheckBox.AutoSize = true;
            this.MinimizeToTrayCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MinimizeToTrayCheckBox.Location = new System.Drawing.Point(12, 41);
            this.MinimizeToTrayCheckBox.Name = "MinimizeToTrayCheckBox";
            this.MinimizeToTrayCheckBox.Size = new System.Drawing.Size(98, 17);
            this.MinimizeToTrayCheckBox.TabIndex = 1;
            this.MinimizeToTrayCheckBox.CheckedChanged += new System.EventHandler(this.MinimizeToTrayCheckBox_CheckedChanged);
            resources.ApplyResources(this.MinimizeToTrayCheckBox, "MinimizeToTrayCheckBox");
            //
            // RefreshPreLabel
            //
            this.RefreshPreLabel.AutoSize = false;
            this.RefreshPreLabel.Location = new System.Drawing.Point(9, 66);
            this.RefreshPreLabel.Name = "RefreshPreLabel";
            this.RefreshPreLabel.Size = new System.Drawing.Size(114, 26);
            this.RefreshPreLabel.TabIndex = 2;
            resources.ApplyResources(this.RefreshPreLabel, "RefreshPreLabel");
            //
            // RefreshTextBox
            //
            this.RefreshTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.RefreshTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.RefreshTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RefreshTextBox.Location = new System.Drawing.Point(125, 64);
            this.RefreshTextBox.Name = "RefreshTextBox";
            this.RefreshTextBox.Size = new System.Drawing.Size(25, 20);
            this.RefreshTextBox.TabIndex = 3;
            this.toolTip1.SetToolTip(this.RefreshTextBox, "Setting to 0 will not refresh modlist");
            this.RefreshTextBox.TextChanged += new System.EventHandler(this.RefreshTextBox_TextChanged);
            this.RefreshTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RefreshTextBox_KeyPress);
            //
            // RefreshPostLabel
            //
            this.RefreshPostLabel.AutoSize = true;
            this.RefreshPostLabel.Location = new System.Drawing.Point(153, 66);
            this.RefreshPostLabel.Name = "RefreshPostLabel";
            this.RefreshPostLabel.Size = new System.Drawing.Size(49, 13);
            this.RefreshPostLabel.TabIndex = 4;
            resources.ApplyResources(this.RefreshPostLabel, "RefreshPostLabel");
            //
            // PauseRefreshCheckBox
            //
            this.PauseRefreshCheckBox.AutoSize = true;
            this.PauseRefreshCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PauseRefreshCheckBox.Location = new System.Drawing.Point(12, 103);
            this.PauseRefreshCheckBox.Name = "PauseRefreshCheckBox";
            this.PauseRefreshCheckBox.Size = new System.Drawing.Size(105, 17);
            this.PauseRefreshCheckBox.TabIndex = 5;
            this.PauseRefreshCheckBox.CheckedChanged += new System.EventHandler(this.PauseRefreshCheckBox_CheckedChanged);
            resources.ApplyResources(this.PauseRefreshCheckBox, "PauseRefreshCheckBox");
            //
            // MoreSettingsGroupBox
            //
            this.MoreSettingsGroupBox.Controls.Add(this.LanguageSelectionLabel);
            this.MoreSettingsGroupBox.Controls.Add(this.LanguageSelectionComboBox);
            this.MoreSettingsGroupBox.Controls.Add(this.AutoSortUpdateCheckBox);
            this.MoreSettingsGroupBox.Controls.Add(this.RefreshOnStartupCheckbox);
            this.MoreSettingsGroupBox.Controls.Add(this.HideEpochsCheckbox);
            this.MoreSettingsGroupBox.Controls.Add(this.HideVCheckbox);
            this.MoreSettingsGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MoreSettingsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MoreSettingsGroupBox.Location = new System.Drawing.Point(280, 310);
            this.MoreSettingsGroupBox.Name = "MoreSettingsGroupBox";
            this.MoreSettingsGroupBox.Size = new System.Drawing.Size(476, 150);
            this.MoreSettingsGroupBox.TabIndex = 5;
            this.MoreSettingsGroupBox.TabStop = false;
            resources.ApplyResources(this.MoreSettingsGroupBox, "MoreSettingsGroupBox");
            //
            // LanguageSelectionLabel
            //
            this.LanguageSelectionLabel.AutoSize = true;
            this.LanguageSelectionLabel.Location = new System.Drawing.Point(12, 18);
            this.LanguageSelectionLabel.Name = "LanguageSelectionLabel";
            this.LanguageSelectionLabel.Size = new System.Drawing.Size(220, 17);
            this.LanguageSelectionLabel.TabStop = false;
            resources.ApplyResources(this.LanguageSelectionLabel, "LanguageSelectionLabel");
            //
            // LanguageSelectionComboBox
            //
            this.LanguageSelectionComboBox.AutoSize = true;
            this.LanguageSelectionComboBox.Location = new System.Drawing.Point(244, 18);
            this.LanguageSelectionComboBox.Name = "LanguageSelectionComboBox";
            this.LanguageSelectionComboBox.Size = new System.Drawing.Size(220, 17);
            this.LanguageSelectionComboBox.TabIndex = 0;
            this.LanguageSelectionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LanguageSelectionComboBox.SelectionChangeCommitted += new System.EventHandler(this.LanguageSelectionComboBox_SelectionChanged);
            this.LanguageSelectionComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.LanguageSelectionComboBox_MouseWheel);
            //
            // RefreshOnStartupCheckbox
            //
            this.RefreshOnStartupCheckbox.AutoSize = true;
            this.RefreshOnStartupCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RefreshOnStartupCheckbox.Location = new System.Drawing.Point(12, 41);
            this.RefreshOnStartupCheckbox.Name = "RefreshOnStartupCheckbox";
            this.RefreshOnStartupCheckbox.Size = new System.Drawing.Size(167, 17);
            this.RefreshOnStartupCheckbox.TabIndex = 1;
            this.RefreshOnStartupCheckbox.CheckedChanged += new System.EventHandler(this.RefreshOnStartupCheckbox_CheckedChanged);
            resources.ApplyResources(this.RefreshOnStartupCheckbox, "RefreshOnStartupCheckbox");
            //
            // HideEpochsCheckbox
            //
            this.HideEpochsCheckbox.AutoSize = true;
            this.HideEpochsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.HideEpochsCheckbox.Location = new System.Drawing.Point(12, 64);
            this.HideEpochsCheckbox.Name = "HideEpochsCheckbox";
            this.HideEpochsCheckbox.Size = new System.Drawing.Size(261, 17);
            this.HideEpochsCheckbox.TabIndex = 2;
            this.HideEpochsCheckbox.CheckedChanged += new System.EventHandler(this.HideEpochsCheckbox_CheckedChanged);
            resources.ApplyResources(this.HideEpochsCheckbox, "HideEpochsCheckbox");
            //
            // HideVCheckbox
            //
            this.HideVCheckbox.AutoSize = true;
            this.HideVCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.HideVCheckbox.Location = new System.Drawing.Point(12, 87);
            this.HideVCheckbox.Name = "HideVCheckbox";
            this.HideVCheckbox.Size = new System.Drawing.Size(204, 17);
            this.HideVCheckbox.TabIndex = 3;
            this.HideVCheckbox.CheckedChanged += new System.EventHandler(this.HideVCheckbox_CheckedChanged);
            resources.ApplyResources(this.HideVCheckbox, "HideVCheckbox");
            //
            // AutoSortUpdateCheckBox
            //
            this.AutoSortUpdateCheckBox.AutoSize = false;
            this.AutoSortUpdateCheckBox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AutoSortUpdateCheckBox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.AutoSortUpdateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AutoSortUpdateCheckBox.Location = new System.Drawing.Point(12, 110);
            this.AutoSortUpdateCheckBox.Name = "AutoSortUpdateCheckBox";
            this.AutoSortUpdateCheckBox.Size = new System.Drawing.Size(452, 32);
            this.AutoSortUpdateCheckBox.TabIndex = 4;
            this.AutoSortUpdateCheckBox.CheckedChanged += new System.EventHandler(this.AutoSortUpdateCheckBox_CheckedChanged);
            resources.ApplyResources(this.AutoSortUpdateCheckBox, "AutoSortUpdateCheckBox");
            //
            //
            // SettingsDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 470);
            this.Controls.Add(this.RepositoryGroupBox);
            this.Controls.Add(this.AuthTokensGroupBox);
            this.Controls.Add(this.CacheGroupBox);
            this.Controls.Add(this.AutoUpdateGroupBox);
            this.Controls.Add(this.BehaviourGroupBox);
            this.Controls.Add(this.MoreSettingsGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = EmbeddedImages.AppIcon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.SettingsDialog_Load);
            resources.ApplyResources(this, "$this");
            this.RepositoryGroupBox.ResumeLayout(false);
            this.AuthTokensGroupBox.ResumeLayout(false);
            this.CacheGroupBox.ResumeLayout(false);
            this.CacheGroupBox.PerformLayout();
            this.ClearCacheMenu.ResumeLayout(false);
            this.AutoUpdateGroupBox.ResumeLayout(false);
            this.AutoUpdateGroupBox.PerformLayout();
            this.BehaviourGroupBox.ResumeLayout(false);
            this.BehaviourGroupBox.PerformLayout();
            this.MoreSettingsGroupBox.ResumeLayout(false);
            this.MoreSettingsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox RepositoryGroupBox;
        private System.Windows.Forms.ListView ReposListBox;
        private System.Windows.Forms.ColumnHeader RepoNameHeader;
        private System.Windows.Forms.ColumnHeader RepoURLHeader;
        private System.Windows.Forms.Button NewRepoButton;
        private System.Windows.Forms.Button UpRepoButton;
        private System.Windows.Forms.Button DownRepoButton;
        private System.Windows.Forms.Button DeleteRepoButton;
        private System.Windows.Forms.GroupBox AuthTokensGroupBox;
        private System.Windows.Forms.ListView AuthTokensListBox;
        private System.Windows.Forms.ColumnHeader AuthHostHeader;
        private System.Windows.Forms.ColumnHeader AuthTokenHeader;
        private System.Windows.Forms.Button NewAuthTokenButton;
        private System.Windows.Forms.Button DeleteAuthTokenButton;
        private System.Windows.Forms.GroupBox CacheGroupBox;
        private System.Windows.Forms.TextBox CachePath;
        private System.Windows.Forms.Label CacheSummary;
        private System.Windows.Forms.Label CacheLimitPreLabel;
        private System.Windows.Forms.TextBox CacheLimit;
        private System.Windows.Forms.Label CacheLimitPostLabel;
        private System.Windows.Forms.Button ChangeCacheButton;
        private CKAN.GUI.DropdownMenuButton ClearCacheButton;
        private System.Windows.Forms.ContextMenuStrip ClearCacheMenu;
        private System.Windows.Forms.ToolStripMenuItem PurgeToLimitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PurgeAllMenuItem;
        private System.Windows.Forms.Button ResetCacheButton;
        private System.Windows.Forms.Button OpenCacheButton;
        private System.Windows.Forms.GroupBox AutoUpdateGroupBox;
        private System.Windows.Forms.Label LocalVersionPreLabel;
        private System.Windows.Forms.Label LocalVersionLabel;
        private System.Windows.Forms.Label LatestVersionPreLabel;
        private System.Windows.Forms.Label LatestVersionLabel;
        private System.Windows.Forms.CheckBox CheckUpdateOnLaunchCheckbox;
        private System.Windows.Forms.CheckBox DevBuildsCheckbox;
        private System.Windows.Forms.Button CheckForUpdatesButton;
        private System.Windows.Forms.Button InstallUpdateButton;
        private System.Windows.Forms.GroupBox BehaviourGroupBox;
        private System.Windows.Forms.CheckBox MinimizeToTrayCheckBox;
        private System.Windows.Forms.CheckBox EnableTrayIconCheckBox;
        private System.Windows.Forms.Label RefreshPreLabel;
        private System.Windows.Forms.TextBox RefreshTextBox;
        private System.Windows.Forms.Label RefreshPostLabel;
        private System.Windows.Forms.CheckBox PauseRefreshCheckBox;
        private System.Windows.Forms.GroupBox MoreSettingsGroupBox;
        private System.Windows.Forms.Label LanguageSelectionLabel;
        private System.Windows.Forms.ComboBox LanguageSelectionComboBox;
        private System.Windows.Forms.CheckBox RefreshOnStartupCheckbox;
        private System.Windows.Forms.CheckBox HideEpochsCheckbox;
        private System.Windows.Forms.CheckBox HideVCheckbox;
        private System.Windows.Forms.CheckBox AutoSortUpdateCheckBox;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
