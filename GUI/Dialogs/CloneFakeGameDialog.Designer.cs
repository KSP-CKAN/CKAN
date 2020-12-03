namespace CKAN
{
    partial class CloneFakeGameDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(CloneFakeGameDialog));
            this.radioButtonClone = new System.Windows.Forms.RadioButton();
            this.cloneGroupBox = new System.Windows.Forms.GroupBox();
            this.labelOldInstance = new System.Windows.Forms.Label();
            this.comboBoxKnownInstance = new System.Windows.Forms.ComboBox();
            this.labelOldPath = new System.Windows.Forms.Label();
            this.textBoxClonePath = new System.Windows.Forms.TextBox();
            this.buttonInstancePathSelection = new System.Windows.Forms.Button();
            this.radioButtonFake = new System.Windows.Forms.RadioButton();
            this.fakeGroupBox = new System.Windows.Forms.GroupBox();
            this.labelVersion = new System.Windows.Forms.Label();
            this.comboBoxGameVersion = new System.Windows.Forms.ComboBox();
            this.labelDlcVersions = new System.Windows.Forms.Label();
            this.textBoxMHDlcVersion = new System.Windows.Forms.TextBox();
            this.labelMakingHistory = new System.Windows.Forms.Label();
            this.textBoxBGDlcVersion = new System.Windows.Forms.TextBox();
            this.labelBreakingGround = new System.Windows.Forms.Label();
            this.labelNewName = new System.Windows.Forms.Label();
            this.textBoxNewName = new System.Windows.Forms.TextBox();
            this.labelNewPath = new System.Windows.Forms.Label();
            this.textBoxNewPath = new System.Windows.Forms.TextBox();
            this.buttonPathBrowser = new System.Windows.Forms.Button();
            this.checkBoxSetAsDefault = new System.Windows.Forms.CheckBox();
            this.checkBoxSwitchInstance = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.folderBrowserDialogNewPath = new System.Windows.Forms.FolderBrowserDialog();
            this.cloneGroupBox.SuspendLayout();
            this.fakeGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // radioButtonClone
            //
            this.radioButtonClone.AutoSize = true;
            this.radioButtonClone.Location = new System.Drawing.Point(20, 8);
            this.radioButtonClone.Name = "radioButtonClone";
            this.radioButtonClone.Size = new System.Drawing.Size(157, 17);
            this.radioButtonClone.TabIndex = 99;
            this.radioButtonClone.TabStop = true;
            this.radioButtonClone.UseVisualStyleBackColor = true;
            this.radioButtonClone.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            resources.ApplyResources(this.radioButtonClone, "radioButtonClone");
            //
            // cloneGroupBox
            //
            this.cloneGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cloneGroupBox.Controls.Add(this.labelOldInstance);
            this.cloneGroupBox.Controls.Add(this.comboBoxKnownInstance);
            this.cloneGroupBox.Controls.Add(this.labelOldPath);
            this.cloneGroupBox.Controls.Add(this.textBoxClonePath);
            this.cloneGroupBox.Controls.Add(this.buttonInstancePathSelection);
            this.cloneGroupBox.Location = new System.Drawing.Point(13, 13);
            this.cloneGroupBox.Name = "cloneGroupBox";
            this.cloneGroupBox.Size = new System.Drawing.Size(399, 100);
            this.cloneGroupBox.TabIndex = 1;
            this.cloneGroupBox.TabStop = false;
            //
            // labelOldInstance
            //
            this.labelOldInstance.AutoSize = true;
            this.labelOldInstance.Location = new System.Drawing.Point(12, 22);
            this.labelOldInstance.Name = "labelOldInstance";
            this.labelOldInstance.Size = new System.Drawing.Size(131, 13);
            this.labelOldInstance.TabIndex = 2;
            resources.ApplyResources(this.labelOldInstance, "labelOldInstance");
            //
            // comboBoxKnownInstance
            //
            this.comboBoxKnownInstance.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxKnownInstance.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.comboBoxKnownInstance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxKnownInstance.FormattingEnabled = true;
            this.comboBoxKnownInstance.Location = new System.Drawing.Point(168, 19);
            this.comboBoxKnownInstance.MaxDropDownItems = 10;
            this.comboBoxKnownInstance.Name = "comboBoxKnownInstance";
            this.comboBoxKnownInstance.Size = new System.Drawing.Size(218, 20);
            this.comboBoxKnownInstance.TabIndex = 3;
            this.comboBoxKnownInstance.SelectedIndexChanged += new System.EventHandler(this.comboBoxKnownInstance_SelectedIndexChanged);
            //
            // labelOldPath
            //
            this.labelOldPath.AutoSize = true;
            this.labelOldPath.Location = new System.Drawing.Point(12, 52);
            this.labelOldPath.Name = "labelOldPath";
            this.labelOldPath.Size = new System.Drawing.Size(131, 13);
            this.labelOldPath.TabIndex = 4;
            resources.ApplyResources(this.labelOldPath, "labelOldPath");
            //
            // textBoxClonePath
            //
            this.textBoxClonePath.AllowDrop = true;
            this.textBoxClonePath.Location = new System.Drawing.Point(168, 49);
            this.textBoxClonePath.Name = "textBoxClonePath";
            this.textBoxClonePath.Size = new System.Drawing.Size(163, 20);
            this.textBoxClonePath.TabIndex = 5;
            this.textBoxClonePath.Text = "";
            //
            // buttonInstancePathSelection
            //
            this.buttonInstancePathSelection.Location = new System.Drawing.Point(331, 48);
            this.buttonInstancePathSelection.Name = "buttonInstancePathSelection";
            this.buttonInstancePathSelection.Size = new System.Drawing.Size(55, 22);
            this.buttonInstancePathSelection.TabIndex = 6;
            this.buttonInstancePathSelection.UseVisualStyleBackColor = true;
            this.buttonInstancePathSelection.Click += new System.EventHandler(this.buttonInstancePathSelection_Click);
            resources.ApplyResources(this.buttonInstancePathSelection, "buttonInstancePathSelection");
            //
            // fakeGroupBox
            //
            this.fakeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fakeGroupBox.Controls.Add(this.labelDlcVersions);
            this.fakeGroupBox.Controls.Add(this.textBoxMHDlcVersion);
            this.fakeGroupBox.Controls.Add(this.labelMakingHistory);
            this.fakeGroupBox.Controls.Add(this.textBoxBGDlcVersion);
            this.fakeGroupBox.Controls.Add(this.labelBreakingGround);
            this.fakeGroupBox.Controls.Add(this.labelVersion);
            this.fakeGroupBox.Controls.Add(this.comboBoxGameVersion);
            this.fakeGroupBox.Location = new System.Drawing.Point(13, 130);
            this.fakeGroupBox.Name = "fakeGroupBox";
            this.fakeGroupBox.Size = new System.Drawing.Size(400, 104);
            this.fakeGroupBox.TabIndex = 7;
            this.fakeGroupBox.TabStop = false;
            //
            // radioButtonFake
            //
            this.radioButtonFake.AutoSize = true;
            this.radioButtonFake.Location = new System.Drawing.Point(20, 125);
            this.radioButtonFake.Name = "radioButtonFake";
            this.radioButtonFake.Size = new System.Drawing.Size(139, 17);
            this.radioButtonFake.TabIndex = 8;
            this.radioButtonFake.TabStop = true;
            this.radioButtonFake.UseVisualStyleBackColor = true;
            this.radioButtonFake.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            resources.ApplyResources(this.radioButtonFake, "radioButtonFake");
            //
            // labelVersion
            //
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(12, 23);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(174, 26);
            this.labelVersion.TabIndex = 9;
            this.labelVersion.Text = "Version:";
            resources.ApplyResources(this.labelVersion, "labelVersion");
            //
            // comboBoxGameVersion
            //
            this.comboBoxGameVersion.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxGameVersion.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.comboBoxGameVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxGameVersion.FormattingEnabled = true;
            this.comboBoxGameVersion.Location = new System.Drawing.Point(168, 20);
            this.comboBoxGameVersion.MaxDropDownItems = 6;
            this.comboBoxGameVersion.Name = "comboBoxGameVersion";
            this.comboBoxGameVersion.Size = new System.Drawing.Size(83, 21);
            this.comboBoxGameVersion.TabIndex = 10;
            //
            // labelDlcVersions
            //
            this.labelDlcVersions.AutoSize = true;
            this.labelDlcVersions.Location = new System.Drawing.Point(12, 52);
            this.labelDlcVersions.Name = "labelDlcVersion";
            this.labelDlcVersions.Size = new System.Drawing.Size(174, 26);
            this.labelDlcVersions.TabIndex = 11;
            resources.ApplyResources(this.labelDlcVersions, "labelDlcVersions");
            //
            // textBoxMHDlcVersion
            //
            this.textBoxMHDlcVersion.Location = new System.Drawing.Point(170, 49);
            this.textBoxMHDlcVersion.Name = "textBoxDlcVersion";
            this.textBoxMHDlcVersion.Size = new System.Drawing.Size(83, 20);
            this.textBoxMHDlcVersion.TabIndex = 12;
            //
            // labelMakingHistory
            //
            this.labelMakingHistory.AutoSize = true;
            this.labelMakingHistory.Location = new System.Drawing.Point(257, 52);
            this.labelMakingHistory.Name = "labelMakingHistory";
            this.labelMakingHistory.Size = new System.Drawing.Size(174, 26);
            this.labelMakingHistory.TabStop = false;
            resources.ApplyResources(this.labelMakingHistory, "labelMakingHistory");
            //
            // textBoxBGDlcVersion
            //
            this.textBoxBGDlcVersion.Location = new System.Drawing.Point(170, 71);
            this.textBoxBGDlcVersion.Name = "textBoxDlcVersion";
            this.textBoxBGDlcVersion.Size = new System.Drawing.Size(83, 20);
            this.textBoxBGDlcVersion.TabIndex = 13;
            //
            // labelBreakingGround
            //
            this.labelBreakingGround.AutoSize = true;
            this.labelBreakingGround.Location = new System.Drawing.Point(257, 74);
            this.labelBreakingGround.Name = "labelBreakingGround";
            this.labelBreakingGround.Size = new System.Drawing.Size(174, 26);
            this.labelBreakingGround.TabStop = false;
            resources.ApplyResources(this.labelBreakingGround, "labelBreakingGround");
            //
            // labelNewName
            //
            this.labelNewName.AutoSize = true;
            this.labelNewName.Location = new System.Drawing.Point(12, 251);
            this.labelNewName.Name = "labelNewName";
            this.labelNewName.Size = new System.Drawing.Size(137, 13);
            this.labelNewName.TabIndex = 14;
            resources.ApplyResources(this.labelNewName, "labelNewName");
            //
            // textBoxNewName
            //
            this.textBoxNewName.Location = new System.Drawing.Point(181, 248);
            this.textBoxNewName.Name = "textBoxNewName";
            this.textBoxNewName.Size = new System.Drawing.Size(218, 20);
            this.textBoxNewName.TabIndex = 15;
            //
            // labelNewPath
            //
            this.labelNewPath.AutoSize = true;
            this.labelNewPath.Location = new System.Drawing.Point(12, 278);
            this.labelNewPath.Name = "labelNewPath";
            this.labelNewPath.Size = new System.Drawing.Size(131, 13);
            this.labelNewPath.TabIndex = 16;
            resources.ApplyResources(this.labelNewPath, "labelNewPath");
            //
            // textBoxNewPath
            //
            this.textBoxNewPath.Location = new System.Drawing.Point(181, 275);
            this.textBoxNewPath.Name = "textBoxNewPath";
            this.textBoxNewPath.Size = new System.Drawing.Size(163, 20);
            this.textBoxNewPath.TabIndex = 17;
            //
            // buttonPathBrowser
            //
            this.buttonPathBrowser.Location = new System.Drawing.Point(344, 274);
            this.buttonPathBrowser.Name = "buttonPathBrowser";
            this.buttonPathBrowser.Size = new System.Drawing.Size(55, 22);
            this.buttonPathBrowser.TabIndex = 18;
            this.buttonPathBrowser.UseVisualStyleBackColor = true;
            this.buttonPathBrowser.Click += new System.EventHandler(this.buttonPathBrowser_Click);
            resources.ApplyResources(this.buttonPathBrowser, "buttonPathBrowser");
            //
            // checkBoxSetAsDefault
            //
            this.checkBoxSetAsDefault.AutoSize = true;
            this.checkBoxSetAsDefault.Location = new System.Drawing.Point(12, 310);
            this.checkBoxSetAsDefault.Name = "checkBoxSetAsDefault";
            this.checkBoxSetAsDefault.Size = new System.Drawing.Size(157, 17);
            this.checkBoxSetAsDefault.TabIndex = 19;
            this.checkBoxSetAsDefault.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.checkBoxSetAsDefault, "checkBoxSetAsDefault");
            //
            // checkBoxSwitchInstance
            //
            this.checkBoxSwitchInstance.AutoSize = true;
            this.checkBoxSwitchInstance.Checked = true;
            this.checkBoxSwitchInstance.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSwitchInstance.Location = new System.Drawing.Point(181, 310);
            this.checkBoxSwitchInstance.Name = "checkBoxSwitchInstance";
            this.checkBoxSwitchInstance.Size = new System.Drawing.Size(136, 17);
            this.checkBoxSwitchInstance.TabIndex = 20;
            this.checkBoxSwitchInstance.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.checkBoxSwitchInstance, "checkBoxSwitchInstance");
            //
            // buttonOK
            //
            this.buttonOK.Location = new System.Drawing.Point(256, 336);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 21;
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            resources.ApplyResources(this.buttonOK, "buttonOK");
            //
            // buttonCancel
            //
            this.buttonCancel.Location = new System.Drawing.Point(337, 336);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 22;
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            //
            // progressBar
            //
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(13, 336);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(230, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 23;
            this.progressBar.Visible = false;
            //
            // CloneFakeKspDialog
            //
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 371);
            this.Controls.Add(this.radioButtonClone);
            this.Controls.Add(this.radioButtonFake);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.buttonPathBrowser);
            this.Controls.Add(this.checkBoxSwitchInstance);
            this.Controls.Add(this.textBoxNewPath);
            this.Controls.Add(this.labelNewPath);
            this.Controls.Add(this.checkBoxSetAsDefault);
            this.Controls.Add(this.textBoxNewName);
            this.Controls.Add(this.labelNewName);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.fakeGroupBox);
            this.Controls.Add(this.cloneGroupBox);
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = Properties.Resources.AppIcon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CloneFakeKspDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            this.cloneGroupBox.ResumeLayout(false);
            this.cloneGroupBox.PerformLayout();
            this.fakeGroupBox.ResumeLayout(false);
            this.fakeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonClone;
        private System.Windows.Forms.GroupBox cloneGroupBox;
        private System.Windows.Forms.Label labelOldInstance;
        private System.Windows.Forms.ComboBox comboBoxKnownInstance;
        private System.Windows.Forms.Label labelOldPath;
        private System.Windows.Forms.TextBox textBoxClonePath;
        private System.Windows.Forms.Button buttonInstancePathSelection;
        private System.Windows.Forms.RadioButton radioButtonFake;
        private System.Windows.Forms.GroupBox fakeGroupBox;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.ComboBox comboBoxGameVersion;
        private System.Windows.Forms.Label labelDlcVersions;
        private System.Windows.Forms.TextBox textBoxMHDlcVersion;
        private System.Windows.Forms.Label labelMakingHistory;
        private System.Windows.Forms.TextBox textBoxBGDlcVersion;
        private System.Windows.Forms.Label labelBreakingGround;
        private System.Windows.Forms.Label labelNewName;
        private System.Windows.Forms.TextBox textBoxNewName;
        private System.Windows.Forms.Label labelNewPath;
        private System.Windows.Forms.TextBox textBoxNewPath;
        private System.Windows.Forms.Button buttonPathBrowser;
        private System.Windows.Forms.CheckBox checkBoxSetAsDefault;
        private System.Windows.Forms.CheckBox checkBoxSwitchInstance;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogNewPath;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}
