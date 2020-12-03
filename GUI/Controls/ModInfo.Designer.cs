namespace CKAN
{
    partial class ModInfo
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(ModInfo));
            this.ModInfoTabControl = new ThemedTabControl();
            this.MetadataTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.MetaDataUpperLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.MetadataModuleNameTextBox = new TransparentTextBox();
            this.MetadataTagsLabelsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.MetadataModuleAbstractLabel = new System.Windows.Forms.Label();
            this.MetadataModuleDescriptionTextBox = new TransparentTextBox();
            this.MetaDataLowerLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.IdentifierLabel = new System.Windows.Forms.Label();
            this.MetadataIdentifierTextBox = new TransparentTextBox();
            this.ReplacementLabel = new System.Windows.Forms.Label();
            this.ReplacementTextBox = new TransparentTextBox();
            this.GameCompatibilityLabel = new System.Windows.Forms.Label();
            this.ReleaseLabel = new System.Windows.Forms.Label();
            this.AuthorLabel = new System.Windows.Forms.Label();
            this.LicenseLabel = new System.Windows.Forms.Label();
            this.MetadataModuleVersionTextBox = new TransparentTextBox();
            this.MetadataModuleLicenseTextBox = new TransparentTextBox();
            this.MetadataModuleAuthorTextBox = new TransparentTextBox();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.MetadataModuleReleaseStatusTextBox = new TransparentTextBox();
            this.MetadataModuleGameCompatibilityTextBox = new TransparentTextBox();
            this.RelationshipTabPage = new System.Windows.Forms.TabPage();
            this.DependsGraphTree = new System.Windows.Forms.TreeView();
            this.LegendDependsImage = new System.Windows.Forms.PictureBox();
            this.LegendRecommendsImage = new System.Windows.Forms.PictureBox();
            this.LegendSuggestsImage = new System.Windows.Forms.PictureBox();
            this.LegendSupportsImage = new System.Windows.Forms.PictureBox();
            this.LegendConflictsImage = new System.Windows.Forms.PictureBox();
            this.LegendDependsLabel = new System.Windows.Forms.Label();
            this.LegendRecommendsLabel = new System.Windows.Forms.Label();
            this.LegendSuggestsLabel = new System.Windows.Forms.Label();
            this.LegendSupportsLabel = new System.Windows.Forms.Label();
            this.LegendConflictsLabel = new System.Windows.Forms.Label();
            this.ContentTabPage = new System.Windows.Forms.TabPage();
            this.ContentsPreviewTree = new System.Windows.Forms.TreeView();
            this.ContentsDownloadButton = new System.Windows.Forms.Button();
            this.ContentsOpenButton = new System.Windows.Forms.Button();
            this.NotCachedLabel = new System.Windows.Forms.Label();
            this.AllModVersionsTabPage = new System.Windows.Forms.TabPage();
            this.AllModVersions = new CKAN.AllModVersions();
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
            this.AllModVersions.SuspendLayout();
            this.AllModVersionsTabPage.SuspendLayout();
            this.SuspendLayout();
            //
            // ModInfoTabControl
            //
            this.ModInfoTabControl.Appearance = System.Windows.Forms.TabAppearance.Normal;
            this.ModInfoTabControl.BackColor = System.Drawing.SystemColors.Control;
            this.ModInfoTabControl.Multiline = true;
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
            this.MetadataTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.MetadataTabPage.Controls.Add(this.MetaDataLowerLayoutPanel);
            this.MetadataTabPage.Location = new System.Drawing.Point(4, 25);
            this.MetadataTabPage.Name = "MetadataTabPage";
            this.MetadataTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.MetadataTabPage.Size = new System.Drawing.Size(354, 502);
            this.MetadataTabPage.TabIndex = 0;
            resources.ApplyResources(this.MetadataTabPage, "MetadataTabPage");
            //
            // splitContainer2
            //
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitContainer2.Panel1
            //
            this.splitContainer2.Panel1.Controls.Add(this.MetaDataUpperLayoutPanel);
            this.splitContainer2.Panel1MinSize = 75;
            //
            // splitContainer2.Panel2
            //
            this.splitContainer2.Panel2.Controls.Add(this.ModInfoTabControl);
            this.splitContainer2.Panel2MinSize = 225;
            this.splitContainer2.Size = new System.Drawing.Size(348, 496);
            this.splitContainer2.SplitterWidth = 10;
            this.splitContainer2.SplitterDistance = 235;
            this.splitContainer2.TabIndex = 0;
            //
            // MetaDataUpperLayoutPanel
            //
            this.MetaDataUpperLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MetaDataUpperLayoutPanel.ColumnCount = 1;
            this.MetaDataUpperLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MetaDataUpperLayoutPanel.Controls.Add(this.MetadataModuleNameTextBox, 0, 0);
            this.MetaDataUpperLayoutPanel.Controls.Add(this.MetadataTagsLabelsPanel, 0, 1);
            this.MetaDataUpperLayoutPanel.Controls.Add(this.MetadataModuleAbstractLabel, 0, 2);
            this.MetaDataUpperLayoutPanel.Controls.Add(this.MetadataModuleDescriptionTextBox, 0, 3);
            this.MetaDataUpperLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetaDataUpperLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.MetaDataUpperLayoutPanel.Name = "MetaDataUpperLayoutPanel";
            this.MetaDataUpperLayoutPanel.RowCount = 4;
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 20F));
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 30F));
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.MetaDataUpperLayoutPanel.Size = new System.Drawing.Size(346, 283);
            this.MetaDataUpperLayoutPanel.TabIndex = 0;
            //
            // MetadataModuleNameTextBox
            //
            this.MetadataModuleNameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MetadataModuleNameTextBox.Location = new System.Drawing.Point(3, 0);
            this.MetadataModuleNameTextBox.Name = "MetadataModuleNameTextBox";
            this.MetadataModuleNameTextBox.Size = new System.Drawing.Size(340, 46);
            this.MetadataModuleNameTextBox.TabIndex = 0;
            this.MetadataModuleNameTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MetadataModuleNameTextBox.ReadOnly = true;
            this.MetadataModuleNameTextBox.BackColor = MetaDataUpperLayoutPanel.BackColor;
            this.MetadataModuleNameTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.MetadataModuleNameTextBox, "MetadataModuleNameTextBox");
            // 
            // MetadataTagsLabelsPanel
            // 
            this.MetadataTagsLabelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataTagsLabelsPanel.Padding = new System.Windows.Forms.Padding(0);
            this.MetadataTagsLabelsPanel.Location = new System.Drawing.Point(0, 0);
            this.MetadataTagsLabelsPanel.Name = "MetadataTagsLabelsPanel";
            this.MetadataTagsLabelsPanel.Size = new System.Drawing.Size(340, 12);
            //
            // MetadataModuleAbstractLabel
            //
            this.MetadataModuleAbstractLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAbstractLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleAbstractLabel.Location = new System.Drawing.Point(3, 49);
            this.MetadataModuleAbstractLabel.Name = "MetadataModuleAbstractLabel";
            this.MetadataModuleAbstractLabel.Size = new System.Drawing.Size(340, 61);
            this.MetadataModuleAbstractLabel.AutoSize = true;
            this.MetadataModuleAbstractLabel.TabIndex = 27;
            this.MetadataModuleAbstractLabel.Text = "";
            //
            // MetadataModuleDescriptionTextBox
            //
            this.MetadataModuleDescriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleDescriptionTextBox.Location = new System.Drawing.Point(3, 129);
            this.MetadataModuleDescriptionTextBox.Name = "MetadataModuleDescriptionLabel";
            this.MetadataModuleDescriptionTextBox.Size = new System.Drawing.Size(340, 121);
            this.MetadataModuleDescriptionTextBox.TabIndex = 28;
            this.MetadataModuleDescriptionTextBox.Text = "";
            this.MetadataModuleDescriptionTextBox.ReadOnly = true;
            this.MetadataModuleDescriptionTextBox.BackColor = MetaDataUpperLayoutPanel.BackColor;
            this.MetadataModuleDescriptionTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleDescriptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            //
            // MetaDataLowerLayoutPanel
            //
            this.MetaDataLowerLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MetaDataLowerLayoutPanel.ColumnCount = 2;
            this.MetaDataLowerLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.MetaDataLowerLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetaDataLowerLayoutPanel.Controls.Add(this.VersionLabel, 0, 0);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.LicenseLabel, 0, 1);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.AuthorLabel, 0, 2);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.ReleaseLabel, 0, 3);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.GameCompatibilityLabel, 0, 4);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.IdentifierLabel, 0, 5);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.ReplacementLabel, 0, 6);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleVersionTextBox, 1, 0);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleLicenseTextBox, 1, 1);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleAuthorTextBox, 1, 2);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleReleaseStatusTextBox, 1, 3);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataModuleGameCompatibilityTextBox, 1, 4);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.MetadataIdentifierTextBox, 1, 5);
            this.MetaDataLowerLayoutPanel.Controls.Add(this.ReplacementTextBox, 1, 6);
            this.MetaDataLowerLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetaDataLowerLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.MetaDataLowerLayoutPanel.Name = "MetaDataLowerLayoutPanel";
            this.MetaDataLowerLayoutPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.MetaDataLowerLayoutPanel.RowCount = 7;
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetaDataLowerLayoutPanel.Size = new System.Drawing.Size(346, 255);
            this.MetaDataLowerLayoutPanel.AutoSize = true;
            this.MetaDataLowerLayoutPanel.AutoScroll = true;
            this.MetaDataLowerLayoutPanel.AutoScrollMinSize = new System.Drawing.Size(MetaDataLowerLayoutPanel.Width, MetaDataLowerLayoutPanel.Height + 20);
            this.MetaDataLowerLayoutPanel.VerticalScroll.Enabled = true;
            this.MetaDataLowerLayoutPanel.HorizontalScroll.Enabled = true;
            this.MetaDataLowerLayoutPanel.TabIndex = 0;
            //
            // IdentifierLabel
            //
            this.IdentifierLabel.AutoSize = true;
            this.IdentifierLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IdentifierLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.IdentifierLabel.Location = new System.Drawing.Point(3, 210);
            this.IdentifierLabel.Name = "IdentifierLabel";
            this.IdentifierLabel.Size = new System.Drawing.Size(84, 20);
            this.IdentifierLabel.TabIndex = 28;
            resources.ApplyResources(this.IdentifierLabel, "IdentifierLabel");
            //
            // MetadataIdentifierTextBox
            //
            this.MetadataIdentifierTextBox.AutoSize = true;
            this.MetadataIdentifierTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataIdentifierTextBox.Location = new System.Drawing.Point(93, 210);
            this.MetadataIdentifierTextBox.Name = "MetadataIdentifierTextBox";
            this.MetadataIdentifierTextBox.Size = new System.Drawing.Size(250, 20);
            this.MetadataIdentifierTextBox.TabIndex = 27;
            this.MetadataIdentifierTextBox.ReadOnly = true;
            this.MetadataIdentifierTextBox.BackColor = MetadataTabPage.BackColor;
            this.MetadataIdentifierTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataIdentifierTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.MetadataIdentifierTextBox, "MetadataIdentifierTextBox");
            //
            // ReplacementLabel
            //
            this.ReplacementLabel.AutoSize = true;
            this.ReplacementLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReplacementLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.ReplacementLabel.Location = new System.Drawing.Point(3, 240);
            this.ReplacementLabel.Name = "ReplacementLabel";
            this.ReplacementLabel.Size = new System.Drawing.Size(84, 20);
            this.ReplacementLabel.TabIndex = 28;
            resources.ApplyResources(this.ReplacementLabel, "ReplacementLabel");
            //
            // ReplacementTextBox
            //
            this.ReplacementTextBox.AutoSize = true;
            this.ReplacementTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReplacementTextBox.Location = new System.Drawing.Point(93, 240);
            this.ReplacementTextBox.Name = "ReplacementTextBox";
            this.ReplacementTextBox.Size = new System.Drawing.Size(250, 20);
            this.ReplacementTextBox.TabIndex = 27;
            this.ReplacementTextBox.ReadOnly = true;
            this.ReplacementTextBox.BackColor = MetadataTabPage.BackColor;
            this.ReplacementTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ReplacementTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.ReplacementTextBox, "ReplacementTextBox");
            //
            // GameCompatibilityLabel
            //
            this.GameCompatibilityLabel.AutoSize = true;
            this.GameCompatibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GameCompatibilityLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.GameCompatibilityLabel.Location = new System.Drawing.Point(3, 180);
            this.GameCompatibilityLabel.Name = "GameCompatibilityLabel";
            this.GameCompatibilityLabel.Size = new System.Drawing.Size(84, 30);
            this.GameCompatibilityLabel.TabIndex = 13;
            resources.ApplyResources(this.GameCompatibilityLabel, "GameCompatibilityLabel");
            //
            // ReleaseLabel
            //
            this.ReleaseLabel.AutoSize = true;
            this.ReleaseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReleaseLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.ReleaseLabel.Location = new System.Drawing.Point(3, 150);
            this.ReleaseLabel.Name = "ReleaseLabel";
            this.ReleaseLabel.Size = new System.Drawing.Size(84, 30);
            this.ReleaseLabel.TabIndex = 12;
            resources.ApplyResources(this.ReleaseLabel, "ReleaseLabel");
            //
            // AuthorLabel
            //
            this.AuthorLabel.AutoSize = true;
            this.AuthorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AuthorLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.AuthorLabel.Location = new System.Drawing.Point(3, 60);
            this.AuthorLabel.Name = "AuthorLabel";
            this.AuthorLabel.Size = new System.Drawing.Size(84, 30);
            this.AuthorLabel.TabIndex = 5;
            resources.ApplyResources(this.AuthorLabel, "AuthorLabel");
            //
            // LicenseLabel
            //
            this.LicenseLabel.AutoSize = true;
            this.LicenseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LicenseLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LicenseLabel.Location = new System.Drawing.Point(3, 30);
            this.LicenseLabel.Name = "LicenseLabel";
            this.LicenseLabel.Size = new System.Drawing.Size(84, 30);
            this.LicenseLabel.TabIndex = 3;
            resources.ApplyResources(this.LicenseLabel, "LicenseLabel");
            //
            // MetadataModuleVersionTextBox
            //
            this.MetadataModuleVersionTextBox.AutoSize = true;
            this.MetadataModuleVersionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleVersionTextBox.Location = new System.Drawing.Point(93, 0);
            this.MetadataModuleVersionTextBox.Name = "MetadataModuleVersionTextBox";
            this.MetadataModuleVersionTextBox.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleVersionTextBox.TabIndex = 2;
            this.MetadataModuleVersionTextBox.ReadOnly = true;
            this.MetadataModuleVersionTextBox.BackColor = MetadataTabPage.BackColor;
            this.MetadataModuleVersionTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.MetadataModuleVersionTextBox, "MetadataModuleVersionTextBox");
            //
            // MetadataModuleLicenseTextBox
            //
            this.MetadataModuleLicenseTextBox.AutoSize = true;
            this.MetadataModuleLicenseTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleLicenseTextBox.Location = new System.Drawing.Point(93, 30);
            this.MetadataModuleLicenseTextBox.Name = "MetadataModuleLicenseTextBox";
            this.MetadataModuleLicenseTextBox.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleLicenseTextBox.TabIndex = 4;
            this.MetadataModuleLicenseTextBox.ReadOnly = true;
            this.MetadataModuleLicenseTextBox.BackColor = MetadataTabPage.BackColor;
            this.MetadataModuleLicenseTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleLicenseTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.MetadataModuleLicenseTextBox, "MetadataModuleLicenseTextBox");
            //
            // MetadataModuleAuthorTextBox
            //
            this.MetadataModuleAuthorTextBox.AutoSize = true;
            this.MetadataModuleAuthorTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAuthorTextBox.Location = new System.Drawing.Point(93, 60);
            this.MetadataModuleAuthorTextBox.Name = "MetadataModuleAuthorTextBox";
            this.MetadataModuleAuthorTextBox.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleAuthorTextBox.TabIndex = 6;
            this.MetadataModuleAuthorTextBox.ReadOnly = true;
            this.MetadataModuleAuthorTextBox.BackColor = MetadataTabPage.BackColor;
            this.MetadataModuleAuthorTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleAuthorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.MetadataModuleAuthorTextBox, "MetadataModuleAuthorTextBox");
            //
            // VersionLabel
            //
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VersionLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.VersionLabel.Location = new System.Drawing.Point(3, 0);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(84, 30);
            this.VersionLabel.TabIndex = 1;
            resources.ApplyResources(this.VersionLabel, "VersionLabel");
            //
            // MetadataModuleReleaseStatusTextBox
            //
            this.MetadataModuleReleaseStatusTextBox.AutoSize = true;
            this.MetadataModuleReleaseStatusTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleReleaseStatusTextBox.Location = new System.Drawing.Point(93, 150);
            this.MetadataModuleReleaseStatusTextBox.Name = "MetadataModuleReleaseStatusTextBox";
            this.MetadataModuleReleaseStatusTextBox.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleReleaseStatusTextBox.TabIndex = 11;
            this.MetadataModuleReleaseStatusTextBox.ReadOnly = true;
            this.MetadataModuleReleaseStatusTextBox.BackColor = MetadataTabPage.BackColor;
            this.MetadataModuleReleaseStatusTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleReleaseStatusTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.MetadataModuleReleaseStatusTextBox, "MetadataModuleReleaseStatusTextBox");
            //
            // MetadataModuleGameCompatibilityTextBox
            //
            this.MetadataModuleGameCompatibilityTextBox.AutoSize = true;
            this.MetadataModuleGameCompatibilityTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleGameCompatibilityTextBox.Location = new System.Drawing.Point(93, 180);
            this.MetadataModuleGameCompatibilityTextBox.Name = "MetadataModuleGameCompatibilityTextBox";
            this.MetadataModuleGameCompatibilityTextBox.Size = new System.Drawing.Size(250, 30);
            this.MetadataModuleGameCompatibilityTextBox.TabIndex = 14;
            this.MetadataModuleGameCompatibilityTextBox.ReadOnly = true;
            this.MetadataModuleGameCompatibilityTextBox.BackColor = MetadataTabPage.BackColor;
            this.MetadataModuleGameCompatibilityTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleGameCompatibilityTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.MetadataModuleGameCompatibilityTextBox, "MetadataModuleGameCompatibilityTextBox");
            //
            // RelationshipTabPage
            //
            this.RelationshipTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.RelationshipTabPage.Controls.Add(this.DependsGraphTree);
            this.RelationshipTabPage.Controls.Add(this.LegendDependsImage);
            this.RelationshipTabPage.Controls.Add(this.LegendRecommendsImage);
            this.RelationshipTabPage.Controls.Add(this.LegendSuggestsImage);
            this.RelationshipTabPage.Controls.Add(this.LegendSupportsImage);
            this.RelationshipTabPage.Controls.Add(this.LegendConflictsImage);
            this.RelationshipTabPage.Controls.Add(this.LegendDependsLabel);
            this.RelationshipTabPage.Controls.Add(this.LegendRecommendsLabel);
            this.RelationshipTabPage.Controls.Add(this.LegendSuggestsLabel);
            this.RelationshipTabPage.Controls.Add(this.LegendSupportsLabel);
            this.RelationshipTabPage.Controls.Add(this.LegendConflictsLabel);
            this.RelationshipTabPage.Location = new System.Drawing.Point(4, 25);
            this.RelationshipTabPage.Name = "RelationshipTabPage";
            this.RelationshipTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.RelationshipTabPage.Size = new System.Drawing.Size(354, 502);
            this.RelationshipTabPage.TabIndex = 1;
            resources.ApplyResources(this.RelationshipTabPage, "RelationshipTabPage");
            //
            // DependsGraphTree
            //
            this.DependsGraphTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DependsGraphTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DependsGraphTree.Location = new System.Drawing.Point(3, 96);
            this.DependsGraphTree.Name = "DependsGraphTree";
            this.DependsGraphTree.Size = new System.Drawing.Size(345, 400);
            this.DependsGraphTree.TabIndex = 0;
            this.DependsGraphTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.DependsGraphTree_NodeMouseDoubleClick);
            this.DependsGraphTree.ShowNodeToolTips = true;
            this.DependsGraphTree.ImageList = new System.Windows.Forms.ImageList()
            {
                // ImageList's default makes icons look like garbage
                ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
            };
            this.DependsGraphTree.ImageList.Images.Add("Root", global::CKAN.Properties.Resources.ksp);
            this.DependsGraphTree.ImageList.Images.Add("Depends", global::CKAN.Properties.Resources.star);
            this.DependsGraphTree.ImageList.Images.Add("Recommends", global::CKAN.Properties.Resources.thumbup);
            this.DependsGraphTree.ImageList.Images.Add("Suggests", global::CKAN.Properties.Resources.info);
            this.DependsGraphTree.ImageList.Images.Add("Supports", global::CKAN.Properties.Resources.smile);
            this.DependsGraphTree.ImageList.Images.Add("Conflicts", global::CKAN.Properties.Resources.alert);
            //
            // LegendDependsImage
            //
            this.LegendDependsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendDependsImage.Image = global::CKAN.Properties.Resources.star;
            this.LegendDependsImage.Location = new System.Drawing.Point(6, 3);
            this.LegendDependsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendDependsImage.ClientSize = new System.Drawing.Size(14, 14);
            //
            // LegendRecommendsImage
            //
            this.LegendRecommendsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendRecommendsImage.Image = global::CKAN.Properties.Resources.thumbup;
            this.LegendRecommendsImage.Location = new System.Drawing.Point(6, 21);
            this.LegendRecommendsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendRecommendsImage.ClientSize = new System.Drawing.Size(14, 14);
            //
            // LegendSuggestsImage
            //
            this.LegendSuggestsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendSuggestsImage.Image = global::CKAN.Properties.Resources.info;
            this.LegendSuggestsImage.Location = new System.Drawing.Point(6, 39);
            this.LegendSuggestsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendSuggestsImage.ClientSize = new System.Drawing.Size(14, 14);
            //
            // LegendSupportsImage
            //
            this.LegendSupportsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendSupportsImage.Image = global::CKAN.Properties.Resources.smile;
            this.LegendSupportsImage.Location = new System.Drawing.Point(6, 57);
            this.LegendSupportsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendSupportsImage.ClientSize = new System.Drawing.Size(14, 14);
            //
            // LegendConflictsImage
            //
            this.LegendConflictsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendConflictsImage.Image = global::CKAN.Properties.Resources.alert;
            this.LegendConflictsImage.Location = new System.Drawing.Point(6, 75);
            this.LegendConflictsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendConflictsImage.ClientSize = new System.Drawing.Size(14, 14);
            //
            // LegendDependsLabel
            //
            this.LegendDependsLabel.AutoSize = true;
            this.LegendDependsLabel.Location = new System.Drawing.Point(24, 3);
            resources.ApplyResources(this.LegendDependsLabel, "LegendDependsLabel");
            //
            // LegendRecommendsLabel
            //
            this.LegendRecommendsLabel.AutoSize = true;
            this.LegendRecommendsLabel.Location = new System.Drawing.Point(24, 21);
            resources.ApplyResources(this.LegendRecommendsLabel, "LegendRecommendsLabel");
            //
            // LegendSuggestsLabel
            //
            this.LegendSuggestsLabel.AutoSize = true;
            this.LegendSuggestsLabel.Location = new System.Drawing.Point(24, 39);
            resources.ApplyResources(this.LegendSuggestsLabel, "LegendSuggestsLabel");
            //
            // LegendSupportsLabel
            //
            this.LegendSupportsLabel.AutoSize = true;
            this.LegendSupportsLabel.Location = new System.Drawing.Point(24, 57);
            resources.ApplyResources(this.LegendSupportsLabel, "LegendSupportsLabel");
            //
            // LegendConflictsLabel
            //
            this.LegendConflictsLabel.AutoSize = true;
            this.LegendConflictsLabel.Location = new System.Drawing.Point(24, 75);
            resources.ApplyResources(this.LegendConflictsLabel, "LegendConflictsLabel");
            //
            // ContentTabPage
            //
            this.ContentTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.ContentTabPage.Controls.Add(this.ContentsPreviewTree);
            this.ContentTabPage.Controls.Add(this.ContentsDownloadButton);
            this.ContentTabPage.Controls.Add(this.ContentsOpenButton);
            this.ContentTabPage.Controls.Add(this.NotCachedLabel);
            this.ContentTabPage.Location = new System.Drawing.Point(4, 25);
            this.ContentTabPage.Name = "ContentTabPage";
            this.ContentTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ContentTabPage.Size = new System.Drawing.Size(354, 502);
            this.ContentTabPage.TabIndex = 2;
            resources.ApplyResources(this.ContentTabPage, "ContentTabPage");
            //
            // ContentsPreviewTree
            //
            this.ContentsPreviewTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ContentsPreviewTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ContentsPreviewTree.ShowPlusMinus = false;
            this.ContentsPreviewTree.ShowRootLines = false;
            this.ContentsPreviewTree.Indent = 12;
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
            this.ContentsDownloadButton.UseVisualStyleBackColor = true;
            this.ContentsDownloadButton.Click += new System.EventHandler(this.ContentsDownloadButton_Click);
            resources.ApplyResources(this.ContentsDownloadButton, "ContentsDownloadButton");
            //
            // ContentsOpenButton
            //
            this.ContentsOpenButton.Location = new System.Drawing.Point(115, 36);
            this.ContentsOpenButton.Name = "ContentsOpenButton";
            this.ContentsOpenButton.Size = new System.Drawing.Size(103, 23);
            this.ContentsOpenButton.TabIndex = 1;
            this.ContentsOpenButton.UseVisualStyleBackColor = true;
            this.ContentsOpenButton.Click += new System.EventHandler(this.ContentsOpenButton_Click);
            resources.ApplyResources(this.ContentsOpenButton, "ContentsOpenButton");
            //
            // NotCachedLabel
            //
            this.NotCachedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NotCachedLabel.Location = new System.Drawing.Point(6, 3);
            this.NotCachedLabel.Name = "NotCachedLabel";
            this.NotCachedLabel.Size = new System.Drawing.Size(342, 434);
            this.NotCachedLabel.TabIndex = 0;
            resources.ApplyResources(this.NotCachedLabel, "NotCachedLabel");
            //
            // AllModVersionsTabPage
            //
            this.AllModVersionsTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.AllModVersionsTabPage.Controls.Add(this.AllModVersions);
            this.AllModVersionsTabPage.Location = new System.Drawing.Point(4, 25);
            this.AllModVersionsTabPage.Margin = new System.Windows.Forms.Padding(2);
            this.AllModVersionsTabPage.Name = "AllModVersionsTabPage";
            this.AllModVersionsTabPage.Size = new System.Drawing.Size(354, 502);
            this.AllModVersionsTabPage.TabIndex = 1;
            resources.ApplyResources(this.AllModVersionsTabPage, "AllModVersionsTabPage");
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
            // ModInfo
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer2);
            this.Name = "ModInfo";
            this.Size = new System.Drawing.Size(362, 531);
            resources.ApplyResources(this, "$this");
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
            this.AllModVersions.ResumeLayout(false);
            this.AllModVersionsTabPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TabControl ModInfoTabControl;
        private System.Windows.Forms.TabPage MetadataTabPage;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TableLayoutPanel MetaDataUpperLayoutPanel;
        private TransparentTextBox MetadataModuleNameTextBox;
        private System.Windows.Forms.FlowLayoutPanel MetadataTagsLabelsPanel;
        private System.Windows.Forms.Label MetadataModuleAbstractLabel;
        private TransparentTextBox MetadataModuleDescriptionTextBox;
        private System.Windows.Forms.TableLayoutPanel MetaDataLowerLayoutPanel;
        private System.Windows.Forms.Label IdentifierLabel;
        private TransparentTextBox MetadataIdentifierTextBox;
        private System.Windows.Forms.Label ReplacementLabel;
        private TransparentTextBox ReplacementTextBox;
        private System.Windows.Forms.Label GameCompatibilityLabel;
        private System.Windows.Forms.Label ReleaseLabel;
        private System.Windows.Forms.Label AuthorLabel;
        private System.Windows.Forms.Label LicenseLabel;
        private TransparentTextBox MetadataModuleVersionTextBox;
        private TransparentTextBox MetadataModuleLicenseTextBox;
        private TransparentTextBox MetadataModuleAuthorTextBox;
        private System.Windows.Forms.Label VersionLabel;
        private TransparentTextBox MetadataModuleReleaseStatusTextBox;
        private TransparentTextBox MetadataModuleGameCompatibilityTextBox;
        private System.Windows.Forms.TabPage RelationshipTabPage;
        private System.Windows.Forms.TreeView DependsGraphTree;
        private System.Windows.Forms.PictureBox LegendDependsImage;
        private System.Windows.Forms.PictureBox LegendRecommendsImage;
        private System.Windows.Forms.PictureBox LegendSuggestsImage;
        private System.Windows.Forms.PictureBox LegendSupportsImage;
        private System.Windows.Forms.PictureBox LegendConflictsImage;
        private System.Windows.Forms.Label LegendDependsLabel;
        private System.Windows.Forms.Label LegendRecommendsLabel;
        private System.Windows.Forms.Label LegendSuggestsLabel;
        private System.Windows.Forms.Label LegendSupportsLabel;
        private System.Windows.Forms.Label LegendConflictsLabel;
        private System.Windows.Forms.TabPage ContentTabPage;
        private System.Windows.Forms.TreeView ContentsPreviewTree;
        private System.Windows.Forms.Button ContentsDownloadButton;
        private System.Windows.Forms.Button ContentsOpenButton;
        private System.Windows.Forms.Label NotCachedLabel;
        private System.Windows.Forms.TabPage AllModVersionsTabPage;
        private AllModVersions AllModVersions;
    }
}
