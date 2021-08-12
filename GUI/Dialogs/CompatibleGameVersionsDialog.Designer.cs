namespace CKAN.GUI
{
    partial class CompatibleGameVersionsDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(CompatibleGameVersionsDialog));
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
            this.label1.Location = new System.Drawing.Point(11, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(422, 32);
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.label1.TabIndex = 0;
            resources.ApplyResources(this.label1, "label1");
            //
            // label2
            //
            this.label2.Location = new System.Drawing.Point(11, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 1;
            resources.ApplyResources(this.label2, "label2");
            //
            // label3
            //
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(9, 315);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(422, 32);
            this.label3.TabIndex = 2;
            resources.ApplyResources(this.label3, "label3");
            //
            // gameVersionLabel
            //
            this.GameVersionLabel.AutoSize = true;
            this.GameVersionLabel.Location = new System.Drawing.Point(100, 32);
            this.GameVersionLabel.Name = "gameVersionLabel";
            this.GameVersionLabel.Size = new System.Drawing.Size(53, 13);
            this.GameVersionLabel.TabIndex = 3;
            resources.ApplyResources(this.GameVersionLabel, "GameVersionLabel");
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
            this.ClearSelectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ClearSelectionButton.Location = new System.Drawing.Point(12, 211);
            this.ClearSelectionButton.Name = "clearSelectionButton";
            this.ClearSelectionButton.Size = new System.Drawing.Size(95, 23);
            this.ClearSelectionButton.TabIndex = 5;
            this.ClearSelectionButton.UseVisualStyleBackColor = true;
            this.ClearSelectionButton.Click += new System.EventHandler(this.ClearSelectionButton_Click);
            resources.ApplyResources(this.ClearSelectionButton, "ClearSelectionButton");
            //
            // addVersionToListButton
            //
            this.AddVersionToListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddVersionToListButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AddVersionToListButton.Location = new System.Drawing.Point(343, 209);
            this.AddVersionToListButton.Name = "addVersionToListButton";
            this.AddVersionToListButton.Size = new System.Drawing.Size(75, 23);
            this.AddVersionToListButton.TabIndex = 6;
            this.AddVersionToListButton.UseVisualStyleBackColor = true;
            this.AddVersionToListButton.Click += new System.EventHandler(this.AddVersionToListButton_Click);
            resources.ApplyResources(this.AddVersionToListButton, "AddVersionToListButton");
            //
            // label5
            //
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(111, 216);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 13);
            this.label5.TabIndex = 7;
            resources.ApplyResources(this.label5, "label5");
            //
            // addVersionToListTextBox
            //
            this.AddVersionToListTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddVersionToListTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.AddVersionToListTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.AddVersionToListTextBox.Location = new System.Drawing.Point(242, 211);
            this.AddVersionToListTextBox.Name = "addVersionToListTextBox";
            this.AddVersionToListTextBox.Size = new System.Drawing.Size(100, 15);
            this.AddVersionToListTextBox.TabIndex = 8;
            //
            // label6
            //
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.Location = new System.Drawing.Point(9, 240);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(422, 32);
            this.label6.TabIndex = 9;
            resources.ApplyResources(this.label6, "label6");
            //
            // label7
            //
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.Location = new System.Drawing.Point(9, 273);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(422, 32);
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label7.TabIndex = 10;
            resources.ApplyResources(this.label7, "label7");
            //
            // saveButton
            //
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SaveButton.Location = new System.Drawing.Point(362, 350);
            this.SaveButton.Name = "saveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 11;
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            resources.ApplyResources(this.SaveButton, "SaveButton");
            //
            // label8
            //
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 11);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 13);
            this.label8.TabIndex = 12;
            resources.ApplyResources(this.label8, "label8");
            //
            // gameLocationLabel
            //
            this.GameLocationLabel.AutoSize = true;
            this.GameLocationLabel.Location = new System.Drawing.Point(95, 11);
            this.GameLocationLabel.Name = "gameLocationLabel";
            this.GameLocationLabel.Size = new System.Drawing.Size(56, 13);
            this.GameLocationLabel.TabIndex = 13;
            resources.ApplyResources(this.GameLocationLabel, "GameLocationLabel");
            //
            // cancelButton
            //
            this.CancelChooseCompatibleVersionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelChooseCompatibleVersionsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelChooseCompatibleVersionsButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelChooseCompatibleVersionsButton.Location = new System.Drawing.Point(281, 350);
            this.CancelChooseCompatibleVersionsButton.Name = "cancelButton";
            this.CancelChooseCompatibleVersionsButton.Size = new System.Drawing.Size(75, 23);
            this.CancelChooseCompatibleVersionsButton.TabIndex = 14;
            this.CancelChooseCompatibleVersionsButton.UseVisualStyleBackColor = true;
            this.CancelChooseCompatibleVersionsButton.Click += new System.EventHandler(this.CancelButton_Click);
            resources.ApplyResources(this.CancelChooseCompatibleVersionsButton, "CancelChooseCompatibleVersionsButton");
            //
            // CompatibleGameVersionsDialog
            //
            this.AcceptButton = this.SaveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelChooseCompatibleVersionsButton;
            this.ClientSize = new System.Drawing.Size(443, 383);
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
            this.Icon = EmbeddedImages.AppIcon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.HelpButton = true;
            this.Name = "CompatibleGameVersionsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Shown += new System.EventHandler(this.CompatibleGameVersionsDialog_Shown);
            resources.ApplyResources(this, "$this");
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
