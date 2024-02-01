namespace CKAN.GUI
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
            this.ModInfoTable = new System.Windows.Forms.TableLayoutPanel();
            this.MetadataModuleNameTextBox = new CKAN.GUI.TransparentTextBox();
            this.tagsLabelsLinkList = new CKAN.GUI.TagsLabelsLinkList();
            this.MetadataModuleAbstractLabel = new System.Windows.Forms.Label();
            this.MetadataModuleDescriptionTextBox = new CKAN.GUI.TransparentTextBox();
            this.ModInfoTabControl = new CKAN.GUI.ThemedTabControl();
            this.MetadataTabPage = new System.Windows.Forms.TabPage();
            this.Metadata = new CKAN.GUI.Metadata();
            this.RelationshipTabPage = new System.Windows.Forms.TabPage();
            this.Relationships = new CKAN.GUI.Relationships();
            this.ContentTabPage = new System.Windows.Forms.TabPage();
            this.Contents = new CKAN.GUI.Contents();
            this.VersionsTabPage = new System.Windows.Forms.TabPage();
            this.Versions = new CKAN.GUI.Versions();
            this.SuspendLayout();
            //
            // ModInfoTable
            //
            this.ModInfoTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ModInfoTable.ColumnCount = 1;
            this.ModInfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ModInfoTable.Controls.Add(this.MetadataModuleNameTextBox, 0, 0);
            this.ModInfoTable.Controls.Add(this.tagsLabelsLinkList, 0, 1);
            this.ModInfoTable.Controls.Add(this.MetadataModuleAbstractLabel, 0, 2);
            this.ModInfoTable.Controls.Add(this.MetadataModuleDescriptionTextBox, 0, 3);
            this.ModInfoTable.Controls.Add(this.ModInfoTabControl, 0, 4);
            this.ModInfoTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModInfoTable.Name = "ModInfoTable";
            this.ModInfoTable.RowCount = 5;
            this.ModInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 20F));
            this.ModInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.ModInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 30F));
            this.ModInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 20F));
            this.ModInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 20F));
            this.ModInfoTable.TabIndex = 0;
            //
            // MetadataModuleNameTextBox
            //
            this.MetadataModuleNameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MetadataModuleNameTextBox.Location = new System.Drawing.Point(3, 0);
            this.MetadataModuleNameTextBox.Name = "MetadataModuleNameTextBox";
            this.MetadataModuleNameTextBox.Size = new System.Drawing.Size(494, 46);
            this.MetadataModuleNameTextBox.TabIndex = 1;
            this.MetadataModuleNameTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.MetadataModuleNameTextBox.ReadOnly = true;
            this.MetadataModuleNameTextBox.BackColor = ModInfoTable.BackColor;
            this.MetadataModuleNameTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.MetadataModuleNameTextBox, "MetadataModuleNameTextBox");
            //
            // tagsLabelsLinkList
            //
            this.tagsLabelsLinkList.AutoSize = true;
            this.tagsLabelsLinkList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tagsLabelsLinkList.Padding = new System.Windows.Forms.Padding(0);
            this.tagsLabelsLinkList.Location = new System.Drawing.Point(0, 0);
            this.tagsLabelsLinkList.Name = "tagsLabelsLinkList";
            this.tagsLabelsLinkList.Size = new System.Drawing.Size(500, 20);
            this.tagsLabelsLinkList.TagClicked += this.tagsLabelsLinkList_TagClicked;
            this.tagsLabelsLinkList.LabelClicked += this.tagsLabelsLinkList_LabelClicked;
            //
            // MetadataModuleAbstractLabel
            //
            this.MetadataModuleAbstractLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleAbstractLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleAbstractLabel.Location = new System.Drawing.Point(3, 49);
            this.MetadataModuleAbstractLabel.Name = "MetadataModuleAbstractLabel";
            this.MetadataModuleAbstractLabel.Size = new System.Drawing.Size(494, 61);
            this.MetadataModuleAbstractLabel.AutoSize = true;
            this.MetadataModuleAbstractLabel.TabIndex = 2;
            this.MetadataModuleAbstractLabel.Text = "";
            //
            // MetadataModuleDescriptionTextBox
            //
            this.MetadataModuleDescriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MetadataModuleDescriptionTextBox.Location = new System.Drawing.Point(3, 129);
            this.MetadataModuleDescriptionTextBox.Name = "MetadataModuleDescriptionLabel";
            this.MetadataModuleDescriptionTextBox.Size = new System.Drawing.Size(494, 121);
            this.MetadataModuleDescriptionTextBox.TabIndex = 3;
            this.MetadataModuleDescriptionTextBox.Text = "";
            this.MetadataModuleDescriptionTextBox.ReadOnly = true;
            this.MetadataModuleDescriptionTextBox.BackColor = ModInfoTable.BackColor;
            this.MetadataModuleDescriptionTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleDescriptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MetadataModuleDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
            //
            // ModInfoTabControl
            //
            this.ModInfoTabControl.Appearance = System.Windows.Forms.TabAppearance.Normal;
            this.ModInfoTabControl.BackColor = System.Drawing.SystemColors.Control;
            this.ModInfoTabControl.Multiline = true;
            this.ModInfoTabControl.Controls.Add(this.MetadataTabPage);
            this.ModInfoTabControl.Controls.Add(this.RelationshipTabPage);
            this.ModInfoTabControl.Controls.Add(this.ContentTabPage);
            this.ModInfoTabControl.Controls.Add(this.VersionsTabPage);
            this.ModInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModInfoTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.ModInfoTabControl.Name = "ModInfoTabControl";
            this.ModInfoTabControl.TabIndex = 4;
            this.ModInfoTabControl.ImageList = new System.Windows.Forms.ImageList(this.components)
            {
                // ImageList's default makes icons look like garbage
                ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
            };
            this.ModInfoTabControl.ImageList.Images.Add("Stop", global::CKAN.GUI.EmbeddedImages.stop);
            this.ModInfoTabControl.SelectedIndexChanged += new System.EventHandler(this.ModInfoTabControl_SelectedIndexChanged);
            //
            // MetadataTabPage
            //
            this.MetadataTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.MetadataTabPage.Controls.Add(this.Metadata);
            this.MetadataTabPage.Name = "MetadataTabPage";
            this.MetadataTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.MetadataTabPage.TabIndex = 5;
            resources.ApplyResources(this.MetadataTabPage, "MetadataTabPage");
            //
            // Metadata
            //
            this.Metadata.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Metadata.Margin = new System.Windows.Forms.Padding(2);
            this.Metadata.Name = "Metadata";
            this.Metadata.TabIndex = 6;
            this.Metadata.OnChangeFilter += this.Metadata_OnChangeFilter;
            //
            // RelationshipTabPage
            //
            this.RelationshipTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.RelationshipTabPage.Controls.Add(this.Relationships);
            this.RelationshipTabPage.Name = "RelationshipTabPage";
            this.RelationshipTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.RelationshipTabPage.Size = new System.Drawing.Size(494, 300);
            this.RelationshipTabPage.TabIndex = 7;
            resources.ApplyResources(this.RelationshipTabPage, "RelationshipTabPage");
            //
            // Relationships
            //
            this.Relationships.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Relationships.Margin = new System.Windows.Forms.Padding(2);
            this.Relationships.Name = "Relationships";
            this.Relationships.Size = new System.Drawing.Size(494, 300);
            this.Relationships.TabIndex = 8;
            //
            // ContentTabPage
            //
            this.ContentTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.ContentTabPage.Controls.Add(this.Contents);
            this.ContentTabPage.Name = "ContentTabPage";
            this.ContentTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ContentTabPage.TabIndex = 9;
            resources.ApplyResources(this.ContentTabPage, "ContentTabPage");
            //
            // Contents
            //
            this.Contents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Contents.Margin = new System.Windows.Forms.Padding(2);
            this.Contents.Name = "Contents";
            this.Contents.TabIndex = 10;
            //
            // VersionsTabPage
            //
            this.VersionsTabPage.BackColor = System.Drawing.SystemColors.Control;
            this.VersionsTabPage.Controls.Add(this.Versions);
            this.VersionsTabPage.Margin = new System.Windows.Forms.Padding(3);
            this.VersionsTabPage.Name = "VersionsTabPage";
            this.VersionsTabPage.TabIndex = 11;
            resources.ApplyResources(this.VersionsTabPage, "VersionsTabPage");
            //
            // Versions
            //
            this.Versions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Versions.Margin = new System.Windows.Forms.Padding(2);
            this.Versions.Name = "Versions";
            this.Versions.TabIndex = 12;
            //
            // ModInfo
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ModInfoTable);
            this.Name = "ModInfo";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel ModInfoTable;
        private CKAN.GUI.TransparentTextBox MetadataModuleNameTextBox;
        private CKAN.GUI.TagsLabelsLinkList tagsLabelsLinkList;
        private System.Windows.Forms.Label MetadataModuleAbstractLabel;
        private CKAN.GUI.TransparentTextBox MetadataModuleDescriptionTextBox;
        private System.Windows.Forms.TabControl ModInfoTabControl;
        private System.Windows.Forms.TabPage MetadataTabPage;
        private CKAN.GUI.Metadata Metadata;
        private System.Windows.Forms.TabPage RelationshipTabPage;
        private CKAN.GUI.Relationships Relationships;
        private System.Windows.Forms.TabPage ContentTabPage;
        private CKAN.GUI.Contents Contents;
        private System.Windows.Forms.TabPage VersionsTabPage;
        private CKAN.GUI.Versions Versions;
    }
}
