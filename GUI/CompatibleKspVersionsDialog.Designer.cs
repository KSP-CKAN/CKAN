namespace CKAN
{
    partial class CompatibleKspVersionsDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.gameVersionLabel = new System.Windows.Forms.Label();
            this.selectedVersionsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.clearSelectionButton = new System.Windows.Forms.Button();
            this.addVersionToListButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.addVersionToListTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.gameLocationLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(306, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Additionally install mods compatible with following KSP versions:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Game version:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(9, 296);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(401, 32);
            this.label3.TabIndex = 2;
            this.label3.Text = "Warning! There is no way to check if mod is trully compatible with versions selec" +
    "ted here! Please act carefully.";
            // 
            // gameVersionLabel
            // 
            this.gameVersionLabel.AutoSize = true;
            this.gameVersionLabel.Location = new System.Drawing.Point(92, 32);
            this.gameVersionLabel.Name = "gameVersionLabel";
            this.gameVersionLabel.Size = new System.Drawing.Size(53, 13);
            this.gameVersionLabel.TabIndex = 3;
            this.gameVersionLabel.Text = "<version>";
            // 
            // selectedVersionsCheckedListBox
            // 
            this.selectedVersionsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectedVersionsCheckedListBox.CheckOnClick = true;
            this.selectedVersionsCheckedListBox.FormattingEnabled = true;
            this.selectedVersionsCheckedListBox.Location = new System.Drawing.Point(12, 79);
            this.selectedVersionsCheckedListBox.Name = "selectedVersionsCheckedListBox";
            this.selectedVersionsCheckedListBox.Size = new System.Drawing.Size(406, 124);
            this.selectedVersionsCheckedListBox.TabIndex = 4;
            // 
            // clearSelectionButton
            // 
            this.clearSelectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.clearSelectionButton.Location = new System.Drawing.Point(12, 211);
            this.clearSelectionButton.Name = "clearSelectionButton";
            this.clearSelectionButton.Size = new System.Drawing.Size(92, 23);
            this.clearSelectionButton.TabIndex = 5;
            this.clearSelectionButton.Text = "Clear selection";
            this.clearSelectionButton.UseVisualStyleBackColor = true;
            this.clearSelectionButton.Click += new System.EventHandler(this.clearSelectionButton_Click);
            // 
            // addVersionToListButton
            // 
            this.addVersionToListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addVersionToListButton.Location = new System.Drawing.Point(343, 209);
            this.addVersionToListButton.Name = "addVersionToListButton";
            this.addVersionToListButton.Size = new System.Drawing.Size(75, 23);
            this.addVersionToListButton.TabIndex = 6;
            this.addVersionToListButton.Text = "Add";
            this.addVersionToListButton.UseVisualStyleBackColor = true;
            this.addVersionToListButton.Click += new System.EventHandler(this.addVersionToListButton_Click);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(106, 216);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Add version to list:";
            // 
            // addVersionToListTextBox
            // 
            this.addVersionToListTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addVersionToListTextBox.Location = new System.Drawing.Point(206, 211);
            this.addVersionToListTextBox.Name = "addVersionToListTextBox";
            this.addVersionToListTextBox.Size = new System.Drawing.Size(131, 20);
            this.addVersionToListTextBox.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.Location = new System.Drawing.Point(9, 240);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(422, 32);
            this.label6.TabIndex = 9;
            this.label6.Text = "Note: if you add version like \"1.2\" you will force all mods compatible with 1.2.x" +
    "x (1.2.0, 1.2.1, etc) to be compatible with this KSP installation";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 273);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(382, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "If KSP is updated this dialog will be shown again so you can adjust your settings" +
    "";
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Location = new System.Drawing.Point(362, 331);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 11;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 11);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 13);
            this.label8.TabIndex = 12;
            this.label8.Text = "Installation:";
            // 
            // gameLocationLabel
            // 
            this.gameLocationLabel.AutoSize = true;
            this.gameLocationLabel.Location = new System.Drawing.Point(69, 11);
            this.gameLocationLabel.Name = "gameLocationLabel";
            this.gameLocationLabel.Size = new System.Drawing.Size(56, 13);
            this.gameLocationLabel.TabIndex = 13;
            this.gameLocationLabel.Text = "<location>";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(281, 331);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 14;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // CompatibleKspVersionsDialog
            // 
            this.AcceptButton = this.saveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(443, 364);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.gameLocationLabel);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.addVersionToListTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.addVersionToListButton);
            this.Controls.Add(this.clearSelectionButton);
            this.Controls.Add(this.selectedVersionsCheckedListBox);
            this.Controls.Add(this.gameVersionLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "CompatibleKspVersionsDialog";
            this.Text = "Compatible Ksp Versions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label gameVersionLabel;
        private System.Windows.Forms.CheckedListBox selectedVersionsCheckedListBox;
        private System.Windows.Forms.Button clearSelectionButton;
        private System.Windows.Forms.Button addVersionToListButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox addVersionToListTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label gameLocationLabel;
        private System.Windows.Forms.Button cancelButton;
    }
}