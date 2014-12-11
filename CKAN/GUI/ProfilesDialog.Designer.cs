namespace CKAN
{
    partial class ProfilesDialog
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
            this.CKANActiveProfileGroupBox = new System.Windows.Forms.GroupBox();
            this.CKANActiveProfileApplyButton = new System.Windows.Forms.Button();
            this.CKANActiveProfileComboBox = new System.Windows.Forms.ComboBox();
            this.CKANNewProfileGroupBox = new System.Windows.Forms.GroupBox();
            this.CKANNewProfileCreateButton = new System.Windows.Forms.Button();
            this.CKANCopyFromLabel = new System.Windows.Forms.Label();
            this.CKANCopyFromProfileComboBox = new System.Windows.Forms.ComboBox();
            this.CKANNewProfileTextBox = new System.Windows.Forms.TextBox();
            this.CKANActiveProfileGroupBox.SuspendLayout();
            this.CKANNewProfileGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // CKANActiveProfileGroupBox
            // 
            this.CKANActiveProfileGroupBox.Controls.Add(this.CKANActiveProfileApplyButton);
            this.CKANActiveProfileGroupBox.Controls.Add(this.CKANActiveProfileComboBox);
            this.CKANActiveProfileGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CKANActiveProfileGroupBox.Location = new System.Drawing.Point(12, 12);
            this.CKANActiveProfileGroupBox.Name = "CKANActiveProfileGroupBox";
            this.CKANActiveProfileGroupBox.Size = new System.Drawing.Size(455, 53);
            this.CKANActiveProfileGroupBox.TabIndex = 0;
            this.CKANActiveProfileGroupBox.TabStop = false;
            this.CKANActiveProfileGroupBox.Text = "Active profile";
            // 
            // CKANActiveProfileApplyButton
            // 
            this.CKANActiveProfileApplyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CKANActiveProfileApplyButton.Location = new System.Drawing.Point(374, 18);
            this.CKANActiveProfileApplyButton.Name = "CKANActiveProfileApplyButton";
            this.CKANActiveProfileApplyButton.Size = new System.Drawing.Size(75, 23);
            this.CKANActiveProfileApplyButton.TabIndex = 1;
            this.CKANActiveProfileApplyButton.Text = "Apply";
            this.CKANActiveProfileApplyButton.UseVisualStyleBackColor = true;
            this.CKANActiveProfileApplyButton.Click += new System.EventHandler(this.CKANActiveProfileApplyButton_Click);
            // 
            // CKANActiveProfileComboBox
            // 
            this.CKANActiveProfileComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CKANActiveProfileComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CKANActiveProfileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CKANActiveProfileComboBox.FormattingEnabled = true;
            this.CKANActiveProfileComboBox.Location = new System.Drawing.Point(7, 20);
            this.CKANActiveProfileComboBox.Name = "CKANActiveProfileComboBox";
            this.CKANActiveProfileComboBox.Size = new System.Drawing.Size(361, 21);
            this.CKANActiveProfileComboBox.TabIndex = 0;
            // 
            // CKANNewProfileGroupBox
            // 
            this.CKANNewProfileGroupBox.Controls.Add(this.CKANNewProfileCreateButton);
            this.CKANNewProfileGroupBox.Controls.Add(this.CKANCopyFromLabel);
            this.CKANNewProfileGroupBox.Controls.Add(this.CKANCopyFromProfileComboBox);
            this.CKANNewProfileGroupBox.Controls.Add(this.CKANNewProfileTextBox);
            this.CKANNewProfileGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CKANNewProfileGroupBox.Location = new System.Drawing.Point(12, 71);
            this.CKANNewProfileGroupBox.Name = "CKANNewProfileGroupBox";
            this.CKANNewProfileGroupBox.Size = new System.Drawing.Size(455, 100);
            this.CKANNewProfileGroupBox.TabIndex = 1;
            this.CKANNewProfileGroupBox.TabStop = false;
            this.CKANNewProfileGroupBox.Text = "New profile";
            // 
            // CKANNewProfileCreateButton
            // 
            this.CKANNewProfileCreateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CKANNewProfileCreateButton.Location = new System.Drawing.Point(374, 61);
            this.CKANNewProfileCreateButton.Name = "CKANNewProfileCreateButton";
            this.CKANNewProfileCreateButton.Size = new System.Drawing.Size(75, 23);
            this.CKANNewProfileCreateButton.TabIndex = 3;
            this.CKANNewProfileCreateButton.Text = "Create";
            this.CKANNewProfileCreateButton.UseVisualStyleBackColor = true;
            this.CKANNewProfileCreateButton.Click += new System.EventHandler(this.CKANNewProfileCreateButton_Click);
            // 
            // CKANCopyFromLabel
            // 
            this.CKANCopyFromLabel.AutoSize = true;
            this.CKANCopyFromLabel.Location = new System.Drawing.Point(6, 47);
            this.CKANCopyFromLabel.Name = "CKANCopyFromLabel";
            this.CKANCopyFromLabel.Size = new System.Drawing.Size(85, 13);
            this.CKANCopyFromLabel.TabIndex = 2;
            this.CKANCopyFromLabel.Text = "Copy from profile";
            // 
            // CKANCopyFromProfileComboBox
            // 
            this.CKANCopyFromProfileComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CKANCopyFromProfileComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.CKANCopyFromProfileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CKANCopyFromProfileComboBox.FormattingEnabled = true;
            this.CKANCopyFromProfileComboBox.Location = new System.Drawing.Point(7, 63);
            this.CKANCopyFromProfileComboBox.Name = "CKANCopyFromProfileComboBox";
            this.CKANCopyFromProfileComboBox.Size = new System.Drawing.Size(361, 21);
            this.CKANCopyFromProfileComboBox.TabIndex = 1;
            // 
            // CKANNewProfileTextBox
            // 
            this.CKANNewProfileTextBox.Location = new System.Drawing.Point(7, 20);
            this.CKANNewProfileTextBox.Name = "CKANNewProfileTextBox";
            this.CKANNewProfileTextBox.Size = new System.Drawing.Size(442, 20);
            this.CKANNewProfileTextBox.TabIndex = 0;
            // 
            // ProfilesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 187);
            this.Controls.Add(this.CKANNewProfileGroupBox);
            this.Controls.Add(this.CKANActiveProfileGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ProfilesDialog";
            this.Text = "Profiles";
            this.Load += new System.EventHandler(this.ProfilesDialog_Load);
            this.CKANActiveProfileGroupBox.ResumeLayout(false);
            this.CKANNewProfileGroupBox.ResumeLayout(false);
            this.CKANNewProfileGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox CKANActiveProfileGroupBox;
        private System.Windows.Forms.ComboBox CKANActiveProfileComboBox;
        private System.Windows.Forms.Button CKANActiveProfileApplyButton;
        private System.Windows.Forms.GroupBox CKANNewProfileGroupBox;
        private System.Windows.Forms.TextBox CKANNewProfileTextBox;
        private System.Windows.Forms.Label CKANCopyFromLabel;
        private System.Windows.Forms.ComboBox CKANCopyFromProfileComboBox;
        private System.Windows.Forms.Button CKANNewProfileCreateButton;

    }
}