namespace CKAN.GUI
{
    partial class ManageGameInstancesDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(ManageGameInstancesDialog));
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "dsadsa",
            "",
            ""}, -1);
            this.GameInstancesListView = new ThemedListView();
            this.GameInstallName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Game = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.GameInstallVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.GamePlayTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.GameInstallPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SelectButton = new System.Windows.Forms.Button();
            this.AddNewButton = new CKAN.GUI.DropdownMenuButton();
            this.AddNewMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.InstanceListContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openDirectoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddToCKANMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportFromSteamMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CloneGameInstanceMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RenameButton = new System.Windows.Forms.Button();
            this.SetAsDefaultCheckbox = new System.Windows.Forms.CheckBox();
            this.ForgetButton = new System.Windows.Forms.Button();
            this.AddNewMenu.SuspendLayout();
            this.SuspendLayout();
            //
            // GameInstancesListView
            //
            this.GameInstancesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GameInstancesListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GameInstancesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.GameInstallName,
            this.Game,
            this.GameInstallVersion,
            this.GamePlayTime,
            this.GameInstallPath});
            this.GameInstancesListView.FullRowSelect = true;
            this.GameInstancesListView.HideSelection = false;
            this.GameInstancesListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.GameInstancesListView.Location = new System.Drawing.Point(12, 13);
            this.GameInstancesListView.MultiSelect = false;
            this.GameInstancesListView.Name = "GameInstancesListView";
            this.GameInstancesListView.Size = new System.Drawing.Size(522, 301);
            this.GameInstancesListView.TabIndex = 0;
            this.GameInstancesListView.UseCompatibleStateImageBehavior = false;
            this.GameInstancesListView.View = System.Windows.Forms.View.Details;
            this.GameInstancesListView.SelectedIndexChanged += new System.EventHandler(this.GameInstancesListView_SelectedIndexChanged);
            this.GameInstancesListView.DoubleClick += new System.EventHandler(this.GameInstancesListView_DoubleClick);
            this.GameInstancesListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GameInstancesListView_Click);
            this.GameInstancesListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GameInstancesListView_KeyDown);
            //
            // InstanceListContextMenuStrip
            //
            this.InstanceListContextMenuStrip.Items.Add(this.openDirectoryMenuItem);
            this.InstanceListContextMenuStrip.Name = "InstanceListContextMenuStrip";
            this.InstanceListContextMenuStrip.Size = new System.Drawing.Size(180, 30);
            //
            // openDirectoryToolStripMenuItem
            //
            this.openDirectoryMenuItem.Name = "openDirectoryMenuItem";
            this.openDirectoryMenuItem.Size = new System.Drawing.Size(180, 30);
            this.openDirectoryMenuItem.Click += new System.EventHandler(this.OpenDirectoryMenuItem_Click);
            resources.ApplyResources(this.openDirectoryMenuItem, "openDirectoryMenuItem");
            //
            // GameInstallName
            //
            this.GameInstallName.Width = 130;
            resources.ApplyResources(this.GameInstallName, "GameInstallName");
            //
            // Game
            //
            this.Game.Width = 70;
            resources.ApplyResources(this.Game, "Game");
            //
            // GameInstallVersion
            //
            this.GameInstallVersion.Width = 70;
            resources.ApplyResources(this.GameInstallVersion, "GameInstallVersion");
            //
            // GamePlayTime
            //
            this.GamePlayTime.Width = 120;
            this.GamePlayTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            resources.ApplyResources(this.GamePlayTime, "GamePlayTime");
            //
            // GameInstallPath
            //
            this.GameInstallPath.Width = 370;
            resources.ApplyResources(this.GameInstallPath, "GameInstallPath");
            //
            // SelectButton
            //
            this.SelectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectButton.Enabled = false;
            this.SelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SelectButton.Location = new System.Drawing.Point(459, 320);
            this.SelectButton.Name = "SelectButton";
            this.SelectButton.Size = new System.Drawing.Size(75, 23);
            this.SelectButton.TabIndex = 1;
            this.SelectButton.UseVisualStyleBackColor = true;
            this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
            resources.ApplyResources(this.SelectButton, "SelectButton");
            //
            // AddNewButton
            //
            this.AddNewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AddNewButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AddNewButton.Location = new System.Drawing.Point(327, 320);
            this.AddNewButton.Menu = this.AddNewMenu;
            this.AddNewButton.Name = "AddNewButton";
            this.AddNewButton.Size = new System.Drawing.Size(126, 23);
            this.AddNewButton.TabIndex = 2;
            this.AddNewButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.AddNewButton, "AddNewButton");
            //
            // AddNewMenu
            //
            this.AddNewMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddToCKANMenuItem,
            this.ImportFromSteamMenuItem,
            this.CloneGameInstanceMenuItem});
            this.AddNewMenu.Name = "AddNewMenu";
            this.AddNewMenu.Size = new System.Drawing.Size(222, 48);
            //
            // AddToCKANMenuItem
            //
            this.AddToCKANMenuItem.Name = "AddToCKANMenuItem";
            this.AddToCKANMenuItem.Size = new System.Drawing.Size(216, 22);
            this.AddToCKANMenuItem.Click += new System.EventHandler(this.AddToCKANMenuItem_Click);
            resources.ApplyResources(this.AddToCKANMenuItem, "AddToCKANMenuItem");
            //
            // ImportFromSteamMenuItem
            //
            this.ImportFromSteamMenuItem.Name = "ImportFromSteamMenuItem";
            this.ImportFromSteamMenuItem.Size = new System.Drawing.Size(216, 22);
            this.ImportFromSteamMenuItem.Click += new System.EventHandler(this.ImportFromSteamMenuItem_Click);
            resources.ApplyResources(this.ImportFromSteamMenuItem, "ImportFromSteamMenuItem");
            //
            // CloneGameInstanceMenuItem
            //
            this.CloneGameInstanceMenuItem.Name = "CloneGameInstanceMenuItem";
            this.CloneGameInstanceMenuItem.Size = new System.Drawing.Size(216, 22);
            this.CloneGameInstanceMenuItem.Click += new System.EventHandler(this.CloneGameInstanceMenuItem_Click);
            resources.ApplyResources(this.CloneGameInstanceMenuItem, "CloneGameInstanceMenuItem");
            //
            // RenameButton
            //
            this.RenameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RenameButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RenameButton.Location = new System.Drawing.Point(216, 320);
            this.RenameButton.Name = "RenameButton";
            this.RenameButton.Size = new System.Drawing.Size(105, 23);
            this.RenameButton.TabIndex = 3;
            this.RenameButton.UseVisualStyleBackColor = true;
            this.RenameButton.Click += new System.EventHandler(this.RenameButton_Click);
            resources.ApplyResources(this.RenameButton, "RenameButton");
            //
            // SetAsDefaultCheckbox
            //
            this.SetAsDefaultCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SetAsDefaultCheckbox.AutoSize = true;
            this.SetAsDefaultCheckbox.AutoCheck = false;
            this.SetAsDefaultCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SetAsDefaultCheckbox.Location = new System.Drawing.Point(12, 324);
            this.SetAsDefaultCheckbox.Name = "SetAsDefaultCheckbox";
            this.SetAsDefaultCheckbox.Size = new System.Drawing.Size(91, 17);
            this.SetAsDefaultCheckbox.TabIndex = 4;
            this.SetAsDefaultCheckbox.UseVisualStyleBackColor = true;
            this.SetAsDefaultCheckbox.Click += new System.EventHandler(this.SetAsDefaultCheckbox_Click);
            resources.ApplyResources(this.SetAsDefaultCheckbox, "SetAsDefaultCheckbox");
            //
            // ForgetButton
            //
            this.ForgetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ForgetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ForgetButton.Location = new System.Drawing.Point(140, 320);
            this.ForgetButton.Name = "ForgetButton";
            this.ForgetButton.Size = new System.Drawing.Size(70, 23);
            this.ForgetButton.TabIndex = 5;
            this.ForgetButton.UseVisualStyleBackColor = true;
            this.ForgetButton.Click += new System.EventHandler(this.Forget_Click);
            resources.ApplyResources(this.ForgetButton, "ForgetButton");
            //
            // ManageGameInstancesDialog
            //
            this.AcceptButton = this.SelectButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 355);
            this.Controls.Add(this.ForgetButton);
            this.Controls.Add(this.SetAsDefaultCheckbox);
            this.Controls.Add(this.RenameButton);
            this.Controls.Add(this.AddNewButton);
            this.Controls.Add(this.SelectButton);
            this.Controls.Add(this.GameInstancesListView);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.HelpButton = true;
            this.Icon = EmbeddedImages.AppIcon;
            this.MinimumSize = new System.Drawing.Size(560, 200);
            this.Name = "ManageGameInstancesDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            this.InstanceListContextMenuStrip.ResumeLayout(false);
            this.AddNewMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView GameInstancesListView;
        private System.Windows.Forms.ColumnHeader GameInstallName;
        private System.Windows.Forms.ColumnHeader Game;
        private System.Windows.Forms.ColumnHeader GameInstallVersion;
        private System.Windows.Forms.ColumnHeader GamePlayTime;
        private System.Windows.Forms.ColumnHeader GameInstallPath;
        private System.Windows.Forms.Button SelectButton;
        private CKAN.GUI.DropdownMenuButton AddNewButton;
        private System.Windows.Forms.ContextMenuStrip AddNewMenu;
        private System.Windows.Forms.ContextMenuStrip InstanceListContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem openDirectoryMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddToCKANMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ImportFromSteamMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CloneGameInstanceMenuItem;
        private System.Windows.Forms.Button RenameButton;
        private System.Windows.Forms.CheckBox SetAsDefaultCheckbox;
        private System.Windows.Forms.Button ForgetButton;
    }
}
