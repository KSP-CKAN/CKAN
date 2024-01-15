namespace CKAN.GUI
{
    partial class InstallationHistory
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(InstallationHistory));
            this.ToolTip = new System.Windows.Forms.ToolTip();
            this.Toolbar = new System.Windows.Forms.MenuStrip();
            this.InstallButton = new System.Windows.Forms.ToolStripMenuItem();
            this.Splitter = new System.Windows.Forms.SplitContainer();
            this.HistoryListView = new ThemedListView();
            this.TimestampColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ModsListView = new ThemedListView();
            this.NotInstalledGroup = new System.Windows.Forms.ListViewGroup();
            this.InstalledGroup = new System.Windows.Forms.ListViewGroup();
            this.NameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.VersionColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AuthorColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DescriptionColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SelectInstallMessage = new System.Windows.Forms.ListViewItem();
            this.NoModsMessage = new System.Windows.Forms.ListViewItem();
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.OKButton = new System.Windows.Forms.Button();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // Toolbar
            //
            this.Toolbar.AutoSize = false;
            this.Toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.Toolbar.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.Toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InstallButton});
            this.Toolbar.CanOverflow = true;
            this.Toolbar.Location = new System.Drawing.Point(0, 0);
            this.Toolbar.Name = "Toolbar";
            this.Toolbar.ShowItemToolTips = true;
            this.Toolbar.Size = new System.Drawing.Size(5876, 48);
            this.Toolbar.TabStop = true;
            this.Toolbar.TabIndex = 0;
            this.Toolbar.Text = "Toolbar";
            //
            // InstallButton
            //
            //this.InstallButton.Image = global::CKAN.GUI.Properties.Resources.install;
            this.InstallButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.InstallButton.Name = "InstallButton";
            this.InstallButton.Size = new System.Drawing.Size(114, 56);
            this.InstallButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.InstallButton.Click += new System.EventHandler(this.InstallButton_Click);
            resources.ApplyResources(this.InstallButton, "InstallButton");
            //
            // Splitter
            //
            this.Splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.Splitter.IsSplitterFixed = true;
            this.Splitter.Location = new System.Drawing.Point(5, 70);
            this.Splitter.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.Splitter.Name = "Splitter";
            this.Splitter.Size = new System.Drawing.Size(490, 385);
            this.Splitter.SplitterDistance = 100;
            this.Splitter.SplitterWidth = 10;
            this.Splitter.TabIndex = 1;
            //
            // Splitter.Panel1
            //
            this.Splitter.Panel1.Controls.Add(this.HistoryListView);
            this.Splitter.Panel1MinSize = 100;
            //
            // Splitter.Panel2
            //
            this.Splitter.Panel2.Controls.Add(this.ModsListView);
            this.Splitter.Panel2MinSize = 400;
            //
            // HistoryListView
            //
            this.HistoryListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TimestampColumn});
            this.HistoryListView.CheckBoxes = false;
            this.HistoryListView.FullRowSelect = true;
            this.HistoryListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.HistoryListView.HideSelection = false;
            this.HistoryListView.Location = new System.Drawing.Point(0, 0);
            this.HistoryListView.MultiSelect = false;
            this.HistoryListView.Name = "HistoryListView";
            this.HistoryListView.Size = new System.Drawing.Size(230, 455);
            this.HistoryListView.TabIndex = 2;
            this.HistoryListView.UseCompatibleStateImageBehavior = false;
            this.HistoryListView.View = System.Windows.Forms.View.Details;
            this.HistoryListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HistoryListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(HistoryListView_ItemSelectionChanged);
            //
            // TimestampColumn
            //
            this.TimestampColumn.Width = -1;
            resources.ApplyResources(this.TimestampColumn, "TimestampColumn");
            //
            // ModsListView
            //
            this.ModsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NameColumn,
            this.VersionColumn,
            this.AuthorColumn,
            this.DescriptionColumn});
            this.ModsListView.CheckBoxes = false;
            this.ModsListView.FullRowSelect = true;
            this.ModsListView.HideSelection = false;
            this.ModsListView.Groups.Add(this.NotInstalledGroup);
            this.ModsListView.Groups.Add(this.InstalledGroup);
            this.ModsListView.Location = new System.Drawing.Point(0, 0);
            this.ModsListView.MultiSelect = false;
            this.ModsListView.Name = "ModsListView";
            this.ModsListView.Size = new System.Drawing.Size(230, 455);
            this.ModsListView.TabIndex = 2;
            this.ModsListView.UseCompatibleStateImageBehavior = false;
            this.ModsListView.View = System.Windows.Forms.View.Details;
            this.ModsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModsListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(ModsListView_ItemSelectionChanged);
            //
            // NotInstalledGroup
            //
            this.NotInstalledGroup.Name = "NotInstalledGroup";
            this.NotInstalledGroup.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            resources.ApplyResources(this.NotInstalledGroup, "NotInstalledGroup");
            //
            // InstalledGroup
            //
            this.InstalledGroup.Name = "InstalledGroup";
            this.InstalledGroup.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            resources.ApplyResources(this.InstalledGroup, "InstalledGroup");
            //
            // NameColumn
            //
            this.NameColumn.Width = -1;
            resources.ApplyResources(this.NameColumn, "NameColumn");
            //
            // VersionColumn
            //
            this.VersionColumn.Width = 70;
            resources.ApplyResources(this.VersionColumn, "VersionColumn");
            //
            // AuthorColumn
            //
            this.AuthorColumn.Width = 120;
            resources.ApplyResources(this.AuthorColumn, "AuthorColumn");
            //
            // DescriptionColumn
            //
            this.DescriptionColumn.Width = -1;
            resources.ApplyResources(this.DescriptionColumn, "DescriptionColumn");
            //
            // SelectInstallMessage
            //
            resources.ApplyResources(this.SelectInstallMessage, "SelectInstallMessage");
            //
            // NoModsMessage
            //
            resources.ApplyResources(this.NoModsMessage, "NoModsMessage");
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
            // InstallationHistory
            //
            this.Controls.Add(this.Splitter);
            this.Controls.Add(this.Toolbar);
            this.Controls.Add(this.BottomButtonPanel);
            this.Name = "InstallationHistory";
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
        private System.Windows.Forms.ToolStripMenuItem InstallButton;
        private System.Windows.Forms.SplitContainer Splitter;
        private System.Windows.Forms.ListView HistoryListView;
        private System.Windows.Forms.ColumnHeader TimestampColumn;
        private System.Windows.Forms.ListView ModsListView;
        private System.Windows.Forms.ListViewGroup NotInstalledGroup;
        private System.Windows.Forms.ListViewGroup InstalledGroup;
        private System.Windows.Forms.ColumnHeader NameColumn;
        private System.Windows.Forms.ColumnHeader VersionColumn;
        private System.Windows.Forms.ColumnHeader AuthorColumn;
        private System.Windows.Forms.ColumnHeader DescriptionColumn;
        private System.Windows.Forms.ListViewItem SelectInstallMessage;
        private System.Windows.Forms.ListViewItem NoModsMessage;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Button OKButton;
    }
}
