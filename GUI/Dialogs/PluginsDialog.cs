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
    public partial class PluginsDialog : Form
    {
        public PluginsDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private PluginController pluginController => Main.Instance.pluginController;

        private readonly OpenFileDialog m_AddNewPluginDialog = new OpenFileDialog();

        private void PluginsDialog_Load(object sender, EventArgs e)
        {
            DeactivateButton.Enabled = false;
            ReloadPluginButton.Enabled = false;
            ActivatePluginButton.Enabled = false;
            UnloadPluginButton.Enabled = false;

            RefreshActivePlugins();
            RefreshDormantPlugins();

            m_AddNewPluginDialog.Filter = Properties.Resources.PluginsDialogFilter;
            m_AddNewPluginDialog.Multiselect = false;
        }

        private void RefreshActivePlugins()
        {
            var activePlugins = pluginController.ActivePlugins;

            ActivePluginsListBox.Items.Clear();
            foreach (var plugin in activePlugins)
            {
                ActivePluginsListBox.Items.Add(plugin);
            }
        }

        private void RefreshDormantPlugins()
        {
            var dormantPlugins = pluginController.DormantPlugins;

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

            var plugin = (IGUIPlugin) ActivePluginsListBox.SelectedItem;
            pluginController.DeactivatePlugin(plugin);
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
            pluginController.DeactivatePlugin(plugin);
            pluginController.ActivatePlugin(plugin);
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
            pluginController.ActivatePlugin(plugin);
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
            pluginController.UnloadPlugin(plugin);
            RefreshActivePlugins();
            RefreshDormantPlugins();
        }

        private void AddNewPluginButton_Click(object sender, EventArgs e)
        {
            if (m_AddNewPluginDialog.ShowDialog(this) == DialogResult.OK)
            {
                var path = m_AddNewPluginDialog.FileName;
                pluginController.AddNewAssemblyToPluginsPath(path);
                RefreshDormantPlugins();
            }
        }

    }
}
