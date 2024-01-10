namespace CKAN.GUI
{
    partial class ChooseProvidedMods
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(ChooseProvidedMods));
            this.ChooseProvidedModsLabel = new System.Windows.Forms.Label();
            this.ChooseProvidedModsListView = new ThemedListView();
            this.modNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.modDescriptionColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.ChooseProvidedModsCancelButton = new System.Windows.Forms.Button();
            this.ChooseProvidedModsContinueButton = new System.Windows.Forms.Button();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // ChooseProvidedModsLabel
            //
            this.ChooseProvidedModsLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ChooseProvidedModsLabel.Location = new System.Drawing.Point(9, 18);
            this.ChooseProvidedModsLabel.Margin = new System.Windows.Forms.Padding(5,5,5,5);
            this.ChooseProvidedModsLabel.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.ChooseProvidedModsLabel.Name = "ChooseProvidedModsLabel";
            this.ChooseProvidedModsLabel.Size = new System.Drawing.Size(490, 40);
            this.ChooseProvidedModsLabel.TabIndex = 0;
            resources.ApplyResources(this.ChooseProvidedModsLabel, "ChooseProvidedModsLabel");
            //
            // ChooseProvidedModsListView
            //
            this.ChooseProvidedModsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChooseProvidedModsListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ChooseProvidedModsListView.CheckBoxes = true;
            this.ChooseProvidedModsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.modNameColumnHeader,
            this.modDescriptionColumnHeader});
            this.ChooseProvidedModsListView.FullRowSelect = true;
            this.ChooseProvidedModsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ChooseProvidedModsListView.Location = new System.Drawing.Point(9, 43);
            this.ChooseProvidedModsListView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ChooseProvidedModsListView.MultiSelect = false;
            this.ChooseProvidedModsListView.Name = "ChooseProvidedModsListView";
            this.ChooseProvidedModsListView.Size = new System.Drawing.Size(1510, 841);
            this.ChooseProvidedModsListView.TabIndex = 1;
            this.ChooseProvidedModsListView.UseCompatibleStateImageBehavior = false;
            this.ChooseProvidedModsListView.View = System.Windows.Forms.View.Details;
            this.ChooseProvidedModsListView.SelectedIndexChanged += new System.EventHandler(ChooseProvidedModsListView_SelectedIndexChanged);
            this.ChooseProvidedModsListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(ChooseProvidedModsListView_ItemChecked);
            //
            // modNameColumnHeader
            //
            this.modNameColumnHeader.Width = 332;
            resources.ApplyResources(this.modNameColumnHeader, "modNameColumnHeader");
            //
            // modDescriptionColumnHeader
            //
            this.modDescriptionColumnHeader.Width = 606;
            resources.ApplyResources(this.modDescriptionColumnHeader, "modDescriptionColumnHeader");
            // 
            // BottomButtonPanel
            // 
            this.BottomButtonPanel.RightControls.Add(this.ChooseProvidedModsContinueButton);
            this.BottomButtonPanel.RightControls.Add(this.ChooseProvidedModsCancelButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            //
            // ChooseProvidedModsCancelButton
            //
            this.ChooseProvidedModsCancelButton.AutoSize = true;
            this.ChooseProvidedModsCancelButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.ChooseProvidedModsCancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChooseProvidedModsCancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ChooseProvidedModsCancelButton.Name = "ChooseProvidedModsCancelButton";
            this.ChooseProvidedModsCancelButton.Size = new System.Drawing.Size(112, 30);
            this.ChooseProvidedModsCancelButton.TabIndex = 2;
            this.ChooseProvidedModsCancelButton.Click += new System.EventHandler(this.ChooseProvidedModsCancelButton_Click);
            resources.ApplyResources(this.ChooseProvidedModsCancelButton, "ChooseProvidedModsCancelButton");
            //
            // ChooseProvidedModsContinueButton
            //
            this.ChooseProvidedModsContinueButton.AutoSize = true;
            this.ChooseProvidedModsContinueButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.ChooseProvidedModsContinueButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChooseProvidedModsContinueButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ChooseProvidedModsContinueButton.Name = "ChooseProvidedModsContinueButton";
            this.ChooseProvidedModsContinueButton.Size = new System.Drawing.Size(112, 30);
            this.ChooseProvidedModsContinueButton.TabIndex = 3;
            this.ChooseProvidedModsContinueButton.Click += new System.EventHandler(this.ChooseProvidedModsContinueButton_Click);
            resources.ApplyResources(this.ChooseProvidedModsContinueButton, "ChooseProvidedModsContinueButton");
            // 
            // ChooseProvidedMods
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.ChooseProvidedModsListView);
            this.Controls.Add(this.ChooseProvidedModsLabel);
            this.Controls.Add(this.BottomButtonPanel);
            this.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.Name = "ChooseProvidedMods";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label ChooseProvidedModsLabel;
        private System.Windows.Forms.ListView ChooseProvidedModsListView;
        private System.Windows.Forms.ColumnHeader modNameColumnHeader;
        private System.Windows.Forms.ColumnHeader modDescriptionColumnHeader;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Button ChooseProvidedModsCancelButton;
        private System.Windows.Forms.Button ChooseProvidedModsContinueButton;
    }
}
