using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;
using log4net;

using CKAN.Extensions;
using CKAN.Versioning;
using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class ManageMods : UserControl
    {
        public ManageMods()
        {
            InitializeComponent();

            ToolTip.SetToolTip(InstallAllCheckbox, Properties.Resources.ManageModsInstallAllCheckboxTooltip);
            FilterCompatibleButton.ToolTipText      = Properties.Resources.FilterLinkToolTip;
            FilterInstalledButton.ToolTipText       = Properties.Resources.FilterLinkToolTip;
            FilterInstalledUpdateButton.ToolTipText = Properties.Resources.FilterLinkToolTip;
            FilterReplaceableButton.ToolTipText     = Properties.Resources.FilterLinkToolTip;
            FilterCachedButton.ToolTipText          = Properties.Resources.FilterLinkToolTip;
            FilterUncachedButton.ToolTipText        = Properties.Resources.FilterLinkToolTip;
            FilterNewButton.ToolTipText             = Properties.Resources.FilterLinkToolTip;
            FilterNotInstalledButton.ToolTipText    = Properties.Resources.FilterLinkToolTip;
            FilterIncompatibleButton.ToolTipText    = Properties.Resources.FilterLinkToolTip;

            mainModList = new ModList();
            mainModList.ModFiltersUpdated += UpdateFilters;
            FilterToolButton.MouseHover += (sender, args) => FilterToolButton.ShowDropDown();
            ApplyToolButton.MouseHover += (sender, args) => ApplyToolButton.ShowDropDown();
            ApplyToolButton.Enabled = false;

            repoData = ServiceLocator.Container.Resolve<RepositoryDataManager>();

            // History is read-only until the UI is started. We switch
            // out of it at the end of OnLoad() when we call NavInit().
            navHistory = new NavigationHistory<GUIMod> { IsReadOnly = true };

            // Initialize navigation. This should be called as late as
            // possible, once the UI is "settled" from its initial load.
            NavInit();

            if (Platform.IsMono)
            {
                menuStrip2.Renderer = new FlatToolStripRenderer();
                FilterToolButton.DropDown.Renderer = new FlatToolStripRenderer();
                FilterTagsToolButton.DropDown.Renderer = new FlatToolStripRenderer();
                FilterLabelsToolButton.DropDown.Renderer = new FlatToolStripRenderer();
                LaunchGameToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
                ModListContextMenuStrip.Renderer = new FlatToolStripRenderer();
                ModListHeaderContextMenuStrip.Renderer = new FlatToolStripRenderer();
                LabelsContextMenuStrip.Renderer = new FlatToolStripRenderer();
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(ManageMods));
        private readonly RepositoryDataManager repoData;
        private DateTime lastSearchTime;
        private string lastSearchKey;
        private readonly NavigationHistory<GUIMod> navHistory;
        private static readonly Font uninstallingFont = new Font(SystemFonts.DefaultFont, FontStyle.Strikeout);

        private List<ModChange> currentChangeSet;
        private Dictionary<GUIMod, string> conflicts;
        private bool freezeChangeSet = false;

        public event Action<string> RaiseMessage;
        public event Action<string> RaiseError;
        public event Action<string> SetStatusBar;
        public event Action         ClearStatusBar;
        public event Action<string> LaunchGame;
        public event Action         EditCommandLines;

        public readonly ModList mainModList;
        private List<string> SortColumns
        {
            get
            {
                // Make sure we don't return any column the GUI doesn't know about.
                var unknownCols = guiConfig.SortColumns.Where(col => !ModGrid.Columns.Contains(col)).ToList();
                foreach (var unknownCol in unknownCols)
                {
                    int index = guiConfig.SortColumns.IndexOf(unknownCol);
                    guiConfig.SortColumns.RemoveAt(index);
                    guiConfig.MultiSortDescending.RemoveAt(index);
                }
                return guiConfig.SortColumns;
            }
        }

        private GUIConfiguration    guiConfig       => Main.Instance.configuration;
        private GameInstance        currentInstance => Main.Instance.CurrentInstance;
        private GameInstanceManager manager         => Main.Instance.Manager;
        private IUser               user            => Main.Instance.currentUser;

        private List<bool> descending => guiConfig.MultiSortDescending;

        public event Action<GUIMod> OnSelectedModuleChanged;
        public event Action<List<ModChange>, Dictionary<GUIMod, string>> OnChangeSetChanged;
        public event Action OnRegistryChanged;

        public event Action<List<ModChange>, Dictionary<GUIMod, string>> StartChangeSet;
        public event Action<IEnumerable<GUIMod>> LabelsAfterUpdate;

        private void EditModSearches_ShowError(string error)
        {
            RaiseError?.Invoke(error);
        }

        private List<ModChange> ChangeSet
        {
            get => currentChangeSet;
            [ForbidGUICalls]
            set
            {
                var orig = currentChangeSet;
                currentChangeSet = value;
                if (!ReferenceEquals(orig, value))
                {
                    ChangeSetUpdated();
                }
            }
        }

        [ForbidGUICalls]
        private void ChangeSetUpdated()
        {
            Util.Invoke(this, () =>
            {
                if (ChangeSet != null && ChangeSet.Any())
                {
                    ApplyToolButton.Enabled = true;
                }
                else
                {
                    ApplyToolButton.Enabled = false;
                    InstallAllCheckbox.Checked = true;
                }
                OnChangeSetChanged?.Invoke(ChangeSet, Conflicts);

                var removing = changeIdentifiersOfType(GUIModChangeType.Remove)
                               .Except(changeIdentifiersOfType(GUIModChangeType.Install))
                               .ToHashSet();
                foreach ((string ident, DataGridViewRow row) in mainModList.full_list_of_mod_rows)
                {
                    if (removing.Contains(ident))
                    {
                        // Set strikeout font for rows being uninstalled
                        row.DefaultCellStyle.Font = uninstallingFont;
                    }
                    else if (row.DefaultCellStyle.Font != null)
                    {
                        // Clear strikeout font for rows not being uninstalled
                        row.DefaultCellStyle.Font = null;
                    }
                }
            });
        }

        private IEnumerable<string> changeIdentifiersOfType(GUIModChangeType changeType)
            => (currentChangeSet ?? Enumerable.Empty<ModChange>())
                .Where(ch => ch?.ChangeType == changeType)
                .Select(ch => ch.Mod.identifier);

        private Dictionary<GUIMod, string> Conflicts
        {
            get => conflicts;
            [ForbidGUICalls]
            set
            {
                var orig = conflicts;
                conflicts = value;
                if (orig != value)
                {
                    Util.Invoke(this, () => ConflictsUpdated(orig));
                }
            }
        }

        private void ConflictsUpdated(Dictionary<GUIMod, string> prevConflicts)
        {
            if (Conflicts == null)
            {
                // Clear status bar if no conflicts
                ClearStatusBar?.Invoke();
            }

            var registry = RegistryManager.Instance(currentInstance, repoData).registry;
            if (prevConflicts != null)
            {
                // Mark old conflicts as non-conflicted
                // (rows that are _still_ conflicted will be marked as such in the next loop)
                foreach (GUIMod guiMod in prevConflicts.Keys)
                {
                    SetUnsetRowConflicted(guiMod, false, null, currentInstance, registry);
                }
            }
            if (Conflicts != null)
            {
                // Mark current conflicts as conflicted
                foreach ((GUIMod guiMod, string conflict_text) in Conflicts)
                {
                    SetUnsetRowConflicted(guiMod, true, conflict_text, currentInstance, registry);
                }
            }
        }

        private void SetUnsetRowConflicted(GUIMod       guiMod,
                                           bool         conflicted,
                                           string       tooltip,
                                           GameInstance inst,
                                           Registry     registry)
        {
            var row = mainModList.ReapplyLabels(guiMod, conflicted,
                                                inst.Name, inst.game, registry);
            if (row != null)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.ToolTipText = tooltip;
                }
                if (row.Visible)
                {
                    ModGrid.InvalidateRow(row.Index);
                }
            }
        }

        private void RefreshToolButton_Click(object sender, EventArgs e)
        {
            // If user is holding Shift or Ctrl, force a full update
            Main.Instance.UpdateRepo(ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift));
        }

        #region Filter dropdown

        private void FilterToolButton_DropDown_Opening(object sender, CancelEventArgs e)
        {
            // The menu items' dropdowns can't be accessed if they're empty
            FilterTagsToolButton_DropDown_Opening(null, null);
            FilterLabelsToolButton_DropDown_Opening(null, null);
        }

        private void FilterTagsToolButton_DropDown_Opening(object sender, CancelEventArgs e)
        {
            var registry = RegistryManager.Instance(currentInstance, repoData).registry;
            FilterTagsToolButton.DropDownItems.Clear();
            foreach (var kvp in registry.Tags.OrderBy(kvp => kvp.Key))
            {
                FilterTagsToolButton.DropDownItems.Add(new ToolStripMenuItem(
                    $"{kvp.Key} ({kvp.Value.ModuleIdentifiers.Count})",
                    null, tagFilterButton_Click
                )
                {
                    Tag         = kvp.Value,
                    ToolTipText = Properties.Resources.FilterLinkToolTip,
                });
            }
            FilterTagsToolButton.DropDownItems.Add(untaggedFilterToolStripSeparator);
            FilterTagsToolButton.DropDownItems.Add(new ToolStripMenuItem(
                string.Format(Properties.Resources.MainLabelsUntagged, registry.Untagged.Count),
                null, tagFilterButton_Click
            )
            {
                Tag = null
            });
        }

        private void FilterLabelsToolButton_DropDown_Opening(object sender, CancelEventArgs e)
        {
            FilterLabelsToolButton.DropDownItems.Clear();
            foreach (ModuleLabel mlbl in mainModList.ModuleLabels.LabelsFor(currentInstance.Name))
            {
                FilterLabelsToolButton.DropDownItems.Add(new ToolStripMenuItem(
                    $"{mlbl.Name} ({mlbl.ModuleCount(currentInstance.game)})",
                    null, customFilterButton_Click
                )
                {
                    Tag         = mlbl,
                    ToolTipText = Properties.Resources.FilterLinkToolTip,
                });
            }
        }

        #endregion

        #region Filter right click menu

        private void LabelsContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            LabelsContextMenuStrip.Items.Clear();

            var module = SelectedModule;
            foreach (ModuleLabel mlbl in mainModList.ModuleLabels.LabelsFor(currentInstance.Name))
            {
                LabelsContextMenuStrip.Items.Add(
                    new ToolStripMenuItem(mlbl.Name, null, labelMenuItem_Click)
                    {
                        BackColor    = mlbl.Color,
                        Checked      = mlbl.ContainsModule(currentInstance.game, module.Identifier),
                        CheckOnClick = true,
                        Tag          = mlbl,
                    }
                );
            }
            LabelsContextMenuStrip.Items.Add(labelToolStripSeparator);
            LabelsContextMenuStrip.Items.Add(editLabelsToolStripMenuItem);
            e.Cancel = false;
        }

        private void labelMenuItem_Click(object sender, EventArgs e)
        {
            var item   = sender   as ToolStripMenuItem;
            var mlbl   = item.Tag as ModuleLabel;
            var module = SelectedModule;
            if (item.Checked)
            {
                mlbl.Add(currentInstance.game, module.Identifier);
            }
            else
            {
                mlbl.Remove(currentInstance.game, module.Identifier);
            }
            var registry = RegistryManager.Instance(currentInstance, repoData).registry;
            mainModList.ReapplyLabels(module, Conflicts?.ContainsKey(module) ?? false,
                                      currentInstance.Name, currentInstance.game, registry);
            mainModList.ModuleLabels.Save(ModuleLabelList.DefaultPath);
            UpdateHiddenTagsAndLabels();
            if (mlbl.HoldVersion)
            {
                UpdateCol.Visible = UpdateAllToolButton.Enabled =
                    mainModList.ResetHasUpdate(currentInstance, registry, ChangeSet, ModGrid.Rows);
            }
        }

        private void editLabelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var eld = new EditLabelsDialog(user, manager, mainModList.ModuleLabels);
            eld.ShowDialog(this);
            eld.Dispose();
            mainModList.ModuleLabels.Save(ModuleLabelList.DefaultPath);
            var registry = RegistryManager.Instance(currentInstance, repoData).registry;
            foreach (var module in mainModList.Modules)
            {
                mainModList.ReapplyLabels(module, Conflicts?.ContainsKey(module) ?? false,
                                          currentInstance.Name, currentInstance.game, registry);
            }
            UpdateHiddenTagsAndLabels();
            UpdateCol.Visible = UpdateAllToolButton.Enabled =
                mainModList.ResetHasUpdate(currentInstance, registry, ChangeSet, ModGrid.Rows);
        }

        #endregion

        private void tagFilterButton_Click(object sender, EventArgs e)
        {
            var clicked = sender as ToolStripMenuItem;
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.Tag, clicked.Tag as ModuleTag, null), merge);
        }

        private void customFilterButton_Click(object sender, EventArgs e)
        {
            var clicked = sender as ToolStripMenuItem;
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.CustomLabel, null, clicked.Tag as ModuleLabel), merge);
        }

        private void FilterCompatibleButton_Click(object sender, EventArgs e)
        {
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.Compatible), merge);
        }

        private void FilterInstalledButton_Click(object sender, EventArgs e)
        {
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.Installed), merge);
        }

        private void FilterInstalledUpdateButton_Click(object sender, EventArgs e)
        {
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.InstalledUpdateAvailable), merge);
        }

        private void FilterReplaceableButton_Click(object sender, EventArgs e)
        {
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.Replaceable), merge);
        }

        private void FilterCachedButton_Click(object sender, EventArgs e)
        {
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.Cached), merge);
        }

        private void FilterUncachedButton_Click(object sender, EventArgs e)
        {
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.Uncached), merge);
        }

        private void FilterNewButton_Click(object sender, EventArgs e)
        {
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.NewInRepository), merge);
        }

        private void FilterNotInstalledButton_Click(object sender, EventArgs e)
        {
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.NotInstalled), merge);
        }

        private void FilterIncompatibleButton_Click(object sender, EventArgs e)
        {
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.Incompatible), merge);
        }

        private void FilterAllButton_Click(object sender, EventArgs e)
        {
            var merge = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            Filter(ModList.FilterToSavedSearch(GUIModFilter.All), merge);
        }

        /// <summary>
        /// Called when the ModGrid filter (all, compatible, incompatible...) is changed.
        /// </summary>
        /// <param name="search">Search string</param>
        /// <param name="merge">If true, merge with current searches, else replace</param>
        public void Filter(SavedSearch search, bool merge)
        {
            var searches = search.Values.Select(s => ModSearch.Parse(s,
                mainModList.ModuleLabels.LabelsFor(currentInstance.Name).ToList()
            )).ToList();

            Util.Invoke(ModGrid, () =>
            {
                if (merge)
                {
                    EditModSearches.MergeSearches(searches);
                }
                else
                {
                    EditModSearches.SetSearches(searches);
                }
                ShowHideColumns(searches);
            });
        }

        public void SetSearches(List<ModSearch> searches)
        {
            Util.Invoke(ModGrid, () =>
            {
                mainModList.SetSearches(searches);
                EditModSearches.SetSearches(searches);
                ShowHideColumns(searches);
            });
        }

        private void ShowHideColumns(List<ModSearch> searches)
        {
            // Ask the configuration which columns to show.
            foreach (DataGridViewColumn col in ModGrid.Columns)
            {
                // Some columns are always shown, and others are handled by UpdateModsList()
                if (col.Name != "Installed" && col.Name != "UpdateCol" && col.Name != "ReplaceCol"
                    && !installedColumnNames.Contains(col.Name))
                {
                    col.Visible = !guiConfig.HiddenColumnNames.Contains(col.Name);
                }
            }

            // If these columns aren't hidden by the user, show them if the search includes installed modules
            setInstalledColumnsVisible(mainModList.HasAnyInstalled
                                       && !SearchesExcludeInstalled(searches)
                                       && mainModList.HasVisibleInstalled());
        }

        private static readonly string[] installedColumnNames = new string[]
        {
            "AutoInstalled", "InstalledVersion", "InstallDate"
        };

        private void setInstalledColumnsVisible(bool visible)
        {
            var hiddenColumnNames = guiConfig.HiddenColumnNames;
            foreach (var colName in installedColumnNames.Where(nm => ModGrid.Columns.Contains(nm)))
            {
                ModGrid.Columns[colName].Visible = visible && !hiddenColumnNames.Contains(colName);
            }
        }

        private static bool SearchesExcludeInstalled(List<ModSearch> searches)
            => searches?.All(s => s != null && s.Installed == false) ?? false;

        public void MarkAllUpdates()
        {
            WithFrozenChangeset(() =>
            {
                foreach (var gmod in mainModList.full_list_of_mod_rows
                                                .Values
                                                .Select(row => row.Tag)
                                                .OfType<GUIMod>())
                {
                    if (gmod?.HasUpdate ?? false)
                    {
                        if (!Main.Instance.LabelsHeld(gmod.Identifier))
                        {
                            gmod.SelectedMod = gmod.LatestCompatibleMod;
                        }
                    }
                }

                // only sort by Update column if checkbox in settings checked
                if (guiConfig.AutoSortByUpdate)
                {
                    // Retain their current sort as secondaries
                    AddSort(UpdateCol, true);
                    UpdateFilters();
                    // Select the top row and scroll the list to it.
                    if (ModGrid.Rows.Count > 0)
                    {
                        ModGrid.CurrentCell = ModGrid.Rows[0].Cells[SelectableColumnIndex()];
                    }
                }
            });
        }

        private void UpdateAllToolButton_Click(object sender, EventArgs e)
        {
            MarkAllUpdates();
        }

        private void ApplyToolButton_Click(object sender, EventArgs e)
        {
            StartChangeSet?.Invoke(currentChangeSet, Conflicts);
        }

        private void LaunchGameToolStripMenuItem_MouseHover(object sender, EventArgs e)
        {
            var cmdLines = guiConfig.CommandLines;
            LaunchGameToolStripMenuItem.DropDownItems.Clear();
            LaunchGameToolStripMenuItem.DropDownItems.AddRange(
                cmdLines.Select(cmdLine => (ToolStripItem)
                                           new ToolStripMenuItem(cmdLine, null,
                                                                 LaunchGameToolStripMenuItem_Click)
                                           {
                                               Tag = cmdLine,
                                               ShortcutKeyDisplayString = CmdLineHelp(cmdLine),
                                           })
                        .Append(CommandLinesToolStripSeparator)
                        .Append(EditCommandLinesToolStripMenuItem)
                        .ToArray());
            LaunchGameToolStripMenuItem.ShowDropDown();
        }

        private string CmdLineHelp(string cmdLine)
            => manager.SteamLibrary.Games.Length > 0
                ? cmdLine.StartsWith("steam://", StringComparison.InvariantCultureIgnoreCase)
                    ? Properties.Resources.ManageModsSteamPlayTimeYesTooltip
                    : Properties.Resources.ManageModsSteamPlayTimeNoTooltip
                : "";

        private void LaunchGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            LaunchGame?.Invoke(menuItem?.Tag as string);
        }

        private void EditCommandLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditCommandLines?.Invoke();
        }

        private void NavBackwardToolButton_Click(object sender, EventArgs e)
        {
            NavGoBackward();
        }

        private void NavForwardToolButton_Click(object sender, EventArgs e)
        {
            NavGoForward();
        }

        private void ModGrid_SelectionChanged(object sender, EventArgs e)
        {
            // Skip if already disposed (i.e. after the form has been closed).
            // Needed for TransparentTextBoxes
            if (IsDisposed)
            {
                return;
            }

            var module = SelectedModule;
            if (module != null)
            {
                OnSelectedModuleChanged?.Invoke(module);
                NavSelectMod(module);
            }
        }

        /// <summary>
        /// Called when there's a click on the ModGrid header row.
        /// Handles sorting and the header right click context menu.
        /// </summary>
        private void ModGrid_HeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Left click -> sort by new column / change sorting direction.
            if (e.Button == MouseButtons.Left)
            {
                if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    AddSort(ModGrid.Columns[e.ColumnIndex]);
                }
                else
                {
                    SetSort(ModGrid.Columns[e.ColumnIndex]);
                }
                UpdateFilters();
            }
            // Right click -> Bring up context menu to change visibility of columns.
            else if (e.Button == MouseButtons.Right)
            {
                ShowHeaderContextMenu();
            }
        }

        private void ShowHeaderContextMenu(bool columns = true,
                                           bool tags    = true)
        {
            if (!columns && !tags)
            {
                // Don't show a blank menu
                return;
            }

            // Start from scratch: clear the entire item list, then add all options again
            ModListHeaderContextMenuStrip.Items.Clear();

            if (columns)
            {
                // Add columns
                ModListHeaderContextMenuStrip.Items.AddRange(
                    ModGrid.Columns.Cast<DataGridViewColumn>()
                    .Where(col => col.Name != "Installed" && col.Name != "UpdateCol" && col.Name != "ReplaceCol")
                    .Select(col => new ToolStripMenuItem()
                    {
                        Name    = col.Name,
                        Text    = col.HeaderText,
                        Checked = col.Visible,
                        Tag     = col
                    })
                    .ToArray()
                );
            }

            if (columns && tags)
            {
                // Separator
                ModListHeaderContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (tags)
            {
                // Add tags
                var registry = RegistryManager.Instance(currentInstance, repoData).registry;
                ModListHeaderContextMenuStrip.Items.AddRange(
                    registry.Tags.OrderBy(kvp => kvp.Key)
                    .Select(kvp => new ToolStripMenuItem()
                    {
                        Name    = kvp.Key,
                        Text    = kvp.Key,
                        Checked = !mainModList.ModuleTags.HiddenTags.Contains(kvp.Key),
                        Tag     = kvp.Value,
                    })
                    .ToArray()
                );
            }

            // Show the context menu on cursor position.
            ModListHeaderContextMenuStrip.Show(Cursor.Position);
        }

        /// <summary>
        /// Called if a ToolStripButton of the header context menu is pressed.
        /// </summary>
        private void ModListHeaderContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // ClickedItem is of type ToolStripItem, we need ToolStripButton.
            ToolStripMenuItem  clickedItem = e.ClickedItem    as ToolStripMenuItem;

            if (clickedItem?.Tag is DataGridViewColumn col)
            {
                col.Visible = !clickedItem.Checked;
                guiConfig.SetColumnVisibility(col.Name, !clickedItem.Checked);
                if (col.Index == 0)
                {
                    InstallAllCheckbox.Visible = col.Visible;
                }
            }
            else if (clickedItem?.Tag is ModuleTag tag)
            {
                if (!clickedItem.Checked)
                {
                    mainModList.ModuleTags.HiddenTags.Remove(tag.Name);
                }
                else
                {
                    mainModList.ModuleTags.HiddenTags.Add(tag.Name);
                }
                mainModList.ModuleTags.Save(ModuleTagList.DefaultPath);
                UpdateFilters();
                UpdateHiddenTagsAndLabels();
            }
        }

        /// <summary>
        /// Called on key down when the mod list is focused.
        /// Makes the Home/End keys go to the top/bottom of the list respectively.
        /// </summary>
        private void ModGrid_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Home:
                    // First row.
                    if (ModGrid.Rows.Count > 0) //Handles for empty filters
                    {
                        ModGrid.CurrentCell = ModGrid.Rows[0].Cells[SelectableColumnIndex()];
                    }

                    e.Handled = true;
                    break;

                case Keys.End:
                    // Last row.
                    if (ModGrid.Rows.Count > 0) //Handles for empty filters
                    {
                        ModGrid.CurrentCell = ModGrid.Rows[ModGrid.Rows.Count - 1].Cells[SelectableColumnIndex()];
                    }

                    e.Handled = true;
                    break;

                case Keys.Space:
                    // If they've focused one of the checkbox columns, don't intercept
                    if (ModGrid.CurrentCell != null && ModGrid.CurrentCell.ColumnIndex > 3)
                    {
                        DataGridViewRow row = ModGrid.CurrentRow;
                        // Toggle Update column if enabled, otherwise Install
                        for (int colIndex = 2; colIndex >= 0; --colIndex)
                        {
                            if (row?.Cells[colIndex] is DataGridViewCheckBoxCell)
                            {
                                // Need to change the state here, because the user hasn't clicked on a checkbox
                                row.Cells[colIndex].Value = !(bool)row.Cells[colIndex].Value;
                                ModGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                                e.Handled = true;
                                break;
                            }
                        }
                    }
                    break;

                case Keys.Apps:
                    ShowModContextMenu();
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Called on key press when the mod is focused. Scrolls to the first mod with name
        /// beginning with the key pressed. If more than one unique keys are pressed in under
        /// a second, it searches for the combination of the keys pressed. If the same key is
        /// being pressed repeatedly, it cycles through mods names beginning with that key.
        /// If space is pressed, the checkbox at the current row is toggled.
        /// </summary>
        private void ModGrid_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Don't search for spaces or newlines
            if (e.KeyChar == (char)Keys.Space || e.KeyChar == (char)Keys.Enter)
            {
                return;
            }

            var key = e.KeyChar.ToString();
            // Determine time passed since last key press.
            TimeSpan interval = DateTime.Now - lastSearchTime;
            if (interval.TotalSeconds < 1)
            {
                // Last keypress was < 1 sec ago, so combine the last and current keys.
                key = lastSearchKey + key;
            }

            // Remember the current time and key.
            lastSearchTime = DateTime.Now;
            lastSearchKey = key;

            if (key.Distinct().Count() == 1)
            {
                // Treat repeating and single keypresses the same.
                key = key.Substring(0, 1);
            }

            FocusMod(key, false);
            e.Handled = true;
        }

        /// <summary>
        /// I'm pretty sure this is what gets called when the user clicks on a ticky in the mod list.
        /// </summary>
        private void ModGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            ModGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void ModGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = ModGrid.Rows[e.RowIndex];
            if (!(row.Cells[0] is DataGridViewCheckBoxCell))
            {
                return;
            }

            // Need to change the state here, because the user hasn't clicked on a checkbox.
            row.Cells[0].Value = !(bool)row.Cells[0].Value;
            ModGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void ModGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = ModGrid?.Rows?[e.RowIndex];
                switch (row?.Cells[e.ColumnIndex])
                {
                    case DataGridViewLinkCell linkCell:
                        // Launch URLs if found in grid
                        string cmd = linkCell.Value.ToString();
                        if (!string.IsNullOrEmpty(cmd))
                        {
                            Utilities.ProcessStartURL(cmd);
                        }
                        break;

                    case DataGridViewCheckBoxCell checkCell:
                        // checked is a keyword in C#
                        var nowChecked = (bool)checkCell.Value;
                        if (row?.Tag is GUIMod gmod)
                        {
                            switch (ModGrid.Columns[e.ColumnIndex].Name)
                            {
                                case "Installed":
                                    gmod.SelectedMod = nowChecked ? gmod.SelectedMod
                                                                    ?? gmod.InstalledMod?.Module
                                                                    ?? gmod.LatestCompatibleMod
                                                                  : null;
                                    break;
                                case "UpdateCol":
                                    gmod.SelectedMod = nowChecked
                                        ? gmod.SelectedMod != null
                                          && (gmod.InstalledMod == null
                                              || gmod.InstalledMod.Module.version < gmod.SelectedMod.version)
                                            ? gmod.SelectedMod
                                            : gmod.LatestCompatibleMod
                                        : gmod.InstalledMod?.Module;

                                    if (nowChecked && gmod.SelectedMod == gmod.LatestCompatibleMod)
                                    {
                                        // Reinstall, force update without change
                                        UpdateChangeSetAndConflicts(currentInstance,
                                            RegistryManager.Instance(currentInstance, repoData).registry);
                                    }
                                    break;
                                case "AutoInstalled":
                                    gmod.SetAutoInstallChecked(row, AutoInstalled);
                                    OnRegistryChanged?.Invoke();
                                    break;
                                case "ReplaceCol":
                                    UpdateChangeSetAndConflicts(currentInstance,
                                        RegistryManager.Instance(currentInstance, repoData).registry);
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        private void guiModule_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is GUIMod gmod
                && mainModList.full_list_of_mod_rows.TryGetValue(gmod.Identifier,
                                                                 out DataGridViewRow row))
            {
                switch (e.PropertyName)
                {
                    case "SelectedMod":
                        Util.Invoke(this, () =>
                        {
                            if (row.Cells[Installed.Index] is DataGridViewCheckBoxCell instCell)
                            {
                                bool newVal = gmod.SelectedMod != null;
                                if ((bool)instCell.Value != newVal)
                                {
                                    instCell.Value = newVal;
                                }
                            }
                            if (row.Cells[UpdateCol.Index] is DataGridViewCheckBoxCell upgCell)
                            {
                                bool newVal = gmod.SelectedMod != null
                                              && (gmod.InstalledMod == null
                                                  || gmod.InstalledMod.Module.version < gmod.SelectedMod.version);
                                if ((bool)upgCell.Value != newVal)
                                {
                                    upgCell.Value = newVal;
                                }
                            }

                            if (Platform.IsWindows)
                            {
                                // This call is needed to force the UI to update on Windows,
                                // otherwise the checkboxes can look checked when unchecked or vice versa.
                                // Unfortunately, it crashes on Mono.
                                ModGrid.RefreshEdit();
                            }
                            // Update the changeset
                            UpdateChangeSetAndConflicts(currentInstance,
                                RegistryManager.Instance(currentInstance, repoData).registry);
                        });
                        break;
                }
            }
        }

        public void RemoveChangesetItem(ModChange change)
        {
            if (currentChangeSet != null
                && currentChangeSet.Contains(change)
                && change.IsRemovable
                && mainModList.full_list_of_mod_rows.TryGetValue(change.Mod.identifier,
                                                                 out DataGridViewRow row)
                && row.Tag is GUIMod guiMod)
            {
                if (change.IsAutoRemoval)
                {
                    guiMod.SetAutoInstallChecked(row, AutoInstalled, false);
                    OnRegistryChanged?.Invoke();
                }
                else if (change.IsUserRequested)
                {
                    guiMod.SelectedMod = guiMod.InstalledMod?.Module;
                    switch (change.ChangeType)
                    {
                        case GUIModChangeType.Replace:
                            if (row.Cells[ReplaceCol.Index] is DataGridViewCheckBoxCell checkCell)
                            {
                                checkCell.Value = false;
                            }
                            break;
                        case GUIModChangeType.Update:
                            if (row.Cells[UpdateCol.Index] is DataGridViewCheckBoxCell updateCell)
                            {
                                updateCell.Value = false;
                            }
                            break;
                    }
                }
                UpdateChangeSetAndConflicts(
                    currentInstance, RegistryManager.Instance(currentInstance, repoData).registry);
            }
        }

        private void ModGrid_GotFocus(object sender, EventArgs e)
        {
            Util.Invoke(this, () =>
            {
                // Give the selected row the standard highlight color
                ModGrid.RowsDefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                ModGrid.RowsDefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            });
        }

        private void ModGrid_LostFocus(object sender, EventArgs e)
        {
            Util.Invoke(this, () =>
            {
                // Gray out the selected row so you can tell the mod list is not focused
                ModGrid.RowsDefaultCellStyle.SelectionBackColor = SystemColors.Control;
                ModGrid.RowsDefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
            });
        }

        private void InstallAllCheckbox_CheckChanged(object sender, EventArgs e)
        {
            WithFrozenChangeset(() =>
            {
                if (InstallAllCheckbox.Checked)
                {
                    // Reset changeset
                    ClearChangeSet();
                }
                else
                {
                    // Uninstall all and cancel upgrades
                    foreach (var row in mainModList.full_list_of_mod_rows.Values)
                    {
                        if (row.Tag is GUIMod gmod)
                        {
                            gmod.SelectedMod = null;
                        }
                    }
                }
            });
        }

        public void ClearChangeSet()
        {
            WithFrozenChangeset(() =>
            {
                foreach (DataGridViewRow row in mainModList.full_list_of_mod_rows.Values)
                {
                    if (row.Tag is GUIMod gmod)
                    {
                        gmod.SelectedMod = gmod.InstalledMod?.Module;
                    }
                    if (row.Cells[ReplaceCol.Index] is DataGridViewCheckBoxCell checkCell)
                    {
                        checkCell.Value = false;
                    }
                    if (row.Cells[UpdateCol.Index] is DataGridViewCheckBoxCell updateCell)
                    {
                        updateCell.Value = false;
                    }
                }
                // Marking a mod as AutoInstalled can immediately queue it for removal if there is no dependent mod.
                // Reset the state of the AutoInstalled checkbox for these by deducing it from the changeset.
                foreach (DataGridViewRow row in mainModList.full_list_of_mod_rows.Values)
                {
                    GUIMod mod = row.Tag as GUIMod;
                    if (mod.InstalledMod != null
                        && ChangeSet.Contains(new ModChange(mod.InstalledMod?.Module,
                                                            GUIModChangeType.Remove,
                                                            new SelectionReason.NoLongerUsed())))
                    {
                        mod.SetAutoInstallChecked(row, AutoInstalled, false);
                    }
                }
            });
        }

        private void WithFrozenChangeset(Action action)
        {
            if (freezeChangeSet)
            {
                // Already frozen by some outer block, let it handle the cleanup
                action?.Invoke();
            }
            else
            {
                freezeChangeSet = true;
                try
                {
                    action?.Invoke();
                }
                finally
                {
                    // Don't let anything ever prevent us from unfreezing the changeset
                    freezeChangeSet = false;
                    ModGrid.Refresh();
                    UpdateChangeSetAndConflicts(currentInstance,
                                                RegistryManager.Instance(currentInstance, repoData).registry);
                }
            }
        }

        /// <summary>
        /// Find a column of the grid that can contain the CurrentCell.
        /// Can't be hidden or an exception is thrown.
        /// Shouldn't be a checkbox because we don't want the space bar to toggle.
        /// </summary>
        /// <returns>
        /// Index of the column to use.
        /// </returns>
        private int SelectableColumnIndex()
        {
            // First try the currently active cell's column
            return ModGrid.CurrentCell?.ColumnIndex
                // If there's no currently active cell, use the first visible non-checkbox column
                ?? ModGrid.Columns.Cast<DataGridViewColumn>()
                    .FirstOrDefault(c => c is DataGridViewTextBoxColumn && c.Visible)?.Index
                // Otherwise use the Installed checkbox column since it can't be hidden
                ?? Installed.Index;
        }

        public void FocusMod(string key, bool exactMatch, bool showAsFirst = false)
        {
            DataGridViewRow current_row = ModGrid.CurrentRow;
            int currentIndex = current_row?.Index ?? 0;
            DataGridViewRow first_match = null;

            var does_name_begin_with_key = new Func<DataGridViewRow, bool>(row =>
            {
                GUIMod mod = row.Tag as GUIMod;
                bool row_match;
                if (exactMatch)
                {
                    row_match = mod.Name == key || mod.Identifier == key;
                }
                else
                {
                    row_match = mod.Name.StartsWith(key, StringComparison.OrdinalIgnoreCase) ||
                                mod.Abbrevation.StartsWith(key, StringComparison.OrdinalIgnoreCase) ||
                                mod.Identifier.StartsWith(key, StringComparison.OrdinalIgnoreCase);
                }

                if (row_match && first_match == null)
                {
                    // Remember the first match to allow cycling back to it if necessary.
                    first_match = row;
                }

                if (key.Length == 1 && row_match && row.Index <= currentIndex)
                {
                    // Keep going forward if it's a single key match and not ahead of the current row.
                    return false;
                }

                return row_match;
            });

            ModGrid.ClearSelection();
            DataGridViewRow match = ModGrid.Rows.Cast<DataGridViewRow>()
                .Where(row => row.Visible)
                .FirstOrDefault(does_name_begin_with_key);
            if (match == null && first_match != null)
            {
                // If there were no matches after the first match, cycle over to the beginning.
                match = first_match;
            }

            if (match != null)
            {
                match.Selected = true;

                ModGrid.CurrentCell = match.Cells[SelectableColumnIndex()];
                if (showAsFirst)
                {
                    ModGrid.FirstDisplayedScrollingRowIndex = match.Index;
                }
            }
            else
            {
                RaiseMessage?.Invoke(Properties.Resources.MainNotFound);
            }
        }

        private void ModGrid_MouseDown(object sender, MouseEventArgs e)
        {
            var rowIndex = ModGrid.HitTest(e.X, e.Y).RowIndex;

            // Ignore header column to prevent errors
            if (rowIndex != -1 && e.Button == MouseButtons.Right)
            {
                // Detect the clicked cell and select the row
                ModGrid.ClearSelection();
                ModGrid.Rows[rowIndex].Selected = true;

                // Show the context menu
                ShowModContextMenu();
            }
        }

        private bool ShowModContextMenu()
        {
            var guiMod = SelectedModule;
            if (guiMod != null)
            {
                ModListContextMenuStrip.Show(Cursor.Position);
                var isDownloadable = !guiMod.ToModule()?.IsMetapackage ?? false;
                // Set the menu options
                downloadContentsToolStripMenuItem.Enabled = isDownloadable && !guiMod.IsCached;
                purgeContentsToolStripMenuItem.Enabled    = isDownloadable && guiMod.IsCached;
                reinstallToolStripMenuItem.Enabled = guiMod.IsInstalled && !guiMod.IsAutodetected;
                return true;
            }
            return false;
        }

        private void ModGrid_Resize(object sender, EventArgs e)
        {
            InstallAllCheckbox.Top = ModGrid.Top - InstallAllCheckbox.Height;
        }

        private void reinstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var module = SelectedModule?.ToModule();
            if (module != null)
            {
                IRegistryQuerier registry = RegistryManager.Instance(currentInstance, repoData).registry;
                StartChangeSet?.Invoke(new List<ModChange>()
                {
                    // "Upgrade" to latest metadata for same module version
                    // (avoids removing and re-installing dependencies)
                    new ModUpgrade(module, GUIModChangeType.Update,
                                   registry.GetModuleByVersion(module.identifier,
                                                               module.version)
                                           ?? module,
                                   true)
                }, null);
            }
        }

        [ForbidGUICalls]
        public Dictionary<string, GUIMod> AllGUIMods()
            => mainModList.Modules.ToDictionary(guiMod => guiMod.Identifier,
                                                guiMod => guiMod);

        private void purgeContentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Purge other versions as well since the user is likely to want that
            // and has no other way to achieve it
            var selected = SelectedModule;
            if (selected != null)
            {
                IRegistryQuerier registry = RegistryManager.Instance(currentInstance, repoData).registry;
                var allAvail = registry.AvailableByIdentifier(selected.Identifier);
                foreach (CkanModule mod in allAvail)
                {
                    manager.Cache.Purge(mod);
                }
            }
        }

        private void downloadContentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main.Instance.StartDownload(SelectedModule);
        }

        private void EditModSearches_ApplySearches(List<ModSearch> searches)
        {
            mainModList.SetSearches(searches);

            // If these columns aren't hidden by the user, show them if the search includes installed modules
            setInstalledColumnsVisible(mainModList.HasAnyInstalled
                                       && !SearchesExcludeInstalled(searches)
                                       && mainModList.HasVisibleInstalled());
        }

        private void EditModSearches_SurrenderFocus()
        {
            Util.Invoke(this, () => ModGrid.Focus());
        }

        [ForbidGUICalls]
        public void UpdateFilters()
        {
            Util.Invoke(this, _UpdateFilters);
        }

        private void _UpdateFilters()
        {
            if (ModGrid == null || mainModList?.full_list_of_mod_rows == null)
            {
                return;
            }

            // Each time a row in DataGridViewRow is changed, DataGridViewRow updates the view. Which is slow.
            // To make the filtering process faster, Copy the list of rows. Filter out the hidden and replace the
            // rows in DataGridView.

            var rows = new DataGridViewRow[mainModList.full_list_of_mod_rows.Count];
            mainModList.full_list_of_mod_rows.Values.CopyTo(rows, 0);
            // Try to remember the current scroll position and selected mod
            var scroll_col = Math.Max(0, ModGrid.FirstDisplayedScrollingColumnIndex);
            GUIMod selected_mod = null;
            if (ModGrid.CurrentRow != null)
            {
                selected_mod = (GUIMod)ModGrid.CurrentRow.Tag;
            }

            var registry = RegistryManager.Instance(currentInstance, repoData).registry;
            ModGrid.Rows.Clear();
            var instName = currentInstance.Name;
            var instGame = currentInstance.game;
            rows.AsParallel().ForAll(row =>
                row.Visible = mainModList.IsVisible((GUIMod)row.Tag,
                                                    instName, instGame, registry));
            ApplyHeaderGlyphs();
            ModGrid.Rows.AddRange(Sort(rows.Where(row => row.Visible)).ToArray());

            // Find and select the previously selected row
            if (selected_mod != null)
            {
                var selected_row = ModGrid.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(row => selected_mod.Identifier.Equals(((GUIMod)row.Tag).Identifier));
                if (selected_row != null)
                {
                    ModGrid.CurrentCell = selected_row.Cells[scroll_col];
                }
            }
        }

        [ForbidGUICalls]
        public void Update(object sender, DoWorkEventArgs e)
        {
            e.Result = _UpdateModsList(e.Argument as Dictionary<string, bool>);
        }

        [ForbidGUICalls]
        private bool _UpdateModsList(Dictionary<string, bool> old_modules = null)
        {
            log.Info("Updating the mod list");

            var regMgr = RegistryManager.Instance(currentInstance, repoData);
            IRegistryQuerier registry = regMgr.registry;

            repoData.Prepopulate(
                registry.Repositories.Values.ToList(),
                new Progress<int>(p => user.RaiseProgress(
                    Properties.Resources.LoadingCachedRepoData, p)));

            if (!regMgr.registry.HasAnyAvailable())
            {
                // Abort the refresh so we can update the repo data
                return false;
            }

            RaiseMessage?.Invoke(Properties.Resources.MainRepoScanning);
            regMgr.ScanUnmanagedFiles();

            RaiseMessage?.Invoke(Properties.Resources.MainModListLoadingInstalled);

            var guiMods = mainModList.GetGUIMods(registry, repoData, currentInstance, guiConfig)
                                     .ToHashSet();

            foreach (var gmod in mainModList.full_list_of_mod_rows
                                            ?.Values
                                             .Select(row => row.Tag)
                                             .OfType<GUIMod>()
                                            ?? Enumerable.Empty<GUIMod>())
            {
                gmod.PropertyChanged -= guiModule_PropertyChanged;
            }
            foreach (var gmod in guiMods)
            {
                gmod.PropertyChanged += guiModule_PropertyChanged;
            }

            RaiseMessage?.Invoke(Properties.Resources.MainModListPreservingNew);
            var toNotify = new HashSet<GUIMod>();
            if (old_modules != null)
            {
                foreach (GUIMod gm in guiMods)
                {
                    if (old_modules.TryGetValue(gm.Identifier, out bool oldIncompat))
                    {
                        // Found it; check if newly compatible
                        if (!gm.IsIncompatible && oldIncompat)
                        {
                            gm.IsNew = true;
                            toNotify.Add(gm);
                        }
                    }
                    else
                    {
                        // Newly indexed, show regardless of compatibility
                        gm.IsNew = true;
                    }
                }
            }
            else
            {
                // Copy the new mod flag from the old list.
                var oldNewMods = mainModList.Modules.Where(m => m.IsNew)
                                                    .ToHashSet();
                foreach (var guiMod in guiMods.Intersect(oldNewMods))
                {
                    guiMod.IsNew = true;
                }
            }
            LabelsAfterUpdate?.Invoke(toNotify);

            RaiseMessage?.Invoke(Properties.Resources.MainModListPopulatingList);
            // Update our mod listing
            mainModList.ConstructModList(guiMods, currentInstance.Name, currentInstance.game, ChangeSet);

            UpdateChangeSetAndConflicts(currentInstance, registry);

            RaiseMessage?.Invoke(Properties.Resources.MainModListUpdatingFilters);

            var has_unheld_updates = mainModList.Modules.Any(mod => mod.HasUpdate && !Main.Instance.LabelsHeld(mod.Identifier));
            Util.Invoke(menuStrip2, () =>
            {
                FilterCompatibleButton.Text = string.Format(Properties.Resources.MainModListCompatible,
                    mainModList.CountModsByFilter(GUIModFilter.Compatible));
                FilterInstalledButton.Text = string.Format(Properties.Resources.MainModListInstalled,
                    mainModList.CountModsByFilter(GUIModFilter.Installed));
                FilterInstalledUpdateButton.Text = string.Format(Properties.Resources.MainModListUpgradeable,
                    mainModList.CountModsByFilter(GUIModFilter.InstalledUpdateAvailable));
                FilterReplaceableButton.Text = string.Format(Properties.Resources.MainModListReplaceable,
                    mainModList.CountModsByFilter(GUIModFilter.Replaceable));
                FilterCachedButton.Text = string.Format(Properties.Resources.MainModListCached,
                    mainModList.CountModsByFilter(GUIModFilter.Cached));
                FilterUncachedButton.Text = string.Format(Properties.Resources.MainModListUncached,
                    mainModList.CountModsByFilter(GUIModFilter.Uncached));
                FilterNewButton.Text = string.Format(Properties.Resources.MainModListNewlyCompatible,
                    mainModList.CountModsByFilter(GUIModFilter.NewInRepository));
                FilterNotInstalledButton.Text = string.Format(Properties.Resources.MainModListNotInstalled,
                    mainModList.CountModsByFilter(GUIModFilter.NotInstalled));
                FilterIncompatibleButton.Text = string.Format(Properties.Resources.MainModListIncompatible,
                    mainModList.CountModsByFilter(GUIModFilter.Incompatible));
                FilterAllButton.Text = string.Format(Properties.Resources.MainModListAll,
                    mainModList.CountModsByFilter(GUIModFilter.All));

                UpdateAllToolButton.Enabled = has_unheld_updates;
            });

            UpdateFilters();

            // Hide update and replacement columns if not needed.
            // Write it to the configuration, else they are hidden again after a filter change.
            // After the update / replacement, they are hidden again.
            Util.Invoke(ModGrid, () =>
            {
                UpdateCol.Visible  = has_unheld_updates;
                ReplaceCol.Visible = mainModList.Modules.Any(mod => mod.IsInstalled && mod.HasReplacement);
            });

            UpdateHiddenTagsAndLabels();

            var timeSinceUpdate = guiConfig.RefreshOnStartup ? TimeSpan.Zero
                                                             : repoData.LastUpdate(registry.Repositories.Values);
            Util.Invoke(this, () =>
            {
                if (timeSinceUpdate < RepositoryDataManager.TimeTillStale)
                {
                    RefreshToolButton.Image = EmbeddedImages.refresh;
                    RefreshToolButton.ToolTipText = new SingleAssemblyComponentResourceManager(typeof(ManageMods))
                                                    .GetString($"{RefreshToolButton.Name}.ToolTipText");
                }
                else if (timeSinceUpdate < RepositoryDataManager.TimeTillVeryStale)
                {
                    RefreshToolButton.Image = EmbeddedImages.refreshStale;
                    RefreshToolButton.ToolTipText = string.Format(Properties.Resources.ManageModsRefreshStaleToolTip,
                                                                  Math.Round(timeSinceUpdate.TotalDays));
                }
                else
                {
                    RefreshToolButton.Image = EmbeddedImages.refreshVeryStale;
                    RefreshToolButton.ToolTipText = string.Format(Properties.Resources.ManageModsRefreshVeryStaleToolTip,
                                                                  Math.Round(timeSinceUpdate.TotalDays));
                }
            });

            ClearStatusBar?.Invoke();
            Util.Invoke(this, () => ModGrid.Focus());
            return true;
        }

        private void ModGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            ModGrid_CellContentClick(sender, null);
        }

        private void SetSort(DataGridViewColumn col)
        {
            if (SortColumns.Count == 1 && SortColumns[0] == col.Name)
            {
                descending[0] = !descending[0];
            }
            else
            {
                SortColumns.Clear();
                descending.Clear();
                AddSort(col);
            }
        }

        private void AddSort(DataGridViewColumn col, bool atStart = false)
        {
            if (SortColumns.Count > 0 && SortColumns[SortColumns.Count - 1] == col.Name)
            {
                descending[descending.Count - 1] = !descending[descending.Count - 1];
            }
            else
            {
                int middlePosition = SortColumns.IndexOf(col.Name);
                if (middlePosition > -1)
                {
                    SortColumns.RemoveAt(middlePosition);
                    descending.RemoveAt(middlePosition);
                }
                if (atStart)
                {
                    SortColumns.Insert(0, col.Name);
                    descending.Insert(0, false);
                }
                else
                {
                    SortColumns.Add(col.Name);
                    descending.Add(false);
                }
            }
        }

        private IEnumerable<DataGridViewRow> Sort(IEnumerable<DataGridViewRow> rows)
        {
            var sorted = rows.ToList();
            sorted.Sort(CompareRows);
            return sorted;
        }

        private void ApplyHeaderGlyphs()
        {
            foreach (DataGridViewColumn col in ModGrid.Columns)
            {
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            }
            for (int i = 0; i < SortColumns.Count; ++i)
            {
                if (!ModGrid.Columns.Contains(SortColumns[i]))
                {
                    // Shouldn't be possible, but better safe than sorry.
                    continue;
                }
                ModGrid.Columns[SortColumns[i]].HeaderCell.SortGlyphDirection = descending[i]
                    ? SortOrder.Descending : SortOrder.Ascending;
            }
        }

        private int CompareRows(DataGridViewRow a, DataGridViewRow b)
        {
            for (int i = 0; i < SortColumns.Count; ++i)
            {
                var val = CompareColumn(a, b, ModGrid.Columns[SortColumns[i]]);
                if (val != 0)
                {
                    return descending[i] ? -val : val;
                }
            }
            return CompareColumn(a, b, ModName);
        }

        /// <summary>
        /// Compare two rows based on one of their columns
        /// </summary>
        /// <param name="a">First row</param>
        /// <param name="b">Second row</param>
        /// <param name="col">The column to compare</param>
        /// <returns>
        /// -1 if a&lt;b, 1 if a&gt;b, 0 if a==b
        /// </returns>
        private int CompareColumn(DataGridViewRow a, DataGridViewRow b, DataGridViewColumn col)
        {
            GUIMod gmodA = a.Tag as GUIMod;
            GUIMod gmodB = b.Tag as GUIMod;
            CkanModule modA = gmodA.ToModule();
            CkanModule modB = gmodB.ToModule();
            var cellA = a.Cells[col.Index];
            var cellB = b.Cells[col.Index];
            if (col is DataGridViewCheckBoxColumn)
            {
                // Checked < non-"-" text < unchecked < "-" text
                if (cellA is DataGridViewCheckBoxCell checkboxA)
                {
                    return cellB is DataGridViewCheckBoxCell checkboxB
                            ? -((bool)checkboxA.Value).CompareTo((bool)checkboxB.Value)
                        : (bool)checkboxA.Value || ((string)cellB.Value == "-") ? -1
                        : 1;
                }
                else
                {
                    return cellB is DataGridViewCheckBoxCell ? -CompareColumn(b, a, col)
                        : (string)cellA.Value == (string)cellB.Value ? 0
                        : (string)cellA.Value == "-" ? 1
                        : (string)cellB.Value == "-" ? -1
                        : ((string)cellA.Value).CompareTo((string)cellB.Value);
                }
            }
            else
            {
                switch (col.Name)
                {
                    case "ModName":           return gmodA.Name.CompareTo(gmodB.Name);
                    case "GameCompatibility": return GameCompatComparison(a, b);
                    case "InstallDate":       return CompareToNullable(gmodA.InstallDate,
                                                                       gmodB.InstallDate);
                    case "ReleaseDate":       return CompareToNullable(modA.release_date,
                                                                       modB.release_date);
                    case "DownloadSize":      return modA.download_size.CompareTo(modB.download_size);
                    case "InstallSize":       return modA.install_size.CompareTo(modB.install_size);
                    case "DownloadCount":     return CompareToNullable(gmodA.DownloadCount,
                                                                       gmodB.DownloadCount);
                    default:
                        var valA = (cellA.Value as string) ?? "";
                        var valB = (cellB.Value as string) ?? "";
                        return valA.CompareTo(valB);
                }
            }
        }

        private static int CompareToNullable<T>(T? a, T? b) where T : struct, IComparable
            => a.HasValue ? b.HasValue ? a.Value.CompareTo(b.Value)
                                       : 1
                          : b.HasValue ? -1
                                       : 0;

        /// <summary>
        /// Compare two rows' GameVersions as max versions.
        /// GameVersion.CompareTo sorts IsAny to the beginning instead
        /// of the end, and we can't change that without breaking many things.
        /// Similarly, 1.8 should sort after 1.8.0.
        /// </summary>
        /// <param name="a">First row to compare</param>
        /// <param name="b">Second row to compare</param>
        /// <returns>
        /// Positive to sort as a lessthan b, negative to sort as b lessthan a
        /// </returns>
        private int GameCompatComparison(DataGridViewRow a, DataGridViewRow b)
        {
            GameVersion verA = ((GUIMod)a.Tag)?.GameCompatibilityVersion;
            GameVersion verB = ((GUIMod)b.Tag)?.GameCompatibilityVersion;
            if (verA == null)
            {
                return verB == null ? 0 : -1;
            }
            else if (verB == null)
            {
                return 1;
            }
            var majorCompare = VersionPieceCompare(verA.IsMajorDefined, verA.Major, verB.IsMajorDefined, verB.Major);
            if (majorCompare != 0)
            {
                return majorCompare;
            }
            else
            {
                var minorCompare = VersionPieceCompare(verA.IsMinorDefined, verA.Minor, verB.IsMinorDefined, verB.Minor);
                if (minorCompare != 0)
                {
                    return minorCompare;
                }
                else
                {
                    var patchCompare = VersionPieceCompare(verA.IsPatchDefined, verA.Patch, verB.IsPatchDefined, verB.Patch);
                    return patchCompare != 0
                        ? patchCompare
                        : VersionPieceCompare(verA.IsBuildDefined, verA.Build, verB.IsBuildDefined, verB.Build);
                }
            }
        }

        /// <summary>
        /// Compare pieces of two versions, each of which may be undefined,
        /// sorting undefined toward the end.
        /// </summary>
        /// <param name="definedA">true if the first version piece is defined, false if undefined</param>
        /// <param name="valA">Value of the first version piece</param>
        /// <param name="definedB">true if the second version piece is defined, false if undefined</param>
        /// <param name="valB">Value of the second version piece</param>
        /// <returns>
        /// Positive to sort a lessthan b, negative to sort b lessthan a
        /// </returns>
        private int VersionPieceCompare(bool definedA, int valA, bool definedB, int valB)
            => definedA
                ? (definedB ? valA.CompareTo(valB) : -1)
                : (definedB ? 1                    :  0);

        public void ResetFilterAndSelectModOnList(CkanModule module)
        {
            EditModSearches.Clear();
            FocusMod(module.identifier, true);
        }

        public GUIMod SelectedModule =>
            ModGrid.SelectedRows.Count == 0
                ? null
                : ModGrid.SelectedRows[0]?.Tag as GUIMod;

        public void CloseSearch(Point screenCoords)
        {
            EditModSearches.CloseSearch(screenCoords);
        }

        public void ParentMoved()
        {
            EditModSearches.ParentMoved();
        }

        #region Hidden tags and labels links

        [ForbidGUICalls]
        private void UpdateHiddenTagsAndLabels()
        {
            var registry = RegistryManager.Instance(currentInstance, repoData).registry;
            var tags = mainModList.ModuleTags.HiddenTags
                                             .Intersect(registry.Tags.Keys)
                                             .OrderByDescending(tagName => tagName)
                                             .Select(tagName => registry.Tags[tagName])
                                             .ToList();
            var labels = mainModList.ModuleLabels.LabelsFor(currentInstance.Name)
                                                 .Where(l => l.Hide && l.ModuleCount(currentInstance.game) > 0)
                                                 .ToList();
            hiddenTagsLabelsLinkList.UpdateTagsAndLabels(tags, labels);
            Util.Invoke(hiddenTagsLabelsLinkList, () =>
            {
                hiddenTagsLabelsLinkList.Visible = tags.Count > 0 || labels.Count > 0;
                if (tags.Count > 0 || labels.Count > 0)
                {
                    hiddenTagsLabelsLinkList.Controls.Add(new Label()
                    {
                        Text = tags.Count   == 0 ? Properties.Resources.ManageModsHiddenLabels
                             : labels.Count == 0 ? Properties.Resources.ManageModsHiddenTags
                             :                     Properties.Resources.ManageModsHiddenLabelsAndTags,
                        AutoSize = true,
                        Padding  = new Padding(0),
                        Margin   = new Padding(0, 2, 0, 2),
                    });
                }
            });
        }

        private void hiddenTagsLabelsLinkList_TagClicked(ModuleTag tag, bool merge)
        {
            ShowHeaderContextMenu(columns: false);
        }

        private void hiddenTagsLabelsLinkList_LabelClicked(ModuleLabel label, bool merge)
        {
            Filter(ModList.FilterToSavedSearch(GUIModFilter.CustomLabel, null, label),
                   merge);
        }

        #endregion

        #region Navigation History

        private void NavInit()
        {
            navHistory.OnHistoryChange += NavOnHistoryChange;
            navHistory.IsReadOnly = false;
            var currentMod = SelectedModule;
            if (currentMod != null)
            {
                navHistory.AddToHistory(currentMod);
            }
        }

        private void NavUpdateUI()
        {
            NavBackwardToolButton.Enabled = navHistory.CanNavigateBackward;
            NavForwardToolButton.Enabled = navHistory.CanNavigateForward;
        }

        private void NavSelectMod(GUIMod module)
        {
            navHistory.AddToHistory(module);
        }

        public void NavGoBackward()
        {
            if (navHistory.CanNavigateBackward)
            {
                NavGoToMod(navHistory.NavigateBackward());
            }
        }

        public void NavGoForward()
        {
            if (navHistory.CanNavigateForward)
            {
                NavGoToMod(navHistory.NavigateForward());
            }
        }

        private void NavGoToMod(GUIMod module)
        {
            // Focussing on a mod also causes navigation, but we don't want
            // this to affect the history. so we switch to read-only mode.
            navHistory.IsReadOnly = true;
            FocusMod(module.Name, true);
            navHistory.IsReadOnly = false;
        }

        private void NavOnHistoryChange()
        {
            NavUpdateUI();
        }

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.F:
                    ActiveControl = EditModSearches;
                    return true;

                case Keys.Control | Keys.Shift | Keys.F:
                    EditModSearches.ExpandCollapse();
                    ActiveControl = EditModSearches;
                    return true;

                case Keys.Control | Keys.S:
                    if (ChangeSet != null && ChangeSet.Any())
                    {
                        ApplyToolButton_Click(null, null);
                    }

                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public bool AllowClose()
        {
            if (Conflicts != null && Conflicts.Any())
            {
                // Ask if they want to resolve conflicts
                string confDescrip = Conflicts
                    .Select(kvp => kvp.Value)
                    .Aggregate((a, b) => $"{a}, {b}");
                if (!Main.Instance.YesNoDialog(string.Format(Properties.Resources.MainQuitWithConflicts, confDescrip),
                    Properties.Resources.MainQuit,
                    Properties.Resources.MainGoBack))
                {
                    return false;
                }
            }
            else if (ChangeSet?.Any() ?? false)
            {
                // Ask if they want to discard the change set
                string changeDescrip = ChangeSet
                    .GroupBy(ch => ch.ChangeType, ch => ch.Mod.name)
                    .Select(grp => $"{grp.Key}: "
                        + grp.Aggregate((a, b) => $"{a}, {b}"))
                    .Aggregate((a, b) => $"{a}\r\n{b}");
                if (!Main.Instance.YesNoDialog(string.Format(Properties.Resources.MainQuitWithUnappliedChanges, changeDescrip),
                    Properties.Resources.MainQuit,
                    Properties.Resources.MainGoBack))
                {
                    return false;
                }
            }
            return true;
        }

        public void InstanceUpdated()
        {
            Conflicts = null;
            ChangeSet = null;
            ModGrid.CurrentCell = null;
        }

        public HashSet<ModChange> ComputeUserChangeSet()
            => mainModList.ComputeUserChangeSet(
                  RegistryManager.Instance(currentInstance, repoData).registry,
                  currentInstance.VersionCriteria(),
                  currentInstance,
                  UpdateCol, ReplaceCol);

        [ForbidGUICalls]
        public void UpdateChangeSetAndConflicts(GameInstance inst, IRegistryQuerier registry)
        {
            if (freezeChangeSet)
            {
                log.Debug("Skipping refresh because changeset is frozen");
                return;
            }

            List<ModChange> full_change_set = null;
            Dictionary<GUIMod, string> new_conflicts = null;

            var gameVersion = inst.VersionCriteria();
            var user_change_set = mainModList.ComputeUserChangeSet(registry, gameVersion, inst, UpdateCol, ReplaceCol);
            try
            {
                // Set the target versions of upgrading mods based on what's actually allowed
                foreach (var ch in user_change_set.OfType<ModUpgrade>())
                {
                    if (mainModList.full_list_of_mod_rows[ch.Mod.identifier].Tag is GUIMod gmod)
                    {
                       // This setter calls UpdateChangeSetAndConflicts, so there's a risk of
                       // an infinite loop here. Tread lightly!
                       gmod.SelectedMod = ch.targetMod;
                    }
                }
                var tuple = mainModList.ComputeFullChangeSetFromUserChangeSet(registry, user_change_set, gameVersion);
                full_change_set = tuple.Item1.ToList();
                new_conflicts = tuple.Item2.ToDictionary(
                    item => new GUIMod(item.Key, repoData, registry, gameVersion, null,
                                       guiConfig.HideEpochs, guiConfig.HideV),
                    item => item.Value);
                if (new_conflicts.Count > 0)
                {
                    SetStatusBar?.Invoke(string.Join("; ", tuple.Item3));
                }
                else
                {
                    // Clear the conflict area if no conflicts
                    ClearStatusBar?.Invoke();
                }
            }
            catch (DependencyNotSatisfiedKraken k)
            {
                RaiseError?.Invoke(string.Format(Properties.Resources.MainDepNotSatisfied,
                                                 k.parent, k.module));
                // Uncheck the box
                if (mainModList.full_list_of_mod_rows[k.parent.identifier].Tag is GUIMod gmod)
                {
                    gmod.SelectedMod = null;
                }
            }

            Conflicts = new_conflicts;
            ChangeSet = full_change_set;
        }

    }
}
