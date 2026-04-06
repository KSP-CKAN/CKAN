namespace CKAN.GUI
{
    partial class DownloadsFailedDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(DownloadsFailedDialog));
            this.ExplanationLabel = new System.Windows.Forms.Label();
            this.DownloadsGrid = new System.Windows.Forms.DataGridView();
            this.RetryColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.SkipColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ModColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ErrorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GridContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.CopyErrorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.RetryButton = new System.Windows.Forms.Button();
            this.AbortButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DownloadsGrid)).BeginInit();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // ExplanationLabel
            //
            this.ExplanationLabel.AutoSize = true;
            this.ExplanationLabel.MaximumSize = new System.Drawing.Size(490, 0);
            this.ExplanationLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ExplanationLabel.Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont, System.Drawing.FontStyle.Bold);
            this.ExplanationLabel.Location = new System.Drawing.Point(5, 0);
            this.ExplanationLabel.Name = "ExplanationLabel";
            this.ExplanationLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.ExplanationLabel.Size = new System.Drawing.Size(490, 60);
            resources.ApplyResources(this.ExplanationLabel, "ExplanationLabel");
            //
            // DownloadsGrid
            //
            this.DownloadsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DownloadsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DownloadsGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.DownloadsGrid.AllowUserToAddRows = false;
            this.DownloadsGrid.AllowUserToDeleteRows = false;
            this.DownloadsGrid.AllowUserToResizeRows = false;
            this.DownloadsGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.DownloadsGrid.EnableHeadersVisualStyles = false;
            this.DownloadsGrid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
            this.DownloadsGrid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.DownloadsGrid.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.DownloadsGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.DownloadsGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.DownloadsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DownloadsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RetryColumn,
            this.SkipColumn,
            this.ModColumn,
            this.ErrorColumn});
            this.DownloadsGrid.Location = new System.Drawing.Point(0, 111);
            this.DownloadsGrid.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.DownloadsGrid.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.DownloadsGrid.MultiSelect = false;
            this.DownloadsGrid.Name = "DownloadsGrid";
            this.DownloadsGrid.RowHeadersVisible = false;
            this.DownloadsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DownloadsGrid.Size = new System.Drawing.Size(1536, 837);
            this.DownloadsGrid.StandardTab = true;
            this.DownloadsGrid.TabIndex = 0;
            this.DownloadsGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DownloadsGrid_CellContentClick);
            this.DownloadsGrid.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DownloadsGrid_CellMouseDoubleClick);
            this.DownloadsGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DownloadsGrid_CellEndEdit);
            this.DownloadsGrid.SelectionChanged += new System.EventHandler(DownloadsGrid_SelectionChanged);
            this.DownloadsGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DownloadsGrid_MouseDown);
            //
            // RetryColumn
            //
            this.RetryColumn.Name = "RetryColumn";
            this.RetryColumn.DataPropertyName = "Retry";
            this.RetryColumn.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.RetryColumn.Width = 46;
            this.RetryColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            resources.ApplyResources(this.RetryColumn, "RetryColumn");
            //
            // SkipColumn
            //
            this.SkipColumn.Name = "SkipColumn";
            this.SkipColumn.DataPropertyName = "Skip";
            this.SkipColumn.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.SkipColumn.Width = 46;
            this.SkipColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            resources.ApplyResources(this.SkipColumn, "SkipColumn");
            //
            // ModColumn
            //
            this.ModColumn.Name = "ModColumn";
            this.ModColumn.DataPropertyName = "Data";
            this.ModColumn.ReadOnly = true;
            this.ModColumn.Width = 250;
            this.ModColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            resources.ApplyResources(this.ModColumn, "ModColumn");
            //
            // ErrorColumn
            //
            this.ErrorColumn.Name = "ErrorColumn";
            this.ErrorColumn.DataPropertyName = "Error";
            this.ErrorColumn.ReadOnly = true;
            this.ErrorColumn.Width = 500;
            this.ErrorColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ErrorColumn.FillWeight = 500;
            this.ErrorColumn.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            resources.ApplyResources(this.ErrorColumn, "ErrorColumn");
            //
            // GridContextMenuStrip
            //
            this.GridContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CopyErrorToolStripMenuItem});
            this.GridContextMenuStrip.Name = "GridContextMenuStrip";
            this.GridContextMenuStrip.Size = new System.Drawing.Size(180, 70);
            //
            // CopyErrorToolStripMenuItem
            //
            this.CopyErrorToolStripMenuItem.Name = "CopyErrorToolStripMenuItem";
            this.CopyErrorToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.CopyErrorToolStripMenuItem.Click += new System.EventHandler(this.CopyErrorToolStripMenuItem_Click);
            resources.ApplyResources(this.CopyErrorToolStripMenuItem, "CopyErrorToolStripMenuItem");
            //
            // BottomButtonPanel
            //
            this.BottomButtonPanel.RightControls.Add(this.AbortButton);
            this.BottomButtonPanel.RightControls.Add(this.RetryButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            //
            // RetryButton
            //
            this.RetryButton.AutoSize = true;
            this.RetryButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.RetryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RetryButton.Name = "RetryButton";
            this.RetryButton.Size = new System.Drawing.Size(112, 30);
            this.RetryButton.TabIndex = 3;
            this.RetryButton.Click += new System.EventHandler(this.RetryButton_Click);
            resources.ApplyResources(this.RetryButton, "RetryButton");
            //
            // AbortButton
            //
            this.AbortButton.AutoSize = true;
            this.AbortButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.AbortButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AbortButton.Name = "AbortButton";
            this.AbortButton.Size = new System.Drawing.Size(112, 30);
            this.AbortButton.TabIndex = 3;
            this.AbortButton.Click += new System.EventHandler(this.AbortButton_Click);
            resources.ApplyResources(this.AbortButton, "AbortButton");
            //
            // DownloadsFailedDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.DownloadsGrid);
            this.Controls.Add(this.ExplanationLabel);
            this.Controls.Add(this.BottomButtonPanel);
            this.ClientSize = new System.Drawing.Size(800, 300);
            this.MinimumSize = new System.Drawing.Size(675, 200);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Icon = EmbeddedImages.AppIcon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
            this.HelpButton = true;
            this.Name = "DownloadsFailedDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)(this.DownloadsGrid)).EndInit();
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label ExplanationLabel;
        private System.Windows.Forms.DataGridView DownloadsGrid;
        private System.Windows.Forms.DataGridViewCheckBoxColumn RetryColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn SkipColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ModColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ErrorColumn;
        private System.Windows.Forms.ContextMenuStrip GridContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem CopyErrorToolStripMenuItem;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Button RetryButton;
        private System.Windows.Forms.Button AbortButton;
    }
}
