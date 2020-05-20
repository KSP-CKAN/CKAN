namespace CKAN
{
    partial class Wait
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(Wait));
            this.TopPanel = new System.Windows.Forms.Panel();
            this.MessageTextBox = new System.Windows.Forms.TextBox();
            this.DialogProgressBar = new System.Windows.Forms.ProgressBar();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.BottomButtonPanel = new System.Windows.Forms.Panel();
            this.CancelCurrentActionButton = new System.Windows.Forms.Button();
            this.RetryCurrentActionButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TopPanel
            // 
            this.TopPanel.Controls.Add(this.MessageTextBox);
            this.TopPanel.Controls.Add(this.DialogProgressBar);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = new System.Drawing.Size(500, 85);
            // 
            // MessageTextBox
            // 
            this.MessageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));            this.MessageTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.MessageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MessageTextBox.Enabled = false;
            this.MessageTextBox.Location = new System.Drawing.Point(5, 5);
            this.MessageTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MessageTextBox.Multiline = true;
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.ReadOnly = true;
            this.MessageTextBox.Size = new System.Drawing.Size(490, 30);
            this.MessageTextBox.TabIndex = 0;
            this.MessageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            resources.ApplyResources(this.MessageTextBox, "MessageTextBox");
            // 
            // DialogProgressBar
            // 
            this.DialogProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DialogProgressBar.Location = new System.Drawing.Point(5, 45);
            this.DialogProgressBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DialogProgressBar.Minimum = 0;
            this.DialogProgressBar.Maximum = 100;
            this.DialogProgressBar.Name = "DialogProgressBar";
            this.DialogProgressBar.Size = new System.Drawing.Size(490, 25);
            this.DialogProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.DialogProgressBar.TabIndex = 1;
            // 
            // LogTextBox
            // 
            this.LogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LogTextBox.Location = new System.Drawing.Point(14, 89);
            this.LogTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(500, 400);
            this.LogTextBox.TabIndex = 2;
            // 
            // BottomButtonPanel
            // 
            this.BottomButtonPanel.Controls.Add(this.CancelCurrentActionButton);
            this.BottomButtonPanel.Controls.Add(this.RetryCurrentActionButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            this.BottomButtonPanel.Size = new System.Drawing.Size(500, 40);
            // 
            // RetryCurrentActionButton
            // 
            this.RetryCurrentActionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RetryCurrentActionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RetryCurrentActionButton.Location = new System.Drawing.Point(266, 5);
            this.RetryCurrentActionButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RetryCurrentActionButton.Name = "RetryCurrentActionButton";
            this.RetryCurrentActionButton.Size = new System.Drawing.Size(112, 30);
            this.RetryCurrentActionButton.TabIndex = 3;
            this.RetryCurrentActionButton.Click += new System.EventHandler(this.RetryCurrentActionButton_Click);
            resources.ApplyResources(this.RetryCurrentActionButton, "RetryCurrentActionButton");
            // 
            // CancelCurrentActionButton
            // 
            this.CancelCurrentActionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelCurrentActionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelCurrentActionButton.Location = new System.Drawing.Point(383, 5);
            this.CancelCurrentActionButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CancelCurrentActionButton.Name = "CancelCurrentActionButton";
            this.CancelCurrentActionButton.Size = new System.Drawing.Size(112, 30);
            this.CancelCurrentActionButton.TabIndex = 4;
            this.CancelCurrentActionButton.Click += new System.EventHandler(this.CancelCurrentActionButton_Click);
            resources.ApplyResources(this.CancelCurrentActionButton, "CancelCurrentActionButton");
            // 
            // Wait
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.TopPanel);
            this.Controls.Add(this.BottomButtonPanel);
            this.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.Name = "Wait";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel TopPanel;
        private System.Windows.Forms.TextBox MessageTextBox;
        private System.Windows.Forms.ProgressBar DialogProgressBar;
        private System.Windows.Forms.TextBox LogTextBox;
        private System.Windows.Forms.Panel BottomButtonPanel;
        private System.Windows.Forms.Button RetryCurrentActionButton;
        private System.Windows.Forms.Button CancelCurrentActionButton;
    }
}
