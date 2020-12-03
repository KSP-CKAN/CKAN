using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CKAN.Versioning;

namespace CKAN
{
    public sealed class GUIMod : INotifyPropertyChanged
    {
        private CkanModule      Mod                 { get; set; }
        private CkanModule      LatestCompatibleMod { get; set; }
        private InstalledModule InstalledMod        { get; set; }

        /// <summary>
        /// The module of the checkbox that is checked in the MainAllModVersions list if any,
        /// null otherwise.
        /// Used for generating this mod's part of the change set.
        /// </summary>
        public  CkanModule      SelectedMod
        {
            get { return selectedMod; }
            set
            {
                if (!(selectedMod?.Equals(value) ?? value?.Equals(selectedMod) ?? true))
                {
                    selectedMod = value;

                    if (IsInstalled && HasUpdate)
                    {
                        var isLatest = (LatestCompatibleMod?.Equals(selectedMod) ?? false);
                        if (IsUpgradeChecked ^ isLatest)
                        {
                            // Try upgrading if they pick the latest
                            Main.Instance.ManageMods.MarkModForUpdate(Identifier, isLatest);
                        }

                    }
                    Main.Instance.ManageMods.MarkModForInstall(Identifier, selectedMod == null);

                    // C# 7.0: Executes the task and discards it
                    _ = Main.Instance.ManageMods.UpdateChangeSetAndConflicts(
                        Main.Instance.Manager.CurrentInstance,
                        RegistryManager.Instance(Main.Instance.Manager.CurrentInstance).registry
                    );

                    OnPropertyChanged();
                }
            }
        }
        private CkanModule      selectedMod = null;

        /// <summary>
        /// Notify listeners when certain properties change.
        /// Currently used to tell MainAllModVersions to update its checkboxes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string Name { get; private set; }
        public bool IsInstalled { get; private set; }
        public bool IsAutoInstalled { get; private set; }
        public bool HasUpdate { get; private set; }
        public bool HasReplacement { get; private set; }
        public bool IsIncompatible { get; private set; }
        public bool IsAutodetected { get; private set; }
        public string Authors { get; private set; }
        public string InstalledVersion { get; private set; }
        public DateTime? InstallDate { get; private set; }
        public string LatestVersion { get; private set; }
        public string DownloadSize { get; private set; }
        public int? DownloadCount { get; private set; }
        public bool IsCached { get; private set; }

        // These indicate the maximum game version that the maximum available
        // version of this mod can handle. The "Long" version also indicates
        // to the user if a mod upgrade would be required. (#1270)
        public GameVersion GameCompatibilityVersion { get; private set; }
        public string GameCompatibility { get; private set; }
        public string GameCompatibilityLong { get; private set; }

        public string Abstract { get; private set; }
        public string Description { get; private set; }
        public string Identifier { get; private set; }
        public bool IsInstallChecked { get; set; }
        public bool IsUpgradeChecked { get; private set; }
        public bool IsReplaceChecked { get; private set; }
        public bool IsNew { get; set; }
        public bool IsCKAN { get; private set; }
        public string Abbrevation { get; private set; }

        public string SearchableName { get; private set; }
        public string SearchableIdentifier { get; private set; }
        public string SearchableAbstract { get; private set; }
        public string SearchableDescription { get; private set; }
        public List<string> SearchableAuthors { get; private set; }

        /// <summary>
        /// Return whether this mod is installable.
        /// Used for determining whether to show a checkbox in the leftmost column.
        /// </summary>
        /// <returns>
        /// False if the mod is auto detected,
        /// otherwise true if it's compatible or already installed,
        /// otherwise false.
        /// </returns>
        public bool IsInstallable()
        {
            // Compatible mods are installable, but so are mods that are already installed
            return !IsIncompatible || IsInstalled;
        }

        public string Version
        {
            get { return IsInstalled ? InstalledVersion : LatestVersion; }
        }

        /// <summary>
        /// Initialize a GUIMod based on an InstalledModule
        /// </summary>
        /// <param name="instMod">The installed module to represent</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="current_game_version">Current game version</param>
        /// <param name="incompatible">If true, mark this module as incompatible</param>
        public GUIMod(InstalledModule instMod, IRegistryQuerier registry, GameVersionCriteria current_game_version, bool? incompatible = null)
            : this(instMod.Module, registry, current_game_version, incompatible)
        {
            IsInstalled      = true;
            IsInstallChecked = true;
            InstalledMod     = instMod;
            selectedMod      = instMod.Module;
            IsAutoInstalled  = instMod.AutoInstalled;
            InstallDate      = instMod.InstallTime;
            InstalledVersion = instMod.Module.version.ToString();
            if (LatestVersion == null || LatestVersion.Equals("-"))
            {
                LatestVersion = InstalledVersion;
            }
        }

