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
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.RepositoryGroupBox.SuspendLayout();
            this.CacheGroupBox.SuspendLayout();
            this.SuspendLayout();
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
            this.RepositoryGroupBox.Location = new System.Drawing.Point(12, 12);
            this.RepositoryGroupBox.Name = "RepositoryGroupBox";
            this.RepositoryGroupBox.Size = new System.Drawing.Size(476, 56);
            this.RepositoryGroupBox.TabIndex = 8;
            this.RepositoryGroupBox.TabStop = false;
            this.RepositoryGroupBox.Text = "CKAN Repository";
            // 
            // CacheGroupBox
            // 
            this.CacheGroupBox.Controls.Add(this.button1);
            this.CacheGroupBox.Controls.Add(this.label1);
            this.CacheGroupBox.Location = new System.Drawing.Point(12, 74);
            this.CacheGroupBox.Name = "CacheGroupBox";
            this.CacheGroupBox.Size = new System.Drawing.Size(476, 49);
            this.CacheGroupBox.TabIndex = 9;
            this.CacheGroupBox.TabStop = false;
            this.CacheGroupBox.Text = "CKAN Cache";
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(273, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "There are currently N files in the cache, taking up M MiB";
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
    }
}