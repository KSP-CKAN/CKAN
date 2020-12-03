namespace CKAN
{
    partial class ManageMods
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(ManageMods));
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.launchGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.EditModSearch = new CKAN.EditModSearch();
            this.ModGrid = new System.Windows.Forms.DataGridView();
            this.InstallAllCheckbox = new System.Windows.Forms.CheckBox();
            this.Installed = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.AutoInstalled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.UpdateCol = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ReplaceCol = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ModName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LatestVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GameCompatibility = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SizeCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReleaseDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.menuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ModGrid)).BeginInit();
            this.ModListContextMenuStrip.SuspendLayout();
            this.ModListHeaderContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // menuStrip2
            //
            this.menuStrip2.AutoSize = false;
            this.menuStrip2.Dock = System.Windows.Forms.DockStyle.Top;
            this.menuStrip2.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchGameToolStripMenuItem,
            this.RefreshToolButton,
            this.UpdateAllToolButton,
            this.ApplyToolButton,
            this.FilterToolButton,
            this.NavBackwardToolButton,
            this.NavForwardToolButton});
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(5876, 48);
            this.menuStrip2.TabIndex = 4;
            this.menuStrip2.Text = "menuStrip2";
            //
            // launchGameToolStripMenuItem
            //
            this.launchGameToolStripMenuItem.Image = global::CKAN.Properties.Resources.ksp;
            this.launchGameToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.launchGameToolStripMenuItem.Name = "launchGameToolStripMenuItem";
            this.launchGameToolStripMenuItem.Size = new System.Drawing.Size(146, 56);
            this.launchGameToolStripMenuItem.Click += new System.EventHandler(this.launchGameToolStripMenuItem_Click);
            resources.ApplyResources(this.launchGameToolStripMenuItem, "launchGameToolStripMenuItem");
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
            // EditModSearch
            //
            this.EditModSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.EditModSearch.ApplySearch += EditModSearch_ApplySearch;
            this.EditModSearch.SurrenderFocus += EditModSearch_SurrenderFocus;
            //
            // ModGrid
            //
            this.ModGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModGrid.AllowUserToAddRows = false;
            this.ModGrid.AllowUserToDeleteRows = false;
            this.ModGrid.AllowUserToResizeRows = false;
            this.ModGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.ModGrid.EnableHeadersVisualStyles = false;
            this.ModGrid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
            this.ModGrid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ModGrid.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ModGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.ModGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.ModGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ModGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Installed,
            this.AutoInstalled,
            this.UpdateCol,
            this.ReplaceCol,
            this.ModName,
            this.Author,
            this.InstalledVersion,
            this.LatestVersion,
            this.GameCompatibility,
            this.SizeCol,
            this.ReleaseDate,
            this.InstallDate,
            this.DownloadCount,
            this.Description});
            this.ModGrid.Location = new System.Drawing.Point(0, 111);
            this.ModGrid.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ModGrid.MultiSelect = false;
            this.ModGrid.Name = "ModGrid";
            this.ModGrid.RowHeadersVisible = false;
            this.ModGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ModGrid.Size = new System.Drawing.Size(1536, 837);
            this.ModGrid.StandardTab = true;
            this.ModGrid.TabIndex = 12;
            this.ModGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ModList_CellContentClick);
            this.ModGrid.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ModList_CellMouseDoubleClick);
            this.ModGrid.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ModList_HeaderMouseClick);
            this.ModGrid.SelectionChanged += new System.EventHandler(this.ModList_SelectedIndexChanged);
            this.ModGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ModList_KeyDown);
            this.ModGrid.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ModList_KeyPress);
            this.ModGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ModList_MouseDown);
            this.ModGrid.GotFocus += new System.EventHandler(this.ModList_GotFocus);
            this.ModGrid.LostFocus += new System.EventHandler(this.ModList_LostFocus);
            this.ModGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.ModList_CurrentCellDirtyStateChanged);
            this.ModGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.ModList_CellValueChanged);
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
            // GameCompatibility
            //
            this.GameCompatibility.Name = "GameCompatibility";
            this.GameCompatibility.ReadOnly = true;
            this.GameCompatibility.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.GameCompatibility.Width = 78;
            resources.ApplyResources(this.GameCompatibility, "GameCompatibility");
            //
            // SizeCol
            //
            this.SizeCol.Name = "SizeCol";
            this.SizeCol.ReadOnly = true;
            this.SizeCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.SizeCol.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            resources.ApplyResources(this.SizeCol, "SizeCol");
            //
            // ReleaseDate
            //
            this.ReleaseDate.Name = "ReleaseDate";
            this.ReleaseDate.ReadOnly = true;
            this.ReleaseDate.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.ReleaseDate.Width = 140;
            resources.ApplyResources(this.ReleaseDate, "ReleaseDate");
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
            // InstallAllCheckbox
            //
            this.InstallAllCheckbox.Location = new System.Drawing.Point(2, 86);
            this.InstallAllCheckbox.Size = new System.Drawing.Size(20, 20);
            this.InstallAllCheckbox.Checked = true;
            this.InstallAllCheckbox.CheckedChanged += new System.EventHandler(this.InstallAllCheckbox_CheckChanged);
            this.InstallAllCheckbox.TabIndex = 11;
            this.InstallAllCheckbox.TabStop = false;
            //
            // ManageMods
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.InstallAllCheckbox);
            this.Controls.Add(this.ModGrid);
            this.Controls.Add(this.EditModSearch);
            this.Controls.Add(this.menuStrip2);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ManageMods";
            this.Size = new System.Drawing.Size(1544, 948);
            resources.ApplyResources(this, "$this");
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ModGrid)).EndInit();
            this.ModListContextMenuStrip.ResumeLayout(false);
            this.ModListHeaderContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem launchGameToolStripMenuItem;
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
        public System.Windows.Forms.DataGridView ModGrid;
        private System.Windows.Forms.CheckBox InstallAllCheckbox;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Installed;
        private System.Windows.Forms.DataGridViewCheckBoxColumn AutoInstalled;
        private System.Windows.Forms.DataGridViewCheckBoxColumn UpdateCol;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ReplaceCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn ModName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Author;
        private System.Windows.Forms.DataGridViewTextBoxColumn InstalledVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn LatestVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn GameCompatibility;
        private System.Windows.Forms.DataGridViewTextBoxColumn SizeCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn ReleaseDate;
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
        private CKAN.EditModSearch EditModSearch;
    }
}
