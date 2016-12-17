namespace CKAN
{
    partial class MainAllModVersions
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "",
            "i1",
            "i2"}, -1);
            this.label1 = new System.Windows.Forms.Label();
            this.ModVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CompatibleKSPVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.VersionsListView = new System.Windows.Forms.ListView();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(303, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "All available versions of selected mod (installed version is bold):";
            // 
            // ModVersion
            // 
            this.ModVersion.Text = "Mod Version";
            this.ModVersion.Width = 73;
            // 
            // CompatibleKSPVersion
            // 
            this.CompatibleKSPVersion.Text = "Compatible KSP Version";
            this.CompatibleKSPVersion.Width = 248;
            // 
            // VersionsListView
            // 
            this.VersionsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.VersionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ModVersion,
            this.CompatibleKSPVersion});
            this.VersionsListView.FullRowSelect = true;
            this.VersionsListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.VersionsListView.Location = new System.Drawing.Point(3, 28);
            this.VersionsListView.Name = "VersionsListView";
            this.VersionsListView.Size = new System.Drawing.Size(606, 101);
            this.VersionsListView.TabIndex = 1;
            this.VersionsListView.UseCompatibleStateImageBehavior = false;
            this.VersionsListView.View = System.Windows.Forms.View.Details;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(759, 28);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(758, 227);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // MainAllModVersions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.VersionsListView);
            this.Controls.Add(this.label1);
            this.Name = "MainAllModVersions";
            this.Size = new System.Drawing.Size(837, 253);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader ModVersion;
        private System.Windows.Forms.ColumnHeader CompatibleKSPVersion;
        private System.Windows.Forms.ListView VersionsListView;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}
