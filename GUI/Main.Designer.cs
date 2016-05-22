﻿using System.Windows.Forms;

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
            this.openKspDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.ckanModListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.exportCurrentSetToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.ExitToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cKANSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kSPCommandlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusPanel = new System.Windows.Forms.Panel();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.MainTabControl = new CKAN.MainTabControl();
            this.ManageModsTabPage = new System.Windows.Forms.TabPage();
            this.FilterByAuthorTextBox = new System.Windows.Forms.TextBox();
            this.FilterByAuthorLabel = new System.Windows.Forms.Label();
            this.KSPVersionLabel = new System.Windows.Forms.Label();
            this.FilterByNameLabel = new System.Windows.Forms.Label();
            this.FilterByNameTextBox = new System.Windows.Forms.TextBox();
            this.FilterByDescriptionLabel = new System.Windows.Forms.Label();
            this.FilterByDescriptionTextBox = new System.Windows.Forms.TextBox();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.launchKSPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RefreshToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.UpdateAllToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ApplyToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterCompatibleButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterInstalledButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterInstalledUpdateButton = new System.Windows.Forms.ToolStripMenuItem();
            this.cachedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterNewButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterNotInstalledButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterIncompatibleButton = new System.Windows.Forms.ToolStripMenuItem();
            this.FilterAllButton = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ModList = new CKAN.MainModListGUI();
            this.Installed = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Update = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ModName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LatestVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.KSPCompatibility = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SizeCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ModInfoTabControl = new System.Windows.Forms.TabControl();
            this.MetadataTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.MetaDataUpperLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.MetadataModuleNameLabel = new System.Windows.Forms.Label();
            this.MetadataModuleAbstractLabel = new System.Windows.Forms.RichTextBox();
            this.MetaDataLowerLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.IdentifierLabel = new System.Windows.Forms.Label();
            this.MetadataIdentifierLabel = new System.Windows.Forms.Label();
            this.KSPCompatibilityLabel = new System.Windows.Forms.Label();
            this.ReleaseLabel = new System.Windows.Forms.Label();
            this.GitHubLabel = new System.Windows.Forms.Label();
            this.HomePageLabel = new System.Windows.Forms.Label();
            this.AuthorLabel = new System.Windows.Forms.Label();
            this.LicenseLabel = new System.Windows.Forms.Label();
            this.MetadataModuleVersionLabel = new System.Windows.Forms.Label();
            this.MetadataModuleLicenseLabel = new System.Windows.Forms.Label();
            this.MetadataModuleAuthorLabel = new System.Windows.Forms.Label();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.MetadataModuleReleaseStatusLabel = new System.Windows.Forms.Label();
            this.MetadataModuleHomePageLinkLabel = new System.Windows.Forms.LinkLabel();
            this.MetadataModuleKSPCompatibilityLabel = new System.Windows.Forms.Label();
            this.MetadataModuleGitHubLinkLabel = new System.Windows.Forms.LinkLabel();
            this.RelationshipTabPage = new System.Windows.Forms.TabPage();
            this.ModuleRelationshipType = new System.Windows.Forms.ComboBox();
            this.DependsGraphTree = new System.Windows.Forms.TreeView();
            this.ContentTabPage = new System.Windows.Forms.TabPage();
            this.ContentsPreviewTree = new System.Windows.Forms.TreeView();
            this.ContentsDownloadButton = new System.Windows.Forms.Button();
            this.NotCachedLabel = new System.Windows.Forms.Label();
            this.ChangesetTabPage = new System.Windows.Forms.TabPage();
            this.CancelChangesButton = new System.Windows.Forms.Button();
            this.ConfirmChangesButton = new System.Windows.Forms.Button();
            this.ChangesListView = new System.Windows.Forms.ListView();
            this.Mod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ChangeType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Reason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.WaitTabPage = new System.Windows.Forms.TabPage();
            this.CancelCurrentActionButton = new System.Windows.Forms.Button();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.DialogProgressBar = new System.Windows.Forms.ProgressBar();
            this.MessageTextBox = new System.Windows.Forms.TextBox();
            this.ChooseRecommendedModsTabPage = new System.Windows.Forms.TabPage();
            this.RecommendedModsCancelButton = new System.Windows.Forms.Button();
            this.RecommendedModsContinueButton = new System.Windows.Forms.Button();
            this.RecommendedModsToggleCheckbox = new System.Windows.Forms.CheckBox();
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
            this.StatusPanel.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.ManageModsTabPage.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ModList)).BeginInit();
            this.ModInfoTabControl.SuspendLayout();
            this.MetadataTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.MetaDataUpperLayoutPanel.SuspendLayout();
            this.MetaDataLowerLayoutPanel.SuspendLayout();
            this.RelationshipTabPage.SuspendLayout();
            this.ContentTabPage.SuspendLayout();
            this.ChangesetTabPage.SuspendLayout();
            this.WaitTabPage.SuspendLayout();
            this.ChooseRecommendedModsTabPage.SuspendLayout();
            this.ChooseProvidedModsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1372, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectKSPInstallMenuItem,
            this.openKspDirectoryToolStripMenuItem,
            this.toolStripMenuItem1,
            this.ckanModListToolStripMenuItem,
            this.toolStripMenuItem3,
            this.ExitToolButton});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // selectKSPInstallMenuItem
            // 
            this.selectKSPInstallMenuItem.Name = "selectKSPInstallMenuItem";
            this.selectKSPInstallMenuItem.Size = new System.Drawing.Size(214, 26);
            this.selectKSPInstallMenuItem.Text = "Select KSP Install...";
            this.selectKSPInstallMenuItem.Click += new System.EventHandler(this.selectKSPInstallMenuItem_Click);
            // 
            // openKspDirectoryToolStripMenuItem
            // 
            this.openKspDirectoryToolStripMenuItem.Name = "openKspDirectoryToolStripMenuItem";
            this.openKspDirectoryToolStripMenuItem.Size = new System.Drawing.Size(214, 26);
            this.openKspDirectoryToolStripMenuItem.Text = "Open KSP Directory";
            this.openKspDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openKspDirectoryToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(211, 6);
            // 
            // ckanModListToolStripMenuItem
            // 
            this.ckanModListToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.switchToToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exportCurrentSetToolStripMenuItem1});
            this.ckanModListToolStripMenuItem.Name = "ckanModListToolStripMenuItem";
            this.ckanModListToolStripMenuItem.Size = new System.Drawing.Size(214, 26);
            this.ckanModListToolStripMenuItem.Text = ".ckan Mod List";
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(213, 26);
            this.importToolStripMenuItem.Text = "Import...";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // switchToToolStripMenuItem
            // 
            this.switchToToolStripMenuItem.Name = "switchToToolStripMenuItem";
            this.switchToToolStripMenuItem.Size = new System.Drawing.Size(213, 26);
            this.switchToToolStripMenuItem.Text = "Switch To...";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(210, 6);
            // 
            // exportCurrentSetToolStripMenuItem1
            // 
            this.exportCurrentSetToolStripMenuItem1.Name = "exportCurrentSetToolStripMenuItem1";
            this.exportCurrentSetToolStripMenuItem1.Size = new System.Drawing.Size(213, 26);
            this.exportCurrentSetToolStripMenuItem1.Text = "Export Current Set...";
            this.exportCurrentSetToolStripMenuItem1.Click += new System.EventHandler(this.exportCurrentSetToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(211, 6);
            // 
            // ExitToolButton
            // 
            this.ExitToolButton.Name = "ExitToolButton";
            this.ExitToolButton.Size = new System.Drawing.Size(214, 26);
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
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(74, 24);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // cKANSettingsToolStripMenuItem
            // 
            this.cKANSettingsToolStripMenuItem.Name = "cKANSettingsToolStripMenuItem";
            this.cKANSettingsToolStripMenuItem.Size = new System.Drawing.Size(210, 26);
            this.cKANSettingsToolStripMenuItem.Text = "CKAN settings";
            this.cKANSettingsToolStripMenuItem.Click += new System.EventHandler(this.CKANSettingsToolStripMenuItem_Click);
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(210, 26);
            this.pluginsToolStripMenuItem.Text = "CKAN plugins";
            this.pluginsToolStripMenuItem.Click += new System.EventHandler(this.pluginsToolStripMenuItem_Click);
            // 
            // kSPCommandlineToolStripMenuItem
            // 
            this.kSPCommandlineToolStripMenuItem.Name = "kSPCommandlineToolStripMenuItem";
            this.kSPCommandlineToolStripMenuItem.Size = new System.Drawing.Size(210, 26);
            this.kSPCommandlineToolStripMenuItem.Text = "KSP command-line";
            this.kSPCommandlineToolStripMenuItem.Click += new System.EventHandler(this.KSPCommandlineToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Location = new System.Drawing.Point(0, 862);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1372, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StatusPanel
            // 
            this.StatusPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StatusPanel.Controls.Add(this.StatusLabel);
            this.StatusPanel.Location = new System.Drawing.Point(0, 859);
            this.StatusPanel.Margin = new System.Windows.Forms.Padding(4);
            this.StatusPanel.Name = "StatusPanel";
            this.StatusPanel.Size = new System.Drawing.Size(1063, 23);
            this.StatusPanel.TabIndex = 8;
            // 
            // StatusLabel
            // 
            this.StatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StatusLabel.Location = new System.Drawing.Point(0, 0);
            this.StatusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(1063, 23);
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
            this.MainTabControl.Location = new System.Drawing.Point(0, 28);
            this.MainTabControl.Margin = new System.Windows.Forms.Padding(4);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(1372, 834);
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
            this.ManageModsTabPage.Controls.Add(this.FilterByDescriptionLabel);
            this.ManageModsTabPage.Controls.Add(this.FilterByDescriptionTextBox);
            this.ManageModsTabPage.Controls.Add(this.menuStrip2);
            this.ManageModsTabPage.Controls.Add(this.splitContainer1);
            this.ManageModsTabPage.Location = new System.Drawing.Point(4, 25);
            this.ManageModsTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.ManageModsTabPage.Name = "ManageModsTabPage";
            this.ManageModsTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.ManageModsTabPage.Size = new System.Drawing.Size(1364, 805);
            this.ManageModsTabPage.TabIndex = 0;
            this.ManageModsTabPage.Text = "Manage mods";
            // 
            // FilterByAuthorTextBox
            // 
            this.FilterByAuthorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByAuthorTextBox.Location = new System.Drawing.Point(483, 59);
            this.FilterByAuthorTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.FilterByAuthorTextBox.Name = "FilterByAuthorTextBox";
            this.FilterByAuthorTextBox.Size = new System.Drawing.Size(165, 22);
            this.FilterByAuthorTextBox.TabIndex = 10;
            this.FilterByAuthorTextBox.TextChanged += new System.EventHandler(this.FilterByAuthorTextBox_TextChanged);
            // 
            // FilterByAuthorLabel
            // 
            this.FilterByAuthorLabel.AutoSize = true;
            this.FilterByAuthorLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByAuthorLabel.Location = new System.Drawing.Point(331, 62);
            this.FilterByAuthorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByAuthorLabel.Name = "FilterByAuthorLabel";
            this.FilterByAuthorLabel.Size = new System.Drawing.Size(146, 17);
            this.FilterByAuthorLabel.TabIndex = 11;
            this.FilterByAuthorLabel.Text = "Filter by author name:";
            // 
            // KSPVersionLabel
            // 
            this.KSPVersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.KSPVersionLabel.AutoSize = true;
            this.KSPVersionLabel.Location = new System.Drawing.Point(1149, 12);
            this.KSPVersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.KSPVersionLabel.Name = "KSPVersionLabel";
            this.KSPVersionLabel.Size = new System.Drawing.Size(195, 17);
            this.KSPVersionLabel.TabIndex = 8;
            this.KSPVersionLabel.Text = "Kerbal Space Program 0.25.0";
            // 
            // FilterByNameLabel
            // 
            this.FilterByNameLabel.AutoSize = true;
            this.FilterByNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByNameLabel.Location = new System.Drawing.Point(5, 62);
            this.FilterByNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByNameLabel.Name = "FilterByNameLabel";
            this.FilterByNameLabel.Size = new System.Drawing.Size(132, 17);
            this.FilterByNameLabel.TabIndex = 10;
            this.FilterByNameLabel.Text = "Filter by mod name:";
            // 
            // FilterByNameTextBox
            // 
            this.FilterByNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByNameTextBox.Location = new System.Drawing.Point(143, 59);
            this.FilterByNameTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.FilterByNameTextBox.Name = "FilterByNameTextBox";
            this.FilterByNameTextBox.Size = new System.Drawing.Size(165, 22);
            this.FilterByNameTextBox.TabIndex = 9;
            this.FilterByNameTextBox.TextChanged += new System.EventHandler(this.FilterByNameTextBox_TextChanged);
            // 
            // FilterByDescriptionLabel
            // 
            this.FilterByDescriptionLabel.AutoSize = true;
            this.FilterByDescriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByDescriptionLabel.Location = new System.Drawing.Point(671, 62);
            this.FilterByDescriptionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByDescriptionLabel.Name = "FilterByDescriptionLabel";
            this.FilterByDescriptionLabel.Size = new System.Drawing.Size(135, 17);
            this.FilterByDescriptionLabel.TabIndex = 10;
            this.FilterByDescriptionLabel.Text = "Filter by description:";
            // 
            // FilterByDescriptionTextBox
            // 
            this.FilterByDescriptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByDescriptionTextBox.Location = new System.Drawing.Point(811, 59);
            this.FilterByDescriptionTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.FilterByDescriptionTextBox.Name = "FilterByDescriptionTextBox";
            this.FilterByDescriptionTextBox.Size = new System.Drawing.Size(165, 22);
            this.FilterByDescriptionTextBox.TabIndex = 9;
            this.FilterByDescriptionTextBox.TextChanged += new System.EventHandler(this.FilterByDescriptionTextBox_TextChanged);
            // 
            // menuStrip2
            // 
            this.menuStrip2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.menuStrip2.AutoSize = false;
            this.menuStrip2.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchKSPToolStripMenuItem,
            this.RefreshToolButton,
            this.UpdateAllToolButton,
            this.ApplyToolButton,
            this.FilterToolButton});
            this.menuStrip2.Location = new System.Drawing.Point(0, 4);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip2.Size = new System.Drawing.Size(5223, 49);
            this.menuStrip2.TabIndex = 2;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // launchKSPToolStripMenuItem
            // 
            this.launchKSPToolStripMenuItem.Image = global::CKAN.Properties.Resources.ksp;
            this.launchKSPToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.launchKSPToolStripMenuItem.Name = "launchKSPToolStripMenuItem";
            this.launchKSPToolStripMenuItem.Size = new System.Drawing.Size(128, 45);
            this.launchKSPToolStripMenuItem.Text = "Launch KSP";
            this.launchKSPToolStripMenuItem.Click += new System.EventHandler(this.launchKSPToolStripMenuItem_Click);
            // 
            // RefreshToolButton
            // 
            this.RefreshToolButton.Image = global::CKAN.Properties.Resources.refresh;
            this.RefreshToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.RefreshToolButton.Name = "RefreshToolButton";
            this.RefreshToolButton.Size = new System.Drawing.Size(102, 45);
            this.RefreshToolButton.Text = "Refresh";
            this.RefreshToolButton.Click += new System.EventHandler(this.RefreshToolButton_Click);
            // 
            // UpdateAllToolButton
            // 
            this.UpdateAllToolButton.Image = global::CKAN.Properties.Resources.update;
            this.UpdateAllToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.UpdateAllToolButton.Name = "UpdateAllToolButton";
            this.UpdateAllToolButton.Size = new System.Drawing.Size(202, 45);
            this.UpdateAllToolButton.Text = "Add available updates";
            this.UpdateAllToolButton.Click += new System.EventHandler(this.MarkAllUpdatesToolButton_Click);
            // 
            // ApplyToolButton
            // 
            this.ApplyToolButton.Image = global::CKAN.Properties.Resources.apply;
            this.ApplyToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ApplyToolButton.Name = "ApplyToolButton";
            this.ApplyToolButton.Size = new System.Drawing.Size(150, 45);
            this.ApplyToolButton.Text = "Apply changes";
            this.ApplyToolButton.Click += new System.EventHandler(this.ApplyToolButton_Click);
            // 
            // FilterToolButton
            // 
            this.FilterToolButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FilterCompatibleButton,
            this.FilterInstalledButton,
            this.FilterInstalledUpdateButton,
            this.cachedToolStripMenuItem,
            this.FilterNewButton,
            this.FilterNotInstalledButton,
            this.FilterIncompatibleButton,
            this.FilterAllButton});
            this.FilterToolButton.Image = global::CKAN.Properties.Resources.search;
            this.FilterToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.FilterToolButton.Name = "FilterToolButton";
            this.FilterToolButton.Size = new System.Drawing.Size(178, 45);
            this.FilterToolButton.Text = "Filter (Compatible)";
            // 
            // FilterCompatibleButton
            // 
            this.FilterCompatibleButton.Name = "FilterCompatibleButton";
            this.FilterCompatibleButton.Size = new System.Drawing.Size(265, 26);
            this.FilterCompatibleButton.Text = "Compatible";
            this.FilterCompatibleButton.Click += new System.EventHandler(this.FilterCompatibleButton_Click);
            // 
            // FilterInstalledButton
            // 
            this.FilterInstalledButton.Name = "FilterInstalledButton";
            this.FilterInstalledButton.Size = new System.Drawing.Size(265, 26);
            this.FilterInstalledButton.Text = "Installed";
            this.FilterInstalledButton.Click += new System.EventHandler(this.FilterInstalledButton_Click);
            // 
            // FilterInstalledUpdateButton
            // 
            this.FilterInstalledUpdateButton.Name = "FilterInstalledUpdateButton";
            this.FilterInstalledUpdateButton.Size = new System.Drawing.Size(265, 26);
            this.FilterInstalledUpdateButton.Text = "Installed (update available)";
            this.FilterInstalledUpdateButton.Click += new System.EventHandler(this.FilterInstalledUpdateButton_Click);
            // 
            // cachedToolStripMenuItem
            // 
            this.cachedToolStripMenuItem.Name = "cachedToolStripMenuItem";
            this.cachedToolStripMenuItem.Size = new System.Drawing.Size(265, 26);
            this.cachedToolStripMenuItem.Text = "Cached";
            this.cachedToolStripMenuItem.Click += new System.EventHandler(this.cachedToolStripMenuItem_Click);
            // 
            // FilterNewButton
            // 
            this.FilterNewButton.Name = "FilterNewButton";
            this.FilterNewButton.Size = new System.Drawing.Size(265, 26);
            this.FilterNewButton.Text = "New in repository";
            this.FilterNewButton.Click += new System.EventHandler(this.FilterNewButton_Click);
            // 
            // FilterNotInstalledButton
            // 
            this.FilterNotInstalledButton.Name = "FilterNotInstalledButton";
            this.FilterNotInstalledButton.Size = new System.Drawing.Size(265, 26);
            this.FilterNotInstalledButton.Text = "Not installed";
            this.FilterNotInstalledButton.Click += new System.EventHandler(this.FilterNotInstalledButton_Click);
            // 
            // FilterIncompatibleButton
            // 
            this.FilterIncompatibleButton.Name = "FilterIncompatibleButton";
            this.FilterIncompatibleButton.Size = new System.Drawing.Size(265, 26);
            this.FilterIncompatibleButton.Text = "Incompatible";
            this.FilterIncompatibleButton.Click += new System.EventHandler(this.FilterIncompatibleButton_Click);
            // 
            // FilterAllButton
            // 
            this.FilterAllButton.Name = "FilterAllButton";
            this.FilterAllButton.Size = new System.Drawing.Size(265, 26);
            this.FilterAllButton.Text = "All";
            this.FilterAllButton.Click += new System.EventHandler(this.FilterAllButton_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 89);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ModList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ModInfoTabControl);
            this.splitContainer1.Size = new System.Drawing.Size(1353, 718);
            this.splitContainer1.SplitterDistance = 984;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 7;
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
            this.Update,
            this.ModName,
            this.Author,
            this.InstalledVersion,
            this.LatestVersion,
            this.KSPCompatibility,
            this.SizeCol,
            this.Description});
            this.ModList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModList.Location = new System.Drawing.Point(0, 0);
            this.ModList.Margin = new System.Windows.Forms.Padding(4);
            this.ModList.MultiSelect = false;
            this.ModList.Name = "ModList";
            this.ModList.RowHeadersVisible = false;
            this.ModList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ModList.Size = new System.Drawing.Size(984, 718);
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
            this.Update.HeaderText = "Update";
            this.Update.Name = "Update";
            this.Update.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.Update.Width = 46;
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
            // KSPCompatibility
            // 
            this.KSPCompatibility.HeaderText = "Max KSP version";
            this.KSPCompatibility.Name = "KSPCompatibility";
            this.KSPCompatibility.ReadOnly = true;
            this.KSPCompatibility.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.KSPCompatibility.Width = 78;
            // 
            // SizeCol
            // 
            this.SizeCol.HeaderText = "Download (KB)";
            this.SizeCol.Name = "SizeCol";
            this.SizeCol.ReadOnly = true;
            this.SizeCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // Description
            // 
            this.Description.HeaderText = "Description";
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.Description.Width = 821;
            // 
            // ModInfoTabControl
            // 
            this.ModInfoTabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.ModInfoTabControl.Controls.Add(this.MetadataTabPage);
            this.ModInfoTabControl.Controls.Add(this.RelationshipTabPage);
            this.ModInfoTabControl.Controls.Add(this.ContentTabPage);
            this.ModInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModInfoTabControl.Location = new System.Drawing.Point(0, 0);
            this.ModInfoTabControl.Margin = new System.Windows.Forms.Padding(4);
            this.ModInfoTabControl.Name = "ModInfoTabControl";
            this.ModInfoTabControl.SelectedIndex = 0;
            this.ModInfoTabControl.Size = new System.Drawing.Size(364, 718);
            this.ModInfoTabControl.TabIndex = 0;
            this.ModInfoTabControl.SelectedIndexChanged += new System.EventHandler(this.ModInfoIndexChanged);
            // 
            // MetadataTabPage
            // 
            this.MetadataTabPage.Controls.Add(this.splitContainer2);
            this.MetadataTabPage.Location = new System.Drawing.Point(4, 28);
            this.MetadataTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataTabPage.Name = "MetadataTabPage";
            this.MetadataTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.MetadataTabPage.Size = new System.Drawing.Size(356, 686);
            this.MetadataTabPage.TabIndex = 0;
            this.MetadataTabPage.Text = "Metadata";
            this.MetadataTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(4, 4);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.MetaDataUpperLayoutPanel);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.MetaDataLowerLayoutPanel);
            this.splitContainer2.Size = new System.Drawing.Size(348, 678);
            this.splitContainer2.SplitterDistance = 325;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            // 
            // MetaDataUpperLayoutPanel
            // 
            this.MetaDataUpperLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MetaDataUpperLayoutPanel.ColumnCount = 1;
            this.MetaDataUpperLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MetaDataUpperLayoutPanel.Controls.Add(this.MetadataModuleNameLabel, 0, 0);
            this.MetaDataUpperLayoutPanel.Controls.Add(this.MetadataModuleAbstractLabel, 0, 1);
            this.MetaDataUpperLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetaDataUpperLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.MetaDataUpperLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.MetaDataUpperLayoutPanel.Name = "MetaDataUpperLayoutPanel";
            this.MetaDataUpperLayoutPanel.RowCount = 2;
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.MetaDataUpperLayoutPanel.Size = new System.Drawing.Size(346, 323);
            this.MetaDataUpperLayoutPanel.TabIndex = 0;
            // 
            // MetadataModuleNameLabel
            // 
            this.MetadataModuleNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MetadataModuleNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.MetadataModuleNameLabel.Location = new System.Drawing.Point(4, 0);
            this.MetadataModuleNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MetadataModuleNameLabel.Name = "MetadataModuleNameLabel";
            this.MetadataModuleNameLabel.Size = new System.Drawing.Size(338, 64);
            this.MetadataModuleNameLabel.TabIndex = 0;
            this.MetadataModuleNameLabel.Text = "Mod Name";
            this.MetadataModuleNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MetadataModuleAbstractLabel
            // 
            this.MetadataModuleAbstractLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataModuleAbstractLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAbstractLabel.Location = new System.Drawing.Point(4, 68);
            this.MetadataModuleAbstractLabel.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataModuleAbstractLabel.Name = "MetadataModuleAbstractLabel";
            this.MetadataModuleAbstractLabel.ReadOnly = true;
            this.MetadataModuleAbstractLabel.Size = new System.Drawing.Size(338, 251);
            this.MetadataModuleAbstractLabel.TabIndex = 27;
            this.MetadataModuleAbstractLabel.Text = "";
            // 
            // MetaDataLowerLayoutPanel
            // 
            this.MetaDataLowerLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MetaDataLowerLayoutPanel.ColumnCount = 2;
            this.MetaDataLowerLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.16279F));
            this.MetaDataLowerLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.83721F));
            this.MetaDataLowerLayoutPanel.Controls.Add(this.IdentifierLabel, 0, 7);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataIdentifierLabel, 0, 7);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.KSPCompatibilityLabel, 0, 6);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.ReleaseLabel, 0, 5);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.GitHubLabel, 0, 4);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.HomePageLabel, 0, 3);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.AuthorLabel, 0, 2);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.LicenseLabel, 0, 1);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleVersionLabel, 1, 0);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleLicenseLabel, 1, 1);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleAuthorLabel, 1, 2);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.VersionLabel, 0, 0);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleReleaseStatusLabel, 1, 5);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleHomePageLinkLabel, 1, 3);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleKSPCompatibilityLabel, 1, 6);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleGitHubLinkLabel, 1, 4);
            this.MetaDataLowerLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetaDataLowerLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.MetaDataLowerLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.MetaDataLowerLayoutPanel.Name = "MetaDataLowerLayoutPanel";
            this.MetaDataLowerLayoutPanel.RowCount = 9;
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MetaDataLowerLayoutPanel.Size = new System.Drawing.Size(346, 346);
            this.MetaDataLowerLayoutPanel.TabIndex = 0;
            // 
            // IdentifierLabel
            // 
            this.IdentifierLabel.AutoSize = true;
            this.IdentifierLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IdentifierLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.IdentifierLabel.Location = new System.Drawing.Point(4, 259);
            this.IdentifierLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.IdentifierLabel.Name = "IdentifierLabel";
            this.IdentifierLabel.Size = new System.Drawing.Size(82, 25);
            this.IdentifierLabel.TabIndex = 28;
            this.IdentifierLabel.Text = "Identifier";
            // 
            // MetadataIdentifierLabel
            // 
            this.MetadataIdentifierLabel.AutoSize = true;
            this.MetadataIdentifierLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataIdentifierLabel.ForeColor = System.Drawing.Color.Black;
            this.MetadataIdentifierLabel.Location = new System.Drawing.Point(94, 259);
            this.MetadataIdentifierLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MetadataIdentifierLabel.Name = "MetadataIdentifierLabel";
            this.MetadataIdentifierLabel.Size = new System.Drawing.Size(248, 25);
            this.MetadataIdentifierLabel.TabIndex = 27;
            this.MetadataIdentifierLabel.Text = "-";
            // 
            // KSPCompatibilityLabel
            // 
            this.KSPCompatibilityLabel.AutoSize = true;
            this.KSPCompatibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.KSPCompatibilityLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.KSPCompatibilityLabel.Location = new System.Drawing.Point(4, 222);
            this.KSPCompatibilityLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.KSPCompatibilityLabel.Name = "KSPCompatibilityLabel";
            this.KSPCompatibilityLabel.Size = new System.Drawing.Size(82, 37);
            this.KSPCompatibilityLabel.TabIndex = 13;
            this.KSPCompatibilityLabel.Text = "Max KSP ver.:";
            // 
            // ReleaseLabel
            // 
            this.ReleaseLabel.AutoSize = true;
            this.ReleaseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReleaseLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ReleaseLabel.Location = new System.Drawing.Point(4, 185);
            this.ReleaseLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ReleaseLabel.Name = "ReleaseLabel";
            this.ReleaseLabel.Size = new System.Drawing.Size(82, 37);
            this.ReleaseLabel.TabIndex = 12;
            this.ReleaseLabel.Text = "Release status:";
            // 
            // GitHubLabel
            // 
            this.GitHubLabel.AutoSize = true;
            this.GitHubLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitHubLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.GitHubLabel.Location = new System.Drawing.Point(4, 148);
            this.GitHubLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.GitHubLabel.Name = "GitHubLabel";
            this.GitHubLabel.Size = new System.Drawing.Size(82, 37);
            this.GitHubLabel.TabIndex = 10;
            this.GitHubLabel.Text = "Source Code:";
            // 
            // HomePageLabel
            // 
            this.HomePageLabel.AutoSize = true;
            this.HomePageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HomePageLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.HomePageLabel.Location = new System.Drawing.Point(4, 111);
            this.HomePageLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.HomePageLabel.Name = "HomePageLabel";
            this.HomePageLabel.Size = new System.Drawing.Size(82, 37);
            this.HomePageLabel.TabIndex = 7;
            this.HomePageLabel.Text = "Homepage:";
            // 
            // AuthorLabel
            // 
            this.AuthorLabel.AutoSize = true;
            this.AuthorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AuthorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AuthorLabel.Location = new System.Drawing.Point(4, 74);
            this.AuthorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.AuthorLabel.Name = "AuthorLabel";
            this.AuthorLabel.Size = new System.Drawing.Size(82, 37);
            this.AuthorLabel.TabIndex = 5;
            this.AuthorLabel.Text = "Author:";
            // 
            // LicenseLabel
            // 
            this.LicenseLabel.AutoSize = true;
            this.LicenseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LicenseLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.LicenseLabel.Location = new System.Drawing.Point(4, 37);
            this.LicenseLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LicenseLabel.Name = "LicenseLabel";
            this.LicenseLabel.Size = new System.Drawing.Size(82, 37);
            this.LicenseLabel.TabIndex = 3;
            this.LicenseLabel.Text = "License:";
            // 
            // MetadataModuleVersionLabel
            // 
            this.MetadataModuleVersionLabel.AutoSize = true;
            this.MetadataModuleVersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleVersionLabel.Location = new System.Drawing.Point(94, 0);
            this.MetadataModuleVersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MetadataModuleVersionLabel.Name = "MetadataModuleVersionLabel";
            this.MetadataModuleVersionLabel.Size = new System.Drawing.Size(248, 37);
            this.MetadataModuleVersionLabel.TabIndex = 2;
            this.MetadataModuleVersionLabel.Text = "0.0.0";
            // 
            // MetadataModuleLicenseLabel
            // 
            this.MetadataModuleLicenseLabel.AutoSize = true;
            this.MetadataModuleLicenseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleLicenseLabel.Location = new System.Drawing.Point(94, 37);
            this.MetadataModuleLicenseLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MetadataModuleLicenseLabel.Name = "MetadataModuleLicenseLabel";
            this.MetadataModuleLicenseLabel.Size = new System.Drawing.Size(248, 37);
            this.MetadataModuleLicenseLabel.TabIndex = 4;
            this.MetadataModuleLicenseLabel.Text = "None";
            // 
            // MetadataModuleAuthorLabel
            // 
            this.MetadataModuleAuthorLabel.AutoSize = true;
            this.MetadataModuleAuthorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAuthorLabel.Location = new System.Drawing.Point(94, 74);
            this.MetadataModuleAuthorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MetadataModuleAuthorLabel.Name = "MetadataModuleAuthorLabel";
            this.MetadataModuleAuthorLabel.Size = new System.Drawing.Size(248, 37);
            this.MetadataModuleAuthorLabel.TabIndex = 6;
            this.MetadataModuleAuthorLabel.Text = "Nobody";
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VersionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.VersionLabel.Location = new System.Drawing.Point(4, 0);
            this.VersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(82, 37);
            this.VersionLabel.TabIndex = 1;
            this.VersionLabel.Text = "Version:";
            // 
            // MetadataModuleReleaseStatusLabel
            // 
            this.MetadataModuleReleaseStatusLabel.AutoSize = true;
            this.MetadataModuleReleaseStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleReleaseStatusLabel.Location = new System.Drawing.Point(94, 185);
            this.MetadataModuleReleaseStatusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MetadataModuleReleaseStatusLabel.Name = "MetadataModuleReleaseStatusLabel";
            this.MetadataModuleReleaseStatusLabel.Size = new System.Drawing.Size(248, 37);
            this.MetadataModuleReleaseStatusLabel.TabIndex = 11;
            this.MetadataModuleReleaseStatusLabel.Text = "Stable";
            // 
            // MetadataModuleHomePageLinkLabel
            // 
            this.MetadataModuleHomePageLinkLabel.AutoEllipsis = true;
            this.MetadataModuleHomePageLinkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleHomePageLinkLabel.Location = new System.Drawing.Point(94, 111);
            this.MetadataModuleHomePageLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MetadataModuleHomePageLinkLabel.Name = "MetadataModuleHomePageLinkLabel";
            this.MetadataModuleHomePageLinkLabel.Size = new System.Drawing.Size(248, 37);
            this.MetadataModuleHomePageLinkLabel.TabIndex = 25;
            this.MetadataModuleHomePageLinkLabel.TabStop = true;
            this.MetadataModuleHomePageLinkLabel.Text = "linkLabel1";
            this.MetadataModuleHomePageLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // MetadataModuleKSPCompatibilityLabel
            // 
            this.MetadataModuleKSPCompatibilityLabel.AutoSize = true;
            this.MetadataModuleKSPCompatibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleKSPCompatibilityLabel.Location = new System.Drawing.Point(94, 222);
            this.MetadataModuleKSPCompatibilityLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MetadataModuleKSPCompatibilityLabel.Name = "MetadataModuleKSPCompatibilityLabel";
            this.MetadataModuleKSPCompatibilityLabel.Size = new System.Drawing.Size(248, 37);
            this.MetadataModuleKSPCompatibilityLabel.TabIndex = 14;
            this.MetadataModuleKSPCompatibilityLabel.Text = "0.0.0";
            // 
            // MetadataModuleGitHubLinkLabel
            // 
            this.MetadataModuleGitHubLinkLabel.AutoEllipsis = true;
            this.MetadataModuleGitHubLinkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleGitHubLinkLabel.Location = new System.Drawing.Point(94, 148);
            this.MetadataModuleGitHubLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MetadataModuleGitHubLinkLabel.Name = "MetadataModuleGitHubLinkLabel";
            this.MetadataModuleGitHubLinkLabel.Size = new System.Drawing.Size(248, 37);
            this.MetadataModuleGitHubLinkLabel.TabIndex = 26;
            this.MetadataModuleGitHubLinkLabel.TabStop = true;
            this.MetadataModuleGitHubLinkLabel.Text = "linkLabel2";
            this.MetadataModuleGitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // RelationshipTabPage
            // 
            this.RelationshipTabPage.Controls.Add(this.ModuleRelationshipType);
            this.RelationshipTabPage.Controls.Add(this.DependsGraphTree);
            this.RelationshipTabPage.Location = new System.Drawing.Point(4, 28);
            this.RelationshipTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.RelationshipTabPage.Name = "RelationshipTabPage";
            this.RelationshipTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.RelationshipTabPage.Size = new System.Drawing.Size(356, 686);
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
            this.ModuleRelationshipType.Location = new System.Drawing.Point(8, 9);
            this.ModuleRelationshipType.Margin = new System.Windows.Forms.Padding(4);
            this.ModuleRelationshipType.Name = "ModuleRelationshipType";
            this.ModuleRelationshipType.Size = new System.Drawing.Size(335, 24);
            this.ModuleRelationshipType.TabIndex = 1;
            this.ModuleRelationshipType.SelectedIndexChanged += new System.EventHandler(this.ModuleRelationshipType_SelectedIndexChanged);
            // 
            // DependsGraphTree
            // 
            this.DependsGraphTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DependsGraphTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DependsGraphTree.Location = new System.Drawing.Point(4, 42);
            this.DependsGraphTree.Margin = new System.Windows.Forms.Padding(4);
            this.DependsGraphTree.Name = "DependsGraphTree";
            this.DependsGraphTree.Size = new System.Drawing.Size(344, 629);
            this.DependsGraphTree.TabIndex = 0;
            this.DependsGraphTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.DependsGraphTree_NodeMouseDoubleClick);
            // 
            // ContentTabPage
            // 
            this.ContentTabPage.Controls.Add(this.ContentsPreviewTree);
            this.ContentTabPage.Controls.Add(this.ContentsDownloadButton);
            this.ContentTabPage.Controls.Add(this.NotCachedLabel);
            this.ContentTabPage.Location = new System.Drawing.Point(4, 28);
            this.ContentTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.ContentTabPage.Name = "ContentTabPage";
            this.ContentTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.ContentTabPage.Size = new System.Drawing.Size(356, 686);
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
            this.ContentsPreviewTree.Location = new System.Drawing.Point(0, 80);
            this.ContentsPreviewTree.Margin = new System.Windows.Forms.Padding(4);
            this.ContentsPreviewTree.Name = "ContentsPreviewTree";
            this.ContentsPreviewTree.Size = new System.Drawing.Size(348, 595);
            this.ContentsPreviewTree.TabIndex = 2;
            this.ContentsPreviewTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ContentsPreviewTree_NodeMouseDoubleClick);
            // 
            // ContentsDownloadButton
            // 
            this.ContentsDownloadButton.Location = new System.Drawing.Point(8, 44);
            this.ContentsDownloadButton.Margin = new System.Windows.Forms.Padding(4);
            this.ContentsDownloadButton.Name = "ContentsDownloadButton";
            this.ContentsDownloadButton.Size = new System.Drawing.Size(137, 28);
            this.ContentsDownloadButton.TabIndex = 1;
            this.ContentsDownloadButton.Text = "Download";
            this.ContentsDownloadButton.UseVisualStyleBackColor = true;
            this.ContentsDownloadButton.Click += new System.EventHandler(this.ContentsDownloadButton_Click);
            // 
            // NotCachedLabel
            // 
            this.NotCachedLabel.Location = new System.Drawing.Point(4, 4);
            this.NotCachedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.NotCachedLabel.Name = "NotCachedLabel";
            this.NotCachedLabel.Size = new System.Drawing.Size(288, 37);
            this.NotCachedLabel.TabIndex = 0;
            this.NotCachedLabel.Text = "This mod is not in the cache, click \'Download\' to preview contents";
            // 
            // ChangesetTabPage
            // 
            this.ChangesetTabPage.Controls.Add(this.CancelChangesButton);
            this.ChangesetTabPage.Controls.Add(this.ConfirmChangesButton);
            this.ChangesetTabPage.Controls.Add(this.ChangesListView);
            this.ChangesetTabPage.Location = new System.Drawing.Point(4, 25);
            this.ChangesetTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.ChangesetTabPage.Name = "ChangesetTabPage";
            this.ChangesetTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.ChangesetTabPage.Size = new System.Drawing.Size(1364, 805);
            this.ChangesetTabPage.TabIndex = 2;
            this.ChangesetTabPage.Text = "Changeset";
            this.ChangesetTabPage.UseVisualStyleBackColor = true;
            // 
            // CancelChangesButton
            // 
            this.CancelChangesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelChangesButton.Location = new System.Drawing.Point(1145, 759);
            this.CancelChangesButton.Margin = new System.Windows.Forms.Padding(4);
            this.CancelChangesButton.Name = "CancelChangesButton";
            this.CancelChangesButton.Size = new System.Drawing.Size(100, 28);
            this.CancelChangesButton.TabIndex = 6;
            this.CancelChangesButton.Text = "Clear";
            this.CancelChangesButton.UseVisualStyleBackColor = true;
            this.CancelChangesButton.Click += new System.EventHandler(this.CancelChangesButton_Click);
            // 
            // ConfirmChangesButton
            // 
            this.ConfirmChangesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ConfirmChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConfirmChangesButton.Location = new System.Drawing.Point(1253, 759);
            this.ConfirmChangesButton.Margin = new System.Windows.Forms.Padding(4);
            this.ConfirmChangesButton.Name = "ConfirmChangesButton";
            this.ConfirmChangesButton.Size = new System.Drawing.Size(100, 28);
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
            this.Mod,
            this.ChangeType,
            this.Reason});
            this.ChangesListView.FullRowSelect = true;
            this.ChangesListView.Location = new System.Drawing.Point(-1, 0);
            this.ChangesListView.Margin = new System.Windows.Forms.Padding(4);
            this.ChangesListView.Name = "ChangesListView";
            this.ChangesListView.Size = new System.Drawing.Size(1362, 752);
            this.ChangesListView.TabIndex = 4;
            this.ChangesListView.UseCompatibleStateImageBehavior = false;
            this.ChangesListView.View = System.Windows.Forms.View.Details;
            // 
            // Mod
            // 
            this.Mod.Text = "Mod";
            this.Mod.Width = 332;
            // 
            // ChangeType
            // 
            this.ChangeType.Text = "Change";
            this.ChangeType.Width = 111;
            // 
            // Reason
            // 
            this.Reason.Text = "Reason for action";
            this.Reason.Width = 606;
            // 
            // WaitTabPage
            // 
            this.WaitTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.WaitTabPage.Controls.Add(this.CancelCurrentActionButton);
            this.WaitTabPage.Controls.Add(this.LogTextBox);
            this.WaitTabPage.Controls.Add(this.DialogProgressBar);
            this.WaitTabPage.Controls.Add(this.MessageTextBox);
            this.WaitTabPage.Location = new System.Drawing.Point(4, 25);
            this.WaitTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.WaitTabPage.Name = "WaitTabPage";
            this.WaitTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.WaitTabPage.Size = new System.Drawing.Size(1364, 805);
            this.WaitTabPage.TabIndex = 1;
            this.WaitTabPage.Text = "Status log";
            // 
            // CancelCurrentActionButton
            // 
            this.CancelCurrentActionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelCurrentActionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelCurrentActionButton.Location = new System.Drawing.Point(1253, 761);
            this.CancelCurrentActionButton.Margin = new System.Windows.Forms.Padding(4);
            this.CancelCurrentActionButton.Name = "CancelCurrentActionButton";
            this.CancelCurrentActionButton.Size = new System.Drawing.Size(100, 28);
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
            this.LogTextBox.Location = new System.Drawing.Point(12, 71);
            this.LogTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(1338, 681);
            this.LogTextBox.TabIndex = 8;
            // 
            // DialogProgressBar
            // 
            this.DialogProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DialogProgressBar.Location = new System.Drawing.Point(12, 36);
            this.DialogProgressBar.Margin = new System.Windows.Forms.Padding(4);
            this.DialogProgressBar.Name = "DialogProgressBar";
            this.DialogProgressBar.Size = new System.Drawing.Size(1339, 28);
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
            this.MessageTextBox.Location = new System.Drawing.Point(11, 7);
            this.MessageTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.MessageTextBox.Multiline = true;
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.ReadOnly = true;
            this.MessageTextBox.Size = new System.Drawing.Size(1343, 21);
            this.MessageTextBox.TabIndex = 6;
            this.MessageTextBox.Text = "Waiting for operation to complete";
            this.MessageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ChooseRecommendedModsTabPage
            // 
            this.ChooseRecommendedModsTabPage.Controls.Add(this.RecommendedModsCancelButton);
            this.ChooseRecommendedModsTabPage.Controls.Add(this.RecommendedModsContinueButton);
            this.ChooseRecommendedModsTabPage.Controls.Add(this.RecommendedModsToggleCheckbox);
            this.ChooseRecommendedModsTabPage.Controls.Add(this.RecommendedDialogLabel);
            this.ChooseRecommendedModsTabPage.Controls.Add(this.RecommendedModsListView);
            this.ChooseRecommendedModsTabPage.Location = new System.Drawing.Point(4, 25);
            this.ChooseRecommendedModsTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.ChooseRecommendedModsTabPage.Name = "ChooseRecommendedModsTabPage";
            this.ChooseRecommendedModsTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.ChooseRecommendedModsTabPage.Size = new System.Drawing.Size(1364, 805);
            this.ChooseRecommendedModsTabPage.TabIndex = 3;
            this.ChooseRecommendedModsTabPage.Text = "Choose recommended mods";
            this.ChooseRecommendedModsTabPage.UseVisualStyleBackColor = true;
            // 
            // RecommendedModsCancelButton
            // 
            this.RecommendedModsCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RecommendedModsCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RecommendedModsCancelButton.Location = new System.Drawing.Point(1145, 759);
            this.RecommendedModsCancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.RecommendedModsCancelButton.Name = "RecommendedModsCancelButton";
            this.RecommendedModsCancelButton.Size = new System.Drawing.Size(100, 28);
            this.RecommendedModsCancelButton.TabIndex = 8;
            this.RecommendedModsCancelButton.Text = "Cancel";
            this.RecommendedModsCancelButton.UseVisualStyleBackColor = true;
            this.RecommendedModsCancelButton.Click += new System.EventHandler(this.RecommendedModsCancelButton_Click);
            // 
            // RecommendedModsContinueButton
            // 
            this.RecommendedModsContinueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RecommendedModsContinueButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RecommendedModsContinueButton.Location = new System.Drawing.Point(1253, 759);
            this.RecommendedModsContinueButton.Margin = new System.Windows.Forms.Padding(4);
            this.RecommendedModsContinueButton.Name = "RecommendedModsContinueButton";
            this.RecommendedModsContinueButton.Size = new System.Drawing.Size(100, 28);
            this.RecommendedModsContinueButton.TabIndex = 7;
            this.RecommendedModsContinueButton.Text = "Continue";
            this.RecommendedModsContinueButton.UseVisualStyleBackColor = true;
            this.RecommendedModsContinueButton.Click += new System.EventHandler(this.RecommendedModsContinueButton_Click);
            // 
            // RecommendedModsToggleCheckbox
            // 
            this.RecommendedModsToggleCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RecommendedModsToggleCheckbox.AutoSize = true;
            this.RecommendedModsToggleCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RecommendedModsToggleCheckbox.Location = new System.Drawing.Point(11, 763);
            this.RecommendedModsToggleCheckbox.Margin = new System.Windows.Forms.Padding(4);
            this.RecommendedModsToggleCheckbox.Name = "RecommendedModsToggleCheckbox";
            this.RecommendedModsToggleCheckbox.Size = new System.Drawing.Size(117, 21);
            this.RecommendedModsToggleCheckbox.TabIndex = 9;
            this.RecommendedModsToggleCheckbox.Text = "Toggle * Mods";
            this.RecommendedModsToggleCheckbox.UseVisualStyleBackColor = true;
            this.RecommendedModsToggleCheckbox.CheckedChanged += new System.EventHandler(this.RecommendedModsToggleCheckbox_CheckedChanged);
            // 
            // RecommendedDialogLabel
            // 
            this.RecommendedDialogLabel.AutoSize = true;
            this.RecommendedDialogLabel.Location = new System.Drawing.Point(4, 16);
            this.RecommendedDialogLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.RecommendedDialogLabel.Name = "RecommendedDialogLabel";
            this.RecommendedDialogLabel.Size = new System.Drawing.Size(564, 17);
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
            this.RecommendedModsListView.FullRowSelect = true;
            this.RecommendedModsListView.Location = new System.Drawing.Point(8, 36);
            this.RecommendedModsListView.Margin = new System.Windows.Forms.Padding(4);
            this.RecommendedModsListView.Name = "RecommendedModsListView";
            this.RecommendedModsListView.Size = new System.Drawing.Size(1342, 716);
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
            this.ChooseProvidedModsTabPage.Location = new System.Drawing.Point(4, 25);
            this.ChooseProvidedModsTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.ChooseProvidedModsTabPage.Name = "ChooseProvidedModsTabPage";
            this.ChooseProvidedModsTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.ChooseProvidedModsTabPage.Size = new System.Drawing.Size(1364, 805);
            this.ChooseProvidedModsTabPage.TabIndex = 4;
            this.ChooseProvidedModsTabPage.Text = "Choose mods";
            this.ChooseProvidedModsTabPage.UseVisualStyleBackColor = true;
            // 
            // ChooseProvidedModsCancelButton
            // 
            this.ChooseProvidedModsCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ChooseProvidedModsCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChooseProvidedModsCancelButton.Location = new System.Drawing.Point(1143, 758);
            this.ChooseProvidedModsCancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.ChooseProvidedModsCancelButton.Name = "ChooseProvidedModsCancelButton";
            this.ChooseProvidedModsCancelButton.Size = new System.Drawing.Size(100, 28);
            this.ChooseProvidedModsCancelButton.TabIndex = 10;
            this.ChooseProvidedModsCancelButton.Text = "Cancel";
            this.ChooseProvidedModsCancelButton.UseVisualStyleBackColor = true;
            this.ChooseProvidedModsCancelButton.Click += new System.EventHandler(this.ChooseProvidedModsCancelButton_Click);
            // 
            // ChooseProvidedModsContinueButton
            // 
            this.ChooseProvidedModsContinueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ChooseProvidedModsContinueButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChooseProvidedModsContinueButton.Location = new System.Drawing.Point(1251, 758);
            this.ChooseProvidedModsContinueButton.Margin = new System.Windows.Forms.Padding(4);
            this.ChooseProvidedModsContinueButton.Name = "ChooseProvidedModsContinueButton";
            this.ChooseProvidedModsContinueButton.Size = new System.Drawing.Size(100, 28);
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
            this.ChooseProvidedModsListView.FullRowSelect = true;
            this.ChooseProvidedModsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ChooseProvidedModsListView.Location = new System.Drawing.Point(8, 34);
            this.ChooseProvidedModsListView.Margin = new System.Windows.Forms.Padding(4);
            this.ChooseProvidedModsListView.MultiSelect = false;
            this.ChooseProvidedModsListView.Name = "ChooseProvidedModsListView";
            this.ChooseProvidedModsListView.Size = new System.Drawing.Size(1342, 716);
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
            this.ChooseProvidedModsLabel.Location = new System.Drawing.Point(8, 15);
            this.ChooseProvidedModsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ChooseProvidedModsLabel.Name = "ChooseProvidedModsLabel";
            this.ChooseProvidedModsLabel.Size = new System.Drawing.Size(511, 17);
            this.ChooseProvidedModsLabel.TabIndex = 7;
            this.ChooseProvidedModsLabel.Text = "Several mods provide the virtual module Foo, choose one of the following mods:";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1372, 884);
            this.Controls.Add(this.MainTabControl);
            this.Controls.Add(this.StatusPanel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1165, 806);
            this.Name = "Main";
            this.Text = "CKAN-GUI";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.StatusPanel.ResumeLayout(false);
            this.MainTabControl.ResumeLayout(false);
            this.ManageModsTabPage.ResumeLayout(false);
            this.ManageModsTabPage.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ModList)).EndInit();
            this.ModInfoTabControl.ResumeLayout(false);
            this.MetadataTabPage.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.MetaDataUpperLayoutPanel.ResumeLayout(false);
            this.MetaDataLowerLayoutPanel.ResumeLayout(false);
            this.MetaDataLowerLayoutPanel.PerformLayout();
            this.RelationshipTabPage.ResumeLayout(false);
            this.ContentTabPage.ResumeLayout(false);
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
        private CKAN.MainTabControl MainTabControl;
        private TabPage ManageModsTabPage;
        private Label KSPVersionLabel;
        private Label FilterByNameLabel;
        private TextBox FilterByNameTextBox;
        private Label FilterByDescriptionLabel;
        private TextBox FilterByDescriptionTextBox;
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
        private ColumnHeader Mod;
        private ColumnHeader ChangeType;
        private ColumnHeader Reason;
        private TabPage ChooseRecommendedModsTabPage;
        private Label RecommendedDialogLabel;
        private ListView RecommendedModsListView;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader5;
        private Button RecommendedModsCancelButton;
        private Button RecommendedModsContinueButton;
        private CheckBox RecommendedModsToggleCheckbox;
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
        private ToolStripMenuItem pluginsToolStripMenuItem;
        public ToolStripMenuItem settingsToolStripMenuItem;
        private TextBox FilterByAuthorTextBox;
        private Label FilterByAuthorLabel;
        private ToolStripMenuItem FilterAllButton;
        private ToolStripMenuItem selectKSPInstallMenuItem;
        private ToolStripMenuItem openKspDirectoryToolStripMenuItem;
        private SplitContainer splitContainer2;
        private TableLayoutPanel MetaDataUpperLayoutPanel;
        public MainModListGUI ModList;
        private TableLayoutPanel MetaDataLowerLayoutPanel;
        private DataGridViewCheckBoxColumn Installed;
        private DataGridViewCheckBoxColumn Update;
        private DataGridViewTextBoxColumn ModName;
        private DataGridViewTextBoxColumn Author;
        private DataGridViewTextBoxColumn InstalledVersion;
        private DataGridViewTextBoxColumn LatestVersion;
        private DataGridViewTextBoxColumn KSPCompatibility;
        private DataGridViewTextBoxColumn SizeCol;
        private DataGridViewTextBoxColumn Description;
        private ToolStripMenuItem cachedToolStripMenuItem;
        private Label IdentifierLabel;
        private Label MetadataIdentifierLabel;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem ckanModListToolStripMenuItem;
        private ToolStripMenuItem importToolStripMenuItem;
        private ToolStripMenuItem switchToToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem exportCurrentSetToolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem3;
    }
}
