namespace CKAN.GUI
{
    partial class EditModSearchDetails
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(EditModSearchDetails));
            this.FilterByNameLabel = new System.Windows.Forms.Label();
            this.FilterByNameTextBox = new CKAN.GUI.HintTextBox();
            this.FilterByAuthorLabel = new System.Windows.Forms.Label();
            this.FilterByAuthorTextBox = new CKAN.GUI.HintTextBox();
            this.FilterByDescriptionLabel = new System.Windows.Forms.Label();
            this.FilterByDescriptionTextBox = new CKAN.GUI.HintTextBox();
            this.FilterByLanguageLabel = new System.Windows.Forms.Label();
            this.FilterByLanguageTextBox = new CKAN.GUI.HintTextBox();
            this.FilterByDependsLabel = new System.Windows.Forms.Label();
            this.FilterByDependsTextBox = new CKAN.GUI.HintTextBox();
            this.FilterByRecommendsLabel = new System.Windows.Forms.Label();
            this.FilterByRecommendsTextBox = new CKAN.GUI.HintTextBox();
            this.FilterBySuggestsLabel = new System.Windows.Forms.Label();
            this.FilterBySuggestsTextBox = new CKAN.GUI.HintTextBox();
            this.FilterByConflictsLabel = new System.Windows.Forms.Label();
            this.FilterByConflictsTextBox = new CKAN.GUI.HintTextBox();
            this.FilterByTagsLabel = new System.Windows.Forms.Label();
            this.FilterByTagsTextBox = new CKAN.GUI.HintTextBox();
            this.FilterByLabelsLabel = new System.Windows.Forms.Label();
            this.FilterByLabelsTextBox = new CKAN.GUI.HintTextBox();
            this.CompatibleLabel = new System.Windows.Forms.Label();
            this.CompatibleToggle = new CKAN.GUI.TriStateToggle();
            this.InstalledLabel = new System.Windows.Forms.Label();
            this.InstalledToggle = new CKAN.GUI.TriStateToggle();
            this.CachedLabel = new System.Windows.Forms.Label();
            this.CachedToggle = new CKAN.GUI.TriStateToggle();
            this.NewlyCompatibleLabel = new System.Windows.Forms.Label();
            this.NewlyCompatibleToggle = new CKAN.GUI.TriStateToggle();
            this.UpgradeableLabel = new System.Windows.Forms.Label();
            this.UpgradeableToggle = new CKAN.GUI.TriStateToggle();
            this.ReplaceableLabel = new System.Windows.Forms.Label();
            this.ReplaceableToggle = new CKAN.GUI.TriStateToggle();
            this.SuspendLayout();
            //
            // FilterByNameLabel
            //
            this.FilterByNameLabel.AutoSize = true;
            this.FilterByNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByNameLabel.Location = new System.Drawing.Point(6, 9);
            this.FilterByNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByNameLabel.Name = "FilterByNameLabel";
            this.FilterByNameLabel.Size = new System.Drawing.Size(147, 20);
            this.FilterByNameLabel.TabIndex = 0;
            resources.ApplyResources(this.FilterByNameLabel, "FilterByNameLabel");
            //
            // FilterByNameTextBox
            //
            this.FilterByNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.FilterByNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByNameTextBox.Location = new System.Drawing.Point(130, 7);
            this.FilterByNameTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByNameTextBox.Name = "FilterByNameTextBox";
            this.FilterByNameTextBox.Size = new System.Drawing.Size(160, 26);
            this.FilterByNameTextBox.TabIndex = 1;
            this.FilterByNameTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByNameTextBox, "FilterByNameTextBox");
            //
            // FilterByAuthorLabel
            //
            this.FilterByAuthorLabel.AutoSize = true;
            this.FilterByAuthorLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByAuthorLabel.Location = new System.Drawing.Point(6, 35);
            this.FilterByAuthorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByAuthorLabel.Name = "FilterByAuthorLabel";
            this.FilterByAuthorLabel.Size = new System.Drawing.Size(162, 20);
            this.FilterByAuthorLabel.TabIndex = 2;
            resources.ApplyResources(this.FilterByAuthorLabel, "FilterByAuthorLabel");
            //
            // FilterByAuthorTextBox
            //
            this.FilterByAuthorTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.FilterByAuthorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByAuthorTextBox.Location = new System.Drawing.Point(130, 33);
            this.FilterByAuthorTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByAuthorTextBox.Name = "FilterByAuthorTextBox";
            this.FilterByAuthorTextBox.Size = new System.Drawing.Size(160, 26);
            this.FilterByAuthorTextBox.TabIndex = 3;
            this.FilterByAuthorTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByAuthorTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByAuthorTextBox, "FilterByAuthorTextBox");
            //
            // FilterByDescriptionLabel
            //
            this.FilterByDescriptionLabel.AutoSize = true;
            this.FilterByDescriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByDescriptionLabel.Location = new System.Drawing.Point(6, 61);
            this.FilterByDescriptionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByDescriptionLabel.Name = "FilterByDescriptionLabel";
            this.FilterByDescriptionLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByDescriptionLabel.TabIndex = 4;
            resources.ApplyResources(this.FilterByDescriptionLabel, "FilterByDescriptionLabel");
            //
            // FilterByDescriptionTextBox
            //
            this.FilterByDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.FilterByDescriptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByDescriptionTextBox.Location = new System.Drawing.Point(130, 59);
            this.FilterByDescriptionTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByDescriptionTextBox.Name = "FilterByDescriptionTextBox";
            this.FilterByDescriptionTextBox.Size = new System.Drawing.Size(160, 26);
            this.FilterByDescriptionTextBox.TabIndex = 5;
            this.FilterByDescriptionTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByDescriptionTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByDescriptionTextBox, "FilterByDescriptionTextBox");
            //
            // FilterByLanguageLabel
            //
            this.FilterByLanguageLabel.AutoSize = true;
            this.FilterByLanguageLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByLanguageLabel.Location = new System.Drawing.Point(6, 87);
            this.FilterByLanguageLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByLanguageLabel.Name = "FilterByLanguageLabel";
            this.FilterByLanguageLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByLanguageLabel.TabIndex = 6;
            resources.ApplyResources(this.FilterByLanguageLabel, "FilterByLanguageLabel");
            //
            // FilterByLanguageTextBox
            //
            this.FilterByLanguageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.FilterByLanguageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByLanguageTextBox.Location = new System.Drawing.Point(130, 85);
            this.FilterByLanguageTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByLanguageTextBox.Name = "FilterByLanguageTextBox";
            this.FilterByLanguageTextBox.Size = new System.Drawing.Size(160, 26);
            this.FilterByLanguageTextBox.TabIndex = 7;
            this.FilterByLanguageTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByLanguageTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByLanguageTextBox, "FilterByLanguageTextBox");
            //
            // FilterByDependsLabel
            //
            this.FilterByDependsLabel.AutoSize = true;
            this.FilterByDependsLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByDependsLabel.Location = new System.Drawing.Point(6, 113);
            this.FilterByDependsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByDependsLabel.Name = "FilterByDependsLabel";
            this.FilterByDependsLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByDependsLabel.TabIndex = 8;
            resources.ApplyResources(this.FilterByDependsLabel, "FilterByDependsLabel");
            //
            // FilterByDependsTextBox
            //
            this.FilterByDependsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.FilterByDependsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByDependsTextBox.Location = new System.Drawing.Point(130, 111);
            this.FilterByDependsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByDependsTextBox.Name = "FilterByDependsTextBox";
            this.FilterByDependsTextBox.Size = new System.Drawing.Size(160, 26);
            this.FilterByDependsTextBox.TabIndex = 9;
            this.FilterByDependsTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByDependsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByDependsTextBox, "FilterByDependsTextBox");
            //
            // FilterByRecommendsLabel
            //
            this.FilterByRecommendsLabel.AutoSize = true;
            this.FilterByRecommendsLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByRecommendsLabel.Location = new System.Drawing.Point(6, 139);
            this.FilterByRecommendsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByRecommendsLabel.Name = "FilterByRecommendsLabel";
            this.FilterByRecommendsLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByRecommendsLabel.TabIndex = 10;
            resources.ApplyResources(this.FilterByRecommendsLabel, "FilterByRecommendsLabel");
            //
            // FilterByRecommendsTextBox
            //
            this.FilterByRecommendsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.FilterByRecommendsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByRecommendsTextBox.Location = new System.Drawing.Point(130, 137);
            this.FilterByRecommendsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByRecommendsTextBox.Name = "FilterByRecommendsTextBox";
            this.FilterByRecommendsTextBox.Size = new System.Drawing.Size(160, 26);
            this.FilterByRecommendsTextBox.TabIndex = 11;
            this.FilterByRecommendsTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByRecommendsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByRecommendsTextBox, "FilterByRecommendsTextBox");
            //
            // FilterBySuggestsLabel
            //
            this.FilterBySuggestsLabel.AutoSize = true;
            this.FilterBySuggestsLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterBySuggestsLabel.Location = new System.Drawing.Point(6, 165);
            this.FilterBySuggestsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterBySuggestsLabel.Name = "FilterBySuggestsLabel";
            this.FilterBySuggestsLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterBySuggestsLabel.TabIndex = 12;
            resources.ApplyResources(this.FilterBySuggestsLabel, "FilterBySuggestsLabel");
            //
            // FilterBySuggestsTextBox
            //
            this.FilterBySuggestsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.FilterBySuggestsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterBySuggestsTextBox.Location = new System.Drawing.Point(130, 163);
            this.FilterBySuggestsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterBySuggestsTextBox.Name = "FilterBySuggestsTextBox";
            this.FilterBySuggestsTextBox.Size = new System.Drawing.Size(160, 26);
            this.FilterBySuggestsTextBox.TabIndex = 13;
            this.FilterBySuggestsTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterBySuggestsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterBySuggestsTextBox, "FilterBySuggestsTextBox");
            //
            // FilterByConflictsLabel
            //
            this.FilterByConflictsLabel.AutoSize = true;
            this.FilterByConflictsLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByConflictsLabel.Location = new System.Drawing.Point(6, 191);
            this.FilterByConflictsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByConflictsLabel.Name = "FilterByConflictsLabel";
            this.FilterByConflictsLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByConflictsLabel.TabIndex = 14;
            resources.ApplyResources(this.FilterByConflictsLabel, "FilterByConflictsLabel");
            //
            // FilterByConflictsTextBox
            //
            this.FilterByConflictsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.FilterByConflictsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByConflictsTextBox.Location = new System.Drawing.Point(130, 189);
            this.FilterByConflictsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByConflictsTextBox.Name = "FilterByConflictsTextBox";
            this.FilterByConflictsTextBox.Size = new System.Drawing.Size(160, 26);
            this.FilterByConflictsTextBox.TabIndex = 15;
            this.FilterByConflictsTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByConflictsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByConflictsTextBox, "FilterByConflictsTextBox");
            //
            // FilterByTagsLabel
            //
            this.FilterByTagsLabel.AutoSize = true;
            this.FilterByTagsLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByTagsLabel.Location = new System.Drawing.Point(6, 217);
            this.FilterByTagsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByTagsLabel.Name = "FilterByTagsLabel";
            this.FilterByTagsLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByTagsLabel.TabIndex = 14;
            resources.ApplyResources(this.FilterByTagsLabel, "FilterByTagsLabel");
            //
            // FilterByTagsTextBox
            //
            this.FilterByTagsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.FilterByTagsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByTagsTextBox.Location = new System.Drawing.Point(130, 215);
            this.FilterByTagsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByTagsTextBox.Name = "FilterByTagsTextBox";
            this.FilterByTagsTextBox.Size = new System.Drawing.Size(160, 26);
            this.FilterByTagsTextBox.TabIndex = 15;
            this.FilterByTagsTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByTagsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByTagsTextBox, "FilterByTagsTextBox");
            //
            // FilterByLabelsLabel
            //
            this.FilterByLabelsLabel.AutoSize = true;
            this.FilterByLabelsLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByLabelsLabel.Location = new System.Drawing.Point(6, 243);
            this.FilterByLabelsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByLabelsLabel.Name = "FilterByLabelsLabel";
            this.FilterByLabelsLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByLabelsLabel.TabIndex = 14;
            resources.ApplyResources(this.FilterByLabelsLabel, "FilterByLabelsLabel");
            //
            // FilterByLabelsTextBox
            //
            this.FilterByLabelsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.FilterByLabelsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByLabelsTextBox.Location = new System.Drawing.Point(130, 241);
            this.FilterByLabelsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByLabelsTextBox.Name = "FilterByLabelsTextBox";
            this.FilterByLabelsTextBox.Size = new System.Drawing.Size(160, 26);
            this.FilterByLabelsTextBox.TabIndex = 15;
            this.FilterByLabelsTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByLabelsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByLabelsTextBox, "FilterByLabelsTextBox");
            //
            // CompatibleLabel
            //
            this.CompatibleLabel.AutoSize = true;
            this.CompatibleLabel.BackColor = System.Drawing.Color.Transparent;
            this.CompatibleLabel.Location = new System.Drawing.Point(6, 269);
            this.CompatibleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CompatibleLabel.Name = "CompatibleLabel";
            this.CompatibleLabel.Size = new System.Drawing.Size(149, 20);
            this.CompatibleLabel.TabIndex = 14;
            resources.ApplyResources(this.CompatibleLabel, "CompatibleLabel");
            //
            // CompatibleToggle
            //
            this.CompatibleToggle.Location = new System.Drawing.Point(130, 267);
            this.CompatibleToggle.Changed += TriStateChanged;
            //
            // InstalledLabel
            //
            this.InstalledLabel.AutoSize = true;
            this.InstalledLabel.BackColor = System.Drawing.Color.Transparent;
            this.InstalledLabel.Location = new System.Drawing.Point(6, 295);
            this.InstalledLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.InstalledLabel.Name = "InstalledLabel";
            this.InstalledLabel.Size = new System.Drawing.Size(149, 20);
            this.InstalledLabel.TabIndex = 14;
            resources.ApplyResources(this.InstalledLabel, "InstalledLabel");
            //
            // InstalledToggle
            //
            this.InstalledToggle.Location = new System.Drawing.Point(130, 293);
            this.InstalledToggle.Changed += TriStateChanged;
            //
            // CachedLabel
            //
            this.CachedLabel.AutoSize = true;
            this.CachedLabel.BackColor = System.Drawing.Color.Transparent;
            this.CachedLabel.Location = new System.Drawing.Point(6, 321);
            this.CachedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CachedLabel.Name = "CachedLabel";
            this.CachedLabel.Size = new System.Drawing.Size(149, 20);
            this.CachedLabel.TabIndex = 14;
            resources.ApplyResources(this.CachedLabel, "CachedLabel");
            //
            // CachedToggle
            //
            this.CachedToggle.Location = new System.Drawing.Point(130, 319);
            this.CachedToggle.Changed += TriStateChanged;
            //
            // NewlyCompatibleLabel
            //
            this.NewlyCompatibleLabel.AutoSize = true;
            this.NewlyCompatibleLabel.BackColor = System.Drawing.Color.Transparent;
            this.NewlyCompatibleLabel.Location = new System.Drawing.Point(6, 347);
            this.NewlyCompatibleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.NewlyCompatibleLabel.Name = "NewlyCompatibleLabel";
            this.NewlyCompatibleLabel.Size = new System.Drawing.Size(149, 20);
            this.NewlyCompatibleLabel.TabIndex = 14;
            resources.ApplyResources(this.NewlyCompatibleLabel, "NewlyCompatibleLabel");
            //
            // NewlyCompatibleToggle
            //
            this.NewlyCompatibleToggle.Location = new System.Drawing.Point(130, 345);
            this.NewlyCompatibleToggle.Changed += TriStateChanged;
            //
            // UpgradeableLabel
            //
            this.UpgradeableLabel.AutoSize = true;
            this.UpgradeableLabel.BackColor = System.Drawing.Color.Transparent;
            this.UpgradeableLabel.Location = new System.Drawing.Point(6, 373);
            this.UpgradeableLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.UpgradeableLabel.Name = "UpgradeableLabel";
            this.UpgradeableLabel.Size = new System.Drawing.Size(149, 20);
            this.UpgradeableLabel.TabIndex = 14;
            resources.ApplyResources(this.UpgradeableLabel, "UpgradeableLabel");
            //
            // UpgradeableToggle
            //
            this.UpgradeableToggle.Location = new System.Drawing.Point(130, 371);
            this.UpgradeableToggle.Changed += TriStateChanged;
            //
            // ReplaceableLabel
            //
            this.ReplaceableLabel.AutoSize = true;
            this.ReplaceableLabel.BackColor = System.Drawing.Color.Transparent;
            this.ReplaceableLabel.Location = new System.Drawing.Point(6, 399);
            this.ReplaceableLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ReplaceableLabel.Name = "ReplaceableLabel";
            this.ReplaceableLabel.Size = new System.Drawing.Size(149, 20);
            this.ReplaceableLabel.TabIndex = 14;
            resources.ApplyResources(this.ReplaceableLabel, "ReplaceableLabel");
            //
            // ReplaceableToggle
            //
            this.ReplaceableToggle.Location = new System.Drawing.Point(130, 397);
            this.ReplaceableToggle.Changed += TriStateChanged;
            //
            // EditModSearchDetails
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.FilterByAuthorTextBox);
            this.Controls.Add(this.FilterByAuthorLabel);
            this.Controls.Add(this.FilterByNameLabel);
            this.Controls.Add(this.FilterByNameTextBox);
            this.Controls.Add(this.FilterByDescriptionLabel);
            this.Controls.Add(this.FilterByDescriptionTextBox);
            this.Controls.Add(this.FilterByLanguageLabel);
            this.Controls.Add(this.FilterByLanguageTextBox);
            this.Controls.Add(this.FilterByDependsLabel);
            this.Controls.Add(this.FilterByDependsTextBox);
            this.Controls.Add(this.FilterByRecommendsLabel);
            this.Controls.Add(this.FilterByRecommendsTextBox);
            this.Controls.Add(this.FilterBySuggestsLabel);
            this.Controls.Add(this.FilterBySuggestsTextBox);
            this.Controls.Add(this.FilterByConflictsLabel);
            this.Controls.Add(this.FilterByConflictsTextBox);
            this.Controls.Add(this.FilterByTagsLabel);
            this.Controls.Add(this.FilterByTagsTextBox);
            this.Controls.Add(this.FilterByLabelsLabel);
            this.Controls.Add(this.FilterByLabelsTextBox);
            this.Controls.Add(this.CompatibleLabel);
            this.Controls.Add(this.CompatibleToggle);
            this.Controls.Add(this.InstalledLabel);
            this.Controls.Add(this.InstalledToggle);
            this.Controls.Add(this.CachedLabel);
            this.Controls.Add(this.CachedToggle);
            this.Controls.Add(this.NewlyCompatibleLabel);
            this.Controls.Add(this.NewlyCompatibleToggle);
            this.Controls.Add(this.UpgradeableLabel);
            this.Controls.Add(this.UpgradeableToggle);
            this.Controls.Add(this.ReplaceableLabel);
            this.Controls.Add(this.ReplaceableToggle);
            this.Name = "EditModSearchDetails";
            this.Size = new System.Drawing.Size(300, 434);
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label FilterByNameLabel;
        internal CKAN.GUI.HintTextBox FilterByNameTextBox;
        private System.Windows.Forms.Label FilterByAuthorLabel;
        internal CKAN.GUI.HintTextBox FilterByAuthorTextBox;
        private System.Windows.Forms.Label FilterByDescriptionLabel;
        internal CKAN.GUI.HintTextBox FilterByDescriptionTextBox;
        private System.Windows.Forms.Label FilterByLanguageLabel;
        internal CKAN.GUI.HintTextBox FilterByLanguageTextBox;
        private System.Windows.Forms.Label FilterByDependsLabel;
        internal CKAN.GUI.HintTextBox FilterByDependsTextBox;
        private System.Windows.Forms.Label FilterByRecommendsLabel;
        internal CKAN.GUI.HintTextBox FilterByRecommendsTextBox;
        private System.Windows.Forms.Label FilterBySuggestsLabel;
        internal CKAN.GUI.HintTextBox FilterBySuggestsTextBox;
        private System.Windows.Forms.Label FilterByConflictsLabel;
        internal CKAN.GUI.HintTextBox FilterByConflictsTextBox;
        private System.Windows.Forms.Label FilterByTagsLabel;
        internal CKAN.GUI.HintTextBox FilterByTagsTextBox;
        private System.Windows.Forms.Label FilterByLabelsLabel;
        internal CKAN.GUI.HintTextBox FilterByLabelsTextBox;
        private System.Windows.Forms.Label CompatibleLabel;
        internal CKAN.GUI.TriStateToggle CompatibleToggle;
        private System.Windows.Forms.Label InstalledLabel;
        internal CKAN.GUI.TriStateToggle InstalledToggle;
        private System.Windows.Forms.Label CachedLabel;
        internal CKAN.GUI.TriStateToggle CachedToggle;
        private System.Windows.Forms.Label NewlyCompatibleLabel;
        internal CKAN.GUI.TriStateToggle NewlyCompatibleToggle;
        private System.Windows.Forms.Label UpgradeableLabel;
        internal CKAN.GUI.TriStateToggle UpgradeableToggle;
        private System.Windows.Forms.Label ReplaceableLabel;
        internal CKAN.GUI.TriStateToggle ReplaceableToggle;
    }
}
