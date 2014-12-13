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
            CKANRepositoryTextBox.Text = Main.Instance.m_Configuration.Repository;
            KSPInstallPathLabel.Text = Main.Instance.CurrentInstance.GameDir();

            UpdateCacheInfo();
        }

        private void UpdateCacheInfo()
        {
            long cacheSize = 0;
            var cachePath = Path.Combine(Main.Instance.CurrentInstance.CkanDir(), "downloads");

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
            Main.Instance.m_Configuration.Repository = CKANRepositoryTextBox.Text;
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
            var cachePath = Path.Combine(Main.Instance.CurrentInstance.CkanDir(), "downloads");
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
            Main.Instance.Manager.ClearAutoStart();

            Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Application.Exit();
        }

    }
}