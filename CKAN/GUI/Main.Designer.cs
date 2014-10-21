using System;
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.RefreshToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.UpdateAllToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ApplyToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterAllButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterInstalledButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterInstalledUpdateButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterNewButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterNotInstalledButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterIncompatibleButton = new System.Windows.Forms.ToolStripMenuItem();
            this.customToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ModList = new System.Windows.Forms.DataGridView();
            this.Installed = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Update = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ModName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LatestVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.KSPVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Homepage = new System.Windows.Forms.DataGridViewLinkColumn();
            this.FilterByNameTextBox = new System.Windows.Forms.TextBox();
            this.FilterByNameLabel = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.MetadataModuleAbstractLabel = new System.Windows.Forms.TextBox();
            this.MetadataModuleNameLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.MetadataModuleVersionLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.MetadataModuleLicenseLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.MetadataModuleAuthorLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.MetadataModuleReleaseStatusLabel = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.MetadataModuleHomePageLinkLabel = new System.Windows.Forms.LinkLabel();
            this.MetadataModuleGitHubLinkLabel = new System.Windows.Forms.LinkLabel();
            this.RelationshipTabPage = new System.Windows.Forms.TabPage();
            this.DependsGraphTree = new System.Windows.Forms.TreeView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.ContentsPreviewTree = new System.Windows.Forms.TreeView();
            this.ContentsDownloadButton = new System.Windows.Forms.Button();
            this.NotCachedLabel = new System.Windows.Forms.Label();
            this.StatusPanel = new System.Windows.Forms.Panel();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.KSPVersionLabel = new System.Windows.Forms.Label();
            this.ModuleRelationshipType = new System.Windows.Forms.ComboBox();
            this.menuStrip1.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ModList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.RelationshipTabPage.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.StatusPanel.SuspendLayout();
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
            this.menuStrip1.Size = new System.Drawing.Size(870, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExitToolButton});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // ExitToolButton
            // 
            this.ExitToolButton.Name = "ExitToolButton";
            this.ExitToolButton.Size = new System.Drawing.Size(92, 22);
            this.ExitToolButton.Text = "Exit";
            this.ExitToolButton.Click += new System.EventHandler(this.ExitToolButton_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 615);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(870, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // menuStrip2
            // 
            this.menuStrip2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.menuStrip2.AutoSize = false;
            this.menuStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RefreshToolButton,
            this.UpdateAllToolButton,
            this.ApplyToolButton,
            this.FilterToolButton});
            this.menuStrip2.Location = new System.Drawing.Point(0, 24);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(870, 40);
            this.menuStrip2.TabIndex = 2;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // RefreshToolButton
            // 
            this.RefreshToolButton.Image = global::CKAN.Properties.Resources.refresh;
            this.RefreshToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.RefreshToolButton.Name = "RefreshToolButton";
            this.RefreshToolButton.Size = new System.Drawing.Size(89, 36);
            this.RefreshToolButton.Text = "Refresh";
            this.RefreshToolButton.Click += new System.EventHandler(this.RefreshToolButton_Click);
            // 
            // UpdateAllToolButton
            // 
            this.UpdateAllToolButton.Image = global::CKAN.Properties.Resources.update;
            this.UpdateAllToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.UpdateAllToolButton.Name = "UpdateAllToolButton";
            this.UpdateAllToolButton.Size = new System.Drawing.Size(130, 36);
            this.UpdateAllToolButton.Text = "Mark all updated";
            this.UpdateAllToolButton.Click += new System.EventHandler(this.MarkAllUpdatesToolButton_Click);
            // 
            // ApplyToolButton
            // 
            this.ApplyToolButton.Image = global::CKAN.Properties.Resources.apply;
            this.ApplyToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ApplyToolButton.Name = "ApplyToolButton";
            this.ApplyToolButton.Size = new System.Drawing.Size(121, 36);
            this.ApplyToolButton.Text = "Apply changes";
            this.ApplyToolButton.Click += new System.EventHandler(this.ApplyToolButton_Click);
            // 
            // FilterToolButton
            // 
            this.FilterToolButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FilterAllButton,
            this.FilterInstalledButton,
            this.FilterInstalledUpdateButton,
            this.FilterNewButton,
            this.FilterNotInstalledButton,
            this.FilterIncompatibleButton,
            this.customToolStripMenuItem});
            this.FilterToolButton.Image = global::CKAN.Properties.Resources.search;
            this.FilterToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.FilterToolButton.Name = "FilterToolButton";
            this.FilterToolButton.Size = new System.Drawing.Size(97, 36);
            this.FilterToolButton.Text = "Filter (All)";
            // 
            // FilterAllButton
            // 
            this.FilterAllButton.Name = "FilterAllButton";
            this.FilterAllButton.Size = new System.Drawing.Size(205, 22);
            this.FilterAllButton.Text = "All";
            this.FilterAllButton.Click += new System.EventHandler(this.FilterAllButton_Click);
            // 
            // FilterInstalledButton
            // 
            this.FilterInstalledButton.Name = "FilterInstalledButton";
            this.FilterInstalledButton.Size = new System.Drawing.Size(205, 22);
            this.FilterInstalledButton.Text = "Installed";
            this.FilterInstalledButton.Click += new System.EventHandler(this.FilterInstalledButton_Click);
            // 
            // FilterInstalledUpdateButton
            // 
            this.FilterInstalledUpdateButton.Name = "FilterInstalledUpdateButton";
            this.FilterInstalledUpdateButton.Size = new System.Drawing.Size(205, 22);
            this.FilterInstalledUpdateButton.Text = "Installed (update available)";
            this.FilterInstalledUpdateButton.Click += new System.EventHandler(this.FilterInstalledUpdateButton_Click);
            // 
            // FilterNewButton
            // 
            this.FilterNewButton.Name = "FilterNewButton";
            this.FilterNewButton.Size = new System.Drawing.Size(205, 22);
            this.FilterNewButton.Text = "New in repository";
            this.FilterNewButton.Click += new System.EventHandler(this.FilterNewButton_Click);
            // 
            // FilterNotInstalledButton
            // 
            this.FilterNotInstalledButton.Name = "FilterNotInstalledButton";
            this.FilterNotInstalledButton.Size = new System.Drawing.Size(205, 22);
            this.FilterNotInstalledButton.Text = "Not installed";
            this.FilterNotInstalledButton.Click += new System.EventHandler(this.FilterNotInstalledButton_Click);
            // 
            // FilterIncompatibleButton
            // 
            this.FilterIncompatibleButton.Name = "FilterIncompatibleButton";
            this.FilterIncompatibleButton.Size = new System.Drawing.Size(205, 22);
            this.FilterIncompatibleButton.Text = "Incompatible";
            this.FilterIncompatibleButton.Click += new System.EventHandler(this.FilterIncompatibleButton_Click);
            // 
            // customToolStripMenuItem
            // 
            this.customToolStripMenuItem.Name = "customToolStripMenuItem";
            this.customToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.customToolStripMenuItem.Text = "Custom";
            // 
            // ModList
            // 
            this.ModList.AllowUserToAddRows = false;
            this.ModList.AllowUserToDeleteRows = false;
            this.ModList.AllowUserToResizeColumns = false;
            this.ModList.AllowUserToResizeRows = false;
            this.ModList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.ModList.BackgroundColor = System.Drawing.SystemColors.Window;
            this.ModList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.ModList.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.ModList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ModList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Installed,
            this.Update,
            this.ModName,
            this.Author,
            this.InstalledVersion,
            this.LatestVersion,
            this.KSPVersion,
            this.Description,
            this.Homepage});
            this.ModList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModList.Location = new System.Drawing.Point(0, 0);
            this.ModList.MultiSelect = false;
            this.ModList.Name = "ModList";
            this.ModList.RowHeadersVisible = false;
            this.ModList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ModList.Size = new System.Drawing.Size(626, 518);
            this.ModList.TabIndex = 3;
            this.ModList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ModList_CellContentClick);
            this.ModList.SelectionChanged += new System.EventHandler(this.ModList_SelectedIndexChanged);
            // 
            // Installed
            // 
            this.Installed.HeaderText = "Installed";
            this.Installed.Name = "Installed";
            this.Installed.Width = 50;
            // 
            // Update
            // 
            this.Update.HeaderText = "Update";
            this.Update.Name = "Update";
            this.Update.Width = 46;
            // 
            // ModName
            // 
            this.ModName.HeaderText = "Name";
            this.ModName.Name = "ModName";
            this.ModName.ReadOnly = true;
            this.ModName.Width = 58;
            // 
            // Author
            // 
            this.Author.HeaderText = "Author";
            this.Author.Name = "Author";
            this.Author.ReadOnly = true;
            this.Author.Width = 61;
            // 
            // InstalledVersion
            // 
            this.InstalledVersion.HeaderText = "Installed version";
            this.InstalledVersion.Name = "InstalledVersion";
            this.InstalledVersion.ReadOnly = true;
            this.InstalledVersion.Width = 97;
            // 
            // LatestVersion
            // 
            this.LatestVersion.HeaderText = "Latest version";
            this.LatestVersion.Name = "LatestVersion";
            this.LatestVersion.ReadOnly = true;
            this.LatestVersion.Width = 88;
            // 
            // KSPVersion
            // 
            this.KSPVersion.HeaderText = "KSP version";
            this.KSPVersion.Name = "KSPVersion";
            this.KSPVersion.Width = 81;
            // 
            // Description
            // 
            this.Description.HeaderText = "Description";
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.Width = 83;
            // 
            // Homepage
            // 
            this.Homepage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Homepage.HeaderText = "Homepage";
            this.Homepage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.Homepage.Name = "Homepage";
            this.Homepage.ReadOnly = true;
            this.Homepage.Width = 63;
            // 
            // FilterByNameTextBox
            // 
            this.FilterByNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.FilterByNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByNameTextBox.Location = new System.Drawing.Point(83, 591);
            this.FilterByNameTextBox.Name = "FilterByNameTextBox";
            this.FilterByNameTextBox.Size = new System.Drawing.Size(124, 20);
            this.FilterByNameTextBox.TabIndex = 4;
            this.FilterByNameTextBox.TextChanged += new System.EventHandler(this.FilterByNameTextBox_TextChanged);
            // 
            // FilterByNameLabel
            // 
            this.FilterByNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.FilterByNameLabel.AutoSize = true;
            this.FilterByNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByNameLabel.Location = new System.Drawing.Point(2, 594);
            this.FilterByNameLabel.Name = "FilterByNameLabel";
            this.FilterByNameLabel.Size = new System.Drawing.Size(75, 13);
            this.FilterByNameLabel.TabIndex = 5;
            this.FilterByNameLabel.Text = "Filter by name:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(3, 67);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ModList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(863, 518);
            this.splitContainer1.SplitterDistance = 626;
            this.splitContainer1.TabIndex = 7;
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.RelationshipTabPage);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(233, 518);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(225, 489);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Metadata";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.MetadataModuleVersionLabel, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.MetadataModuleLicenseLabel, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.MetadataModuleAuthorLabel, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.MetadataModuleReleaseStatusLabel, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.label10, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.MetadataModuleHomePageLinkLabel, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.MetadataModuleGitHubLinkLabel, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.MetadataModuleNameLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.MetadataModuleAbstractLabel, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 14;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(219, 483);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // MetadataModuleAbstractLabel
            // 
            this.MetadataModuleAbstractLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tableLayoutPanel1.SetColumnSpan(this.MetadataModuleAbstractLabel, 2);
            this.MetadataModuleAbstractLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAbstractLabel.Location = new System.Drawing.Point(3, 43);
            this.MetadataModuleAbstractLabel.Multiline = true;
            this.MetadataModuleAbstractLabel.Name = "MetadataModuleAbstractLabel";
            this.MetadataModuleAbstractLabel.ReadOnly = true;
            this.MetadataModuleAbstractLabel.Size = new System.Drawing.Size(213, 58);
            this.MetadataModuleAbstractLabel.TabIndex = 27;
            this.MetadataModuleAbstractLabel.Text = "Description";
            // 
            // MetadataModuleNameLabel
            // 
            this.MetadataModuleNameLabel.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.MetadataModuleNameLabel, 2);
            this.MetadataModuleNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MetadataModuleNameLabel.Location = new System.Drawing.Point(3, 0);
            this.MetadataModuleNameLabel.Name = "MetadataModuleNameLabel";
            this.tableLayoutPanel1.SetRowSpan(this.MetadataModuleNameLabel, 2);
            this.MetadataModuleNameLabel.Size = new System.Drawing.Size(213, 40);
            this.MetadataModuleNameLabel.TabIndex = 0;
            this.MetadataModuleNameLabel.Text = "Mod Name";
            this.MetadataModuleNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 104);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Version:";
            // 
            // MetadataModuleVersionLabel
            // 
            this.MetadataModuleVersionLabel.AutoSize = true;
            this.MetadataModuleVersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleVersionLabel.Location = new System.Drawing.Point(89, 104);
            this.MetadataModuleVersionLabel.Name = "MetadataModuleVersionLabel";
            this.MetadataModuleVersionLabel.Size = new System.Drawing.Size(127, 32);
            this.MetadataModuleVersionLabel.TabIndex = 2;
            this.MetadataModuleVersionLabel.Text = "0.0.0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 136);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 32);
            this.label2.TabIndex = 3;
            this.label2.Text = "License:";
            // 
            // MetadataModuleLicenseLabel
            // 
            this.MetadataModuleLicenseLabel.AutoSize = true;
            this.MetadataModuleLicenseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleLicenseLabel.Location = new System.Drawing.Point(89, 136);
            this.MetadataModuleLicenseLabel.Name = "MetadataModuleLicenseLabel";
            this.MetadataModuleLicenseLabel.Size = new System.Drawing.Size(127, 32);
            this.MetadataModuleLicenseLabel.TabIndex = 4;
            this.MetadataModuleLicenseLabel.Text = "None";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 168);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 32);
            this.label3.TabIndex = 5;
            this.label3.Text = "Author:";
            // 
            // MetadataModuleAuthorLabel
            // 
            this.MetadataModuleAuthorLabel.AutoSize = true;
            this.MetadataModuleAuthorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAuthorLabel.Location = new System.Drawing.Point(89, 168);
            this.MetadataModuleAuthorLabel.Name = "MetadataModuleAuthorLabel";
            this.MetadataModuleAuthorLabel.Size = new System.Drawing.Size(127, 32);
            this.MetadataModuleAuthorLabel.TabIndex = 6;
            this.MetadataModuleAuthorLabel.Text = "Nobody";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 200);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 32);
            this.label5.TabIndex = 7;
            this.label5.Text = "Homepage:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(3, 232);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 32);
            this.label8.TabIndex = 10;
            this.label8.Text = "GitHub:";
            // 
            // MetadataModuleReleaseStatusLabel
            // 
            this.MetadataModuleReleaseStatusLabel.AutoSize = true;
            this.MetadataModuleReleaseStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleReleaseStatusLabel.Location = new System.Drawing.Point(89, 264);
            this.MetadataModuleReleaseStatusLabel.Name = "MetadataModuleReleaseStatusLabel";
            this.MetadataModuleReleaseStatusLabel.Size = new System.Drawing.Size(127, 32);
            this.MetadataModuleReleaseStatusLabel.TabIndex = 11;
            this.MetadataModuleReleaseStatusLabel.Text = "Stable";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Location = new System.Drawing.Point(3, 264);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(80, 32);
            this.label10.TabIndex = 12;
            this.label10.Text = "Release status:";
            // 
            // MetadataModuleHomePageLinkLabel
            // 
            this.MetadataModuleHomePageLinkLabel.AutoEllipsis = true;
            this.MetadataModuleHomePageLinkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleHomePageLinkLabel.Location = new System.Drawing.Point(89, 200);
            this.MetadataModuleHomePageLinkLabel.Name = "MetadataModuleHomePageLinkLabel";
            this.MetadataModuleHomePageLinkLabel.Size = new System.Drawing.Size(127, 32);
            this.MetadataModuleHomePageLinkLabel.TabIndex = 25;
            this.MetadataModuleHomePageLinkLabel.TabStop = true;
            this.MetadataModuleHomePageLinkLabel.Text = "linkLabel1";
            this.MetadataModuleHomePageLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.MetadataModuleHomePageLinkLabel_LinkClicked);
            // 
            // MetadataModuleGitHubLinkLabel
            // 
            this.MetadataModuleGitHubLinkLabel.AutoEllipsis = true;
            this.MetadataModuleGitHubLinkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleGitHubLinkLabel.Location = new System.Drawing.Point(89, 232);
            this.MetadataModuleGitHubLinkLabel.Name = "MetadataModuleGitHubLinkLabel";
            this.MetadataModuleGitHubLinkLabel.Size = new System.Drawing.Size(127, 32);
            this.MetadataModuleGitHubLinkLabel.TabIndex = 26;
            this.MetadataModuleGitHubLinkLabel.TabStop = true;
            this.MetadataModuleGitHubLinkLabel.Text = "linkLabel2";
            this.MetadataModuleGitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.MetadataModuleGitHubLinkLabel_LinkClicked);
            // 
            // RelationshipTabPage
            // 
            this.RelationshipTabPage.Controls.Add(this.ModuleRelationshipType);
            this.RelationshipTabPage.Controls.Add(this.DependsGraphTree);
            this.RelationshipTabPage.Location = new System.Drawing.Point(4, 25);
            this.RelationshipTabPage.Name = "RelationshipTabPage";
            this.RelationshipTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.RelationshipTabPage.Size = new System.Drawing.Size(225, 489);
            this.RelationshipTabPage.TabIndex = 1;
            this.RelationshipTabPage.Text = "Relationships";
            this.RelationshipTabPage.UseVisualStyleBackColor = true;
            // 
            // DependsGraphTree
            // 
            this.DependsGraphTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DependsGraphTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DependsGraphTree.Location = new System.Drawing.Point(3, 34);
            this.DependsGraphTree.Name = "DependsGraphTree";
            this.DependsGraphTree.Size = new System.Drawing.Size(219, 449);
            this.DependsGraphTree.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.ContentsPreviewTree);
            this.tabPage2.Controls.Add(this.ContentsDownloadButton);
            this.tabPage2.Controls.Add(this.NotCachedLabel);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(225, 489);
            this.tabPage2.TabIndex = 2;
            this.tabPage2.Text = "Contents";
            this.tabPage2.UseVisualStyleBackColor = true;
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
            this.ContentsPreviewTree.Size = new System.Drawing.Size(222, 421);
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
            this.StatusPanel.Location = new System.Drawing.Point(0, 617);
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
            // KSPVersionLabel
            // 
            this.KSPVersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.KSPVersionLabel.AutoSize = true;
            this.KSPVersionLabel.Location = new System.Drawing.Point(763, 594);
            this.KSPVersionLabel.Name = "KSPVersionLabel";
            this.KSPVersionLabel.Size = new System.Drawing.Size(99, 13);
            this.KSPVersionLabel.TabIndex = 1;
            this.KSPVersionLabel.Text = "KSP Version 0.25.0";
            // 
            // ModuleRelationshipType
            // 
            this.ModuleRelationshipType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModuleRelationshipType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ModuleRelationshipType.FormattingEnabled = true;
            this.ModuleRelationshipType.Items.AddRange(new object[] {
            "Depends",
            "Pre-depends",
            "Recommends",
            "Suggests"});
            this.ModuleRelationshipType.Location = new System.Drawing.Point(6, 7);
            this.ModuleRelationshipType.Name = "ModuleRelationshipType";
            this.ModuleRelationshipType.Size = new System.Drawing.Size(213, 21);
            this.ModuleRelationshipType.TabIndex = 1;
            this.ModuleRelationshipType.SelectedIndexChanged += new System.EventHandler(this.ModuleRelationshipType_SelectedIndexChanged);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(870, 637);
            this.Controls.Add(this.KSPVersionLabel);
            this.Controls.Add(this.FilterByNameLabel);
            this.Controls.Add(this.StatusPanel);
            this.Controls.Add(this.FilterByNameTextBox);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.menuStrip2);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(878, 664);
            this.Name = "Main";
            this.Text = "CKAN-GUI";
            this.Load += new System.EventHandler(this.Main_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ModList)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.RelationshipTabPage.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.StatusPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExitToolButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem RefreshToolButton;
        private System.Windows.Forms.ToolStripMenuItem UpdateAllToolButton;
        private System.Windows.Forms.ToolStripMenuItem ApplyToolButton;
        private System.Windows.Forms.TextBox FilterByNameTextBox;
        private System.Windows.Forms.Label FilterByNameLabel;
        private System.Windows.Forms.DataGridView ModList;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private SplitContainer splitContainer1;
        private Panel StatusPanel;
        private Label StatusLabel;
        private DataGridViewCheckBoxColumn Installed;
        private DataGridViewCheckBoxColumn Update;
        private DataGridViewTextBoxColumn ModName;
        private DataGridViewTextBoxColumn Author;
        private DataGridViewTextBoxColumn InstalledVersion;
        private DataGridViewTextBoxColumn LatestVersion;
        private DataGridViewTextBoxColumn KSPVersion;
        private DataGridViewTextBoxColumn Description;
        private DataGridViewLinkColumn Homepage;
        private ToolStripMenuItem FilterToolButton;
        private ToolStripMenuItem FilterAllButton;
        private ToolStripMenuItem FilterInstalledButton;
        private ToolStripMenuItem FilterInstalledUpdateButton;
        private ToolStripMenuItem FilterNewButton;
        private ToolStripMenuItem FilterNotInstalledButton;
        private ToolStripMenuItem FilterIncompatibleButton;
        private TabControl tabControl1;
        private TabPage RelationshipTabPage;
        private TreeView DependsGraphTree;
        private TabPage tabPage2;
        private Label NotCachedLabel;
        private TreeView ContentsPreviewTree;
        private Button ContentsDownloadButton;
        private ToolStripMenuItem customToolStripMenuItem;
        private TabPage tabPage1;
        private Label KSPVersionLabel;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private Label MetadataModuleVersionLabel;
        private Label label2;
        private Label MetadataModuleLicenseLabel;
        private Label label3;
        private Label MetadataModuleAuthorLabel;
        private Label label5;
        private Label label8;
        private Label MetadataModuleReleaseStatusLabel;
        private Label label10;
        private LinkLabel MetadataModuleHomePageLinkLabel;
        private LinkLabel MetadataModuleGitHubLinkLabel;
        private Label MetadataModuleNameLabel;
        private TextBox MetadataModuleAbstractLabel;
        private ComboBox ModuleRelationshipType;
    }
}

