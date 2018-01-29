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
            this.GameVersionLabel = new System.Windows.Forms.Label();
            this.SelectedVersionsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.ClearSelectionButton = new System.Windows.Forms.Button();
            this.AddVersionToListButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.AddVersionToListTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.SaveButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.GameLocationLabel = new System.Windows.Forms.Label();
            this.CancelChooseCompatibleVersionsButton = new System.Windows.Forms.Button();
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
            this.label3.Text = "Warning! There is no way to check if mod is truly compatible with versions selec" +
    "ted here! Please act carefully.";
            // 
            // gameVersionLabel
            // 
            this.GameVersionLabel.AutoSize = true;
            this.GameVersionLabel.Location = new System.Drawing.Point(86, 32);
            this.GameVersionLabel.Name = "gameVersionLabel";
            this.GameVersionLabel.Size = new System.Drawing.Size(53, 13);
            this.GameVersionLabel.TabIndex = 3;
            this.GameVersionLabel.Text = "<version>";
            // 
            // selectedVersionsCheckedListBox
            // 
            this.SelectedVersionsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectedVersionsCheckedListBox.CheckOnClick = true;
            this.SelectedVersionsCheckedListBox.FormattingEnabled = true;
            this.SelectedVersionsCheckedListBox.Location = new System.Drawing.Point(12, 79);
            this.SelectedVersionsCheckedListBox.Name = "selectedVersionsCheckedListBox";
            this.SelectedVersionsCheckedListBox.Size = new System.Drawing.Size(406, 124);
            this.SelectedVersionsCheckedListBox.TabIndex = 4;
            // 
            // clearSelectionButton
            // 
            this.ClearSelectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ClearSelectionButton.Location = new System.Drawing.Point(12, 211);
            this.ClearSelectionButton.Name = "clearSelectionButton";
            this.ClearSelectionButton.Size = new System.Drawing.Size(92, 23);
            this.ClearSelectionButton.TabIndex = 5;
            this.ClearSelectionButton.Text = "Clear selection";
            this.ClearSelectionButton.UseVisualStyleBackColor = true;
            this.ClearSelectionButton.Click += new System.EventHandler(this.ClearSelectionButton_Click);
            // 
            // addVersionToListButton
            // 
            this.AddVersionToListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddVersionToListButton.Location = new System.Drawing.Point(343, 209);
            this.AddVersionToListButton.Name = "addVersionToListButton";
            this.AddVersionToListButton.Size = new System.Drawing.Size(75, 23);
            this.AddVersionToListButton.TabIndex = 6;
            this.AddVersionToListButton.Text = "Add";
            this.AddVersionToListButton.UseVisualStyleBackColor = true;
            this.AddVersionToListButton.Click += new System.EventHandler(this.AddVersionToListButton_Click);
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
            this.AddVersionToListTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddVersionToListTextBox.Location = new System.Drawing.Point(206, 211);
            this.AddVersionToListTextBox.Name = "addVersionToListTextBox";
            this.AddVersionToListTextBox.Size = new System.Drawing.Size(131, 20);
            this.AddVersionToListTextBox.TabIndex = 8;
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
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveButton.Location = new System.Drawing.Point(362, 331);
            this.SaveButton.Name = "saveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 11;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
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
            this.GameLocationLabel.AutoSize = true;
            this.GameLocationLabel.Location = new System.Drawing.Point(69, 11);
            this.GameLocationLabel.Name = "gameLocationLabel";
            this.GameLocationLabel.Size = new System.Drawing.Size(56, 13);
            this.GameLocationLabel.TabIndex = 13;
            this.GameLocationLabel.Text = "<location>";
            // 
            // cancelButton
            // 
            this.CancelChooseCompatibleVersionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelChooseCompatibleVersionsButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelChooseCompatibleVersionsButton.Location = new System.Drawing.Point(281, 331);
            this.CancelChooseCompatibleVersionsButton.Name = "cancelButton";
            this.CancelChooseCompatibleVersionsButton.Size = new System.Drawing.Size(75, 23);
            this.CancelChooseCompatibleVersionsButton.TabIndex = 14;
            this.CancelChooseCompatibleVersionsButton.Text = "Cancel";
            this.CancelChooseCompatibleVersionsButton.UseVisualStyleBackColor = true;
            this.CancelChooseCompatibleVersionsButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // CompatibleKspVersionsDialog
            // 
            this.AcceptButton = this.SaveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelChooseCompatibleVersionsButton;
            this.ClientSize = new System.Drawing.Size(443, 364);
            this.ControlBox = false;
            this.Controls.Add(this.CancelChooseCompatibleVersionsButton);
            this.Controls.Add(this.GameLocationLabel);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.AddVersionToListTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.AddVersionToListButton);
            this.Controls.Add(this.ClearSelectionButton);
            this.Controls.Add(this.SelectedVersionsCheckedListBox);
            this.Controls.Add(this.GameVersionLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "CompatibleKspVersionsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Compatible Ksp Versions";
            this.Shown += new System.EventHandler(this.CompatibleKspVersionsDialog_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label GameVersionLabel;
        private System.Windows.Forms.CheckedListBox SelectedVersionsCheckedListBox;
        private System.Windows.Forms.Button ClearSelectionButton;
        private System.Windows.Forms.Button AddVersionToListButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox AddVersionToListTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label GameLocationLabel;
        private System.Windows.Forms.Button CancelChooseCompatibleVersionsButton;
    }
}
