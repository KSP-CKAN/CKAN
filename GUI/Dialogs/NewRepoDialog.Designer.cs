namespace CKAN
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
            this.RepoUrlTextBox = new System.Windows.Forms.TextBox();
            this.RepoCancel = new System.Windows.Forms.Button();
            this.RepoOK = new System.Windows.Forms.Button();
            this.ReposListBox = new System.Windows.Forms.ListBox();
            this.RepositoryGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // RepositoryGroupBox
            //
            this.RepositoryGroupBox.Controls.Add(this.RepoUrlTextBox);
            this.RepositoryGroupBox.Controls.Add(this.RepoCancel);
            this.RepositoryGroupBox.Controls.Add(this.RepoOK);
            this.RepositoryGroupBox.Controls.Add(this.ReposListBox);
            this.RepositoryGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RepositoryGroupBox.Location = new System.Drawing.Point(12, 12);
            this.RepositoryGroupBox.Name = "RepositoryGroupBox";
            this.RepositoryGroupBox.Size = new System.Drawing.Size(476, 410);
            this.RepositoryGroupBox.TabIndex = 8;
            this.RepositoryGroupBox.TabStop = false;
            resources.ApplyResources(this.RepositoryGroupBox, "RepositoryGroupBox");
            //
            // RepoUrlTextBox
            //
            this.RepoUrlTextBox.Location = new System.Drawing.Point(6, 384);
            this.RepoUrlTextBox.Name = "RepoUrlTextBox";
            this.RepoUrlTextBox.Size = new System.Drawing.Size(332, 20);
            this.RepoUrlTextBox.TabIndex = 11;
            this.RepoUrlTextBox.TextChanged += new System.EventHandler(this.RepoUrlTextBox_TextChanged);
            //
            // RepoCancel
            //
            this.RepoCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.RepoCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RepoCancel.Location = new System.Drawing.Point(344, 380);
            this.RepoCancel.Name = "RepoCancel";
            this.RepoCancel.Size = new System.Drawing.Size(70, 26);
            this.RepoCancel.TabIndex = 10;
            this.RepoCancel.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.RepoCancel, "RepoCancel");
            //
            // RepoOK
            //
            this.RepoOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.RepoOK.Enabled = false;
            this.RepoOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RepoOK.Location = new System.Drawing.Point(420, 380);
            this.RepoOK.Name = "RepoOK";
            this.RepoOK.Size = new System.Drawing.Size(50, 26);
            this.RepoOK.TabIndex = 9;
            this.RepoOK.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.RepoOK, "RepoOK");
            //
            // ReposListBox
            //
            this.ReposListBox.FormattingEnabled = true;
            this.ReposListBox.Location = new System.Drawing.Point(6, 19);
            this.ReposListBox.Name = "ReposListBox";
            this.ReposListBox.Size = new System.Drawing.Size(464, 355);
            this.ReposListBox.TabIndex = 8;
            this.ReposListBox.SelectedIndexChanged += new System.EventHandler(this.ReposListBox_SelectedIndexChanged);
            //
            // NewRepoDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 434);
            this.Controls.Add(this.RepositoryGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = Properties.Resources.AppIcon;
            this.Name = "NewRepoDialog";
            this.Load += new System.EventHandler(this.NewRepoDialog_Load);
            resources.ApplyResources(this, "$this");
            this.RepositoryGroupBox.ResumeLayout(false);
            this.RepositoryGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox RepositoryGroupBox;
        private System.Windows.Forms.Button RepoOK;
        private System.Windows.Forms.ListBox ReposListBox;
        private System.Windows.Forms.Button RepoCancel;
        public System.Windows.Forms.TextBox RepoUrlTextBox;
    }
}
