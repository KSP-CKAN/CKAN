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
            this.AuthorsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.MetadataModuleReleaseStatusTextBox = new TransparentTextBox();
            this.MetadataModuleGameCompatibilityTextBox = new TransparentTextBox();
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
            this.MetadataTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.MetadataTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MetadataTable.Controls.Add(this.VersionLabel, 0, 0);
            this.MetadataTable.Controls.Add(this.MetadataModuleVersionTextBox, 1, 0);
            this.MetadataTable.Controls.Add(this.LicenseLabel, 0, 1);
            this.MetadataTable.Controls.Add(this.MetadataModuleLicenseTextBox, 1, 1);
            this.MetadataTable.Controls.Add(this.AuthorLabel, 0, 2);
            this.MetadataTable.Controls.Add(this.AuthorsPanel, 1, 2);
            this.MetadataTable.Controls.Add(this.ReleaseLabel, 0, 3);
            this.MetadataTable.Controls.Add(this.MetadataModuleReleaseStatusTextBox, 1, 3);
            this.MetadataTable.Controls.Add(this.GameCompatibilityLabel, 0, 4);
            this.MetadataTable.Controls.Add(this.MetadataModuleGameCompatibilityTextBox, 1, 4);
            this.MetadataTable.Controls.Add(this.IdentifierLabel, 0, 5);
            this.MetadataTable.Controls.Add(this.MetadataIdentifierTextBox, 1, 5);
            this.MetadataTable.Controls.Add(this.ReplacementLabel, 0, 6);
            this.MetadataTable.Controls.Add(this.ReplacementTextBox, 1, 6);
            this.MetadataTable.Dock = System.Windows.Forms.DockStyle.Top;
            this.MetadataTable.Location = new System.Drawing.Point(0, 0);
            this.MetadataTable.Name = "MetadataTable";
            this.MetadataTable.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.MetadataTable.RowCount = 7;
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetadataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MetadataTable.Size = new System.Drawing.Size(346, 255);
            this.MetadataTable.AutoSize = true;
            this.MetadataTable.TabIndex = 0;
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
            this.MetadataIdentifierTextBox.BackColor = System.Drawing.SystemColors.Control;
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
            this.ReplacementTextBox.BackColor = System.Drawing.SystemColors.Control;
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
            this.MetadataModuleVersionTextBox.BackColor = System.Drawing.SystemColors.Control;
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
            this.MetadataModuleLicenseTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.MetadataModuleLicenseTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleLicenseTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.MetadataModuleLicenseTextBox, "MetadataModuleLicenseTextBox");
            //
            // AuthorsPanel
            //
            this.AuthorsPanel.AutoSize = true;
            this.AuthorsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AuthorsPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.AuthorsPanel.Padding = new System.Windows.Forms.Padding(0);
            this.AuthorsPanel.Location = new System.Drawing.Point(0, 0);
            this.AuthorsPanel.Name = "AuthorsPanel";
            this.AuthorsPanel.Size = new System.Drawing.Size(500, 20);
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
            this.MetadataModuleReleaseStatusTextBox.BackColor = System.Drawing.SystemColors.Control;
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
            this.MetadataModuleGameCompatibilityTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.MetadataModuleGameCompatibilityTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MetadataModuleGameCompatibilityTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.MetadataModuleGameCompatibilityTextBox, "MetadataModuleGameCompatibilityTextBox");
            //
            // Metadata
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
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
        private System.Windows.Forms.FlowLayoutPanel AuthorsPanel;
        private System.Windows.Forms.Label VersionLabel;
        private TransparentTextBox MetadataModuleReleaseStatusTextBox;
        private TransparentTextBox MetadataModuleGameCompatibilityTextBox;
    }
}
