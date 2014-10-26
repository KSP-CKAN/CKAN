namespace CKAN
{
    partial class WaitDialog
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
            this.MessageTextBox = new System.Windows.Forms.TextBox();
            this.DialogProgressBar = new System.Windows.Forms.ProgressBar();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.CancelCurrentActionButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // MessageTextBox
            // 
            this.MessageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MessageTextBox.Enabled = false;
            this.MessageTextBox.Location = new System.Drawing.Point(12, 12);
            this.MessageTextBox.Multiline = true;
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.ReadOnly = true;
            this.MessageTextBox.Size = new System.Drawing.Size(460, 17);
            this.MessageTextBox.TabIndex = 0;
            this.MessageTextBox.Text = "Waiting for operation to complete";
            this.MessageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // DialogProgressBar
            // 
            this.DialogProgressBar.Location = new System.Drawing.Point(13, 35);
            this.DialogProgressBar.Name = "DialogProgressBar";
            this.DialogProgressBar.Size = new System.Drawing.Size(459, 23);
            this.DialogProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.DialogProgressBar.TabIndex = 1;
            // 
            // LogTextBox
            // 
            this.LogTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LogTextBox.Location = new System.Drawing.Point(13, 64);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.Size = new System.Drawing.Size(459, 175);
            this.LogTextBox.TabIndex = 2;
            // 
            // CancelCurrentActionButton
            // 
            this.CancelCurrentActionButton.Location = new System.Drawing.Point(397, 245);
            this.CancelCurrentActionButton.Name = "CancelCurrentActionButton";
            this.CancelCurrentActionButton.Size = new System.Drawing.Size(75, 23);
            this.CancelCurrentActionButton.TabIndex = 3;
            this.CancelCurrentActionButton.Text = "Cancel";
            this.CancelCurrentActionButton.UseVisualStyleBackColor = true;
            this.CancelCurrentActionButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // WaitDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 280);
            this.ControlBox = false;
            this.Controls.Add(this.CancelCurrentActionButton);
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.DialogProgressBar);
            this.Controls.Add(this.MessageTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "WaitDialog";
            this.Text = "Please wait";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox MessageTextBox;
        private System.Windows.Forms.ProgressBar DialogProgressBar;
        private System.Windows.Forms.TextBox LogTextBox;
        private System.Windows.Forms.Button CancelCurrentActionButton;

    }
}