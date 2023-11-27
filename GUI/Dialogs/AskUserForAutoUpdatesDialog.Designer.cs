namespace CKAN.GUI
{
    partial class AskUserForAutoUpdatesDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(AskUserForAutoUpdatesDialog));
            this.autoCheckLabel = new System.Windows.Forms.Label();
            this.YesButton = new System.Windows.Forms.Button();
            this.NoButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // autoCheckLabel
            //
            this.autoCheckLabel.Location = new System.Drawing.Point(12, 9);
            this.autoCheckLabel.Name = "autoCheckLabel";
            this.autoCheckLabel.Size = new System.Drawing.Size(538, 50);
            this.autoCheckLabel.TabIndex = 0;
            resources.ApplyResources(this.autoCheckLabel, "autoCheckLabel");
            //
            // YesButton
            //
            this.YesButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.YesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.YesButton.Location = new System.Drawing.Point(401, 44);
            this.YesButton.Name = "YesButton";
            this.YesButton.Size = new System.Drawing.Size(149, 23);
            this.YesButton.TabIndex = 1;
            this.YesButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.YesButton, "YesButton");
            //
            // NoButton
            //
            this.NoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NoButton.Location = new System.Drawing.Point(323, 44);
            this.NoButton.Size = new System.Drawing.Size(72, 23);
            this.NoButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.NoButton.Name = "NoButton";
            this.NoButton.TabIndex = 2;
            this.NoButton.UseVisualStyleBackColor = true;
            resources.ApplyResources(this.NoButton, "NoButton");
            //
            // AskUserForAutoUpdatesDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 79);
            this.Controls.Add(this.NoButton);
            this.Controls.Add(this.YesButton);
            this.Controls.Add(this.autoCheckLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = EmbeddedImages.AppIcon;
            this.Name = "AskUserForAutoUpdatesDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label autoCheckLabel;
        private System.Windows.Forms.Button YesButton;
        private System.Windows.Forms.Button NoButton;
    }
}
