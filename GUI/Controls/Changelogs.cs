using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using CKAN.Versioning;

namespace CKAN
{
    public partial class Changelogs : UserControl
    {
        public Changelogs()
        {
            InitializeComponent();
            getters.Add(new GithubChangelogGetter());
        }

        readonly List<IChangelogGetter> getters = new List<IChangelogGetter>();

        public GUIMod SelectedModule
        {
            set
            {
                if (value == null)
                {
                    return;
                }

                // Get all the data; can put this in bg if slow
                GameInstance currentInstance = Main.Instance.Manager.CurrentInstance;
                IRegistryQuerier registry = RegistryManager.Instance(currentInstance).registry;

                ModuleVersion installedVersion = registry.InstalledVersion(value.Identifier);

                richTextBox1.Text = installedVersion.ToString();
                richTextBox1.Text = "";
                string outtext = "";
                foreach(var getter in getters)
                {
                    Uri repository = registry.GetModuleByVersion(value.Identifier, installedVersion).resources.repository;
                    if (getter.CanHandle(repository))
                    {
                        var result = getter.GetChangelog(repository);
                        outtext = String.Join("\n\n------------------------\n\n", from r in result select r.tagName + " := " + r.body);
                        break;
                    }
                }
                richTextBox1.AppendText(outtext);
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
    }
}
