namespace CKAN
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
            this.FilterByNameTextBox.Location = new System.Drawing.Point(100, 7);
            this.FilterByNameTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByNameTextBox.Name = "FilterByNameTextBox";
            this.FilterByNameTextBox.Size = new System.Drawing.Size(190, 26);
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
            this.FilterByAuthorTextBox.Location = new System.Drawing.Point(100, 33);
            this.FilterByAuthorTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByAuthorTextBox.Name = "FilterByAuthorTextBox";
            this.FilterByAuthorTextBox.Size = new System.Drawing.Size(190, 26);
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
            this.FilterByDescriptionTextBox.Location = new System.Drawing.Point(100, 59);
            this.FilterByDescriptionTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByDescriptionTextBox.Name = "FilterByDescriptionTextBox";
            this.FilterByDescriptionTextBox.Size = new System.Drawing.Size(190, 26);
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
            this.FilterByLanguageTextBox.Location = new System.Drawing.Point(100, 85);
            this.FilterByLanguageTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByLanguageTextBox.Name = "FilterByLanguageTextBox";
            this.FilterByLanguageTextBox.Size = new System.Drawing.Size(190, 26);
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
            this.FilterByDependsTextBox.Location = new System.Drawing.Point(100, 111);
            this.FilterByDependsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByDependsTextBox.Name = "FilterByDependsTextBox";
            this.FilterByDependsTextBox.Size = new System.Drawing.Size(190, 26);
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
            this.FilterByRecommendsTextBox.Location = new System.Drawing.Point(100, 137);
            this.FilterByRecommendsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByRecommendsTextBox.Name = "FilterByRecommendsTextBox";
            this.FilterByRecommendsTextBox.Size = new System.Drawing.Size(190, 26);
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
            this.FilterBySuggestsTextBox.Location = new System.Drawing.Point(100, 163);
            this.FilterBySuggestsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterBySuggestsTextBox.Name = "FilterBySuggestsTextBox";
            this.FilterBySuggestsTextBox.Size = new System.Drawing.Size(190, 26);
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
            this.FilterByConflictsTextBox.Location = new System.Drawing.Point(100, 189);
            this.FilterByConflictsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterByConflictsTextBox.Name = "FilterByConflictsTextBox";
            this.FilterByConflictsTextBox.Size = new System.Drawing.Size(190, 26);
            this.FilterByConflictsTextBox.TabIndex = 15;
            this.FilterByConflictsTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.FilterByConflictsTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            resources.ApplyResources(this.FilterByConflictsTextBox, "FilterByConflictsTextBox");
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
            this.Name = "EditModSearchDetails";
            this.Size = new System.Drawing.Size(300, 218);
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label FilterByNameLabel;
        internal CKAN.HintTextBox FilterByNameTextBox;
        private System.Windows.Forms.Label FilterByAuthorLabel;
        internal CKAN.HintTextBox FilterByAuthorTextBox;
        private System.Windows.Forms.Label FilterByDescriptionLabel;
        internal CKAN.HintTextBox FilterByDescriptionTextBox;
        private System.Windows.Forms.Label FilterByLanguageLabel;
        internal CKAN.HintTextBox FilterByLanguageTextBox;
        private System.Windows.Forms.Label FilterByDependsLabel;
        internal CKAN.HintTextBox FilterByDependsTextBox;
        private System.Windows.Forms.Label FilterByRecommendsLabel;
        internal CKAN.HintTextBox FilterByRecommendsTextBox;
        private System.Windows.Forms.Label FilterBySuggestsLabel;
        internal CKAN.HintTextBox FilterBySuggestsTextBox;
        private System.Windows.Forms.Label FilterByConflictsLabel;
        internal CKAN.HintTextBox FilterByConflictsTextBox;
    }
}
