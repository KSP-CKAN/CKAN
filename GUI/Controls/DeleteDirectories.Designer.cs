namespace CKAN.GUI
{
    partial class DeleteDirectories
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(DeleteDirectories));
            this.ExplanationLabel = new System.Windows.Forms.Label();
            this.Splitter = new System.Windows.Forms.SplitContainer();
            this.DirectoriesListView = new ThemedListView();
            this.DirectoryColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ContentsListView = new ThemedListView();
            this.FileColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SelectDirPrompt = new System.Windows.Forms.ListViewItem();
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.OpenDirectoryButton = new System.Windows.Forms.Button();
            this.KeepAllButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.Splitter)).BeginInit();
            this.Splitter.Panel1.SuspendLayout();
            this.Splitter.Panel2.SuspendLayout();
            this.Splitter.SuspendLayout();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ExplanationLabel
            // 
            this.ExplanationLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ExplanationLabel.Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.Name, 12, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.ExplanationLabel.Location = new System.Drawing.Point(5, 0);
            this.ExplanationLabel.Name = "ExplanationLabel";
            this.ExplanationLabel.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.ExplanationLabel.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.ExplanationLabel.Size = new System.Drawing.Size(490, 70);
            resources.ApplyResources(this.ExplanationLabel, "ExplanationLabel");
            // 
            // Splitter
            // 
            this.Splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Splitter.Location = new System.Drawing.Point(5, 70);
            this.Splitter.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.Splitter.Name = "Splitter";
            this.Splitter.Size = new System.Drawing.Size(490, 385);
            this.Splitter.SplitterDistance = 250;
            this.Splitter.SplitterWidth = 10;
            this.Splitter.TabIndex = 0;
            //
            // Splitter.Panel1
            //
            this.Splitter.Panel1.Controls.Add(this.DirectoriesListView);
            this.Splitter.Panel1MinSize = 200;
            //
            // Splitter.Panel2
            //
            this.Splitter.Panel2.Controls.Add(this.ContentsListView);
            this.Splitter.Panel2MinSize = 200;
            // 
            // DirectoriesListView
            // 
            this.DirectoriesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DirectoryColumn});
            this.DirectoriesListView.CheckBoxes = true;
            this.DirectoriesListView.FullRowSelect = true;
            this.DirectoriesListView.HideSelection = false;
            this.DirectoriesListView.Location = new System.Drawing.Point(0, 0);
            this.DirectoriesListView.Name = "DirectoriesListView";
            this.DirectoriesListView.Size = new System.Drawing.Size(230, 455);
            this.DirectoriesListView.TabIndex = 0;
            this.DirectoriesListView.UseCompatibleStateImageBehavior = false;
            this.DirectoriesListView.View = System.Windows.Forms.View.Details;
            this.DirectoriesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DirectoriesListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(DirectoriesListView_ItemSelectionChanged);
            // 
            // DirectoryColumn
            // 
            this.DirectoryColumn.Width = -1;
            resources.ApplyResources(this.DirectoryColumn, "DirectoryColumn");
            // 
            // ContentsListView
            // 
            this.ContentsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FileColumn});
            this.ContentsListView.FullRowSelect = true;
            this.ContentsListView.Location = new System.Drawing.Point(0, 0);
            this.ContentsListView.Name = "ContentsListView";
            this.ContentsListView.Size = new System.Drawing.Size(230, 455);
            this.ContentsListView.TabIndex = 1;
            this.ContentsListView.UseCompatibleStateImageBehavior = false;
            this.ContentsListView.View = System.Windows.Forms.View.Details;
            this.ContentsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // FileColumn
            // 
            this.FileColumn.Width = -1;
            resources.ApplyResources(this.FileColumn, "FileColumn");
            // 
            // SelectDirPrompt
            // 
            resources.ApplyResources(this.SelectDirPrompt, "SelectDirPrompt");
            // 
            // BottomButtonPanel
            // 
            this.BottomButtonPanel.LeftControls.Add(this.OpenDirectoryButton);
            this.BottomButtonPanel.RightControls.Add(this.DeleteButton);
            this.BottomButtonPanel.RightControls.Add(this.KeepAllButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            // 
            // OpenDirectoryButton
            // 
            this.OpenDirectoryButton.AutoSize = true;
            this.OpenDirectoryButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.OpenDirectoryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OpenDirectoryButton.Name = "OpenDirectoryButton";
            this.OpenDirectoryButton.Size = new System.Drawing.Size(112, 30);
            this.OpenDirectoryButton.TabIndex = 2;
            this.OpenDirectoryButton.UseVisualStyleBackColor = true;
            this.OpenDirectoryButton.Click += new System.EventHandler(this.OpenDirectoryButton_Click);
            resources.ApplyResources(this.OpenDirectoryButton, "OpenDirectoryButton");
            // 
            // KeepAllButton
            // 
            this.KeepAllButton.AutoSize = true;
            this.KeepAllButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.KeepAllButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.KeepAllButton.Name = "KeepAllButton";
            this.KeepAllButton.Size = new System.Drawing.Size(112, 30);
            this.KeepAllButton.TabIndex = 3;
            this.KeepAllButton.UseVisualStyleBackColor = true;
            this.KeepAllButton.Click += new System.EventHandler(this.KeepAllButton_Click);
            resources.ApplyResources(this.KeepAllButton, "KeepAllButton");
            // 
            // DeleteButton
            // 
            this.DeleteButton.AutoSize = true;
            this.DeleteButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.DeleteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteButton.Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont, System.Drawing.FontStyle.Bold);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(112, 30);
            this.DeleteButton.TabIndex = 4;
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            resources.ApplyResources(this.DeleteButton, "DeleteButton");
            // 
            // DeleteDirectories
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.Splitter);
            this.Controls.Add(this.ExplanationLabel);
            this.Controls.Add(this.BottomButtonPanel);
            this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.Name = "DeleteDirectories";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.Splitter.Panel1.ResumeLayout(false);
            this.Splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Splitter)).EndInit();
            this.Splitter.ResumeLayout(false);
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label ExplanationLabel;
        private System.Windows.Forms.SplitContainer Splitter;
        private System.Windows.Forms.ListView DirectoriesListView;
        private System.Windows.Forms.ColumnHeader DirectoryColumn;
        private System.Windows.Forms.ListView ContentsListView;
        private System.Windows.Forms.ColumnHeader FileColumn;
        private System.Windows.Forms.ListViewItem SelectDirPrompt;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Button OpenDirectoryButton;
        private System.Windows.Forms.Button KeepAllButton;
        private System.Windows.Forms.Button DeleteButton;
    }
}
