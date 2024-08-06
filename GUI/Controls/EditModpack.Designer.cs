namespace CKAN.GUI
{
    partial class EditModpack
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(EditModpack));
            this.ToolTip = new System.Windows.Forms.ToolTip();
            this.TopEditPanel = new System.Windows.Forms.Panel();
            this.IdentifierLabel = new System.Windows.Forms.Label();
            this.IdentifierTextBox = new System.Windows.Forms.TextBox();
            this.NameLabel = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.AbstractLabel = new System.Windows.Forms.Label();
            this.AbstractTextBox = new System.Windows.Forms.TextBox();
            this.AuthorLabel = new System.Windows.Forms.Label();
            this.AuthorTextBox = new System.Windows.Forms.TextBox();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.VersionTextBox = new System.Windows.Forms.TextBox();
            this.GameVersionLabel = new System.Windows.Forms.Label();
            this.GameVersionMinComboBox = new System.Windows.Forms.ComboBox();
            this.GameVersionMaxComboBox = new System.Windows.Forms.ComboBox();
            this.LicenseLabel = new System.Windows.Forms.Label();
            this.LicenseComboBox = new System.Windows.Forms.ComboBox();
            this.IncludeVersionsCheckbox = new System.Windows.Forms.CheckBox();
            this.IncludeOptRelsCheckbox = new System.Windows.Forms.CheckBox();
            this.RelationshipsListView = new CKAN.GUI.ThemedListView();
            this.ModNameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ModVersionColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ModAbstractColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DependsGroup = new System.Windows.Forms.ListViewGroup();
            this.RecommendationsGroup = new System.Windows.Forms.ListViewGroup();
            this.SuggestionsGroup = new System.Windows.Forms.ListViewGroup();
            this.IgnoredGroup = new System.Windows.Forms.ListViewGroup();
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.DependsRadioButton = new System.Windows.Forms.RadioButton();
            this.RecommendsRadioButton = new System.Windows.Forms.RadioButton();
            this.SuggestsRadioButton = new System.Windows.Forms.RadioButton();
            this.IgnoreRadioButton = new System.Windows.Forms.RadioButton();
            this.CancelExportButton = new System.Windows.Forms.Button();
            this.ExportModpackButton = new System.Windows.Forms.Button();
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
            // TopEditPanel
            // 
            this.TopEditPanel.Controls.Add(this.IdentifierLabel);
            this.TopEditPanel.Controls.Add(this.IdentifierTextBox);
            this.TopEditPanel.Controls.Add(this.NameLabel);
            this.TopEditPanel.Controls.Add(this.NameTextBox);
            this.TopEditPanel.Controls.Add(this.AbstractLabel);
            this.TopEditPanel.Controls.Add(this.AbstractTextBox);
            this.TopEditPanel.Controls.Add(this.AuthorLabel);
            this.TopEditPanel.Controls.Add(this.AuthorTextBox);
            this.TopEditPanel.Controls.Add(this.VersionLabel);
            this.TopEditPanel.Controls.Add(this.VersionTextBox);
            this.TopEditPanel.Controls.Add(this.GameVersionLabel);
            this.TopEditPanel.Controls.Add(this.GameVersionMinComboBox);
            this.TopEditPanel.Controls.Add(this.GameVersionMaxComboBox);
            this.TopEditPanel.Controls.Add(this.LicenseLabel);
            this.TopEditPanel.Controls.Add(this.LicenseComboBox);
            this.TopEditPanel.Controls.Add(this.IncludeVersionsCheckbox);
            this.TopEditPanel.Controls.Add(this.IncludeOptRelsCheckbox);
            this.TopEditPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopEditPanel.Name = "TopEditPanel";
            this.TopEditPanel.Size = new System.Drawing.Size(500, 160);
            // 
            // IdentifierLabel
            // 
            this.IdentifierLabel.AutoSize = true;
            this.IdentifierLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.IdentifierLabel.Location = new System.Drawing.Point(10, 10);
            this.IdentifierLabel.Name = "IdentifierLabel";
            this.IdentifierLabel.Size = new System.Drawing.Size(75, 23);
            this.IdentifierLabel.TabIndex = 0;
            resources.ApplyResources(this.IdentifierLabel, "IdentifierLabel");
            // 
            // IdentifierTextBox
            // 
            this.IdentifierTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.IdentifierTextBox.Location = new System.Drawing.Point(125, 10);
            this.IdentifierTextBox.Name = "IdentifierTextBox";
            this.IdentifierTextBox.Size = new System.Drawing.Size(250, 23);
            this.IdentifierTextBox.TabIndex = 1;
            this.IdentifierTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.IdentifierTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            resources.ApplyResources(this.IdentifierTextBox, "IdentifierTextBox");
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.NameLabel.Location = new System.Drawing.Point(10, 40);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(75, 23);
            this.NameLabel.TabIndex = 2;
            resources.ApplyResources(this.NameLabel, "NameLabel");
            // 
            // NameTextBox
            // 
            this.NameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.NameTextBox.Location = new System.Drawing.Point(125, 40);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(250, 23);
            this.NameTextBox.TabIndex = 3;
            this.NameTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.NameTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            resources.ApplyResources(this.NameTextBox, "NameTextBox");
            // 
            // AbstractLabel
            // 
            this.AbstractLabel.AutoSize = true;
            this.AbstractLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.AbstractLabel.Location = new System.Drawing.Point(10, 70);
            this.AbstractLabel.Name = "AbstractLabel";
            this.AbstractLabel.Size = new System.Drawing.Size(75, 23);
            this.AbstractLabel.TabIndex = 4;
            resources.ApplyResources(this.AbstractLabel, "AbstractLabel");
            // 
            // AbstractTextBox
            // 
            this.AbstractTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.AbstractTextBox.Location = new System.Drawing.Point(125, 70);
            this.AbstractTextBox.Multiline = true;
            this.AbstractTextBox.Name = "AbstractTextBox";
            this.AbstractTextBox.Size = new System.Drawing.Size(250, 50);
            this.AbstractTextBox.TabIndex = 5;
            this.AbstractTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.AbstractTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            resources.ApplyResources(this.AbstractTextBox, "AbstractTextBox");
            // 
            // AuthorLabel
            // 
            this.AuthorLabel.AutoSize = true;
            this.AuthorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.AuthorLabel.Location = new System.Drawing.Point(10, 130);
            this.AuthorLabel.Name = "AuthorLabel";
            this.AuthorLabel.Size = new System.Drawing.Size(75, 23);
            this.AuthorLabel.TabIndex = 6;
            resources.ApplyResources(this.AuthorLabel, "AuthorLabel");
            // 
            // AuthorTextBox
            // 
            this.AuthorTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.AuthorTextBox.Location = new System.Drawing.Point(125, 130);
            this.AuthorTextBox.Name = "AbstractTextBox";
            this.AuthorTextBox.Size = new System.Drawing.Size(250, 23);
            this.AuthorTextBox.TabIndex = 7;
            this.AuthorTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.AuthorTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            resources.ApplyResources(this.AuthorTextBox, "AuthorTextBox");
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.VersionLabel.Location = new System.Drawing.Point(400, 10);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(75, 23);
            this.VersionLabel.TabIndex = 8;
            resources.ApplyResources(this.VersionLabel, "VersionLabel");
            // 
            // VersionTextBox
            // 
            this.VersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.VersionTextBox.Location = new System.Drawing.Point(515, 10);
            this.VersionTextBox.Name = "VersionTextBox";
            this.VersionTextBox.Size = new System.Drawing.Size(250, 23);
            this.VersionTextBox.TabIndex = 9;
            this.VersionTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.VersionTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            resources.ApplyResources(this.VersionTextBox, "VersionTextBox");
            // 
            // GameVersionLabel
            // 
            this.GameVersionLabel.AutoSize = true;
            this.GameVersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.GameVersionLabel.Location = new System.Drawing.Point(400, 40);
            this.GameVersionLabel.Name = "GameVersionLabel";
            this.GameVersionLabel.Size = new System.Drawing.Size(75, 23);
            this.GameVersionLabel.TabIndex = 10;
            resources.ApplyResources(this.GameVersionLabel, "GameVersionLabel");
            // 
            // GameVersionMinComboBox
            // 
            this.GameVersionMinComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.GameVersionMinComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.GameVersionMinComboBox.Location = new System.Drawing.Point(515, 40);
            this.GameVersionMinComboBox.Name = "GameVersionMinComboBox";
            this.GameVersionMinComboBox.Size = new System.Drawing.Size(70, 23);
            this.GameVersionMinComboBox.TabIndex = 11;
            resources.ApplyResources(this.GameVersionMinComboBox, "GameVersionMinComboBox");
            // 
            // GameVersionMaxComboBox
            // 
            this.GameVersionMaxComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.GameVersionMaxComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.GameVersionMaxComboBox.Location = new System.Drawing.Point(595, 40);
            this.GameVersionMaxComboBox.Name = "GameVersionMaxComboBox";
            this.GameVersionMaxComboBox.Size = new System.Drawing.Size(70, 23);
            this.GameVersionMaxComboBox.TabIndex = 12;
            resources.ApplyResources(this.GameVersionMaxComboBox, "GameVersionMaxComboBox");
            // 
            // LicenseLabel
            // 
            this.LicenseLabel.AutoSize = true;
            this.LicenseLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.LicenseLabel.Location = new System.Drawing.Point(400, 70);
            this.LicenseLabel.Name = "LicenseLabel";
            this.LicenseLabel.Size = new System.Drawing.Size(75, 23);
            this.LicenseLabel.TabIndex = 13;
            resources.ApplyResources(this.LicenseLabel, "LicenseLabel");
            // 
            // LicenseComboBox
            // 
            this.LicenseComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.LicenseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LicenseComboBox.Location = new System.Drawing.Point(515, 70);
            this.LicenseComboBox.Name = "LicenseComboBox";
            this.LicenseComboBox.Size = new System.Drawing.Size(150, 23);
            this.LicenseComboBox.TabIndex = 14;
            resources.ApplyResources(this.LicenseComboBox, "LicenseComboBox");
            //
            // IncludeVersionsCheckbox
            //
            this.IncludeVersionsCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.IncludeVersionsCheckbox.AutoSize = true;
            this.IncludeVersionsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.IncludeVersionsCheckbox.Location = new System.Drawing.Point(515, 100);
            this.IncludeVersionsCheckbox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.IncludeVersionsCheckbox.Name = "IncludeVersionsCheckbox";
            this.IncludeVersionsCheckbox.Size = new System.Drawing.Size(131, 24);
            this.IncludeVersionsCheckbox.TabIndex = 15;
            resources.ApplyResources(this.IncludeVersionsCheckbox, "IncludeVersionsCheckbox");
            //
            // IncludeOptRelsCheckbox
            //
            this.IncludeOptRelsCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.IncludeOptRelsCheckbox.AutoSize = true;
            this.IncludeOptRelsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.IncludeOptRelsCheckbox.Location = new System.Drawing.Point(515, 130);
            this.IncludeOptRelsCheckbox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.IncludeOptRelsCheckbox.Name = "IncludeOptRelsCheckbox";
            this.IncludeOptRelsCheckbox.Size = new System.Drawing.Size(131, 24);
            this.IncludeOptRelsCheckbox.TabIndex = 16;
            resources.ApplyResources(this.IncludeOptRelsCheckbox, "IncludeOptRelsCheckbox");
            //
            // RelationshipsListView
            //
            this.RelationshipsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RelationshipsListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RelationshipsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ModNameColumn,
            this.ModVersionColumn,
            this.ModAbstractColumn});
            this.RelationshipsListView.FullRowSelect = true;
            this.RelationshipsListView.HideSelection = false;
            this.RelationshipsListView.Location = new System.Drawing.Point(9, 45);
            this.RelationshipsListView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RelationshipsListView.Name = "RelationshipsListView";
            this.RelationshipsListView.Size = new System.Drawing.Size(1510, 841);
            this.RelationshipsListView.TabIndex = 17;
            this.RelationshipsListView.UseCompatibleStateImageBehavior = false;
            this.RelationshipsListView.View = System.Windows.Forms.View.Details;
            this.RelationshipsListView.Groups.Add(this.DependsGroup);
            this.RelationshipsListView.Groups.Add(this.RecommendationsGroup);
            this.RelationshipsListView.Groups.Add(this.SuggestionsGroup);
            this.RelationshipsListView.Groups.Add(this.IgnoredGroup);
            this.RelationshipsListView.KeyDown += new System.Windows.Forms.KeyEventHandler(RelationshipsListView_KeyDown);
            this.RelationshipsListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(RelationshipsListView_ItemSelectionChanged);
            // 
            // ModNameColumn
            // 
            this.ModNameColumn.Width = 332;
            resources.ApplyResources(this.ModNameColumn, "ModNameColumn");
            // 
            // ModVersionColumn
            // 
            this.ModVersionColumn.Width = 180;
            resources.ApplyResources(this.ModVersionColumn, "ModVersionColumn");
            // 
            // ModAbstractColumn
            // 
            this.ModAbstractColumn.Width = 500;
            resources.ApplyResources(this.ModAbstractColumn, "ModAbstractColumn");
            // 
            // DependsGroup
            // 
            this.DependsGroup.Name = "DependsGroup";
            this.DependsGroup.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            resources.ApplyResources(this.DependsGroup, "DependsGroup");
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
            // IgnoredGroup
            // 
            this.IgnoredGroup.Name = "IgnoredGroup";
            this.IgnoredGroup.HeaderAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            resources.ApplyResources(this.IgnoredGroup, "IgnoredGroup");
            // 
            // BottomButtonPanel
            // 
            this.BottomButtonPanel.LeftControls.Add(this.DependsRadioButton);
            this.BottomButtonPanel.LeftControls.Add(this.RecommendsRadioButton);
            this.BottomButtonPanel.LeftControls.Add(this.SuggestsRadioButton);
            this.BottomButtonPanel.LeftControls.Add(this.IgnoreRadioButton);
            this.BottomButtonPanel.RightControls.Add(this.ExportModpackButton);
            this.BottomButtonPanel.RightControls.Add(this.CancelExportButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            // 
            // DependsRadioButton
            // 
            this.DependsRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.DependsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DependsRadioButton.Name = "DependsRadioButton";
            this.DependsRadioButton.Size = new System.Drawing.Size(112, 30);
            this.DependsRadioButton.TabIndex = 18;
            this.DependsRadioButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DependsRadioButton.Click += new System.EventHandler(this.DependsRadioButton_CheckedChanged);
            resources.ApplyResources(this.DependsRadioButton, "DependsRadioButton");
            // 
            // RecommendsRadioButton
            // 
            this.RecommendsRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.RecommendsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RecommendsRadioButton.Name = "RecommendsRadioButton";
            this.RecommendsRadioButton.Size = new System.Drawing.Size(112, 30);
            this.RecommendsRadioButton.TabIndex = 19;
            this.RecommendsRadioButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RecommendsRadioButton.Click += new System.EventHandler(this.RecommendsRadioButton_CheckedChanged);
            resources.ApplyResources(this.RecommendsRadioButton, "RecommendsRadioButton");
            // 
            // SuggestsRadioButton
            // 
            this.SuggestsRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.SuggestsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SuggestsRadioButton.Name = "SuggestsRadioButton";
            this.SuggestsRadioButton.Size = new System.Drawing.Size(112, 30);
            this.SuggestsRadioButton.TabIndex = 20;
            this.SuggestsRadioButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.SuggestsRadioButton.Click += new System.EventHandler(this.SuggestsRadioButton_CheckedChanged);
            resources.ApplyResources(this.SuggestsRadioButton, "SuggestsRadioButton");
            // 
            // IgnoreRadioButton
            // 
            this.IgnoreRadioButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.IgnoreRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.IgnoreRadioButton.Name = "IgnoreRadioButton";
            this.IgnoreRadioButton.Size = new System.Drawing.Size(112, 30);
            this.IgnoreRadioButton.TabIndex = 21;
            this.IgnoreRadioButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.IgnoreRadioButton.Click += new System.EventHandler(this.IgnoreRadioButton_CheckedChanged);
            resources.ApplyResources(this.IgnoreRadioButton, "IgnoreRadioButton");
            // 
            // CancelExportButton
            // 
            this.CancelExportButton.AutoSize = true;
            this.CancelExportButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.CancelExportButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelExportButton.Name = "CancelExportButton";
            this.CancelExportButton.Size = new System.Drawing.Size(112, 30);
            this.CancelExportButton.TabIndex = 22;
            this.CancelExportButton.UseVisualStyleBackColor = true;
            this.CancelExportButton.Click += new System.EventHandler(this.CancelExportButton_Click);
            resources.ApplyResources(this.CancelExportButton, "CancelExportButton");
            // 
            // ExportModpackButton
            // 
            this.ExportModpackButton.AutoSize = true;
            this.ExportModpackButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.ExportModpackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ExportModpackButton.Name = "ExportModpackButton";
            this.ExportModpackButton.Size = new System.Drawing.Size(112, 30);
            this.ExportModpackButton.TabIndex = 22;
            this.ExportModpackButton.UseVisualStyleBackColor = true;
            this.ExportModpackButton.Click += new System.EventHandler(this.ExportModpackButton_Click);
            resources.ApplyResources(this.ExportModpackButton, "ExportModpackButton");
            // 
            // EditModpack
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.RelationshipsListView);
            this.Controls.Add(this.TopEditPanel);
            this.Controls.Add(this.BottomButtonPanel);
            this.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.Name = "EditModpack";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Panel TopEditPanel;
        private System.Windows.Forms.Label IdentifierLabel;
        private System.Windows.Forms.TextBox IdentifierTextBox;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label AbstractLabel;
        private System.Windows.Forms.TextBox AbstractTextBox;
        private System.Windows.Forms.Label AuthorLabel;
        private System.Windows.Forms.TextBox AuthorTextBox;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.TextBox VersionTextBox;
        private System.Windows.Forms.Label GameVersionLabel;
        private System.Windows.Forms.ComboBox GameVersionMinComboBox;
        private System.Windows.Forms.ComboBox GameVersionMaxComboBox;
        private System.Windows.Forms.Label LicenseLabel;
        private System.Windows.Forms.ComboBox LicenseComboBox;
        private System.Windows.Forms.CheckBox IncludeVersionsCheckbox;
        private System.Windows.Forms.CheckBox IncludeOptRelsCheckbox;
        private System.Windows.Forms.ListView RelationshipsListView;
        private System.Windows.Forms.ColumnHeader ModNameColumn;
        private System.Windows.Forms.ColumnHeader ModVersionColumn;
        private System.Windows.Forms.ColumnHeader ModAbstractColumn;
        private System.Windows.Forms.ListViewGroup DependsGroup;
        private System.Windows.Forms.ListViewGroup RecommendationsGroup;
        private System.Windows.Forms.ListViewGroup SuggestionsGroup;
        private System.Windows.Forms.ListViewGroup IgnoredGroup;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.RadioButton DependsRadioButton;
        private System.Windows.Forms.RadioButton RecommendsRadioButton;
        private System.Windows.Forms.RadioButton SuggestsRadioButton;
        private System.Windows.Forms.RadioButton IgnoreRadioButton;
        private System.Windows.Forms.Button CancelExportButton;
        private System.Windows.Forms.Button ExportModpackButton;
    }
}
