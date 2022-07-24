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
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.MetaDataUpperLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.MetadataModuleNameTextBox = new CKAN.TransparentTextBox();
            this.MetadataTagsLabelsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.MetadataModuleAbstractLabel = new System.Windows.Forms.Label();
            this.MetadataModuleDescriptionTextBox = new CKAN.TransparentTextBox();
            this.ModInfoTabControl = new CKAN.ThemedTabControl();
            this.MetadataTabPage = new System.Windows.Forms.TabPage();
            this.MetaDataLowerLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.LicenseLabel = new System.Windows.Forms.Label();
            this.AuthorLabel = new System.Windows.Forms.Label();
            this.ReleaseLabel = new System.Windows.Forms.Label();
            this.GameCompatibilityLabel = new System.Windows.Forms.Label();
            this.IdentifierLabel = new System.Windows.Forms.Label();
            this.ReplacementLabel = new System.Windows.Forms.Label();
            this.MetadataModuleVersionTextBox = new CKAN.TransparentTextBox();
            this.MetadataModuleLicenseTextBox = new CKAN.TransparentTextBox();
            this.MetadataModuleAuthorTextBox = new CKAN.TransparentTextBox();
            this.MetadataModuleReleaseStatusTextBox = new CKAN.TransparentTextBox();
            this.MetadataModuleGameCompatibilityTextBox = new CKAN.TransparentTextBox();
            this.MetadataIdentifierTextBox = new CKAN.TransparentTextBox();
            this.ReplacementTextBox = new CKAN.TransparentTextBox();
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
            this.ChangelogTab = new System.Windows.Forms.TabPage();
            this.changelogs1 = new CKAN.Changelogs();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.MetaDataUpperLayoutPanel.SuspendLayout();
            this.ModInfoTabControl.SuspendLayout();
            this.MetadataTabPage.SuspendLayout();
            this.MetaDataLowerLayoutPanel.SuspendLayout();
            this.RelationshipTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LegendDependsImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LegendRecommendsImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LegendSuggestsImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LegendSupportsImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LegendConflictsImage)).BeginInit();
            this.ContentTabPage.SuspendLayout();
            this.AllModVersionsTabPage.SuspendLayout();
            this.ChangelogTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
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
            this.splitContainer2.Size = new System.Drawing.Size(483, 654);
            this.splitContainer2.SplitterDistance = 309;
            this.splitContainer2.SplitterWidth = 12;
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
            this.MetaDataUpperLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.MetaDataUpperLayoutPanel.Name = "MetaDataUpperLayoutPanel";
            this.MetaDataUpperLayoutPanel.RowCount = 4;
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MetaDataUpperLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.MetaDataUpperLayoutPanel.Size = new System.Drawing.Size(483, 309);
            this.MetaDataUpperLayoutPanel.TabIndex = 0;
            // 
            // MetadataModuleNameTextBox
            // 
            this.MetadataModuleNameTextBox.BackColor = this.MetaDataUpperLayoutPanel.BackColor;
            this.MetadataModuleNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataModuleNameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MetadataModuleNameTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleNameTextBox.Location = new System.Drawing.Point(4, 4);
            this.MetadataModuleNameTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataModuleNameTextBox.Multiline = true;
            this.MetadataModuleNameTextBox.Name = "MetadataModuleNameTextBox";
            this.MetadataModuleNameTextBox.ReadOnly = true;
            this.MetadataModuleNameTextBox.Size = new System.Drawing.Size(475, 57);
            this.MetadataModuleNameTextBox.TabIndex = 0;
            this.MetadataModuleNameTextBox.Text = "Mod Name";
            this.MetadataModuleNameTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MetadataTagsLabelsPanel
            // 
            this.MetadataTagsLabelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataTagsLabelsPanel.Location = new System.Drawing.Point(4, 69);
            this.MetadataTagsLabelsPanel.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataTagsLabelsPanel.Name = "MetadataTagsLabelsPanel";
            this.MetadataTagsLabelsPanel.Size = new System.Drawing.Size(475, 31);
            this.MetadataTagsLabelsPanel.TabIndex = 1;
            // 
            // MetadataModuleAbstractLabel
            // 
            this.MetadataModuleAbstractLabel.AutoSize = true;
            this.MetadataModuleAbstractLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAbstractLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleAbstractLabel.Location = new System.Drawing.Point(4, 104);
            this.MetadataModuleAbstractLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MetadataModuleAbstractLabel.Name = "MetadataModuleAbstractLabel";
            this.MetadataModuleAbstractLabel.Size = new System.Drawing.Size(475, 17);
            this.MetadataModuleAbstractLabel.TabIndex = 27;
            // 
            // MetadataModuleDescriptionTextBox
            // 
            this.MetadataModuleDescriptionTextBox.BackColor = this.MetaDataUpperLayoutPanel.BackColor;
            this.MetadataModuleDescriptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataModuleDescriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleDescriptionTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleDescriptionTextBox.Location = new System.Drawing.Point(4, 125);
            this.MetadataModuleDescriptionTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataModuleDescriptionTextBox.Multiline = true;
            this.MetadataModuleDescriptionTextBox.Name = "MetadataModuleDescriptionTextBox";
            this.MetadataModuleDescriptionTextBox.ReadOnly = true;
            this.MetadataModuleDescriptionTextBox.Size = new System.Drawing.Size(475, 180);
            this.MetadataModuleDescriptionTextBox.TabIndex = 28;
            // 
            // ModInfoTabControl
            // 
            this.ModInfoTabControl.Controls.Add(this.MetadataTabPage);
            this.ModInfoTabControl.Controls.Add(this.RelationshipTabPage);
            this.ModInfoTabControl.Controls.Add(this.ContentTabPage);
            this.ModInfoTabControl.Controls.Add(this.AllModVersionsTabPage);
            this.ModInfoTabControl.Controls.Add(this.ChangelogTab);
            this.ModInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModInfoTabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.ModInfoTabControl.Location = new System.Drawing.Point(0, 0);
            this.ModInfoTabControl.Margin = new System.Windows.Forms.Padding(4);
            this.ModInfoTabControl.Multiline = true;
            this.ModInfoTabControl.Name = "ModInfoTabControl";
            this.ModInfoTabControl.SelectedIndex = 0;
            this.ModInfoTabControl.Size = new System.Drawing.Size(483, 333);
            this.ModInfoTabControl.TabIndex = 1;
            this.ModInfoTabControl.SelectedIndexChanged += new System.EventHandler(this.ModInfoIndexChanged);
            // 
            // MetadataTabPage
            // 
            this.MetadataTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.MetadataTabPage.Controls.Add(this.MetaDataLowerLayoutPanel);
            this.MetadataTabPage.Location = new System.Drawing.Point(4, 25);
            this.MetadataTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataTabPage.Name = "MetadataTabPage";
            this.MetadataTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.MetadataTabPage.Size = new System.Drawing.Size(475, 304);
            this.MetadataTabPage.TabIndex = 0;
            this.MetadataTabPage.Text = "Metadata";
            // 
            // MetaDataLowerLayoutPanel
            // 
            this.MetaDataLowerLayoutPanel.AutoScroll = true;
            this.MetaDataLowerLayoutPanel.AutoScrollMinSize = new System.Drawing.Size(346, 255);
            this.MetaDataLowerLayoutPanel.AutoSize = true;
            this.MetaDataLowerLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MetaDataLowerLayoutPanel.ColumnCount = 2;
            this.MetaDataLowerLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.MetaDataLowerLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
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
            this.MetaDataLowerLayoutPanel.Location = new System.Drawing.Point(4, 4);
            this.MetaDataLowerLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.MetaDataLowerLayoutPanel.Name = "MetaDataLowerLayoutPanel";
            this.MetaDataLowerLayoutPanel.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.MetaDataLowerLayoutPanel.RowCount = 7;
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.MetaDataLowerLayoutPanel.Size = new System.Drawing.Size(467, 296);
            this.MetaDataLowerLayoutPanel.TabIndex = 0;
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VersionLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.VersionLabel.Location = new System.Drawing.Point(4, 10);
            this.VersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(112, 37);
            this.VersionLabel.TabIndex = 1;
            this.VersionLabel.Text = "Version:";
            // 
            // LicenseLabel
            // 
            this.LicenseLabel.AutoSize = true;
            this.LicenseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LicenseLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LicenseLabel.Location = new System.Drawing.Point(4, 47);
            this.LicenseLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LicenseLabel.Name = "LicenseLabel";
            this.LicenseLabel.Size = new System.Drawing.Size(112, 37);
            this.LicenseLabel.TabIndex = 3;
            this.LicenseLabel.Text = "Licence:";
            // 
            // AuthorLabel
            // 
            this.AuthorLabel.AutoSize = true;
            this.AuthorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AuthorLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.AuthorLabel.Location = new System.Drawing.Point(4, 84);
            this.AuthorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.AuthorLabel.Name = "AuthorLabel";
            this.AuthorLabel.Size = new System.Drawing.Size(112, 37);
            this.AuthorLabel.TabIndex = 5;
            this.AuthorLabel.Text = "Author:";
            // 
            // ReleaseLabel
            // 
            this.ReleaseLabel.AutoSize = true;
            this.ReleaseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReleaseLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.ReleaseLabel.Location = new System.Drawing.Point(4, 121);
            this.ReleaseLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ReleaseLabel.Name = "ReleaseLabel";
            this.ReleaseLabel.Size = new System.Drawing.Size(112, 37);
            this.ReleaseLabel.TabIndex = 12;
            this.ReleaseLabel.Text = "Release status:";
            // 
            // GameCompatibilityLabel
            // 
            this.GameCompatibilityLabel.AutoSize = true;
            this.GameCompatibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GameCompatibilityLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.GameCompatibilityLabel.Location = new System.Drawing.Point(4, 158);
            this.GameCompatibilityLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.GameCompatibilityLabel.Name = "GameCompatibilityLabel";
            this.GameCompatibilityLabel.Size = new System.Drawing.Size(112, 37);
            this.GameCompatibilityLabel.TabIndex = 13;
            this.GameCompatibilityLabel.Text = "Max game ver.:";
            // 
            // IdentifierLabel
            // 
            this.IdentifierLabel.AutoSize = true;
            this.IdentifierLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IdentifierLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.IdentifierLabel.Location = new System.Drawing.Point(4, 195);
            this.IdentifierLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.IdentifierLabel.Name = "IdentifierLabel";
            this.IdentifierLabel.Size = new System.Drawing.Size(112, 37);
            this.IdentifierLabel.TabIndex = 28;
            this.IdentifierLabel.Text = "Identifier:";
            // 
            // ReplacementLabel
            // 
            this.ReplacementLabel.AutoSize = true;
            this.ReplacementLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReplacementLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.ReplacementLabel.Location = new System.Drawing.Point(4, 232);
            this.ReplacementLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ReplacementLabel.Name = "ReplacementLabel";
            this.ReplacementLabel.Size = new System.Drawing.Size(112, 64);
            this.ReplacementLabel.TabIndex = 28;
            this.ReplacementLabel.Text = "Replaced by:";
            // 
            // MetadataModuleVersionTextBox
            // 
            this.MetadataModuleVersionTextBox.BackColor = this.MetadataTabPage.BackColor;
            this.MetadataModuleVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataModuleVersionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleVersionTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleVersionTextBox.Location = new System.Drawing.Point(124, 14);
            this.MetadataModuleVersionTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataModuleVersionTextBox.Multiline = true;
            this.MetadataModuleVersionTextBox.Name = "MetadataModuleVersionTextBox";
            this.MetadataModuleVersionTextBox.ReadOnly = true;
            this.MetadataModuleVersionTextBox.Size = new System.Drawing.Size(339, 29);
            this.MetadataModuleVersionTextBox.TabIndex = 2;
            this.MetadataModuleVersionTextBox.Text = "0.0.0";
            // 
            // MetadataModuleLicenseTextBox
            // 
            this.MetadataModuleLicenseTextBox.BackColor = this.MetadataTabPage.BackColor;
            this.MetadataModuleLicenseTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataModuleLicenseTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleLicenseTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleLicenseTextBox.Location = new System.Drawing.Point(124, 51);
            this.MetadataModuleLicenseTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataModuleLicenseTextBox.Multiline = true;
            this.MetadataModuleLicenseTextBox.Name = "MetadataModuleLicenseTextBox";
            this.MetadataModuleLicenseTextBox.ReadOnly = true;
            this.MetadataModuleLicenseTextBox.Size = new System.Drawing.Size(339, 29);
            this.MetadataModuleLicenseTextBox.TabIndex = 4;
            this.MetadataModuleLicenseTextBox.Text = "None";
            // 
            // MetadataModuleAuthorTextBox
            // 
            this.MetadataModuleAuthorTextBox.BackColor = this.MetadataTabPage.BackColor;
            this.MetadataModuleAuthorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataModuleAuthorTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAuthorTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleAuthorTextBox.Location = new System.Drawing.Point(124, 88);
            this.MetadataModuleAuthorTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataModuleAuthorTextBox.Multiline = true;
            this.MetadataModuleAuthorTextBox.Name = "MetadataModuleAuthorTextBox";
            this.MetadataModuleAuthorTextBox.ReadOnly = true;
            this.MetadataModuleAuthorTextBox.Size = new System.Drawing.Size(339, 29);
            this.MetadataModuleAuthorTextBox.TabIndex = 6;
            this.MetadataModuleAuthorTextBox.Text = "Nobody";
            // 
            // MetadataModuleReleaseStatusTextBox
            // 
            this.MetadataModuleReleaseStatusTextBox.BackColor = this.MetadataTabPage.BackColor;
            this.MetadataModuleReleaseStatusTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataModuleReleaseStatusTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleReleaseStatusTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleReleaseStatusTextBox.Location = new System.Drawing.Point(124, 125);
            this.MetadataModuleReleaseStatusTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataModuleReleaseStatusTextBox.Multiline = true;
            this.MetadataModuleReleaseStatusTextBox.Name = "MetadataModuleReleaseStatusTextBox";
            this.MetadataModuleReleaseStatusTextBox.ReadOnly = true;
            this.MetadataModuleReleaseStatusTextBox.Size = new System.Drawing.Size(339, 29);
            this.MetadataModuleReleaseStatusTextBox.TabIndex = 11;
            this.MetadataModuleReleaseStatusTextBox.Text = "Stable";
            // 
            // MetadataModuleGameCompatibilityTextBox
            // 
            this.MetadataModuleGameCompatibilityTextBox.BackColor = this.MetadataTabPage.BackColor;
            this.MetadataModuleGameCompatibilityTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataModuleGameCompatibilityTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleGameCompatibilityTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleGameCompatibilityTextBox.Location = new System.Drawing.Point(124, 162);
            this.MetadataModuleGameCompatibilityTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataModuleGameCompatibilityTextBox.Multiline = true;
            this.MetadataModuleGameCompatibilityTextBox.Name = "MetadataModuleGameCompatibilityTextBox";
            this.MetadataModuleGameCompatibilityTextBox.ReadOnly = true;
            this.MetadataModuleGameCompatibilityTextBox.Size = new System.Drawing.Size(339, 29);
            this.MetadataModuleGameCompatibilityTextBox.TabIndex = 14;
            this.MetadataModuleGameCompatibilityTextBox.Text = "0.0.0";
            // 
            // MetadataIdentifierTextBox
            // 
            this.MetadataIdentifierTextBox.BackColor = this.MetadataTabPage.BackColor;
            this.MetadataIdentifierTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataIdentifierTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataIdentifierTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataIdentifierTextBox.Location = new System.Drawing.Point(124, 199);
            this.MetadataIdentifierTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.MetadataIdentifierTextBox.Multiline = true;
            this.MetadataIdentifierTextBox.Name = "MetadataIdentifierTextBox";
            this.MetadataIdentifierTextBox.ReadOnly = true;
            this.MetadataIdentifierTextBox.Size = new System.Drawing.Size(339, 29);
            this.MetadataIdentifierTextBox.TabIndex = 27;
            this.MetadataIdentifierTextBox.Text = "-";
            // 
            // ReplacementTextBox
            // 
            this.ReplacementTextBox.BackColor = this.MetadataTabPage.BackColor;
            this.ReplacementTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ReplacementTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReplacementTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ReplacementTextBox.Location = new System.Drawing.Point(124, 236);
            this.ReplacementTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.ReplacementTextBox.Multiline = true;
            this.ReplacementTextBox.Name = "ReplacementTextBox";
            this.ReplacementTextBox.ReadOnly = true;
            this.ReplacementTextBox.Size = new System.Drawing.Size(339, 56);
            this.ReplacementTextBox.TabIndex = 27;
            this.ReplacementTextBox.Text = "-";
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
            this.RelationshipTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.RelationshipTabPage.Name = "RelationshipTabPage";
            this.RelationshipTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.RelationshipTabPage.Size = new System.Drawing.Size(475, 304);
            this.RelationshipTabPage.TabIndex = 1;
            this.RelationshipTabPage.Text = "Relationships";
            // 
            // DependsGraphTree
            // 
            this.DependsGraphTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DependsGraphTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DependsGraphTree.Location = new System.Drawing.Point(4, 118);
            this.DependsGraphTree.Margin = new System.Windows.Forms.Padding(4);
            this.DependsGraphTree.Name = "DependsGraphTree";
            this.DependsGraphTree.ShowNodeToolTips = true;
            this.DependsGraphTree.Size = new System.Drawing.Size(478, 516);
            this.DependsGraphTree.TabIndex = 0;
            this.DependsGraphTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.DependsGraphTree_NodeMouseDoubleClick);
            // 
            // LegendDependsImage
            // 
            this.LegendDependsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendDependsImage.Image = global::CKAN.Properties.Resources.star;
            this.LegendDependsImage.Location = new System.Drawing.Point(8, 4);
            this.LegendDependsImage.Margin = new System.Windows.Forms.Padding(4);
            this.LegendDependsImage.Name = "LegendDependsImage";
            this.LegendDependsImage.Size = new System.Drawing.Size(19, 17);
            this.LegendDependsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendDependsImage.TabIndex = 1;
            this.LegendDependsImage.TabStop = false;
            // 
            // LegendRecommendsImage
            // 
            this.LegendRecommendsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendRecommendsImage.Image = global::CKAN.Properties.Resources.thumbup;
            this.LegendRecommendsImage.Location = new System.Drawing.Point(8, 26);
            this.LegendRecommendsImage.Margin = new System.Windows.Forms.Padding(4);
            this.LegendRecommendsImage.Name = "LegendRecommendsImage";
            this.LegendRecommendsImage.Size = new System.Drawing.Size(19, 17);
            this.LegendRecommendsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendRecommendsImage.TabIndex = 2;
            this.LegendRecommendsImage.TabStop = false;
            // 
            // LegendSuggestsImage
            // 
            this.LegendSuggestsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendSuggestsImage.Image = global::CKAN.Properties.Resources.info;
            this.LegendSuggestsImage.Location = new System.Drawing.Point(8, 48);
            this.LegendSuggestsImage.Margin = new System.Windows.Forms.Padding(4);
            this.LegendSuggestsImage.Name = "LegendSuggestsImage";
            this.LegendSuggestsImage.Size = new System.Drawing.Size(19, 17);
            this.LegendSuggestsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendSuggestsImage.TabIndex = 3;
            this.LegendSuggestsImage.TabStop = false;
            // 
            // LegendSupportsImage
            // 
            this.LegendSupportsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendSupportsImage.Image = global::CKAN.Properties.Resources.smile;
            this.LegendSupportsImage.Location = new System.Drawing.Point(8, 70);
            this.LegendSupportsImage.Margin = new System.Windows.Forms.Padding(4);
            this.LegendSupportsImage.Name = "LegendSupportsImage";
            this.LegendSupportsImage.Size = new System.Drawing.Size(19, 17);
            this.LegendSupportsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendSupportsImage.TabIndex = 4;
            this.LegendSupportsImage.TabStop = false;
            // 
            // LegendConflictsImage
            // 
            this.LegendConflictsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendConflictsImage.Image = global::CKAN.Properties.Resources.alert;
            this.LegendConflictsImage.Location = new System.Drawing.Point(8, 92);
            this.LegendConflictsImage.Margin = new System.Windows.Forms.Padding(4);
            this.LegendConflictsImage.Name = "LegendConflictsImage";
            this.LegendConflictsImage.Size = new System.Drawing.Size(19, 17);
            this.LegendConflictsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendConflictsImage.TabIndex = 5;
            this.LegendConflictsImage.TabStop = false;
            // 
            // LegendDependsLabel
            // 
            this.LegendDependsLabel.AutoSize = true;
            this.LegendDependsLabel.Location = new System.Drawing.Point(32, 4);
            this.LegendDependsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LegendDependsLabel.Name = "LegendDependsLabel";
            this.LegendDependsLabel.Size = new System.Drawing.Size(65, 17);
            this.LegendDependsLabel.TabIndex = 6;
            this.LegendDependsLabel.Text = "Depends";
            // 
            // LegendRecommendsLabel
            // 
            this.LegendRecommendsLabel.AutoSize = true;
            this.LegendRecommendsLabel.Location = new System.Drawing.Point(32, 26);
            this.LegendRecommendsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LegendRecommendsLabel.Name = "LegendRecommendsLabel";
            this.LegendRecommendsLabel.Size = new System.Drawing.Size(94, 17);
            this.LegendRecommendsLabel.TabIndex = 7;
            this.LegendRecommendsLabel.Text = "Recommends";
            // 
            // LegendSuggestsLabel
            // 
            this.LegendSuggestsLabel.AutoSize = true;
            this.LegendSuggestsLabel.Location = new System.Drawing.Point(32, 48);
            this.LegendSuggestsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LegendSuggestsLabel.Name = "LegendSuggestsLabel";
            this.LegendSuggestsLabel.Size = new System.Drawing.Size(67, 17);
            this.LegendSuggestsLabel.TabIndex = 8;
            this.LegendSuggestsLabel.Text = "Suggests";
            // 
            // LegendSupportsLabel
            // 
            this.LegendSupportsLabel.AutoSize = true;
            this.LegendSupportsLabel.Location = new System.Drawing.Point(32, 70);
            this.LegendSupportsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LegendSupportsLabel.Name = "LegendSupportsLabel";
            this.LegendSupportsLabel.Size = new System.Drawing.Size(65, 17);
            this.LegendSupportsLabel.TabIndex = 9;
            this.LegendSupportsLabel.Text = "Supports";
            // 
            // LegendConflictsLabel
            // 
            this.LegendConflictsLabel.AutoSize = true;
            this.LegendConflictsLabel.Location = new System.Drawing.Point(32, 92);
            this.LegendConflictsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LegendConflictsLabel.Name = "LegendConflictsLabel";
            this.LegendConflictsLabel.Size = new System.Drawing.Size(61, 17);
            this.LegendConflictsLabel.TabIndex = 10;
            this.LegendConflictsLabel.Text = "Conflicts";
            // 
            // ContentTabPage
            // 
            this.ContentTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.ContentTabPage.Controls.Add(this.ContentsPreviewTree);
            this.ContentTabPage.Controls.Add(this.ContentsDownloadButton);
            this.ContentTabPage.Controls.Add(this.ContentsOpenButton);
            this.ContentTabPage.Controls.Add(this.NotCachedLabel);
            this.ContentTabPage.Location = new System.Drawing.Point(4, 25);
            this.ContentTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.ContentTabPage.Name = "ContentTabPage";
            this.ContentTabPage.Padding = new System.Windows.Forms.Padding(4);
            this.ContentTabPage.Size = new System.Drawing.Size(475, 304);
            this.ContentTabPage.TabIndex = 2;
            this.ContentTabPage.Text = "Contents";
            // 
            // ContentsPreviewTree
            // 
            this.ContentsPreviewTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ContentsPreviewTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ContentsPreviewTree.Enabled = false;
            this.ContentsPreviewTree.Indent = 12;
            this.ContentsPreviewTree.Location = new System.Drawing.Point(8, 80);
            this.ContentsPreviewTree.Margin = new System.Windows.Forms.Padding(4);
            this.ContentsPreviewTree.Name = "ContentsPreviewTree";
            this.ContentsPreviewTree.ShowPlusMinus = false;
            this.ContentsPreviewTree.ShowRootLines = false;
            this.ContentsPreviewTree.Size = new System.Drawing.Size(474, 558);
            this.ContentsPreviewTree.TabIndex = 2;
            this.ContentsPreviewTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ContentsPreviewTree_NodeMouseDoubleClick);
            // 
            // ContentsDownloadButton
            // 
            this.ContentsDownloadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ContentsDownloadButton.Location = new System.Drawing.Point(8, 44);
            this.ContentsDownloadButton.Margin = new System.Windows.Forms.Padding(4);
            this.ContentsDownloadButton.Name = "ContentsDownloadButton";
            this.ContentsDownloadButton.Size = new System.Drawing.Size(137, 28);
            this.ContentsDownloadButton.TabIndex = 1;
            this.ContentsDownloadButton.Text = "Download";
            this.ContentsDownloadButton.UseVisualStyleBackColor = true;
            this.ContentsDownloadButton.Click += new System.EventHandler(this.ContentsDownloadButton_Click);
            // 
            // ContentsOpenButton
            // 
            this.ContentsOpenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ContentsOpenButton.Location = new System.Drawing.Point(153, 44);
            this.ContentsOpenButton.Margin = new System.Windows.Forms.Padding(4);
            this.ContentsOpenButton.Name = "ContentsOpenButton";
            this.ContentsOpenButton.Size = new System.Drawing.Size(137, 28);
            this.ContentsOpenButton.TabIndex = 1;
            this.ContentsOpenButton.Text = "Open ZIP";
            this.ContentsOpenButton.UseVisualStyleBackColor = true;
            this.ContentsOpenButton.Click += new System.EventHandler(this.ContentsOpenButton_Click);
            // 
            // NotCachedLabel
            // 
            this.NotCachedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NotCachedLabel.Location = new System.Drawing.Point(8, 4);
            this.NotCachedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.NotCachedLabel.Name = "NotCachedLabel";
            this.NotCachedLabel.Size = new System.Drawing.Size(475, 558);
            this.NotCachedLabel.TabIndex = 0;
            this.NotCachedLabel.Text = "This mod is not in the cache, click \'Download\' to preview contents";
            // 
            // AllModVersionsTabPage
            // 
            this.AllModVersionsTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.AllModVersionsTabPage.Controls.Add(this.AllModVersions);
            this.AllModVersionsTabPage.Location = new System.Drawing.Point(4, 25);
            this.AllModVersionsTabPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AllModVersionsTabPage.Name = "AllModVersionsTabPage";
            this.AllModVersionsTabPage.Size = new System.Drawing.Size(475, 304);
            this.AllModVersionsTabPage.TabIndex = 1;
            this.AllModVersionsTabPage.Text = "Versions";
            // 
            // AllModVersions
            // 
            this.AllModVersions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AllModVersions.Location = new System.Drawing.Point(0, 0);
            this.AllModVersions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AllModVersions.Name = "AllModVersions";
            this.AllModVersions.Size = new System.Drawing.Size(475, 304);
            this.AllModVersions.TabIndex = 0;
            // 
            // ChangelogTab
            // 
            this.ChangelogTab.Controls.Add(this.changelogs1);
            this.ChangelogTab.Location = new System.Drawing.Point(4, 25);
            this.ChangelogTab.Name = "ChangelogTab";
            this.ChangelogTab.Padding = new System.Windows.Forms.Padding(3);
            this.ChangelogTab.Size = new System.Drawing.Size(475, 304);
            this.ChangelogTab.TabIndex = 3;
            this.ChangelogTab.Text = "Changelogs";
            this.ChangelogTab.UseVisualStyleBackColor = true;
            // 
            // changelogs1
            // 
            this.changelogs1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.changelogs1.Location = new System.Drawing.Point(0, 0);
            this.changelogs1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.changelogs1.Name = "changelogs1";
            this.changelogs1.Size = new System.Drawing.Size(475, 304);
            this.changelogs1.TabIndex = 0;
            // 
            // ModInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer2);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ModInfo";
            this.Size = new System.Drawing.Size(483, 654);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.MetaDataUpperLayoutPanel.ResumeLayout(false);
            this.MetaDataUpperLayoutPanel.PerformLayout();
            this.ModInfoTabControl.ResumeLayout(false);
            this.MetadataTabPage.ResumeLayout(false);
            this.MetadataTabPage.PerformLayout();
            this.MetaDataLowerLayoutPanel.ResumeLayout(false);
            this.MetaDataLowerLayoutPanel.PerformLayout();
            this.RelationshipTabPage.ResumeLayout(false);
            this.RelationshipTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LegendDependsImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LegendRecommendsImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LegendSuggestsImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LegendSupportsImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LegendConflictsImage)).EndInit();
            this.ContentTabPage.ResumeLayout(false);
            this.AllModVersionsTabPage.ResumeLayout(false);
            this.ChangelogTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
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
        private System.Windows.Forms.TabPage ChangelogTab;
        private ThemedTabControl ModInfoTabControl;
        private Changelogs changelogs1;
    }
}