        /// <summary>
        /// Initialize a GUIMod based on a CkanModule
        /// </summary>
        /// <param name="mod">The module to represent</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="current_game_version">Current game version</param>
        /// <param name="incompatible">If true, mark this module as incompatible</param>
        public GUIMod(CkanModule mod, IRegistryQuerier registry, GameVersionCriteria current_game_version, bool? incompatible = null)
            : this(mod.identifier, registry, current_game_version, incompatible)
        {
            Mod           = mod;
            IsCKAN        = mod is CkanModule;

            Name          = mod.name.Trim();
            Abstract      = mod.@abstract.Trim();
            Description   = mod.description?.Trim() ?? string.Empty;
            Abbrevation   = new string(Name.Split(' ').Where(s => s.Length > 0).Select(s => s[0]).ToArray());
            Authors       = mod.author == null ? Properties.Resources.GUIModNSlashA : String.Join(",", mod.author);

            HasUpdate      = registry.HasUpdate(mod.identifier, current_game_version);
            HasReplacement = registry.GetReplacement(mod, current_game_version) != null;
            DownloadSize   = mod.download_size == 0 ? Properties.Resources.GUIModNSlashA : CkanModule.FmtSize(mod.download_size);

            // Get the Searchables.
            SearchableName        = mod.SearchableName;
            SearchableAbstract    = mod.SearchableAbstract;
            SearchableDescription = mod.SearchableDescription;
            SearchableAuthors     = mod.SearchableAuthors;

            // If not set in GUIMod(identifier, ...) (because the mod is not known to the registry),
            // set based on the the data we have from the CkanModule.
            if (GameCompatibilityVersion == null)
            {
                GameCompatibilityVersion = mod.LatestCompatibleKSP();
                GameCompatibility = GameCompatibilityVersion?.ToYalovString() ?? Properties.Resources.GUIModUnknown;
                GameCompatibilityLong = string.Format(
                    Properties.Resources.GUIModGameCompatibilityLong,
                    GameCompatibility,
                    mod.version
                );
            }

            UpdateIsCached();
        }

        /// <summary>
        /// Initialize a GUIMod based on just an identifier
        /// </summary>
        /// <param name="identifier">The id of the module to represent</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="current_game_version">Current game version</param>
        /// <param name="incompatible">If true, mark this module as incompatible</param>
        public GUIMod(string identifier, IRegistryQuerier registry, GameVersionCriteria current_game_version, bool? incompatible = null)
        {
            Identifier     = identifier;
            IsAutodetected = registry.IsAutodetected(identifier);
            DownloadCount  = registry.DownloadCount(identifier);
            if (IsAutodetected)
            {
                IsInstalled = true;
            }

            ModuleVersion latest_version = null;
            try
            {
                LatestCompatibleMod = registry.LatestAvailable(identifier, current_game_version);
                latest_version = LatestCompatibleMod?.version;
            }
            catch (ModuleNotFoundKraken)
            {
            }

            IsIncompatible = incompatible ?? LatestCompatibleMod == null;

            // Let's try to find the compatibility for this mod. If it's not in the registry at
            // all (because it's a DarkKAN mod) then this might fail.

            CkanModule latest_available_for_any_ksp = null;

            try
            {
                latest_available_for_any_ksp = registry.LatestAvailable(identifier, null);
            }
            catch
            { }

            // If there's known information for this mod in any form, calculate the highest compatible
            // KSP.
            if (latest_available_for_any_ksp != null)
            {
                GameCompatibilityVersion = registry.LatestCompatibleKSP(identifier);
                GameCompatibility = GameCompatibilityVersion?.ToYalovString()
                    ?? Properties.Resources.GUIModUnknown;
                GameCompatibilityLong = string.Format(Properties.Resources.GUIModGameCompatibilityLong, GameCompatibility, latest_available_for_any_ksp.version);
            }

            if (latest_version != null)
            {
                LatestVersion = latest_version.ToString();
            }
            else if (latest_available_for_any_ksp != null)
            {
                LatestVersion = latest_available_for_any_ksp.version.ToString();
            }
            else
            {
                LatestVersion = "-";
            }

            SearchableIdentifier = CkanModule.nonAlphaNums.Replace(Identifier, "");
        }

        /// <summary>
        /// Refresh the IsCached property based on current contents of the cache
        /// </summary>
        public void UpdateIsCached()
        {
            if (Main.Instance?.Manager?.Cache == null || Mod?.download == null)
            {
                return;
            }

            IsCached = Main.Instance.Manager.Cache.IsMaybeCachedZip(Mod);
        }

        public CkanModule ToCkanModule()
        {
            if (!IsCKAN)
                throw new InvalidCastException(Properties.Resources.GUIModMethodNotCKAN);
            var mod = Mod as CkanModule;
            return mod;
        }

        public CkanModule ToModule()
        {
            return Mod;
        }

