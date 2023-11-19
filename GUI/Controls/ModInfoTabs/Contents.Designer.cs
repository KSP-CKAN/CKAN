namespace CKAN.GUI
{
    partial class Contents
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(Contents));
            this.NotCachedLabel = new System.Windows.Forms.Label();
            this.ContentsPreviewTree = new System.Windows.Forms.TreeView();
            this.ContentsDownloadButton = new System.Windows.Forms.Button();
            this.ContentsOpenButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // NotCachedLabel
            //
            this.NotCachedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NotCachedLabel.Location = new System.Drawing.Point(3, 3);
            this.NotCachedLabel.Name = "NotCachedLabel";
            this.NotCachedLabel.Size = new System.Drawing.Size(494, 30);
            this.NotCachedLabel.TabIndex = 0;
            resources.ApplyResources(this.NotCachedLabel, "NotCachedLabel");
            //
            // ContentsDownloadButton
            //
            this.ContentsDownloadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ContentsDownloadButton.Location = new System.Drawing.Point(6, 36);
            this.ContentsDownloadButton.Name = "ContentsDownloadButton";
            this.ContentsDownloadButton.Size = new System.Drawing.Size(103, 23);
            this.ContentsDownloadButton.TabIndex = 1;
            this.ContentsDownloadButton.UseVisualStyleBackColor = true;
            this.ContentsDownloadButton.Click += new System.EventHandler(this.ContentsDownloadButton_Click);
            resources.ApplyResources(this.ContentsDownloadButton, "ContentsDownloadButton");
            //
            // ContentsOpenButton
            //
            this.ContentsOpenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ContentsOpenButton.Location = new System.Drawing.Point(115, 36);
            this.ContentsOpenButton.Name = "ContentsOpenButton";
            this.ContentsOpenButton.Size = new System.Drawing.Size(103, 23);
            this.ContentsOpenButton.TabIndex = 1;
            this.ContentsOpenButton.UseVisualStyleBackColor = true;
            this.ContentsOpenButton.Click += new System.EventHandler(this.ContentsOpenButton_Click);
            resources.ApplyResources(this.ContentsOpenButton, "ContentsOpenButton");
            //
            // ContentsPreviewTree
            //
            this.ContentsPreviewTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ContentsPreviewTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ContentsPreviewTree.ImageList = new System.Windows.Forms.ImageList(this.components)
            {
                // ImageList's default makes icons look like garbage
                ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
            };
            this.ContentsPreviewTree.ImageList.Images.Add("folderZip", global::CKAN.GUI.EmbeddedImages.folderZip);
            this.ContentsPreviewTree.ImageList.Images.Add("folder", global::CKAN.GUI.EmbeddedImages.folder);
            this.ContentsPreviewTree.ImageList.Images.Add("file", global::CKAN.GUI.EmbeddedImages.file);
            this.ContentsPreviewTree.ShowPlusMinus = true;
            this.ContentsPreviewTree.ShowRootLines = false;
            this.ContentsPreviewTree.Location = new System.Drawing.Point(3, 65);
            this.ContentsPreviewTree.Name = "ContentsPreviewTree";
            this.ContentsPreviewTree.Size = new System.Drawing.Size(494, 431);
            this.ContentsPreviewTree.TabIndex = 2;
            this.ContentsPreviewTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ContentsPreviewTree_NodeMouseDoubleClick);
            //
            // Contents
            //
            this.Controls.Add(this.ContentsPreviewTree);
            this.Controls.Add(this.ContentsDownloadButton);
            this.Controls.Add(this.ContentsOpenButton);
            this.Controls.Add(this.NotCachedLabel);
            this.Name = "Contents";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label NotCachedLabel;
        private System.Windows.Forms.Button ContentsDownloadButton;
        private System.Windows.Forms.Button ContentsOpenButton;
        private System.Windows.Forms.TreeView ContentsPreviewTree;
    }
}
