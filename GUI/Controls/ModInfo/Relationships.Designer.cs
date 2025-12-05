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
            this.LegendTable = new System.Windows.Forms.TableLayoutPanel();
            this.LegendInstalledLabel = new System.Windows.Forms.Label();
            this.LegendIncompatibleLabel = new System.Windows.Forms.Label();
            this.LegendVirtualLabel = new System.Windows.Forms.Label();
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
            this.LegendTable.SuspendLayout();
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
            this.DependsGraphTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DependsGraphTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DependsGraphTree.Location = new System.Drawing.Point(3, 132);
            this.DependsGraphTree.Name = "DependsGraphTree";
            this.DependsGraphTree.Size = new System.Drawing.Size(494, 340);
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
            // LegendTable
            //
            this.LegendTable.AutoSize = true;
            this.LegendTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.LegendTable.ColumnCount = 6;
            this.LegendTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LegendTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33f));
            this.LegendTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LegendTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33f));
            this.LegendTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LegendTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33f));
            this.LegendTable.Controls.Add(this.LegendInstalledLabel, 1, 0);
            this.LegendTable.Controls.Add(this.LegendIncompatibleLabel, 3, 0);
            this.LegendTable.Controls.Add(this.LegendVirtualLabel, 5, 0);
            this.LegendTable.Controls.Add(this.LegendProvidesImage, 0, 1);
            this.LegendTable.Controls.Add(this.LegendProvidesLabel, 1, 1);
            this.LegendTable.Controls.Add(this.LegendDependsImage, 0, 2);
            this.LegendTable.Controls.Add(this.LegendDependsLabel, 1, 2);
            this.LegendTable.Controls.Add(this.LegendRecommendsImage, 2, 1);
            this.LegendTable.Controls.Add(this.LegendRecommendsLabel, 3, 1);
            this.LegendTable.Controls.Add(this.LegendSuggestsImage, 2, 2);
            this.LegendTable.Controls.Add(this.LegendSuggestsLabel, 3, 2);
            this.LegendTable.Controls.Add(this.LegendSupportsImage, 4, 1);
            this.LegendTable.Controls.Add(this.LegendSupportsLabel, 5, 1);
            this.LegendTable.Controls.Add(this.LegendConflictsImage, 4, 2);
            this.LegendTable.Controls.Add(this.LegendConflictsLabel, 5, 2);
            this.LegendTable.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LegendTable.Location = new System.Drawing.Point(0, 0);
            this.LegendTable.Name = "LegendTable";
            this.LegendTable.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.LegendTable.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.LegendTable.RowCount = 3;
            this.LegendTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LegendTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LegendTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.LegendTable.Size = new System.Drawing.Size(346, 255);
            this.LegendTable.TabIndex = 0;
            //
            // LegendInstalledLabel
            //
            this.LegendInstalledLabel.AutoSize = true;
            this.LegendInstalledLabel.Font = Styling.Fonts.Bold;
            this.LegendInstalledLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            resources.ApplyResources(this.LegendInstalledLabel, "LegendInstalledLabel");
            //
            // LegendIncompatibleLabel
            //
            this.LegendIncompatibleLabel.AutoSize = true;
            this.LegendIncompatibleLabel.ForeColor = System.Drawing.Color.Red;
            this.LegendIncompatibleLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            resources.ApplyResources(this.LegendIncompatibleLabel, "LegendIncompatibleLabel");
            //
            // LegendVirtualLabel
            //
            this.LegendVirtualLabel.AutoSize = true;
            this.LegendVirtualLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LegendVirtualLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            resources.ApplyResources(this.LegendVirtualLabel, "LegendVirtualLabel");
            //
            // LegendProvidesImage
            //
            this.LegendProvidesImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendProvidesImage.Image = global::CKAN.GUI.EmbeddedImages.ballot;
            this.LegendProvidesImage.Margin = new System.Windows.Forms.Padding(0);
            this.LegendProvidesImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendProvidesImage.ClientSize = new System.Drawing.Size(16, 16);
            //
            // LegendProvidesLabel
            //
            this.LegendProvidesLabel.AutoSize = true;
            this.LegendProvidesLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            resources.ApplyResources(this.LegendProvidesLabel, "LegendProvidesLabel");
            //
            // LegendDependsImage
            //
            this.LegendDependsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendDependsImage.Image = global::CKAN.GUI.EmbeddedImages.star;
            this.LegendDependsImage.Margin = new System.Windows.Forms.Padding(0);
            this.LegendDependsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendDependsImage.ClientSize = new System.Drawing.Size(16, 16);
            //
            // LegendDependsLabel
            //
            this.LegendDependsLabel.AutoSize = true;
            this.LegendDependsLabel.Location = new System.Drawing.Point(24, 39);
            this.LegendDependsLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            resources.ApplyResources(this.LegendDependsLabel, "LegendDependsLabel");
            //
            // LegendRecommendsImage
            //
            this.LegendRecommendsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendRecommendsImage.Image = global::CKAN.GUI.EmbeddedImages.thumbup;
            this.LegendRecommendsImage.Margin = new System.Windows.Forms.Padding(0);
            this.LegendRecommendsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendRecommendsImage.ClientSize = new System.Drawing.Size(16, 16);
            //
            // LegendRecommendsLabel
            //
            this.LegendRecommendsLabel.AutoSize = true;
            this.LegendRecommendsLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            resources.ApplyResources(this.LegendRecommendsLabel, "LegendRecommendsLabel");
            //
            // LegendSuggestsImage
            //
            this.LegendSuggestsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendSuggestsImage.Image = global::CKAN.GUI.EmbeddedImages.info;
            this.LegendSuggestsImage.Margin = new System.Windows.Forms.Padding(0);
            this.LegendSuggestsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendSuggestsImage.ClientSize = new System.Drawing.Size(16, 16);
            //
            // LegendSuggestsLabel
            //
            this.LegendSuggestsLabel.AutoSize = true;
            this.LegendSuggestsLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            resources.ApplyResources(this.LegendSuggestsLabel, "LegendSuggestsLabel");
            //
            // LegendSupportsImage
            //
            this.LegendSupportsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendSupportsImage.Image = global::CKAN.GUI.EmbeddedImages.smile;
            this.LegendSupportsImage.Margin = new System.Windows.Forms.Padding(0);
            this.LegendSupportsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendSupportsImage.ClientSize = new System.Drawing.Size(16, 16);
            //
            // LegendSupportsLabel
            //
            this.LegendSupportsLabel.AutoSize = true;
            this.LegendSupportsLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            resources.ApplyResources(this.LegendSupportsLabel, "LegendSupportsLabel");
            //
            // LegendConflictsImage
            //
            this.LegendConflictsImage.BackColor = System.Drawing.SystemColors.Window;
            this.LegendConflictsImage.Image = global::CKAN.GUI.EmbeddedImages.alert;
            this.LegendConflictsImage.Margin = new System.Windows.Forms.Padding(0);
            this.LegendConflictsImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LegendConflictsImage.ClientSize = new System.Drawing.Size(16, 16);
            //
            // LegendConflictsLabel
            //
            this.LegendConflictsLabel.AutoSize = true;
            this.LegendConflictsLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            resources.ApplyResources(this.LegendConflictsLabel, "LegendConflictsLabel");
            //
            // ReverseRelationshipsCheckbox
            //
            this.ReverseRelationshipsCheckbox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ReverseRelationshipsCheckbox.AutoSize = true;
            this.ReverseRelationshipsCheckbox.AutoCheck = false;
            this.ReverseRelationshipsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReverseRelationshipsCheckbox.Name = "ReverseRelationshipsCheckbox";
            this.ReverseRelationshipsCheckbox.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.ReverseRelationshipsCheckbox.Size = new System.Drawing.Size(494, 24);
            this.ReverseRelationshipsCheckbox.Click += new System.EventHandler(this.ReverseRelationshipsCheckbox_Click);
            this.ReverseRelationshipsCheckbox.CheckedChanged += new System.EventHandler(this.ReverseRelationshipsCheckbox_CheckedChanged);
            resources.ApplyResources(this.ReverseRelationshipsCheckbox, "ReverseRelationshipsCheckbox");
            //
            // Relationships
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DependsGraphTree);
            this.Controls.Add(this.LegendTable);
            this.Controls.Add(this.ReverseRelationshipsCheckbox);
            this.Name = "Relationships";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.LegendTable.ResumeLayout(false);
            this.LegendTable.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.TreeView DependsGraphTree;
        private System.Windows.Forms.TableLayoutPanel LegendTable;
        private System.Windows.Forms.Label LegendInstalledLabel;
        private System.Windows.Forms.Label LegendIncompatibleLabel;
        private System.Windows.Forms.Label LegendVirtualLabel;
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
