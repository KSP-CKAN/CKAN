namespace CKAN
{
    partial class EditModSearch
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(EditModSearch));
            this.ToolTip = new System.Windows.Forms.ToolTip();
            this.FilterByNameLabel = new System.Windows.Forms.Label();
            this.FilterByNameTextBox = new CKAN.HintTextBox();
            this.FilterByAuthorLabel = new System.Windows.Forms.Label();
            this.FilterByAuthorTextBox = new CKAN.HintTextBox();
            this.FilterByDescriptionLabel = new System.Windows.Forms.Label();
            this.FilterByDescriptionTextBox = new CKAN.HintTextBox();
            this.FilterByLanguageLabel = new System.Windows.Forms.Label();
            this.FilterByLanguageTextBox = new CKAN.HintTextBox();
            this.FilterByDependsLabel = new System.Windows.Forms.Label();
            this.FilterByDependsTextBox = new CKAN.HintTextBox();
            this.FilterByRecommendsLabel = new System.Windows.Forms.Label();
            this.FilterByRecommendsTextBox = new CKAN.HintTextBox();
            this.FilterBySuggestsLabel = new System.Windows.Forms.Label();
            this.FilterBySuggestsTextBox = new CKAN.HintTextBox();
            this.FilterByConflictsLabel = new System.Windows.Forms.Label();
            this.FilterByConflictsTextBox = new CKAN.HintTextBox();
            this.FilterCombinedLabel = new System.Windows.Forms.Label();
            this.FilterCombinedTextBox = new CKAN.HintTextBox();
            this.ExpandButton = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // ToolTip
            // 
            this.ToolTip.AutoPopDelay = 10000;
            this.ToolTip.InitialDelay = 250;
            this.ToolTip.ReshowDelay = 250;
            this.ToolTip.ShowAlways = true;
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
            this.FilterByNameLabel.Visible = false;
            resources.ApplyResources(this.FilterByNameLabel, "FilterByNameLabel");
            //
            // FilterByNameTextBox
            //
            this.FilterByNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByNameTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByNameTextBox.Name = "FilterByNameTextBox";
            this.FilterByNameTextBox.Size = new System.Drawing.Size(170, 26);
            this.FilterByNameTextBox.TabIndex = 1;
            this.FilterByNameTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            this.FilterByNameTextBox.Visible = false;
            resources.ApplyResources(this.FilterByNameTextBox, "FilterByNameTextBox");
            //
            // FilterByAuthorLabel
            //
            this.FilterByAuthorLabel.AutoSize = true;
            this.FilterByAuthorLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByAuthorLabel.Location = new System.Drawing.Point(372, 9);
            this.FilterByAuthorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByAuthorLabel.Name = "FilterByAuthorLabel";
            this.FilterByAuthorLabel.Size = new System.Drawing.Size(162, 20);
            this.FilterByAuthorLabel.TabIndex = 2;
            this.FilterByAuthorLabel.Visible = false;
            resources.ApplyResources(this.FilterByAuthorLabel, "FilterByAuthorLabel");
            //
            // FilterByAuthorTextBox
            //
            this.FilterByAuthorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByAuthorTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByAuthorTextBox.Name = "FilterByAuthorTextBox";
            this.FilterByAuthorTextBox.Size = new System.Drawing.Size(170, 26);
            this.FilterByAuthorTextBox.TabIndex = 3;
            this.FilterByAuthorTextBox.Visible = false;
            this.FilterByAuthorTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByAuthorTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByAuthorTextBox, "FilterByAuthorTextBox");
            //
            // FilterByDescriptionLabel
            //
            this.FilterByDescriptionLabel.AutoSize = true;
            this.FilterByDescriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByDescriptionLabel.Location = new System.Drawing.Point(754, 9);
            this.FilterByDescriptionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByDescriptionLabel.Name = "FilterByDescriptionLabel";
            this.FilterByDescriptionLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByDescriptionLabel.TabIndex = 4;
            this.FilterByDescriptionLabel.Visible = false;
            resources.ApplyResources(this.FilterByDescriptionLabel, "FilterByDescriptionLabel");
            //
            // FilterByDescriptionTextBox
            //
            this.FilterByDescriptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByDescriptionTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByDescriptionTextBox.Name = "FilterByDescriptionTextBox";
            this.FilterByDescriptionTextBox.Size = new System.Drawing.Size(170, 26);
            this.FilterByDescriptionTextBox.TabIndex = 5;
            this.FilterByDescriptionTextBox.Visible = false;
            this.FilterByDescriptionTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByDescriptionTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByDescriptionTextBox, "FilterByDescriptionTextBox");
            //
            // FilterByLanguageLabel
            //
            this.FilterByLanguageLabel.AutoSize = true;
            this.FilterByLanguageLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByLanguageLabel.Location = new System.Drawing.Point(754, 9);
            this.FilterByLanguageLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByLanguageLabel.Name = "FilterByLanguageLabel";
            this.FilterByLanguageLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByLanguageLabel.TabIndex = 6;
            this.FilterByLanguageLabel.Visible = false;
            resources.ApplyResources(this.FilterByLanguageLabel, "FilterByLanguageLabel");
            //
            // FilterByLanguageTextBox
            //
            this.FilterByLanguageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByLanguageTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByLanguageTextBox.Name = "FilterByLanguageTextBox";
            this.FilterByLanguageTextBox.Size = new System.Drawing.Size(170, 26);
            this.FilterByLanguageTextBox.TabIndex = 7;
            this.FilterByLanguageTextBox.Visible = false;
            this.FilterByLanguageTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByLanguageTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByLanguageTextBox, "FilterByLanguageTextBox");
            //
            // FilterByDependsLabel
            //
            this.FilterByDependsLabel.AutoSize = true;
            this.FilterByDependsLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByDependsLabel.Location = new System.Drawing.Point(754, 9);
            this.FilterByDependsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByDependsLabel.Name = "FilterByDependsLabel";
            this.FilterByDependsLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByDependsLabel.TabIndex = 8;
            this.FilterByDependsLabel.Visible = false;
            resources.ApplyResources(this.FilterByDependsLabel, "FilterByDependsLabel");
            //
            // FilterByDependsTextBox
            //
            this.FilterByDependsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByDependsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByDependsTextBox.Name = "FilterByDependsTextBox";
            this.FilterByDependsTextBox.Size = new System.Drawing.Size(170, 26);
            this.FilterByDependsTextBox.TabIndex = 9;
            this.FilterByDependsTextBox.Visible = false;
            this.FilterByDependsTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByDependsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByDependsTextBox, "FilterByDependsTextBox");
            //
            // FilterByRecommendsLabel
            //
            this.FilterByRecommendsLabel.AutoSize = true;
            this.FilterByRecommendsLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByRecommendsLabel.Location = new System.Drawing.Point(754, 9);
            this.FilterByRecommendsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByRecommendsLabel.Name = "FilterByRecommendsLabel";
            this.FilterByRecommendsLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByRecommendsLabel.TabIndex = 10;
            this.FilterByRecommendsLabel.Visible = false;
            resources.ApplyResources(this.FilterByRecommendsLabel, "FilterByRecommendsLabel");
            //
            // FilterByRecommendsTextBox
            //
            this.FilterByRecommendsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByRecommendsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByRecommendsTextBox.Name = "FilterByRecommendsTextBox";
            this.FilterByRecommendsTextBox.Size = new System.Drawing.Size(170, 26);
            this.FilterByRecommendsTextBox.TabIndex = 11;
            this.FilterByRecommendsTextBox.Visible = false;
            this.FilterByRecommendsTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByRecommendsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByRecommendsTextBox, "FilterByRecommendsTextBox");
            //
            // FilterBySuggestsLabel
            //
            this.FilterBySuggestsLabel.AutoSize = true;
            this.FilterBySuggestsLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterBySuggestsLabel.Location = new System.Drawing.Point(754, 9);
            this.FilterBySuggestsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterBySuggestsLabel.Name = "FilterBySuggestsLabel";
            this.FilterBySuggestsLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterBySuggestsLabel.TabIndex = 12;
            this.FilterBySuggestsLabel.Visible = false;
            resources.ApplyResources(this.FilterBySuggestsLabel, "FilterBySuggestsLabel");
            //
            // FilterBySuggestsTextBox
            //
            this.FilterBySuggestsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterBySuggestsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterBySuggestsTextBox.Name = "FilterBySuggestsTextBox";
            this.FilterBySuggestsTextBox.Size = new System.Drawing.Size(170, 26);
            this.FilterBySuggestsTextBox.TabIndex = 13;
            this.FilterBySuggestsTextBox.Visible = false;
            this.FilterBySuggestsTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterBySuggestsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterBySuggestsTextBox, "FilterBySuggestsTextBox");
            //
            // FilterByConflictsLabel
            //
            this.FilterByConflictsLabel.AutoSize = true;
            this.FilterByConflictsLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterByConflictsLabel.Location = new System.Drawing.Point(754, 9);
            this.FilterByConflictsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterByConflictsLabel.Name = "FilterByConflictsLabel";
            this.FilterByConflictsLabel.Size = new System.Drawing.Size(149, 20);
            this.FilterByConflictsLabel.TabIndex = 14;
            this.FilterByConflictsLabel.Visible = false;
            resources.ApplyResources(this.FilterByConflictsLabel, "FilterByConflictsLabel");
            //
            // FilterByConflictsTextBox
            //
            this.FilterByConflictsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterByConflictsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByConflictsTextBox.Name = "FilterByConflictsTextBox";
            this.FilterByConflictsTextBox.Size = new System.Drawing.Size(170, 26);
            this.FilterByConflictsTextBox.TabIndex = 15;
            this.FilterByConflictsTextBox.Visible = false;
            this.FilterByConflictsTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByConflictsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByConflictsTextBox, "FilterByConflictsTextBox");
            //
            // FilterCombinedLabel
            //
            this.FilterCombinedLabel.AutoSize = true;
            this.FilterCombinedLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterCombinedLabel.Location = new System.Drawing.Point(6, 9);
            this.FilterCombinedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterCombinedLabel.Name = "FilterCombinedLabel";
            this.FilterCombinedLabel.Size = new System.Drawing.Size(147, 20);
            this.FilterCombinedLabel.TabIndex = 16;
            resources.ApplyResources(this.FilterCombinedLabel, "FilterCombinedLabel");
            //
            // FilterCombinedTextBox
            //
            this.FilterCombinedTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterCombinedTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterCombinedTextBox.Name = "FilterCombinedTextBox";
            this.FilterCombinedTextBox.Size = new System.Drawing.Size(340, 26);
            this.FilterCombinedTextBox.TabIndex = 17;
            this.FilterCombinedTextBox.TextChanged += new System.EventHandler(this.FilterCombinedTextBox_TextChanged);
            this.FilterCombinedTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterCombinedTextBox, "FilterCombinedTextBox");
            // 
            // ExpandButton
            // 
            this.ExpandButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.ExpandButton.Location = new System.Drawing.Point(330, 5);
            this.ExpandButton.Name = "ExpandButton";
            this.ExpandButton.Size = new System.Drawing.Size(20, 26);
            this.ExpandButton.TabIndex = 18;
            this.ExpandButton.Text = "▴";
            this.ExpandButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ExpandButton.UseVisualStyleBackColor = true;
            this.ExpandButton.CheckedChanged += new System.EventHandler(this.ExpandButton_CheckedChanged);
            resources.ApplyResources(this.ExpandButton, "ExpandButton");
            //
            // EditModSearch
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.FilterCombinedLabel);
            this.Controls.Add(this.FilterCombinedTextBox);
            this.Controls.Add(this.ExpandButton);
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
            this.Name = "EditModSearch";
            this.Size = new System.Drawing.Size(500, 26);
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Label FilterByNameLabel;
        private CKAN.HintTextBox FilterByNameTextBox;
        private System.Windows.Forms.Label FilterByAuthorLabel;
        private CKAN.HintTextBox FilterByAuthorTextBox;
        private System.Windows.Forms.Label FilterByDescriptionLabel;
        private CKAN.HintTextBox FilterByDescriptionTextBox;
        private System.Windows.Forms.Label FilterByLanguageLabel;
        private CKAN.HintTextBox FilterByLanguageTextBox;
        private System.Windows.Forms.Label FilterByDependsLabel;
        private CKAN.HintTextBox FilterByDependsTextBox;
        private System.Windows.Forms.Label FilterByRecommendsLabel;
        private CKAN.HintTextBox FilterByRecommendsTextBox;
        private System.Windows.Forms.Label FilterBySuggestsLabel;
        private CKAN.HintTextBox FilterBySuggestsTextBox;
        private System.Windows.Forms.Label FilterByConflictsLabel;
        private CKAN.HintTextBox FilterByConflictsTextBox;
        private System.Windows.Forms.Label FilterCombinedLabel;
        private CKAN.HintTextBox FilterCombinedTextBox;
        private System.Windows.Forms.CheckBox ExpandButton;
    }
}
