namespace CKAN.GUI
{
    partial class UnmanagedFiles
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(UnmanagedFiles));
            this.ToolTip = new System.Windows.Forms.ToolTip();
            this.Toolbar = new System.Windows.Forms.MenuStrip();
            this.RefreshButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ExpandCollapseSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.ExpandAllButton = new System.Windows.Forms.ToolStripMenuItem();
            this.CollapseAllButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ResetCollapseButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowDeleteSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.ShowInFolderButton = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteButton = new System.Windows.Forms.ToolStripMenuItem();
            this.GameFolderTree = new System.Windows.Forms.TreeView();
            this.OKButton = new System.Windows.Forms.Button();
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // Toolbar
            //
            this.Toolbar.AutoSize = false;
            this.Toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.Toolbar.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.Toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RefreshButton,
            this.ExpandCollapseSeparator,
            this.ExpandAllButton,
            this.CollapseAllButton,
            this.ResetCollapseButton,
            this.ShowDeleteSeparator,
            this.ShowInFolderButton,
            this.DeleteButton});
            this.Toolbar.CanOverflow = true;
            this.Toolbar.Location = new System.Drawing.Point(0, 0);
            this.Toolbar.Name = "Toolbar";
            this.Toolbar.ShowItemToolTips = true;
            this.Toolbar.Size = new System.Drawing.Size(5876, 48);
            this.Toolbar.TabStop = true;
            this.Toolbar.TabIndex = 0;
            this.Toolbar.Text = "Toolbar";
            //
            // RefreshButton
            //
            this.RefreshButton.Image = global::CKAN.GUI.EmbeddedImages.refresh;
            this.RefreshButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit;
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(114, 56);
            this.RefreshButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            resources.ApplyResources(this.RefreshButton, "RefreshButton");
            //
            // ExpandAllButton
            //
            this.ExpandAllButton.Image = global::CKAN.GUI.EmbeddedImages.expandAll;
            this.ExpandAllButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit;
            this.ExpandAllButton.Name = "ExpandAllButton";
            this.ExpandAllButton.Size = new System.Drawing.Size(114, 56);
            this.ExpandAllButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.ExpandAllButton.Click += new System.EventHandler(this.ExpandAllButton_Click);
            resources.ApplyResources(this.ExpandAllButton, "ExpandAllButton");
            //
            // CollapseAllButton
            //
            this.CollapseAllButton.Image = global::CKAN.GUI.EmbeddedImages.collapseAll;
            this.CollapseAllButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit;
            this.CollapseAllButton.Name = "CollapseAllButton";
            this.CollapseAllButton.Size = new System.Drawing.Size(114, 56);
            this.CollapseAllButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.CollapseAllButton.Click += new System.EventHandler(this.CollapseAllButton_Click);
            resources.ApplyResources(this.CollapseAllButton, "CollapseAllButton");
            //
            // ResetCollapseButton
            //
            this.ResetCollapseButton.Image = global::CKAN.GUI.EmbeddedImages.resetCollapse;
            this.ResetCollapseButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit;
            this.ResetCollapseButton.Name = "ResetCollapseButton";
            this.ResetCollapseButton.Size = new System.Drawing.Size(114, 56);
            this.ResetCollapseButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.ResetCollapseButton.Click += new System.EventHandler(this.ResetCollapseButton_Click);
            resources.ApplyResources(this.ResetCollapseButton, "ResetCollapseButton");
            //
            // ShowInFolderButton
            //
            this.ShowInFolderButton.Image = global::CKAN.GUI.EmbeddedImages.search;
            this.ShowInFolderButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit;
            this.ShowInFolderButton.Name = "ShowInFolderButton";
            this.ShowInFolderButton.Size = new System.Drawing.Size(114, 56);
            this.ShowInFolderButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.ShowInFolderButton.Click += new System.EventHandler(this.ShowInFolderButton_Click);
            resources.ApplyResources(this.ShowInFolderButton, "ShowInFolderButton");
            //
            // DeleteButton
            //
            this.DeleteButton.Image = global::CKAN.GUI.EmbeddedImages.delete;
            this.DeleteButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit;
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(114, 56);
            this.DeleteButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            resources.ApplyResources(this.DeleteButton, "DeleteButton");
            //
            // GameFolderTree
            //
            this.GameFolderTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GameFolderTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GameFolderTree.ImageList = new System.Windows.Forms.ImageList(this.components)
            {
                // ImageList's default makes icons look like garbage
                ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
            };
            this.GameFolderTree.ImageList.Images.Add("folder", global::CKAN.GUI.EmbeddedImages.folder);
            this.GameFolderTree.ImageList.Images.Add("file", global::CKAN.GUI.EmbeddedImages.file);
            this.GameFolderTree.ShowPlusMinus = true;
            this.GameFolderTree.ShowRootLines = false;
            this.GameFolderTree.Location = new System.Drawing.Point(3, 3);
            this.GameFolderTree.Name = "GameFolderTree";
            this.GameFolderTree.Size = new System.Drawing.Size(494, 494);
            this.GameFolderTree.TabIndex = 1;
            this.GameFolderTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.GameFolderTree_NodeMouseDoubleClick);
            //
            // BottomButtonPanel
            //
            this.BottomButtonPanel.RightControls.Add(this.OKButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            //
            // OKButton
            //
            this.OKButton.AutoSize = true;
            this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(112, 30);
            this.OKButton.TabIndex = 2;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            resources.ApplyResources(this.OKButton, "OKButton");
            //
            // UnmanagedFiles
            //
            this.Controls.Add(this.GameFolderTree);
            this.Controls.Add(this.Toolbar);
            this.Controls.Add(this.BottomButtonPanel);
            this.Name = "UnmanagedFiles";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.MenuStrip Toolbar;
        private System.Windows.Forms.ToolStripMenuItem RefreshButton;
        private System.Windows.Forms.ToolStripSeparator ExpandCollapseSeparator;
        private System.Windows.Forms.ToolStripMenuItem ExpandAllButton;
        private System.Windows.Forms.ToolStripMenuItem CollapseAllButton;
        private System.Windows.Forms.ToolStripMenuItem ResetCollapseButton;
        private System.Windows.Forms.ToolStripSeparator ShowDeleteSeparator;
        private System.Windows.Forms.ToolStripMenuItem ShowInFolderButton;
        private System.Windows.Forms.ToolStripMenuItem DeleteButton;
        private System.Windows.Forms.TreeView GameFolderTree;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Button OKButton;
    }
}
