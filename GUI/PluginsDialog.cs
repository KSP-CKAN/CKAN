using System;
using System.Windows.Forms;

namespace CKAN
{
    public partial class PluginsDialog : Form
    {
        public PluginsDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private OpenFileDialog m_AddNewPluginDialog = new OpenFileDialog();

        private void PluginsDialog_Load(object sender, EventArgs e)
        {
            DeactivateButton.Enabled = false;
            ReloadPluginButton.Enabled = false;
            ActivatePluginButton.Enabled = false;
            UnloadPluginButton.Enabled = false;

            RefreshActivePlugins();
            RefreshDormantPlugins();

            m_AddNewPluginDialog.Filter = "CKAN Plugins (*.dll)|*.dll";
            m_AddNewPluginDialog.Multiselect = false;
        }

        private void RefreshActivePlugins()
        {
            var activePlugins = Main.Instance.m_PluginController.ActivePlugins;

            ActivePluginsListBox.Items.Clear();
            foreach (var plugin in activePlugins)
            {
                ActivePluginsListBox.Items.Add(plugin);
            }
        }

        private void RefreshDormantPlugins()
        {
            var dormantPlugins = Main.Instance.m_PluginController.DormantPlugins;

            DormantPluginsListBox.Items.Clear();
            foreach (var plugin in dormantPlugins)
            {
                DormantPluginsListBox.Items.Add(plugin);
            }
        }

        private void ActivePluginsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool state = ActivePluginsListBox.SelectedItem != null;
            DeactivateButton.Enabled = state;
            ReloadPluginButton.Enabled = state;
        }

        private void DeactivateButton_Click(object sender, EventArgs e)
        {
            if (ActivePluginsListBox.SelectedItem == null)
            {
                return;
            }

            var plugin = (IGUIPlugin)ActivePluginsListBox.SelectedItem;
            Main.Instance.m_PluginController.DeactivatePlugin(plugin);
            RefreshActivePlugins();
            RefreshDormantPlugins();
        }

        private void ReloadPluginButton_Click(object sender, EventArgs e)
        {
            if (ActivePluginsListBox.SelectedItem == null)
            {
                return;
            }

            var plugin = (IGUIPlugin)ActivePluginsListBox.SelectedItem;
            Main.Instance.m_PluginController.DeactivatePlugin(plugin);
            Main.Instance.m_PluginController.ActivatePlugin(plugin);
            RefreshActivePlugins();
            RefreshDormantPlugins();
        }

        private void DormantPluginsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool state = DormantPluginsListBox.SelectedItem != null;
            ActivatePluginButton.Enabled = state;
            UnloadPluginButton.Enabled = state;
        }

        private void ActivatePluginButton_Click(object sender, EventArgs e)
        {
            if (DormantPluginsListBox.SelectedItem == null)
            {
                return;
            }

            var plugin = (IGUIPlugin)DormantPluginsListBox.SelectedItem;
            Main.Instance.m_PluginController.ActivatePlugin(plugin);
            RefreshActivePlugins();
            RefreshDormantPlugins();
        }

        private void UnloadPluginButton_Click(object sender, EventArgs e)
        {
            if (DormantPluginsListBox.SelectedItem == null)
            {
                return;
            }

            var plugin = (IGUIPlugin)DormantPluginsListBox.SelectedItem;
            Main.Instance.m_PluginController.UnloadPlugin(plugin);
            RefreshActivePlugins();
            RefreshDormantPlugins();
        }

        private void AddNewPluginButton_Click(object sender, EventArgs e)
        {
            if (m_AddNewPluginDialog.ShowDialog() == DialogResult.OK)
            {
                var path = m_AddNewPluginDialog.FileName;
                Main.Instance.m_PluginController.AddNewAssemblyToPluginsPath(path);
                RefreshDormantPlugins();
            }
        }
    }
}