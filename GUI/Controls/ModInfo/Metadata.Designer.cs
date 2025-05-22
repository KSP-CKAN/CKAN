namespace CKAN.GUI
{
    partial class Metadata
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(Metadata));
            this.ToolTip = new System.Windows.Forms.ToolTip();
            this.MetadataTable = new System.Windows.Forms.TableLayoutPanel();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.VersionTextBox = new CKAN.GUI.TransparentTextBox();
            this.LicenseLabel = new System.Windows.Forms.Label();
            this.LicenseTextBox = new CKAN.GUI.TransparentTextBox();
            this.AuthorLabel = new System.Windows.Forms.Label();
            this.AuthorsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.DownloadCountLabel = new System.Windows.Forms.Label();
            this.DownloadCountTextBox = new CKAN.GUI.TransparentTextBox();
            this.ReleaseLabel = new System.Windows.Forms.Label();
            this.ReleaseStatusTextBox = new CKAN.GUI.TransparentTextBox();
            this.GameCompatibilityLabel = new System.Windows.Forms.Label();
            this.GameCompatibilityTextBox = new CKAN.GUI.TransparentTextBox();
            this.IdentifierLabel = new System.Windows.Forms.Label();
            this.IdentifierTextBox = new CKAN.GUI.TransparentTextBox();
            this.ReplacementLabel = new System.Windows.Forms.Label();
            this.ReplacementTextBox = new CKAN.GUI.TransparentTextBox();
            this.SuspendLayout();
            //
            // ToolTip
            //
            this.ToolTip.AutoPopDelay = 10000;
            this.ToolTip.InitialDelay = 250;
            this.ToolTip.ReshowDelay = 250;
            this.ToolTip.ShowAlways = true;
            //
            // MetadataTable
            //
            this.MetadataTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MetadataTable.ColumnCount = 2;
            this.MetadataTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetadataTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetadataTable.Controls.Add(this.VersionLabel, 0, 0);
            this.MetadataTable.Controls.Add(this.VersionTextBox, 1, 0);
            this.MetadataTable.Controls.Add(this.LicenseLabel, 0, 1);
            this.MetadataTable.Controls.Add(this.LicenseTextBox, 1, 1);
            this.MetadataTable.Controls.Add(this.AuthorLabel, 0, 2);
            this.MetadataTable.Controls.Add(this.AuthorsPanel, 1, 2);
            this.MetadataTable.Controls.Add(this.DownloadCountLabel, 0, 3);
            this.MetadataTable.Controls.Add(this.DownloadCountTextBox, 1, 3);
            this.MetadataTable.Controls.Add(this.ReleaseLabel, 0, 4);
            this.MetadataTable.Controls.Add(this.ReleaseStatusTextBox, 1, 4);
            this.MetadataTable.Controls.Add(this.GameCompatibilityLabel, 0, 5);
            this.MetadataTable.Controls.Add(this.GameCompatibilityTextBox, 1, 5);
            this.MetadataTable.Controls.Add(this.IdentifierLabel, 0, 6);
            this.MetadataTable.Controls.Add(this.IdentifierTextBox, 1, 6);
            this.MetadataTable.Controls.Add(this.ReplacementLabel, 0, 7);
            this.MetadataTable.Controls.Add(this.ReplacementTextBox, 1, 7);
            this.MetadataTable.Dock = System.Windows.Forms.DockStyle.Top;
            this.MetadataTable.Location = new System.Drawing.Point(0, 0);
            this.MetadataTable.Name = "MetadataTable";
            this.MetadataTable.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.MetadataTable.RowCount = 8;
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetadataTable.Size = new System.Drawing.Size(346, 255);
            this.MetadataTable.AutoSize = true;
            this.MetadataTable.TabIndex = 0;
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
            // VersionTextBox
            //
            this.VersionTextBox.AutoSize = true;
            this.VersionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VersionTextBox.Location = new System.Drawing.Point(93, 0);
            this.VersionTextBox.Name = "VersionTextBox";
            this.VersionTextBox.Size = new System.Drawing.Size(250, 30);
            this.VersionTextBox.TabIndex = 2;
            this.VersionTextBox.ReadOnly = true;
            this.VersionTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.VersionTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.VersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.VersionTextBox, "VersionTextBox");
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
            // LicenseTextBox
            //
            this.LicenseTextBox.AutoSize = true;
            this.LicenseTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LicenseTextBox.Location = new System.Drawing.Point(93, 30);
            this.LicenseTextBox.Name = "LicenseTextBox";
            this.LicenseTextBox.Size = new System.Drawing.Size(250, 30);
            this.LicenseTextBox.TabIndex = 4;
            this.LicenseTextBox.ReadOnly = true;
            this.LicenseTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.LicenseTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LicenseTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.LicenseTextBox, "LicenseTextBox");
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
            // AuthorsPanel
            //
            this.AuthorsPanel.AutoSize = true;
            this.AuthorsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AuthorsPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.AuthorsPanel.Padding = new System.Windows.Forms.Padding(0);
            this.AuthorsPanel.Location = new System.Drawing.Point(0, 0);
            this.AuthorsPanel.Name = "AuthorsPanel";
            this.AuthorsPanel.TabIndex = 6;
            this.AuthorsPanel.Size = new System.Drawing.Size(500, 20);
            //
            // DownloadCountLabel
            //
            this.DownloadCountLabel.AutoSize = true;
            this.DownloadCountLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DownloadCountLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.DownloadCountLabel.Location = new System.Drawing.Point(0, 3);
            this.DownloadCountLabel.Name = "DownloadCountLabel";
            this.DownloadCountLabel.Size = new System.Drawing.Size(84, 30);
            this.DownloadCountLabel.TabIndex = 7;
            resources.ApplyResources(this.DownloadCountLabel, "DownloadCountLabel");
            //
            // DownloadCountTextBox
            //
            this.DownloadCountTextBox.AutoSize = true;
            this.DownloadCountTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DownloadCountTextBox.Location = new System.Drawing.Point(0, 3);
            this.DownloadCountTextBox.Name = "DownloadCountTextBox";
            this.DownloadCountTextBox.Size = new System.Drawing.Size(250, 30);
            this.DownloadCountTextBox.TabIndex = 8;
            this.DownloadCountTextBox.ReadOnly = true;
            this.DownloadCountTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.DownloadCountTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.DownloadCountTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.DownloadCountTextBox, "DownloadCountTextBox");
            //
            // ReleaseLabel
            //
            this.ReleaseLabel.AutoSize = true;
            this.ReleaseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReleaseLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.ReleaseLabel.Location = new System.Drawing.Point(3, 150);
            this.ReleaseLabel.Name = "ReleaseLabel";
            this.ReleaseLabel.Size = new System.Drawing.Size(84, 30);
            this.ReleaseLabel.TabIndex = 9;
            resources.ApplyResources(this.ReleaseLabel, "ReleaseLabel");
            //
            // ReleaseStatusTextBox
            //
            this.ReleaseStatusTextBox.AutoSize = true;
            this.ReleaseStatusTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReleaseStatusTextBox.Location = new System.Drawing.Point(93, 150);
            this.ReleaseStatusTextBox.Name = "ReleaseStatusTextBox";
            this.ReleaseStatusTextBox.Size = new System.Drawing.Size(250, 30);
            this.ReleaseStatusTextBox.TabIndex = 10;
            this.ReleaseStatusTextBox.ReadOnly = true;
            this.ReleaseStatusTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.ReleaseStatusTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ReleaseStatusTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.ReleaseStatusTextBox, "ReleaseStatusTextBox");
            //
            // GameCompatibilityLabel
            //
            this.GameCompatibilityLabel.AutoSize = true;
            this.GameCompatibilityLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GameCompatibilityLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.GameCompatibilityLabel.Location = new System.Drawing.Point(3, 180);
            this.GameCompatibilityLabel.Name = "GameCompatibilityLabel";
            this.GameCompatibilityLabel.Size = new System.Drawing.Size(84, 30);
            this.GameCompatibilityLabel.TabIndex = 11;
            resources.ApplyResources(this.GameCompatibilityLabel, "GameCompatibilityLabel");
            //
            // GameCompatibilityTextBox
            //
            this.GameCompatibilityTextBox.AutoSize = true;
            this.GameCompatibilityTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GameCompatibilityTextBox.Location = new System.Drawing.Point(93, 180);
            this.GameCompatibilityTextBox.Name = "GameCompatibilityTextBox";
            this.GameCompatibilityTextBox.Size = new System.Drawing.Size(250, 30);
            this.GameCompatibilityTextBox.TabIndex = 12;
            this.GameCompatibilityTextBox.ReadOnly = true;
            this.GameCompatibilityTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.GameCompatibilityTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.GameCompatibilityTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.GameCompatibilityTextBox, "GameCompatibilityTextBox");
            //
            // IdentifierLabel
            //
            this.IdentifierLabel.AutoSize = true;
            this.IdentifierLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IdentifierLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.IdentifierLabel.Location = new System.Drawing.Point(3, 210);
            this.IdentifierLabel.Name = "IdentifierLabel";
            this.IdentifierLabel.Size = new System.Drawing.Size(84, 20);
            this.IdentifierLabel.TabIndex = 13;
            resources.ApplyResources(this.IdentifierLabel, "IdentifierLabel");
            //
            // IdentifierTextBox
            //
            this.IdentifierTextBox.AutoSize = true;
            this.IdentifierTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IdentifierTextBox.Location = new System.Drawing.Point(93, 210);
            this.IdentifierTextBox.Name = "IdentifierTextBox";
            this.IdentifierTextBox.Size = new System.Drawing.Size(250, 20);
            this.IdentifierTextBox.TabIndex = 14;
            this.IdentifierTextBox.ReadOnly = true;
            this.IdentifierTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.IdentifierTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.IdentifierTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.IdentifierTextBox, "IdentifierTextBox");
            //
            // ReplacementLabel
            //
            this.ReplacementLabel.AutoSize = true;
            this.ReplacementLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReplacementLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.ReplacementLabel.Location = new System.Drawing.Point(3, 240);
            this.ReplacementLabel.Name = "ReplacementLabel";
            this.ReplacementLabel.Size = new System.Drawing.Size(84, 20);
            this.ReplacementLabel.TabIndex = 15;
            resources.ApplyResources(this.ReplacementLabel, "ReplacementLabel");
            //
            // ReplacementTextBox
            //
            this.ReplacementTextBox.AutoSize = true;
            this.ReplacementTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReplacementTextBox.Location = new System.Drawing.Point(93, 240);
            this.ReplacementTextBox.Name = "ReplacementTextBox";
            this.ReplacementTextBox.Size = new System.Drawing.Size(250, 20);
            this.ReplacementTextBox.TabIndex = 16;
            this.ReplacementTextBox.ReadOnly = true;
            this.ReplacementTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.ReplacementTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ReplacementTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.ReplacementTextBox, "ReplacementTextBox");
            //
            // Metadata
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MetadataTable);
            this.Name = "Metadata";
            this.Size = new System.Drawing.Size(362, 531);
            resources.ApplyResources(this, "$this");
            this.MetadataTable.ResumeLayout(false);
            this.MetadataTable.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.TableLayoutPanel MetadataTable;
        private System.Windows.Forms.Label VersionLabel;
        private CKAN.GUI.TransparentTextBox VersionTextBox;
        private System.Windows.Forms.Label LicenseLabel;
        private CKAN.GUI.TransparentTextBox LicenseTextBox;
        private System.Windows.Forms.Label AuthorLabel;
        private System.Windows.Forms.FlowLayoutPanel AuthorsPanel;
        private System.Windows.Forms.Label DownloadCountLabel;
        private CKAN.GUI.TransparentTextBox DownloadCountTextBox;
        private System.Windows.Forms.Label ReleaseLabel;
        private CKAN.GUI.TransparentTextBox ReleaseStatusTextBox;
        private System.Windows.Forms.Label GameCompatibilityLabel;
        private CKAN.GUI.TransparentTextBox GameCompatibilityTextBox;
        private System.Windows.Forms.Label IdentifierLabel;
        private CKAN.GUI.TransparentTextBox IdentifierTextBox;
        private System.Windows.Forms.Label ReplacementLabel;
        private CKAN.GUI.TransparentTextBox ReplacementTextBox;
    }
}
