namespace CKAN.GUI
{
    partial class AboutDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(AboutDialog));
            this.projectNameLabel = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.licenseLabel = new System.Windows.Forms.Label();
            this.licenseLinkLabel = new System.Windows.Forms.LinkLabel();
            this.authorsLabel = new System.Windows.Forms.Label();
            this.authorsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.sourceLabel = new System.Windows.Forms.Label();
            this.sourceLinkLabel = new System.Windows.Forms.LinkLabel();
            this.homepageLabel = new System.Windows.Forms.Label();
            this.homepageLinkLabel = new System.Windows.Forms.LinkLabel();
            this.forumthreadLabel = new System.Windows.Forms.Label();
            this.forumthreadLinkLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            //
            // projectNameLabel
            //
            this.projectNameLabel.AutoSize = true;
            this.projectNameLabel.Location = new System.Drawing.Point(6, 5);
            this.projectNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.projectNameLabel.Font = new System.Drawing.Font(System.Drawing.SystemFonts.CaptionFont, System.Drawing.FontStyle.Bold);
            this.projectNameLabel.Name = "projectNameLabel";
            this.projectNameLabel.Size = new System.Drawing.Size(530, 16);
            this.projectNameLabel.TabIndex = 0;
            resources.ApplyResources(this.projectNameLabel, "projectNameLabel");
            //
            // versionLabel
            //
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(6, 25);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(100, 16);
            this.versionLabel.TabIndex = 1;
            resources.ApplyResources(this.versionLabel, "versionLabel");
            //
            // licenseLabel
            //
            this.licenseLabel.AutoSize = true;
            this.licenseLabel.Location = new System.Drawing.Point(6, 55);
            this.licenseLabel.Name = "licenseLabel";
            this.licenseLabel.Size = new System.Drawing.Size(100, 16);
            this.licenseLabel.TabIndex = 2;
            resources.ApplyResources(this.licenseLabel, "licenseLabel");
            //
            // licenseLinkLabel
            //
            this.licenseLinkLabel.AutoSize = true;
            this.licenseLinkLabel.Location = new System.Drawing.Point(110, 55);
            this.licenseLinkLabel.Name = "licenseLinkLabel";
            this.licenseLinkLabel.Text = "https://github.com/KSP-CKAN/CKAN/blob/master/LICENSE.md";
            this.licenseLinkLabel.Size = new System.Drawing.Size(430, 16);
            this.licenseLinkLabel.TabIndex = 3;
            this.licenseLinkLabel.TabStop = true;
            this.licenseLinkLabel.UseCompatibleTextRendering = true;
            this.licenseLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.licenseLinkLabel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.linkLabel_KeyDown);
            //
            // authorsLabel
            //
            this.authorsLabel.AutoSize = true;
            this.authorsLabel.Location = new System.Drawing.Point(6, 75);
            this.authorsLabel.Name = "authorsLabel";
            this.authorsLabel.Size = new System.Drawing.Size(100, 16);
            this.authorsLabel.TabIndex = 4;
            resources.ApplyResources(this.authorsLabel, "authorsLabel");
            //
            // authorsLinkLabel
            //
            this.authorsLinkLabel.AutoSize = true;
            this.authorsLinkLabel.Location = new System.Drawing.Point(110, 75);
            this.authorsLinkLabel.Name = "authorsLinkLabel";
            this.authorsLinkLabel.Text = "https://github.com/KSP-CKAN/CKAN/graphs/contributors";
            this.authorsLinkLabel.Size = new System.Drawing.Size(430, 16);
            this.authorsLinkLabel.TabIndex = 5;
            this.authorsLinkLabel.TabStop = true;
            this.authorsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.authorsLinkLabel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.linkLabel_KeyDown);
            //
            // sourceLabel
            //
            this.sourceLabel.AutoSize = true;
            this.sourceLabel.Location = new System.Drawing.Point(6, 95);
            this.sourceLabel.Name = "sourceLabel";
            this.sourceLabel.Size = new System.Drawing.Size(100, 16);
            this.sourceLabel.TabIndex = 6;
            resources.ApplyResources(this.sourceLabel, "sourceLabel");
            //
            // sourceLinkLabel
            //
            this.sourceLinkLabel.AutoSize = true;
            this.sourceLinkLabel.Location = new System.Drawing.Point(110, 95);
            this.sourceLinkLabel.Name = "sourceLinkLabel";
            this.sourceLinkLabel.Text = "https://github.com/KSP-CKAN/CKAN/";
            this.sourceLinkLabel.Size = new System.Drawing.Size(430, 16);
            this.sourceLinkLabel.TabIndex = 7;
            this.sourceLinkLabel.TabStop = true;
            this.sourceLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.sourceLinkLabel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.linkLabel_KeyDown);
            //
            // forumthreadLabel
            //
            this.forumthreadLabel.AutoSize = true;
            this.forumthreadLabel.Location = new System.Drawing.Point(6, 115);
            this.forumthreadLabel.Name = "forumthreadLinkLabel";
            this.forumthreadLabel.Size = new System.Drawing.Size(100, 16);
            this.forumthreadLabel.TabIndex = 8;
            resources.ApplyResources(this.forumthreadLabel, "forumthreadLabel");
            //
            // forumthreadLinkLabel
            //
            this.forumthreadLinkLabel.AutoSize = true;
            this.forumthreadLinkLabel.Location = new System.Drawing.Point(110, 115);
            this.forumthreadLinkLabel.Name = "forumthreadLinkLabel";
            this.forumthreadLinkLabel.Text = "https://forum.kerbalspaceprogram.com/index.php?/topic/197082-ckan";
            this.forumthreadLinkLabel.Size = new System.Drawing.Size(430, 16);
            this.forumthreadLinkLabel.TabIndex = 9;
            this.forumthreadLinkLabel.TabStop = true;
            this.forumthreadLinkLabel.UseCompatibleTextRendering = true;
            this.forumthreadLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.forumthreadLinkLabel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.linkLabel_KeyDown);
            //
            // homepageLabel
            //
            this.homepageLabel.AutoSize = true;
            this.homepageLabel.Location = new System.Drawing.Point(6, 135);
            this.homepageLabel.Name = "homepageLinkLabel";
            this.homepageLabel.Size = new System.Drawing.Size(100, 16);
            this.homepageLabel.TabIndex = 10;
            resources.ApplyResources(this.homepageLabel, "homepageLabel");
            //
            // homepageLinkLabel
            //
            this.homepageLinkLabel.AutoSize = true;
            this.homepageLinkLabel.Location = new System.Drawing.Point(110, 135);
            this.homepageLinkLabel.Name = "homepageLinkLabel";
            this.homepageLinkLabel.Text = "https://ksp-ckan.space";
            this.homepageLinkLabel.Size = new System.Drawing.Size(430, 16);
            this.homepageLinkLabel.TabIndex = 11;
            this.homepageLinkLabel.TabStop = true;
            this.homepageLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.homepageLinkLabel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.linkLabel_KeyDown);
            //
            // AboutDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(550, 165);
            this.Controls.Add(this.projectNameLabel);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.licenseLabel);
            this.Controls.Add(this.licenseLinkLabel);
            this.Controls.Add(this.authorsLabel);
            this.Controls.Add(this.authorsLinkLabel);
            this.Controls.Add(this.sourceLabel);
            this.Controls.Add(this.sourceLinkLabel);
            this.Controls.Add(this.forumthreadLabel);
            this.Controls.Add(this.forumthreadLinkLabel);
            this.Controls.Add(this.homepageLabel);
            this.Controls.Add(this.homepageLinkLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = EmbeddedImages.AppIcon;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label projectNameLabel;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Label licenseLabel;
        private System.Windows.Forms.LinkLabel licenseLinkLabel;
        private System.Windows.Forms.Label authorsLabel;
        private System.Windows.Forms.LinkLabel authorsLinkLabel;
        private System.Windows.Forms.Label sourceLabel;
        private System.Windows.Forms.LinkLabel sourceLinkLabel;
        private System.Windows.Forms.Label homepageLabel;
        private System.Windows.Forms.LinkLabel homepageLinkLabel;
        private System.Windows.Forms.Label forumthreadLabel;
        private System.Windows.Forms.LinkLabel forumthreadLinkLabel;

    }
}
