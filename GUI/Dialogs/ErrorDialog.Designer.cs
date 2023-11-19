namespace CKAN.GUI
{
    partial class ErrorDialog
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(ErrorDialog));
            this.panel1 = new System.Windows.Forms.Panel();
            this.ErrorMessage = new System.Windows.Forms.RichTextBox();
            this.DismissButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            //
            // panel1
            //
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.ErrorMessage);
            this.panel1.Location = new System.Drawing.Point(13, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(775, 105);
            this.panel1.TabIndex = 0;
            //
            // ErrorMessage
            //
            this.ErrorMessage.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ErrorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ErrorMessage.Location = new System.Drawing.Point(0, 0);
            this.ErrorMessage.Name = "ErrorMessage";
            this.ErrorMessage.Size = new System.Drawing.Size(267, 105);
            this.ErrorMessage.TabIndex = 0;
            this.ErrorMessage.ReadOnly = true;
            resources.ApplyResources(this.ErrorMessage, "ErrorMessage");
            //
            // DismissButton
            //
            this.DismissButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom));
            this.DismissButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DismissButton.Location = new System.Drawing.Point(387, 124);
            this.DismissButton.Name = "DismissButton";
            this.DismissButton.Size = new System.Drawing.Size(75, 23);
            this.DismissButton.TabIndex = 1;
            this.DismissButton.UseVisualStyleBackColor = true;
            this.DismissButton.Click += new System.EventHandler(this.DismissButton_Click);
            resources.ApplyResources(this.DismissButton, "DismissButton");
            //
            // ErrorDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 160);
            this.ControlBox = true;
            this.Controls.Add(this.DismissButton);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = EmbeddedImages.AppIcon;
            this.Name = "ErrorDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox ErrorMessage;
        private System.Windows.Forms.Button DismissButton;
    }
}
