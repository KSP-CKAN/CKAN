namespace CKAN
{
    partial class ErrorDialog
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
            if(disposing && (components != null))
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.ErrorMessage = new System.Windows.Forms.RichTextBox();
            this.DismissButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ErrorMessage);
            this.panel1.Location = new System.Drawing.Point(13, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(267, 117);
            this.panel1.TabIndex = 0;
            // 
            // ErrorMessage
            // 
            this.ErrorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ErrorMessage.Location = new System.Drawing.Point(0, 0);
            this.ErrorMessage.Name = "ErrorMessage";
            this.ErrorMessage.Size = new System.Drawing.Size(267, 117);
            this.ErrorMessage.TabIndex = 0;
            this.ErrorMessage.Text = "Error!";
            this.ErrorMessage.ReadOnly = true;
            // 
            // DismissButton
            // 
            this.DismissButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DismissButton.Location = new System.Drawing.Point(205, 136);
            this.DismissButton.Name = "DismissButton";
            this.DismissButton.Size = new System.Drawing.Size(75, 23);
            this.DismissButton.TabIndex = 1;
            this.DismissButton.Text = "Dismiss";
            this.DismissButton.UseVisualStyleBackColor = true;
            this.DismissButton.Click += new System.EventHandler(this.DismissButton_Click);
            // 
            // ErrorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 174);
            this.ControlBox = false;
            this.Controls.Add(this.DismissButton);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ErrorDialog";
            this.Text = "Error!";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox ErrorMessage;
        private System.Windows.Forms.Button DismissButton;
    }
}