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
            this.RecommendedDialogLabel = new System.Windows.Forms.Label();
            this.RecommendedModsListView = new ThemedListView();
            this.RecommendationsGroup = new System.Windows.Forms.ListViewGroup();
            this.SuggestionsGroup = new System.Windows.Forms.ListViewGroup();
            this.SupportedByGroup = new System.Windows.Forms.ListViewGroup();
            this.ModNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SourceModulesHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DescriptionHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BottomButtonPanel = new LeftRightRowPanel();
            this.RecommendedModsToggleCheckbox = new System.Windows.Forms.CheckBox();
            this.RecommendedModsCancelButton = new System.Windows.Forms.Button();
            this.RecommendedModsContinueButton = new System.Windows.Forms.Button();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
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
            this.BottomButtonPanel.LeftControls.Add(this.RecommendedModsToggleCheckbox);
            this.BottomButtonPanel.RightControls.Add(this.RecommendedModsContinueButton);
            this.BottomButtonPanel.RightControls.Add(this.RecommendedModsCancelButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            //
            // RecommendedModsToggleCheckbox
            //
            this.RecommendedModsToggleCheckbox.AutoSize = true;
            this.RecommendedModsToggleCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RecommendedModsToggleCheckbox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RecommendedModsToggleCheckbox.Name = "RecommendedModsToggleCheckbox";
            this.RecommendedModsToggleCheckbox.Size = new System.Drawing.Size(131, 24);
            this.RecommendedModsToggleCheckbox.TabIndex = 2;
            this.RecommendedModsToggleCheckbox.CheckedChanged += new System.EventHandler(this.RecommendedModsToggleCheckbox_CheckedChanged);
            resources.ApplyResources(this.RecommendedModsToggleCheckbox, "RecommendedModsToggleCheckbox");
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

        private System.Windows.Forms.Label RecommendedDialogLabel;
        private System.Windows.Forms.ListView RecommendedModsListView;
        private System.Windows.Forms.ListViewGroup RecommendationsGroup;
        private System.Windows.Forms.ListViewGroup SuggestionsGroup;
        private System.Windows.Forms.ListViewGroup SupportedByGroup;
        private System.Windows.Forms.ColumnHeader ModNameHeader;
        private System.Windows.Forms.ColumnHeader SourceModulesHeader;
        private System.Windows.Forms.ColumnHeader DescriptionHeader;
        private LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.CheckBox RecommendedModsToggleCheckbox;
        private System.Windows.Forms.Button RecommendedModsCancelButton;
        private System.Windows.Forms.Button RecommendedModsContinueButton;
    }
}
