namespace CKAN.GUI
{
    partial class InstallFiltersDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(InstallFiltersDialog));
            this.GlobalFiltersGroupBox = new System.Windows.Forms.GroupBox();
            this.InstanceFiltersGroupBox = new System.Windows.Forms.GroupBox();
            this.GlobalFiltersTextBox = new System.Windows.Forms.TextBox();
            this.InstanceFiltersTextBox = new System.Windows.Forms.TextBox();
            this.AddMiniAVCButton = new System.Windows.Forms.Button();
            this.WarningLabel = new System.Windows.Forms.Label();
            this.GlobalFiltersGroupBox.SuspendLayout();
            this.InstanceFiltersGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // GlobalFiltersGroupBox
            //
            this.GlobalFiltersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobalFiltersGroupBox.Controls.Add(this.AddMiniAVCButton);
            this.GlobalFiltersGroupBox.Controls.Add(this.GlobalFiltersTextBox);
            this.GlobalFiltersGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GlobalFiltersGroupBox.Location = new System.Drawing.Point(12, 12);
            this.GlobalFiltersGroupBox.Name = "GlobalFiltersGroupBox";
            this.GlobalFiltersGroupBox.Size = new System.Drawing.Size(360, 173);
            this.GlobalFiltersGroupBox.TabIndex = 0;
            this.GlobalFiltersGroupBox.TabStop = false;
            resources.ApplyResources(this.GlobalFiltersGroupBox, "GlobalFiltersGroupBox");
            //
            // InstanceFiltersGroupBox
            //
            this.InstanceFiltersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InstanceFiltersGroupBox.Controls.Add(this.InstanceFiltersTextBox);
            this.InstanceFiltersGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.InstanceFiltersGroupBox.Location = new System.Drawing.Point(12, 187);
            this.InstanceFiltersGroupBox.Name = "InstanceFiltersGroupBox";
            this.InstanceFiltersGroupBox.Size = new System.Drawing.Size(360, 167);
            this.InstanceFiltersGroupBox.TabIndex = 3;
            this.InstanceFiltersGroupBox.TabStop = false;
            resources.ApplyResources(this.InstanceFiltersGroupBox, "InstanceFiltersGroupBox");
            //
            // GlobalFiltersTextBox
            //
            this.GlobalFiltersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobalFiltersTextBox.Location = new System.Drawing.Point(6, 17);
            this.GlobalFiltersTextBox.Multiline = true;
            this.GlobalFiltersTextBox.Name = "GlobalFiltersTextBox";
            this.GlobalFiltersTextBox.Size = new System.Drawing.Size(219, 147);
            this.GlobalFiltersTextBox.TabIndex = 1;
            resources.ApplyResources(this.GlobalFiltersTextBox, "GlobalFiltersTextBox");
            //
            // AddMiniAVCButton
            //
            this.AddMiniAVCButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddMiniAVCButton.Location = new System.Drawing.Point(231, 17);
            this.AddMiniAVCButton.Name = "AddMiniAVCButton";
            this.AddMiniAVCButton.Size = new System.Drawing.Size(124, 23);
            this.AddMiniAVCButton.TabIndex = 2;
            this.AddMiniAVCButton.UseVisualStyleBackColor = true;
            this.AddMiniAVCButton.Click += new System.EventHandler(this.AddMiniAVCButton_Click);
            resources.ApplyResources(this.AddMiniAVCButton, "AddMiniAVCButton");
            //
            // InstanceFiltersTextBox
            //
            this.InstanceFiltersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InstanceFiltersTextBox.Location = new System.Drawing.Point(6, 19);
            this.InstanceFiltersTextBox.Multiline = true;
            this.InstanceFiltersTextBox.Name = "InstanceFiltersTextBox";
            this.InstanceFiltersTextBox.Size = new System.Drawing.Size(219, 134);
            this.InstanceFiltersTextBox.TabIndex = 4;
            resources.ApplyResources(this.InstanceFiltersTextBox, "InstanceFiltersTextBox");
            //
            // WarningLabel
            //
            this.WarningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.WarningLabel.ForeColor = System.Drawing.Color.Red;
            this.WarningLabel.Location = new System.Drawing.Point(9, 354);
            this.WarningLabel.Name = "WarningLabel";
            this.WarningLabel.Size = new System.Drawing.Size(360, 32);
            this.WarningLabel.TabIndex = 5;
            resources.ApplyResources(this.WarningLabel, "WarningLabel");
            //
            // InstallFiltersDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 390);
            this.Controls.Add(this.GlobalFiltersGroupBox);
            this.Controls.Add(this.InstanceFiltersGroupBox);
            this.Controls.Add(this.WarningLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = EmbeddedImages.AppIcon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.HelpButton = true;
            this.Name = "InstallFiltersDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.InstallFiltersDialog_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.InstallFiltersDialog_Closing);
            resources.ApplyResources(this, "$this");
            this.GlobalFiltersGroupBox.ResumeLayout(false);
            this.InstanceFiltersGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox GlobalFiltersGroupBox;
        private System.Windows.Forms.GroupBox InstanceFiltersGroupBox;
        private System.Windows.Forms.Button AddMiniAVCButton;
        private System.Windows.Forms.TextBox GlobalFiltersTextBox;
        private System.Windows.Forms.TextBox InstanceFiltersTextBox;
        private System.Windows.Forms.Label WarningLabel;
    }
}
