namespace CKAN.GUI
{
    partial class ChooseRecommendedMods
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(ChooseRecommendedMods));
            this.Toolbar = new System.Windows.Forms.MenuStrip();
            this.UncheckAllButton = new System.Windows.Forms.ToolStripSplitButton();
            this.UncheckAllDropdown = new System.Windows.Forms.ToolStripDropDownMenu();
            this.AlwaysUncheckAllButton = new System.Windows.Forms.ToolStripMenuItem();
            this.UncheckCheckSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.CheckAllButton = new System.Windows.Forms.ToolStripMenuItem();
            this.CheckRecommendationsButton = new System.Windows.Forms.ToolStripMenuItem();
            this.RecommendedDialogLabel = new System.Windows.Forms.Label();
            this.RecommendedModsListView = new ThemedListView();
            this.RecommendationsGroup = new System.Windows.Forms.ListViewGroup();
            this.SuggestionsGroup = new System.Windows.Forms.ListViewGroup();
            this.SupportedByGroup = new System.Windows.Forms.ListViewGroup();
            this.ModNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SourceModulesHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DescriptionHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.RecommendedModsCancelButton = new System.Windows.Forms.Button();
            this.RecommendedModsContinueButton = new System.Windows.Forms.Button();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // Toolbar
            //
            this.Toolbar.AutoSize = false;
            this.Toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.Toolbar.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.Toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UncheckAllButton,
            this.UncheckCheckSeparator,
            this.CheckAllButton,
            this.CheckRecommendationsButton});
            this.Toolbar.CanOverflow = true;
            this.Toolbar.Location = new System.Drawing.Point(0, 0);
            this.Toolbar.Name = "Toolbar";
            this.Toolbar.ShowItemToolTips = true;
            this.Toolbar.Size = new System.Drawing.Size(5876, 48);
            this.Toolbar.TabStop = true;
            this.Toolbar.TabIndex = 0;
            this.Toolbar.Text = "Toolbar";
            //
            // UncheckAllButton
            //
            this.UncheckAllButton.Image = global::CKAN.GUI.EmbeddedImages.uncheckAll;
            this.UncheckAllButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit;
            this.UncheckAllButton.Name = "UncheckAllButton";
            this.UncheckAllButton.Size = new System.Drawing.Size(114, 56);
            this.UncheckAllButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.UncheckAllButton.ButtonClick += new System.EventHandler(this.UncheckAllButton_Click);
            this.UncheckAllButton.DropDown = this.UncheckAllDropdown;
            resources.ApplyResources(this.UncheckAllButton, "UncheckAllButton");
            //
            // UncheckAllDropdown
            //
            this.UncheckAllDropdown.AutoSize = true;
            this.UncheckAllDropdown.Name = "UncheckAllDropdown";
            this.UncheckAllDropdown.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AlwaysUncheckAllButton});
            resources.ApplyResources(this.UncheckAllDropdown, "UncheckAllDropdown");
            //
            // AlwaysUncheckAllButton
            //
            this.AlwaysUncheckAllButton.AutoSize = true;
            this.AlwaysUncheckAllButton.CheckOnClick = true;
            this.AlwaysUncheckAllButton.Name = "AlwaysUncheckAllButton";
            this.AlwaysUncheckAllButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.AlwaysUncheckAllButton.CheckedChanged += new System.EventHandler(this.AlwaysUncheckAllButton_CheckedChanged);
            resources.ApplyResources(this.AlwaysUncheckAllButton, "AlwaysUncheckAllButton");
            //
            // CheckAllButton
            //
            this.CheckAllButton.Image = global::CKAN.GUI.EmbeddedImages.checkAll;
            this.CheckAllButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit;
            this.CheckAllButton.Name = "CheckAllButton";
            this.CheckAllButton.Size = new System.Drawing.Size(114, 56);
            this.CheckAllButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.CheckAllButton.Click += new System.EventHandler(this.CheckAllButton_Click);
            resources.ApplyResources(this.CheckAllButton, "CheckAllButton");
            //
            // CheckRecommendationsButton
            //
            this.CheckRecommendationsButton.Image = global::CKAN.GUI.EmbeddedImages.checkRecommendations;
            this.CheckRecommendationsButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit;
            this.CheckRecommendationsButton.Name = "CheckRecommendationsButton";
            this.CheckRecommendationsButton.Size = new System.Drawing.Size(114, 56);
            this.CheckRecommendationsButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
            this.CheckRecommendationsButton.Click += new System.EventHandler(this.CheckRecommendationsButton_Click);
            resources.ApplyResources(this.CheckRecommendationsButton, "CheckRecommendationsButton");
            //
            // RecommendedDialogLabel
            //
            this.RecommendedDialogLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.RecommendedDialogLabel.Location = new System.Drawing.Point(4, 20);
            this.RecommendedDialogLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.RecommendedDialogLabel.Name = "RecommendedDialogLabel";
            this.RecommendedDialogLabel.Size = new System.Drawing.Size(627, 20);
            this.RecommendedDialogLabel.TabIndex = 0;
            resources.ApplyResources(this.RecommendedDialogLabel, "RecommendedDialogLabel");
            //
            // RecommendedModsListView
            //
            this.RecommendedModsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RecommendedModsListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RecommendedModsListView.CheckBoxes = true;
            this.RecommendedModsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ModNameHeader,
            this.SourceModulesHeader,
            this.DescriptionHeader});
            this.RecommendedModsListView.FullRowSelect = true;
            this.RecommendedModsListView.Location = new System.Drawing.Point(9, 45);
            this.RecommendedModsListView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RecommendedModsListView.Name = "RecommendedModsListView";
            this.RecommendedModsListView.Size = new System.Drawing.Size(1510, 841);
            this.RecommendedModsListView.TabIndex = 1;
            this.RecommendedModsListView.UseCompatibleStateImageBehavior = false;
            this.RecommendedModsListView.View = System.Windows.Forms.View.Details;
            this.RecommendedModsListView.SelectedIndexChanged += new System.EventHandler(RecommendedModsListView_SelectedIndexChanged);
            this.RecommendedModsListView.Groups.Add(this.RecommendationsGroup);
            this.RecommendedModsListView.Groups.Add(this.SuggestionsGroup);
            this.RecommendedModsListView.Groups.Add(this.SupportedByGroup);
            this.RecommendedModsListView.ColumnWidthChanged += this.RecommendedModsListView_ColumnWidthChanged;
            //
            // RecommendationsGroup
            //
            this.RecommendationsGroup.Name = "RecommendationsGroup";
            this.RecommendationsGroup.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            resources.ApplyResources(this.RecommendationsGroup, "RecommendationsGroup");
            //
            // SuggestionsGroup
            //
            this.SuggestionsGroup.Name = "SuggestionsGroup";
            this.SuggestionsGroup.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            resources.ApplyResources(this.SuggestionsGroup, "SuggestionsGroup");
            //
            // SupportedByGroup
            //
            this.SupportedByGroup.Name = "SupportedByGroup";
            this.SupportedByGroup.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            resources.ApplyResources(this.SupportedByGroup, "SupportedByGroup");
            //
            // ModNameHeader
            //
            this.ModNameHeader.Width = 332;
            resources.ApplyResources(this.ModNameHeader, "ModNameHeader");
            //
            // SourceModulesHeader
            //
            this.SourceModulesHeader.Width = 180;
            resources.ApplyResources(this.SourceModulesHeader, "SourceModulesHeader");
            //
            // DescriptionHeader
            //
            this.DescriptionHeader.Width = 606;
            resources.ApplyResources(this.DescriptionHeader, "DescriptionHeader");
            //
            // BottomButtonPanel
            //
            this.BottomButtonPanel.RightControls.Add(this.RecommendedModsContinueButton);
            this.BottomButtonPanel.RightControls.Add(this.RecommendedModsCancelButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            //
            // RecommendedModsCancelButton
            //
            this.RecommendedModsCancelButton.AutoSize = true;
            this.RecommendedModsCancelButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.RecommendedModsCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RecommendedModsCancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RecommendedModsCancelButton.Name = "RecommendedModsCancelButton";
            this.RecommendedModsCancelButton.Size = new System.Drawing.Size(112, 30);
            this.RecommendedModsCancelButton.TabIndex = 3;
            this.RecommendedModsCancelButton.Click += new System.EventHandler(this.RecommendedModsCancelButton_Click);
            resources.ApplyResources(this.RecommendedModsCancelButton, "RecommendedModsCancelButton");
            //
            // RecommendedModsContinueButton
            //
            this.RecommendedModsContinueButton.AutoSize = true;
            this.RecommendedModsContinueButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.RecommendedModsContinueButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RecommendedModsContinueButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RecommendedModsContinueButton.Name = "RecommendedModsContinueButton";
            this.RecommendedModsContinueButton.Size = new System.Drawing.Size(112, 30);
            this.RecommendedModsContinueButton.TabIndex = 4;
            this.RecommendedModsContinueButton.Click += new System.EventHandler(this.RecommendedModsContinueButton_Click);
            resources.ApplyResources(this.RecommendedModsContinueButton, "RecommendedModsContinueButton");
            //
            // ChooseRecommendedMods
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.RecommendedModsListView);
            this.Controls.Add(this.RecommendedDialogLabel);
            this.Controls.Add(this.Toolbar);
            this.Controls.Add(this.BottomButtonPanel);
            this.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.Name = "ChooseRecommendedMods";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip Toolbar;
        private System.Windows.Forms.ToolStripSplitButton UncheckAllButton;
        private System.Windows.Forms.ToolStripDropDownMenu UncheckAllDropdown;
        private System.Windows.Forms.ToolStripMenuItem AlwaysUncheckAllButton;
        private System.Windows.Forms.ToolStripSeparator UncheckCheckSeparator;
        private System.Windows.Forms.ToolStripMenuItem CheckAllButton;
        private System.Windows.Forms.ToolStripMenuItem CheckRecommendationsButton;
        private System.Windows.Forms.Label RecommendedDialogLabel;
        private System.Windows.Forms.ListView RecommendedModsListView;
        private System.Windows.Forms.ListViewGroup RecommendationsGroup;
        private System.Windows.Forms.ListViewGroup SuggestionsGroup;
        private System.Windows.Forms.ListViewGroup SupportedByGroup;
        private System.Windows.Forms.ColumnHeader ModNameHeader;
        private System.Windows.Forms.ColumnHeader SourceModulesHeader;
        private System.Windows.Forms.ColumnHeader DescriptionHeader;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Button RecommendedModsCancelButton;
        private System.Windows.Forms.Button RecommendedModsContinueButton;
    }
}
