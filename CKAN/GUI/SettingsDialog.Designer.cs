namespace CKAN
{
    partial class SettingsDialog
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
            this.KSPPathStatusLabel = new System.Windows.Forms.Label();
            this.KSPPathTextBox = new System.Windows.Forms.TextBox();
            this.KSPPathLabel = new System.Windows.Forms.Label();
            this.KSPPathBrowseButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // KSPPathStatusLabel
            // 
            this.KSPPathStatusLabel.AutoSize = true;
            this.KSPPathStatusLabel.Location = new System.Drawing.Point(12, 9);
            this.KSPPathStatusLabel.Name = "KSPPathStatusLabel";
            this.KSPPathStatusLabel.Size = new System.Drawing.Size(453, 13);
            this.KSPPathStatusLabel.TabIndex = 0;
            this.KSPPathStatusLabel.Text = "We\'ve auto-detected Kerbal Space Program\'s root folder for you, you can override " +
    "it if you wish";
            // 
            // KSPPathTextBox
            // 
            this.KSPPathTextBox.Location = new System.Drawing.Point(74, 29);
            this.KSPPathTextBox.Name = "KSPPathTextBox";
            this.KSPPathTextBox.Size = new System.Drawing.Size(317, 20);
            this.KSPPathTextBox.TabIndex = 1;
            this.KSPPathTextBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // KSPPathLabel
            // 
            this.KSPPathLabel.AutoSize = true;
            this.KSPPathLabel.Location = new System.Drawing.Point(12, 33);
            this.KSPPathLabel.Name = "KSPPathLabel";
            this.KSPPathLabel.Size = new System.Drawing.Size(56, 13);
            this.KSPPathLabel.TabIndex = 2;
            this.KSPPathLabel.Text = "KSP Path:";
            // 
            // KSPPathBrowseButton
            // 
            this.KSPPathBrowseButton.Location = new System.Drawing.Point(397, 29);
            this.KSPPathBrowseButton.Name = "KSPPathBrowseButton";
            this.KSPPathBrowseButton.Size = new System.Drawing.Size(68, 20);
            this.KSPPathBrowseButton.TabIndex = 3;
            this.KSPPathBrowseButton.Text = "Browse";
            this.KSPPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 156);
            this.Controls.Add(this.KSPPathBrowseButton);
            this.Controls.Add(this.KSPPathLabel);
            this.Controls.Add(this.KSPPathTextBox);
            this.Controls.Add(this.KSPPathStatusLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsDialog";
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label KSPPathStatusLabel;
        private System.Windows.Forms.TextBox KSPPathTextBox;
        private System.Windows.Forms.Label KSPPathLabel;
        private System.Windows.Forms.Button KSPPathBrowseButton;
    }
}