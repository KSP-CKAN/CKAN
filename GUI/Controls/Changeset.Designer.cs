namespace CKAN.GUI
{
    partial class Changeset
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(Changeset));
            this.ChangesGrid = new System.Windows.Forms.DataGridView();
            this.ModColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ChangeTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReasonsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DeleteColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.CloseTheGameLabel = new System.Windows.Forms.Label();
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.ConfirmChangesButton = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.CancelChangesButton = new System.Windows.Forms.Button();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // ChangesGrid
            //
            this.ChangesGrid.AllowUserToResizeColumns = true;
            this.ChangesGrid.AllowUserToOrderColumns = true;
            this.ChangesGrid.AllowUserToResizeRows = false;
            this.ChangesGrid.AutoGenerateColumns = false;
            this.ChangesGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.ChangesGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ChangesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChangesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ModColumn,
            this.ChangeTypeColumn,
            this.ReasonsColumn,
            this.DeleteColumn});
            this.ChangesGrid.ColumnHeadersDefaultCellStyle.Padding = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.ChangesGrid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
            this.ChangesGrid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ChangesGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Control;
            this.ChangesGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Raised;
            this.ChangesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ChangesGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.ChangesGrid.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ChangesGrid.EnableHeadersVisualStyles = false;
            this.ChangesGrid.Location = new System.Drawing.Point(-2, 0);
            this.ChangesGrid.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ChangesGrid.MultiSelect = false;
            this.ChangesGrid.Name = "ChangesGrid";
            this.ChangesGrid.RowHeadersVisible = false;
            this.ChangesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ChangesGrid.ShowCellToolTips = true;
            this.ChangesGrid.Size = new System.Drawing.Size(1532, 886);
            this.ChangesGrid.TabIndex = 0;
            this.ChangesGrid.TabStop = false;
            this.ChangesGrid.SelectionChanged += new System.EventHandler(this.ChangesGrid_SelectionChanged);
            this.ChangesGrid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.ChangesGrid_DataBindingComplete);
            this.ChangesGrid.CellClick += this.ChangesGrid_CellClick;
            //
            // ModColumn
            //
            this.ModColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.ModColumn.Width = 332;
            this.ModColumn.ValueType = typeof(string);
            this.ModColumn.DataPropertyName = "Mod";
            this.ModColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            resources.ApplyResources(this.ModColumn, "ModColumn");
            //
            // ChangeTypeColumn
            //
            this.ReasonsColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.ChangeTypeColumn.Width = 111;
            this.ChangeTypeColumn.ValueType = typeof(string);
            this.ChangeTypeColumn.DataPropertyName = "ChangeType";
            this.ChangeTypeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            resources.ApplyResources(this.ChangeTypeColumn, "ChangeTypeColumn");
            //
            // ReasonsColumn
            //
            this.ReasonsColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ReasonsColumn.Width = 606;
            this.ReasonsColumn.ValueType = typeof(string);
            this.ReasonsColumn.DataPropertyName = "Reasons";
            this.ReasonsColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            resources.ApplyResources(this.ReasonsColumn, "ReasonsColumn");
            //
            // DeleteColumn
            //
            this.DeleteColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.DeleteColumn.Width = 200;
            this.DeleteColumn.ValueType = typeof(System.Drawing.Bitmap);
            this.DeleteColumn.DataPropertyName = "DeleteImage";
            this.DeleteColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            resources.ApplyResources(this.DeleteColumn, "DeleteColumn");
            //
            // CloseTheGameLabel
            //
            this.CloseTheGameLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.CloseTheGameLabel.Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.Name, 12, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.CloseTheGameLabel.Location = new System.Drawing.Point(9, 18);
            this.CloseTheGameLabel.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.CloseTheGameLabel.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.CloseTheGameLabel.Name = "CloseTheGameLabel";
            this.CloseTheGameLabel.Size = new System.Drawing.Size(490, 50);
            this.CloseTheGameLabel.TabIndex = 0;
            this.CloseTheGameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            resources.ApplyResources(this.CloseTheGameLabel, "CloseTheGameLabel");
            //
            // BottomButtonPanel
            //
            this.BottomButtonPanel.RightControls.Add(this.ConfirmChangesButton);
            this.BottomButtonPanel.RightControls.Add(this.CancelChangesButton);
            this.BottomButtonPanel.RightControls.Add(this.BackButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            //
            // BackButton
            //
            this.BackButton.AutoSize = true;
            this.BackButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.BackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BackButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(112, 30);
            this.BackButton.TabIndex = 1;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            resources.ApplyResources(this.BackButton, "BackButton");
            //
            // CancelChangesButton
            //
            this.CancelChangesButton.AutoSize = true;
            this.CancelChangesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.CancelChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelChangesButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CancelChangesButton.Name = "CancelChangesButton";
            this.CancelChangesButton.Size = new System.Drawing.Size(112, 30);
            this.CancelChangesButton.TabIndex = 1;
            this.CancelChangesButton.Click += new System.EventHandler(this.CancelChangesButton_Click);
            resources.ApplyResources(this.CancelChangesButton, "CancelChangesButton");
            //
            // ConfirmChangesButton
            //
            this.ConfirmChangesButton.AutoSize = true;
            this.ConfirmChangesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.ConfirmChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConfirmChangesButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ConfirmChangesButton.Name = "ConfirmChangesButton";
            this.ConfirmChangesButton.Size = new System.Drawing.Size(112, 30);
            this.ConfirmChangesButton.TabIndex = 2;
            this.ConfirmChangesButton.Click += new System.EventHandler(this.ConfirmChangesButton_Click);
            resources.ApplyResources(this.ConfirmChangesButton, "ConfirmChangesButton");
            //
            // Changeset
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.ChangesGrid);
            this.Controls.Add(this.CloseTheGameLabel);
            this.Controls.Add(this.BottomButtonPanel);
            this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.Name = "Changeset";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DataGridView ChangesGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn ModColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ChangeTypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ReasonsColumn;
        private System.Windows.Forms.DataGridViewImageColumn DeleteColumn;
        private System.Windows.Forms.Label CloseTheGameLabel;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Button CancelChangesButton;
        private System.Windows.Forms.Button ConfirmChangesButton;
    }
}
