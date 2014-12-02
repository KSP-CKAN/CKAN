using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace CKAN
{
    public partial class SettingsDialog : Form
    {
        public SettingsDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            MirrorsList mirrors = new MirrorsList();

            try
            {
                mirrors = Repo.FetchMasterList();
            }
            catch
            {
                User.Error("Couldn't fetch CKAN mirrors master list from {0}", Repo.repo_master_list.ToString());
            }
            
            CKANRepositoryComboBox.Items.Clear();
            foreach (Mirror mirror in mirrors.mirrors)
            {
                CKANRepositoryComboBox.Items.Add(mirror);
            }

            if (CKANRepositoryComboBox.Items.Count > 0)
            {
                CKANRepositoryComboBox.SelectedIndex = 0;
            }

            KSPInstallPathLabel.Text = KSPManager.CurrentInstance.GameDir();
            UpdateCacheInfo();
        }

        private void UpdateCacheInfo()
        {
            long cacheSize = 0;
            var cachePath = Path.Combine(KSPManager.CurrentInstance.CkanDir(), "downloads");

            var cacheDirectory = new DirectoryInfo(cachePath);
            int count = 0;
            foreach (var file in cacheDirectory.GetFiles())
            {
                count++;
                cacheSize += file.Length;
            }

            CKANCacheLabel.Text = String.Format
            (
                "There are currently {0} files in the cache for a total of {1} MiB",
                count,
                cacheSize / 1024 / 1024
            );
        }

        private void CKANRepositoryApplyButton_Click(object sender, EventArgs e)
        {
            Main.Instance.m_Configuration.Repository = ((Mirror)CKANRepositoryComboBox.SelectedItem).url.ToString();
            Main.Instance.UpdateRepo();
            Main.Instance.m_Configuration.Save();
            Close();
        }

        private void CKANRepositoryDefaultButton_Click(object sender, EventArgs e)
        {
            Main.Instance.m_Configuration.Repository = Repo.default_ckan_repo.ToString();
            Main.Instance.UpdateRepo();
            Main.Instance.m_Configuration.Save();
            Close();
        }

        private void ClearCKANCacheButton_Click(object sender, EventArgs e)
        {
            var cachePath = Path.Combine(KSPManager.CurrentInstance.CkanDir(), "downloads");
            foreach (var file in Directory.GetFiles(cachePath))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                }
            }

            UpdateCacheInfo();
        }

        private void ResetAutoStartChoice_Click(object sender, EventArgs e)
        {
            KSPManager.ClearAutoStart();

            Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Application.Exit();
        }

        private void CKANRepositoryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}