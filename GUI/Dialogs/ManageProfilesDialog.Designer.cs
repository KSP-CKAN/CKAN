namespace CKAN.GUI
{
    partial class ManageProfilesDialog
    {
        private void InitializeComponent()
        {
            var resources = new SingleAssemblyComponentResourceManager(typeof(ManageProfilesDialog));
            var layout = new System.Windows.Forms.TableLayoutPanel();
            var buttons = new System.Windows.Forms.FlowLayoutPanel();
            var actions = new System.Windows.Forms.FlowLayoutPanel();
            var nameLabel = new System.Windows.Forms.Label();
            var description = new System.Windows.Forms.Label();
            var close = new System.Windows.Forms.Button();
            ProfilesListBox = new System.Windows.Forms.ListBox();
            NameTextBox = new System.Windows.Forms.TextBox();
            ActiveLabel = new System.Windows.Forms.Label();
            CreateButton = new System.Windows.Forms.Button();
            RenameButton = new System.Windows.Forms.Button();
            DeleteButton = new System.Windows.Forms.Button();
            SwitchButton = new System.Windows.Forms.Button();
            SuspendLayout();
            layout.Dock = System.Windows.Forms.DockStyle.Fill;
            layout.Padding = new System.Windows.Forms.Padding(12);
            layout.ColumnCount = 1;
            layout.RowCount = 7;
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            for (int i = 0; i < 4; ++i)
            {
                layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            }
            description.AutoSize = true;
            description.MaximumSize = new System.Drawing.Size(450, 0);
            resources.ApplyResources(description, "Description");
            ActiveLabel.AutoSize = true;
            ProfilesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            ProfilesListBox.SelectedIndexChanged += SelectionChanged;
            resources.ApplyResources(ProfilesListBox, "ProfilesListBox");
            nameLabel.AutoSize = true;
            resources.ApplyResources(nameLabel, "NameLabel");
            NameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            resources.ApplyResources(NameTextBox, "NameTextBox");
            buttons.AutoSize = true;
            buttons.Dock = System.Windows.Forms.DockStyle.Fill;
            actions.AutoSize = true;
            actions.Dock = System.Windows.Forms.DockStyle.Fill;
            actions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            foreach (var button in new[] { CreateButton, RenameButton, DeleteButton, SwitchButton, close })
            {
                button.AutoSize = true;
                button.UseVisualStyleBackColor = true;
            }
            resources.ApplyResources(CreateButton, "CreateButton");
            resources.ApplyResources(RenameButton, "RenameButton");
            resources.ApplyResources(DeleteButton, "DeleteButton");
            resources.ApplyResources(SwitchButton, "SwitchButton");
            resources.ApplyResources(close, "CloseButton");
            CreateButton.Click += CreateButton_Click;
            RenameButton.Click += RenameButton_Click;
            DeleteButton.Click += DeleteButton_Click;
            SwitchButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttons.Controls.AddRange(new System.Windows.Forms.Control[] { CreateButton, RenameButton, DeleteButton });
            actions.Controls.AddRange(new System.Windows.Forms.Control[] { close, SwitchButton });
            layout.Controls.Add(description, 0, 0);
            layout.Controls.Add(ActiveLabel, 0, 1);
            layout.Controls.Add(ProfilesListBox, 0, 2);
            layout.Controls.Add(nameLabel, 0, 3);
            layout.Controls.Add(NameTextBox, 0, 4);
            layout.Controls.Add(buttons, 0, 5);
            layout.Controls.Add(actions, 0, 6);
            Controls.Add(layout);
            AcceptButton = SwitchButton;
            CancelButton = close;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            ClientSize = new System.Drawing.Size(490, 390);
            MinimumSize = new System.Drawing.Size(490, 390);
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            resources.ApplyResources(this, "$this");
            ResumeLayout(false);
        }

        private System.Windows.Forms.ListBox ProfilesListBox = null!;
        private System.Windows.Forms.TextBox NameTextBox = null!;
        private System.Windows.Forms.Label ActiveLabel = null!;
        private System.Windows.Forms.Button CreateButton = null!;
        private System.Windows.Forms.Button RenameButton = null!;
        private System.Windows.Forms.Button DeleteButton = null!;
        private System.Windows.Forms.Button SwitchButton = null!;
    }
}
