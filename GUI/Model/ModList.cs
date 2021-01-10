using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CKAN.Versioning;

namespace CKAN
{
    public enum GUIModFilter
    {
        Compatible               = 0,
        Installed                = 1,
        InstalledUpdateAvailable = 2,
        NewInRepository          = 3,
        NotInstalled             = 4,
        Incompatible             = 5,
        All                      = 6,
        Cached                   = 7,
        Replaceable              = 8,
        Uncached                 = 9,
        CustomLabel              = 10,
        Tag                      = 11,
    }

    public class ModList
    {
        //identifier, row
        internal Dictionary<string, DataGridViewRow> full_list_of_mod_rows;

        public ModList(ModFiltersUpdatedEvent onModFiltersUpdated)
        {
            Modules = new ReadOnlyCollection<GUIMod>(new List<GUIMod>());
            ModFiltersUpdated += onModFiltersUpdated ?? (source => { });
            ModFiltersUpdated(this);
        }

        public delegate void ModFiltersUpdatedEvent(ModList source);

        //TODO Move to relationship resolver and have it use this.
        public delegate Task<CkanModule> HandleTooManyProvides(TooManyModsProvideKraken kraken);

        public event ModFiltersUpdatedEvent ModFiltersUpdated;
        public ReadOnlyCollection<GUIMod> Modules { get; set; }

        public GUIModFilter ModFilter
        {
            get { return _modFilter; }
            set
            {
                var old = _modFilter;
                _modFilter = value;
                if (!old.Equals(value)) ModFiltersUpdated(this);
            }
        }

        public readonly ModuleLabelList ModuleLabels = ModuleLabelList.Load(ModuleLabelList.DefaultPath)
            ?? ModuleLabelList.GetDefaultLabels();

        public readonly ModuleTagList ModuleTags = ModuleTagList.Load(ModuleTagList.DefaultPath)
            ?? new ModuleTagList();

        private ModuleTag _tagFilter;
        public ModuleTag TagFilter
        {
            get { return _tagFilter; }
            set
            {
                var old = _tagFilter;
                _tagFilter = value;
                if (!old?.Equals(value) ?? !value?.Equals(old) ?? false)
                {
                    ModFiltersUpdated(this);
                }
            }
        }

        private ModuleLabel _customLabelFilter;
        public ModuleLabel CustomLabelFilter
        {
            get { return _customLabelFilter; }
            set
            {
                var old = _customLabelFilter;
                _customLabelFilter = value;
                if (!old?.Equals(value) ?? !value?.Equals(old) ?? false)
                {
                    ModFiltersUpdated(this);
                }
            }
        }

        private GUIModFilter _modFilter = GUIModFilter.Compatible;
        private ModSearch activeSearch = null;

        public void SetSearch(ModSearch newSearch)
        {
            activeSearch = newSearch;
            ModFiltersUpdated(this);
        }

