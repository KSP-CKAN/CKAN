namespace CKAN.GUI
{
    partial class YesNoDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(YesNoDialog));
            this.panel1 = new System.Windows.Forms.Panel();
            this.DescriptionLabel = new CKAN.GUI.TransparentTextBox();
            this.BottomButtonPanel = new CKAN.GUI.LeftRightRowPanel();
            this.SuppressCheckbox = new System.Windows.Forms.CheckBox();
            this.YesButton = new System.Windows.Forms.Button();
            this.NoButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.DescriptionLabel);
            this.panel1.Location = new System.Drawing.Point(13, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(393, 73);
            this.panel1.TabIndex = 0;
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DescriptionLabel.Location = new System.Drawing.Point(0, 0);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(393, 73);
            this.DescriptionLabel.TabIndex = 0;
            this.DescriptionLabel.TabStop = false;
            this.DescriptionLabel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.DescriptionLabel.ReadOnly = true;
            this.DescriptionLabel.BackColor = System.Drawing.SystemColors.Control;
            this.DescriptionLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.DescriptionLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.DescriptionLabel, "DescriptionLabel");
            // 
            // BottomButtonPanel
            //
            this.BottomButtonPanel.LeftControls.Add(this.SuppressCheckbox);
            this.BottomButtonPanel.RightControls.Add(this.NoButton);
            this.BottomButtonPanel.RightControls.Add(this.YesButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            //
            // SuppressCheckbox
            // 
            this.SuppressCheckbox.AutoSize = false;
            this.SuppressCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom
            | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.SuppressCheckbox.Location = new System.Drawing.Point(13, 80);
            this.SuppressCheckbox.Name = "SuppressCheckbox";
            this.SuppressCheckbox.Size = new System.Drawing.Size(225, 50);
            this.SuppressCheckbox.TabIndex = 1;
            this.SuppressCheckbox.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.SuppressCheckbox, "SuppressCheckbox");
            // 
            // YesButton
            // 
            this.YesButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom
            | System.Windows.Forms.AnchorStyles.Right));
            this.YesButton.AutoSize = true;
            this.YesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.YesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.YesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.YesButton.Location = new System.Drawing.Point(250, 92);
            this.YesButton.Name = "YesButton";
            this.YesButton.Size = new System.Drawing.Size(75, 23);
            this.YesButton.TabIndex = 2;
            this.YesButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.YesButton, "YesButton");
            // 
            // NoButton
            // 
            this.NoButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom
            | System.Windows.Forms.AnchorStyles.Right));
            this.NoButton.AutoSize = true;
            this.NoButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.NoButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.NoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NoButton.Location = new System.Drawing.Point(331, 92);
            this.NoButton.Name = "NoButton";
            this.NoButton.Size = new System.Drawing.Size(75, 23);
            this.NoButton.TabIndex = 3;
            this.NoButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.NoButton, "NoButton");
            // 
            // YesNoDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 127);
            this.Controls.Add(this.BottomButtonPanel);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = EmbeddedImages.AppIcon;
            this.Name = "YesNoDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            this.panel1.ResumeLayout(false);
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
             this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private CKAN.GUI.TransparentTextBox DescriptionLabel;
        private CKAN.GUI.LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.CheckBox SuppressCheckbox;
        private System.Windows.Forms.Button YesButton;
        private System.Windows.Forms.Button NoButton;
    }
}
