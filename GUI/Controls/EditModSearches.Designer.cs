namespace CKAN.GUI
{
    partial class EditModSearches
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
            this.ToolTip = new System.Windows.Forms.ToolTip();
            this.AddSearchButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // ToolTip
            //
            this.ToolTip.AutoPopDelay = 10000;
            this.ToolTip.InitialDelay = 250;
            this.ToolTip.ReshowDelay = 250;
            this.ToolTip.ShowAlways = true;
            //
            // AddSearchButton
            //
            this.AddSearchButton.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Right
            | System.Windows.Forms.AnchorStyles.Top);
            this.AddSearchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AddSearchButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.AddSearchButton.Enabled = false;
            this.AddSearchButton.Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont.Name, 8F, System.Drawing.FontStyle.Bold);
            this.AddSearchButton.Location = new System.Drawing.Point(426, 2);
            this.AddSearchButton.Name = "AddSearchButton";
            this.AddSearchButton.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.AddSearchButton.Size = new System.Drawing.Size(22, 22);
            this.AddSearchButton.TabIndex = 1;
            this.AddSearchButton.Text = "+";
            this.AddSearchButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.AddSearchButton.UseVisualStyleBackColor = true;
            this.AddSearchButton.Click += new System.EventHandler(this.AddSearchButton_Click);
            //
            // EditModSearches
            //
            this.Controls.Add(this.AddSearchButton);
            this.Name = "EditModSearches";
            this.Size = new System.Drawing.Size(500, 54);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Button AddSearchButton;
    }
}
