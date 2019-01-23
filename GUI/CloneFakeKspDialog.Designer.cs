namespace CKAN
{
    partial class CloneFakeKspDialog
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxClonePath = new System.Windows.Forms.TextBox();
            this.buttonInstancePathSelection = new System.Windows.Forms.Button();
            this.buttonOpenInstanceSelection = new System.Windows.Forms.Button();
            this.radioButtonClone = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonFake = new System.Windows.Forms.RadioButton();
            this.folderBrowserDialogNewPath = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelNewName = new System.Windows.Forms.Label();
            this.textBoxNewName = new System.Windows.Forms.TextBox();
            this.checkBoxSetAsDefault = new System.Windows.Forms.CheckBox();
            this.labelNewPath = new System.Windows.Forms.Label();
            this.textBoxNewPath = new System.Windows.Forms.TextBox();
            this.checkBoxSwitchInstance = new System.Windows.Forms.CheckBox();
            this.comboBoxKspVersion = new System.Windows.Forms.ComboBox();
            this.textBoxDlcVersion = new System.Windows.Forms.TextBox();
            this.labelDlcVersion = new System.Windows.Forms.Label();
            this.buttonPathBrowser = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxClonePath);
            this.groupBox1.Controls.Add(this.buttonInstancePathSelection);
            this.groupBox1.Controls.Add(this.buttonOpenInstanceSelection);
            this.groupBox1.Controls.Add(this.radioButtonClone);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(399, 80);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // textBoxClonePath
            // 
            this.textBoxClonePath.AllowDrop = true;
            this.textBoxClonePath.Location = new System.Drawing.Point(170, 19);
            this.textBoxClonePath.Name = "textBoxClonePath";
            this.textBoxClonePath.Size = new System.Drawing.Size(223, 20);
            this.textBoxClonePath.TabIndex = 1;
            this.textBoxClonePath.Text = "You can enter a path here";
            // 
            // buttonInstancePathSelection
            // 
            this.buttonInstancePathSelection.Location = new System.Drawing.Point(263, 51);
            this.buttonInstancePathSelection.Name = "buttonInstancePathSelection";
            this.buttonInstancePathSelection.Size = new System.Drawing.Size(130, 23);
            this.buttonInstancePathSelection.TabIndex = 3;
            this.buttonInstancePathSelection.Text = "Select Instance By Path";
            this.buttonInstancePathSelection.UseVisualStyleBackColor = true;
            this.buttonInstancePathSelection.Click += new System.EventHandler(this.buttonInstancePathSelection_Click);
            // 
            // buttonOpenInstanceSelection
            // 
            this.buttonOpenInstanceSelection.Location = new System.Drawing.Point(7, 51);
            this.buttonOpenInstanceSelection.Name = "buttonOpenInstanceSelection";
            this.buttonOpenInstanceSelection.Size = new System.Drawing.Size(129, 23);
            this.buttonOpenInstanceSelection.TabIndex = 2;
            this.buttonOpenInstanceSelection.Text = "Select Known Instance";
            this.buttonOpenInstanceSelection.UseVisualStyleBackColor = true;
            this.buttonOpenInstanceSelection.Click += new System.EventHandler(this.buttonOpenInstanceSelection_Click);
            // 
            // radioButtonClone
            // 
            this.radioButtonClone.AutoSize = true;
            this.radioButtonClone.Location = new System.Drawing.Point(7, 20);
            this.radioButtonClone.Name = "radioButtonClone";
            this.radioButtonClone.Size = new System.Drawing.Size(157, 17);
            this.radioButtonClone.TabIndex = 0;
            this.radioButtonClone.TabStop = true;
            this.radioButtonClone.Text = "Clone existing KSP instance";
            this.radioButtonClone.UseVisualStyleBackColor = true;
            this.radioButtonClone.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.labelDlcVersion);
            this.groupBox2.Controls.Add(this.textBoxDlcVersion);
            this.groupBox2.Controls.Add(this.comboBoxKspVersion);
            this.groupBox2.Controls.Add(this.radioButtonFake);
            this.groupBox2.Location = new System.Drawing.Point(13, 100);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(399, 80);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            // 
            // radioButtonFake
            // 
            this.radioButtonFake.AutoSize = true;
            this.radioButtonFake.Location = new System.Drawing.Point(7, 20);
            this.radioButtonFake.Name = "radioButtonFake";
            this.radioButtonFake.Size = new System.Drawing.Size(139, 17);
            this.radioButtonFake.TabIndex = 0;
            this.radioButtonFake.TabStop = true;
            this.radioButtonFake.Text = "Fake new KSP instance";
            this.radioButtonFake.UseVisualStyleBackColor = true;
            this.radioButtonFake.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(337, 252);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelNewName
            // 
            this.labelNewName.AutoSize = true;
            this.labelNewName.Location = new System.Drawing.Point(12, 197);
            this.labelNewName.Name = "labelNewName";
            this.labelNewName.Size = new System.Drawing.Size(137, 13);
            this.labelNewName.TabIndex = 9;
            this.labelNewName.Text = "Name for the new instance:";
            // 
            // textBoxNewName
            // 
            this.textBoxNewName.Location = new System.Drawing.Point(155, 194);
            this.textBoxNewName.Name = "textBoxNewName";
            this.textBoxNewName.Size = new System.Drawing.Size(257, 20);
            this.textBoxNewName.TabIndex = 2;
            // 
            // checkBoxSetAsDefault
            // 
            this.checkBoxSetAsDefault.AutoSize = true;
            this.checkBoxSetAsDefault.Location = new System.Drawing.Point(12, 256);
            this.checkBoxSetAsDefault.Name = "checkBoxSetAsDefault";
            this.checkBoxSetAsDefault.Size = new System.Drawing.Size(157, 17);
            this.checkBoxSetAsDefault.TabIndex = 4;
            this.checkBoxSetAsDefault.Text = "Set new instance as default";
            this.checkBoxSetAsDefault.UseVisualStyleBackColor = true;
            // 
            // labelNewPath
            // 
            this.labelNewPath.AutoSize = true;
            this.labelNewPath.Location = new System.Drawing.Point(12, 224);
            this.labelNewPath.Name = "labelNewPath";
            this.labelNewPath.Size = new System.Drawing.Size(131, 13);
            this.labelNewPath.TabIndex = 9;
            this.labelNewPath.Text = "Path for the new instance:";
            // 
            // textBoxNewPath
            // 
            this.textBoxNewPath.Location = new System.Drawing.Point(227, 221);
            this.textBoxNewPath.Name = "textBoxNewPath";
            this.textBoxNewPath.Size = new System.Drawing.Size(185, 20);
            this.textBoxNewPath.TabIndex = 3;
            // 
            // checkBoxSwitchInstance
            // 
            this.checkBoxSwitchInstance.AutoSize = true;
            this.checkBoxSwitchInstance.Checked = true;
            this.checkBoxSwitchInstance.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSwitchInstance.Location = new System.Drawing.Point(181, 256);
            this.checkBoxSwitchInstance.Name = "checkBoxSwitchInstance";
            this.checkBoxSwitchInstance.Size = new System.Drawing.Size(136, 17);
            this.checkBoxSwitchInstance.TabIndex = 5;
            this.checkBoxSwitchInstance.Text = "Switch to new instance";
            this.checkBoxSwitchInstance.UseVisualStyleBackColor = true;
            // 
            // comboBoxKspVersion
            // 
            this.comboBoxKspVersion.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxKspVersion.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.comboBoxKspVersion.FormattingEnabled = true;
            this.comboBoxKspVersion.Location = new System.Drawing.Point(7, 53);
            this.comboBoxKspVersion.MaxDropDownItems = 6;
            this.comboBoxKspVersion.Name = "comboBoxKspVersion";
            this.comboBoxKspVersion.Size = new System.Drawing.Size(183, 21);
            this.comboBoxKspVersion.TabIndex = 1;
            // 
            // textBoxDlcVersion
            // 
            this.textBoxDlcVersion.Location = new System.Drawing.Point(214, 53);
            this.textBoxDlcVersion.Name = "textBoxDlcVersion";
            this.textBoxDlcVersion.Size = new System.Drawing.Size(179, 20);
            this.textBoxDlcVersion.TabIndex = 2;
            // 
            // labelDlcVersion
            // 
            this.labelDlcVersion.AutoSize = true;
            this.labelDlcVersion.Location = new System.Drawing.Point(219, 20);
            this.labelDlcVersion.Name = "labelDlcVersion";
            this.labelDlcVersion.Size = new System.Drawing.Size(174, 26);
            this.labelDlcVersion.TabIndex = 3;
            this.labelDlcVersion.Text = "Enter DLC version here (e.g. 1.6.1).\r\nLeave empty to fake no DLC.";
            // 
            // buttonPathBrowser
            // 
            this.buttonPathBrowser.Location = new System.Drawing.Point(149, 219);
            this.buttonPathBrowser.Name = "buttonPathBrowser";
            this.buttonPathBrowser.Size = new System.Drawing.Size(72, 23);
            this.buttonPathBrowser.TabIndex = 11;
            this.buttonPathBrowser.Text = "Select Path";
            this.buttonPathBrowser.UseVisualStyleBackColor = true;
            this.buttonPathBrowser.Click += new System.EventHandler(this.buttonPathBrowser_Click);
            // 
            // CloneFakeKspDialog
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 287);
            this.Controls.Add(this.buttonPathBrowser);
            this.Controls.Add(this.checkBoxSwitchInstance);
            this.Controls.Add(this.textBoxNewPath);
            this.Controls.Add(this.labelNewPath);
            this.Controls.Add(this.checkBoxSetAsDefault);
            this.Controls.Add(this.textBoxNewName);
            this.Controls.Add(this.labelNewName);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CloneFakeKspDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Clone KSP instance";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonClone;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonFake;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogNewPath;
        private System.Windows.Forms.TextBox textBoxClonePath;
        private System.Windows.Forms.Button buttonInstancePathSelection;
        private System.Windows.Forms.Button buttonOpenInstanceSelection;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelNewName;
        private System.Windows.Forms.TextBox textBoxNewName;
        private System.Windows.Forms.CheckBox checkBoxSetAsDefault;
        private System.Windows.Forms.Label labelNewPath;
        private System.Windows.Forms.TextBox textBoxNewPath;
        private System.Windows.Forms.CheckBox checkBoxSwitchInstance;
        private System.Windows.Forms.ComboBox comboBoxKspVersion;
        private System.Windows.Forms.TextBox textBoxDlcVersion;
        private System.Windows.Forms.Label labelDlcVersion;
        private System.Windows.Forms.Button buttonPathBrowser;
    }
}