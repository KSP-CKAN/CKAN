using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.Versioning;

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
            Toolbar.Renderer = new FlatToolStripRenderer();
            Toolbar.ScaleFonts();
            ModsListView.EnsureReadableGroupHeaders();
        }

        public void LoadHistory(GameInstance          inst,
                                GUIConfiguration      config,
                                RepositoryDataManager repoData)
        {
            this.inst   = inst;
            registry    = RegistryManager.Instance(inst, repoData).registry;
            this.config = config;
            var token = ++historyToken;
            Util.Invoke(this, () =>
            {
                loading = true;
                selection.Reset();
                // Ignore anything still being computed for the previous contents
                ++selectionToken;
                HistoryListView.Items.Clear();
                ModsListView.Items.Clear();
                ModsListView.Items.Add(LoadingMessage);
                UseWaitCursor = true;
                Task.Run(() =>
                {
                    var items = inst.InstallHistoryFiles()
                                    .Select(fi => new ListViewItem(fi.CreationTime.ToString("g"))
                                                  {
                                                      Tag = fi
                                                  })
                                    .ToArray();
                    Util.Invoke(this, () =>
                    {
                        if (token != historyToken)
                        {
                            return;
                        }
                        HistoryListView.BeginUpdate();
                        HistoryListView.Items.AddRange(items);
                        HistoryListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                        Splitter.Panel1MinSize = Splitter.SplitterDistance =
                            TimestampColumn.Width
                                + SystemInformation.VerticalScrollBarWidth
                                + (4 * SystemInformation.BorderSize.Width);
                        HistoryListView.EndUpdate();
                        loading = false;
                        ShowSelection();
                        UseWaitCursor = false;
                    });
                });
            });
        }

        /// <summary>
        /// Invoked when the user selects a module
        /// </summary>
        public event Action<CkanModule?>? OnSelectedModuleChanged;

        /// <summary>
        /// Invoked when the user clicks the Install toolbar button
        /// </summary>
        public event Action<CkanModule[]>? Install;

        /// <summary>
        /// Invoked when the user clicks OK
        /// </summary>
        public event Action? Done;

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.InstallationHistory);
        }

        private void HistoryListView_ItemSelectionChanged(object? sender, ListViewItemSelectionChangedEventArgs? e)
        {
            if (adjustingSelection || e == null)
            {
                return;
            }
            if (e.IsSelected)
            {
                selection.Select(e.ItemIndex,
                                 ModifierKeys.HasFlag(Keys.Control),
                                 ModifierKeys.HasFlag(Keys.Shift),
                                 HistoryListView.SelectedIndices.Cast<int>().ToArray(),
                                 HistoryListView.FocusedItem?.Index);
                HighlightSelection();
            }
            else if (ModifierKeys.HasFlag(Keys.Control))
            {
                selection.Deselect(e.ItemIndex);
                HighlightSelection();
            }
            // A click makes several changes, wait for all of them
            if (!showPending && IsHandleCreated)
            {
                showPending = true;
                BeginInvoke(new Action(() =>
                {
                    showPending = false;
                    if (!loading)
                    {
                        // Plain clicks on selected entries or on empty space deselect without telling us why
                        selection.Reconcile(HistoryListView.SelectedIndices.Cast<int>().ToArray());
                        ShowSelection();
                    }
                }));
            }
        }

        private void ChangesCheckBox_CheckedChanged(object? sender, EventArgs? e)
        {
            // While loading there is nothing to show yet, the list will call ShowSelection itself
            if (!loading)
            {
                ShowSelection();
            }
        }

        /// <summary>
        /// Highlight only the entries that take part in the comparison
        /// </summary>
        private void HighlightSelection()
        {
            var wanted = selection.Indices;
            adjustingSelection = true;
            try
            {
                foreach (var index in HistoryListView.SelectedIndices.Cast<int>().ToArray())
                {
                    if (!wanted.Contains(index))
                    {
                        HistoryListView.Items[index].Selected = false;
                    }
                }
                foreach (var index in wanted)
                {
                    HistoryListView.Items[index].Selected = true;
                }
            }
            finally
            {
                adjustingSelection = false;
            }
        }

        private void ShowSelection()
        {
            if (selection.Compared(HistoryListView.Items.Count) is not { } pair
                || EntryFile(pair.Newer) is not FileInfo newer)
            {
                ShowMessage(SelectInstallMessage);
                return;
            }
            var older = pair.Older is int olderIndex ? EntryFile(olderIndex) : null;
            // Picking two entries is always a comparison, the button is for the single entry
            var diff = selection.Other != null || ChangesCheckBox.Checked;
            var loader = new RowLoader(registry, config, inst);
            var token = ++selectionToken;
            UseWaitCursor = true;
            Task.Run(() =>
            {
                try
                {
                    // Only data in here, the controls are for the UI thread
                    var rows = loader.Load(newer, older, diff);
                    Util.Invoke(this, () =>
                    {
                        if (token == selectionToken)
                        {
                            ShowRows(rows, diff);
                        }
                    });
                }
                catch
                {
                    Util.Invoke(this, () =>
                    {
                        if (token == selectionToken)
                        {
                            ShowMessage(SelectInstallMessage);
                        }
                    });
                }
            });
        }

        private FileInfo? EntryFile(int index)
            => index < HistoryListView.Items.Count
                   ? HistoryListView.Items[index].Tag as FileInfo
                   : null;

        private void ShowMessage(ListViewItem message)
        {
            ModsListView.BeginUpdate();
            ModsListView.Items.Clear();
            ModsListView.ShowGroups = false;
            ModsListView.HeaderStyle = ColumnHeaderStyle.None;
            ModsListView.Items.Add(message);
            ModsListView.AutoResizeColumn(NameColumn.Index, ColumnHeaderAutoResizeStyle.ColumnContent);
            ModsListView.EndUpdate();
            UseWaitCursor = false;
        }

        private void ShowRows(HistoryRow[] rows, bool diff)
        {
            ModsListView.BeginUpdate();
            ModsListView.Items.Clear();
            if (rows.Length > 0)
            {
                if (!ModsListView.ShowGroups)
                {
                    // Work around first group header not showing up on first click
                    ModsListView.EndUpdate();
                    ModsListView.ShowGroups = true;
                    ModsListView.HeaderStyle = ColumnHeaderStyle.Clickable;
                    ModsListView.BeginUpdate();
                }
                ModsListView.Items.AddRange(rows.Select(ItemFromRow).ToArray());
            }
            else
            {
                ModsListView.ShowGroups = false;
                ModsListView.HeaderStyle = ColumnHeaderStyle.None;
                ModsListView.Items.Add(diff ? NoChangesMessage : NoModsMessage);
            }
            // Don't auto-resize author column, version only to fit "old -> new"
            ModsListView.AutoResizeColumn(NameColumn.Index,        ColumnHeaderAutoResizeStyle.ColumnContent);
            ModsListView.AutoResizeColumn(DescriptionColumn.Index, ColumnHeaderAutoResizeStyle.ColumnContent);
            if (diff)
            {
                ModsListView.AutoResizeColumn(VersionColumn.Index, ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            // Install only makes sense for the full list
            InstallButton.Enabled = !diff;
            ModsListView.EndUpdate();
            OnSelectedModuleChanged?.Invoke(null);
            UseWaitCursor = false;
        }

        private ListViewItem ItemFromRow(HistoryRow row)
            => new ListViewItem(row.Cells)
               {
                   Tag   = row.Module,
                   Group = row.Change switch
                   {
                       HistoryChangeType.Added      => AddedGroup,
                       HistoryChangeType.Removed    => RemovedGroup,
                       HistoryChangeType.Upgraded   => UpgradedGroup,
                       HistoryChangeType.Downgraded => DowngradedGroup,
                       _                            => row.Installed ? InstalledGroup
                                                                     : NotInstalledGroup,
                   },
               };

        private void ModsListView_ItemSelectionChanged(object? sender, ListViewItemSelectionChangedEventArgs? e)
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

        private void InstallButton_Click(object? sender, EventArgs? e)
        {
            Install?.Invoke(ModsListView.Items
                                        .Cast<ListViewItem>()
                                        .Where(lvi => lvi.Group == NotInstalledGroup)
                                        .Select(lvi => lvi.Tag)
                                        .OfType<CkanModule>()
                                        .ToArray());
        }

        private void OKButton_Click(object? sender, EventArgs? e)
        {
            Done?.Invoke();
        }

        private GameInstance?     inst;
        private Registry?         registry;
        private GUIConfiguration? config;

        // Incremented to discard results that are no longer wanted
        private int  historyToken;
        private int  selectionToken;
        // True while we are changing the selected entries ourselves
        private bool adjustingSelection;
        // True while the list of entries is being loaded
        private bool loading;
        // True when a refresh of the mod list is already queued
        private bool showPending;

        private readonly ComparisonSelection selection = new ComparisonSelection();

        /// <summary>
        /// A row of the mod list that doesn't touch any controls,
        /// so it can be prepared on another thread
        /// </summary>
        private sealed class HistoryRow
        {
            public HistoryRow(string[]           cells,
                              CkanModule?        module,
                              bool               installed,
                              HistoryChangeType? change)
            {
                Cells     = cells;
                Module    = module;
                Installed = installed;
                Change    = change;
            }

            public readonly string[]           Cells;
            public readonly CkanModule?        Module;
            public readonly bool               Installed;
            public readonly HistoryChangeType? Change;
        }

        /// <summary>
        /// Reads history files and looks up their mods
        /// </summary>
        private sealed class RowLoader
        {
            public RowLoader(Registry?         registry,
                             GUIConfiguration? config,
                             GameInstance?     inst)
            {
                this.registry = registry;
                this.config   = config;
                this.inst     = inst;
            }

            /// <summary>
            /// Get the rows for an entry
            /// </summary>
            /// <param name="newer">The entry to show</param>
            /// <param name="older">The entry to compare with, null if there is none</param>
            /// <param name="diff">
            /// True to show only what changed since the older entry,
            /// everything counts as added if there is none.
            /// False to show all the mods of the newer entry.
            /// </param>
            public HistoryRow[] Load(FileInfo newer, FileInfo? older, bool diff)
                => diff ? LoadChanges(older, newer)
                        : LoadAll(newer);

            private HistoryRow[] LoadAll(FileInfo file)
                => CkanModule.FromFile(file.FullName)
                             .depends
                             ?.OfType<ModuleRelationshipDescriptor>()
                              .Where(rel => rel.version != null)
                              .Select(rel => RowFromVersion(rel.name, rel.version!, null))
                              .OfType<HistoryRow>()
                              .ToArray()
                            ?? Array.Empty<HistoryRow>();

            private HistoryRow[] LoadChanges(FileInfo? older, FileInfo newer)
                => InstallHistoryDiff.Between(older == null ? null
                                                            : CkanModule.FromFile(older.FullName),
                                              CkanModule.FromFile(newer.FullName))
                                     .Select(change => RowFromVersion(change.Identifier,
                                                                      // Both can't be null for a change
                                                                      (change.After ?? change.Before)!,
                                                                      change))
                                     .OfType<HistoryRow>()
                                     .ToArray();

            /// <summary>
            /// Look up a mod and make a row for it
            /// </summary>
            /// <param name="identifier">Identifier of the mod</param>
            /// <param name="version">The version the history says was installed</param>
            /// <param name="change">What happened to the mod, null if we are not showing a comparison</param>
            private HistoryRow? RowFromVersion(string identifier, ModuleVersion version, HistoryChange? change)
            {
                if (registry == null || config == null)
                {
                    return null;
                }
                var mod = registry.GetModuleByVersion(identifier, version)
                          ?? SaneLatestAvail(identifier);
                if (mod != null && mod.IsDLC)
                {
                    // Never show DLC
                    return null;
                }
                return new HistoryRow(new string[]
                                      {
                                          mod?.name ?? identifier,
                                          change != null ? ChangeText(change, config)
                                                         : (mod?.version ?? version).ToString(config.HideEpochs, config.HideV),
                                          mod == null ? "???" : string.Join(", ", mod.author),
                                          mod == null ? "???" : mod.@abstract
                                      },
                                      mod,
                                      registry.IsInstalled(identifier, false),
                                      change?.Type);
            }

            // The registry may only have other versions of the mod,
            // so for a comparison show what the history says
            private static string ChangeText(HistoryChange change, GUIConfiguration config)
            {
                var before = change.Before?.ToString(config.HideEpochs, config.HideV);
                var after  = change.After?.ToString(config.HideEpochs, config.HideV);
                return before != null && after != null ? $"{before} -> {after}"
                                                       : after ?? before ?? "";
            }

            // Registry.LatestAvailable without exceptions
            private CkanModule? SaneLatestAvail(string identifier)
            {
                if (inst != null)
                {
                    var stabilityTolerance = inst.StabilityToleranceConfig;
                    try
                    {
                        return registry?.LatestAvailable(identifier, stabilityTolerance, inst.VersionCriteria());
                    }
                    catch
                    {
                        try
                        {
                            return registry?.LatestAvailable(identifier, stabilityTolerance, null);
                        }
                        catch
                        {
                        }
                    }
                }
                return null;
            }

            private readonly Registry?         registry;
            private readonly GUIConfiguration? config;
            private readonly GameInstance?     inst;
        }
    }
}
