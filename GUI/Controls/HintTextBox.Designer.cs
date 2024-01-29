namespace CKAN.GUI
{
    partial class HintTextBox
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
            this.ClearIcon = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            // 
            // ClearIcon
            // 
            this.ClearIcon.BackColor = System.Drawing.Color.Transparent;
            this.ClearIcon.Visible = false;
            this.ClearIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ClearIcon.Image = global::CKAN.GUI.EmbeddedImages.textClear;
            this.ClearIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.ClearIcon.Size = new System.Drawing.Size(18, 18);
            this.ClearIcon.Click += this.HintClearIcon_Click;
            // 
            // HintTextBox
            // 
            this.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.Controls.Add(ClearIcon);
            this.SizeChanged += new System.EventHandler(this.HintTextBox_SizeChanged);
            this.TextChanged += new System.EventHandler(this.HintTextBox_TextChanged);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox ClearIcon;
    }
}
