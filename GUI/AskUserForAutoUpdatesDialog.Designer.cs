namespace CKAN
{
    partial class AskUserForAutoUpdatesDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AskUserForAutoUpdatesDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.YesButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(511, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Do you wish for CKAN to automatically check for updates on start-up? (Can be chan" +
    "ged later from Settings)";
            // 
            // YesButton
            // 
            this.YesButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.YesButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.YesButton.Location = new System.Drawing.Point(401, 44);
            this.YesButton.Name = "YesButton";
            this.YesButton.Size = new System.Drawing.Size(149, 23);
            this.YesButton.TabIndex = 1;
            this.YesButton.Text = "Yes, check for updates";
            this.YesButton.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.No;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Location = new System.Drawing.Point(323, 44);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "No";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // AskUserForAutoUpdatesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 79);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.YesButton);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AskUserForAutoUpdatesDialog";
            this.Text = "Check for updates";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button YesButton;
        private System.Windows.Forms.Button button1;
    }
}