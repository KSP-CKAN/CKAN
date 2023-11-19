using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class InstallationHistory : UserControl
    {
        public InstallationHistory()
        {
            InitializeComponent();
        }

        public void LoadHistory(GameInstance inst, GUIConfiguration config, RepositoryDataManager repoData)
        {
            this.inst     = inst;
            registry = RegistryManager.Instance(inst, repoData).registry;
            this.config   = config;
            Util.Invoke(this, () =>
            {
                UseWaitCursor = true;
                Task.Factory.StartNew(() =>
                {
                    var items = inst.InstallHistoryFiles()
                                    .Select(fi => new ListViewItem(fi.CreationTime.ToString("g"))
                                                  {
                                                      Tag = fi
                                                  })
                                    .ToArray();
                    Util.Invoke(this, () =>
                    {
                        HistoryListView.BeginUpdate();
                        HistoryListView.Items.Clear();
                        HistoryListView.Items.AddRange(items);
                        HistoryListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                        Splitter.Panel1MinSize = Splitter.SplitterDistance =
                            TimestampColumn.Width
                                + SystemInformation.VerticalScrollBarWidth
                                + (4 * SystemInformation.BorderSize.Width);
                        HistoryListView.EndUpdate();
                        HistoryListView_ItemSelectionChanged(null, null);
                        UseWaitCursor = false;
                    });
                });
            });
        }

        /// <summary>
        /// Invoked when the user selects a module
        /// </summary>
        public event Action<CkanModule> OnSelectedModuleChanged;

        /// <summary>
        /// Invoked when the user clicks the Install toolbar button
        /// </summary>
        public event Action<CkanModule[]> Install;

        /// <summary>
        /// Invoked when the user clicks OK
        /// </summary>
        public event Action Done;

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.InstallationHistory);
        }

        private void HistoryListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            UseWaitCursor = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var path = HistoryListView.SelectedItems
                                              .Cast<ListViewItem>()
                                              .Select(lvi => lvi.Tag as FileInfo)
                                              .First();
                    var modRows = CkanModule.FromFile(path.FullName)
                                            .depends
                                            .OfType<ModuleRelationshipDescriptor>()
                                            .Select(ItemFromRelationship)
                                            .Where(row => row != null)
                                            .ToArray();
                    Util.Invoke(this, () =>
                    {
                        ModsListView.BeginUpdate();
                        ModsListView.Items.Clear();
                        if (modRows.Length > 0)
                        {
                            if (!ModsListView.ShowGroups)
                            {
                                // Work around first group header not showing up on first click
                                ModsListView.EndUpdate();
                                ModsListView.ShowGroups = true;
                                ModsListView.HeaderStyle = ColumnHeaderStyle.Clickable;
                                ModsListView.BeginUpdate();
                            }
                            ModsListView.Items.AddRange(modRows);
                        }
                        else
                        {
                            ModsListView.ShowGroups = false;
                            ModsListView.HeaderStyle = ColumnHeaderStyle.None;
                            ModsListView.Items.Add(NoModsMessage);
                        }
                        // Don't auto-resize version or author columns
                        ModsListView.AutoResizeColumn(NameColumn.Index,        ColumnHeaderAutoResizeStyle.ColumnContent);
                        ModsListView.AutoResizeColumn(DescriptionColumn.Index, ColumnHeaderAutoResizeStyle.ColumnContent);
                        ModsListView.EndUpdate();
                        OnSelectedModuleChanged?.Invoke(null);
                        UseWaitCursor = false;
                    });
                }
                catch
                {
                    Util.Invoke(this, () =>
                    {
                        ModsListView.BeginUpdate();
                        ModsListView.Items.Clear();
                        ModsListView.ShowGroups = false;
                        ModsListView.HeaderStyle = ColumnHeaderStyle.None;
                        ModsListView.Items.Add(SelectInstallMessage);
                        ModsListView.AutoResizeColumn(NameColumn.Index, ColumnHeaderAutoResizeStyle.ColumnContent);
                        ModsListView.EndUpdate();
                        UseWaitCursor = false;
                    });
                }
            });
        }

        private ListViewItem ItemFromRelationship(ModuleRelationshipDescriptor rel)
        {
            var mod = registry.GetModuleByVersion(rel.name, rel.version)
                      ?? SaneLatestAvail(rel.name);
            return mod == null
                ? new ListViewItem(new string[]
                  {
                      rel.name,
                      rel.version.ToString(config.HideEpochs, config.HideV),
                      "???",
                      "???"
                  })
                  {
                      Tag   = rel,
                      Group = registry.IsInstalled(rel.name, false)
                              ? InstalledGroup
                              : NotInstalledGroup
                  }
                : mod.IsDLC
                    // Never show DLC
                    ? null
                    : new ListViewItem(new string[]
                      {
                          mod.name,
                          mod.version.ToString(config.HideEpochs, config.HideV),
                          string.Join(", ", mod.author),
                          mod.@abstract
                      })
                      {
                          Tag   = mod,
                          Group = registry.IsInstalled(mod.identifier, false)
                                  ? InstalledGroup
                                  : NotInstalledGroup
                      };
        }

        // Registry.LatestAvailable without exceptions
        private CkanModule SaneLatestAvail(string identifier)
        {
            try
            {
                return registry.LatestAvailable(identifier, inst.VersionCriteria());
            }
            catch
            {
                try
                {
                    return registry.LatestAvailable(identifier, null);
                }
                catch
                {
                    return null;
                }
            }
        }

        private void ModsListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var mod = ModsListView.SelectedItems
                                  .Cast<ListViewItem>()
                                  .Select(lvi => lvi.Tag as CkanModule)
                                  .FirstOrDefault();
            if (mod != null)
            {
                OnSelectedModuleChanged?.Invoke(mod);
            }
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            Install?.Invoke(ModsListView.Items
                                        .Cast<ListViewItem>()
                                        .Where(lvi => lvi.Group == NotInstalledGroup)
                                        .Select(lvi => lvi.Tag)
                                        .OfType<CkanModule>()
                                        .ToArray());
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            Done?.Invoke();
        }

        private GameInstance     inst;
        private Registry         registry;
        private GUIConfiguration config;
    }
}
