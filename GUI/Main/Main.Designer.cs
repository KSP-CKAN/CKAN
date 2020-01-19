namespace CKAN
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(Main));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageKspInstancesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openKspDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.installFromckanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportModListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportModPackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.importDownloadsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.auditRecommendationsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cKANSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kSPCommandlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compatibleKSPVersionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportClientIssueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportMetadataIssueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.launchKSPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RefreshToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.UpdateAllToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ApplyToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterCompatibleButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterInstalledButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterInstalledUpdateButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterReplaceableButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterCachedButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterUncachedButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterNewButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterNotInstalledButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterIncompatibleButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterAllButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterLabelsToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterTagsToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.NavBackwardToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.NavForwardToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ModList = new CKAN.MainModListGUI();
            this.InstallAllCheckbox = new System.Windows.Forms.CheckBox();
            this.Installed = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.AutoInstalled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.UpdateCol = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ReplaceCol = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ModName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LatestVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.KSPCompatibility = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SizeCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstallDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DownloadCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ModListContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.LabelsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ModListHeaderContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.modListToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tagFilterToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.untaggedFilterToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.labelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.editLabelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reinstallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downloadContentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.purgeContentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ModInfo = new CKAN.ModInfo();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusInstanceLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.MainTabControl = new CKAN.MainTabControl();
            this.ManageModsTabPage = new System.Windows.Forms.TabPage();
            this.FilterByAuthorTextBox = new CKAN.HintTextBox();
            this.FilterByAuthorLabel = new System.Windows.Forms.Label();
            this.FilterByNameLabel = new System.Windows.Forms.Label();
            this.FilterByNameTextBox = new CKAN.HintTextBox();
            this.FilterByDescriptionLabel = new System.Windows.Forms.Label();
            this.FilterByDescriptionTextBox = new CKAN.HintTextBox();
            this.ChangesetTabPage = new System.Windows.Forms.TabPage();
            this.Changeset = new CKAN.Changeset();
            this.WaitTabPage = new System.Windows.Forms.TabPage();
            this.Wait = new CKAN.Wait();
            this.ChooseRecommendedModsTabPage = new System.Windows.Forms.TabPage();
            this.ChooseRecommendedMods = new CKAN.ChooseRecommendedMods();
            this.ChooseProvidedModsTabPage = new System.Windows.Forms.TabPage();
            this.ChooseProvidedMods = new CKAN.ChooseProvidedMods();
            this.minimizeNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.minimizedContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.updatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.openCKANToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openKSPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openKSPDirectoryToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.cKANSettingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteDirectoriesTabPage = new System.Windows.Forms.TabPage();
            this.DeleteDirectories = new CKAN.DeleteDirectories();
            this.EditModpackTabPage = new System.Windows.Forms.TabPage();
            this.EditModpack = new CKAN.EditModpack();
            this.menuStrip1.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ModList)).BeginInit();
            this.ModListContextMenuStrip.SuspendLayout();
            this.ModListHeaderContextMenuStrip.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.ManageModsTabPage.SuspendLayout();
            this.ChangesetTabPage.SuspendLayout();
            this.WaitTabPage.SuspendLayout();
            this.ChooseRecommendedModsTabPage.SuspendLayout();
            this.ChooseProvidedModsTabPage.SuspendLayout();
            this.minimizedContextMenuStrip.SuspendLayout();
            this.DeleteDirectoriesTabPage.SuspendLayout();
            this.EditModpackTabPage.SuspendLayout();
            this.SuspendLayout();
            //
            // menuStrip1
            //
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1544, 35);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            //
            // fileToolStripMenuItem
            //
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageKspInstancesMenuItem,
            this.openKspDirectoryToolStripMenuItem,
            this.toolStripSeparator1,
            this.installFromckanToolStripMenuItem,
            this.importDownloadsToolStripMenuItem,
            this.toolStripSeparator2,
            this.exportModListToolStripMenuItem,
            this.exportModPackToolStripMenuItem,
            this.toolStripSeparator3,
            this.auditRecommendationsMenuItem,
            this.toolStripSeparator7,
            this.ExitToolButton});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(50, 29);
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            //
            // manageKspInstancesMenuItem
            //
            this.manageKspInstancesMenuItem.Name = "manageKspInstancesMenuItem";
            this.manageKspInstancesMenuItem.Size = new System.Drawing.Size(281, 30);
            this.manageKspInstancesMenuItem.Click += new System.EventHandler(this.manageKspInstancesMenuItem_Click);
            resources.ApplyResources(this.manageKspInstancesMenuItem, "manageKspInstancesMenuItem");
            //
            // openKspDirectoryToolStripMenuItem
            //
            this.openKspDirectoryToolStripMenuItem.Name = "openKspDirectoryToolStripMenuItem";
            this.openKspDirectoryToolStripMenuItem.Size = new System.Drawing.Size(281, 30);
            this.openKspDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openKspDirectoryToolStripMenuItem_Click);
            resources.ApplyResources(this.openKspDirectoryToolStripMenuItem, "openKspDirectoryToolStripMenuItem");
            //
            // toolStripSeparator1
            //
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(278, 6);
            //
            // installFromckanToolStripMenuItem
            //
            this.installFromckanToolStripMenuItem.Name = "installFromckanToolStripMenuItem";
            this.installFromckanToolStripMenuItem.Size = new System.Drawing.Size(281, 30);
            this.installFromckanToolStripMenuItem.Click += new System.EventHandler(this.installFromckanToolStripMenuItem_Click);
            resources.ApplyResources(this.installFromckanToolStripMenuItem, "installFromckanToolStripMenuItem");
            //
            // exportModListToolStripMenuItem
            //
            this.exportModListToolStripMenuItem.Name = "exportModListToolStripMenuItem";
            this.exportModListToolStripMenuItem.Size = new System.Drawing.Size(281, 30);
            this.exportModListToolStripMenuItem.Click += new System.EventHandler(this.exportModListToolStripMenuItem_Click);
            resources.ApplyResources(this.exportModListToolStripMenuItem, "exportModListToolStripMenuItem");
            //
            // exportModPackToolStripMenuItem
            //
            this.exportModPackToolStripMenuItem.Name = "exportModPackToolStripMenuItem";
            this.exportModPackToolStripMenuItem.Size = new System.Drawing.Size(281, 30);
            this.exportModPackToolStripMenuItem.Click += new System.EventHandler(this.exportModPackToolStripMenuItem_Click);
            resources.ApplyResources(this.exportModPackToolStripMenuItem, "exportModPackToolStripMenuItem");
            //
            // toolStripSeparator2
            //
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(278, 6);
            //
            // importDownloadsToolStripMenuItem
            //
            this.importDownloadsToolStripMenuItem.Name = "importDownloadsToolStripMenuItem";
            this.importDownloadsToolStripMenuItem.Size = new System.Drawing.Size(281, 30);
            this.importDownloadsToolStripMenuItem.Click += new System.EventHandler(this.importDownloadsToolStripMenuItem_Click);
            resources.ApplyResources(this.importDownloadsToolStripMenuItem, "importDownloadsToolStripMenuItem");
            //
            // toolStripSeparator3
            //
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(278, 6);
            //
            // toolStripSeparator7
            //
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(278, 6);
            //
            // auditRecommendationsMenuItem
            //
            this.auditRecommendationsMenuItem.Name = "auditRecommendationsMenuItem";
            this.auditRecommendationsMenuItem.Size = new System.Drawing.Size(281, 30);
            this.auditRecommendationsMenuItem.Click += new System.EventHandler(this.auditRecommendationsMenuItem_Click);
            resources.ApplyResources(this.auditRecommendationsMenuItem, "auditRecommendationsMenuItem");
            //
            // ExitToolButton
            //
            this.ExitToolButton.Name = "ExitToolButton";
            this.ExitToolButton.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.ExitToolButton.Size = new System.Drawing.Size(281, 30);
            this.ExitToolButton.Click += new System.EventHandler(this.ExitToolButton_Click);
            resources.ApplyResources(this.ExitToolButton, "ExitToolButton");
            //
            // settingsToolStripMenuItem
            //
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cKANSettingsToolStripMenuItem,
            this.pluginsToolStripMenuItem,
            this.kSPCommandlineToolStripMenuItem,
            this.compatibleKSPVersionsToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(88, 29);
            resources.ApplyResources(this.settingsToolStripMenuItem, "settingsToolStripMenuItem");
            //
            // cKANSettingsToolStripMenuItem
            //
            this.cKANSettingsToolStripMenuItem.Name = "cKANSettingsToolStripMenuItem";
            this.cKANSettingsToolStripMenuItem.Size = new System.Drawing.Size(247, 30);
            this.cKANSettingsToolStripMenuItem.Click += new System.EventHandler(this.CKANSettingsToolStripMenuItem_Click);
            resources.ApplyResources(this.cKANSettingsToolStripMenuItem, "cKANSettingsToolStripMenuItem");
            //
            // pluginsToolStripMenuItem
            //
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(247, 30);
            this.pluginsToolStripMenuItem.Click += new System.EventHandler(this.pluginsToolStripMenuItem_Click);
            resources.ApplyResources(this.pluginsToolStripMenuItem, "pluginsToolStripMenuItem");
            //
            // kSPCommandlineToolStripMenuItem
            //
            this.kSPCommandlineToolStripMenuItem.Name = "kSPCommandlineToolStripMenuItem";
            this.kSPCommandlineToolStripMenuItem.Size = new System.Drawing.Size(247, 30);
            this.kSPCommandlineToolStripMenuItem.Click += new System.EventHandler(this.KSPCommandlineToolStripMenuItem_Click);
            resources.ApplyResources(this.kSPCommandlineToolStripMenuItem, "kSPCommandlineToolStripMenuItem");
            //
            // compatibleKSPVersionsToolStripMenuItem
            //
            this.compatibleKSPVersionsToolStripMenuItem.Name = "compatibleKSPVersionsToolStripMenuItem";
            this.compatibleKSPVersionsToolStripMenuItem.Size = new System.Drawing.Size(233, 24);
            this.compatibleKSPVersionsToolStripMenuItem.Click += new System.EventHandler(this.CompatibleKspVersionsToolStripMenuItem_Click);
            resources.ApplyResources(this.compatibleKSPVersionsToolStripMenuItem, "compatibleKSPVersionsToolStripMenuItem");
            //
            // helpToolStripMenuItem
            //
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reportClientIssueToolStripMenuItem,
            this.reportMetadataIssueToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(61, 29);
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            //
            // reportClientIssueToolStripMenuItem
            //
            this.reportClientIssueToolStripMenuItem.Name = "reportClientIssueToolStripMenuItem";
            this.reportClientIssueToolStripMenuItem.Size = new System.Drawing.Size(230, 30);
            this.reportClientIssueToolStripMenuItem.Click += new System.EventHandler(this.reportClientIssueToolStripMenuItem_Click);
            resources.ApplyResources(this.reportClientIssueToolStripMenuItem, "reportClientIssueToolStripMenuItem");
            //
            // reportMetadataIssueToolStripMenuItem
            //
            this.reportMetadataIssueToolStripMenuItem.Name = "reportMetadataIssueToolStripMenuItem";
            this.reportMetadataIssueToolStripMenuItem.Size = new System.Drawing.Size(230, 30);
            this.reportMetadataIssueToolStripMenuItem.Click += new System.EventHandler(this.reportMetadataIssueToolStripMenuItem_Click);
            resources.ApplyResources(this.reportMetadataIssueToolStripMenuItem, "reportMetadataIssueToolStripMenuItem");
            //
            // aboutToolStripMenuItem
            //
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(230, 30);
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            //
            // statusStrip1
            //
            this.statusStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Location = new System.Drawing.Point(0, 1016);
            this.statusStrip1.Size = new System.Drawing.Size(1544, 22);
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.statusStrip1.TabIndex = 35;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this.StatusLabel,
                this.StatusInstanceLabel,
                this.StatusProgress,
            });
            //
            // menuStrip2
            //
            this.menuStrip2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.menuStrip2.AutoSize = false;
            this.menuStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip2.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchKSPToolStripMenuItem,
            this.RefreshToolButton,
            this.UpdateAllToolButton,
            this.ApplyToolButton,
            this.FilterToolButton,
            this.NavBackwardToolButton,
            this.NavForwardToolButton});
            this.menuStrip2.Location = new System.Drawing.Point(0, 5);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip2.Size = new System.Drawing.Size(5876, 62);
            this.menuStrip2.TabIndex = 4;
            this.menuStrip2.Text = "menuStrip2";
            //
            // launchKSPToolStripMenuItem
            //
            this.launchKSPToolStripMenuItem.Image = global::CKAN.Properties.Resources.ksp;
            this.launchKSPToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.launchKSPToolStripMenuItem.Name = "launchKSPToolStripMenuItem";
            this.launchKSPToolStripMenuItem.Size = new System.Drawing.Size(146, 56);
            this.launchKSPToolStripMenuItem.Click += new System.EventHandler(this.launchKSPToolStripMenuItem_Click);
            resources.ApplyResources(this.launchKSPToolStripMenuItem, "launchKSPToolStripMenuItem");
            //
            // RefreshToolButton
            //
            this.RefreshToolButton.Image = global::CKAN.Properties.Resources.refresh;
            this.RefreshToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.RefreshToolButton.Name = "RefreshToolButton";
            this.RefreshToolButton.Size = new System.Drawing.Size(114, 56);
            this.RefreshToolButton.Click += new System.EventHandler(this.RefreshToolButton_Click);
            resources.ApplyResources(this.RefreshToolButton, "RefreshToolButton");
            //
            // UpdateAllToolButton
            //
            this.UpdateAllToolButton.Image = global::CKAN.Properties.Resources.update;
            this.UpdateAllToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.UpdateAllToolButton.Name = "UpdateAllToolButton";
            this.UpdateAllToolButton.Size = new System.Drawing.Size(232, 56);
            this.UpdateAllToolButton.Click += new System.EventHandler(this.MarkAllUpdatesToolButton_Click);
            resources.ApplyResources(this.UpdateAllToolButton, "UpdateAllToolButton");
            //
            // ApplyToolButton
            //
            this.ApplyToolButton.Image = global::CKAN.Properties.Resources.apply;
            this.ApplyToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ApplyToolButton.Name = "ApplyToolButton";
            this.ApplyToolButton.Size = new System.Drawing.Size(173, 56);
            this.ApplyToolButton.Click += new System.EventHandler(this.ApplyToolButton_Click);
            resources.ApplyResources(this.ApplyToolButton, "ApplyToolButton");
            //
            // FilterToolButton
            //
            this.FilterToolButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FilterCompatibleButton,
            this.FilterInstalledButton,
            this.FilterInstalledUpdateButton,
            this.FilterReplaceableButton,
            this.FilterCachedButton,
            this.FilterUncachedButton,
            this.FilterNewButton,
            this.FilterNotInstalledButton,
            this.FilterIncompatibleButton,
            this.FilterAllButton,
            this.tagFilterToolStripSeparator,
            this.FilterTagsToolButton,
            this.FilterLabelsToolButton});
            this.FilterToolButton.DropDown.Opening += new System.ComponentModel.CancelEventHandler(FilterToolButton_DropDown_Opening);
            this.FilterToolButton.Image = global::CKAN.Properties.Resources.filter;
            this.FilterToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.FilterToolButton.Name = "FilterToolButton";
            this.FilterToolButton.Size = new System.Drawing.Size(201, 56);
            resources.ApplyResources(this.FilterToolButton, "FilterToolButton");
            //
            // FilterCompatibleButton
            //
            this.FilterCompatibleButton.Name = "FilterCompatibleButton";
            this.FilterCompatibleButton.Size = new System.Drawing.Size(307, 30);
            this.FilterCompatibleButton.Click += new System.EventHandler(this.FilterCompatibleButton_Click);
            resources.ApplyResources(this.FilterCompatibleButton, "FilterCompatibleButton");
            //
            // FilterInstalledButton
            //
            this.FilterInstalledButton.Name = "FilterInstalledButton";
            this.FilterInstalledButton.Size = new System.Drawing.Size(307, 30);
            this.FilterInstalledButton.Click += new System.EventHandler(this.FilterInstalledButton_Click);
            resources.ApplyResources(this.FilterInstalledButton, "FilterInstalledButton");
            //
            // FilterInstalledUpdateButton
            //
            this.FilterInstalledUpdateButton.Name = "FilterInstalledUpdateButton";
            this.FilterInstalledUpdateButton.Size = new System.Drawing.Size(307, 30);
            this.FilterInstalledUpdateButton.Click += new System.EventHandler(this.FilterInstalledUpdateButton_Click);
            resources.ApplyResources(this.FilterInstalledUpdateButton, "FilterInstalledUpdateButton");
            //
            // FilterReplaceableButton
            //
            this.FilterReplaceableButton.Name = "FilterReplaceableButton";
            this.FilterReplaceableButton.Size = new System.Drawing.Size(307, 30);
            this.FilterReplaceableButton.Click += new System.EventHandler(this.FilterReplaceableButton_Click);
            resources.ApplyResources(this.FilterReplaceableButton, "FilterReplaceableButton");
            //
            // FilterCachedButton
            //
            this.FilterCachedButton.Name = "FilterCachedButton";
            this.FilterCachedButton.Size = new System.Drawing.Size(307, 30);
            this.FilterCachedButton.Click += new System.EventHandler(this.FilterCachedButton_Click);
            resources.ApplyResources(this.FilterCachedButton, "FilterCachedButton");
            //
            // FilterUncachedButton
            //
            this.FilterUncachedButton.Name = "FilterUncachedButton";
            this.FilterUncachedButton.Size = new System.Drawing.Size(307, 30);
            this.FilterUncachedButton.Click += new System.EventHandler(this.FilterUncachedButton_Click);
            resources.ApplyResources(this.FilterUncachedButton, "FilterUncachedButton");
            //
            // FilterNewButton
            //
            this.FilterNewButton.Name = "FilterNewButton";
            this.FilterNewButton.Size = new System.Drawing.Size(307, 30);
            this.FilterNewButton.Click += new System.EventHandler(this.FilterNewButton_Click);
            resources.ApplyResources(this.FilterNewButton, "FilterNewButton");
            //
            // FilterNotInstalledButton
            //
            this.FilterNotInstalledButton.Name = "FilterNotInstalledButton";
            this.FilterNotInstalledButton.Size = new System.Drawing.Size(307, 30);
            this.FilterNotInstalledButton.Click += new System.EventHandler(this.FilterNotInstalledButton_Click);
            resources.ApplyResources(this.FilterNotInstalledButton, "FilterNotInstalledButton");
            //
            // FilterIncompatibleButton
            //
            this.FilterIncompatibleButton.Name = "FilterIncompatibleButton";
            this.FilterIncompatibleButton.Size = new System.Drawing.Size(307, 30);
            this.FilterIncompatibleButton.Click += new System.EventHandler(this.FilterIncompatibleButton_Click);
            resources.ApplyResources(this.FilterIncompatibleButton, "FilterIncompatibleButton");
            //
            // FilterAllButton
            //
            this.FilterAllButton.Name = "FilterAllButton";
            this.FilterAllButton.Size = new System.Drawing.Size(307, 30);
            this.FilterAllButton.Click += new System.EventHandler(this.FilterAllButton_Click);
            resources.ApplyResources(this.FilterAllButton, "FilterAllButton");
            // 
            // FilterTagsToolButton
            // 
            this.FilterTagsToolButton.Name = "FilterTagsToolButton";
            this.FilterTagsToolButton.Size = new System.Drawing.Size(179, 22);
            resources.ApplyResources(this.FilterTagsToolButton, "FilterTagsToolButton");
            this.FilterTagsToolButton.DropDown.Opening += new System.ComponentModel.CancelEventHandler(FilterTagsToolButton_DropDown_Opening);
            // 
            // FilterLabelsToolButton
            // 
            this.FilterLabelsToolButton.Name = "FilterLabelsToolButton";
            this.FilterLabelsToolButton.Size = new System.Drawing.Size(179, 22);
            resources.ApplyResources(this.FilterLabelsToolButton, "FilterLabelsToolButton");
            this.FilterLabelsToolButton.DropDown.Opening += new System.ComponentModel.CancelEventHandler(FilterLabelsToolButton_DropDown_Opening);
            //
            // NavBackwardToolButton
            //
            this.NavBackwardToolButton.Image = global::CKAN.Properties.Resources.backward;
            this.NavBackwardToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.NavBackwardToolButton.Name = "NavBackwardToolButton";
            this.NavBackwardToolButton.Size = new System.Drawing.Size(44, 56);
            this.NavBackwardToolButton.Click += new System.EventHandler(this.NavBackwardToolButton_Click);
            resources.ApplyResources(this.NavBackwardToolButton, "NavBackwardToolButton");
            //
            // NavForwardToolButton
            //
            this.NavForwardToolButton.Image = global::CKAN.Properties.Resources.forward;
            this.NavForwardToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.NavForwardToolButton.Name = "NavForwardToolButton";
            this.NavForwardToolButton.Size = new System.Drawing.Size(44, 56);
            this.NavForwardToolButton.Click += new System.EventHandler(this.NavForwardToolButton_Click);
            resources.ApplyResources(this.NavForwardToolButton, "NavForwardToolButton");
            //
            // splitContainer1
            //
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 35);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            //
            // splitContainer1.Panel1
            //
            this.splitContainer1.Panel1.Controls.Add(this.MainTabControl);
            this.splitContainer1.Panel1MinSize = 200;
            //
            // splitContainer1.Panel2
            //
            this.splitContainer1.Panel2.Controls.Add(this.ModInfo);
            this.splitContainer1.Panel2MinSize = 300;
            this.splitContainer1.Size = new System.Drawing.Size(1544, 981);
            this.splitContainer1.SplitterDistance = 1156;
            this.splitContainer1.SplitterWidth = 10;
            this.splitContainer1.TabIndex = 1;
            //
            // ModList
            //
            this.ModList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModList.AllowUserToAddRows = false;
            this.ModList.AllowUserToDeleteRows = false;
            this.ModList.AllowUserToResizeRows = false;
            this.ModList.BackgroundColor = System.Drawing.SystemColors.Window;
            this.ModList.EnableHeadersVisualStyles = false;
            this.ModList.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
            this.ModList.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ModList.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ModList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.ModList.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.ModList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ModList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Installed,
            this.AutoInstalled,
            this.UpdateCol,
            this.ReplaceCol,
            this.ModName,
            this.Author,
            this.InstalledVersion,
            this.LatestVersion,
            this.KSPCompatibility,
            this.SizeCol,
            this.InstallDate,
            this.DownloadCount,
            this.Description});
            this.ModList.Location = new System.Drawing.Point(0, 111);
            this.ModList.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ModList.MultiSelect = false;
            this.ModList.Name = "ModList";
            this.ModList.RowHeadersVisible = false;
            this.ModList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ModList.Size = new System.Drawing.Size(1536, 837);
            this.ModList.StandardTab = true;
            this.ModList.TabIndex = 12;
            this.ModList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ModList_CellContentClick);
            this.ModList.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ModList_CellMouseDoubleClick);
            this.ModList.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ModList_HeaderMouseClick);
            this.ModList.SelectionChanged += new System.EventHandler(this.ModList_SelectedIndexChanged);
            this.ModList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ModList_KeyDown);
            this.ModList.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ModList_KeyPress);
            this.ModList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ModList_MouseDown);
            this.ModList.GotFocus += new System.EventHandler(this.ModList_GotFocus);
            this.ModList.LostFocus += new System.EventHandler(this.ModList_LostFocus);
            //
            // Installed
            //
            this.Installed.Name = "Installed";
            this.Installed.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.Installed.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.Installed.Width = 50;
            resources.ApplyResources(this.Installed, "Installed");
            //
            // AutoInstalled
            //
            this.AutoInstalled.Name = "AutoInstalled";
            this.AutoInstalled.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.AutoInstalled.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.AutoInstalled.Width = 50;
            resources.ApplyResources(this.AutoInstalled, "AutoInstalled");
            //
            // UpdateCol
            //
            this.UpdateCol.Name = "UpdateCol";
            this.UpdateCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.UpdateCol.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.UpdateCol.Width = 46;
            resources.ApplyResources(this.UpdateCol, "UpdateCol");
            //
            // ReplaceCol
            //
            this.ReplaceCol.Name = "ReplaceCol";
            this.ReplaceCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.ReplaceCol.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ReplaceCol.Width = 46;
            resources.ApplyResources(this.ReplaceCol, "ReplaceCol");
            //
            // ModName
            //
            this.ModName.Name = "ModName";
            this.ModName.ReadOnly = true;
            this.ModName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.ModName.Width = 250;
            resources.ApplyResources(this.ModName, "ModName");
            //
            // Author
            //
            this.Author.Name = "Author";
            this.Author.ReadOnly = true;
            this.Author.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.Author.Width = 120;
            resources.ApplyResources(this.Author, "Author");
            //
            // InstalledVersion
            //
            this.InstalledVersion.Name = "InstalledVersion";
            this.InstalledVersion.ReadOnly = true;
            this.InstalledVersion.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.InstalledVersion.Width = 70;
            resources.ApplyResources(this.InstalledVersion, "InstalledVersion");
            //
            // LatestVersion
            //
            this.LatestVersion.Name = "LatestVersion";
            this.LatestVersion.ReadOnly = true;
            this.LatestVersion.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.LatestVersion.Width = 70;
            resources.ApplyResources(this.LatestVersion, "LatestVersion");
            //
            // KSPCompatibility
            //
            this.KSPCompatibility.Name = "KSPCompatibility";
            this.KSPCompatibility.ReadOnly = true;
            this.KSPCompatibility.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.KSPCompatibility.Width = 78;
            resources.ApplyResources(this.KSPCompatibility, "KSPCompatibility");
            //
            // SizeCol
            //
            this.SizeCol.Name = "SizeCol";
            this.SizeCol.ReadOnly = true;
            this.SizeCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.SizeCol.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            resources.ApplyResources(this.SizeCol, "SizeCol");
            //
            // InstallDate
            //
            this.InstallDate.Name = "InstallDate";
            this.InstallDate.ReadOnly = true;
            this.InstallDate.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.InstallDate.Width = 140;
            resources.ApplyResources(this.InstallDate, "InstallDate");
            //
            // DownloadCount
            //
            this.DownloadCount.Name = "DownloadCount";
            this.DownloadCount.ReadOnly = true;
            this.DownloadCount.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.DownloadCount.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.DownloadCount.Width = 70;
            resources.ApplyResources(this.DownloadCount, "DownloadCount");
            //
            // Description
            //
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.Description.Width = 821;
            resources.ApplyResources(this.Description, "Description");
            //
            // ModListContextMenuStrip
            //
            this.ModListContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelsToolStripMenuItem,
            this.modListToolStripSeparator,
            this.reinstallToolStripMenuItem,
            this.downloadContentsToolStripMenuItem,
            this.purgeContentsToolStripMenuItem});
            this.ModListContextMenuStrip.Name = "ModListContextMenuStrip";
            this.ModListContextMenuStrip.Size = new System.Drawing.Size(180, 70);
            //
            // labelsToolStripMenuItem
            //
            this.labelsToolStripMenuItem.Name = "labelsToolStripMenuItem";
            this.labelsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.labelsToolStripMenuItem.DropDown = this.LabelsContextMenuStrip;
            resources.ApplyResources(this.labelsToolStripMenuItem, "labelsToolStripMenuItem");
            //
            // reinstallToolStripMenuItem
            //
            this.reinstallToolStripMenuItem.Name = "reinstallToolStripMenuItem";
            this.reinstallToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.reinstallToolStripMenuItem.Click += new System.EventHandler(this.reinstallToolStripMenuItem_Click);
            resources.ApplyResources(this.reinstallToolStripMenuItem, "reinstallToolStripMenuItem");
            //
            // downloadContentsToolStripMenuItem
            //
            this.downloadContentsToolStripMenuItem.Name = "downloadContentsToolStripMenuItem";
            this.downloadContentsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.downloadContentsToolStripMenuItem.Click += new System.EventHandler(this.downloadContentsToolStripMenuItem_Click);
            resources.ApplyResources(this.downloadContentsToolStripMenuItem, "downloadContentsToolStripMenuItem");
            //
            // purgeContentsToolStripMenuItem
            //
            this.purgeContentsToolStripMenuItem.Name = "purgeContentsToolStripMenuItem";
            this.purgeContentsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.purgeContentsToolStripMenuItem.Click += new System.EventHandler(this.purgeContentsToolStripMenuItem_Click);
            resources.ApplyResources(this.purgeContentsToolStripMenuItem, "purgeContentsToolStripMenuItem");
            //
            // LabelsContextMenuStrip
            //
            this.LabelsContextMenuStrip.Name = "LabelsContextMenuStrip";
            this.LabelsContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.editLabelsToolStripMenuItem});
            this.LabelsContextMenuStrip.Size = new System.Drawing.Size(180, 70);
            this.LabelsContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(LabelsContextMenuStrip_Opening);
            //
            // editLabelsToolStripMenuItem
            //
            this.editLabelsToolStripMenuItem.Name = "editLabelsToolStripMenuItem";
            this.editLabelsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.editLabelsToolStripMenuItem.Click += new System.EventHandler(this.editLabelsToolStripMenuItem_Click);
            resources.ApplyResources(this.editLabelsToolStripMenuItem, "editLabelsToolStripMenuItem");
            //
            // ModListHeaderContextMenuStrip
            //
            this.ModListHeaderContextMenuStrip.Name = "ModListHeaderContextMenuStrip";
            this.ModListHeaderContextMenuStrip.AutoSize = true;
            this.ModListHeaderContextMenuStrip.ShowCheckMargin = true;
            this.ModListHeaderContextMenuStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(ModListHeaderContextMenuStrip_ItemClicked);
            //
            // ModInfo
            //
            this.ModInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModInfo.Location = new System.Drawing.Point(0, 0);
            this.ModInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ModInfo.Name = "ModInfo";
            this.ModInfo.Size = new System.Drawing.Size(360, 836);
            this.ModInfo.TabIndex = 34;
            this.ModInfo.OnDownloadClick += ModInfo_OnDownloadClick;
            //
            // StatusLabel
            //
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Font = System.Drawing.SystemFonts.DefaultFont;
            this.StatusLabel.Size = new System.Drawing.Size(1050, 29);
            this.StatusLabel.Spring = true;
            this.StatusLabel.Text = "";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // StatusInstanceLabel
            //
            this.StatusInstanceLabel.AutoSize = true;
            this.StatusInstanceLabel.Name = "StatusInstanceLabel";
            this.StatusInstanceLabel.Font = System.Drawing.SystemFonts.DefaultFont;
            this.StatusInstanceLabel.Size = new System.Drawing.Size(1050, 29);
            this.StatusInstanceLabel.Spring = true;
            this.StatusInstanceLabel.Text = "";
            this.StatusInstanceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // StatusProgress
            //
            this.StatusProgress.Name = "StatusProgress";
            this.StatusProgress.Enabled = true;
            this.StatusProgress.Visible = false;
            this.StatusProgress.Minimum = 0;
            this.StatusProgress.Maximum = 100;
            this.StatusProgress.Size = new System.Drawing.Size(300, 20);
            this.StatusProgress.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            //
            // MainTabControl
            //
            this.MainTabControl.BackColor = System.Drawing.SystemColors.Control;
            this.MainTabControl.Controls.Add(this.ManageModsTabPage);
            this.MainTabControl.Controls.Add(this.ChangesetTabPage);
            this.MainTabControl.Controls.Add(this.WaitTabPage);
            this.MainTabControl.Controls.Add(this.ChooseRecommendedModsTabPage);
            this.MainTabControl.Controls.Add(this.ChooseProvidedModsTabPage);
            this.MainTabControl.Controls.Add(this.DeleteDirectoriesTabPage);
            this.MainTabControl.Controls.Add(this.EditModpackTabPage);
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.Location = new System.Drawing.Point(0, 35);
            this.MainTabControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(1544, 981);
            this.MainTabControl.TabIndex = 2;
            this.MainTabControl.SelectedIndexChanged += new System.EventHandler(this.MainTabControl_OnSelectedIndexChanged);
            //
            // ManageModsTabPage
            //
            this.ManageModsTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.ManageModsTabPage.Controls.Add(this.InstallAllCheckbox);
            this.ManageModsTabPage.Controls.Add(this.FilterByAuthorTextBox);
            this.ManageModsTabPage.Controls.Add(this.FilterByAuthorLabel);
            this.ManageModsTabPage.Controls.Add(this.FilterByNameLabel);
            this.ManageModsTabPage.Controls.Add(this.FilterByNameTextBox);
            this.ManageModsTabPage.Controls.Add(this.FilterByDescriptionLabel);
            this.ManageModsTabPage.Controls.Add(this.FilterByDescriptionTextBox);
            this.ManageModsTabPage.Controls.Add(this.menuStrip2);
            this.ManageModsTabPage.Controls.Add(this.ModList);
            this.ManageModsTabPage.Location = new System.Drawing.Point(4, 29);
            this.ManageModsTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ManageModsTabPage.Name = "ManageModsTabPage";
            this.ManageModsTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ManageModsTabPage.Size = new System.Drawing.Size(1536, 948);
            this.ManageModsTabPage.TabIndex = 3;
            resources.ApplyResources(this.ManageModsTabPage, "ManageModsTabPage");
            //
            // InstallAllCheckbox
            //
            this.InstallAllCheckbox.Location = new System.Drawing.Point(4, 118);
            this.InstallAllCheckbox.Size = new System.Drawing.Size(20, 20);
            this.InstallAllCheckbox.Checked = true;
            this.InstallAllCheckbox.CheckedChanged += new System.EventHandler(this.InstallAllCheckbox_CheckChanged);
            this.InstallAllCheckbox.TabIndex = 11;
            this.InstallAllCheckbox.TabStop = false;
            //
            // FilterByAuthorTextBox
            //
            this.FilterByAuthorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByAuthorTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByAuthorTextBox.Name = "FilterByAuthorTextBox";
            this.FilterByAuthorTextBox.TabIndex = 8;
            this.FilterByAuthorTextBox.TextChanged += new System.EventHandler(this.FilterByAuthorTextBox_TextChanged);
            this.FilterByAuthorTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByAuthorTextBox, "FilterByAuthorTextBox");
            //
            // FilterByAuthorLabel
            //
            this.FilterByAuthorLabel.AutoSize = true;
            this.FilterByAuthorLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByAuthorLabel.Location = new System.Drawing.Point(372, 77);
            this.FilterByAuthorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByAuthorLabel.Name = "FilterByAuthorLabel";
            this.FilterByAuthorLabel.Size = new System.Drawing.Size(162, 20);
            this.FilterByAuthorLabel.TabIndex = 7;
            resources.ApplyResources(this.FilterByAuthorLabel, "FilterByAuthorLabel");
            //
            // FilterByNameLabel
            //
            this.FilterByNameLabel.AutoSize = true;
            this.FilterByNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByNameLabel.Location = new System.Drawing.Point(6, 77);
            this.FilterByNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByNameLabel.Name = "FilterByNameLabel";
            this.FilterByNameLabel.Size = new System.Drawing.Size(147, 20);
            this.FilterByNameLabel.TabIndex = 5;
            resources.ApplyResources(this.FilterByNameLabel, "FilterByNameLabel");
            //
            // FilterByNameTextBox
            //
            this.FilterByNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByNameTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByNameTextBox.Name = "FilterByNameTextBox";
            this.FilterByNameTextBox.TabIndex = 6;
            this.FilterByNameTextBox.TextChanged += new System.EventHandler(this.FilterByNameTextBox_TextChanged);
            this.FilterByNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByNameTextBox, "FilterByNameTextBox");
            //
            // FilterByDescriptionLabel
            //
            this.FilterByDescriptionLabel.AutoSize = true;
            this.FilterByDescriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByDescriptionLabel.Location = new System.Drawing.Point(754, 77);
            this.FilterByDescriptionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByDescriptionLabel.Name = "FilterByDescriptionLabel";
            this.FilterByDescriptionLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByDescriptionLabel.TabIndex = 9;
            resources.ApplyResources(this.FilterByDescriptionLabel, "FilterByDescriptionLabel");
            //
            // FilterByDescriptionTextBox
            //
            this.FilterByDescriptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByDescriptionTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByDescriptionTextBox.Name = "FilterByDescriptionTextBox";
            this.FilterByDescriptionTextBox.TabIndex = 10;
            this.FilterByDescriptionTextBox.TextChanged += new System.EventHandler(this.FilterByDescriptionTextBox_TextChanged);
            this.FilterByDescriptionTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByDescriptionTextBox, "FilterByDescriptionTextBox");
            //
            // ChangesetTabPage
            //
            this.ChangesetTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.ChangesetTabPage.Controls.Add(this.Changeset);
            this.ChangesetTabPage.Location = new System.Drawing.Point(4, 29);
            this.ChangesetTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ChangesetTabPage.Name = "ChangesetTabPage";
            this.ChangesetTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ChangesetTabPage.Size = new System.Drawing.Size(1536, 948);
            this.ChangesetTabPage.TabIndex = 13;
            resources.ApplyResources(this.ChangesetTabPage, "ChangesetTabPage");
            //
            // Changeset
            //
            this.Changeset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Changeset.Location = new System.Drawing.Point(0, 0);
            this.Changeset.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.Changeset.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.Changeset.Name = "Changeset";
            this.Changeset.Size = new System.Drawing.Size(500, 500);
            this.Changeset.TabIndex = 32;
            this.Changeset.OnSelectedItemsChanged += Changeset_OnSelectedItemsChanged;
            this.Changeset.OnConfirmChanges += Changeset_OnConfirmChanges;
            this.Changeset.OnCancelChanges += Changeset_OnCancelChanges;
            //
            // WaitTabPage
            //
            this.WaitTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.WaitTabPage.Controls.Add(this.Wait);
            this.WaitTabPage.Location = new System.Drawing.Point(4, 29);
            this.WaitTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.WaitTabPage.Name = "WaitTabPage";
            this.WaitTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.WaitTabPage.Size = new System.Drawing.Size(1536, 948);
            this.WaitTabPage.TabIndex = 17;
            resources.ApplyResources(this.WaitTabPage, "WaitTabPage");
            //
            // Wait
            //
            this.Wait.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Wait.Location = new System.Drawing.Point(0, 0);
            this.Wait.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.Wait.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.Wait.Name = "Wait";
            this.Wait.Size = new System.Drawing.Size(500, 500);
            this.Wait.TabIndex = 32;
            this.Wait.OnRetry += Wait_OnRetry;
            this.Wait.OnCancel += Wait_OnCancel;
            //
            // ChooseRecommendedModsTabPage
            //
            this.ChooseRecommendedModsTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.ChooseRecommendedModsTabPage.Controls.Add(this.ChooseRecommendedMods);
            this.ChooseRecommendedModsTabPage.Location = new System.Drawing.Point(4, 29);
            this.ChooseRecommendedModsTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ChooseRecommendedModsTabPage.Name = "ChooseRecommendedModsTabPage";
            this.ChooseRecommendedModsTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ChooseRecommendedModsTabPage.Size = new System.Drawing.Size(1536, 948);
            this.ChooseRecommendedModsTabPage.TabIndex = 23;
            resources.ApplyResources(this.ChooseRecommendedModsTabPage, "ChooseRecommendedModsTabPage");
            //
            // ChooseRecommendedMods
            //
            this.ChooseRecommendedMods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChooseRecommendedMods.Location = new System.Drawing.Point(0, 0);
            this.ChooseRecommendedMods.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.ChooseRecommendedMods.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.ChooseRecommendedMods.Name = "ChooseRecommendedMods";
            this.ChooseRecommendedMods.Size = new System.Drawing.Size(500, 500);
            this.ChooseRecommendedMods.TabIndex = 32;
            this.ChooseRecommendedMods.OnSelectedItemsChanged += ChooseRecommendedMods_OnSelectedItemsChanged;
            //
            // ChooseProvidedModsTabPage
            //
            this.ChooseProvidedModsTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.ChooseProvidedModsTabPage.Controls.Add(this.ChooseProvidedMods);
            this.ChooseProvidedModsTabPage.Location = new System.Drawing.Point(4, 29);
            this.ChooseProvidedModsTabPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ChooseProvidedModsTabPage.Name = "ChooseProvidedModsTabPage";
            this.ChooseProvidedModsTabPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ChooseProvidedModsTabPage.Size = new System.Drawing.Size(1536, 948);
            this.ChooseProvidedModsTabPage.TabIndex = 29;
            resources.ApplyResources(this.ChooseProvidedModsTabPage, "ChooseProvidedModsTabPage");
            //
            // ChooseProvidedMods
            //
            this.ChooseProvidedMods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChooseProvidedMods.Location = new System.Drawing.Point(0, 0);
            this.ChooseProvidedMods.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.ChooseProvidedMods.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.ChooseProvidedMods.Name = "ChooseProvidedMods";
            this.ChooseProvidedMods.Size = new System.Drawing.Size(500, 500);
            this.ChooseProvidedMods.TabIndex = 32;
            this.ChooseProvidedMods.OnSelectedItemsChanged += ChooseProvidedMods_OnSelectedItemsChanged;
            //
            // DeleteDirectoriesTabPage
            //
            this.DeleteDirectoriesTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.DeleteDirectoriesTabPage.Controls.Add(this.DeleteDirectories);
            this.DeleteDirectoriesTabPage.Location = new System.Drawing.Point(0, 0);
            this.DeleteDirectoriesTabPage.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.DeleteDirectoriesTabPage.Name = "DeleteDirectoriesTabPage";
            this.DeleteDirectoriesTabPage.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.DeleteDirectoriesTabPage.Size = new System.Drawing.Size(500, 500);
            this.DeleteDirectoriesTabPage.TabIndex = 31;
            resources.ApplyResources(this.DeleteDirectoriesTabPage, "DeleteDirectoriesTabPage");
            //
            // DeleteDirectories
            //
            this.DeleteDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DeleteDirectories.Location = new System.Drawing.Point(0, 0);
            this.DeleteDirectories.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.DeleteDirectories.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.DeleteDirectories.Name = "DeleteDirectories";
            this.DeleteDirectories.Size = new System.Drawing.Size(500, 500);
            this.DeleteDirectories.TabIndex = 32;
            //
            // EditModpackTabPage
            //
            this.EditModpackTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.EditModpackTabPage.Controls.Add(this.EditModpack);
            this.EditModpackTabPage.Location = new System.Drawing.Point(0, 0);
            this.EditModpackTabPage.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.EditModpackTabPage.Name = "EditModpackTabPage";
            this.EditModpackTabPage.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.EditModpackTabPage.Size = new System.Drawing.Size(500, 500);
            this.EditModpackTabPage.TabIndex = 31;
            resources.ApplyResources(this.EditModpackTabPage, "EditModpackTabPage");
            //
            // EditModpack
            //
            this.EditModpack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EditModpack.Location = new System.Drawing.Point(0, 0);
            this.EditModpack.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.EditModpack.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.EditModpack.Name = "EditModpack";
            this.EditModpack.Size = new System.Drawing.Size(500, 500);
            this.EditModpack.TabIndex = 32;
            this.EditModpack.OnSelectedItemsChanged += this.EditModpack_OnSelectedItemsChanged;
            //
            // minimizeNotifyIcon
            //
            this.minimizeNotifyIcon.ContextMenuStrip = this.minimizedContextMenuStrip;
            this.minimizeNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.minimizeNotifyIcon_MouseDoubleClick);
            this.minimizeNotifyIcon.BalloonTipClicked += new System.EventHandler(this.minimizeNotifyIcon_BalloonTipClicked);
            this.minimizeNotifyIcon.Icon = Properties.Resources.AppIcon;
            resources.ApplyResources(this.minimizeNotifyIcon, "minimizeNotifyIcon");
            //
            // minimizedContextMenuStrip
            //
            this.minimizedContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updatesToolStripMenuItem,
            this.toolStripSeparator4,
            this.refreshToolStripMenuItem,
            this.pauseToolStripMenuItem,
            this.toolStripSeparator5,
            this.openCKANToolStripMenuItem,
            this.openKSPToolStripMenuItem,
            this.openKSPDirectoryToolStripMenuItem1,
            this.cKANSettingsToolStripMenuItem1,
            this.toolStripSeparator6,
            this.quitToolStripMenuItem});
            this.minimizedContextMenuStrip.Name = "minimizedContextMenuStrip";
            this.minimizedContextMenuStrip.Size = new System.Drawing.Size(181, 148);
            this.minimizedContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.minimizedContextMenuStrip_Opening);
            //
            // updatesToolStripMenuItem
            //
            this.updatesToolStripMenuItem.Name = "updatesToolStripMenuItem";
            this.updatesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.updatesToolStripMenuItem.Click += new System.EventHandler(this.updatesToolStripMenuItem_Click);
            resources.ApplyResources(this.updatesToolStripMenuItem, "updatesToolStripMenuItem");
            //
            // toolStripSeparator4
            //
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(177, 6);
            //
            // refreshToolStripMenuItem
            //
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            resources.ApplyResources(this.refreshToolStripMenuItem, "refreshToolStripMenuItem");
            //
            // pauseToolStripMenuItem
            //
            this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
            this.pauseToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pauseToolStripMenuItem.Click += new System.EventHandler(this.pauseToolStripMenuItem_Click);
            resources.ApplyResources(this.pauseToolStripMenuItem, "pauseToolStripMenuItem");
            //
            // toolStripSeparator5
            //
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(177, 6);
            //
            // openCKANToolStripMenuItem
            //
            this.openCKANToolStripMenuItem.Name = "openCKANToolStripMenuItem";
            this.openCKANToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openCKANToolStripMenuItem.Click += new System.EventHandler(this.openCKANToolStripMenuItem_Click);
            resources.ApplyResources(this.openCKANToolStripMenuItem, "openCKANToolStripMenuItem");
            //
            //
            // openKSPToolStripMenuItem
            //
            this.openKSPToolStripMenuItem.Name = "launchKSPToolStripMenuItem";
            this.openKSPToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openKSPToolStripMenuItem.Click += new System.EventHandler(this.launchKSPToolStripMenuItem_Click);
            resources.ApplyResources(this.openKSPToolStripMenuItem, "openKSPToolStripMenuItem");
            //
            // openKSPDirectoryToolStripMenuItem1
            //
            this.openKSPDirectoryToolStripMenuItem1.Name = "openKSPDirectoryToolStripMenuItem1";
            this.openKSPDirectoryToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.openKSPDirectoryToolStripMenuItem1.Click += new System.EventHandler(this.openKspDirectoryToolStripMenuItem_Click);
            resources.ApplyResources(this.openKSPDirectoryToolStripMenuItem1, "openKSPDirectoryToolStripMenuItem1");
            //
            // cKANSettingsToolStripMenuItem1
            //
            this.cKANSettingsToolStripMenuItem1.Name = "cKANSettingsToolStripMenuItem1";
            this.cKANSettingsToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.cKANSettingsToolStripMenuItem1.Click += new System.EventHandler(this.cKANSettingsToolStripMenuItem1_Click);
            resources.ApplyResources(this.cKANSettingsToolStripMenuItem1, "cKANSettingsToolStripMenuItem1");
            //
            // toolStripSeparator6
            //
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(177, 6);
            //
            // quitToolStripMenuItem
            //
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolButton_Click);
            resources.ApplyResources(this.quitToolStripMenuItem, "quitToolStripMenuItem");
            //
            // Main
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1544, 1038);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(1280, 700);
            this.Name = "Main";
            this.Resize += new System.EventHandler(this.Main_Resize);
            this.Icon = Properties.Resources.AppIcon;
            resources.ApplyResources(this, "$this");
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ModList)).EndInit();
            this.ModListContextMenuStrip.ResumeLayout(false);
            this.ModListHeaderContextMenuStrip.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.ManageModsTabPage.ResumeLayout(false);
            this.ManageModsTabPage.PerformLayout();
            this.ChangesetTabPage.ResumeLayout(false);
            this.ChangesetTabPage.PerformLayout();
            this.WaitTabPage.ResumeLayout(false);
            this.WaitTabPage.PerformLayout();
            this.ChooseRecommendedModsTabPage.ResumeLayout(false);
            this.ChooseRecommendedModsTabPage.PerformLayout();
            this.ChooseProvidedModsTabPage.ResumeLayout(false);
            this.ChooseProvidedModsTabPage.PerformLayout();
            this.DeleteDirectoriesTabPage.ResumeLayout(false);
            this.DeleteDirectoriesTabPage.PerformLayout();
            this.EditModpackTabPage.ResumeLayout(false);
            this.EditModpackTabPage.PerformLayout();
            this.minimizedContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageKspInstancesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openKspDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem installFromckanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportModListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportModPackToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem importDownloadsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem auditRecommendationsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExitToolButton;
        public System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cKANSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pluginsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem kSPCommandlineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compatibleKSPVersionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportClientIssueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportMetadataIssueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem launchKSPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RefreshToolButton;
        private System.Windows.Forms.ToolStripMenuItem UpdateAllToolButton;
        private System.Windows.Forms.ToolStripMenuItem ApplyToolButton;
        private System.Windows.Forms.ToolStripMenuItem FilterToolButton;
        private System.Windows.Forms.ToolStripMenuItem FilterCompatibleButton;
        private System.Windows.Forms.ToolStripMenuItem FilterInstalledButton;
        private System.Windows.Forms.ToolStripMenuItem FilterInstalledUpdateButton;
        private System.Windows.Forms.ToolStripMenuItem FilterReplaceableButton;
        private System.Windows.Forms.ToolStripMenuItem FilterCachedButton;
        private System.Windows.Forms.ToolStripMenuItem FilterUncachedButton;
        private System.Windows.Forms.ToolStripMenuItem FilterNewButton;
        private System.Windows.Forms.ToolStripMenuItem FilterNotInstalledButton;
        private System.Windows.Forms.ToolStripMenuItem FilterIncompatibleButton;
        private System.Windows.Forms.ToolStripMenuItem FilterAllButton;
        private System.Windows.Forms.ToolStripMenuItem FilterLabelsToolButton;
        private System.Windows.Forms.ToolStripMenuItem FilterTagsToolButton;
        private System.Windows.Forms.ToolStripMenuItem NavBackwardToolButton;
        private System.Windows.Forms.ToolStripMenuItem NavForwardToolButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        public CKAN.MainModListGUI ModList;
        private System.Windows.Forms.CheckBox InstallAllCheckbox;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Installed;
        private System.Windows.Forms.DataGridViewCheckBoxColumn AutoInstalled;
        private System.Windows.Forms.DataGridViewCheckBoxColumn UpdateCol;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ReplaceCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn ModName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Author;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstalledVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn LatestVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn KSPCompatibility;
        private System.Windows.Forms.DataGridViewTextBoxColumn SizeCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstallDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn DownloadCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
        private System.Windows.Forms.ContextMenuStrip ModListContextMenuStrip;
        private System.Windows.Forms.ToolStripSeparator modListToolStripSeparator;
        private System.Windows.Forms.ToolStripSeparator tagFilterToolStripSeparator;
        private System.Windows.Forms.ToolStripSeparator untaggedFilterToolStripSeparator;
        private System.Windows.Forms.ContextMenuStrip LabelsContextMenuStrip;
        private System.Windows.Forms.ContextMenuStrip ModListHeaderContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem labelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator labelToolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem editLabelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reinstallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downloadContentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem purgeContentsToolStripMenuItem;
        private CKAN.ModInfo ModInfo;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusInstanceLabel;
        private System.Windows.Forms.ToolStripProgressBar StatusProgress;
        private CKAN.MainTabControl MainTabControl;
        private System.Windows.Forms.TabPage ManageModsTabPage;
        private CKAN.HintTextBox FilterByAuthorTextBox;
        private System.Windows.Forms.Label FilterByAuthorLabel;
        private System.Windows.Forms.Label FilterByNameLabel;
        private CKAN.HintTextBox FilterByNameTextBox;
        private System.Windows.Forms.Label FilterByDescriptionLabel;
        private CKAN.HintTextBox FilterByDescriptionTextBox;
        private System.Windows.Forms.TabPage ChangesetTabPage;
        private CKAN.Changeset Changeset;
        private System.Windows.Forms.TabPage WaitTabPage;
        private CKAN.Wait Wait;
        private System.Windows.Forms.TabPage ChooseRecommendedModsTabPage;
        private CKAN.ChooseRecommendedMods ChooseRecommendedMods;
        private System.Windows.Forms.TabPage ChooseProvidedModsTabPage;
        private CKAN.ChooseProvidedMods ChooseProvidedMods;
        private System.Windows.Forms.TabPage DeleteDirectoriesTabPage;
        private CKAN.DeleteDirectories DeleteDirectories;
        private System.Windows.Forms.TabPage EditModpackTabPage;
        private CKAN.EditModpack EditModpack;
        private System.Windows.Forms.NotifyIcon minimizeNotifyIcon;
        private System.Windows.Forms.ContextMenuStrip minimizedContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem updatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem openCKANToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openKSPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openKSPDirectoryToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem cKANSettingsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
    }
}
