namespace CKAN.GUI
{
    partial class EditLabelsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(EditLabelsDialog));
            this.ToolTip = new System.Windows.Forms.ToolTip();
            this.TopButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.CreateButton = new System.Windows.Forms.Button();
            this.Splitter = new System.Windows.Forms.SplitContainer();
            this.LabelSelectionTree = new System.Windows.Forms.TreeView();
            this.SelectOrCreateLabel = new System.Windows.Forms.Label();
            this.EditDetailsPanel = new System.Windows.Forms.Panel();
            this.NameLabel = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.ColorLabel = new System.Windows.Forms.Label();
            this.ColorButton = new System.Windows.Forms.Button();
            this.InstanceNameLabel = new System.Windows.Forms.Label();
            this.InstanceNameComboBox = new System.Windows.Forms.ComboBox();
            this.HideFromOtherFiltersCheckBox = new System.Windows.Forms.CheckBox();
            this.NotifyOnChangesCheckBox = new System.Windows.Forms.CheckBox();
            this.RemoveOnChangesCheckBox = new System.Windows.Forms.CheckBox();
            this.AlertOnInstallCheckBox = new System.Windows.Forms.CheckBox();
            this.RemoveOnInstallCheckBox = new System.Windows.Forms.CheckBox();
            this.HoldVersionCheckBox = new System.Windows.Forms.CheckBox();
            this.IgnoreMissingFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.EditButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.SaveButton = new System.Windows.Forms.Button();
            this.CancelEditButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.MoveUpButton = new System.Windows.Forms.Button();
            this.MoveDownButton = new System.Windows.Forms.Button();
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.CloseButton = new System.Windows.Forms.Button();
            this.TopButtonPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Splitter)).BeginInit();
            this.Splitter.Panel1.SuspendLayout();
            this.Splitter.Panel2.SuspendLayout();
            this.Splitter.SuspendLayout();
            this.EditDetailsPanel.SuspendLayout();
            this.EditButtonPanel.SuspendLayout();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToolTip
            // 
            this.ToolTip.AutoPopDelay = 10000;
            this.ToolTip.InitialDelay = 250;
            this.ToolTip.ReshowDelay = 250;
            this.ToolTip.ShowAlways = true;
            // 
            // TopButtonPanel
            // 
            this.TopButtonPanel.LeftControls.Add(this.CreateButton);
            this.TopButtonPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopButtonPanel.Name = "TopButtonPanel";
            // 
            // CreateButton
            // 
            this.CreateButton.AutoSize = true;
            this.CreateButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.CreateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CreateButton.Name = "CreateButton";
            this.CreateButton.Size = new System.Drawing.Size(75, 23);
            this.CreateButton.TabIndex = 0;
            this.CreateButton.UseVisualStyleBackColor = true;
            this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
            resources.ApplyResources(this.CreateButton, "CreateButton");
            // 
            // Splitter
            // 
            this.Splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Splitter.Name = "Splitter";
            this.Splitter.Size = new System.Drawing.Size(500, 350);
            this.Splitter.SplitterDistance = 125;
            this.Splitter.SplitterWidth = 10;
            this.Splitter.TabIndex = 0;
            // 
            // Splitter.Panel1
            // 
            this.Splitter.Panel1.Controls.Add(this.LabelSelectionTree);
            this.Splitter.Panel1.Padding = new System.Windows.Forms.Padding(10, 6, 0, 6);
            this.Splitter.Panel1MinSize = 125;
            // 
            // Splitter.Panel2
            // 
            this.Splitter.Panel2.Controls.Add(this.SelectOrCreateLabel);
            this.Splitter.Panel2.Controls.Add(this.EditDetailsPanel);
            this.Splitter.Panel2.Controls.Add(this.MoveUpButton);
            this.Splitter.Panel2.Controls.Add(this.MoveDownButton);
            this.Splitter.Panel2.Padding = new System.Windows.Forms.Padding(0, 6, 10, 6);
            this.Splitter.Panel2MinSize = 300;
            // 
            // LabelSelectionTree
            // 
            this.LabelSelectionTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LabelSelectionTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelSelectionTree.FullRowSelect = true;
            this.LabelSelectionTree.HideSelection = false;
            this.LabelSelectionTree.Indent = 16;
            this.LabelSelectionTree.ItemHeight = 24;
            this.LabelSelectionTree.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.LabelSelectionTree.Name = "LabelSelectionTree";
            this.LabelSelectionTree.ShowPlusMinus = false;
            this.LabelSelectionTree.ShowRootLines = false;
            this.LabelSelectionTree.ShowLines = false;
            this.LabelSelectionTree.TabIndex = 0;
            this.LabelSelectionTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(LabelSelectionTree_BeforeSelect);
            this.LabelSelectionTree.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(LabelSelectionTree_BeforeCollapse);
            // 
            // SelectOrCreateLabel
            // 
            this.SelectOrCreateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectOrCreateLabel.Name = "SelectOrCreateLabel";
            this.SelectOrCreateLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            resources.ApplyResources(this.SelectOrCreateLabel, "SelectOrCreateLabel");
            // 
            // EditDetailsPanel
            // 
            this.EditDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EditDetailsPanel.Controls.Add(this.NameLabel);
            this.EditDetailsPanel.Controls.Add(this.NameTextBox);
            this.EditDetailsPanel.Controls.Add(this.ColorLabel);
            this.EditDetailsPanel.Controls.Add(this.ColorButton);
            this.EditDetailsPanel.Controls.Add(this.InstanceNameLabel);
            this.EditDetailsPanel.Controls.Add(this.InstanceNameComboBox);
            this.EditDetailsPanel.Controls.Add(this.HideFromOtherFiltersCheckBox);
            this.EditDetailsPanel.Controls.Add(this.NotifyOnChangesCheckBox);
            this.EditDetailsPanel.Controls.Add(this.RemoveOnChangesCheckBox);
            this.EditDetailsPanel.Controls.Add(this.AlertOnInstallCheckBox);
            this.EditDetailsPanel.Controls.Add(this.RemoveOnInstallCheckBox);
            this.EditDetailsPanel.Controls.Add(this.HoldVersionCheckBox);
            this.EditDetailsPanel.Controls.Add(this.IgnoreMissingFilesCheckBox);
            this.EditDetailsPanel.Controls.Add(this.EditButtonPanel);
            this.EditDetailsPanel.Controls.Add(this.MoveUpButton);
            this.EditDetailsPanel.Controls.Add(this.MoveDownButton);
            this.EditDetailsPanel.Name = "EditDetailsPanel";
            this.EditDetailsPanel.TabIndex = 1;
            this.EditDetailsPanel.Visible = false;
            // 
            // MoveUpButton
            // 
            this.MoveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top
            | System.Windows.Forms.AnchorStyles.Left));
            this.MoveUpButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MoveUpButton.Location = new System.Drawing.Point(3, 0);
            this.MoveUpButton.Name = "MoveUpButton";
            this.MoveUpButton.Size = new System.Drawing.Size(18, 23);
            this.MoveUpButton.TabIndex = 0;
            this.MoveUpButton.Text = "▴";
            this.MoveUpButton.UseVisualStyleBackColor = true;
            this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
            resources.ApplyResources(this.MoveUpButton, "MoveUpButton");
            // 
            // MoveDownButton
            // 
            this.MoveDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top
            | System.Windows.Forms.AnchorStyles.Left));
            this.MoveDownButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MoveDownButton.Location = new System.Drawing.Point(3, 25);
            this.MoveDownButton.Name = "MoveDownButton";
            this.MoveDownButton.Size = new System.Drawing.Size(18, 23);
            this.MoveDownButton.TabIndex = 0;
            this.MoveDownButton.Text = "▾";
            this.MoveDownButton.UseVisualStyleBackColor = true;
            this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButton_Click);
            resources.ApplyResources(this.MoveDownButton, "MoveDownButton");
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.NameLabel.Location = new System.Drawing.Point(38, 13);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(75, 23);
            resources.ApplyResources(this.NameLabel, "NameLabel");
            // 
            // NameTextBox
            // 
            this.NameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.NameTextBox.Location = new System.Drawing.Point(118, 10);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(125, 23);
            this.NameTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.NameTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            resources.ApplyResources(this.NameTextBox, "NameTextBox");
            // 
            // ColorLabel
            // 
            this.ColorLabel.AutoSize = true;
            this.ColorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.ColorLabel.Location = new System.Drawing.Point(38, 43);
            this.ColorLabel.Name = "ColorLabel";
            this.ColorLabel.Size = new System.Drawing.Size(75, 23);
            resources.ApplyResources(this.ColorLabel, "ColorLabel");
            // 
            // ColorButton
            // 
            this.ColorButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top
            | System.Windows.Forms.AnchorStyles.Left));
            this.ColorButton.AutoSize = true;
            this.ColorButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.ColorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ColorButton.Location = new System.Drawing.Point(118, 40);
            this.ColorButton.Name = "ColorButton";
            this.ColorButton.Size = new System.Drawing.Size(80, 20);
            this.ColorButton.UseVisualStyleBackColor = false;
            this.ColorButton.Click += new System.EventHandler(this.ColorButton_Click);
            resources.ApplyResources(this.ColorButton, "ColorButton");
            // 
            // InstanceNameLabel
            // 
            this.InstanceNameLabel.AutoSize = true;
            this.InstanceNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.InstanceNameLabel.Location = new System.Drawing.Point(38, 73);
            this.InstanceNameLabel.Name = "InstanceNameLabel";
            this.InstanceNameLabel.Size = new System.Drawing.Size(75, 23);
            resources.ApplyResources(this.InstanceNameLabel, "InstanceNameLabel");
            // 
            // InstanceNameComboBox
            // 
            this.InstanceNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.InstanceNameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.InstanceNameComboBox.Location = new System.Drawing.Point(118, 70);
            this.InstanceNameComboBox.Name = "InstanceNameComboBox";
            this.InstanceNameComboBox.Size = new System.Drawing.Size(125, 23);
            resources.ApplyResources(this.InstanceNameComboBox, "InstanceNameComboBox");
            // 
            // HideFromOtherFiltersCheckBox
            // 
            this.HideFromOtherFiltersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.HideFromOtherFiltersCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.HideFromOtherFiltersCheckBox.Location = new System.Drawing.Point(118, 100);
            this.HideFromOtherFiltersCheckBox.Name = "HideFromOtherFiltersCheckBox";
            this.HideFromOtherFiltersCheckBox.Size = new System.Drawing.Size(200, 23);
            resources.ApplyResources(this.HideFromOtherFiltersCheckBox, "HideFromOtherFiltersCheckBox");
            // 
            // NotifyOnChangesCheckBox
            // 
            this.NotifyOnChangesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.NotifyOnChangesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NotifyOnChangesCheckBox.Location = new System.Drawing.Point(118, 124);
            this.NotifyOnChangesCheckBox.Name = "NotifyOnChangesCheckBox";
            this.NotifyOnChangesCheckBox.Size = new System.Drawing.Size(200, 23);
            resources.ApplyResources(this.NotifyOnChangesCheckBox, "NotifyOnChangesCheckBox");
            // 
            // RemoveOnChangesCheckBox
            // 
            this.RemoveOnChangesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.RemoveOnChangesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RemoveOnChangesCheckBox.Location = new System.Drawing.Point(118, 148);
            this.RemoveOnChangesCheckBox.Name = "RemoveOnChangesCheckBox";
            this.RemoveOnChangesCheckBox.Size = new System.Drawing.Size(200, 23);
            resources.ApplyResources(this.RemoveOnChangesCheckBox, "RemoveOnChangesCheckBox");
            // 
            // AlertOnInstallCheckBox
            // 
            this.AlertOnInstallCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.AlertOnInstallCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AlertOnInstallCheckBox.Location = new System.Drawing.Point(118, 172);
            this.AlertOnInstallCheckBox.Name = "AlertOnInstallCheckBox";
            this.AlertOnInstallCheckBox.Size = new System.Drawing.Size(200, 23);
            resources.ApplyResources(this.AlertOnInstallCheckBox, "AlertOnInstallCheckBox");
            // 
            // RemoveOnInstallCheckBox
            // 
            this.RemoveOnInstallCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.RemoveOnInstallCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RemoveOnInstallCheckBox.Location = new System.Drawing.Point(118, 196);
            this.RemoveOnInstallCheckBox.Name = "RemoveOnInstallCheckBox";
            this.RemoveOnInstallCheckBox.Size = new System.Drawing.Size(200, 23);
            resources.ApplyResources(this.RemoveOnInstallCheckBox, "RemoveOnInstallCheckBox");
            // 
            // HoldVersionCheckBox
            // 
            this.HoldVersionCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.HoldVersionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.HoldVersionCheckBox.Location = new System.Drawing.Point(118, 220);
            this.HoldVersionCheckBox.Name = "HoldVersionCheckBox";
            this.HoldVersionCheckBox.Size = new System.Drawing.Size(200, 23);
            resources.ApplyResources(this.HoldVersionCheckBox, "HoldVersionCheckBox");
            // 
            // IgnoreMissingFilesCheckBox
            // 
            this.IgnoreMissingFilesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.IgnoreMissingFilesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.IgnoreMissingFilesCheckBox.Location = new System.Drawing.Point(118, 244);
            this.IgnoreMissingFilesCheckBox.Name = "IgnoreMissingFilesCheckBox";
            this.IgnoreMissingFilesCheckBox.Size = new System.Drawing.Size(200, 23);
            resources.ApplyResources(this.IgnoreMissingFilesCheckBox, "IgnoreMissingFilesCheckBox");
            // 
            // EditButtonPanel
            // 
            this.EditButtonPanel.LeftControls.Add(this.SaveButton);
            this.EditButtonPanel.LeftControls.Add(this.CancelEditButton);
            this.EditButtonPanel.RightControls.Add(this.DeleteButton);
            this.EditButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.EditButtonPanel.Name = "EditButtonPanel";
            // 
            // SaveButton
            // 
            this.SaveButton.AutoSize = true;
            this.SaveButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.SaveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 0;
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            resources.ApplyResources(this.SaveButton, "SaveButton");
            // 
            // CancelEditButton
            // 
            this.CancelEditButton.AutoSize = true;
            this.CancelEditButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.CancelEditButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelEditButton.Name = "CancelEditButton";
            this.CancelEditButton.Size = new System.Drawing.Size(75, 23);
            this.CancelEditButton.TabIndex = 0;
            this.CancelEditButton.UseVisualStyleBackColor = true;
            this.CancelEditButton.Click += new System.EventHandler(this.CancelEditButton_Click);
            resources.ApplyResources(this.CancelEditButton, "CancelEditButton");
            // 
            // DeleteButton
            // 
            this.DeleteButton.AutoSize = true;
            this.DeleteButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.DeleteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(75, 23);
            this.DeleteButton.TabIndex = 0;
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            resources.ApplyResources(this.DeleteButton, "DeleteButton");
            // 
            // BottomButtonPanel
            // 
            this.BottomButtonPanel.LeftControls.Add(this.CloseButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            // 
            // CloseButton
            // 
            this.CloseButton.AutoSize = true;
            this.CloseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 2;
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            resources.ApplyResources(this.CloseButton, "CloseButton");
            // 
            // EditLabelsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 430);
            this.Controls.Add(this.Splitter);
            this.Controls.Add(this.TopButtonPanel);
            this.Controls.Add(this.BottomButtonPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Icon = EmbeddedImages.AppIcon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.HelpButton = true;
            this.Name = "EditLabelsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            this.TopButtonPanel.ResumeLayout(false);
            this.TopButtonPanel.PerformLayout();
            this.EditButtonPanel.ResumeLayout(false);
            this.EditButtonPanel.PerformLayout();
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
            this.EditDetailsPanel.ResumeLayout(false);
            this.EditDetailsPanel.PerformLayout();
            this.Splitter.Panel1.ResumeLayout(false);
            this.Splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Splitter)).EndInit();
            this.Splitter.ResumeLayout(false);
            this.Splitter.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip;
        private CKAN.GUI.LeftRightRowPanel TopButtonPanel;
        private System.Windows.Forms.Button CreateButton;
        private System.Windows.Forms.SplitContainer Splitter;
        private System.Windows.Forms.TreeView LabelSelectionTree;
        private System.Windows.Forms.Label SelectOrCreateLabel;
        private System.Windows.Forms.Panel EditDetailsPanel;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label InstanceNameLabel;
        private System.Windows.Forms.ComboBox InstanceNameComboBox;
        private System.Windows.Forms.CheckBox HideFromOtherFiltersCheckBox;
        private System.Windows.Forms.CheckBox NotifyOnChangesCheckBox;
        private System.Windows.Forms.CheckBox RemoveOnChangesCheckBox;
        private System.Windows.Forms.CheckBox AlertOnInstallCheckBox;
        private System.Windows.Forms.CheckBox RemoveOnInstallCheckBox;
        private System.Windows.Forms.CheckBox HoldVersionCheckBox;
        private System.Windows.Forms.CheckBox IgnoreMissingFilesCheckBox;
        private System.Windows.Forms.Label ColorLabel;
        private System.Windows.Forms.Button ColorButton;
        private CKAN.GUI.LeftRightRowPanel EditButtonPanel;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button CancelEditButton;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button MoveUpButton;
        private System.Windows.Forms.Button MoveDownButton;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Button CloseButton;
    }
}
