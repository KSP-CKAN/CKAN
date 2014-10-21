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
            this.CKANRepositoryTextBox = new System.Windows.Forms.TextBox();
            this.CKANRepositoryApplyButton = new System.Windows.Forms.Button();
            this.CKANRepositoryDefaultButton = new System.Windows.Forms.Button();
            this.RepositoryGroupBox = new System.Windows.Forms.GroupBox();
            this.CacheGroupBox = new System.Windows.Forms.GroupBox();
            this.ClearCKANCacheButton = new System.Windows.Forms.Button();
            this.CKANCacheLabel = new System.Windows.Forms.Label();
            this.RepositoryGroupBox.SuspendLayout();
            this.CacheGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // CKANRepositoryTextBox
            // 
            this.CKANRepositoryTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CKANRepositoryTextBox.Location = new System.Drawing.Point(6, 19);
            this.CKANRepositoryTextBox.Name = "CKANRepositoryTextBox";
            this.CKANRepositoryTextBox.Size = new System.Drawing.Size(316, 20);
            this.CKANRepositoryTextBox.TabIndex = 4;
            // 
            // CKANRepositoryApplyButton
            // 
            this.CKANRepositoryApplyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CKANRepositoryApplyButton.Location = new System.Drawing.Point(328, 14);
            this.CKANRepositoryApplyButton.Name = "CKANRepositoryApplyButton";
            this.CKANRepositoryApplyButton.Size = new System.Drawing.Size(56, 26);
            this.CKANRepositoryApplyButton.TabIndex = 6;
            this.CKANRepositoryApplyButton.Text = "Apply";
            this.CKANRepositoryApplyButton.UseVisualStyleBackColor = true;
            this.CKANRepositoryApplyButton.Click += new System.EventHandler(this.CKANRepositoryApplyButton_Click);
            // 
            // CKANRepositoryDefaultButton
            // 
            this.CKANRepositoryDefaultButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CKANRepositoryDefaultButton.Location = new System.Drawing.Point(390, 14);
            this.CKANRepositoryDefaultButton.Name = "CKANRepositoryDefaultButton";
            this.CKANRepositoryDefaultButton.Size = new System.Drawing.Size(53, 26);
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
            this.RepositoryGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RepositoryGroupBox.Location = new System.Drawing.Point(12, 12);
            this.RepositoryGroupBox.Name = "RepositoryGroupBox";
            this.RepositoryGroupBox.Size = new System.Drawing.Size(476, 56);
            this.RepositoryGroupBox.TabIndex = 8;
            this.RepositoryGroupBox.TabStop = false;
            this.RepositoryGroupBox.Text = "CKAN Repository";
            // 
            // CacheGroupBox
            // 
            this.CacheGroupBox.Controls.Add(this.ClearCKANCacheButton);
            this.CacheGroupBox.Controls.Add(this.CKANCacheLabel);
            this.CacheGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CacheGroupBox.Location = new System.Drawing.Point(12, 74);
            this.CacheGroupBox.Name = "CacheGroupBox";
            this.CacheGroupBox.Size = new System.Drawing.Size(476, 49);
            this.CacheGroupBox.TabIndex = 9;
            this.CacheGroupBox.TabStop = false;
            this.CacheGroupBox.Text = "CKAN Cache";
            // 
            // ClearCKANCacheButton
            // 
            this.ClearCKANCacheButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ClearCKANCacheButton.Location = new System.Drawing.Point(328, 16);
            this.ClearCKANCacheButton.Name = "ClearCKANCacheButton";
            this.ClearCKANCacheButton.Size = new System.Drawing.Size(129, 23);
            this.ClearCKANCacheButton.TabIndex = 1;
            this.ClearCKANCacheButton.Text = "Clear cache";
            this.ClearCKANCacheButton.UseVisualStyleBackColor = true;
            this.ClearCKANCacheButton.Click += new System.EventHandler(this.ClearCKANCacheButton_Click);
            // 
            // CKANCacheLabel
            // 
            this.CKANCacheLabel.AutoSize = true;
            this.CKANCacheLabel.Location = new System.Drawing.Point(3, 21);
            this.CKANCacheLabel.Name = "CKANCacheLabel";
            this.CKANCacheLabel.Size = new System.Drawing.Size(273, 13);
            this.CKANCacheLabel.TabIndex = 0;
            this.CKANCacheLabel.Text = "There are currently N files in the cache, taking up M MiB";
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 135);
            this.Controls.Add(this.CacheGroupBox);
            this.Controls.Add(this.RepositoryGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsDialog";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsDialog_Load);
            this.RepositoryGroupBox.ResumeLayout(false);
            this.RepositoryGroupBox.PerformLayout();
            this.CacheGroupBox.ResumeLayout(false);
            this.CacheGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox CKANRepositoryTextBox;
        private System.Windows.Forms.Button CKANRepositoryApplyButton;
        private System.Windows.Forms.Button CKANRepositoryDefaultButton;
        private System.Windows.Forms.GroupBox RepositoryGroupBox;
        private System.Windows.Forms.GroupBox CacheGroupBox;
        private System.Windows.Forms.Label CKANCacheLabel;
        private System.Windows.Forms.Button ClearCKANCacheButton;
    }
}