namespace CKAN.GUI
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
                SearchDetails.Dispose();
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
            this.FilterCombinedLabel = new System.Windows.Forms.Label();
            this.FilterOrLabel = new System.Windows.Forms.Label();
            this.FilterCombinedTextBox = new CKAN.GUI.HintTextBox();
            this.ExpandButton = new System.Windows.Forms.CheckBox();
            this.SearchDetails = new CKAN.GUI.EditModSearchDetails();
            this.SuspendLayout();
            //
            // ToolTip
            //
            this.ToolTip.AutoPopDelay = 10000;
            this.ToolTip.InitialDelay = 250;
            this.ToolTip.ReshowDelay = 250;
            this.ToolTip.ShowAlways = true;
            //
            // FilterCombinedLabel
            //
            this.FilterCombinedLabel.AutoSize = true;
            this.FilterCombinedLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterCombinedLabel.Location = new System.Drawing.Point(80, 4);
            this.FilterCombinedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterCombinedLabel.Name = "FilterCombinedLabel";
            this.FilterCombinedLabel.Size = new System.Drawing.Size(147, 20);
            this.FilterCombinedLabel.TabIndex = 16;
            resources.ApplyResources(this.FilterCombinedLabel, "FilterCombinedLabel");
            //
            // FilterOrLabel
            //
            this.FilterOrLabel.AutoSize = true;
            this.FilterOrLabel.BackColor = System.Drawing.Color.Transparent;
            this.FilterOrLabel.Location = new System.Drawing.Point(80, 4);
            this.FilterOrLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FilterOrLabel.Name = "FilterOrLabel";
            this.FilterOrLabel.Size = new System.Drawing.Size(147, 20);
            this.FilterOrLabel.TabIndex = 16;
            this.FilterOrLabel.Visible = false;
            resources.ApplyResources(this.FilterOrLabel, "FilterOrLabel");
            //
            // FilterCombinedTextBox
            //
            this.FilterCombinedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.FilterCombinedTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FilterCombinedTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FilterCombinedTextBox.Name = "FilterCombinedTextBox";
            this.FilterCombinedTextBox.Location = new System.Drawing.Point(154, 2);
            this.FilterCombinedTextBox.MinimumSize = new System.Drawing.Size(60, 20);
            this.FilterCombinedTextBox.Size = new System.Drawing.Size(247, 26);
            this.FilterCombinedTextBox.TabIndex = 17;
            this.FilterCombinedTextBox.Enter += new System.EventHandler(this.FilterTextBox_Enter);
            resources.ApplyResources(this.FilterCombinedTextBox, "FilterCombinedTextBox");
            //
            // ExpandButton
            //
            this.ExpandButton.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Right);
            this.ExpandButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.ExpandButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ExpandButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ExpandButton.Location = new System.Drawing.Point(400, 0);
            this.ExpandButton.Name = "ExpandButton";
            this.ExpandButton.Size = new System.Drawing.Size(20, 26);
            this.ExpandButton.TabIndex = 18;
            this.ExpandButton.Text = "▾";
            this.ExpandButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ExpandButton.UseVisualStyleBackColor = true;
            this.ExpandButton.CheckedChanged += new System.EventHandler(this.ExpandButton_CheckedChanged);
            resources.ApplyResources(this.ExpandButton, "ExpandButton");
            //
            // SearchDetails
            //
            this.SearchDetails.Name = "SearchDetails";
            this.SearchDetails.Visible = false;
            this.SearchDetails.ApplySearch += this.SearchDetails_ApplySearch;
            this.SearchDetails.SurrenderFocus += this.SearchDetails_SurrenderFocus;
            //
            // EditModSearch
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.FilterCombinedLabel);
            this.Controls.Add(this.FilterOrLabel);
            this.Controls.Add(this.ExpandButton);
            this.Controls.Add(this.FilterCombinedTextBox);
            this.Name = "EditModSearch";
            this.Size = new System.Drawing.Size(500, 29);
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Label FilterCombinedLabel;
        private System.Windows.Forms.Label FilterOrLabel;
        private CKAN.GUI.HintTextBox FilterCombinedTextBox;
        private System.Windows.Forms.CheckBox ExpandButton;
        private CKAN.GUI.EditModSearchDetails SearchDetails;
    }
}