        /// <summary>
        /// This function returns a changeset based on the selections of the user.
        /// Currently returns null if a conflict is detected.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="changeSet"></param>
        /// <param name="installer">A module installer for the current game instance</param>
        /// <param name="version">The version of the current game instance</param>
        public IEnumerable<ModChange> ComputeChangeSetFromModList(
            IRegistryQuerier registry, HashSet<ModChange> changeSet, ModuleInstaller installer,
            GameVersionCriteria version)
        {
            var modules_to_install = new HashSet<CkanModule>();
            var modules_to_remove = new HashSet<CkanModule>();

            foreach (var change in changeSet)
            {
                switch (change.ChangeType)
                {
                    case GUIModChangeType.None:
                        break;
                    case GUIModChangeType.Update:
                    case GUIModChangeType.Install:
                        modules_to_install.Add(change.Mod);
                        break;
                    case GUIModChangeType.Remove:
                        modules_to_remove.Add(change.Mod);
                        break;
                    case GUIModChangeType.Replace:
                        ModuleReplacement repl = registry.GetReplacement(change.Mod, version);
                        if (repl != null)
                        {
                            modules_to_remove.Add(repl.ToReplace);
                            modules_to_install.Add(repl.ReplaceWith);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var installed_modules =
                registry.InstalledModules.Select(imod => imod.Module).ToDictionary(mod => mod.identifier, mod => mod);

            foreach (var dependent in registry.FindReverseDependencies(
                modules_to_remove
                    .Select(mod => mod.identifier)
                    .Except(modules_to_install.Select(m => m.identifier)),
                modules_to_install
            ))
            {
                //TODO This would be a good place to have an event that alters the row's graphics to show it will be removed
                CkanModule depMod;
                if (installed_modules.TryGetValue(dependent, out depMod))
                {
                    CkanModule module_by_version = registry.GetModuleByVersion(depMod.identifier,
                    depMod.version)
                        ?? registry.InstalledModule(dependent).Module;
                    changeSet.Add(new ModChange(module_by_version, GUIModChangeType.Remove, null));
                    modules_to_remove.Add(module_by_version);
                }
            }
            foreach (var im in registry.FindRemovableAutoInstalled(
                registry.InstalledModules.Where(im => !modules_to_remove.Any(m => m.identifier == im.identifier) || modules_to_install.Any(m => m.identifier == im.identifier))
            ))
            {
                changeSet.Add(new ModChange(im.Module, GUIModChangeType.Remove, new SelectionReason.NoLongerUsed()));
                modules_to_remove.Add(im.Module);
            }

            // Get as many dependencies as we can, but leave decisions and prompts for installation time
            RelationshipResolverOptions opts = RelationshipResolver.DependsOnlyOpts();
            opts.without_toomanyprovides_kraken = true;
            opts.without_enforce_consistency    = true;

            var resolver = new RelationshipResolver(
                modules_to_install,
                modules_to_remove,
                opts, registry, version);
            changeSet.UnionWith(
                resolver.ModList()
                    .Select(m => new ModChange(m, GUIModChangeType.Install, resolver.ReasonFor(m))));

            return changeSet;
        }

        public bool IsVisible(GUIMod mod, string instanceName)
        {
            return (activeSearch?.Matches(mod) ?? true)
                && IsModInFilter(ModFilter, TagFilter, CustomLabelFilter, mod)
                && !HiddenByTagsOrLabels(ModFilter, TagFilter, CustomLabelFilter, mod, instanceName);
        }

        private bool HiddenByTagsOrLabels(GUIModFilter filter, ModuleTag tag, ModuleLabel label, GUIMod m, string instanceName)
        {
            if (filter != GUIModFilter.CustomLabel)
            {
                // "Hide" labels apply to all non-custom filters
                if (ModuleLabels?.LabelsFor(instanceName)
                    .Where(l => l != label && l.Hide)
                    .Any(l => l.ModuleIdentifiers.Contains(m.Identifier))
                    ?? false)
                {
                    return true;
                }
                if (ModuleTags?.Tags?.Values
                    .Where(t => t != tag && t.Visible == false)
                    .Any(t => t.ModuleIdentifiers.Contains(m.Identifier))
                    ?? false)
                {
                    return true;
                }
            }
            return false;
        }

        public int CountModsByFilter(GUIModFilter filter)
        {
            if (filter == GUIModFilter.All)
            {
                // Don't check each one
                return Modules.Count;
            }
            // Tags and Labels are not counted here
            return Modules.Count(m => IsModInFilter(filter, null, null, m));
        }

        /// <summary>
        /// Constructs the mod list suitable for display to the user.
        /// Manipulates <c>full_list_of_mod_rows</c>.
        /// </summary>
        /// <param name="modules">A list of modules that may require updating</param>
        /// <param name="mc">Changes the user has made</param>
        /// <param name="hideEpochs">If true, remove epochs from the displayed versions</param>
        /// <param name="hideV">If true, strip 'v' prefix from versions</param>
        /// <returns>The mod list</returns>
        public IEnumerable<DataGridViewRow> ConstructModList(
            IEnumerable<GUIMod> modules, string instanceName, IEnumerable<ModChange> mc = null,
            bool hideEpochs = false, bool hideV = false)
        {
            List<ModChange> changes = mc?.ToList();
            full_list_of_mod_rows = modules.ToDictionary(
                gm => gm.Identifier,
                gm => MakeRow(gm, changes, instanceName, hideEpochs, hideV)
            );
            return full_list_of_mod_rows.Values;
        }

        private DataGridViewRow MakeRow(GUIMod mod, List<ModChange> changes, string instanceName, bool hideEpochs = false, bool hideV = false)
        {
            DataGridViewRow item = new DataGridViewRow() {Tag = mod};

            Color? myColor = ModuleLabels.LabelsFor(instanceName)
                .FirstOrDefault(l => l.ModuleIdentifiers.Contains(mod.Identifier))
                ?.Color;
            if (myColor.HasValue)
            {
                item.DefaultCellStyle.BackColor = myColor.Value;
            }

            ModChange myChange = changes?.FindLast((ModChange ch) => ch.Mod.Equals(mod));

            var selecting = mod.IsAutodetected
                ? new DataGridViewTextBoxCell()
                {
                    Value = Properties.Resources.MainModListAutoDetected
                }
                : mod.IsInstallable()
                ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                {
                    Value = myChange == null ? mod.IsInstalled
                        : myChange.ChangeType == GUIModChangeType.Install ? true
                        : myChange.ChangeType == GUIModChangeType.Remove  ? false
                        : mod.IsInstalled
                }
                : new DataGridViewTextBoxCell()
                {
                    Value = "-"
                };

            var autoInstalled = mod.IsInstalled && !mod.IsAutodetected
                ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                {
                    Value = mod.IsAutoInstalled
                }
                : new DataGridViewTextBoxCell()
                {
                    Value = "-"
                };

            var updating = mod.IsInstallable() && mod.HasUpdate
                ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                {
                    Value = myChange == null ? false
                        : myChange.ChangeType == GUIModChangeType.Update ? true
                        : false
                }
                : new DataGridViewTextBoxCell()
                {
                    Value = "-"
                };

            var replacing = IsModInFilter(GUIModFilter.Replaceable, null, null, mod)
                ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                {
                    Value = myChange == null ? false
                        : myChange.ChangeType == GUIModChangeType.Replace ? true
                        : false
                }
                : new DataGridViewTextBoxCell()
                {
                    Value = "-"
                };

            var name   = new DataGridViewTextBoxCell { Value = mod.Name   .Replace("&", "&&") };
            var author = new DataGridViewTextBoxCell { Value = mod.Authors.Replace("&", "&&") };

            var installVersion = new DataGridViewTextBoxCell()
            {
                Value = hideEpochs
                    ? (hideV
                        ? ModuleInstaller.StripEpoch(ModuleInstaller.StripV(mod.InstalledVersion ?? ""))
                        : ModuleInstaller.StripEpoch(mod.InstalledVersion ?? ""))
                    : (hideV
                        ? ModuleInstaller.StripV(mod.InstalledVersion ?? "")
                        : mod.InstalledVersion ?? "")
            };

            var latestVersion = new DataGridViewTextBoxCell()
            {
                Value =
                    hideEpochs ?
                        (hideV ? ModuleInstaller.StripEpoch(ModuleInstaller.StripV(mod.LatestVersion))
                        : ModuleInstaller.StripEpoch(mod.LatestVersion))
                    : (hideV ? ModuleInstaller.StripV(mod.LatestVersion)
                        : mod.LatestVersion)
            };

            var downloadCount = new DataGridViewTextBoxCell { Value = $"{mod.DownloadCount:N0}"       };
            var compat        = new DataGridViewTextBoxCell { Value = mod.GameCompatibility           };
            var size          = new DataGridViewTextBoxCell { Value = mod.DownloadSize                };
            var releaseDate   = new DataGridViewTextBoxCell { Value = mod.ToModule().release_date     };
            var installDate   = new DataGridViewTextBoxCell { Value = mod.InstallDate                 };
            var desc          = new DataGridViewTextBoxCell { Value = mod.Abstract.Replace("&", "&&") };

            item.Cells.AddRange(selecting, autoInstalled, updating, replacing, name, author, installVersion, latestVersion, compat, size, releaseDate, installDate, downloadCount, desc);

            selecting.ReadOnly     = selecting     is DataGridViewTextBoxCell;
            autoInstalled.ReadOnly = autoInstalled is DataGridViewTextBoxCell;
            updating.ReadOnly      = updating      is DataGridViewTextBoxCell;

            return item;
        }

        public Color GetRowBackground(GUIMod mod, bool conflicted, string instanceName)
        {
            if (conflicted)
            {
                return Color.LightCoral;
            }
            DataGridViewRow row;
            if (full_list_of_mod_rows.TryGetValue(mod.Identifier, out row))
            {
                Color? myColor = ModuleLabels.LabelsFor(instanceName)
                    .FirstOrDefault(l => l.ModuleIdentifiers.Contains(mod.Identifier))
                    ?.Color;
                if (myColor.HasValue)
                {
                    return myColor.Value;
                }
            }
            return Color.Empty;
        }

        /// <summary>
        /// Update the color and visible state of the given row
        /// after it has been added to or removed from a label group
        /// </summary>
        /// <param name="mod">The mod that needs an update</param>
        public void ReapplyLabels(GUIMod mod, bool conflicted, string instanceName)
        {
            DataGridViewRow row;
            if (full_list_of_mod_rows.TryGetValue(mod.Identifier, out row))
            {
                row.DefaultCellStyle.BackColor = GetRowBackground(mod, conflicted, instanceName);
                row.Visible = IsVisible(mod, instanceName);
            }
        }

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        public string StripEpoch(string version)
        {
            // If our version number starts with a string of digits, followed by
            // a colon, and then has no more colons, we're probably safe to assume
            // the first string of digits is an epoch
            //return Regex.IsMatch(version, @"^[0-9][0-9]*:[^:]+$") ? Regex.Replace(version, @"^([^:]+):([^:]+)$", @"$2") : version;
            return ContainsEpoch.IsMatch(version) ? RemoveEpoch.Replace(version, @"$2") : version;
        }

        private static readonly Regex ContainsEpoch = new Regex(@"^[0-9][0-9]*:[^:]+$", RegexOptions.Compiled);
        private static readonly Regex RemoveEpoch   = new Regex(@"^([^:]+):([^:]+)$",   RegexOptions.Compiled);

        private bool IsModInFilter(GUIModFilter filter, ModuleTag tag, ModuleLabel label, GUIMod m)
        {
            switch (filter)
            {
                case GUIModFilter.Compatible:               return !m.IsIncompatible;
                case GUIModFilter.Installed:                return m.IsInstalled;
                case GUIModFilter.InstalledUpdateAvailable: return m.IsInstalled && m.HasUpdate;
                case GUIModFilter.Cached:                   return m.IsCached;
                case GUIModFilter.Uncached:                 return !m.IsCached;
                case GUIModFilter.NewInRepository:          return m.IsNew;
                case GUIModFilter.NotInstalled:             return !m.IsInstalled;
                case GUIModFilter.Incompatible:             return m.IsIncompatible;
                case GUIModFilter.Replaceable:              return m.IsInstalled && m.HasReplacement;
                case GUIModFilter.All:                      return true;
                case GUIModFilter.Tag:                      return tag?.ModuleIdentifiers.Contains(m.Identifier)
                    ?? ModuleTags.Untagged.Contains(m.Identifier);
                case GUIModFilter.CustomLabel:              return label?.ModuleIdentifiers?.Contains(m.Identifier) ?? false;
                default:                                    throw new Kraken(string.Format(Properties.Resources.MainModListUnknownFilter, filter));
            }
        }

        public static Dictionary<GUIMod, string> ComputeConflictsFromModList(IRegistryQuerier registry,
            IEnumerable<ModChange> change_set, GameVersionCriteria ksp_version)
        {
            var modules_to_install = new HashSet<string>();
            var modules_to_remove = new HashSet<string>();
            var options = new RelationshipResolverOptions
            {
                without_toomanyprovides_kraken = true,
                proceed_with_inconsistencies = true,
                without_enforce_consistency = true,
                with_recommends = false
            };

            foreach (var change in change_set)
            {
                switch (change.ChangeType)
                {
                    case GUIModChangeType.None:
                        break;
                    case GUIModChangeType.Install:
                        modules_to_install.Add(change.Mod.identifier);
                        break;
                    case GUIModChangeType.Remove:
                        modules_to_remove.Add(change.Mod.identifier);
                        break;
                    case GUIModChangeType.Update:
                        break;
                    case GUIModChangeType.Replace:
                        ModuleReplacement repl = registry.GetReplacement(change.Mod, ksp_version);
                        if (repl != null)
                        {
                            modules_to_remove.Add(repl.ToReplace.identifier);
                            modules_to_install.Add(repl.ReplaceWith.identifier);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Only check mods that would exist after the changes are made.
            IEnumerable<CkanModule> installed = registry.InstalledModules.Where(
                im => !modules_to_remove.Contains(im.Module.identifier)
            ).Select(im => im.Module);

            // Convert ONLY modules_to_install with CkanModule.FromIDandVersion,
            // because it may not find already-installed modules.
            IEnumerable<CkanModule> mods_to_check = installed.Union(
                modules_to_install.Except(modules_to_remove).Select(
                    name => CkanModule.FromIDandVersion(registry, name, ksp_version)
                )
            );
            var resolver = new RelationshipResolver(
                mods_to_check,
                change_set.Where(ch => ch.ChangeType == GUIModChangeType.Remove)
                    .Select(ch => ch.Mod),
                options, registry, ksp_version
            );
            return resolver.ConflictList.ToDictionary(item => new GUIMod(item.Key, registry, ksp_version),
                item => item.Value);
        }

        public HashSet<ModChange> ComputeUserChangeSet(IRegistryQuerier registry)
        {
            var removableAuto = registry?.FindRemovableAutoInstalled(registry?.InstalledModules)
                ?? new InstalledModule[] {};
            return new HashSet<ModChange>(
                Modules
                    .SelectMany(mod => mod.GetModChanges())
                    .Union(removableAuto.Select(im => new ModChange(
                        im.Module,
                        GUIModChangeType.Remove,
                        new SelectionReason.NoLongerUsed())))
            );
        }
    }
}
