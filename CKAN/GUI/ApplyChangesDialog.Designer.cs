namespace CKAN
{
    partial class ApplyChangesDialog
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "Test",
            "Test2"}, -1);
            this.ChangesListView = new System.Windows.Forms.ListView();
            this.ModName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ChangeType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ConfirmButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ChangesListView
            // 
            this.ChangesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ModName,
            this.ChangeType});
            this.ChangesListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.ChangesListView.Location = new System.Drawing.Point(12, 12);
            this.ChangesListView.Name = "ChangesListView";
            this.ChangesListView.Size = new System.Drawing.Size(451, 259);
            this.ChangesListView.TabIndex = 0;
            this.ChangesListView.UseCompatibleStateImageBehavior = false;
            this.ChangesListView.View = System.Windows.Forms.View.Details;
            // 
            // ModName
            // 
            this.ModName.Text = "Mod";
            this.ModName.Width = 332;
            // 
            // ChangeType
            // 
            this.ChangeType.Text = "Change";
            this.ChangeType.Width = 111;
            // 
            // ConfirmButton
            // 
            this.ConfirmButton.Location = new System.Drawing.Point(390, 277);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = new System.Drawing.Size(75, 23);
            this.ConfirmButton.TabIndex = 2;
            this.ConfirmButton.Text = "Confirm";
            this.ConfirmButton.UseVisualStyleBackColor = true;
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(309, 277);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // ApplyChangesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(475, 312);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.ConfirmButton);
            this.Controls.Add(this.ChangesListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ApplyChangesDialog";
            this.Text = "Apply changes";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView ChangesListView;
        private System.Windows.Forms.Button ConfirmButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.ColumnHeader ModName;
        private System.Windows.Forms.ColumnHeader ChangeType;

    }
}