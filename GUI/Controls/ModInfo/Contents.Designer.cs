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
            this.ButtonsTable = new System.Windows.Forms.TableLayoutPanel();
            this.NotCachedLabel = new System.Windows.Forms.Label();
            this.ContentsPreviewTree = new System.Windows.Forms.TreeView();
            this.ContentsDownloadButton = new System.Windows.Forms.Button();
            this.ContentsOpenButton = new System.Windows.Forms.Button();
            this.ButtonsTable.SuspendLayout();
            this.SuspendLayout();
            //
            // ButtonsTable
            //
            this.ButtonsTable.AutoSize = true;
            this.ButtonsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonsTable.ColumnCount = 2;
            this.ButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.ButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.ButtonsTable.Controls.Add(this.ContentsDownloadButton, 0, 0);
            this.ButtonsTable.Controls.Add(this.ContentsOpenButton, 1, 0);
            this.ButtonsTable.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonsTable.Location = new System.Drawing.Point(0, 10);
            this.ButtonsTable.Name = "ButtonsTable";
            this.ButtonsTable.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.ButtonsTable.RowCount = 1;
            this.ButtonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.ButtonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.ButtonsTable.Size = new System.Drawing.Size(346, 255);
            this.ButtonsTable.TabIndex = 0;
            //
            // NotCachedLabel
            //
            this.NotCachedLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.NotCachedLabel.Location = new System.Drawing.Point(3, 3);
            this.NotCachedLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.NotCachedLabel.Name = "NotCachedLabel";
            this.NotCachedLabel.Size = new System.Drawing.Size(494, 30);
            this.NotCachedLabel.TabIndex = 0;
            resources.ApplyResources(this.NotCachedLabel, "NotCachedLabel");
            //
            // ContentsDownloadButton
            //
            this.ContentsDownloadButton.AutoSize = true;
            this.ContentsDownloadButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
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
            this.ContentsOpenButton.AutoSize = true;
            this.ContentsOpenButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
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
            this.ContentsPreviewTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ContentsPreviewTree.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.ContentsPreviewTree.ShowNodeToolTips = true;
            this.ContentsPreviewTree.Location = new System.Drawing.Point(3, 65);
            this.ContentsPreviewTree.Name = "ContentsPreviewTree";
            this.ContentsPreviewTree.Size = new System.Drawing.Size(494, 431);
            this.ContentsPreviewTree.TabIndex = 2;
            this.ContentsPreviewTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ContentsPreviewTree_NodeMouseDoubleClick);
            this.ContentsPreviewTree.MouseDown += this.ContentsPreviewTree_MouseDown;
            this.ContentsPreviewTree.BeforeExpand += this.ContentsPreviewTree_BeforeExpandCollapse;
            this.ContentsPreviewTree.BeforeCollapse += this.ContentsPreviewTree_BeforeExpandCollapse;
            //
            // Contents
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ContentsPreviewTree);
            this.Controls.Add(this.ButtonsTable);
            this.Controls.Add(this.NotCachedLabel);
            this.Name = "Contents";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.ButtonsTable.ResumeLayout(false);
            this.ButtonsTable.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label NotCachedLabel;
        private System.Windows.Forms.TableLayoutPanel ButtonsTable;
        private System.Windows.Forms.Button ContentsDownloadButton;
        private System.Windows.Forms.Button ContentsOpenButton;
        private System.Windows.Forms.TreeView ContentsPreviewTree;
    }
}
