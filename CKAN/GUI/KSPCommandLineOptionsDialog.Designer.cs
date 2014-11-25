namespace CKAN
{
    partial class KSPCommandLineOptionsDialog
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
            this.AdditionalArguments = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.AcceptChangesButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AdditionalArguments
            // 
            this.AdditionalArguments.Location = new System.Drawing.Point(15, 25);
            this.AdditionalArguments.Name = "AdditionalArguments";
            this.AdditionalArguments.Size = new System.Drawing.Size(457, 20);
            this.AdditionalArguments.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Arguments:";
            // 
            // AcceptChangesButton
            // 
            this.AcceptChangesButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.AcceptChangesButton.Location = new System.Drawing.Point(397, 51);
            this.AcceptChangesButton.Name = "AcceptChangesButton";
            this.AcceptChangesButton.Size = new System.Drawing.Size(75, 23);
            this.AcceptChangesButton.TabIndex = 3;
            this.AcceptChangesButton.Text = "OK";
            this.AcceptChangesButton.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(316, 51);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // KSPCommandLineOptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 85);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.AcceptChangesButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.AdditionalArguments);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "KSPCommandLineOptionsDialog";
            this.Text = "KSP command-line arguments";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button AcceptChangesButton;
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.TextBox AdditionalArguments;
    }
}
