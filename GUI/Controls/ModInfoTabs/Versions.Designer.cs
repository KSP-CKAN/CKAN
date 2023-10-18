namespace CKAN.GUI
{
    partial class Versions
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(Versions));
            this.label1 = new System.Windows.Forms.Label();
            this.VersionsListView = new ThemedListView();
            this.ModVersion = new System.Windows.Forms.ColumnHeader();
            this.CompatibleGameVersion = new System.Windows.Forms.ColumnHeader();
            this.ReleaseDate = new System.Windows.Forms.ColumnHeader();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(183, 13);
            this.label1.TabIndex = 0;
            resources.ApplyResources(this.label1, "label1");
            //
            // VersionsListView
            //
            this.VersionsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.VersionsListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.VersionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ModVersion,
            this.CompatibleGameVersion,
            this.ReleaseDate});
            this.VersionsListView.CheckBoxes = true;
            this.VersionsListView.FullRowSelect = true;
            this.VersionsListView.Location = new System.Drawing.Point(6, 76);
            this.VersionsListView.Name = "VersionsListView";
            this.VersionsListView.Size = new System.Drawing.Size(488, 416);
            this.VersionsListView.TabIndex = 1;
            this.VersionsListView.UseCompatibleStateImageBehavior = false;
            this.VersionsListView.View = System.Windows.Forms.View.Details;
            this.VersionsListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.VersionsListView_ItemCheck);
            //
            // ModVersion
            //
            this.ModVersion.Width = 98;
            resources.ApplyResources(this.ModVersion, "ModVersion");
            //
            // CompatibleGameVersion
            //
            this.CompatibleGameVersion.Width = 132;
            resources.ApplyResources(this.CompatibleGameVersion, "CompatibleGameVersion");
            //
            // ReleaseDate
            //
            this.ReleaseDate.Width = 140;
            resources.ApplyResources(this.ReleaseDate, "ReleaseDate");
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Green;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(4, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 2;
            resources.ApplyResources(this.label2, "label2");
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.LightGreen;
            this.label3.Location = new System.Drawing.Point(4, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 3;
            resources.ApplyResources(this.label3, "label3");
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label4.Location = new System.Drawing.Point(0, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 4;
            resources.ApplyResources(this.label4, "label4");
            //
            // label5
            //
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(65, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(229, 13);
            this.label5.TabIndex = 5;
            resources.ApplyResources(this.label5, "label5");
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(65, 36);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(180, 13);
            this.label6.TabIndex = 6;
            resources.ApplyResources(this.label6, "label6");
            //
            // label7
            //
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(65, 56);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(131, 13);
            this.label7.TabIndex = 7;
            resources.ApplyResources(this.label7, "label7");
            //
            // Versions
            //
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.VersionsListView);
            this.Controls.Add(this.label1);
            this.Name = "Versions";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView VersionsListView;
        private System.Windows.Forms.ColumnHeader ModVersion;
        private System.Windows.Forms.ColumnHeader CompatibleGameVersion;
        private System.Windows.Forms.ColumnHeader ReleaseDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
    }
}
