namespace CKAN.GUI
{
    partial class Changeset
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
            System.ComponentModel.ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(Changeset));
            this.ChangesListView = new ThemedListView();
            this.Mod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ChangeType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Reason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BottomButtonPanel = new LeftRightRowPanel();
            this.ConfirmChangesButton = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.CancelChangesButton = new System.Windows.Forms.Button();
            this.BottomButtonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // ChangesListView
            //
            this.ChangesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChangesListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ChangesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Mod,
            this.ChangeType,
            this.Reason});
            this.ChangesListView.FullRowSelect = true;
            this.ChangesListView.Location = new System.Drawing.Point(-2, 0);
            this.ChangesListView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ChangesListView.Name = "ChangesListView";
            this.ChangesListView.Size = new System.Drawing.Size(1532, 886);
            this.ChangesListView.TabIndex = 0;
            this.ChangesListView.UseCompatibleStateImageBehavior = false;
            this.ChangesListView.View = System.Windows.Forms.View.Details;
            this.ChangesListView.SelectedIndexChanged += new System.EventHandler(ChangesListView_SelectedIndexChanged);
            //
            // Mod
            //
            this.Mod.Width = 332;
            resources.ApplyResources(this.Mod, "Mod");
            //
            // ChangeType
            //
            this.ChangeType.Width = 111;
            resources.ApplyResources(this.ChangeType, "ChangeType");
            //
            // Reason
            //
            this.Reason.Width = 606;
            resources.ApplyResources(this.Reason, "Reason");
            // 
            // BottomButtonPanel
            // 
            this.BottomButtonPanel.RightControls.Add(this.ConfirmChangesButton);
            this.BottomButtonPanel.RightControls.Add(this.CancelChangesButton);
            this.BottomButtonPanel.RightControls.Add(this.BackButton);
            this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomButtonPanel.Name = "BottomButtonPanel";
            //
            // BackButton
            //
            this.BackButton.AutoSize = true;
            this.BackButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.BackButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BackButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(112, 30);
            this.BackButton.TabIndex = 1;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            resources.ApplyResources(this.BackButton, "BackButton");
            //
            // CancelChangesButton
            //
            this.CancelChangesButton.AutoSize = true;
            this.CancelChangesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.CancelChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelChangesButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CancelChangesButton.Name = "CancelChangesButton";
            this.CancelChangesButton.Size = new System.Drawing.Size(112, 30);
            this.CancelChangesButton.TabIndex = 1;
            this.CancelChangesButton.Click += new System.EventHandler(this.CancelChangesButton_Click);
            resources.ApplyResources(this.CancelChangesButton, "CancelChangesButton");
            //
            // ConfirmChangesButton
            //
            this.ConfirmChangesButton.AutoSize = true;
            this.ConfirmChangesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.ConfirmChangesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConfirmChangesButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ConfirmChangesButton.Name = "ConfirmChangesButton";
            this.ConfirmChangesButton.Size = new System.Drawing.Size(112, 30);
            this.ConfirmChangesButton.TabIndex = 2;
            this.ConfirmChangesButton.Click += new System.EventHandler(this.ConfirmChangesButton_Click);
            resources.ApplyResources(this.ConfirmChangesButton, "ConfirmChangesButton");
            // 
            // Changeset
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.ChangesListView);
            this.Controls.Add(this.BottomButtonPanel);
            this.Margin = new System.Windows.Forms.Padding(0,0,0,0);
            this.Padding = new System.Windows.Forms.Padding(0,0,0,0);
            this.Name = "Changeset";
            this.Size = new System.Drawing.Size(500, 500);
            resources.ApplyResources(this, "$this");
            this.BottomButtonPanel.ResumeLayout(false);
            this.BottomButtonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListView ChangesListView;
        private System.Windows.Forms.ColumnHeader Mod;
        private System.Windows.Forms.ColumnHeader ChangeType;
        private System.Windows.Forms.ColumnHeader Reason;
        private LeftRightRowPanel BottomButtonPanel;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Button CancelChangesButton;
        private System.Windows.Forms.Button ConfirmChangesButton;
    }
}
