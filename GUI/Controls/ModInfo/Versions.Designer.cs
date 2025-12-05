namespace CKAN.GUI
{
    partial class Versions
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(Versions));
            this.OverallSummaryLabel = new System.Windows.Forms.Label();
            this.VersionsListView = new ThemedListView();
            this.ModVersion = new System.Windows.Forms.ColumnHeader();
            this.CompatibleGameVersion = new System.Windows.Forms.ColumnHeader();
            this.ReleaseDate = new System.Windows.Forms.ColumnHeader();
            this.LabelTable = new System.Windows.Forms.TableLayoutPanel();
            this.LatestCompatibleLabel = new System.Windows.Forms.Label();
            this.CompatibleLabel = new System.Windows.Forms.Label();
            this.InstalledLabel = new System.Windows.Forms.Label();
            this.PrereleaseLabel = new System.Windows.Forms.Label();
            this.StabilityToleranceLabel = new System.Windows.Forms.Label();
            this.StabilityToleranceComboBox = new System.Windows.Forms.ComboBox();
            this.LabelTable.SuspendLayout();
            this.SuspendLayout();
            //
            // OverallSummaryLabel
            //
            this.OverallSummaryLabel.AutoSize = true;
            this.OverallSummaryLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.OverallSummaryLabel.Location = new System.Drawing.Point(0, 0);
            this.OverallSummaryLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.OverallSummaryLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.OverallSummaryLabel.Name = "OverallSummaryLabel";
            this.OverallSummaryLabel.Size = new System.Drawing.Size(183, 13);
            this.OverallSummaryLabel.TabIndex = 0;
            resources.ApplyResources(this.OverallSummaryLabel, "OverallSummaryLabel");
            //
            // VersionsListView
            //
            this.VersionsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VersionsListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.VersionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ModVersion,
            this.CompatibleGameVersion,
            this.ReleaseDate});
            this.VersionsListView.CheckBoxes = true;
            this.VersionsListView.FullRowSelect = true;
            this.VersionsListView.Location = new System.Drawing.Point(6, 95);
            this.VersionsListView.Name = "VersionsListView";
            this.VersionsListView.Size = new System.Drawing.Size(488, 397);
            this.VersionsListView.TabIndex = 1;
            this.VersionsListView.ShowItemToolTips = true;
            this.VersionsListView.UseCompatibleStateImageBehavior = false;
            this.VersionsListView.View = System.Windows.Forms.View.Details;
            this.VersionsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.VersionsListView_ItemCheck);
            this.VersionsListView.Resize += new System.EventHandler(this.VersionsListView_OnResize);
            this.VersionsListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.VersionsListView_ItemSelectionChanged);
            //
            // ModVersion
            //
            this.ModVersion.Width = 98;
            resources.ApplyResources(this.ModVersion, "ModVersion");
            //
            // CompatibleGameVersion
            //
            this.CompatibleGameVersion.Width = 132;
            resources.ApplyResources(this.CompatibleGameVersion, "CompatibleGameVersion");
            //
            // ReleaseDate
            //
            this.ReleaseDate.Width = 140;
            resources.ApplyResources(this.ReleaseDate, "ReleaseDate");
            //
            // LabelTable
            //
            this.LabelTable.AutoSize = true;
            this.LabelTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.LabelTable.ColumnCount = 1;
            this.LabelTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LabelTable.Controls.Add(this.PrereleaseLabel, 0, 0);
            this.LabelTable.Controls.Add(this.InstalledLabel, 0, 1);
            this.LabelTable.Controls.Add(this.LatestCompatibleLabel, 0, 2);
            this.LabelTable.Controls.Add(this.CompatibleLabel, 0, 3);
            this.LabelTable.Controls.Add(this.StabilityToleranceLabel, 0, 4);
            this.LabelTable.Controls.Add(this.StabilityToleranceComboBox, 0, 5);
            this.LabelTable.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LabelTable.Location = new System.Drawing.Point(0, 0);
            this.LabelTable.Name = "LabelTable";
            this.LabelTable.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.LabelTable.RowCount = 6;
            this.LabelTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LabelTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LabelTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LabelTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LabelTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LabelTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LabelTable.Size = new System.Drawing.Size(346, 255);
            this.LabelTable.TabIndex = 0;
            //
            // LatestCompatibleLabel
            //
            this.LatestCompatibleLabel.AutoSize = true;
            this.LatestCompatibleLabel.BackColor = System.Drawing.Color.Green;
            this.LatestCompatibleLabel.ForeColor = System.Drawing.Color.White;
            this.LatestCompatibleLabel.Location = new System.Drawing.Point(0, 17);
            this.LatestCompatibleLabel.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.LatestCompatibleLabel.Padding = new System.Windows.Forms.Padding(6, 1, 6, 1);
            this.LatestCompatibleLabel.Name = "LatestCompatibleLabel";
            this.LatestCompatibleLabel.Size = new System.Drawing.Size(229, 13);
            this.LatestCompatibleLabel.TabIndex = 5;
            resources.ApplyResources(this.LatestCompatibleLabel, "LatestCompatibleLabel");
            //
            // CompatibleLabel
            //
            this.CompatibleLabel.AutoSize = true;
            this.CompatibleLabel.BackColor = System.Drawing.Color.LightGreen;
            this.CompatibleLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.CompatibleLabel.Location = new System.Drawing.Point(0, 36);
            this.CompatibleLabel.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.CompatibleLabel.Padding = new System.Windows.Forms.Padding(6, 1, 6, 1);
            this.CompatibleLabel.Name = "CompatibleLabel";
            this.CompatibleLabel.Size = new System.Drawing.Size(180, 13);
            this.CompatibleLabel.TabIndex = 6;
            resources.ApplyResources(this.CompatibleLabel, "CompatibleLabel");
            //
            // InstalledLabel
            //
            this.InstalledLabel.AutoSize = true;
            this.InstalledLabel.Font = Styling.Fonts.Bold;
            this.InstalledLabel.BackColor = System.Drawing.SystemColors.Window;
            this.InstalledLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.InstalledLabel.Location = new System.Drawing.Point(0, 55);
            this.InstalledLabel.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.InstalledLabel.Padding = new System.Windows.Forms.Padding(6, 1, 6, 1);
            this.InstalledLabel.Name = "InstalledLabel";
            this.InstalledLabel.Size = new System.Drawing.Size(131, 13);
            this.InstalledLabel.TabIndex = 7;
            this.InstalledLabel.Visible = false;
            resources.ApplyResources(this.InstalledLabel, "InstalledLabel");
            //
            // PrereleaseLabel
            //
            this.PrereleaseLabel.AutoSize = true;
            this.PrereleaseLabel.Font = Styling.Fonts.Italic;
            this.PrereleaseLabel.BackColor = System.Drawing.Color.Gold;
            this.PrereleaseLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.PrereleaseLabel.Location = new System.Drawing.Point(0, 74);
            this.PrereleaseLabel.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.PrereleaseLabel.Padding = new System.Windows.Forms.Padding(6, 1, 6, 1);
            this.PrereleaseLabel.Name = "PrereleaseLabel";
            this.PrereleaseLabel.Size = new System.Drawing.Size(131, 13);
            this.PrereleaseLabel.TabIndex = 7;
            this.PrereleaseLabel.Visible = false;
            resources.ApplyResources(this.PrereleaseLabel, "PrereleaseLabel");
            //
            // StabilityToleranceLabel
            //
            this.StabilityToleranceLabel.AutoSize = true;
            this.StabilityToleranceLabel.Location = new System.Drawing.Point(0, 146);
            this.StabilityToleranceLabel.Name = "StabilityToleranceLabel";
            this.StabilityToleranceLabel.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.StabilityToleranceLabel.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.StabilityToleranceLabel.Size = new System.Drawing.Size(220, 17);
            this.StabilityToleranceLabel.TabStop = false;
            resources.ApplyResources(this.StabilityToleranceLabel, "StabilityToleranceLabel");
            //
            // StabilityToleranceComboBox
            //
            this.StabilityToleranceComboBox.AutoSize = false;
            this.StabilityToleranceComboBox.Location = new System.Drawing.Point(0, 146);
            this.StabilityToleranceComboBox.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.StabilityToleranceComboBox.Padding = new System.Windows.Forms.Padding(0);
            this.StabilityToleranceComboBox.Name = "StabilityToleranceComboBox";
            this.StabilityToleranceComboBox.Size = new System.Drawing.Size(220, 17);
            this.StabilityToleranceComboBox.TabIndex = 8;
            this.StabilityToleranceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.StabilityToleranceComboBox.SelectionChangeCommitted += new System.EventHandler(this.StabilityToleranceComboBox_SelectionChanged);
            this.StabilityToleranceComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.StabilityToleranceComboBox_MouseWheel);
            //
            // Versions
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.VersionsListView);
            this.Controls.Add(this.OverallSummaryLabel);
            this.Controls.Add(this.LabelTable);
            this.Name = "Versions";
            this.Padding = new System.Windows.Forms.Padding(6);
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.LabelTable.ResumeLayout(false);
            this.LabelTable.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label OverallSummaryLabel;
        private CKAN.GUI.ThemedListView VersionsListView;
        private System.Windows.Forms.ColumnHeader ModVersion;
        private System.Windows.Forms.ColumnHeader CompatibleGameVersion;
        private System.Windows.Forms.ColumnHeader ReleaseDate;
        private System.Windows.Forms.TableLayoutPanel LabelTable;
        private System.Windows.Forms.Label LatestCompatibleLabel;
        private System.Windows.Forms.Label CompatibleLabel;
        private System.Windows.Forms.Label InstalledLabel;
        private System.Windows.Forms.Label PrereleaseLabel;
        private System.Windows.Forms.Label StabilityToleranceLabel;
        private System.Windows.Forms.ComboBox StabilityToleranceComboBox;
    }
}
