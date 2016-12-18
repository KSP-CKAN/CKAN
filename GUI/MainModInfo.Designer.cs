namespace CKAN
{
    partial class MainModInfo
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
            this.AllModVersionsTabPage = new System.Windows.Forms.TabPage();
            this.AllModVersions = new CKAN.MainAllModVersions();
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
            this.AllModVersionsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // ModInfoTabControl
            // 
            this.ModInfoTabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.ModInfoTabControl.Controls.Add(this.MetadataTabPage);
            this.ModInfoTabControl.Controls.Add(this.RelationshipTabPage);
            this.ModInfoTabControl.Controls.Add(this.ContentTabPage);
            this.ModInfoTabControl.Controls.Add(this.AllModVersionsTabPage);
            this.ModInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModInfoTabControl.Location = new System.Drawing.Point(0, 0);
            this.ModInfoTabControl.Name = "ModInfoTabControl";
            this.ModInfoTabControl.Size = new System.Drawing.Size(362, 531);
            this.ModInfoTabControl.TabIndex = 1;
            this.ModInfoTabControl.SelectedIndexChanged += new System.EventHandler(this.ModInfoIndexChanged);
            // 
            // MetadataTabPage
            // 
            this.MetadataTabPage.Controls.Add(this.splitContainer2);
            this.MetadataTabPage.Location = new System.Drawing.Point(4, 25);
            this.MetadataTabPage.Name = "MetadataTabPage";
            this.MetadataTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.MetadataTabPage.Size = new System.Drawing.Size(354, 502);
            this.MetadataTabPage.TabIndex = 0;
            this.MetadataTabPage.Text = "Metadata";
            this.MetadataTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
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
            this.splitContainer2.Size = new System.Drawing.Size(348, 496);
            this.splitContainer2.SplitterDistance = 235;
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
            this.MetaDataUpperLayoutPanel.Name = "MetaDataUpperLayoutPanel";
            this.MetaDataUpperLayoutPanel.RowCount = 2;
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.MetaDataUpperLayoutPanel.Size = new System.Drawing.Size(346, 233);
            this.MetaDataUpperLayoutPanel.TabIndex = 0;
            // 
            // MetadataModuleNameLabel
            // 
            this.MetadataModuleNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MetadataModuleNameLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.MetadataModuleNameLabel.Location = new System.Drawing.Point(3, 0);
            this.MetadataModuleNameLabel.Name = "MetadataModuleNameLabel";
            this.MetadataModuleNameLabel.Size = new System.Drawing.Size(340, 46);
            this.MetadataModuleNameLabel.TabIndex = 0;
            this.MetadataModuleNameLabel.Text = "Mod Name";
            this.MetadataModuleNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MetadataModuleAbstractLabel
            // 
            this.MetadataModuleAbstractLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataModuleAbstractLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAbstractLabel.Location = new System.Drawing.Point(3, 49);
            this.MetadataModuleAbstractLabel.Name = "MetadataModuleAbstractLabel";
            this.MetadataModuleAbstractLabel.ReadOnly = true;
            this.MetadataModuleAbstractLabel.Size = new System.Drawing.Size(340, 181);
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
            this.MetaDataLowerLayoutPanel.Name = "MetaDataLowerLayoutPanel";
            this.MetaDataLowerLayoutPanel.RowCount = 9;
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.MetaDataLowerLayoutPanel.Size = new System.Drawing.Size(346, 255);
            this.MetaDataLowerLayoutPanel.TabIndex = 0;
            // 
            // IdentifierLabel
            // 
            this.IdentifierLabel.AutoSize = true;
            this.IdentifierLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IdentifierLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.IdentifierLabel.Location = new System.Drawing.Point(3, 210);
            this.IdentifierLabel.Name = "IdentifierLabel";
            this.IdentifierLabel.Size = new System.Drawing.Size(84, 20);
            this.IdentifierLabel.TabIndex = 28;
            this.IdentifierLabel.Text = "Identifier";
            // 
            // MetadataIdentifierLabel
            // 
            this.MetadataIdentifierLabel.AutoSize = true;
            this.MetadataIdentifierLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataIdentifierLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataIdentifierLabel.Location = new System.Drawing.Point(93, 210);
            this.MetadataIdentifierLabel.Name = "MetadataIdentifierLabel";
            this.MetadataIdentifierLabel.Size = new System.Drawing.Size(250, 20);
            this.MetadataIdentifierLabel.TabIndex = 27;
            this.MetadataIdentifierLabel.Text = "-";
            // 
            // KSPCompatibilityLabel
            // 
            this.KSPCompatibilityLabel.AutoSize = true;
            this.KSPCompatibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.KSPCompatibilityLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.KSPCompatibilityLabel.Location = new System.Drawing.Point(3, 180);
            this.KSPCompatibilityLabel.Name = "KSPCompatibilityLabel";
            this.KSPCompatibilityLabel.Size = new System.Drawing.Size(84, 30);
            this.KSPCompatibilityLabel.TabIndex = 13;
            this.KSPCompatibilityLabel.Text = "Max KSP ver.:";
            // 
            // ReleaseLabel
            // 
            this.ReleaseLabel.AutoSize = true;
            this.ReleaseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReleaseLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ReleaseLabel.Location = new System.Drawing.Point(3, 150);
            this.ReleaseLabel.Name = "ReleaseLabel";
            this.ReleaseLabel.Size = new System.Drawing.Size(84, 30);
            this.ReleaseLabel.TabIndex = 12;
            this.ReleaseLabel.Text = "Release status:";
            // 
            // GitHubLabel
            // 
            this.GitHubLabel.AutoSize = true;
            this.GitHubLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitHubLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.GitHubLabel.Location = new System.Drawing.Point(3, 120);
            this.GitHubLabel.Name = "GitHubLabel";
            this.GitHubLabel.Size = new System.Drawing.Size(84, 30);
            this.GitHubLabel.TabIndex = 10;
            this.GitHubLabel.Text = "Source Code:";
            // 
            // HomePageLabel
            // 
            this.HomePageLabel.AutoSize = true;
            this.HomePageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HomePageLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.HomePageLabel.Location = new System.Drawing.Point(3, 90);
            this.HomePageLabel.Name = "HomePageLabel";
            this.HomePageLabel.Size = new System.Drawing.Size(84, 30);
            this.HomePageLabel.TabIndex = 7;
            this.HomePageLabel.Text = "Homepage:";
            // 
            // AuthorLabel
            // 
            this.AuthorLabel.AutoSize = true;
            this.AuthorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AuthorLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.AuthorLabel.Location = new System.Drawing.Point(3, 60);
            this.AuthorLabel.Name = "AuthorLabel";
            this.AuthorLabel.Size = new System.Drawing.Size(84, 30);
            this.AuthorLabel.TabIndex = 5;
            this.AuthorLabel.Text = "Author:";
            // 
            // LicenseLabel
            // 
            this.LicenseLabel.AutoSize = true;
            this.LicenseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LicenseLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.LicenseLabel.Location = new System.Drawing.Point(3, 30);
            this.LicenseLabel.Name = "LicenseLabel";
            this.LicenseLabel.Size = new System.Drawing.Size(84, 30);
            this.LicenseLabel.TabIndex = 3;
            this.LicenseLabel.Text = "License:";
            // 
            // MetadataModuleVersionLabel
            // 
            this.MetadataModuleVersionLabel.AutoSize = true;
            this.MetadataModuleVersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleVersionLabel.Location = new System.Drawing.Point(93, 0);
            this.MetadataModuleVersionLabel.Name = "MetadataModuleVersionLabel";
            this.MetadataModuleVersionLabel.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleVersionLabel.TabIndex = 2;
            this.MetadataModuleVersionLabel.Text = "0.0.0";
            // 
            // MetadataModuleLicenseLabel
            // 
            this.MetadataModuleLicenseLabel.AutoSize = true;
            this.MetadataModuleLicenseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleLicenseLabel.Location = new System.Drawing.Point(93, 30);
            this.MetadataModuleLicenseLabel.Name = "MetadataModuleLicenseLabel";
            this.MetadataModuleLicenseLabel.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleLicenseLabel.TabIndex = 4;
            this.MetadataModuleLicenseLabel.Text = "None";
            // 
            // MetadataModuleAuthorLabel
            // 
            this.MetadataModuleAuthorLabel.AutoSize = true;
            this.MetadataModuleAuthorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAuthorLabel.Location = new System.Drawing.Point(93, 60);
            this.MetadataModuleAuthorLabel.Name = "MetadataModuleAuthorLabel";
            this.MetadataModuleAuthorLabel.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleAuthorLabel.TabIndex = 6;
            this.MetadataModuleAuthorLabel.Text = "Nobody";
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VersionLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.VersionLabel.Location = new System.Drawing.Point(3, 0);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(84, 30);
            this.VersionLabel.TabIndex = 1;
            this.VersionLabel.Text = "Version:";
            // 
            // MetadataModuleReleaseStatusLabel
            // 
            this.MetadataModuleReleaseStatusLabel.AutoSize = true;
            this.MetadataModuleReleaseStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleReleaseStatusLabel.Location = new System.Drawing.Point(93, 150);
            this.MetadataModuleReleaseStatusLabel.Name = "MetadataModuleReleaseStatusLabel";
            this.MetadataModuleReleaseStatusLabel.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleReleaseStatusLabel.TabIndex = 11;
            this.MetadataModuleReleaseStatusLabel.Text = "Stable";
            // 
            // MetadataModuleHomePageLinkLabel
            // 
            this.MetadataModuleHomePageLinkLabel.AutoEllipsis = true;
            this.MetadataModuleHomePageLinkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleHomePageLinkLabel.Location = new System.Drawing.Point(93, 90);
            this.MetadataModuleHomePageLinkLabel.Name = "MetadataModuleHomePageLinkLabel";
            this.MetadataModuleHomePageLinkLabel.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleHomePageLinkLabel.TabIndex = 25;
            this.MetadataModuleHomePageLinkLabel.TabStop = true;
            this.MetadataModuleHomePageLinkLabel.Text = "linkLabel1";
            this.MetadataModuleHomePageLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // MetadataModuleKSPCompatibilityLabel
            // 
            this.MetadataModuleKSPCompatibilityLabel.AutoSize = true;
            this.MetadataModuleKSPCompatibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleKSPCompatibilityLabel.Location = new System.Drawing.Point(93, 180);
            this.MetadataModuleKSPCompatibilityLabel.Name = "MetadataModuleKSPCompatibilityLabel";
            this.MetadataModuleKSPCompatibilityLabel.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleKSPCompatibilityLabel.TabIndex = 14;
            this.MetadataModuleKSPCompatibilityLabel.Text = "0.0.0";
            // 
            // MetadataModuleGitHubLinkLabel
            // 
            this.MetadataModuleGitHubLinkLabel.AutoEllipsis = true;
            this.MetadataModuleGitHubLinkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleGitHubLinkLabel.Location = new System.Drawing.Point(93, 120);
            this.MetadataModuleGitHubLinkLabel.Name = "MetadataModuleGitHubLinkLabel";
            this.MetadataModuleGitHubLinkLabel.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleGitHubLinkLabel.TabIndex = 26;
            this.MetadataModuleGitHubLinkLabel.TabStop = true;
            this.MetadataModuleGitHubLinkLabel.Text = "linkLabel2";
            this.MetadataModuleGitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // RelationshipTabPage
            // 
            this.RelationshipTabPage.Controls.Add(this.ModuleRelationshipType);
            this.RelationshipTabPage.Controls.Add(this.DependsGraphTree);
            this.RelationshipTabPage.Location = new System.Drawing.Point(4, 25);
            this.RelationshipTabPage.Name = "RelationshipTabPage";
            this.RelationshipTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.RelationshipTabPage.Size = new System.Drawing.Size(354, 502);
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
            this.ModuleRelationshipType.Size = new System.Drawing.Size(185, 21);
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
            this.DependsGraphTree.Size = new System.Drawing.Size(345, 462);
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
            this.ContentTabPage.Size = new System.Drawing.Size(354, 502);
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
            this.ContentsPreviewTree.Location = new System.Drawing.Point(6, 65);
            this.ContentsPreviewTree.Name = "ContentsPreviewTree";
            this.ContentsPreviewTree.Size = new System.Drawing.Size(342, 434);
            this.ContentsPreviewTree.TabIndex = 2;
            this.ContentsPreviewTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ContentsPreviewTree_NodeMouseDoubleClick);
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
            // AllModVersionsTabPage
            // 
            this.AllModVersionsTabPage.Controls.Add(this.AllModVersions);
            this.AllModVersionsTabPage.Location = new System.Drawing.Point(4, 25);
            this.AllModVersionsTabPage.Margin = new System.Windows.Forms.Padding(2);
            this.AllModVersionsTabPage.Name = "AllModVersionsTabPage";
            this.AllModVersionsTabPage.Size = new System.Drawing.Size(354, 502);
            this.AllModVersionsTabPage.TabIndex = 1;
            this.AllModVersionsTabPage.Text = "Versions";
            this.AllModVersionsTabPage.UseVisualStyleBackColor = true;
            // 
            // AllModVersions
            // 
            this.AllModVersions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AllModVersions.Location = new System.Drawing.Point(0, 0);
            this.AllModVersions.Margin = new System.Windows.Forms.Padding(2);
            this.AllModVersions.Name = "AllModVersions";
            this.AllModVersions.Size = new System.Drawing.Size(354, 502);
            this.AllModVersions.TabIndex = 0;
            // 
            // MainModInfoTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ModInfoTabControl);
            this.Name = "MainModInfoTab";
            this.Size = new System.Drawing.Size(362, 531);
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
            this.AllModVersionsTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl ModInfoTabControl;
        private System.Windows.Forms.TabPage MetadataTabPage;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TableLayoutPanel MetaDataUpperLayoutPanel;
        private System.Windows.Forms.Label MetadataModuleNameLabel;
        private System.Windows.Forms.RichTextBox MetadataModuleAbstractLabel;
        private System.Windows.Forms.TableLayoutPanel MetaDataLowerLayoutPanel;
        private System.Windows.Forms.Label IdentifierLabel;
        private System.Windows.Forms.Label MetadataIdentifierLabel;
        private System.Windows.Forms.Label KSPCompatibilityLabel;
        private System.Windows.Forms.Label ReleaseLabel;
        private System.Windows.Forms.Label GitHubLabel;
        private System.Windows.Forms.Label HomePageLabel;
        private System.Windows.Forms.Label AuthorLabel;
        private System.Windows.Forms.Label LicenseLabel;
        private System.Windows.Forms.Label MetadataModuleVersionLabel;
        private System.Windows.Forms.Label MetadataModuleLicenseLabel;
        private System.Windows.Forms.Label MetadataModuleAuthorLabel;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Label MetadataModuleReleaseStatusLabel;
        private System.Windows.Forms.LinkLabel MetadataModuleHomePageLinkLabel;
        private System.Windows.Forms.Label MetadataModuleKSPCompatibilityLabel;
        private System.Windows.Forms.LinkLabel MetadataModuleGitHubLinkLabel;
        private System.Windows.Forms.TabPage RelationshipTabPage;
        private System.Windows.Forms.ComboBox ModuleRelationshipType;
        private System.Windows.Forms.TreeView DependsGraphTree;
        private System.Windows.Forms.TabPage ContentTabPage;
        private System.Windows.Forms.TreeView ContentsPreviewTree;
        private System.Windows.Forms.Button ContentsDownloadButton;
        private System.Windows.Forms.Label NotCachedLabel;
        private System.Windows.Forms.TabPage AllModVersionsTabPage;
        private MainAllModVersions AllModVersions;
    }
}
