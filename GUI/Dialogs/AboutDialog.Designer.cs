using System;
using System.Windows.Forms;

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
            this.projectNameLabel.Location = new System.Drawing.Point(6, 5);
            this.projectNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.projectNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.projectNameLabel.Name = "projectNameLabel";
            this.projectNameLabel.Size = new System.Drawing.Size(380, 13);
            this.projectNameLabel.TabIndex = 0;
            this.projectNameLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            resources.ApplyResources(this.projectNameLabel, "projectNameLabel");
            //
            // versionLabel
            //
            this.versionLabel.Location = new System.Drawing.Point(6, 25);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(100, 13);
            this.versionLabel.TabIndex = 6;
            this.versionLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            resources.ApplyResources(this.versionLabel, "versionLabel");
            //
            // licenseLabel
            //
            this.licenseLabel.Location = new System.Drawing.Point(6, 55);
            this.licenseLabel.Name = "licenseLabel";
            this.licenseLabel.Size = new System.Drawing.Size(100, 13);
            this.licenseLabel.TabIndex = 6;
            this.licenseLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            resources.ApplyResources(this.licenseLabel, "licenseLabel");
            //
            // licenseLinkLabel
            //
            this.licenseLinkLabel.Location = new System.Drawing.Point(110, 55);
            this.licenseLinkLabel.Name = "licenseLinkLabel";
            this.licenseLinkLabel.Text = "https://github.com/KSP-CKAN/CKAN/blob/master/LICENSE.md";
            this.licenseLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 56);
            this.licenseLinkLabel.Size = new System.Drawing.Size(380, 13);
            this.licenseLinkLabel.TabIndex = 1;
            this.licenseLinkLabel.TabStop = true;
            this.licenseLinkLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.licenseLinkLabel.UseCompatibleTextRendering = true;
            this.licenseLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.licenseLinkLabel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.linkLabel_KeyDown);
            //
            // authorsLabel
            //
            this.authorsLabel.Location = new System.Drawing.Point(6, 75);
            this.authorsLabel.Name = "authorsLabel";
            this.authorsLabel.Size = new System.Drawing.Size(100, 13);
            this.authorsLabel.TabIndex = 6;
            this.authorsLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            resources.ApplyResources(this.authorsLabel, "authorsLabel");
            //
            // authorsLinkLabel
            //
            this.authorsLinkLabel.Location = new System.Drawing.Point(110, 75);
            this.authorsLinkLabel.Name = "authorsLinkLabel";
            this.authorsLinkLabel.Text = "https://github.com/KSP-CKAN/CKAN/graphs/contributors";
            this.authorsLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 52);
            this.authorsLinkLabel.Size = new System.Drawing.Size(380, 13);
            this.authorsLinkLabel.TabIndex = 2;
            this.authorsLinkLabel.TabStop = true;
            this.authorsLinkLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.authorsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.authorsLinkLabel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.linkLabel_KeyDown);
            //
            // sourceLabel
            //
            this.sourceLabel.Location = new System.Drawing.Point(6, 95);
            this.sourceLabel.Name = "sourceLabel";
            this.sourceLabel.Size = new System.Drawing.Size(100, 13);
            this.sourceLabel.TabIndex = 6;
            this.sourceLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            resources.ApplyResources(this.sourceLabel, "sourceLabel");
            //
            // sourceLinkLabel
            //
            this.sourceLinkLabel.Location = new System.Drawing.Point(110, 95);
            this.sourceLinkLabel.Name = "sourceLinkLabel";
            this.sourceLinkLabel.Text = "https://github.com/KSP-CKAN/CKAN/";
            this.sourceLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 33);
            this.sourceLinkLabel.Size = new System.Drawing.Size(380, 13);
            this.sourceLinkLabel.TabIndex = 3;
            this.sourceLinkLabel.TabStop = true;
            this.sourceLinkLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.sourceLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.sourceLinkLabel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.linkLabel_KeyDown);
            //
            // forumthreadLabel
            //
            this.forumthreadLabel.Location = new System.Drawing.Point(6, 115);
            this.forumthreadLabel.Name = "forumthreadLinkLabel";
            this.forumthreadLabel.Size = new System.Drawing.Size(100, 13);
            this.forumthreadLabel.TabIndex = 6;
            this.forumthreadLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            resources.ApplyResources(this.forumthreadLabel, "forumthreadLabel");
            //
            // forumthreadLinkLabel
            //
            this.forumthreadLinkLabel.Location = new System.Drawing.Point(110, 115);
            this.forumthreadLinkLabel.Name = "forumthreadLinkLabel";
            this.forumthreadLinkLabel.Text = "http://forum.kerbalspaceprogram.com/index.php?/topic/197082-ckan";
            this.forumthreadLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 64);
            this.forumthreadLinkLabel.Size = new System.Drawing.Size(380, 13);
            this.forumthreadLinkLabel.TabIndex = 4;
            this.forumthreadLinkLabel.TabStop = true;
            this.forumthreadLinkLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.forumthreadLinkLabel.UseCompatibleTextRendering = true;
            this.forumthreadLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.forumthreadLinkLabel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.linkLabel_KeyDown);
            //
            // homepageLabel
            //
            this.homepageLabel.Location = new System.Drawing.Point(6, 135);
            this.homepageLabel.Name = "homepageLinkLabel";
            this.homepageLabel.Size = new System.Drawing.Size(100, 13);
            this.homepageLabel.TabIndex = 6;
            this.homepageLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            resources.ApplyResources(this.homepageLabel, "homepageLabel");
            //
            // homepageLinkLabel
            //
            this.homepageLinkLabel.Location = new System.Drawing.Point(110, 135);
            this.homepageLinkLabel.Name = "homepageLinkLabel";
            this.homepageLinkLabel.Text = "http://ksp-ckan.space";
            this.homepageLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(0, 21);
            this.homepageLinkLabel.Size = new System.Drawing.Size(380, 13);
            this.homepageLinkLabel.TabIndex = 5;
            this.homepageLinkLabel.TabStop = true;
            this.homepageLinkLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.homepageLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.homepageLinkLabel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.linkLabel_KeyDown);
            //
            // AboutDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 155);
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
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
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
