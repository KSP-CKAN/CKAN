namespace CKAN.GUI
{
    partial class Relationships
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(Relationships));
            this.ToolTip = new System.Windows.Forms.ToolTip();
            this.DependsGraphTree = new System.Windows.Forms.TreeView();
            this.LegendProvidesImage = new System.Windows.Forms.PictureBox();
            this.LegendProvidesLabel = new System.Windows.Forms.Label();
            this.LegendDependsImage = new System.Windows.Forms.PictureBox();
            this.LegendDependsLabel = new System.Windows.Forms.Label();
            this.LegendRecommendsImage = new System.Windows.Forms.PictureBox();
            this.LegendRecommendsLabel = new System.Windows.Forms.Label();
            this.LegendSuggestsImage = new System.Windows.Forms.PictureBox();
            this.LegendSuggestsLabel = new System.Windows.Forms.Label();
            this.LegendSupportsImage = new System.Windows.Forms.PictureBox();
            this.LegendSupportsLabel = new System.Windows.Forms.Label();
            this.LegendConflictsImage = new System.Windows.Forms.PictureBox();
            this.LegendConflictsLabel = new System.Windows.Forms.Label();
            this.ReverseRelationshipsCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            //
            // ToolTip
            //
            this.ToolTip.AutoPopDelay = 10000;
            this.ToolTip.InitialDelay = 250;
            this.ToolTip.ReshowDelay = 250;
            this.ToolTip.ShowAlways = true;
            //
            // DependsGraphTree
            //
            this.DependsGraphTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DependsGraphTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DependsGraphTree.Location = new System.Drawing.Point(3, 114);
            this.DependsGraphTree.Name = "DependsGraphTree";
            this.DependsGraphTree.Size = new System.Drawing.Size(494, 364);
            this.DependsGraphTree.TabIndex = 0;
            this.DependsGraphTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.DependsGraphTree_NodeMouseDoubleClick);
            this.DependsGraphTree.ShowNodeToolTips = true;
            this.DependsGraphTree.ImageList = new System.Windows.Forms.ImageList(this.components)
            {
                // ImageList's default makes icons look like garbage
                ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
            };
            this.DependsGraphTree.ImageList.Images.Add("Root", global::CKAN.GUI.EmbeddedImages.ksp);
            this.DependsGraphTree.ImageList.Images.Add("Provides", global::CKAN.GUI.EmbeddedImages.ballot);
            this.DependsGraphTree.ImageList.Images.Add("Depends", global::CKAN.GUI.EmbeddedImages.star);
            this.DependsGraphTree.ImageList.Images.Add("Recommends", global::CKAN.GUI.EmbeddedImages.thumbup);
            this.DependsGraphTree.ImageList.Images.Add("Suggests", global::CKAN.GUI.EmbeddedImages.info);
            this.DependsGraphTree.ImageList.Images.Add("Supports", global::CKAN.GUI.EmbeddedImages.smile);
            this.DependsGraphTree.ImageList.Images.Add("Conflicts", global::CKAN.GUI.EmbeddedImages.alert);
            //
            // LegendProvidesImage
            //
            this.LegendProvidesImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendProvidesImage.Image = global::CKAN.GUI.EmbeddedImages.ballot;
            this.LegendProvidesImage.Location = new System.Drawing.Point(6, 3);
            this.LegendProvidesImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendProvidesImage.ClientSize = new System.Drawing.Size(14, 14);
            //
            // LegendProvidesLabel
            //
            this.LegendProvidesLabel.AutoSize = true;
            this.LegendProvidesLabel.Location = new System.Drawing.Point(24, 3);
            resources.ApplyResources(this.LegendProvidesLabel, "LegendProvidesLabel");
            //
            // LegendDependsImage
            //
            this.LegendDependsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendDependsImage.Image = global::CKAN.GUI.EmbeddedImages.star;
            this.LegendDependsImage.Location = new System.Drawing.Point(6, 21);
            this.LegendDependsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendDependsImage.ClientSize = new System.Drawing.Size(14, 14);
            //
            // LegendDependsLabel
            //
            this.LegendDependsLabel.AutoSize = true;
            this.LegendDependsLabel.Location = new System.Drawing.Point(24, 21);
            resources.ApplyResources(this.LegendDependsLabel, "LegendDependsLabel");
            //
            // LegendRecommendsImage
            //
            this.LegendRecommendsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendRecommendsImage.Image = global::CKAN.GUI.EmbeddedImages.thumbup;
            this.LegendRecommendsImage.Location = new System.Drawing.Point(6, 39);
            this.LegendRecommendsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendRecommendsImage.ClientSize = new System.Drawing.Size(14, 14);
            //
            // LegendRecommendsLabel
            //
            this.LegendRecommendsLabel.AutoSize = true;
            this.LegendRecommendsLabel.Location = new System.Drawing.Point(24, 39);
            resources.ApplyResources(this.LegendRecommendsLabel, "LegendRecommendsLabel");
            //
            // LegendSuggestsImage
            //
            this.LegendSuggestsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendSuggestsImage.Image = global::CKAN.GUI.EmbeddedImages.info;
            this.LegendSuggestsImage.Location = new System.Drawing.Point(6, 57);
            this.LegendSuggestsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendSuggestsImage.ClientSize = new System.Drawing.Size(14, 14);
            //
            // LegendSuggestsLabel
            //
            this.LegendSuggestsLabel.AutoSize = true;
            this.LegendSuggestsLabel.Location = new System.Drawing.Point(24, 57);
            resources.ApplyResources(this.LegendSuggestsLabel, "LegendSuggestsLabel");
            //
            // LegendSupportsImage
            //
            this.LegendSupportsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendSupportsImage.Image = global::CKAN.GUI.EmbeddedImages.smile;
            this.LegendSupportsImage.Location = new System.Drawing.Point(6, 75);
            this.LegendSupportsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendSupportsImage.ClientSize = new System.Drawing.Size(14, 14);
            //
            // LegendSupportsLabel
            //
            this.LegendSupportsLabel.AutoSize = true;
            this.LegendSupportsLabel.Location = new System.Drawing.Point(24, 75);
            resources.ApplyResources(this.LegendSupportsLabel, "LegendSupportsLabel");
            //
            // LegendConflictsImage
            //
            this.LegendConflictsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendConflictsImage.Image = global::CKAN.GUI.EmbeddedImages.alert;
            this.LegendConflictsImage.Location = new System.Drawing.Point(6, 93);
            this.LegendConflictsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendConflictsImage.ClientSize = new System.Drawing.Size(14, 14);
            //
            // LegendConflictsLabel
            //
            this.LegendConflictsLabel.AutoSize = true;
            this.LegendConflictsLabel.Location = new System.Drawing.Point(24, 93);
            resources.ApplyResources(this.LegendConflictsLabel, "LegendConflictsLabel");
            //
            // ReverseRelationshipsCheckbox
            //
            this.ReverseRelationshipsCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReverseRelationshipsCheckbox.AutoSize = true;
            this.ReverseRelationshipsCheckbox.AutoCheck = false;
            this.ReverseRelationshipsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReverseRelationshipsCheckbox.Name = "ReverseRelationshipsCheckbox";
            this.ReverseRelationshipsCheckbox.Location = new System.Drawing.Point(3, 474);
            this.ReverseRelationshipsCheckbox.Size = new System.Drawing.Size(494, 24);
            this.ReverseRelationshipsCheckbox.Click += new System.EventHandler(this.ReverseRelationshipsCheckbox_Click);
            this.ReverseRelationshipsCheckbox.CheckedChanged += new System.EventHandler(this.ReverseRelationshipsCheckbox_CheckedChanged);
            resources.ApplyResources(this.ReverseRelationshipsCheckbox, "ReverseRelationshipsCheckbox");
            //
            // Relationships
            //
            this.Controls.Add(this.DependsGraphTree);
            this.Controls.Add(this.LegendProvidesImage);
            this.Controls.Add(this.LegendProvidesLabel);
            this.Controls.Add(this.LegendDependsImage);
            this.Controls.Add(this.LegendDependsLabel);
            this.Controls.Add(this.LegendRecommendsImage);
            this.Controls.Add(this.LegendRecommendsLabel);
            this.Controls.Add(this.LegendSuggestsImage);
            this.Controls.Add(this.LegendSuggestsLabel);
            this.Controls.Add(this.LegendSupportsImage);
            this.Controls.Add(this.LegendSupportsLabel);
            this.Controls.Add(this.LegendConflictsImage);
            this.Controls.Add(this.LegendConflictsLabel);
            this.Controls.Add(this.ReverseRelationshipsCheckbox);
            this.Name = "Relationships";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.TreeView DependsGraphTree;
        private System.Windows.Forms.PictureBox LegendProvidesImage;
        private System.Windows.Forms.Label LegendProvidesLabel;
        private System.Windows.Forms.PictureBox LegendDependsImage;
        private System.Windows.Forms.Label LegendDependsLabel;
        private System.Windows.Forms.PictureBox LegendRecommendsImage;
        private System.Windows.Forms.Label LegendRecommendsLabel;
        private System.Windows.Forms.PictureBox LegendSuggestsImage;
        private System.Windows.Forms.Label LegendSuggestsLabel;
        private System.Windows.Forms.PictureBox LegendSupportsImage;
        private System.Windows.Forms.Label LegendSupportsLabel;
        private System.Windows.Forms.PictureBox LegendConflictsImage;
        private System.Windows.Forms.Label LegendConflictsLabel;
        private System.Windows.Forms.CheckBox ReverseRelationshipsCheckbox;
    }
}
