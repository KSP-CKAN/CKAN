namespace CKAN
{
    partial class RenameInstanceDialog
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
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelRenameInstanceButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // NameTextBox
            // 
            this.NameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NameTextBox.Location = new System.Drawing.Point(13, 13);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(294, 20);
            this.NameTextBox.TabIndex = 0;
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OKButton.Location = new System.Drawing.Point(232, 39);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // CancelRenameInstanceButton
            // 
            this.CancelRenameInstanceButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelRenameInstanceButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelRenameInstanceButton.Location = new System.Drawing.Point(151, 39);
            this.CancelRenameInstanceButton.Name = "CancelRenameInstanceButton";
            this.CancelRenameInstanceButton.Size = new System.Drawing.Size(75, 23);
            this.CancelRenameInstanceButton.TabIndex = 2;
            this.CancelRenameInstanceButton.Text = "Cancel";
            this.CancelRenameInstanceButton.UseVisualStyleBackColor = true;
            // 
            // RenameInstanceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(319, 68);
            this.ControlBox = false;
            this.Controls.Add(this.CancelRenameInstanceButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.NameTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "RenameInstanceDialog";
            this.Text = "Rename installation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelRenameInstanceButton;
    }
}
