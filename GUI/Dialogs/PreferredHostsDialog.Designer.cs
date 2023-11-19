namespace CKAN.GUI
{
    partial class PreferredHostsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(PreferredHostsDialog));
            this.ToolTip = new System.Windows.Forms.ToolTip();
            this.ExplanationLabel = new System.Windows.Forms.Label();
            this.Splitter = new System.Windows.Forms.SplitContainer();
            this.AvailableHostsLabel = new System.Windows.Forms.Label();
            this.AvailableHostsListBox = new System.Windows.Forms.ListBox();
            this.MoveRightButton = new System.Windows.Forms.Button();
            this.MoveLeftButton = new System.Windows.Forms.Button();
            this.PreferredHostsLabel = new System.Windows.Forms.Label();
            this.PreferredHostsListBox = new System.Windows.Forms.ListBox();
            this.MoveUpButton = new System.Windows.Forms.Button();
            this.MoveDownButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.Splitter)).BeginInit();
            this.Splitter.Panel1.SuspendLayout();
            this.Splitter.Panel2.SuspendLayout();
            this.Splitter.SuspendLayout();
            this.SuspendLayout();
            //
            // ToolTip
            //
            this.ToolTip.AutoPopDelay = 10000;
            this.ToolTip.InitialDelay = 250;
            this.ToolTip.ReshowDelay = 250;
            this.ToolTip.ShowAlways = true;
            //
            // ExplanationLabel
            //
            this.ExplanationLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ExplanationLabel.Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.Name, 12, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.ExplanationLabel.Location = new System.Drawing.Point(5, 0);
            this.ExplanationLabel.Name = "ExplanationLabel";
            this.ExplanationLabel.Padding = new System.Windows.Forms.Padding(10, 10, 10, 10);
            this.ExplanationLabel.Size = new System.Drawing.Size(490, 60);
            resources.ApplyResources(this.ExplanationLabel, "ExplanationLabel");
            //
            // Splitter
            //
            this.Splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Splitter.Location = new System.Drawing.Point(0, 0);
            this.Splitter.Name = "Splitter";
            this.Splitter.Size = new System.Drawing.Size(534, 300);
            this.Splitter.SplitterDistance = 262;
            this.Splitter.SplitterWidth = 10;
            this.Splitter.TabIndex = 0;
            //
            // Splitter.Panel1
            //
            this.Splitter.Panel1.Controls.Add(this.AvailableHostsLabel);
            this.Splitter.Panel1.Controls.Add(this.AvailableHostsListBox);
            this.Splitter.Panel1.Controls.Add(this.MoveRightButton);
            this.Splitter.Panel1.Controls.Add(this.MoveLeftButton);
            this.Splitter.Panel1MinSize = 200;
            //
            // Splitter.Panel2
            //
            this.Splitter.Panel2.Controls.Add(this.PreferredHostsLabel);
            this.Splitter.Panel2.Controls.Add(this.PreferredHostsListBox);
            this.Splitter.Panel2.Controls.Add(this.MoveUpButton);
            this.Splitter.Panel2.Controls.Add(this.MoveDownButton);
            this.Splitter.Panel2MinSize = 200;
            //
            // AvailableHostsLabel
            //
            this.AvailableHostsLabel.AutoSize = true;
            this.AvailableHostsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.AvailableHostsLabel.Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.Name, 12, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.AvailableHostsLabel.Location = new System.Drawing.Point(10, 10);
            this.AvailableHostsLabel.Name = "AvailableHostsLabel";
            this.AvailableHostsLabel.Size = new System.Drawing.Size(75, 23);
            this.AvailableHostsLabel.TabIndex = 1;
            resources.ApplyResources(this.AvailableHostsLabel, "AvailableHostsLabel");
            //
            // AvailableHostsListBox
            //
            this.AvailableHostsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right))));
            this.AvailableHostsListBox.FormattingEnabled = true;
            this.AvailableHostsListBox.Location = new System.Drawing.Point(10, 35);
            this.AvailableHostsListBox.Name = "AvailableHostsListBox";
            this.AvailableHostsListBox.Size = new System.Drawing.Size(210, 255);
            this.AvailableHostsListBox.TabIndex = 2;
            this.AvailableHostsListBox.SelectedIndexChanged += new System.EventHandler(this.AvailableHostsListBox_SelectedIndexChanged);
            this.AvailableHostsListBox.DoubleClick += new System.EventHandler(this.AvailableHostsListBox_DoubleClick);
            //
            // MoveRightButton
            //
            this.MoveRightButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top
            | System.Windows.Forms.AnchorStyles.Right));
            this.MoveRightButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MoveRightButton.Location = new System.Drawing.Point(230, 35);
            this.MoveRightButton.Name = "MoveRightButton";
            this.MoveRightButton.Size = new System.Drawing.Size(32, 32);
            this.MoveRightButton.TabIndex = 3;
            this.MoveRightButton.Text = "▸";
            this.MoveRightButton.UseVisualStyleBackColor = true;
            this.MoveRightButton.Click += new System.EventHandler(this.MoveRightButton_Click);
            resources.ApplyResources(this.MoveRightButton, "MoveRightButton");
            //
            // MoveLeftButton
            //
            this.MoveLeftButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top
            | System.Windows.Forms.AnchorStyles.Right));
            this.MoveLeftButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MoveLeftButton.Location = new System.Drawing.Point(230, 72);
            this.MoveLeftButton.Name = "MoveLeftButton";
            this.MoveLeftButton.Size = new System.Drawing.Size(32, 32);
            this.MoveLeftButton.TabIndex = 4;
            this.MoveLeftButton.Text = "◂";
            this.MoveLeftButton.UseVisualStyleBackColor = true;
            this.MoveLeftButton.Click += new System.EventHandler(this.MoveLeftButton_Click);
            resources.ApplyResources(this.MoveLeftButton, "MoveLeftButton");
            //
            // PreferredHostsLabel
            //
            this.PreferredHostsLabel.AutoSize = true;
            this.PreferredHostsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
            this.PreferredHostsLabel.Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.Name, 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.PreferredHostsLabel.Location = new System.Drawing.Point(0, 10);
            this.PreferredHostsLabel.Name = "PreferredHostsLabel";
            this.PreferredHostsLabel.Size = new System.Drawing.Size(75, 23);
            this.PreferredHostsLabel.TabIndex = 5;
            resources.ApplyResources(this.PreferredHostsLabel, "PreferredHostsLabel");
            //
            // PreferredHostsListBox
            //
            this.PreferredHostsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PreferredHostsListBox.FormattingEnabled = true;
            this.PreferredHostsListBox.Location = new System.Drawing.Point(0, 35);
            this.PreferredHostsListBox.Name = "PreferredHostsListBox";
            this.PreferredHostsListBox.Size = new System.Drawing.Size(216, 255);
            this.PreferredHostsListBox.TabIndex = 6;
            this.PreferredHostsListBox.SelectedIndexChanged += new System.EventHandler(this.PreferredHostsListBox_SelectedIndexChanged);
            this.PreferredHostsListBox.DoubleClick += new System.EventHandler(this.PreferredHostsListBox_DoubleClick);
            //
            // MoveUpButton
            //
            this.MoveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top
            | System.Windows.Forms.AnchorStyles.Right));
            this.MoveUpButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MoveUpButton.Location = new System.Drawing.Point(226, 35);
            this.MoveUpButton.Name = "MoveUpButton";
            this.MoveUpButton.Size = new System.Drawing.Size(32, 32);
            this.MoveUpButton.TabIndex = 7;
            this.MoveUpButton.Text = "▴";
            this.MoveUpButton.UseVisualStyleBackColor = true;
            this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
            resources.ApplyResources(this.MoveUpButton, "MoveUpButton");
            //
            // MoveDownButton
            //
            this.MoveDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top
            | System.Windows.Forms.AnchorStyles.Right));
            this.MoveDownButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MoveDownButton.Location = new System.Drawing.Point(226, 72);
            this.MoveDownButton.Name = "MoveDownButton";
            this.MoveDownButton.Size = new System.Drawing.Size(32, 32);
            this.MoveDownButton.TabIndex = 8;
            this.MoveDownButton.Text = "▾";
            this.MoveDownButton.UseVisualStyleBackColor = true;
            this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButton_Click);
            resources.ApplyResources(this.MoveDownButton, "MoveDownButton");
            //
            // PreferredHostsDialog
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(534, 360);
            this.Controls.Add(this.Splitter);
            this.Controls.Add(this.ExplanationLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Icon = EmbeddedImages.AppIcon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(480, 300);
            this.HelpButton = true;
            this.Name = "PreferredHostsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.PreferredHostsDialog_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.PreferredHostsDialog_Closing);
            resources.ApplyResources(this, "$this");
            this.Splitter.Panel1.ResumeLayout(false);
            this.Splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Splitter)).EndInit();
            this.Splitter.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Label ExplanationLabel;
        private System.Windows.Forms.SplitContainer Splitter;
        private System.Windows.Forms.Label AvailableHostsLabel;
        private System.Windows.Forms.ListBox AvailableHostsListBox;
        private System.Windows.Forms.Button MoveRightButton;
        private System.Windows.Forms.Button MoveLeftButton;
        private System.Windows.Forms.Label PreferredHostsLabel;
        private System.Windows.Forms.ListBox PreferredHostsListBox;
        private System.Windows.Forms.Button MoveUpButton;
        private System.Windows.Forms.Button MoveDownButton;
    }
}
