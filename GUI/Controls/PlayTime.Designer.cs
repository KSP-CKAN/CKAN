namespace CKAN.GUI
{
    partial class PlayTime
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(PlayTime));
            this.EditHelpLabel = new System.Windows.Forms.Label();
            this.PlayTimeGrid = new System.Windows.Forms.DataGridView();
            this.NameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.TotalLabel = new System.Windows.Forms.Label();
            this.OKButton = new System.Windows.Forms.Button();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // EditHelpLabel
            //
            this.EditHelpLabel.AutoSize = true;
            this.EditHelpLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.EditHelpLabel.BackColor = System.Drawing.Color.Transparent;
            this.EditHelpLabel.Location = new System.Drawing.Point(4, 5);
            this.EditHelpLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.EditHelpLabel.Name = "EditHelpLabel";
            this.EditHelpLabel.Size = new System.Drawing.Size(147, 20);
            this.EditHelpLabel.TabIndex = 0;
            this.EditHelpLabel.Visible = true;
            resources.ApplyResources(this.EditHelpLabel, "EditHelpLabel");
            // 
            // PlayTimeGrid
            // 
            this.PlayTimeGrid.AutoGenerateColumns = false;
            this.PlayTimeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PlayTimeGrid.AllowUserToAddRows = false;
            this.PlayTimeGrid.AllowUserToDeleteRows = false;
            this.PlayTimeGrid.AllowUserToResizeRows = false;
            this.PlayTimeGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.PlayTimeGrid.EnableHeadersVisualStyles = false;
            this.PlayTimeGrid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
            this.PlayTimeGrid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PlayTimeGrid.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.PlayTimeGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.PlayTimeGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.PlayTimeGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PlayTimeGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NameColumn,
            this.TimeColumn});
            this.PlayTimeGrid.Location = new System.Drawing.Point(0, 111);
            this.PlayTimeGrid.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.PlayTimeGrid.MultiSelect = false;
            this.PlayTimeGrid.Name = "PlayTimeGrid";
            this.PlayTimeGrid.RowHeadersVisible = false;
            this.PlayTimeGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.PlayTimeGrid.Size = new System.Drawing.Size(1536, 837);
            this.PlayTimeGrid.StandardTab = false;
            this.PlayTimeGrid.TabIndex = 1;
            this.PlayTimeGrid.CellValueChanged += this.PlayTimeGrid_CellValueChanged;
            // 
            // NameColumn
            // 
            this.NameColumn.Name = "NameColumn";
            this.NameColumn.DataPropertyName = "Name";
            this.NameColumn.ReadOnly = true;
            this.NameColumn.Width = 250;
            resources.ApplyResources(this.NameColumn, "NameColumn");
            // 
            // TimeColumn
            // 
            this.TimeColumn.Name = "TimeColumn";
            this.TimeColumn.DataPropertyName = "Time";
            this.TimeColumn.ReadOnly = false;
            this.TimeColumn.ValueType = typeof(double);
            this.TimeColumn.Width = 120;
            this.TimeColumn.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            resources.ApplyResources(this.TimeColumn, "TimeColumn");
            // 
            // BottomButtonPanel
            // 
            this.BottomButtonPanel.RightControls.Add(this.OKButton);
            this.BottomButtonPanel.LeftControls.Add(this.TotalLabel);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            //
            // TotalLabel
            //
            this.TotalLabel.AutoSize = true;
            this.TotalLabel.BackColor = System.Drawing.Color.Transparent;
            this.TotalLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.TotalLabel.Name = "TotalLabel";
            this.TotalLabel.Size = new System.Drawing.Size(147, 20);
            this.TotalLabel.TabIndex = 2;
            this.TotalLabel.Visible = true;
            resources.ApplyResources(this.TotalLabel, "TotalLabel");
            //
            // OKButton
            //
            this.OKButton.AutoSize = true;
            this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(112, 30);
            this.OKButton.TabIndex = 3;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            resources.ApplyResources(this.OKButton, "OKButton");
            // 
            // PlayTime
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.PlayTimeGrid);
            this.Controls.Add(this.EditHelpLabel);
            this.Controls.Add(this.BottomButtonPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "PlayTime";
            this.Size = new System.Drawing.Size(500, 500);
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label EditHelpLabel;
        private System.Windows.Forms.DataGridView PlayTimeGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn TimeColumn;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Label TotalLabel;
        private System.Windows.Forms.Button OKButton;
    }
}
