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
            this.CKANRepositoryTextBox = new System.Windows.Forms.TextBox();
            this.CKANRepositoryApplyButton = new System.Windows.Forms.Button();
            this.CKANRepositoryDefaultButton = new System.Windows.Forms.Button();
            this.RepositoryGroupBox = new System.Windows.Forms.GroupBox();
            this.KSPGroupBox = new System.Windows.Forms.GroupBox();
            this.CacheGroupBox = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.RepositoryGroupBox.SuspendLayout();
            this.KSPGroupBox.SuspendLayout();
            this.CacheGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // KSPPathStatusLabel
            // 
            this.KSPPathStatusLabel.AutoSize = true;
            this.KSPPathStatusLabel.Location = new System.Drawing.Point(6, 50);
            this.KSPPathStatusLabel.Name = "KSPPathStatusLabel";
            this.KSPPathStatusLabel.Size = new System.Drawing.Size(453, 13);
            this.KSPPathStatusLabel.TabIndex = 0;
            this.KSPPathStatusLabel.Text = "We\'ve auto-detected Kerbal Space Program\'s root folder for you, you can override " +
    "it if you wish";
            // 
            // KSPPathTextBox
            // 
            this.KSPPathTextBox.Location = new System.Drawing.Point(44, 27);
            this.KSPPathTextBox.Name = "KSPPathTextBox";
            this.KSPPathTextBox.Size = new System.Drawing.Size(352, 20);
            this.KSPPathTextBox.TabIndex = 1;
            // 
            // KSPPathLabel
            // 
            this.KSPPathLabel.AutoSize = true;
            this.KSPPathLabel.Location = new System.Drawing.Point(6, 31);
            this.KSPPathLabel.Name = "KSPPathLabel";
            this.KSPPathLabel.Size = new System.Drawing.Size(32, 13);
            this.KSPPathLabel.TabIndex = 2;
            this.KSPPathLabel.Text = "Path:";
            // 
            // KSPPathBrowseButton
            // 
            this.KSPPathBrowseButton.Location = new System.Drawing.Point(402, 27);
            this.KSPPathBrowseButton.Name = "KSPPathBrowseButton";
            this.KSPPathBrowseButton.Size = new System.Drawing.Size(68, 20);
            this.KSPPathBrowseButton.TabIndex = 3;
            this.KSPPathBrowseButton.Text = "Browse";
            this.KSPPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // CKANRepositoryTextBox
            // 
            this.CKANRepositoryTextBox.Location = new System.Drawing.Point(6, 19);
            this.CKANRepositoryTextBox.Name = "CKANRepositoryTextBox";
            this.CKANRepositoryTextBox.Size = new System.Drawing.Size(316, 20);
            this.CKANRepositoryTextBox.TabIndex = 4;
            // 
            // CKANRepositoryApplyButton
            // 
            this.CKANRepositoryApplyButton.Location = new System.Drawing.Point(328, 19);
            this.CKANRepositoryApplyButton.Name = "CKANRepositoryApplyButton";
            this.CKANRepositoryApplyButton.Size = new System.Drawing.Size(68, 20);
            this.CKANRepositoryApplyButton.TabIndex = 6;
            this.CKANRepositoryApplyButton.Text = "Apply";
            this.CKANRepositoryApplyButton.UseVisualStyleBackColor = true;
            this.CKANRepositoryApplyButton.Click += new System.EventHandler(this.CKANRepositoryApplyButton_Click);
            // 
            // CKANRepositoryDefaultButton
            // 
            this.CKANRepositoryDefaultButton.Location = new System.Drawing.Point(402, 19);
            this.CKANRepositoryDefaultButton.Name = "CKANRepositoryDefaultButton";
            this.CKANRepositoryDefaultButton.Size = new System.Drawing.Size(68, 20);
            this.CKANRepositoryDefaultButton.TabIndex = 7;
            this.CKANRepositoryDefaultButton.Text = "Default";
            this.CKANRepositoryDefaultButton.UseVisualStyleBackColor = true;
            this.CKANRepositoryDefaultButton.Click += new System.EventHandler(this.CKANRepositoryDefaultButton_Click);
            // 
            // RepositoryGroupBox
            // 
            this.RepositoryGroupBox.Controls.Add(this.CKANRepositoryTextBox);
            this.RepositoryGroupBox.Controls.Add(this.CKANRepositoryDefaultButton);
            this.RepositoryGroupBox.Controls.Add(this.CKANRepositoryApplyButton);
            this.RepositoryGroupBox.Location = new System.Drawing.Point(7, 88);
            this.RepositoryGroupBox.Name = "RepositoryGroupBox";
            this.RepositoryGroupBox.Size = new System.Drawing.Size(476, 54);
            this.RepositoryGroupBox.TabIndex = 8;
            this.RepositoryGroupBox.TabStop = false;
            this.RepositoryGroupBox.Text = "CKAN Repository";
            // 
            // KSPGroupBox
            // 
            this.KSPGroupBox.Controls.Add(this.KSPPathTextBox);
            this.KSPGroupBox.Controls.Add(this.KSPPathLabel);
            this.KSPGroupBox.Controls.Add(this.KSPPathStatusLabel);
            this.KSPGroupBox.Controls.Add(this.KSPPathBrowseButton);
            this.KSPGroupBox.Location = new System.Drawing.Point(7, 12);
            this.KSPGroupBox.Name = "KSPGroupBox";
            this.KSPGroupBox.Size = new System.Drawing.Size(476, 70);
            this.KSPGroupBox.TabIndex = 9;
            this.KSPGroupBox.TabStop = false;
            this.KSPGroupBox.Text = "Kerbal Space Program";
            // 
            // CacheGroupBox
            // 
            this.CacheGroupBox.Controls.Add(this.button1);
            this.CacheGroupBox.Controls.Add(this.label1);
            this.CacheGroupBox.Location = new System.Drawing.Point(7, 148);
            this.CacheGroupBox.Name = "CacheGroupBox";
            this.CacheGroupBox.Size = new System.Drawing.Size(476, 49);
            this.CacheGroupBox.TabIndex = 9;
            this.CacheGroupBox.TabStop = false;
            this.CacheGroupBox.Text = "CKAN Cache";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(273, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "There are currently N files in the cache, taking up M MiB";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(341, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(129, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Clear cache";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 209);
            this.Controls.Add(this.CacheGroupBox);
            this.Controls.Add(this.KSPGroupBox);
            this.Controls.Add(this.RepositoryGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsDialog";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsDialog_Load);
            this.RepositoryGroupBox.ResumeLayout(false);
            this.RepositoryGroupBox.PerformLayout();
            this.KSPGroupBox.ResumeLayout(false);
            this.KSPGroupBox.PerformLayout();
            this.CacheGroupBox.ResumeLayout(false);
            this.CacheGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label KSPPathStatusLabel;
        private System.Windows.Forms.TextBox KSPPathTextBox;
        private System.Windows.Forms.Label KSPPathLabel;
        private System.Windows.Forms.Button KSPPathBrowseButton;
        private System.Windows.Forms.TextBox CKANRepositoryTextBox;
        private System.Windows.Forms.Button CKANRepositoryApplyButton;
        private System.Windows.Forms.Button CKANRepositoryDefaultButton;
        private System.Windows.Forms.GroupBox RepositoryGroupBox;
        private System.Windows.Forms.GroupBox KSPGroupBox;
        private System.Windows.Forms.GroupBox CacheGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
    }
}