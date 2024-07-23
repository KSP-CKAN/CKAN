namespace CKAN.GUI
{
    partial class CloneGameInstanceDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(CloneGameInstanceDialog));
            this.ToolTip = new System.Windows.Forms.ToolTip();
            this.labelOldInstance = new System.Windows.Forms.Label();
            this.comboBoxKnownInstance = new System.Windows.Forms.ComboBox();
            this.labelOldPath = new System.Windows.Forms.Label();
            this.textBoxClonePath = new System.Windows.Forms.TextBox();
            this.buttonInstancePathSelection = new System.Windows.Forms.Button();
            this.labelNewName = new System.Windows.Forms.Label();
            this.textBoxNewName = new System.Windows.Forms.TextBox();
            this.labelNewPath = new System.Windows.Forms.Label();
            this.textBoxNewPath = new System.Windows.Forms.TextBox();
            this.buttonPathBrowser = new System.Windows.Forms.Button();
            this.checkBoxSetAsDefault = new System.Windows.Forms.CheckBox();
            this.checkBoxSwitchInstance = new System.Windows.Forms.CheckBox();
            this.checkBoxShareStock = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.folderBrowserDialogNewPath = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            //
            // ToolTip
            //
            this.ToolTip.AutoPopDelay = 10000;
            this.ToolTip.InitialDelay = 250;
            this.ToolTip.ReshowDelay = 250;
            this.ToolTip.ShowAlways = true;
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
            this.comboBoxKnownInstance.Location = new System.Drawing.Point(181, 19);
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
            this.textBoxClonePath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxClonePath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxClonePath.AllowDrop = true;
            this.textBoxClonePath.Location = new System.Drawing.Point(181, 49);
            this.textBoxClonePath.Name = "textBoxClonePath";
            this.textBoxClonePath.Size = new System.Drawing.Size(158, 20);
            this.textBoxClonePath.TabIndex = 5;
            this.textBoxClonePath.Text = "";
            //
            // buttonInstancePathSelection
            //
            this.buttonInstancePathSelection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonInstancePathSelection.Location = new System.Drawing.Point(339, 48);
            this.buttonInstancePathSelection.Name = "buttonInstancePathSelection";
            this.buttonInstancePathSelection.Size = new System.Drawing.Size(60, 22);
            this.buttonInstancePathSelection.TabIndex = 6;
            this.buttonInstancePathSelection.UseVisualStyleBackColor = true;
            this.buttonInstancePathSelection.Click += new System.EventHandler(this.buttonInstancePathSelection_Click);
            resources.ApplyResources(this.buttonInstancePathSelection, "buttonInstancePathSelection");
            //
            // labelNewName
            //
            this.labelNewName.AutoSize = true;
            this.labelNewName.Location = new System.Drawing.Point(12, 82);
            this.labelNewName.Name = "labelNewName";
            this.labelNewName.Size = new System.Drawing.Size(137, 13);
            this.labelNewName.TabIndex = 14;
            resources.ApplyResources(this.labelNewName, "labelNewName");
            //
            // textBoxNewName
            //
            this.textBoxNewName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxNewName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxNewName.Location = new System.Drawing.Point(181, 79);
            this.textBoxNewName.Name = "textBoxNewName";
            this.textBoxNewName.Size = new System.Drawing.Size(218, 20);
            this.textBoxNewName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxNewName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxNewName.TabIndex = 15;
            //
            // labelNewPath
            //
            this.labelNewPath.AutoSize = true;
            this.labelNewPath.Location = new System.Drawing.Point(12, 112);
            this.labelNewPath.Name = "labelNewPath";
            this.labelNewPath.Size = new System.Drawing.Size(131, 13);
            this.labelNewPath.TabIndex = 16;
            resources.ApplyResources(this.labelNewPath, "labelNewPath");
            //
            // textBoxNewPath
            //
            this.textBoxNewPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxNewPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxNewPath.Location = new System.Drawing.Point(181, 109);
            this.textBoxNewPath.Name = "textBoxNewPath";
            this.textBoxNewPath.Size = new System.Drawing.Size(158, 20);
            this.textBoxNewPath.TabIndex = 17;
            //
            // buttonPathBrowser
            //
            this.buttonPathBrowser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPathBrowser.Location = new System.Drawing.Point(339, 108);
            this.buttonPathBrowser.Name = "buttonPathBrowser";
            this.buttonPathBrowser.Size = new System.Drawing.Size(60, 22);
            this.buttonPathBrowser.TabIndex = 18;
            this.buttonPathBrowser.UseVisualStyleBackColor = true;
            this.buttonPathBrowser.Click += new System.EventHandler(this.buttonPathBrowser_Click);
            resources.ApplyResources(this.buttonPathBrowser, "buttonPathBrowser");
            //
            // checkBoxSetAsDefault
            //
            this.checkBoxSetAsDefault.AutoSize = true;
            this.checkBoxSetAsDefault.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxSetAsDefault.Location = new System.Drawing.Point(181, 144);
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
            this.checkBoxSwitchInstance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxSwitchInstance.Location = new System.Drawing.Point(181, 174);
            this.checkBoxSwitchInstance.Name = "checkBoxSwitchInstance";
            this.checkBoxSwitchInstance.Size = new System.Drawing.Size(136, 17);
            this.checkBoxSwitchInstance.TabIndex = 20;
            this.checkBoxSwitchInstance.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.checkBoxSwitchInstance, "checkBoxSwitchInstance");
            //
            // checkBoxShareStock
            //
            this.checkBoxShareStock.AutoSize = true;
            this.checkBoxShareStock.Checked = true;
            this.checkBoxShareStock.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShareStock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxShareStock.Location = new System.Drawing.Point(181, 204);
            this.checkBoxShareStock.Name = "checkBoxShareStock";
            this.checkBoxShareStock.Size = new System.Drawing.Size(136, 17);
            this.checkBoxShareStock.TabIndex = 21;
            this.checkBoxShareStock.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.checkBoxShareStock, "checkBoxShareStock");
            //
            // buttonOK
            //
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonOK.Location = new System.Drawing.Point(256, 230);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 22;
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            resources.ApplyResources(this.buttonOK, "buttonOK");
            //
            // buttonCancel
            //
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Location = new System.Drawing.Point(337, 230);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 23;
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            //
            // progressBar
            //
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(13, 170);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(230, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 24;
            this.progressBar.Visible = false;
            //
            // CloneGameInstanceDialog
            //
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 265);
            this.Controls.Add(this.labelOldInstance);
            this.Controls.Add(this.comboBoxKnownInstance);
            this.Controls.Add(this.labelOldPath);
            this.Controls.Add(this.textBoxClonePath);
            this.Controls.Add(this.buttonInstancePathSelection);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.buttonPathBrowser);
            this.Controls.Add(this.checkBoxSwitchInstance);
            this.Controls.Add(this.checkBoxShareStock);
            this.Controls.Add(this.textBoxNewPath);
            this.Controls.Add(this.labelNewPath);
            this.Controls.Add(this.checkBoxSetAsDefault);
            this.Controls.Add(this.textBoxNewName);
            this.Controls.Add(this.labelNewName);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = EmbeddedImages.AppIcon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.HelpButton = true;
            this.Name = "CloneGameInstanceDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Label labelOldInstance;
        private System.Windows.Forms.ComboBox comboBoxKnownInstance;
        private System.Windows.Forms.Label labelOldPath;
        private System.Windows.Forms.TextBox textBoxClonePath;
        private System.Windows.Forms.Button buttonInstancePathSelection;
        private System.Windows.Forms.Label labelNewName;
        private System.Windows.Forms.TextBox textBoxNewName;
        private System.Windows.Forms.Label labelNewPath;
        private System.Windows.Forms.TextBox textBoxNewPath;
        private System.Windows.Forms.Button buttonPathBrowser;
        private System.Windows.Forms.CheckBox checkBoxSetAsDefault;
        private System.Windows.Forms.CheckBox checkBoxSwitchInstance;
        private System.Windows.Forms.CheckBox checkBoxShareStock;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogNewPath;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}
