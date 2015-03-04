using System;
using System.Collections;
using System.Collections.Generic;
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
            RefreshReposListBox();

            KSPInstallPathLabel.Text = Main.Instance.CurrentInstance.GameDir();
            UpdateCacheInfo();
        }

        private void RefreshReposListBox()
        {
            List<Repository> sortedRepos = new List<Repository>();
            foreach (var item in Main.Instance.CurrentInstance.Registry.Repositories)
            {
                sortedRepos.Add(item.Value);
            }

            sortedRepos.Sort((repo1, repo2) => repo1.priority.CompareTo(repo2.priority));

            ReposListBox.Items.Clear();
            foreach (var item in sortedRepos)
            {
                ReposListBox.Items.Add(item);
            }

            Main.Instance.CurrentInstance.RegistryManager.Save();
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

            // Mono throws an uninformative error if the file we try to run is not flagged as executable, mark it as such.
            if (Util.IsLinux)
            {
                // Create the command with the filename of the currently running assembly.
                string command = string.Format("+x \"{0}\"", System.Reflection.Assembly.GetExecutingAssembly().Location);

                ProcessStartInfo permsinfo = new ProcessStartInfo("chmod", command);
                permsinfo.UseShellExecute = false;

                // Execute the command.
                Process permsprocess = Process.Start(permsinfo);

                // Wait for chmod to finish and check the exit code.
                permsprocess.WaitForExit();

                // chmod returns 0 for successfull operation.
                if (permsprocess.ExitCode != 0)
                {
                    throw new Kraken("Could not mark CKAN as executable.");
                }
            }

            ProcessStartInfo sinfo = new ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            sinfo.UseShellExecute = false;

            Process.Start(sinfo);
            Environment.Exit(0);
        }

        private void ReposListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeleteRepoButton.Enabled = ReposListBox.SelectedItem != null;

            if (ReposListBox.SelectedIndex > 0 && ReposListBox.SelectedIndex < ReposListBox.Items.Count)
            {
                UpRepoButton.Enabled = true;
            }
            else
            {
                UpRepoButton.Enabled = false;
            }

            if (ReposListBox.SelectedIndex  < ReposListBox.Items.Count - 1 && ReposListBox.SelectedIndex >= 0)
            {
                DownRepoButton.Enabled = true;
            }
            else
            {
                DownRepoButton.Enabled = false;
            }
        }

        private void DeleteRepoButton_Click(object sender, EventArgs e)
        {
            if (ReposListBox.SelectedItem == null)
            {
                return;
            }

            var item = (Repository)ReposListBox.SelectedItem;
            Main.Instance.CurrentInstance.Registry.Repositories.Remove(item.name);
            RefreshReposListBox();
            DeleteRepoButton.Enabled = false;
        }

        private void NewRepoButton_Click(object sender, EventArgs e)
        {
            var dialog = new NewRepoDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var repo = dialog.RepoUrlTextBox.Text.Split('|');
                    var name = repo[0].Trim();
                    var url = repo[1].Trim();

                    SortedDictionary<string, Repository> repositories = Main.Instance.CurrentInstance.Registry.Repositories;
                    if (repositories.ContainsKey(name))
                    {
                        repositories.Remove(name);
                    }

                    repositories.Add(name, new Repository(name, url));
                    Main.Instance.CurrentInstance.Registry.Repositories = repositories;

                    RefreshReposListBox();
                }
                catch (Exception ex)
                {
                    Main.Instance.m_User.RaiseError("Invalid repo format - should be \"<name> | <url>\"");
                }
            }
        }

        private void UpRepoButton_Click(object sender, EventArgs e)
        {
            if (ReposListBox.SelectedItem == null)
            {
                return;
            }

            if (ReposListBox.SelectedIndex == 0)
            {
                return;
            }

            var item = (Repository)ReposListBox.SelectedItem;
            var aboveItem = (Repository)ReposListBox.Items[ReposListBox.SelectedIndex - 1];
            item.priority = aboveItem.priority - 1;
            RefreshReposListBox();
        }

        private void DownRepoButton_Click(object sender, EventArgs e)
        {
            if (ReposListBox.SelectedItem == null)
            {
                return;
            }

            if (ReposListBox.SelectedIndex == ReposListBox.Items.Count - 1)
            {
                return;
            }

            var item = (Repository)ReposListBox.SelectedItem;
            var belowItem = (Repository)ReposListBox.Items[ReposListBox.SelectedIndex + 1];
            item.priority = belowItem.priority + 1;
            RefreshReposListBox();
        }

    }
}