using System;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class ManageProfilesDialog : Form
    {
        public ManageProfilesDialog(RegistryManager manager)
        {
            this.manager = manager;
            InitializeComponent();
            this.ScaleFonts();
            Reload();
        }

        public string? SelectedProfile => ProfilesListBox.SelectedItem as string;

        private readonly RegistryManager manager;

        private void Reload()
        {
            ProfilesListBox.Items.Clear();
            ProfilesListBox.Items.AddRange(manager.registry.ProfileNames);
            ProfilesListBox.SelectedItem = manager.registry.ActiveProfile;
            ActiveLabel.Text = string.Format(Properties.Resources.ProfilesActive,
                                            manager.registry.ActiveProfile ?? Properties.Resources.ProfilesNone);
            SelectionChanged(null, EventArgs.Empty);
        }

        private void SelectionChanged(object? sender, EventArgs e)
        {
            NameTextBox.Text = SelectedProfile ?? "";
            RenameButton.Enabled = SelectedProfile != null;
            DeleteButton.Enabled = SelectedProfile != null && SelectedProfile != manager.registry.ActiveProfile;
            SwitchButton.Enabled = SelectedProfile != null && SelectedProfile != manager.registry.ActiveProfile;
        }

        private void Change(Action action)
        {
            try
            {
                using (var transaction = CkanTransaction.CreateTransactionScope())
                {
                    action();
                    manager.Save();
                    transaction.Complete();
                }
                Reload();
            }
            catch (Exception exc) when (exc is ArgumentException || exc is Kraken || exc is System.IO.IOException)
            {
                MessageBox.Show(this, exc.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateButton_Click(object? sender, EventArgs e)
            => Change(() => manager.registry.CreateProfile(NameTextBox.Text));

        private void RenameButton_Click(object? sender, EventArgs e)
        {
            if (SelectedProfile is string name)
            {
                Change(() => manager.registry.RenameProfile(name, NameTextBox.Text));
            }
        }

        private void DeleteButton_Click(object? sender, EventArgs e)
        {
            if (SelectedProfile is string name
                && MessageBox.Show(this, string.Format(Properties.Resources.ProfilesDeletePrompt, name),
                                   Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Change(() => manager.registry.DeleteProfile(name));
            }
        }
    }
}
