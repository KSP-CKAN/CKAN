namespace CKAN.GUI
{
    partial class NewRepoDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(NewRepoDialog));
            this.RepositoryGroupBox = new System.Windows.Forms.GroupBox();
            this.RepoNameLabel = new System.Windows.Forms.Label();
            this.RepoNameTextBox = new System.Windows.Forms.TextBox();
            this.RepoUrlLabel = new System.Windows.Forms.Label();
            this.RepoUrlTextBox = new System.Windows.Forms.TextBox();
            this.BottomPanel = new System.Windows.Forms.Panel();
            this.RepoCancel = new System.Windows.Forms.Button();
            this.RepoOK = new System.Windows.Forms.Button();
            this.ReposListBox = new ThemedListView();
            this.RepoNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RepoURLHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RepositoryGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // RepositoryGroupBox
            //
            this.RepositoryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RepositoryGroupBox.Controls.Add(this.ReposListBox);
            this.RepositoryGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.RepositoryGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RepositoryGroupBox.Location = new System.Drawing.Point(12, 12);
            this.RepositoryGroupBox.Name = "RepositoryGroupBox";
            this.RepositoryGroupBox.Size = new System.Drawing.Size(476, 380);
            this.RepositoryGroupBox.TabIndex = 8;
            this.RepositoryGroupBox.TabStop = false;
            resources.ApplyResources(this.RepositoryGroupBox, "RepositoryGroupBox");
            //
            // ReposListBox
            //
            this.ReposListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ReposListBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.RepoNameHeader,
            this.RepoURLHeader});
            this.ReposListBox.Location = new System.Drawing.Point(6, 19);
            this.ReposListBox.FullRowSelect = true;
            this.ReposListBox.MultiSelect = false;
            this.ReposListBox.View = System.Windows.Forms.View.Details;
            this.ReposListBox.Name = "ReposListBox";
            this.ReposListBox.Size = new System.Drawing.Size(464, 355);
            this.ReposListBox.TabIndex = 8;
            this.ReposListBox.SelectedIndexChanged += new System.EventHandler(this.ReposListBox_SelectedIndexChanged);
            this.ReposListBox.DoubleClick += new System.EventHandler(this.ReposListBox_DoubleClick);
            //
            // RepoNameHeader
            //
            this.RepoNameHeader.Width = 110;
            resources.ApplyResources(this.RepoNameHeader, "RepoNameHeader");
            //
            // RepoURLHeader
            //
            this.RepoURLHeader.Width = 370;
            resources.ApplyResources(this.RepoURLHeader, "RepoURLHeader");
            // 
            // BottomPanel
            // 
            this.BottomPanel.Controls.Add(this.RepoNameLabel);
            this.BottomPanel.Controls.Add(this.RepoNameTextBox);
            this.BottomPanel.Controls.Add(this.RepoUrlLabel);
            this.BottomPanel.Controls.Add(this.RepoUrlTextBox);
            this.BottomPanel.Controls.Add(this.RepoOK);
            this.BottomPanel.Controls.Add(this.RepoCancel);
            this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomPanel.Name = "BottomPanel";
            this.BottomPanel.Size = new System.Drawing.Size(500, 94);
            //
            // RepoNameLabel
            //
            this.RepoNameLabel.AutoSize = true;
            this.RepoNameLabel.Location = new System.Drawing.Point(6, 6);
            this.RepoNameLabel.Name = "RepoNameLabel";
            this.RepoNameLabel.Size = new System.Drawing.Size(70, 13);
            this.RepoNameLabel.TabIndex = 1;
            resources.ApplyResources(this.RepoNameLabel, "RepoNameLabel");
            //
            // RepoNameTextBox
            //
            this.RepoNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.RepoNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RepoNameTextBox.Location = new System.Drawing.Point(6, 22);
            this.RepoNameTextBox.Name = "RepoNameTextBox";
            this.RepoNameTextBox.Size = new System.Drawing.Size(110, 30);
            this.RepoNameTextBox.TabIndex = 11;
            this.RepoNameTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.RepoNameTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.RepoNameTextBox.TextChanged += new System.EventHandler(this.RepoUrlTextBox_TextChanged);
            //
            // RepoUrlLabel
            //
            this.RepoUrlLabel.AutoSize = true;
            this.RepoUrlLabel.Location = new System.Drawing.Point(122, 6);
            this.RepoUrlLabel.Name = "RepoUrlLabel";
            this.RepoUrlLabel.Size = new System.Drawing.Size(70, 13);
            this.RepoUrlLabel.TabIndex = 1;
            resources.ApplyResources(this.RepoUrlLabel, "RepoUrlLabel");
            //
            // RepoUrlTextBox
            //
            this.RepoUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.RepoUrlTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.RepoUrlTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.RepoUrlTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RepoUrlTextBox.Location = new System.Drawing.Point(122, 22);
            this.RepoUrlTextBox.Name = "RepoUrlTextBox";
            this.RepoUrlTextBox.Size = new System.Drawing.Size(372, 30);
            this.RepoUrlTextBox.TabIndex = 11;
            this.RepoUrlTextBox.TextChanged += new System.EventHandler(this.RepoUrlTextBox_TextChanged);
            //
            // RepoCancel
            //
            this.RepoCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RepoCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.RepoCancel.Location = new System.Drawing.Point(348, 56);
            this.RepoCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RepoCancel.Name = "RepoCancel";
            this.RepoCancel.Size = new System.Drawing.Size(70, 30);
            this.RepoCancel.TabIndex = 10;
            this.RepoCancel.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.RepoCancel, "RepoCancel");
            //
            // RepoOK
            //
            this.RepoOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RepoOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.RepoOK.Enabled = false;
            this.RepoOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RepoOK.Location = new System.Drawing.Point(424, 56);
            this.RepoOK.Name = "RepoOK";
            this.RepoOK.Size = new System.Drawing.Size(70, 30);
            this.RepoOK.TabIndex = 9;
            this.RepoOK.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.RepoOK, "RepoOK");
            //
            // NewRepoDialog
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(500, 220);
            this.MinimumSize = new System.Drawing.Size(520, 260);
            this.Controls.Add(this.RepositoryGroupBox);
            this.Controls.Add(this.BottomPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Icon = EmbeddedImages.AppIcon;
            this.Name = "NewRepoDialog";
            this.Load += new System.EventHandler(this.NewRepoDialog_Load);
            resources.ApplyResources(this, "$this");
            this.RepositoryGroupBox.ResumeLayout(false);
            this.RepositoryGroupBox.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox RepositoryGroupBox;
        private System.Windows.Forms.ListView ReposListBox;
        private System.Windows.Forms.ColumnHeader RepoNameHeader;
        private System.Windows.Forms.ColumnHeader RepoURLHeader;
        private System.Windows.Forms.Label RepoNameLabel;
        private System.Windows.Forms.TextBox RepoNameTextBox;
        private System.Windows.Forms.Label RepoUrlLabel;
        private System.Windows.Forms.TextBox RepoUrlTextBox;
        private System.Windows.Forms.Panel BottomPanel;
        private System.Windows.Forms.Button RepoOK;
        private System.Windows.Forms.Button RepoCancel;
    }
}
