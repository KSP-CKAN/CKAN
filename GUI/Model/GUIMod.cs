using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;

using CKAN.Versioning;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public sealed class GUIMod : INotifyPropertyChanged
    {
        private CkanModule      Mod                 { get; set; }
        public  CkanModule      LatestCompatibleMod { get; private set; }
        public  CkanModule      LatestAvailableMod  { get; private set; }
        public  InstalledModule InstalledMod        { get; private set; }

        private GameInstance        currentInstance => Main.Instance?.CurrentInstance;
        private GameInstanceManager manager         => Main.Instance?.Manager;

        /// <summary>
        /// The module of the checkbox that is checked in the MainAllModVersions list if any,
        /// null otherwise.
        /// Used for generating this mod's part of the change set.
        /// </summary>
        public CkanModule SelectedMod
        {
            get => selectedMod;
            set
            {
                if (!(selectedMod?.Equals(value) ?? value?.Equals(selectedMod) ?? true))
                {
                    selectedMod = value;
                    OnPropertyChanged();
                }
            }
        }
        private CkanModule selectedMod = null;

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
        public bool HasUpdate { get; set; }
        public bool HasReplacement { get; private set; }
        public bool IsIncompatible { get; private set; }
        public bool IsAutodetected { get; private set; }
        public List<string> Authors => Mod.author ?? new List<string>();
        public string InstalledVersion { get; private set; }
        public DateTime? InstallDate { get; private set; }
        public string LatestVersion { get; private set; }
        public string DownloadSize { get; private set; }
        public string InstallSize { get; private set; }
        public int? DownloadCount { get; private set; }
        public bool IsCached { get; private set; }

        // These indicate the maximum game version that the maximum available
        // version of this mod can handle. The "Long" version also indicates
        // to the user if a mod upgrade would be required. (#1270)
        public GameVersion GameCompatibilityVersion { get; private set; }
        public string GameCompatibility
            => GameCompatibilityVersion == null ? Properties.Resources.GUIModUnknown
               : GameCompatibilityVersion.IsAny ? GameVersion.AnyString
               : GameCompatibilityVersion.ToString();

        public string Abstract { get; private set; }
        public string Description { get; private set; }
        public string Identifier { get; private set; }
        public bool IsNew { get; set; }
        public bool IsCKAN => Mod != null;
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
            // Compatible mods are installable, but so are mods that are already installed
            => !IsIncompatible || IsInstalled;

        public string Version
            => IsInstalled ? InstalledVersion : LatestVersion;

        /// <summary>
        /// Initialize a GUIMod based on an InstalledModule
        /// </summary>
        /// <param name="instMod">The installed module to represent</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="current_game_version">Current game version</param>
        /// <param name="incompatible">If true, mark this module as incompatible</param>
        public GUIMod(InstalledModule       instMod,
                      RepositoryDataManager repoDataMgr,
                      IRegistryQuerier      registry,
                      GameVersionCriteria   current_game_version,
                      bool? incompatible,
                      bool  hideEpochs,
                      bool  hideV)
            : this(instMod.Module, repoDataMgr, registry, current_game_version, incompatible, hideEpochs, hideV)
        {
            IsInstalled      = true;
            InstalledMod     = instMod;
            selectedMod      = registry.GetModuleByVersion(instMod.identifier, instMod.Module.version)
                               ?? instMod.Module;
            IsAutoInstalled  = instMod.AutoInstalled;
            InstallDate      = instMod.InstallTime;

            InstalledVersion = instMod.Module.version.ToString(hideEpochs, hideV);
            if (LatestVersion == null || LatestVersion.Equals("-"))
            {
                LatestVersion = InstalledVersion;
            }
            // For mods not known to the registry LatestCompatibleMod is null, however the installed module might be compatible
            IsIncompatible   = incompatible ?? (LatestCompatibleMod == null && !instMod.Module.IsCompatible(current_game_version));
        }

        /// <summary>
        /// Initialize a GUIMod based on a CkanModule
        /// </summary>
        /// <param name="mod">The module to represent</param>
        /// <param name="registry">CKAN registry object for current game instance</param>
        /// <param name="current_game_version">Current game version</param>
        /// <param name="incompatible">If true, mark this module as incompatible</param>
        public GUIMod(CkanModule            mod,
                      RepositoryDataManager repoDataMgr,
                      IRegistryQuerier      registry,
                      GameVersionCriteria   current_game_version,
                      bool? incompatible,
                      bool  hideEpochs,
                      bool  hideV)
            : this(mod.identifier, repoDataMgr, registry, current_game_version, incompatible, hideEpochs, hideV)
        {
            Mod           = mod;

            Name          = mod.name.Trim();
            Abstract      = mod.@abstract.Trim();
            Description   = mod.description?.Trim() ?? string.Empty;
            Abbrevation   = new string(Name.Split(' ').Where(s => s.Length > 0).Select(s => s[0]).ToArray());

            HasReplacement = registry.GetReplacement(mod, current_game_version) != null;
            DownloadSize   = mod.download_size == 0 ? Properties.Resources.GUIModNSlashA : CkanModule.FmtSize(mod.download_size);
            InstallSize    = mod.install_size  == 0 ? Properties.Resources.GUIModNSlashA : CkanModule.FmtSize(mod.install_size);

            // Get the Searchables.
            SearchableName        = mod.SearchableName;
            SearchableAbstract    = mod.SearchableAbstract;
            SearchableDescription = mod.SearchableDescription;
            SearchableAuthors     = mod.SearchableAuthors;

            // If not set in GUIMod(identifier, ...) (because the mod is not known to the registry),
            // set based on the the data we have from the CkanModule.
            if (GameCompatibilityVersion == null)
            {
                GameCompatibilityVersion = mod.LatestCompatibleGameVersion();
                if (GameCompatibilityVersion.IsAny)
                {
                    GameCompatibilityVersion = mod.LatestCompatibleRealGameVersion(
                        currentInstance?.game.KnownVersions
                        ?? new List<GameVersion>() {});
                }
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
        private GUIMod(string                identifier,
                       RepositoryDataManager repoDataMgr,
                       IRegistryQuerier      registry,
                       GameVersionCriteria   current_game_version,
                       bool? incompatible,
                       bool  hideEpochs,
                       bool  hideV)
        {
            Identifier     = identifier;
            IsAutodetected = registry.IsAutodetected(identifier);
            DownloadCount  = repoDataMgr.GetDownloadCount(registry.Repositories.Values, identifier);
            if (IsAutodetected)
            {
                IsInstalled = true;
            }

            ModuleVersion latest_version = null;
            if (incompatible != true)
            {
                try
                {
                    LatestCompatibleMod = registry.LatestAvailable(identifier, current_game_version);
                    latest_version = LatestCompatibleMod?.version;
                }
                catch (ModuleNotFoundKraken)
                {
                    if (!incompatible.HasValue)
                    {
                        incompatible = true;
                    }
                }
            }

            IsIncompatible = incompatible ?? LatestCompatibleMod == null;

            // Let's try to find the compatibility for this mod. If it's not in the registry at
            // all (because it's a DarkKAN mod) then this might fail.

            try
            {
                LatestAvailableMod = registry.LatestAvailable(identifier, null);
            }
            catch
            { }

            // If there's known information for this mod in any form, calculate the highest compatible
            // KSP.
            if (LatestAvailableMod != null)
            {
                GameCompatibilityVersion = registry.LatestCompatibleGameVersion(
                    currentInstance?.game.KnownVersions ?? new List<GameVersion>() {},
                    identifier);
            }

            if (latest_version != null)
            {
                LatestVersion = latest_version.ToString(hideEpochs, hideV);
            }
            else if (LatestAvailableMod != null)
            {
                LatestVersion = LatestAvailableMod.version.ToString(hideEpochs, hideV);
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
            if (manager?.Cache == null || Mod?.download == null)
            {
                return;
            }

            IsCached = manager.Cache.IsMaybeCachedZip(Mod);
        }

        /// <summary>
        /// Get the CkanModule associated with this GUIMod.
        /// See <see cref="ToModule"/> for a method that doesn't throw.
        /// </summary>
        /// <returns>The CkanModule associated with this GUIMod</returns>
        /// <exception cref="InvalidCastException">Thrown if no CkanModule is associated</exception>
        public CkanModule ToCkanModule()
        {
            if (!IsCKAN)
            {
                throw new InvalidCastException(Properties.Resources.GUIModMethodNotCKAN);
            }

            var mod = Mod;
            return mod;
        }

        /// <summary>
        /// Get the CkanModule associated with this GUIMod.
        /// </summary>
        /// <returns>The CkanModule associated with this GUIMod or null if there is none</returns>
        public CkanModule ToModule() => Mod;

        public IEnumerable<ModChange> GetModChanges(bool upgradeChecked, bool replaceChecked)
        {
            if (replaceChecked)
            {
                yield return new ModChange(Mod, GUIModChangeType.Replace);
            }
            else if (!(SelectedMod?.Equals(InstalledMod?.Module)
                       ?? InstalledMod?.Module?.Equals(SelectedMod)
                       // Both null
                       ?? true))
            {
                if (InstalledMod != null && SelectedMod == LatestAvailableMod)
                {
                    yield return new ModUpgrade(Mod,
                                                GUIModChangeType.Update,
                                                SelectedMod,
                                                false);
                }
                else
                {
                    if (InstalledMod != null)
                    {
                        yield return new ModChange(InstalledMod.Module, GUIModChangeType.Remove);
                    }
                    if (SelectedMod != null)
                    {
                        yield return new ModChange(SelectedMod, GUIModChangeType.Install);
                    }
                }
            }
            else if (upgradeChecked)
            {
                // Reinstall
                yield return new ModUpgrade(Mod,
                                            GUIModChangeType.Update,
                                            SelectedMod,
                                            false);
            }
        }

        public void SetAutoInstallChecked(DataGridViewRow row, DataGridViewColumn col, bool? set_value_to = null)
        {
            if (row.Cells[col.Index] is DataGridViewCheckBoxCell auto_cell)
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

        private bool Equals(GUIMod other) => Equals(Identifier, other.Identifier);

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((GUIMod) obj);
        }

        public override int GetHashCode() => Identifier?.GetHashCode() ?? 0;

        public override string ToString() => Mod.ToString();
    }
}
