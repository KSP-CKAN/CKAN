using System.Windows.Forms;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectKSPInstallMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openKspDirectoyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installFromckanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportModListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ExitToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cKANSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kSPCommandlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.FilterNewButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterNotInstalledButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterIncompatibleButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterAllButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ModList = new MainModListGUI();
            this.Installed = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.UpdateCol = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ModName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LatestVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.KSPCompatibility = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ModInfoTabControl = new System.Windows.Forms.TabControl();
            this.MetadataTabPage = new System.Windows.Forms.TabPage();
            this.MetadataLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.MetadataModuleVersionLabel = new System.Windows.Forms.Label();
            this.LicenseLabel = new System.Windows.Forms.Label();
            this.MetadataModuleLicenseLabel = new System.Windows.Forms.Label();
            this.AuthorLabel = new System.Windows.Forms.Label();
            this.MetadataModuleAuthorLabel = new System.Windows.Forms.Label();
            this.HomePageLabel = new System.Windows.Forms.Label();
            this.GitHubLabel = new System.Windows.Forms.Label();
            this.MetadataModuleReleaseStatusLabel = new System.Windows.Forms.Label();
            this.ReleaseLabel = new System.Windows.Forms.Label();
            this.KSPCompatibilityLabel = new System.Windows.Forms.Label();
            this.MetadataModuleKSPCompatibilityLabel = new System.Windows.Forms.Label();
            this.MetadataModuleHomePageLinkLabel = new System.Windows.Forms.LinkLabel();
            this.MetadataModuleGitHubLinkLabel = new System.Windows.Forms.LinkLabel();
            this.MetadataModuleNameLabel = new System.Windows.Forms.Label();
            this.MetadataModuleAbstractLabel = new System.Windows.Forms.RichTextBox();
            this.RelationshipTabPage = new System.Windows.Forms.TabPage();
            this.ModuleRelationshipType = new System.Windows.Forms.ComboBox();
            this.DependsGraphTree = new System.Windows.Forms.TreeView();
            this.ContentTabPage = new System.Windows.Forms.TabPage();
            this.ContentsPreviewTree = new System.Windows.Forms.TreeView();
            this.ContentsDownloadButton = new System.Windows.Forms.Button();
            this.NotCachedLabel = new System.Windows.Forms.Label();
            this.StatusPanel = new System.Windows.Forms.Panel();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.ManageModsTabPage = new System.Windows.Forms.TabPage();
            this.FilterByAuthorTextBox = new System.Windows.Forms.TextBox();
            this.FilterByAuthorLabel = new System.Windows.Forms.Label();
            this.KSPVersionLabel = new System.Windows.Forms.Label();
            this.FilterByNameLabel = new System.Windows.Forms.Label();
            this.FilterByNameTextBox = new System.Windows.Forms.TextBox();
            this.ChangesetTabPage = new System.Windows.Forms.TabPage();
            this.CancelChangesButton = new System.Windows.Forms.Button();
            this.ConfirmChangesButton = new System.Windows.Forms.Button();
            this.ChangesListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ChangeType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.WaitTabPage = new System.Windows.Forms.TabPage();
            this.CancelCurrentActionButton = new System.Windows.Forms.Button();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.DialogProgressBar = new System.Windows.Forms.ProgressBar();
            this.MessageTextBox = new System.Windows.Forms.TextBox();
            this.ChooseRecommendedModsTabPage = new System.Windows.Forms.TabPage();
            this.RecommendedModsCancelButton = new System.Windows.Forms.Button();
            this.RecommendedModsContinueButton = new System.Windows.Forms.Button();
            this.RecommendedDialogLabel = new System.Windows.Forms.Label();
            this.RecommendedModsListView = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ChooseProvidedModsTabPage = new System.Windows.Forms.TabPage();
            this.ChooseProvidedModsCancelButton = new System.Windows.Forms.Button();
            this.ChooseProvidedModsContinueButton = new System.Windows.Forms.Button();
            this.ChooseProvidedModsListView = new System.Windows.Forms.ListView();
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ChooseProvidedModsLabel = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ModList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.ModInfoTabControl.SuspendLayout();
            this.MetadataTabPage.SuspendLayout();
            this.MetadataLayoutPanel.SuspendLayout();
            this.RelationshipTabPage.SuspendLayout();
            this.ContentTabPage.SuspendLayout();
            this.StatusPanel.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.ManageModsTabPage.SuspendLayout();
            this.ChangesetTabPage.SuspendLayout();
            this.WaitTabPage.SuspendLayout();
            this.ChooseRecommendedModsTabPage.SuspendLayout();
            this.ChooseProvidedModsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1029, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectKSPInstallMenuItem,
            this.openKspDirectoyToolStripMenuItem,
            this.installFromckanToolStripMenuItem,
            this.exportModListToolStripMenuItem,
            this.toolStripSeparator1,
            this.ExitToolButton});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // selectKSPInstallMenuItem
            // 
            this.selectKSPInstallMenuItem.Name = "selectKSPInstallMenuItem";
            this.selectKSPInstallMenuItem.Size = new System.Drawing.Size(196, 22);
            this.selectKSPInstallMenuItem.Text = "Select KSP Install...";
            this.selectKSPInstallMenuItem.Click += new System.EventHandler(this.selectKSPInstallMenuItem_Click);
            // 
            // openKspDirectoyToolStripMenuItem
            // 
            this.openKspDirectoyToolStripMenuItem.Name = "openKspDirectoyToolStripMenuItem";
            this.openKspDirectoyToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.openKspDirectoyToolStripMenuItem.Text = "Open KSP Directoy";
            this.openKspDirectoyToolStripMenuItem.Click += new System.EventHandler(this.openKspDirectoyToolStripMenuItem_Click);
            // 
            // installFromckanToolStripMenuItem
            // 
            this.installFromckanToolStripMenuItem.Name = "installFromckanToolStripMenuItem";
            this.installFromckanToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.installFromckanToolStripMenuItem.Text = "Install from .ckan...";
            this.installFromckanToolStripMenuItem.Click += new System.EventHandler(this.installFromckanToolStripMenuItem_Click);
            // 
            // exportModListToolStripMenuItem
            // 
            this.exportModListToolStripMenuItem.Name = "exportModListToolStripMenuItem";
            this.exportModListToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.exportModListToolStripMenuItem.Text = "&Export installed mods...";
            this.exportModListToolStripMenuItem.Click += new System.EventHandler(this.exportModListToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(193, 6);
            // 
            // ExitToolButton
            // 
            this.ExitToolButton.Name = "ExitToolButton";
            this.ExitToolButton.Size = new System.Drawing.Size(196, 22);
            this.ExitToolButton.Text = "Exit";
            this.ExitToolButton.Click += new System.EventHandler(this.ExitToolButton_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cKANSettingsToolStripMenuItem,
            this.pluginsToolStripMenuItem,
            this.kSPCommandlineToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // cKANSettingsToolStripMenuItem
            // 
            this.cKANSettingsToolStripMenuItem.Name = "cKANSettingsToolStripMenuItem";
            this.cKANSettingsToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.cKANSettingsToolStripMenuItem.Text = "CKAN settings";
            this.cKANSettingsToolStripMenuItem.Click += new System.EventHandler(this.CKANSettingsToolStripMenuItem_Click);
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.pluginsToolStripMenuItem.Text = "CKAN plugins";
            this.pluginsToolStripMenuItem.Click += new System.EventHandler(this.pluginsToolStripMenuItem_Click);
            // 
            // kSPCommandlineToolStripMenuItem
            // 
            this.kSPCommandlineToolStripMenuItem.Name = "kSPCommandlineToolStripMenuItem";
            this.kSPCommandlineToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.kSPCommandlineToolStripMenuItem.Text = "KSP command-line";
            this.kSPCommandlineToolStripMenuItem.Click += new System.EventHandler(this.KSPCommandlineToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 696);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1029, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // menuStrip2
            // 
            this.menuStrip2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.menuStrip2.AutoSize = false;
            this.menuStrip2.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchKSPToolStripMenuItem,
            this.RefreshToolButton,
            this.UpdateAllToolButton,
            this.ApplyToolButton,
            this.FilterToolButton});
            this.menuStrip2.Location = new System.Drawing.Point(0, 3);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(3917, 40);
            this.menuStrip2.TabIndex = 2;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // launchKSPToolStripMenuItem
            // 
            this.launchKSPToolStripMenuItem.Image = global::CKAN.Properties.Resources.ksp;
            this.launchKSPToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.launchKSPToolStripMenuItem.Name = "launchKSPToolStripMenuItem";
            this.launchKSPToolStripMenuItem.Size = new System.Drawing.Size(113, 36);
            this.launchKSPToolStripMenuItem.Text = "Launch KSP";
            this.launchKSPToolStripMenuItem.Click += new System.EventHandler(this.launchKSPToolStripMenuItem_Click);
            // 
            // RefreshToolButton
            // 
            this.RefreshToolButton.Image = global::CKAN.Properties.Resources.refresh;
            this.RefreshToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.RefreshToolButton.Name = "RefreshToolButton";
            this.RefreshToolButton.Size = new System.Drawing.Size(90, 36);
            this.RefreshToolButton.Text = "Refresh";
            this.RefreshToolButton.Click += new System.EventHandler(this.RefreshToolButton_Click);
            // 
            // UpdateAllToolButton
            // 
            this.UpdateAllToolButton.Image = global::CKAN.Properties.Resources.update;
            this.UpdateAllToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.UpdateAllToolButton.Name = "UpdateAllToolButton";
            this.UpdateAllToolButton.Size = new System.Drawing.Size(140, 36);
            this.UpdateAllToolButton.Text = "Mark all updated";
            this.UpdateAllToolButton.Click += new System.EventHandler(this.MarkAllUpdatesToolButton_Click);
            // 
            // ApplyToolButton
            // 
            this.ApplyToolButton.Image = global::CKAN.Properties.Resources.apply;
            this.ApplyToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ApplyToolButton.Name = "ApplyToolButton";
            this.ApplyToolButton.Size = new System.Drawing.Size(127, 36);
            this.ApplyToolButton.Text = "Go to changes";
            this.ApplyToolButton.Click += new System.EventHandler(this.ApplyToolButton_Click);
            // 
            // FilterToolButton
            // 
            this.FilterToolButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FilterCompatibleButton,
            this.FilterInstalledButton,
            this.FilterInstalledUpdateButton,
            this.FilterNewButton,
            this.FilterNotInstalledButton,
            this.FilterIncompatibleButton,
            this.FilterAllButton});
            this.FilterToolButton.Image = global::CKAN.Properties.Resources.search;
            this.FilterToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.FilterToolButton.Name = "FilterToolButton";
            this.FilterToolButton.Size = new System.Drawing.Size(150, 36);
            this.FilterToolButton.Text = "Filter (Compatible)";
            // 
            // FilterCompatibleButton
            // 
            this.FilterCompatibleButton.Name = "FilterCompatibleButton";
            this.FilterCompatibleButton.Size = new System.Drawing.Size(215, 22);
            this.FilterCompatibleButton.Text = "Compatible";
            this.FilterCompatibleButton.Click += new System.EventHandler(this.FilterCompatibleButton_Click);
            // 
            // FilterInstalledButton
            // 
            this.FilterInstalledButton.Name = "FilterInstalledButton";
            this.FilterInstalledButton.Size = new System.Drawing.Size(215, 22);
            this.FilterInstalledButton.Text = "Installed";
            this.FilterInstalledButton.Click += new System.EventHandler(this.FilterInstalledButton_Click);
            // 
            // FilterInstalledUpdateButton
            // 
            this.FilterInstalledUpdateButton.Name = "FilterInstalledUpdateButton";
            this.FilterInstalledUpdateButton.Size = new System.Drawing.Size(215, 22);
            this.FilterInstalledUpdateButton.Text = "Installed (update available)";
            this.FilterInstalledUpdateButton.Click += new System.EventHandler(this.FilterInstalledUpdateButton_Click);
            // 
            // FilterNewButton
            // 
            this.FilterNewButton.Name = "FilterNewButton";
            this.FilterNewButton.Size = new System.Drawing.Size(215, 22);
            this.FilterNewButton.Text = "New in repository";
            this.FilterNewButton.Click += new System.EventHandler(this.FilterNewButton_Click);
            // 
            // FilterNotInstalledButton
            // 
            this.FilterNotInstalledButton.Name = "FilterNotInstalledButton";
            this.FilterNotInstalledButton.Size = new System.Drawing.Size(215, 22);
            this.FilterNotInstalledButton.Text = "Not installed";
            this.FilterNotInstalledButton.Click += new System.EventHandler(this.FilterNotInstalledButton_Click);
            // 
            // FilterIncompatibleButton
            // 
            this.FilterIncompatibleButton.Name = "FilterIncompatibleButton";
            this.FilterIncompatibleButton.Size = new System.Drawing.Size(215, 22);
            this.FilterIncompatibleButton.Text = "Incompatible";
            this.FilterIncompatibleButton.Click += new System.EventHandler(this.FilterIncompatibleButton_Click);
            // 
            // FilterAllButton
            // 
            this.FilterAllButton.Name = "FilterAllButton";
            this.FilterAllButton.Size = new System.Drawing.Size(215, 22);
            this.FilterAllButton.Text = "All";
            this.FilterAllButton.Click += new System.EventHandler(this.FilterAllButton_Click);
            // 
            // ModList
            // 
            this.ModList.AllowUserToAddRows = false;
            this.ModList.AllowUserToDeleteRows = false;
            this.ModList.AllowUserToResizeRows = false;
            this.ModList.BackgroundColor = System.Drawing.SystemColors.Window;
            this.ModList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.ModList.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.ModList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ModList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Installed,
            this.UpdateCol,
            this.ModName,
            this.Author,
            this.InstalledVersion,
            this.LatestVersion,
            this.KSPCompatibility,
            this.Description});
            this.ModList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModList.Location = new System.Drawing.Point(0, 0);
            this.ModList.MultiSelect = false;
            this.ModList.Name = "ModList";
            this.ModList.RowHeadersVisible = false;
            this.ModList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ModList.Size = new System.Drawing.Size(651, 578);
            this.ModList.TabIndex = 3;
            this.ModList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ModList_CellContentClick);
            this.ModList.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ModList_CellMouseDoubleClick);
            this.ModList.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ModList_HeaderMouseClick);
            this.ModList.SelectionChanged += new System.EventHandler(this.ModList_SelectedIndexChanged);
            this.ModList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ModList_KeyDown);
            this.ModList.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ModList_KeyPress);
            // 
            // Installed
            // 
            this.Installed.HeaderText = "Installed";
            this.Installed.Name = "Installed";
            this.Installed.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.Installed.Width = 50;
            // 
            // Update
            //
            this.UpdateCol.HeaderText = "Update";
            this.UpdateCol.Name = "Update";
            this.UpdateCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.UpdateCol.Width = 46;
            //
            // ModName
            // 
            this.ModName.HeaderText = "Name";
            this.ModName.Name = "ModName";
            this.ModName.ReadOnly = true;
            this.ModName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.ModName.Width = 250;
            // 
            // Author
            // 
            this.Author.HeaderText = "Author";
            this.Author.Name = "Author";
            this.Author.ReadOnly = true;
            this.Author.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.Author.Width = 120;
            // 
            // InstalledVersion
            // 
            this.InstalledVersion.HeaderText = "Installed version";
            this.InstalledVersion.Name = "InstalledVersion";
            this.InstalledVersion.ReadOnly = true;
            this.InstalledVersion.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.InstalledVersion.Width = 70;
            // 
            // LatestVersion
            // 
            this.LatestVersion.HeaderText = "Latest version";
            this.LatestVersion.Name = "LatestVersion";
            this.LatestVersion.ReadOnly = true;
            this.LatestVersion.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.LatestVersion.Width = 70;
            //
            // Highest compatible version of KSP
            // 
            this.KSPCompatibility.HeaderText = "Max KSP version";
            this.KSPCompatibility.Name = "KSPCompatibility";
            this.KSPCompatibility.ReadOnly = true;
            this.KSPCompatibility.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.KSPCompatibility.Width = 78;
            // 
            // Description
            // 
            this.Description.HeaderText = "Description";
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.Description.Width = 821;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 72);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ModList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ModInfoTabControl);
            this.splitContainer1.Size = new System.Drawing.Size(1015, 578);
            this.splitContainer1.SplitterDistance = 651;
            this.splitContainer1.TabIndex = 7;
            // 
            // ModInfoTabControl
            // 
            this.ModInfoTabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.ModInfoTabControl.Controls.Add(this.MetadataTabPage);
            this.ModInfoTabControl.Controls.Add(this.RelationshipTabPage);
            this.ModInfoTabControl.Controls.Add(this.ContentTabPage);
            this.ModInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModInfoTabControl.Location = new System.Drawing.Point(0, 0);
            this.ModInfoTabControl.Name = "ModInfoTabControl";
            this.ModInfoTabControl.SelectedIndex = 0;
            this.ModInfoTabControl.Size = new System.Drawing.Size(360, 578);
            this.ModInfoTabControl.TabIndex = 0;
            this.ModInfoTabControl.SelectedIndexChanged += new System.EventHandler(this.ModInfoIndexChanged);
            // 
            // MetadataTabPage
            // 
            this.MetadataTabPage.Controls.Add(this.MetadataLayoutPanel);
            this.MetadataTabPage.Location = new System.Drawing.Point(4, 25);
            this.MetadataTabPage.Name = "MetadataTabPage";
            this.MetadataTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.MetadataTabPage.Size = new System.Drawing.Size(352, 549);
            this.MetadataTabPage.TabIndex = 0;
            this.MetadataTabPage.Text = "Metadata";
            this.MetadataTabPage.UseVisualStyleBackColor = true;
            // 
            // MetadataLayoutPanel
            // 
            this.MetadataLayoutPanel.ColumnCount = 2;
            this.MetadataLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MetadataLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MetadataLayoutPanel.Controls.Add(this.VersionLabel, 0, 3);
            this.MetadataLayoutPanel.Controls.Add(this.MetadataModuleVersionLabel, 1, 3);
            this.MetadataLayoutPanel.Controls.Add(this.LicenseLabel, 0, 4);
            this.MetadataLayoutPanel.Controls.Add(this.MetadataModuleLicenseLabel, 1, 4);
            this.MetadataLayoutPanel.Controls.Add(this.AuthorLabel, 0, 5);
            this.MetadataLayoutPanel.Controls.Add(this.MetadataModuleAuthorLabel, 1, 5);
            this.MetadataLayoutPanel.Controls.Add(this.HomePageLabel, 0, 6);
            this.MetadataLayoutPanel.Controls.Add(this.GitHubLabel, 0, 7);
            this.MetadataLayoutPanel.Controls.Add(this.MetadataModuleReleaseStatusLabel, 1, 8);
            this.MetadataLayoutPanel.Controls.Add(this.ReleaseLabel, 0, 8);
            this.MetadataLayoutPanel.Controls.Add(this.MetadataModuleHomePageLinkLabel, 1, 6);
            this.MetadataLayoutPanel.Controls.Add(this.MetadataModuleGitHubLinkLabel, 1, 7);
            this.MetadataLayoutPanel.Controls.Add(this.MetadataModuleNameLabel, 0, 0);
            this.MetadataLayoutPanel.Controls.Add(this.MetadataModuleAbstractLabel, 0, 2);
            this.MetadataLayoutPanel.Controls.Add(this.KSPCompatibilityLabel, 0, 9);
            this.MetadataLayoutPanel.Controls.Add(this.MetadataModuleKSPCompatibilityLabel, 1, 9);
            this.MetadataLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.MetadataLayoutPanel.Name = "MetadataLayoutPanel";
            this.MetadataLayoutPanel.RowCount = 14;
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.MetadataLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MetadataLayoutPanel.Size = new System.Drawing.Size(346, 543);
            this.MetadataLayoutPanel.TabIndex = 0;
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VersionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.VersionLabel.Location = new System.Drawing.Point(3, 111);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(80, 33);
            this.VersionLabel.TabIndex = 1;
            this.VersionLabel.Text = "Version:";
            // 
            // MetadataModuleVersionLabel
            // 
            this.MetadataModuleVersionLabel.AutoSize = true;
            this.MetadataModuleVersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleVersionLabel.Location = new System.Drawing.Point(89, 111);
            this.MetadataModuleVersionLabel.Name = "MetadataModuleVersionLabel";
            this.MetadataModuleVersionLabel.Size = new System.Drawing.Size(257, 33);
            this.MetadataModuleVersionLabel.TabIndex = 2;
            this.MetadataModuleVersionLabel.Text = "0.0.0";
            // 
            // LicenseLabel
            // 
            this.LicenseLabel.AutoSize = true;
            this.LicenseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LicenseLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.LicenseLabel.Location = new System.Drawing.Point(3, 144);
            this.LicenseLabel.Name = "LicenseLabel";
            this.LicenseLabel.Size = new System.Drawing.Size(80, 32);
            this.LicenseLabel.TabIndex = 3;
            this.LicenseLabel.Text = "License:";
            // 
            // MetadataModuleLicenseLabel
            // 
            this.MetadataModuleLicenseLabel.AutoSize = true;
            this.MetadataModuleLicenseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleLicenseLabel.Location = new System.Drawing.Point(89, 144);
            this.MetadataModuleLicenseLabel.Name = "MetadataModuleLicenseLabel";
            this.MetadataModuleLicenseLabel.Size = new System.Drawing.Size(257, 32);
            this.MetadataModuleLicenseLabel.TabIndex = 4;
            this.MetadataModuleLicenseLabel.Text = "None";
            // 
            // AuthorLabel
            // 
            this.AuthorLabel.AutoSize = true;
            this.AuthorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AuthorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AuthorLabel.Location = new System.Drawing.Point(3, 176);
            this.AuthorLabel.Name = "AuthorLabel";
            this.AuthorLabel.Size = new System.Drawing.Size(80, 32);
            this.AuthorLabel.TabIndex = 5;
            this.AuthorLabel.Text = "Author:";
            // 
            // MetadataModuleAuthorLabel
            // 
            this.MetadataModuleAuthorLabel.AutoSize = true;
            this.MetadataModuleAuthorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAuthorLabel.Location = new System.Drawing.Point(89, 176);
            this.MetadataModuleAuthorLabel.Name = "MetadataModuleAuthorLabel";
            this.MetadataModuleAuthorLabel.Size = new System.Drawing.Size(257, 32);
            this.MetadataModuleAuthorLabel.TabIndex = 6;
            this.MetadataModuleAuthorLabel.Text = "Nobody";
            // 
            // HomePageLabel
            // 
            this.HomePageLabel.AutoSize = true;
            this.HomePageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HomePageLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.HomePageLabel.Location = new System.Drawing.Point(3, 208);
            this.HomePageLabel.Name = "HomePageLabel";
            this.HomePageLabel.Size = new System.Drawing.Size(80, 35);
            this.HomePageLabel.TabIndex = 7;
            this.HomePageLabel.Text = "Homepage:";
            // 
            // GitHubLabel
            // 
            this.GitHubLabel.AutoSize = true;
            this.GitHubLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitHubLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.GitHubLabel.Location = new System.Drawing.Point(3, 243);
            this.GitHubLabel.Name = "GitHubLabel";
            this.GitHubLabel.Size = new System.Drawing.Size(80, 43);
            this.GitHubLabel.TabIndex = 10;
            this.GitHubLabel.Text = "GitHub:";
            // 
            // MetadataModuleReleaseStatusLabel
            // 
            this.MetadataModuleReleaseStatusLabel.AutoSize = true;
            this.MetadataModuleReleaseStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleReleaseStatusLabel.Location = new System.Drawing.Point(89, 286);
            this.MetadataModuleReleaseStatusLabel.Name = "MetadataModuleReleaseStatusLabel";
            this.MetadataModuleReleaseStatusLabel.Size = new System.Drawing.Size(257, 25);
            this.MetadataModuleReleaseStatusLabel.TabIndex = 11;
            this.MetadataModuleReleaseStatusLabel.Text = "Stable";
            // 
            // ReleaseLabel
            // 
            this.ReleaseLabel.AutoSize = true;
            this.ReleaseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReleaseLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ReleaseLabel.Location = new System.Drawing.Point(3, 286);
            this.ReleaseLabel.Name = "ReleaseLabel";
            this.ReleaseLabel.Size = new System.Drawing.Size(80, 25);
            this.ReleaseLabel.TabIndex = 12;
            this.ReleaseLabel.Text = "Release status:";
            // 
            // KSPCompatibilityLabel
            // 
            this.KSPCompatibilityLabel.AutoSize = true;
            this.KSPCompatibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.KSPCompatibilityLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.KSPCompatibilityLabel.Location = new System.Drawing.Point(3, 320);
            this.KSPCompatibilityLabel.Name = "KSPCompatibilityLabel";
            this.KSPCompatibilityLabel.Size = new System.Drawing.Size(80, 33);
            this.KSPCompatibilityLabel.TabIndex = 13;
            this.KSPCompatibilityLabel.Text = "Max KSP ver.:";
            // 
            // MetadataModuleKSPCompatibilityLabel
            // 
            this.MetadataModuleKSPCompatibilityLabel.AutoSize = true;
            this.MetadataModuleKSPCompatibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleKSPCompatibilityLabel.Location = new System.Drawing.Point(89, 354);
            this.MetadataModuleKSPCompatibilityLabel.Name = "MetadataModuleKSPCompatibilityLabel";
            this.MetadataModuleKSPCompatibilityLabel.Size = new System.Drawing.Size(257, 33);
            this.MetadataModuleKSPCompatibilityLabel.TabIndex = 14;
            this.MetadataModuleKSPCompatibilityLabel.Text = "0.0.0";
            // 
            // MetadataModuleHomePageLinkLabel
            // 
            this.MetadataModuleHomePageLinkLabel.AutoEllipsis = true;
            this.MetadataModuleHomePageLinkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleHomePageLinkLabel.Location = new System.Drawing.Point(89, 208);
            this.MetadataModuleHomePageLinkLabel.Name = "MetadataModuleHomePageLinkLabel";
            this.MetadataModuleHomePageLinkLabel.Size = new System.Drawing.Size(257, 35);
            this.MetadataModuleHomePageLinkLabel.TabIndex = 25;
            this.MetadataModuleHomePageLinkLabel.TabStop = true;
            this.MetadataModuleHomePageLinkLabel.Text = "linkLabel1";
            this.MetadataModuleHomePageLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // MetadataModuleGitHubLinkLabel
            // 
            this.MetadataModuleGitHubLinkLabel.AutoEllipsis = true;
            this.MetadataModuleGitHubLinkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleGitHubLinkLabel.Location = new System.Drawing.Point(89, 243);
            this.MetadataModuleGitHubLinkLabel.Name = "MetadataModuleGitHubLinkLabel";
            this.MetadataModuleGitHubLinkLabel.Size = new System.Drawing.Size(257, 43);
            this.MetadataModuleGitHubLinkLabel.TabIndex = 26;
            this.MetadataModuleGitHubLinkLabel.TabStop = true;
            this.MetadataModuleGitHubLinkLabel.Text = "linkLabel2";
            this.MetadataModuleGitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // MetadataModuleNameLabel
            // 
            this.MetadataLayoutPanel.SetColumnSpan(this.MetadataModuleNameLabel, 2);
            this.MetadataModuleNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MetadataModuleNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.MetadataModuleNameLabel.Location = new System.Drawing.Point(3, 0);
            this.MetadataModuleNameLabel.Name = "MetadataModuleNameLabel";
            this.MetadataLayoutPanel.SetRowSpan(this.MetadataModuleNameLabel, 2);
            this.MetadataModuleNameLabel.Size = new System.Drawing.Size(343, 57);
            this.MetadataModuleNameLabel.TabIndex = 0;
            this.MetadataModuleNameLabel.Text = "Mod Name";
            this.MetadataModuleNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MetadataModuleAbstractLabel
            // 
            this.MetadataModuleAbstractLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataLayoutPanel.SetColumnSpan(this.MetadataModuleAbstractLabel, 2);
            this.MetadataModuleAbstractLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAbstractLabel.Location = new System.Drawing.Point(3, 60);
            this.MetadataModuleAbstractLabel.Name = "MetadataModuleAbstractLabel";
            this.MetadataModuleAbstractLabel.ReadOnly = true;
            this.MetadataModuleAbstractLabel.Size = new System.Drawing.Size(343, 48);
            this.MetadataModuleAbstractLabel.TabIndex = 27;
            this.MetadataModuleAbstractLabel.Text = "";
            // 
            // RelationshipTabPage
            // 
            this.RelationshipTabPage.Controls.Add(this.ModuleRelationshipType);
            this.RelationshipTabPage.Controls.Add(this.DependsGraphTree);
            this.RelationshipTabPage.Location = new System.Drawing.Point(4, 25);
            this.RelationshipTabPage.Name = "RelationshipTabPage";
            this.RelationshipTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.RelationshipTabPage.Size = new System.Drawing.Size(352, 549);
            this.RelationshipTabPage.TabIndex = 1;
            this.RelationshipTabPage.Text = "Relationships";
            this.RelationshipTabPage.UseVisualStyleBackColor = true;
            // 
            // ModuleRelationshipType
            // 
            this.ModuleRelationshipType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModuleRelationshipType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ModuleRelationshipType.FormattingEnabled = true;
            this.ModuleRelationshipType.Items.AddRange(new object[] {
            "Depends",
            "Recommends",
            "Suggests",
            "Supports",
            "Conflicts"});
            this.ModuleRelationshipType.Location = new System.Drawing.Point(6, 7);
            this.ModuleRelationshipType.Name = "ModuleRelationshipType";
            this.ModuleRelationshipType.Size = new System.Drawing.Size(340, 21);
            this.ModuleRelationshipType.TabIndex = 1;
            this.ModuleRelationshipType.SelectedIndexChanged += new System.EventHandler(this.ModuleRelationshipType_SelectedIndexChanged);
            // 
            // DependsGraphTree
            // 
            this.DependsGraphTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DependsGraphTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DependsGraphTree.Location = new System.Drawing.Point(3, 34);
            this.DependsGraphTree.Name = "DependsGraphTree";
            this.DependsGraphTree.Size = new System.Drawing.Size(346, 509);
            this.DependsGraphTree.TabIndex = 0;
            this.DependsGraphTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.DependsGraphTree_NodeMouseDoubleClick);
            // 
            // ContentTabPage
            // 
            this.ContentTabPage.Controls.Add(this.ContentsPreviewTree);
            this.ContentTabPage.Controls.Add(this.ContentsDownloadButton);
            this.ContentTabPage.Controls.Add(this.NotCachedLabel);
            this.ContentTabPage.Location = new System.Drawing.Point(4, 25);
            this.ContentTabPage.Name = "ContentTabPage";
            this.ContentTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ContentTabPage.Size = new System.Drawing.Size(352, 549);
            this.ContentTabPage.TabIndex = 2;
            this.ContentTabPage.Text = "Contents";
            this.ContentTabPage.UseVisualStyleBackColor = true;
            // 
            // ContentsPreviewTree
            // 
            this.ContentsPreviewTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ContentsPreviewTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ContentsPreviewTree.Enabled = false;
            this.ContentsPreviewTree.Location = new System.Drawing.Point(0, 65);
            this.ContentsPreviewTree.Name = "ContentsPreviewTree";
            this.ContentsPreviewTree.Size = new System.Drawing.Size(349, 481);
            this.ContentsPreviewTree.TabIndex = 2;
            // 
            // ContentsDownloadButton
            // 
            this.ContentsDownloadButton.Location = new System.Drawing.Point(6, 36);
            this.ContentsDownloadButton.Name = "ContentsDownloadButton";
            this.ContentsDownloadButton.Size = new System.Drawing.Size(103, 23);
            this.ContentsDownloadButton.TabIndex = 1;
            this.ContentsDownloadButton.Text = "Download";
            this.ContentsDownloadButton.UseVisualStyleBackColor = true;
            this.ContentsDownloadButton.Click += new System.EventHandler(this.ContentsDownloadButton_Click);
            // 
            // NotCachedLabel
            // 
            this.NotCachedLabel.Location = new System.Drawing.Point(3, 3);
            this.NotCachedLabel.Name = "NotCachedLabel";
            this.NotCachedLabel.Size = new System.Drawing.Size(216, 30);
            this.NotCachedLabel.TabIndex = 0;
            this.NotCachedLabel.Text = "This mod is not in the cache, click \'Download\' to preview contents";
            // 
            // StatusPanel
            // 
            this.StatusPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StatusPanel.Controls.Add(this.StatusLabel);
            this.StatusPanel.Location = new System.Drawing.Point(0, 698);
            this.StatusPanel.Name = "StatusPanel";
            this.StatusPanel.Size = new System.Drawing.Size(797, 19);
            this.StatusPanel.TabIndex = 8;
            // 
            // StatusLabel
            // 
            this.StatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StatusLabel.Location = new System.Drawing.Point(0, 0);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(797, 19);
            this.StatusLabel.TabIndex = 0;
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.ManageModsTabPage);
            this.MainTabControl.Controls.Add(this.ChangesetTabPage);
            this.MainTabControl.Controls.Add(this.WaitTabPage);
            this.MainTabControl.Controls.Add(this.ChooseRecommendedModsTabPage);
            this.MainTabControl.Controls.Add(this.ChooseProvidedModsTabPage);
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.Location = new System.Drawing.Point(0, 24);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(1029, 672);
            this.MainTabControl.TabIndex = 9;
            // 
            // ManageModsTabPage
            // 
            this.ManageModsTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.ManageModsTabPage.Controls.Add(this.FilterByAuthorTextBox);
            this.ManageModsTabPage.Controls.Add(this.FilterByAuthorLabel);
            this.ManageModsTabPage.Controls.Add(this.KSPVersionLabel);
            this.ManageModsTabPage.Controls.Add(this.FilterByNameLabel);
            this.ManageModsTabPage.Controls.Add(this.FilterByNameTextBox);
            this.ManageModsTabPage.Controls.Add(this.menuStrip2);
            this.ManageModsTabPage.Controls.Add(this.splitContainer1);
            this.ManageModsTabPage.Location = new System.Drawing.Point(4, 22);
            this.ManageModsTabPage.Name = "ManageModsTabPage";
            this.ManageModsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ManageModsTabPage.Size = new System.Drawing.Size(1021, 646);
            this.ManageModsTabPage.TabIndex = 0;
            this.ManageModsTabPage.Text = "Manage mods";
            // 
            // FilterByAuthorTextBox
            // 
            this.FilterByAuthorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByAuthorTextBox.Location = new System.Drawing.Point(445, 48);
            this.FilterByAuthorTextBox.Name = "FilterByAuthorTextBox";
            this.FilterByAuthorTextBox.Size = new System.Drawing.Size(124, 20);
            this.FilterByAuthorTextBox.TabIndex = 10;
            this.FilterByAuthorTextBox.TextChanged += new System.EventHandler(this.FilterByAuthorTextBox_TextChanged);
            // 
            // FilterByAuthorLabel
            // 
            this.FilterByAuthorLabel.AutoSize = true;
            this.FilterByAuthorLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByAuthorLabel.Location = new System.Drawing.Point(331, 50);
            this.FilterByAuthorLabel.Name = "FilterByAuthorLabel";
            this.FilterByAuthorLabel.Size = new System.Drawing.Size(108, 13);
            this.FilterByAuthorLabel.TabIndex = 11;
            this.FilterByAuthorLabel.Text = "Filter by author name:";
            // 
            // KSPVersionLabel
            // 
            this.KSPVersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.KSPVersionLabel.AutoSize = true;
            this.KSPVersionLabel.Location = new System.Drawing.Point(862, 50);
            this.KSPVersionLabel.Name = "KSPVersionLabel";
            this.KSPVersionLabel.Size = new System.Drawing.Size(146, 13);
            this.KSPVersionLabel.TabIndex = 8;
            this.KSPVersionLabel.Text = "Kerbal Space Program 0.25.0";
            // 
            // FilterByNameLabel
            // 
            this.FilterByNameLabel.AutoSize = true;
            this.FilterByNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByNameLabel.Location = new System.Drawing.Point(7, 50);
            this.FilterByNameLabel.Name = "FilterByNameLabel";
            this.FilterByNameLabel.Size = new System.Drawing.Size(98, 13);
            this.FilterByNameLabel.TabIndex = 10;
            this.FilterByNameLabel.Text = "Filter by mod name:";
            // 
            // FilterByNameTextBox
            // 
            this.FilterByNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByNameTextBox.Location = new System.Drawing.Point(111, 48);
            this.FilterByNameTextBox.Name = "FilterByNameTextBox";
            this.FilterByNameTextBox.Size = new System.Drawing.Size(124, 20);
            this.FilterByNameTextBox.TabIndex = 9;
            this.FilterByNameTextBox.TextChanged += new System.EventHandler(this.FilterByNameTextBox_TextChanged);
            // 
            // ChangesetTabPage
            // 
            this.ChangesetTabPage.Controls.Add(this.CancelChangesButton);
            this.ChangesetTabPage.Controls.Add(this.ConfirmChangesButton);
            this.ChangesetTabPage.Controls.Add(this.ChangesListView);
            this.ChangesetTabPage.Location = new System.Drawing.Point(4, 22);
            this.ChangesetTabPage.Name = "ChangesetTabPage";
            this.ChangesetTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ChangesetTabPage.Size = new System.Drawing.Size(1021, 646);
            this.ChangesetTabPage.TabIndex = 2;
            this.ChangesetTabPage.Text = "Changeset";
            this.ChangesetTabPage.UseVisualStyleBackColor = true;
            // 
            // CancelChangesButton
            // 
            this.CancelChangesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelChangesButton.Location = new System.Drawing.Point(859, 617);
            this.CancelChangesButton.Name = "CancelChangesButton";
            this.CancelChangesButton.Size = new System.Drawing.Size(75, 23);
            this.CancelChangesButton.TabIndex = 6;
            this.CancelChangesButton.Text = "Clear";
            this.CancelChangesButton.UseVisualStyleBackColor = true;
            this.CancelChangesButton.Click += new System.EventHandler(this.CancelChangesButton_Click);
            // 
            // ConfirmChangesButton
            // 
            this.ConfirmChangesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ConfirmChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConfirmChangesButton.Location = new System.Drawing.Point(940, 617);
            this.ConfirmChangesButton.Name = "ConfirmChangesButton";
            this.ConfirmChangesButton.Size = new System.Drawing.Size(75, 23);
            this.ConfirmChangesButton.TabIndex = 5;
            this.ConfirmChangesButton.Text = " Apply";
            this.ConfirmChangesButton.UseVisualStyleBackColor = true;
            this.ConfirmChangesButton.Click += new System.EventHandler(this.ConfirmChangesButton_Click);
            // 
            // ChangesListView
            // 
            this.ChangesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ChangesListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ChangesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.ChangeType,
            this.columnHeader2});
            this.ChangesListView.Location = new System.Drawing.Point(-1, 0);
            this.ChangesListView.Name = "ChangesListView";
            this.ChangesListView.Size = new System.Drawing.Size(1022, 611);
            this.ChangesListView.TabIndex = 4;
            this.ChangesListView.UseCompatibleStateImageBehavior = false;
            this.ChangesListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Mod";
            this.columnHeader1.Width = 332;
            // 
            // ChangeType
            // 
            this.ChangeType.Text = "Change";
            this.ChangeType.Width = 111;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Description";
            this.columnHeader2.Width = 606;
            // 
            // WaitTabPage
            // 
            this.WaitTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.WaitTabPage.Controls.Add(this.CancelCurrentActionButton);
            this.WaitTabPage.Controls.Add(this.LogTextBox);
            this.WaitTabPage.Controls.Add(this.DialogProgressBar);
            this.WaitTabPage.Controls.Add(this.MessageTextBox);
            this.WaitTabPage.Location = new System.Drawing.Point(4, 22);
            this.WaitTabPage.Name = "WaitTabPage";
            this.WaitTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.WaitTabPage.Size = new System.Drawing.Size(1021, 646);
            this.WaitTabPage.TabIndex = 1;
            this.WaitTabPage.Text = "Installing mods";
            // 
            // CancelCurrentActionButton
            // 
            this.CancelCurrentActionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelCurrentActionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelCurrentActionButton.Location = new System.Drawing.Point(940, 618);
            this.CancelCurrentActionButton.Name = "CancelCurrentActionButton";
            this.CancelCurrentActionButton.Size = new System.Drawing.Size(75, 23);
            this.CancelCurrentActionButton.TabIndex = 9;
            this.CancelCurrentActionButton.Text = "Cancel";
            this.CancelCurrentActionButton.UseVisualStyleBackColor = true;
            this.CancelCurrentActionButton.Click += new System.EventHandler(this.CancelCurrentActionButton_Click);
            // 
            // LogTextBox
            // 
            this.LogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LogTextBox.Location = new System.Drawing.Point(9, 58);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(1004, 554);
            this.LogTextBox.TabIndex = 8;
            // 
            // DialogProgressBar
            // 
            this.DialogProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DialogProgressBar.Location = new System.Drawing.Point(9, 29);
            this.DialogProgressBar.Name = "DialogProgressBar";
            this.DialogProgressBar.Size = new System.Drawing.Size(1004, 23);
            this.DialogProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.DialogProgressBar.TabIndex = 7;
            // 
            // MessageTextBox
            // 
            this.MessageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MessageTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.MessageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MessageTextBox.Enabled = false;
            this.MessageTextBox.Location = new System.Drawing.Point(8, 6);
            this.MessageTextBox.Multiline = true;
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.ReadOnly = true;
            this.MessageTextBox.Size = new System.Drawing.Size(1007, 17);
            this.MessageTextBox.TabIndex = 6;
            this.MessageTextBox.Text = "Waiting for operation to complete";
            this.MessageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ChooseRecommendedModsTabPage
            // 
            this.ChooseRecommendedModsTabPage.Controls.Add(this.RecommendedModsCancelButton);
            this.ChooseRecommendedModsTabPage.Controls.Add(this.RecommendedModsContinueButton);
            this.ChooseRecommendedModsTabPage.Controls.Add(this.RecommendedDialogLabel);
            this.ChooseRecommendedModsTabPage.Controls.Add(this.RecommendedModsListView);
            this.ChooseRecommendedModsTabPage.Location = new System.Drawing.Point(4, 22);
            this.ChooseRecommendedModsTabPage.Name = "ChooseRecommendedModsTabPage";
            this.ChooseRecommendedModsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ChooseRecommendedModsTabPage.Size = new System.Drawing.Size(1021, 646);
            this.ChooseRecommendedModsTabPage.TabIndex = 3;
            this.ChooseRecommendedModsTabPage.Text = "Choose recommended mods";
            this.ChooseRecommendedModsTabPage.UseVisualStyleBackColor = true;
            // 
            // RecommendedModsCancelButton
            // 
            this.RecommendedModsCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RecommendedModsCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RecommendedModsCancelButton.Location = new System.Drawing.Point(859, 617);
            this.RecommendedModsCancelButton.Name = "RecommendedModsCancelButton";
            this.RecommendedModsCancelButton.Size = new System.Drawing.Size(75, 23);
            this.RecommendedModsCancelButton.TabIndex = 8;
            this.RecommendedModsCancelButton.Text = "Cancel";
            this.RecommendedModsCancelButton.UseVisualStyleBackColor = true;
            this.RecommendedModsCancelButton.Click += new System.EventHandler(this.RecommendedModsCancelButton_Click);
            // 
            // RecommendedModsContinueButton
            // 
            this.RecommendedModsContinueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RecommendedModsContinueButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RecommendedModsContinueButton.Location = new System.Drawing.Point(940, 617);
            this.RecommendedModsContinueButton.Name = "RecommendedModsContinueButton";
            this.RecommendedModsContinueButton.Size = new System.Drawing.Size(75, 23);
            this.RecommendedModsContinueButton.TabIndex = 7;
            this.RecommendedModsContinueButton.Text = "Continue";
            this.RecommendedModsContinueButton.UseVisualStyleBackColor = true;
            this.RecommendedModsContinueButton.Click += new System.EventHandler(this.RecommendedModsContinueButton_Click);
            // 
            // RecommendedDialogLabel
            // 
            this.RecommendedDialogLabel.AutoSize = true;
            this.RecommendedDialogLabel.Location = new System.Drawing.Point(3, 13);
            this.RecommendedDialogLabel.Name = "RecommendedDialogLabel";
            this.RecommendedDialogLabel.Size = new System.Drawing.Size(422, 13);
            this.RecommendedDialogLabel.TabIndex = 6;
            this.RecommendedDialogLabel.Text = "The following modules have been recommended by one or more of the chosen modules:" +
    "";
            // 
            // RecommendedModsListView
            // 
            this.RecommendedModsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RecommendedModsListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RecommendedModsListView.CheckBoxes = true;
            this.RecommendedModsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.RecommendedModsListView.Location = new System.Drawing.Point(6, 29);
            this.RecommendedModsListView.Name = "RecommendedModsListView";
            this.RecommendedModsListView.Size = new System.Drawing.Size(1007, 582);
            this.RecommendedModsListView.TabIndex = 5;
            this.RecommendedModsListView.UseCompatibleStateImageBehavior = false;
            this.RecommendedModsListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Mod";
            this.columnHeader3.Width = 332;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Recommended by";
            this.columnHeader4.Width = 180;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Mod description";
            this.columnHeader5.Width = 606;
            // 
            // ChooseProvidedModsTabPage
            // 
            this.ChooseProvidedModsTabPage.Controls.Add(this.ChooseProvidedModsCancelButton);
            this.ChooseProvidedModsTabPage.Controls.Add(this.ChooseProvidedModsContinueButton);
            this.ChooseProvidedModsTabPage.Controls.Add(this.ChooseProvidedModsListView);
            this.ChooseProvidedModsTabPage.Controls.Add(this.ChooseProvidedModsLabel);
            this.ChooseProvidedModsTabPage.Location = new System.Drawing.Point(4, 22);
            this.ChooseProvidedModsTabPage.Name = "ChooseProvidedModsTabPage";
            this.ChooseProvidedModsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ChooseProvidedModsTabPage.Size = new System.Drawing.Size(1021, 646);
            this.ChooseProvidedModsTabPage.TabIndex = 4;
            this.ChooseProvidedModsTabPage.Text = "Choose mods";
            this.ChooseProvidedModsTabPage.UseVisualStyleBackColor = true;
            // 
            // ChooseProvidedModsCancelButton
            // 
            this.ChooseProvidedModsCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ChooseProvidedModsCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChooseProvidedModsCancelButton.Location = new System.Drawing.Point(857, 616);
            this.ChooseProvidedModsCancelButton.Name = "ChooseProvidedModsCancelButton";
            this.ChooseProvidedModsCancelButton.Size = new System.Drawing.Size(75, 23);
            this.ChooseProvidedModsCancelButton.TabIndex = 10;
            this.ChooseProvidedModsCancelButton.Text = "Cancel";
            this.ChooseProvidedModsCancelButton.UseVisualStyleBackColor = true;
            this.ChooseProvidedModsCancelButton.Click += new System.EventHandler(this.ChooseProvidedModsCancelButton_Click);
            // 
            // ChooseProvidedModsContinueButton
            // 
            this.ChooseProvidedModsContinueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ChooseProvidedModsContinueButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChooseProvidedModsContinueButton.Location = new System.Drawing.Point(938, 616);
            this.ChooseProvidedModsContinueButton.Name = "ChooseProvidedModsContinueButton";
            this.ChooseProvidedModsContinueButton.Size = new System.Drawing.Size(75, 23);
            this.ChooseProvidedModsContinueButton.TabIndex = 9;
            this.ChooseProvidedModsContinueButton.Text = "Continue";
            this.ChooseProvidedModsContinueButton.UseVisualStyleBackColor = true;
            this.ChooseProvidedModsContinueButton.Click += new System.EventHandler(this.ChooseProvidedModsContinueButton_Click);
            // 
            // ChooseProvidedModsListView
            // 
            this.ChooseProvidedModsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ChooseProvidedModsListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ChooseProvidedModsListView.CheckBoxes = true;
            this.ChooseProvidedModsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6,
            this.columnHeader8});
            this.ChooseProvidedModsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ChooseProvidedModsListView.Location = new System.Drawing.Point(6, 28);
            this.ChooseProvidedModsListView.MultiSelect = false;
            this.ChooseProvidedModsListView.Name = "ChooseProvidedModsListView";
            this.ChooseProvidedModsListView.Size = new System.Drawing.Size(1007, 582);
            this.ChooseProvidedModsListView.TabIndex = 8;
            this.ChooseProvidedModsListView.UseCompatibleStateImageBehavior = false;
            this.ChooseProvidedModsListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Mod";
            this.columnHeader6.Width = 332;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Mod description";
            this.columnHeader8.Width = 606;
            // 
            // ChooseProvidedModsLabel
            // 
            this.ChooseProvidedModsLabel.AutoSize = true;
            this.ChooseProvidedModsLabel.Location = new System.Drawing.Point(6, 12);
            this.ChooseProvidedModsLabel.Name = "ChooseProvidedModsLabel";
            this.ChooseProvidedModsLabel.Size = new System.Drawing.Size(383, 13);
            this.ChooseProvidedModsLabel.TabIndex = 7;
            this.ChooseProvidedModsLabel.Text = "Several mods provide the virtual module Foo, choose one of the following mods:";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1029, 718);
            this.Controls.Add(this.MainTabControl);
            this.Controls.Add(this.StatusPanel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(878, 664);
            this.Name = "Main";
            this.Text = "CKAN-GUI";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ModList)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ModInfoTabControl.ResumeLayout(false);
            this.MetadataTabPage.ResumeLayout(false);
            this.MetadataLayoutPanel.ResumeLayout(false);
            this.MetadataLayoutPanel.PerformLayout();
            this.RelationshipTabPage.ResumeLayout(false);
            this.ContentTabPage.ResumeLayout(false);
            this.StatusPanel.ResumeLayout(false);
            this.MainTabControl.ResumeLayout(false);
            this.ManageModsTabPage.ResumeLayout(false);
            this.ManageModsTabPage.PerformLayout();
            this.ChangesetTabPage.ResumeLayout(false);
            this.WaitTabPage.ResumeLayout(false);
            this.WaitTabPage.PerformLayout();
            this.ChooseRecommendedModsTabPage.ResumeLayout(false);
            this.ChooseRecommendedModsTabPage.PerformLayout();
            this.ChooseProvidedModsTabPage.ResumeLayout(false);
            this.ChooseProvidedModsTabPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExitToolButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem RefreshToolButton;
        private System.Windows.Forms.ToolStripMenuItem UpdateAllToolButton;
        private System.Windows.Forms.ToolStripMenuItem ApplyToolButton;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private SplitContainer splitContainer1;
        private Panel StatusPanel;
        private Label StatusLabel;
        private ToolStripMenuItem FilterToolButton;
        private ToolStripMenuItem FilterCompatibleButton;
        private ToolStripMenuItem FilterInstalledButton;
        private ToolStripMenuItem FilterInstalledUpdateButton;
        private ToolStripMenuItem FilterNewButton;
        private ToolStripMenuItem FilterNotInstalledButton;
        private ToolStripMenuItem FilterIncompatibleButton;
        private TabControl ModInfoTabControl;
        private TabPage RelationshipTabPage;
        private TreeView DependsGraphTree;
        private TabPage ContentTabPage;
        private Label NotCachedLabel;
        private TreeView ContentsPreviewTree;
        private Button ContentsDownloadButton;
        private TabPage MetadataTabPage;
        private TableLayoutPanel MetadataLayoutPanel;
        private Label VersionLabel;
        private Label MetadataModuleVersionLabel;
        private Label LicenseLabel;
        private Label MetadataModuleLicenseLabel;
        private Label AuthorLabel;
        private Label MetadataModuleAuthorLabel;
        private Label HomePageLabel;
        private Label GitHubLabel;
        private Label MetadataModuleReleaseStatusLabel;
        private Label ReleaseLabel;
        private Label KSPCompatibilityLabel;
        private Label MetadataModuleKSPCompatibilityLabel;
        private LinkLabel MetadataModuleHomePageLinkLabel;
        private LinkLabel MetadataModuleGitHubLinkLabel;
        private Label MetadataModuleNameLabel;
        private ComboBox ModuleRelationshipType;
        private ToolStripMenuItem launchKSPToolStripMenuItem;
        private TabControl MainTabControl;
        private TabPage ManageModsTabPage;
        private Label KSPVersionLabel;
        private Label FilterByNameLabel;
        private TextBox FilterByNameTextBox;
        private TabPage WaitTabPage;
        private Button CancelCurrentActionButton;
        private TextBox LogTextBox;
        private ProgressBar DialogProgressBar;
        private TextBox MessageTextBox;
        private DataGridViewCheckBoxColumn UpdateCol;
        private TabPage ChangesetTabPage;
        private Button CancelChangesButton;
        private Button ConfirmChangesButton;
        private ListView ChangesListView;
        private ColumnHeader columnHeader1;
        private ColumnHeader ChangeType;
        private ColumnHeader columnHeader2;
        private TabPage ChooseRecommendedModsTabPage;
        private Label RecommendedDialogLabel;
        private ListView RecommendedModsListView;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader5;
        private Button RecommendedModsCancelButton;
        private Button RecommendedModsContinueButton;
        private TabPage ChooseProvidedModsTabPage;
        private Label ChooseProvidedModsLabel;
        private ListView ChooseProvidedModsListView;
        private ColumnHeader columnHeader6;
        private ColumnHeader columnHeader8;
        private Button ChooseProvidedModsCancelButton;
        private Button ChooseProvidedModsContinueButton;
        private ToolStripMenuItem cKANSettingsToolStripMenuItem;
        private ToolStripMenuItem kSPCommandlineToolStripMenuItem;
        private RichTextBox MetadataModuleAbstractLabel;
        private DataGridViewCheckBoxColumn Installed;
        private DataGridViewTextBoxColumn ModName;
        private DataGridViewTextBoxColumn Author;
        private DataGridViewTextBoxColumn InstalledVersion;
        private DataGridViewTextBoxColumn KSPCompatibility;
        private DataGridViewTextBoxColumn LatestVersion;
        private DataGridViewTextBoxColumn Description;
        private ToolStripMenuItem pluginsToolStripMenuItem;
        public ToolStripMenuItem settingsToolStripMenuItem;
        public DataGridView ModList;
        private ToolStripMenuItem installFromckanToolStripMenuItem;
        private TextBox FilterByAuthorTextBox;
        private Label FilterByAuthorLabel;
        private ToolStripMenuItem FilterAllButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exportModListToolStripMenuItem;
        private ToolStripMenuItem selectKSPInstallMenuItem;
        private ToolStripMenuItem openKspDirectoyToolStripMenuItem;
    }
}

