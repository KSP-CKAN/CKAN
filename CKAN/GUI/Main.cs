using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKAN
{

    public enum GUIModFilter
    {
        All = 0,
        Installed = 1,
        InstalledUpdateAvailable = 2,
        NewInRepository = 3,
        NotInstalled = 4,
    }

    public enum GUIModChangeType
    {
        None = 0,
        Install = 1,
        Remove = 2,
        Update = 3,
    }

    public partial class Main : Form
    {

        private WaitDialog m_WaitDialog = new WaitDialog();
        private ApplyChangesDialog m_ApplyChangesDialog = new ApplyChangesDialog();

        private BackgroundWorker m_UpdateRepoWorker = null;
        private BackgroundWorker m_InstallWorker = null;

        private GUIModFilter m_ModFilter = GUIModFilter.All;

        public Main()
        {
            m_Instance = this;

            m_UpdateRepoWorker = new BackgroundWorker();
            m_UpdateRepoWorker.WorkerReportsProgress = false;
            m_UpdateRepoWorker.WorkerSupportsCancellation = true;
            m_UpdateRepoWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PostUpdateRepo);
            m_UpdateRepoWorker.DoWork += new DoWorkEventHandler(UpdateRepo);

            m_InstallWorker = new BackgroundWorker();
            m_InstallWorker.WorkerReportsProgress = true;
            m_InstallWorker.WorkerSupportsCancellation = true;
            m_InstallWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PostInstallMods);
            m_InstallWorker.DoWork += new DoWorkEventHandler(InstallMods);

            User.noConsole = true;

            InitializeComponent();
            UpdateModsList();
        }

        private static Main m_Instance = null;
        public static Main Instance
        {
            get
            {
                return m_Instance;
            }
        }

        private void UpdateRepo(object sender, DoWorkEventArgs e)
        {
            Repo.Update();
        }

        private void PostUpdateRepo(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList();
            m_WaitDialog.Close();
            Enabled = true;
        }

        private void InstallModsReportProgress(string message, int percent)
        {
            if (m_WaitDialog != null)
            {
                m_WaitDialog.SetDescription(message + " " + percent.ToString() + "%");
            }

        }

        private void InstallMods(object sender, DoWorkEventArgs e)
        {
            var opts = (KeyValuePair<List<KeyValuePair<CkanModule, GUIModChangeType>>, RelationshipResolverOptions>)e.Argument;

            ModuleInstaller installer = new ModuleInstaller();
            installer.onReportProgress += InstallModsReportProgress;

            // first we uninstall selected mods
            foreach (var change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Remove)
                {
                    installer.Uninstall(change.Key.identifier);
                }
            }

            // install everything else
            List<string> toInstall = new List<string>();
            foreach (var change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Install || change.Value == GUIModChangeType.Update)
                {
                    toInstall.Add(change.Key.identifier);
                }
            }

            installer.InstallList(toInstall, opts.Value);
        }

        private void PostInstallMods(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList();
            m_WaitDialog.Close();
            Enabled = true;
        }

        private List<KeyValuePair<CkanModule, GUIModChangeType>> ComputeChangeSetFromModList()
        {
            List<KeyValuePair<CkanModule, GUIModChangeType>> changeset = new List<KeyValuePair<CkanModule, GUIModChangeType>>();

            foreach (ListViewItem item in ModList.Items)
            {
                var mod = (CkanModule)item.Tag;
                if (mod == null)
                {
                    continue;
                }

                var isInstalled = RegistryManager.Instance().registry.IsInstalled(mod.identifier);
                var isChecked = item.Checked;

                if (isInstalled && !isChecked)
                {
                    changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Remove));
                }
                else if (isInstalled && isChecked && mod.version.IsGreaterThan(RegistryManager.Instance().registry.InstalledVersion(mod.identifier)))
                {
                    changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Update));
                }
                else if (!isInstalled && isChecked)
                {
                    changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Install));
                }
                else
                {
                    changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.None));
                }
            }

            return changeset;
        }

        private void UpdateModsList(bool markUpdates = false)
        {
            ModList.Items.Clear();

            List<CkanModule> modules = RegistryManager.Instance().registry.Available();

            switch (m_ModFilter)
            {
                case GUIModFilter.All:
                    break;
                case GUIModFilter.Installed:
                    modules.RemoveAll(m => !RegistryManager.Instance().registry.IsInstalled(m.identifier));
                    break;
                case GUIModFilter.InstalledUpdateAvailable:
                    modules.RemoveAll
                    (
                        m => !(RegistryManager.Instance().registry.IsInstalled(m.identifier) &&
                            m.version.IsGreaterThan(RegistryManager.Instance().registry.InstalledVersion(m.identifier)))
                    );
                    break;
                case GUIModFilter.NewInRepository:
                    break;
                case GUIModFilter.NotInstalled:
                    modules.RemoveAll(m => RegistryManager.Instance().registry.IsInstalled(m.identifier));
                    break;
            }

            foreach (CkanModule mod in modules)
            {
                ListViewItem item = new ListViewItem();
                item.Tag = mod;

                var isInstalled = RegistryManager.Instance().registry.IsInstalled(mod.identifier);

                if (isInstalled && RegistryManager.Instance().registry.InstalledVersion(mod.identifier).IsLessThan(mod.version) && markUpdates)
                {
                    item.Checked = true;
                }
                else if (isInstalled && !RegistryManager.Instance().registry.InstalledVersion(mod.identifier).IsLessThan(mod.version))
                {
                    item.Checked = isInstalled;
                }

                var subName = new ListViewItem.ListViewSubItem();
                subName.Text = mod.name;
                item.SubItems.Add(subName);

                var subInstalledVersion = new ListViewItem.ListViewSubItem();
                if (isInstalled)
                {
                    subInstalledVersion.Text = RegistryManager.Instance().registry.InstalledVersion(mod.identifier).ToString();
                }
                else
                {
                    subInstalledVersion.Text = "-";
                }
                item.SubItems.Add(subInstalledVersion);

                var subAvailableVersion = new ListViewItem.ListViewSubItem();
                subAvailableVersion.Text = mod.version.ToString();
                item.SubItems.Add(subAvailableVersion);

                var subSize = new ListViewItem.ListViewSubItem();
                subSize.Text = "0 KiB";
                item.SubItems.Add(subSize);

                var subDescription = new ListViewItem.ListViewSubItem();
                subDescription.Text = mod.@abstract;
                item.SubItems.Add(subDescription);

                ModList.Items.Add(item);
            }
        }

        private void RefreshToolButton_Click(object sender, EventArgs e)
        {
            m_UpdateRepoWorker.RunWorkerAsync();
            Enabled = false;
            m_WaitDialog.SetDescription("Contacting repository..");
            m_WaitDialog.ShowWaitDialog();
        }

        private void ModFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_ModFilter = (GUIModFilter)ModFilter.SelectedIndex;
            UpdateModsList();
        }

        private void MarkAllUpdatesToolButton_Click(object sender, EventArgs e)
        {
            UpdateModsList();
        }

        private void ModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ModList.SelectedItems.Count == 0)
            {
                return;
            }

            var selectedItem = ModList.SelectedItems[0];
            if (selectedItem == null)
            {
                return;
            }

            var module = (CkanModule)selectedItem.Tag;
            if (module == null)
            {
                return;
            }

            ModInfo.Text = "";

            ModInfo.AppendText(String.Format("\"{0}\" - version {1}\r\n", module.name, module.version));

            ModInfo.AppendText(String.Format("Abstract: {0}\r\n", module.@abstract));

            if (module.author != null)
            {
                string authors = "";
                foreach (var auth in module.author)
                {
                    authors += auth + ", ";
                }

                ModInfo.AppendText(String.Format("Author: {0}\r\n", authors));
            }

            ModInfo.AppendText(String.Format("Comment: {0}\r\n", module.comment));
            ModInfo.AppendText(String.Format("Download: {0}\r\n", module.download.ToString()));
            ModInfo.AppendText(String.Format("Identifier: {0}\r\n", module.identifier));
            ModInfo.AppendText(String.Format("KSP Version: {0}\r\n", module.ksp_version.ToString()));
            ModInfo.AppendText(String.Format("License: {0}\r\n", module.license.ToString()));
            ModInfo.AppendText(String.Format("Release status: {0}\r\n", module.release_status));
        }

        private void ApplyToolButton_Click(object sender, EventArgs e)
        {
            var changeset = ComputeChangeSetFromModList();
            m_ApplyChangesDialog.ShowApplyChangesDialog(changeset, m_InstallWorker);
        }

        private void ExitToolButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void ShowWaitDialog()
        {
            Enabled = false;
            m_WaitDialog.ShowWaitDialog();
        }

        public void HideWaitDialog()
        {
            m_WaitDialog.Close();
            Enabled = true;
        }
    }
}
