namespace CKAN.GUI
{
    partial class GameCommandLineOptionsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(GameCommandLineOptionsDialog));
            this.CmdLineGrid = new System.Windows.Forms.DataGridView();
            this.CmdLineColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.ResetToDefaultsButton = new System.Windows.Forms.Button();
            this.AddButton = new System.Windows.Forms.Button();
            this.AcceptChangesButton = new System.Windows.Forms.Button();
            this.CancelChangesButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // CmdLineGrid
            //
            this.CmdLineGrid.AutoGenerateColumns = false;
            this.CmdLineGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CmdLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CmdLineGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnKeystrokeOrF2;
            this.CmdLineGrid.AllowUserToAddRows = true;
            this.CmdLineGrid.AllowUserToDeleteRows = true;
            this.CmdLineGrid.AllowUserToResizeRows = false;
            this.CmdLineGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.CmdLineGrid.EnableHeadersVisualStyles = false;
            this.CmdLineGrid.ColumnHeadersDefaultCellStyle.Padding = new System.Windows.Forms.Padding(1, 3, 1, 3);
            this.CmdLineGrid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
            this.CmdLineGrid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.CmdLineGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Control;
            this.CmdLineGrid.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.CmdLineGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.CmdLineGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Raised;
            this.CmdLineGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.CmdLineGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CmdLineColumn});
            this.CmdLineGrid.Location = new System.Drawing.Point(12, 9);
            this.CmdLineGrid.MultiSelect = false;
            this.CmdLineGrid.Name = "CmdLineGrid";
            this.CmdLineGrid.RowHeadersVisible = false;
            this.CmdLineGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.CmdLineGrid.Size = new System.Drawing.Size(457, 20);
            this.CmdLineGrid.StandardTab = false;
            this.CmdLineGrid.TabIndex = 1;
            this.CmdLineGrid.EditingControlShowing += this.CmdLineGrid_EditingControlShowing;
            this.CmdLineGrid.UserDeletingRow += this.CmdLineGrid_UserDeletingRow;
            //
            // CmdLineColumn
            //
            this.CmdLineColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.CmdLineColumn.Name = "CmdLineColumn";
            this.CmdLineColumn.DataPropertyName = "CmdLine";
            this.CmdLineColumn.ReadOnly = false;
            this.CmdLineColumn.ValueType = typeof(string);
            this.CmdLineColumn.Width = 250;
            resources.ApplyResources(this.CmdLineColumn, "CmdLineColumn");
            //
            // BottomButtonPanel
            //
            this.BottomButtonPanel.LeftControls.Add(this.ResetToDefaultsButton);
            this.BottomButtonPanel.LeftControls.Add(this.AddButton);
            this.BottomButtonPanel.RightControls.Add(this.AcceptChangesButton);
            this.BottomButtonPanel.RightControls.Add(this.CancelChangesButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            //
            // ResetToDefaultsButton
            //
            this.ResetToDefaultsButton.AutoSize = true;
            this.ResetToDefaultsButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.ResetToDefaultsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ResetToDefaultsButton.Location = new System.Drawing.Point(316, 51);
            this.ResetToDefaultsButton.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.ResetToDefaultsButton.Name = "ResetToDefaultsButton";
            this.ResetToDefaultsButton.Size = new System.Drawing.Size(75, 23);
            this.ResetToDefaultsButton.TabIndex = 2;
            this.ResetToDefaultsButton.UseVisualStyleBackColor = true;
            this.ResetToDefaultsButton.Click += this.ResetToDefaultsButton_Click;
            resources.ApplyResources(this.ResetToDefaultsButton, "ResetToDefaultsButton");
            //
            // AddButton
            //
            this.AddButton.AutoSize = true;
            this.AddButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.AddButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AddButton.Location = new System.Drawing.Point(316, 51);
            this.AddButton.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(75, 23);
            this.AddButton.TabIndex = 2;
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Visible = false;
            this.AddButton.Click += this.AddButton_Click;
            resources.ApplyResources(this.AddButton, "AddButton");
            //
            // CancelChangesButton
            //
            this.CancelChangesButton.AutoSize = true;
            this.CancelChangesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.CancelChangesButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelChangesButton.Location = new System.Drawing.Point(316, 51);
            this.CancelChangesButton.Margin = new System.Windows.Forms.Padding(8, 4, 0, 4);
            this.CancelChangesButton.Name = "CancelChangesButton";
            this.CancelChangesButton.Size = new System.Drawing.Size(75, 23);
            this.CancelChangesButton.TabIndex = 2;
            this.CancelChangesButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.CancelChangesButton, "CancelChangesButton");
            //
            // AcceptChangesButton
            //
            this.AcceptChangesButton.AutoSize = true;
            this.AcceptChangesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.AcceptChangesButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.AcceptChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AcceptChangesButton.Location = new System.Drawing.Point(397, 51);
            this.AcceptChangesButton.Margin = new System.Windows.Forms.Padding(8, 4, 0, 4);
            this.AcceptChangesButton.Name = "AcceptChangesButton";
            this.AcceptChangesButton.Size = new System.Drawing.Size(75, 23);
            this.AcceptChangesButton.TabIndex = 3;
            this.AcceptChangesButton.UseVisualStyleBackColor = true;
            this.AcceptChangesButton.Click += this.AcceptChangesButton_Click;
            resources.ApplyResources(this.AcceptChangesButton, "AcceptChangesButton");
            //
            // GameCommandLineOptionsDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 180);
            this.ControlBox = false;
            this.Controls.Add(this.CmdLineGrid);
            this.Controls.Add(this.BottomButtonPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = EmbeddedImages.AppIcon;
            this.MinimumSize = new System.Drawing.Size(320, 180);
            this.Name = "GameCommandLineOptionsDialog";
            this.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView CmdLineGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn CmdLineColumn;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Button ResetToDefaultsButton;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.Button AcceptChangesButton;
        private System.Windows.Forms.Button CancelChangesButton;
    }
}