        public IEnumerable<ModChange> GetModChanges()
        {
            bool selectedIsInstalled = SelectedMod?.Equals(InstalledMod?.Module)
                ?? InstalledMod?.Module.Equals(SelectedMod)
                // Both null
                ?? true;
            if (IsInstalled && (IsInstallChecked && HasUpdate && IsUpgradeChecked))
            {
                yield return new ModUpgrade(Mod, GUIModChangeType.Update, null, SelectedMod);
            }
            else if (IsReplaceChecked)
            {
                yield return new ModChange(Mod, GUIModChangeType.Replace, null);
            }
            else if (!selectedIsInstalled)
            {
                if (InstalledMod != null)
                {
                    yield return new ModChange(InstalledMod.Module, GUIModChangeType.Remove, null);
                }
                if (SelectedMod != null)
                {
                    yield return new ModChange(SelectedMod, GUIModChangeType.Install, null);
                }
            }
        }

        /// <summary>
        /// Set the properties to match a change set element.
        /// Doesn't update grid, use SetInstallChecked or SetUpgradeChecked
        /// if you need to update the grid.
        /// </summary>
        /// <param name="change">Type of change</param>
        public void SetRequestedChange(GUIModChangeType change)
        {
            switch (change)
            {
                case GUIModChangeType.Install:
                    IsInstallChecked = true;
                    IsUpgradeChecked = false;
                    break;
                case GUIModChangeType.Remove:
                    IsInstallChecked = false;
                    IsUpgradeChecked = false;
                    IsReplaceChecked = false;
                    break;
                case GUIModChangeType.Update:
                    IsInstallChecked = true;
                    IsUpgradeChecked = true;
                    break;
                case GUIModChangeType.Replace:
                    IsInstallChecked = true;
                    IsReplaceChecked = true;
                    break;
            }
        }

        public void SetUpgradeChecked(DataGridViewRow row, DataGridViewColumn col, bool? set_value_to = null)
        {
            var update_cell = row?.Cells[col.Index] as DataGridViewCheckBoxCell;
            if (update_cell != null)
            {
                var old_value = (bool) update_cell.Value;

                bool value = set_value_to ?? old_value;
                IsUpgradeChecked = value;
                if (old_value != value)
                {
                    update_cell.Value = value;
                    SelectedMod = value ? LatestCompatibleMod : (SelectedMod ?? InstalledMod?.Module);
                }
                else if (!set_value_to.HasValue)
                {
                    var isLatest = (LatestCompatibleMod?.Equals(selectedMod) ?? false);
                    SelectedMod = value ? LatestCompatibleMod
                        : isLatest ? InstalledMod?.Module : SelectedMod;
                }
            }
        }

        public void SetInstallChecked(DataGridViewRow row, DataGridViewColumn col, bool? set_value_to = null)
        {
            var install_cell = row?.Cells[col.Index] as DataGridViewCheckBoxCell;
            if (install_cell != null)
            {
                bool changeTo = set_value_to ?? (bool)install_cell.Value;
                if (IsInstallChecked != changeTo)
                {
                    IsInstallChecked = changeTo;
                }
                SelectedMod = changeTo ? (SelectedMod ?? Mod) : null;
                // Setting this property causes ModList_CellValueChanged to be called,
                // which calls SetInstallChecked again. Treat it conservatively.
                if ((bool)install_cell.Value != IsInstallChecked)
                {
                    install_cell.Value = IsInstallChecked;
                    if (row.DataGridView != null)
                    {
                        // These calls are needed to force the UI to update,
                        // otherwise the checkbox will look checked when it's unchecked or vice versa
                        row.DataGridView.RefreshEdit();
                        row.DataGridView.Refresh();
                    }
                }
            }
        }

        public void SetReplaceChecked(DataGridViewRow row, DataGridViewColumn col, bool? set_value_to = null)
        {
            var replace_cell = row.Cells[col.Index] as DataGridViewCheckBoxCell;
            if (replace_cell != null)
            {
                var old_value = (bool) replace_cell.Value;

                bool value = set_value_to ?? old_value;
                IsReplaceChecked = value;
                if (old_value != value)
                    replace_cell.Value = value;
            }
        }

        public void SetAutoInstallChecked(DataGridViewRow row, DataGridViewColumn col, bool? set_value_to = null)
        {
            var auto_cell = row.Cells[col.Index] as DataGridViewCheckBoxCell;
            if (auto_cell != null)
            {
                var old_value = (bool) auto_cell.Value;

                bool value = set_value_to ?? old_value;
                IsAutoInstalled = value;
                InstalledMod.AutoInstalled = value;

                if (old_value != value)
                {
                    auto_cell.Value = value;
                }
            }
        }

        private bool Equals(GUIMod other)
        {
            return Equals(Identifier, other.Identifier);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GUIMod) obj);
        }

        public override int GetHashCode()
        {
            return Identifier?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"{ToModule()}";
        }
    }
}
