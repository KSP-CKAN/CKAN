namespace CKAN
{
    partial class WaitDialog
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
            this.ModalLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ModalLabel
            // 
            this.ModalLabel.AutoSize = true;
            this.ModalLabel.Location = new System.Drawing.Point(61, 34);
            this.ModalLabel.Name = "ModalLabel";
            this.ModalLabel.Size = new System.Drawing.Size(163, 13);
            this.ModalLabel.TabIndex = 0;
            this.ModalLabel.Text = "Waiting for operation to complete";
            // 
            // WaitDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 89);
            this.Controls.Add(this.ModalLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "WaitDialog";
            this.Text = "Please wait";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ModalLabel;
    }
}