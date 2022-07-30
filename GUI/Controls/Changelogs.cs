using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using CKAN.Versioning;
using System.Runtime.Caching;

namespace CKAN
{
    public partial class Changelogs : UserControl
    {
        public Changelogs()
        {
            InitializeComponent();
            getters.Add(new GithubChangelogGetter());
            cache = new MemoryCache("ChangelogsCache");
        }

        private MemoryCache cache = null;

        readonly List<IChangelogGetter> getters = new List<IChangelogGetter>();

        private GUIMod _selectedModule;
        public GUIMod SelectedModule
        {
            set
            {
                richTextBox1.Text = "... Changelog not loaded - press the button to start loading ...";
                button1.Enabled = true;
                this._selectedModule = value;
                if (value == null)
                {
                    richTextBox1.Text = "... Changelog not available ...";
                    button1.Enabled = false;
                    return;
                }

                // Get all the data; can put this in bg if slow
                GameInstance currentInstance = Main.Instance.Manager.CurrentInstance;
                IRegistryQuerier registry = RegistryManager.Instance(currentInstance).registry;

                ModuleVersion installedVersion = registry.InstalledVersion(value.Identifier);
                CkanModule latest = registry.LatestAvailable(value.Identifier, currentInstance.VersionCriteria());
                bool cached = cache.Contains(value.Identifier);
                if (!cached && latest == null) return;
                if (!cached && latest.version.IsEqualTo(installedVersion)) return;

                ShowChangelogs();
            }
            get
            {
                return this._selectedModule;
            }
        }

        private void ShowChangelogs()
        {
            button1.Enabled = false;
            GUIMod value = this.SelectedModule;
            if (value == null) return;
            GameInstance currentInstance = Main.Instance.Manager.CurrentInstance;
            IRegistryQuerier registry = RegistryManager.Instance(currentInstance).registry;
            ModuleVersion installedVersion = registry.InstalledVersion(value.Identifier);

            richTextBox1.Text = "";
            string outtext = "";
            Uri repository = registry.GetModuleByVersion(value.Identifier, installedVersion).resources.repository;
            ChangelogData cacheEntry = (ChangelogData)cache.Get(value.Identifier);
            ModuleVersion latestVersion = registry.LatestAvailable(value.Identifier, currentInstance.VersionCriteria()).version;
            if (cacheEntry != null && cacheEntry.latestKnownVersion.IsLessThan(latestVersion))
            {
                cache.Remove(value.Identifier);
                cacheEntry = null;
            }

            List<ChangelogResultRow> result = new List<ChangelogResultRow>();

            if (cacheEntry == null)
            {
                foreach (var getter in getters)
                {
                    if (getter.CanHandle(repository))
                    {
                        result = getter.GetChangelog(repository);
                        cache.Set(value.Identifier, new ChangelogData(result, latestVersion), new CacheItemPolicy());
                        break;
                    }
                }
            }
            else
            {
                result = cacheEntry.rows;
            }

            outtext = String.Join("\n\n------------------------\n\n", from r in result select r.tagName + " := " + r.body);


            if (outtext.Length > 0)
            {
                richTextBox1.AppendText(outtext);
                richTextBox1.SelectionStart = 0;
                richTextBox1.ScrollToCaret();
            } else
            {
                richTextBox1.Text = "... Changelog could not be loaded ...";
            }
        }

        private class ChangelogResultRow
        {
            public string tagName;
            public string body;
            public ChangelogResultRow(string tagName, string body)
            {
                this.tagName = tagName;
                this.body = body;
            }
        }

        private class ChangelogData
        {
            public List<ChangelogResultRow> rows;
            public ModuleVersion latestKnownVersion;
            public ChangelogData(List<ChangelogResultRow> rows, ModuleVersion latestKnownVersion)
            {
                this.rows = rows;
                this.latestKnownVersion = latestKnownVersion;
            }
        }

        private interface IChangelogGetter
        {
            bool CanHandle(Uri repository);
            List<ChangelogResultRow> GetChangelog(Uri repository);
        }

        private class GithubChangelogGetter : IChangelogGetter
        {
            public bool CanHandle(Uri repository)
            {
                if (repository == null) return false;
                return repository.Host.Equals("github.com");
            }

            public List<ChangelogResultRow> GetChangelog(Uri repository)
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                UriBuilder apiEndpoint = new UriBuilder(repository);
                apiEndpoint.Host = "api.github.com";
                apiEndpoint.Path = "/repos" + apiEndpoint.Path;
                if (!apiEndpoint.Path.EndsWith("/releases"))
                {
                    apiEndpoint.Path = apiEndpoint.Path + "/releases"; // some modules already had /releases list in their repo url
                }
                var client = new System.Net.WebClient();
                client.Headers.Set(System.Net.HttpRequestHeader.UserAgent, "jkavalik/CKAN-dev");
                List <ChangelogResultRow> results;
                try
                {
                    string jsonResponse = client.DownloadString(apiEndpoint.Uri);
                    Newtonsoft.Json.Linq.JToken json = Newtonsoft.Json.Linq.JToken.Parse(jsonResponse);
                    var data = from r in json select new ChangelogResultRow((string)r.SelectToken("$.tag_name"), (string)r.SelectToken("$.body"));
                    results = data.ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    results = new List<ChangelogResultRow>();
                    return results;
                }
                
                return results;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShowChangelogs();
        }
    }
}
