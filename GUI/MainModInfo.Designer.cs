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
            this.MetadataModuleUrlLabel = new System.Windows.Forms.LinkLabel();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.UrlLabel = new System.Windows.Forms.Label();
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
            this.IdentifierLabel = new System.Windows.Forms.Label();
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
            this.ModInfoTabControl.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ModInfoTabControl.Name = "ModInfoTabControl";
            this.ModInfoTabControl.SelectedIndex = 0;
            this.ModInfoTabControl.Size = new System.Drawing.Size(724, 1021);
            this.ModInfoTabControl.TabIndex = 1;
            this.ModInfoTabControl.SelectedIndexChanged += new System.EventHandler(this.ModInfoIndexChanged);
            // 
            // MetadataTabPage
            // 
            this.MetadataTabPage.Controls.Add(this.splitContainer2);
            this.MetadataTabPage.Location = new System.Drawing.Point(4, 37);
            this.MetadataTabPage.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MetadataTabPage.Name = "MetadataTabPage";
            this.MetadataTabPage.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MetadataTabPage.Size = new System.Drawing.Size(716, 980);
            this.MetadataTabPage.TabIndex = 0;
            this.MetadataTabPage.Text = "Metadata";
            this.MetadataTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(6, 6);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
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
            this.splitContainer2.Size = new System.Drawing.Size(704, 968);
            this.splitContainer2.SplitterDistance = 458;
            this.splitContainer2.SplitterWidth = 8;
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
            this.MetaDataUpperLayoutPanel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MetaDataUpperLayoutPanel.Name = "MetaDataUpperLayoutPanel";
            this.MetaDataUpperLayoutPanel.RowCount = 2;
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.MetaDataUpperLayoutPanel.Size = new System.Drawing.Size(702, 456);
            this.MetaDataUpperLayoutPanel.TabIndex = 0;
            // 
            // MetadataModuleNameLabel
            // 
            this.MetadataModuleNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MetadataModuleNameLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.MetadataModuleNameLabel.Location = new System.Drawing.Point(6, 0);
            this.MetadataModuleNameLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MetadataModuleNameLabel.Name = "MetadataModuleNameLabel";
            this.MetadataModuleNameLabel.Size = new System.Drawing.Size(690, 91);
            this.MetadataModuleNameLabel.TabIndex = 0;
            this.MetadataModuleNameLabel.Text = "Mod Name";
            this.MetadataModuleNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MetadataModuleAbstractLabel
            // 
            this.MetadataModuleAbstractLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataModuleAbstractLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAbstractLabel.Location = new System.Drawing.Point(6, 97);
            this.MetadataModuleAbstractLabel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MetadataModuleAbstractLabel.Name = "MetadataModuleAbstractLabel";
            this.MetadataModuleAbstractLabel.ReadOnly = true;
            this.MetadataModuleAbstractLabel.Size = new System.Drawing.Size(690, 353);
            this.MetadataModuleAbstractLabel.TabIndex = 27;
            this.MetadataModuleAbstractLabel.Text = "";
            // 
            // MetaDataLowerLayoutPanel
            // 
            this.MetaDataLowerLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MetaDataLowerLayoutPanel.ColumnCount = 2;
            this.MetaDataLowerLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.16279F));
            this.MetaDataLowerLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.83721F));
            this.MetaDataLowerLayoutPanel.Controls.Add(this.IdentifierLabel, 0, 8);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataIdentifierLabel, 0, 8);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.KSPCompatibilityLabel, 0, 7);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.ReleaseLabel, 0, 6);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.GitHubLabel, 0, 4);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.HomePageLabel, 0, 3);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.AuthorLabel, 0, 2);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.LicenseLabel, 0, 1);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleVersionLabel, 1, 0);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleLicenseLabel, 1, 1);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleAuthorLabel, 1, 2);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleUrlLabel, 1, 5);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.UrlLabel, 0, 5);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.VersionLabel, 0, 0);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleReleaseStatusLabel, 1, 6);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleHomePageLinkLabel, 1, 3);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleKSPCompatibilityLabel, 1, 7);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleGitHubLinkLabel, 1, 4);
            this.MetaDataLowerLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetaDataLowerLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.MetaDataLowerLayoutPanel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MetaDataLowerLayoutPanel.Name = "MetaDataLowerLayoutPanel";
            this.MetaDataLowerLayoutPanel.RowCount = 9;
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MetaDataLowerLayoutPanel.Size = new System.Drawing.Size(702, 500);
            this.MetaDataLowerLayoutPanel.TabIndex = 0;
            // 
            // MetadataIdentifierLabel
            // 
            this.MetadataIdentifierLabel.AutoSize = true;
            this.MetadataIdentifierLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataIdentifierLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataIdentifierLabel.Location = new System.Drawing.Point(189, 406);
            this.MetadataIdentifierLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MetadataIdentifierLabel.Name = "MetadataIdentifierLabel";
            this.MetadataIdentifierLabel.Size = new System.Drawing.Size(507, 38);
            this.MetadataIdentifierLabel.TabIndex = 27;
            this.MetadataIdentifierLabel.Text = "-";
            // 
            // KSPCompatibilityLabel
            // 
            this.KSPCompatibilityLabel.AutoSize = true;
            this.KSPCompatibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.KSPCompatibilityLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.KSPCompatibilityLabel.Location = new System.Drawing.Point(6, 348);
            this.KSPCompatibilityLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.KSPCompatibilityLabel.Name = "KSPCompatibilityLabel";
            this.KSPCompatibilityLabel.Size = new System.Drawing.Size(171, 58);
            this.KSPCompatibilityLabel.TabIndex = 13;
            this.KSPCompatibilityLabel.Text = "Max KSP ver.:";
            // 
            // ReleaseLabel
            // 
            this.ReleaseLabel.AutoSize = true;
            this.ReleaseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReleaseLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ReleaseLabel.Location = new System.Drawing.Point(6, 290);
            this.ReleaseLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ReleaseLabel.Name = "ReleaseLabel";
            this.ReleaseLabel.Size = new System.Drawing.Size(171, 58);
            this.ReleaseLabel.TabIndex = 12;
            this.ReleaseLabel.Text = "Release status:";
            // 
            // GitHubLabel
            // 
            this.GitHubLabel.AutoSize = true;
            this.GitHubLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GitHubLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.GitHubLabel.Location = new System.Drawing.Point(6, 232);
            this.GitHubLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.GitHubLabel.Name = "GitHubLabel";
            this.GitHubLabel.Size = new System.Drawing.Size(171, 58);
            this.GitHubLabel.TabIndex = 10;
            this.GitHubLabel.Text = "Source Code:";
            // 
            // HomePageLabel
            // 
            this.HomePageLabel.AutoSize = true;
            this.HomePageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HomePageLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.HomePageLabel.Location = new System.Drawing.Point(6, 174);
            this.HomePageLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.HomePageLabel.Name = "HomePageLabel";
            this.HomePageLabel.Size = new System.Drawing.Size(171, 58);
            this.HomePageLabel.TabIndex = 7;
            this.HomePageLabel.Text = "Homepage:";
            // 
            // AuthorLabel
            // 
            this.AuthorLabel.AutoSize = true;
            this.AuthorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AuthorLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.AuthorLabel.Location = new System.Drawing.Point(6, 116);
            this.AuthorLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.AuthorLabel.Name = "AuthorLabel";
            this.AuthorLabel.Size = new System.Drawing.Size(171, 58);
            this.AuthorLabel.TabIndex = 5;
            this.AuthorLabel.Text = "Author:";
            // 
            // LicenseLabel
            // 
            this.LicenseLabel.AutoSize = true;
            this.LicenseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LicenseLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.LicenseLabel.Location = new System.Drawing.Point(6, 58);
            this.LicenseLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LicenseLabel.Name = "LicenseLabel";
            this.LicenseLabel.Size = new System.Drawing.Size(171, 58);
            this.LicenseLabel.TabIndex = 3;
            this.LicenseLabel.Text = "License:";
            //
            // MetadataModuleUrlLabel
            //
            this.MetadataModuleUrlLabel.AutoSize = true;
            this.MetadataModuleUrlLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleUrlLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.MetadataModuleUrlLabel.Location = new System.Drawing.Point(6, 444);
            this.MetadataModuleUrlLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MetadataModuleUrlLabel.Name = "MetadataModuleUrlLabel";
            this.MetadataModuleUrlLabel.Size = new System.Drawing.Size(171, 58);
            this.MetadataModuleUrlLabel.TabIndex = 3;
            this.MetadataModuleUrlLabel.Text = "linkLabel3";
            this.MetadataModuleUrlLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // MetadataModuleVersionLabel
            // 
            this.MetadataModuleVersionLabel.AutoSize = true;
            this.MetadataModuleVersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleVersionLabel.Location = new System.Drawing.Point(189, 0);
            this.MetadataModuleVersionLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MetadataModuleVersionLabel.Name = "MetadataModuleVersionLabel";
            this.MetadataModuleVersionLabel.Size = new System.Drawing.Size(507, 58);
            this.MetadataModuleVersionLabel.TabIndex = 2;
            this.MetadataModuleVersionLabel.Text = "0.0.0";
            // 
            // MetadataModuleLicenseLabel
            // 
            this.MetadataModuleLicenseLabel.AutoSize = true;
            this.MetadataModuleLicenseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleLicenseLabel.Location = new System.Drawing.Point(189, 58);
            this.MetadataModuleLicenseLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MetadataModuleLicenseLabel.Name = "MetadataModuleLicenseLabel";
            this.MetadataModuleLicenseLabel.Size = new System.Drawing.Size(507, 58);
            this.MetadataModuleLicenseLabel.TabIndex = 4;
            this.MetadataModuleLicenseLabel.Text = "None";
            // 
            // MetadataModuleAuthorLabel
            // 
            this.MetadataModuleAuthorLabel.AutoSize = true;
            this.MetadataModuleAuthorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAuthorLabel.Location = new System.Drawing.Point(189, 116);
            this.MetadataModuleAuthorLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MetadataModuleAuthorLabel.Name = "MetadataModuleAuthorLabel";
            this.MetadataModuleAuthorLabel.Size = new System.Drawing.Size(507, 58);
            this.MetadataModuleAuthorLabel.TabIndex = 6;
            this.MetadataModuleAuthorLabel.Text = "Nobody";
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VersionLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.VersionLabel.Location = new System.Drawing.Point(6, 0);
            this.VersionLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(171, 58);
            this.VersionLabel.TabIndex = 1;
            this.VersionLabel.Text = "Version:";
            // 
            // UrlLabel
            // 
            this.UrlLabel.AutoSize = true;
            this.UrlLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UrlLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.UrlLabel.Location = new System.Drawing.Point(6, 0);
            this.UrlLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.UrlLabel.Name = "UrlLabel";
            this.UrlLabel.Size = new System.Drawing.Size(171, 58);
            this.UrlLabel.TabIndex = 1;
            this.UrlLabel.Text = "Package URL:";
            // 
            // MetadataModuleReleaseStatusLabel
            // 
            this.MetadataModuleReleaseStatusLabel.AutoSize = true;
            this.MetadataModuleReleaseStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleReleaseStatusLabel.Location = new System.Drawing.Point(189, 290);
            this.MetadataModuleReleaseStatusLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MetadataModuleReleaseStatusLabel.Name = "MetadataModuleReleaseStatusLabel";
            this.MetadataModuleReleaseStatusLabel.Size = new System.Drawing.Size(507, 58);
            this.MetadataModuleReleaseStatusLabel.TabIndex = 11;
            this.MetadataModuleReleaseStatusLabel.Text = "Stable";
            // 
            // MetadataModuleHomePageLinkLabel
            // 
            this.MetadataModuleHomePageLinkLabel.AutoEllipsis = true;
            this.MetadataModuleHomePageLinkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleHomePageLinkLabel.Location = new System.Drawing.Point(189, 174);
            this.MetadataModuleHomePageLinkLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MetadataModuleHomePageLinkLabel.Name = "MetadataModuleHomePageLinkLabel";
            this.MetadataModuleHomePageLinkLabel.Size = new System.Drawing.Size(507, 58);
            this.MetadataModuleHomePageLinkLabel.TabIndex = 25;
            this.MetadataModuleHomePageLinkLabel.TabStop = true;
            this.MetadataModuleHomePageLinkLabel.Text = "linkLabel1";
            this.MetadataModuleHomePageLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // MetadataModuleKSPCompatibilityLabel
            // 
            this.MetadataModuleKSPCompatibilityLabel.AutoSize = true;
            this.MetadataModuleKSPCompatibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleKSPCompatibilityLabel.Location = new System.Drawing.Point(189, 348);
            this.MetadataModuleKSPCompatibilityLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MetadataModuleKSPCompatibilityLabel.Name = "MetadataModuleKSPCompatibilityLabel";
            this.MetadataModuleKSPCompatibilityLabel.Size = new System.Drawing.Size(507, 58);
            this.MetadataModuleKSPCompatibilityLabel.TabIndex = 14;
            this.MetadataModuleKSPCompatibilityLabel.Text = "0.0.0";
            // 
            // MetadataModuleGitHubLinkLabel
            // 
            this.MetadataModuleGitHubLinkLabel.AutoEllipsis = true;
            this.MetadataModuleGitHubLinkLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleGitHubLinkLabel.Location = new System.Drawing.Point(189, 232);
            this.MetadataModuleGitHubLinkLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MetadataModuleGitHubLinkLabel.Name = "MetadataModuleGitHubLinkLabel";
            this.MetadataModuleGitHubLinkLabel.Size = new System.Drawing.Size(507, 58);
            this.MetadataModuleGitHubLinkLabel.TabIndex = 26;
            this.MetadataModuleGitHubLinkLabel.TabStop = true;
            this.MetadataModuleGitHubLinkLabel.Text = "linkLabel2";
            this.MetadataModuleGitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // RelationshipTabPage
            // 
            this.RelationshipTabPage.Controls.Add(this.ModuleRelationshipType);
            this.RelationshipTabPage.Controls.Add(this.DependsGraphTree);
            this.RelationshipTabPage.Location = new System.Drawing.Point(4, 37);
            this.RelationshipTabPage.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.RelationshipTabPage.Name = "RelationshipTabPage";
            this.RelationshipTabPage.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.RelationshipTabPage.Size = new System.Drawing.Size(716, 980);
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
            this.ModuleRelationshipType.Location = new System.Drawing.Point(12, 13);
            this.ModuleRelationshipType.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ModuleRelationshipType.Name = "ModuleRelationshipType";
            this.ModuleRelationshipType.Size = new System.Drawing.Size(366, 33);
            this.ModuleRelationshipType.TabIndex = 1;
            this.ModuleRelationshipType.SelectedIndexChanged += new System.EventHandler(this.ModuleRelationshipType_SelectedIndexChanged);
            // 
            // DependsGraphTree
            // 
            this.DependsGraphTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DependsGraphTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DependsGraphTree.Location = new System.Drawing.Point(6, 65);
            this.DependsGraphTree.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.DependsGraphTree.Name = "DependsGraphTree";
            this.DependsGraphTree.Size = new System.Drawing.Size(688, 887);
            this.DependsGraphTree.TabIndex = 0;
            this.DependsGraphTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.DependsGraphTree_NodeMouseDoubleClick);
            // 
            // ContentTabPage
            // 
            this.ContentTabPage.Controls.Add(this.ContentsPreviewTree);
            this.ContentTabPage.Controls.Add(this.ContentsDownloadButton);
            this.ContentTabPage.Controls.Add(this.NotCachedLabel);
            this.ContentTabPage.Location = new System.Drawing.Point(4, 37);
            this.ContentTabPage.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ContentTabPage.Name = "ContentTabPage";
            this.ContentTabPage.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ContentTabPage.Size = new System.Drawing.Size(716, 980);
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
            this.ContentsPreviewTree.Location = new System.Drawing.Point(12, 125);
            this.ContentsPreviewTree.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ContentsPreviewTree.Name = "ContentsPreviewTree";
            this.ContentsPreviewTree.Size = new System.Drawing.Size(682, 833);
            this.ContentsPreviewTree.TabIndex = 2;
            this.ContentsPreviewTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ContentsPreviewTree_NodeMouseDoubleClick);
            // 
            // ContentsDownloadButton
            // 
            this.ContentsDownloadButton.Location = new System.Drawing.Point(12, 69);
            this.ContentsDownloadButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ContentsDownloadButton.Name = "ContentsDownloadButton";
            this.ContentsDownloadButton.Size = new System.Drawing.Size(206, 44);
            this.ContentsDownloadButton.TabIndex = 1;
            this.ContentsDownloadButton.Text = "Download";
            this.ContentsDownloadButton.UseVisualStyleBackColor = true;
            this.ContentsDownloadButton.Click += new System.EventHandler(this.ContentsDownloadButton_Click);
            // 
            // NotCachedLabel
            // 
            this.NotCachedLabel.Location = new System.Drawing.Point(6, 6);
            this.NotCachedLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.NotCachedLabel.Name = "NotCachedLabel";
            this.NotCachedLabel.Size = new System.Drawing.Size(432, 58);
            this.NotCachedLabel.TabIndex = 0;
            this.NotCachedLabel.Text = "This mod is not in the cache, click \'Download\' to preview contents";
            // 
            // AllModVersionsTabPage
            // 
            this.AllModVersionsTabPage.Controls.Add(this.AllModVersions);
            this.AllModVersionsTabPage.Location = new System.Drawing.Point(4, 37);
            this.AllModVersionsTabPage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AllModVersionsTabPage.Name = "AllModVersionsTabPage";
            this.AllModVersionsTabPage.Size = new System.Drawing.Size(716, 980);
            this.AllModVersionsTabPage.TabIndex = 1;
            this.AllModVersionsTabPage.Text = "Versions";
            this.AllModVersionsTabPage.UseVisualStyleBackColor = true;
            // 
            // AllModVersions
            // 
            this.AllModVersions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AllModVersions.Location = new System.Drawing.Point(0, 0);
            this.AllModVersions.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AllModVersions.Name = "AllModVersions";
            this.AllModVersions.Size = new System.Drawing.Size(716, 980);
            this.AllModVersions.TabIndex = 0;
            // 
            // IdentifierLabel
            // 
            this.IdentifierLabel.AutoSize = true;
            this.IdentifierLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IdentifierLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.IdentifierLabel.Location = new System.Drawing.Point(6, 406);
            this.IdentifierLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.IdentifierLabel.Name = "IdentifierLabel";
            this.IdentifierLabel.Size = new System.Drawing.Size(171, 38);
            this.IdentifierLabel.TabIndex = 28;
            this.IdentifierLabel.Text = "Identifier";
            // 
            // MainModInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ModInfoTabControl);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "MainModInfo";
            this.Size = new System.Drawing.Size(724, 1021);
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
        private System.Windows.Forms.Label MetadataIdentifierLabel;
        private System.Windows.Forms.Label KSPCompatibilityLabel;
        private System.Windows.Forms.Label ReleaseLabel;
        private System.Windows.Forms.Label GitHubLabel;
        private System.Windows.Forms.Label HomePageLabel;
        private System.Windows.Forms.Label AuthorLabel;
        private System.Windows.Forms.Label LicenseLabel;
        private System.Windows.Forms.Label UrlLabel;
        private System.Windows.Forms.Label MetadataModuleVersionLabel;
        private System.Windows.Forms.Label MetadataModuleLicenseLabel;
        private System.Windows.Forms.Label MetadataModuleAuthorLabel;
        private System.Windows.Forms.LinkLabel MetadataModuleUrlLabel;
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
        private System.Windows.Forms.Label IdentifierLabel;
    }
}
