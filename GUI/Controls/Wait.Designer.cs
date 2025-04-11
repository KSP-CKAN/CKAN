namespace CKAN.GUI
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
            this.VerticalSplitter = new CKAN.GUI.UsableSplitContainer();
            this.DialogProgressBar = new CKAN.GUI.LabeledProgressBar();
            this.ProgressBarTable = new System.Windows.Forms.TableLayoutPanel();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.CancelCurrentActionButton = new System.Windows.Forms.Button();
            this.RetryCurrentActionButton = new System.Windows.Forms.Button();
            this.OkButton = new System.Windows.Forms.Button();
            this.VerticalSplitter.Panel1.SuspendLayout();
            this.VerticalSplitter.Panel2.SuspendLayout();
            this.VerticalSplitter.SuspendLayout();
            this.ProgressBarTable.SuspendLayout();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // VerticalSplitter
            //
            this.VerticalSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VerticalSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.VerticalSplitter.Location = new System.Drawing.Point(0, 35);
            this.VerticalSplitter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.VerticalSplitter.Name = "VerticalSplitter";
            this.VerticalSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.VerticalSplitter.Size = new System.Drawing.Size(500, 500);
            this.VerticalSplitter.SplitterDistance = 50;
            this.VerticalSplitter.SplitterWidth = 10;
            this.VerticalSplitter.TabStop = false;
            //
            // VerticalSplitter.Panel1
            //
            this.VerticalSplitter.Panel1.Controls.Add(this.DialogProgressBar);
            this.VerticalSplitter.Panel1.Controls.Add(this.ProgressBarTable);
            this.VerticalSplitter.Panel1MinSize = 50;
            //
            // VerticalSplitter.Panel2
            //
            this.VerticalSplitter.Panel2.Controls.Add(this.LogTextBox);
            this.VerticalSplitter.Panel2MinSize = 100;
            //
            // DialogProgressBar
            //
            this.DialogProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DialogProgressBar.Location = new System.Drawing.Point(5, 7);
            this.DialogProgressBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DialogProgressBar.Font = new System.Drawing.Font(System.Drawing.SystemFonts.MessageBoxFont.Name, 12, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.DialogProgressBar.Minimum = 0;
            this.DialogProgressBar.Maximum = 100;
            this.DialogProgressBar.Name = "DialogProgressBar";
            this.DialogProgressBar.Size = new System.Drawing.Size(490, 28);
            this.DialogProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.DialogProgressBar.TabIndex = 0;
            //
            // ProgressBarTable
            //
            this.ProgressBarTable.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
            this.ProgressBarTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ProgressBarTable.AutoScroll = true;
            this.ProgressBarTable.AutoSize = false;
            this.ProgressBarTable.ColumnCount = 2;
            this.ProgressBarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.ProgressBarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.ProgressBarTable.Location = new System.Drawing.Point(0, 40);
            this.ProgressBarTable.Name = "ProgressBarTable";
            this.ProgressBarTable.Padding = new System.Windows.Forms.Padding(1);
            this.ProgressBarTable.Size = new System.Drawing.Size(490, 10);
            this.ProgressBarTable.VerticalScroll.Visible = true;
            this.ProgressBarTable.VerticalScroll.SmallChange = 22;
            this.ProgressBarTable.HorizontalScroll.Visible = false;
            this.ProgressBarTable.TabIndex = 1;
            //
            // LogTextBox
            //
            this.LogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LogTextBox.Location = new System.Drawing.Point(14, 89);
            this.LogTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LogTextBox.Padding = new System.Windows.Forms.Padding(4);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.LogTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(500, 400);
            this.LogTextBox.TabIndex = 2;
            //
            // BottomButtonPanel
            //
            this.BottomButtonPanel.RightControls.Add(this.OkButton);
            this.BottomButtonPanel.RightControls.Add(this.CancelCurrentActionButton);
            this.BottomButtonPanel.RightControls.Add(this.RetryCurrentActionButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            //
            // RetryCurrentActionButton
            //
            this.RetryCurrentActionButton.AutoSize = true;
            this.RetryCurrentActionButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.RetryCurrentActionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RetryCurrentActionButton.Name = "RetryCurrentActionButton";
            this.RetryCurrentActionButton.Size = new System.Drawing.Size(112, 30);
            this.RetryCurrentActionButton.TabIndex = 3;
            this.RetryCurrentActionButton.Click += new System.EventHandler(this.RetryCurrentActionButton_Click);
            resources.ApplyResources(this.RetryCurrentActionButton, "RetryCurrentActionButton");
            //
            // CancelCurrentActionButton
            //
            this.CancelCurrentActionButton.AutoSize = true;
            this.CancelCurrentActionButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.CancelCurrentActionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelCurrentActionButton.Name = "CancelCurrentActionButton";
            this.CancelCurrentActionButton.Size = new System.Drawing.Size(112, 30);
            this.CancelCurrentActionButton.TabIndex = 4;
            this.CancelCurrentActionButton.Click += new System.EventHandler(this.CancelCurrentActionButton_Click);
            resources.ApplyResources(this.CancelCurrentActionButton, "CancelCurrentActionButton");
            //
            // OkButton
            //
            this.OkButton.AutoSize = true;
            this.OkButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.OkButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(112, 30);
            this.OkButton.TabIndex = 5;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            resources.ApplyResources(this.OkButton, "OkButton");
            //
            // Wait
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.VerticalSplitter);
            this.Controls.Add(this.BottomButtonPanel);
            this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.Name = "Wait";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.ProgressBarTable.ResumeLayout(false);
            this.ProgressBarTable.PerformLayout();
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
            this.VerticalSplitter.Panel1.ResumeLayout(false);
            this.VerticalSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.VerticalSplitter)).EndInit();
            this.VerticalSplitter.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private CKAN.GUI.UsableSplitContainer VerticalSplitter;
        private CKAN.GUI.LabeledProgressBar DialogProgressBar;
        private System.Windows.Forms.TableLayoutPanel ProgressBarTable;
        private System.Windows.Forms.TextBox LogTextBox;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Button RetryCurrentActionButton;
        private System.Windows.Forms.Button CancelCurrentActionButton;
        private System.Windows.Forms.Button OkButton;
    }
}
