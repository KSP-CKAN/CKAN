using System;
using System.Windows.Forms;

namespace CKAN
{
    partial class Main
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ModList = new System.Windows.Forms.DataGridView();
            this.ModInfo = new System.Windows.Forms.TextBox();
            this.ModFilter = new System.Windows.Forms.ListBox();
            this.FilterByNameTextBox = new System.Windows.Forms.TextBox();
            this.FilterByNameLabel = new System.Windows.Forms.Label();
            this.FilterByNamePanel = new System.Windows.Forms.Panel();
            this.Installed = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Update = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ModName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Author = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InstalledVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LatestVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Homepage = new System.Windows.Forms.DataGridViewLinkColumn();
            this.RefreshToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.UpdateAllToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.ApplyToolButton = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ModList)).BeginInit();
            this.FilterByNamePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(869, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExitToolButton});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // ExitToolButton
            // 
            this.ExitToolButton.Name = "ExitToolButton";
            this.ExitToolButton.Size = new System.Drawing.Size(92, 22);
            this.ExitToolButton.Text = "Exit";
            this.ExitToolButton.Click += new System.EventHandler(this.ExitToolButton_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 634);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(869, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // menuStrip2
            // 
            this.menuStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RefreshToolButton,
            this.UpdateAllToolButton,
            this.ApplyToolButton});
            this.menuStrip2.Location = new System.Drawing.Point(0, 24);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(348, 40);
            this.menuStrip2.TabIndex = 2;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // MainPanel
            // 
            this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainPanel.Controls.Add(this.panel1);
            this.MainPanel.Controls.Add(this.ModInfo);
            this.MainPanel.Controls.Add(this.ModFilter);
            this.MainPanel.Location = new System.Drawing.Point(0, 68);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(869, 563);
            this.MainPanel.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.ModList);
            this.panel1.Location = new System.Drawing.Point(208, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(658, 398);
            this.panel1.TabIndex = 4;
            // 
            // ModList
            // 
            this.ModList.AllowUserToAddRows = false;
            this.ModList.AllowUserToDeleteRows = false;
            this.ModList.AllowUserToResizeRows = false;
            this.ModList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.ModList.BackgroundColor = System.Drawing.SystemColors.Window;
            this.ModList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.ModList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ModList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Installed,
            this.Update,
            this.ModName,
            this.Author,
            this.InstalledVersion,
            this.LatestVersion,
            this.Description,
            this.Homepage});
            this.ModList.Location = new System.Drawing.Point(0, -4);
            this.ModList.MultiSelect = false;
            this.ModList.Name = "ModList";
            this.ModList.RowHeadersVisible = false;
            this.ModList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ModList.Size = new System.Drawing.Size(661, 402);
            this.ModList.TabIndex = 3;
            this.ModList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ModList_CellContentClick);
            this.ModList.SelectionChanged += new System.EventHandler(this.ModList_SelectedIndexChanged);
            // 
            // ModInfo
            // 
            this.ModInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ModInfo.Location = new System.Drawing.Point(208, 408);
            this.ModInfo.Multiline = true;
            this.ModInfo.Name = "ModInfo";
            this.ModInfo.ReadOnly = true;
            this.ModInfo.Size = new System.Drawing.Size(658, 140);
            this.ModInfo.TabIndex = 2;
            this.ModInfo.Text = "Mod description etc";
            // 
            // ModFilter
            // 
            this.ModFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ModFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ModFilter.FormattingEnabled = true;
            this.ModFilter.ItemHeight = 20;
            this.ModFilter.Items.AddRange(new object[] {
            "All",
            "Installed",
            "Installed (update available)",
            "New in repository",
            "Not installed"});
            this.ModFilter.Location = new System.Drawing.Point(4, 4);
            this.ModFilter.Name = "ModFilter";
            this.ModFilter.Size = new System.Drawing.Size(197, 544);
            this.ModFilter.TabIndex = 0;
            this.ModFilter.SelectedIndexChanged += new System.EventHandler(this.ModFilter_SelectedIndexChanged);
            // 
            // FilterByNameTextBox
            // 
            this.FilterByNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterByNameTextBox.Location = new System.Drawing.Point(86, 18);
            this.FilterByNameTextBox.Name = "FilterByNameTextBox";
            this.FilterByNameTextBox.Size = new System.Drawing.Size(124, 20);
            this.FilterByNameTextBox.TabIndex = 4;
            this.FilterByNameTextBox.TextChanged += new System.EventHandler(this.FilterByNameTextBox_TextChanged);
            // 
            // FilterByNameLabel
            // 
            this.FilterByNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterByNameLabel.AutoSize = true;
            this.FilterByNameLabel.Location = new System.Drawing.Point(8, 21);
            this.FilterByNameLabel.Name = "FilterByNameLabel";
            this.FilterByNameLabel.Size = new System.Drawing.Size(75, 13);
            this.FilterByNameLabel.TabIndex = 5;
            this.FilterByNameLabel.Text = "Filter by name:";
            // 
            // FilterByNamePanel
            // 
            this.FilterByNamePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FilterByNamePanel.Controls.Add(this.FilterByNameLabel);
            this.FilterByNamePanel.Controls.Add(this.FilterByNameTextBox);
            this.FilterByNamePanel.Location = new System.Drawing.Point(655, 27);
            this.FilterByNamePanel.Name = "FilterByNamePanel";
            this.FilterByNamePanel.Size = new System.Drawing.Size(214, 39);
            this.FilterByNamePanel.TabIndex = 6;
            // 
            // Installed
            // 
            this.Installed.Frozen = true;
            this.Installed.HeaderText = "Installed";
            this.Installed.Name = "Installed";
            this.Installed.Width = 52;
            // 
            // Update
            // 
            this.Update.Frozen = true;
            this.Update.HeaderText = "Update";
            this.Update.Name = "Update";
            this.Update.Width = 48;
            // 
            // ModName
            // 
            this.ModName.Frozen = true;
            this.ModName.HeaderText = "Name";
            this.ModName.Name = "ModName";
            this.ModName.ReadOnly = true;
            this.ModName.Width = 60;
            // 
            // Author
            // 
            this.Author.Frozen = true;
            this.Author.HeaderText = "Author";
            this.Author.Name = "Author";
            this.Author.ReadOnly = true;
            this.Author.Width = 63;
            // 
            // InstalledVersion
            // 
            this.InstalledVersion.Frozen = true;
            this.InstalledVersion.HeaderText = "Installed version";
            this.InstalledVersion.Name = "InstalledVersion";
            this.InstalledVersion.ReadOnly = true;
            this.InstalledVersion.Width = 99;
            // 
            // LatestVersion
            // 
            this.LatestVersion.Frozen = true;
            this.LatestVersion.HeaderText = "Latest version";
            this.LatestVersion.Name = "LatestVersion";
            this.LatestVersion.ReadOnly = true;
            this.LatestVersion.Width = 90;
            // 
            // Description
            // 
            this.Description.Frozen = true;
            this.Description.HeaderText = "Description";
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.Width = 85;
            // 
            // Homepage
            // 
            this.Homepage.Frozen = true;
            this.Homepage.HeaderText = "Homepage";
            this.Homepage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.Homepage.Name = "Homepage";
            this.Homepage.ReadOnly = true;
            this.Homepage.Width = 65;
            // 
            // RefreshToolButton
            // 
            this.RefreshToolButton.Image = global::CKAN.Properties.Resources.refresh;
            this.RefreshToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.RefreshToolButton.Name = "RefreshToolButton";
            this.RefreshToolButton.Size = new System.Drawing.Size(89, 36);
            this.RefreshToolButton.Text = "Refresh";
            this.RefreshToolButton.Click += new System.EventHandler(this.RefreshToolButton_Click);
            // 
            // UpdateAllToolButton
            // 
            this.UpdateAllToolButton.Image = global::CKAN.Properties.Resources.update;
            this.UpdateAllToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.UpdateAllToolButton.Name = "UpdateAllToolButton";
            this.UpdateAllToolButton.Size = new System.Drawing.Size(130, 36);
            this.UpdateAllToolButton.Text = "Mark all updated";
            this.UpdateAllToolButton.Click += new System.EventHandler(this.MarkAllUpdatesToolButton_Click);
            // 
            // ApplyToolButton
            // 
            this.ApplyToolButton.Image = global::CKAN.Properties.Resources.apply;
            this.ApplyToolButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ApplyToolButton.Name = "ApplyToolButton";
            this.ApplyToolButton.Size = new System.Drawing.Size(121, 36);
            this.ApplyToolButton.Text = "Apply changes";
            this.ApplyToolButton.Click += new System.EventHandler(this.ApplyToolButton_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 656);
            this.Controls.Add(this.FilterByNamePanel);
            this.Controls.Add(this.MainPanel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.menuStrip2);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.Text = "CKAN-GUI v0.1";
            this.Load += new System.EventHandler(this.Main_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ModList)).EndInit();
            this.FilterByNamePanel.ResumeLayout(false);
            this.FilterByNamePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExitToolButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem RefreshToolButton;
        private System.Windows.Forms.ToolStripMenuItem UpdateAllToolButton;
        private System.Windows.Forms.ToolStripMenuItem ApplyToolButton;
        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.ListBox ModFilter;
        private System.Windows.Forms.TextBox ModInfo;
        private System.Windows.Forms.TextBox FilterByNameTextBox;
        private System.Windows.Forms.Label FilterByNameLabel;
        private System.Windows.Forms.DataGridView ModList;
        private Panel panel1;
        private Panel FilterByNamePanel;
        private DataGridViewCheckBoxColumn Installed;
        private DataGridViewCheckBoxColumn Update;
        private DataGridViewTextBoxColumn ModName;
        private DataGridViewTextBoxColumn Author;
        private DataGridViewTextBoxColumn InstalledVersion;
        private DataGridViewTextBoxColumn LatestVersion;
        private DataGridViewTextBoxColumn Description;
        private DataGridViewLinkColumn Homepage;
        private ToolStripMenuItem aboutToolStripMenuItem;
    }
}

