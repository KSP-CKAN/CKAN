using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using log4net;

using CKAN.Versioning;
using CKAN.Extensions;

namespace CKAN.GUI
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

        public ModList()
        {
            Modules = new ReadOnlyCollection<GUIMod>(new List<GUIMod>());
        }

        //TODO Move to relationship resolver and have it use this.
        public delegate Task<CkanModule> HandleTooManyProvides(TooManyModsProvideKraken kraken);

        public event Action ModFiltersUpdated;
        public ReadOnlyCollection<GUIMod> Modules { get; set; }

        public readonly ModuleLabelList ModuleLabels = ModuleLabelList.Load(ModuleLabelList.DefaultPath)
            ?? ModuleLabelList.GetDefaultLabels();

        public readonly ModuleTagList ModuleTags = ModuleTagList.Load(ModuleTagList.DefaultPath)
            ?? new ModuleTagList();

        private List<ModSearch> activeSearches = null;

        public void SetSearches(List<ModSearch> newSearches)
        {
            if (!SearchesEqual(activeSearches, newSearches))
            {
                activeSearches = newSearches;

                Main.Instance.configuration.DefaultSearches = activeSearches?.Select(s => s?.Combined ?? "").ToList()
                    ?? new List<string>() { "" };

                ModFiltersUpdated?.Invoke();
            }
        }

        private static bool SearchesEqual(List<ModSearch> a, List<ModSearch> b)
        {
            return a == null ? b == null
                 : b == null ? false
                 : a.SequenceEqual(b);
        }

        private static string FilterName(GUIModFilter filter, ModuleTag tag = null, ModuleLabel label = null)
        {
            switch (filter)
            {
                case GUIModFilter.Compatible:               return Properties.Resources.MainFilterCompatible;
                case GUIModFilter.Incompatible:             return Properties.Resources.MainFilterIncompatible;
                case GUIModFilter.Installed:                return Properties.Resources.MainFilterInstalled;
                case GUIModFilter.NotInstalled:             return Properties.Resources.MainFilterNotInstalled;
                case GUIModFilter.InstalledUpdateAvailable: return Properties.Resources.MainFilterUpgradeable;
                case GUIModFilter.Replaceable:              return Properties.Resources.MainFilterReplaceable;
                case GUIModFilter.Cached:                   return Properties.Resources.MainFilterCached;
                case GUIModFilter.Uncached:                 return Properties.Resources.MainFilterUncached;
                case GUIModFilter.NewInRepository:          return Properties.Resources.MainFilterNew;
                case GUIModFilter.All:                      return Properties.Resources.MainFilterAll;
                case GUIModFilter.CustomLabel:              return string.Format(Properties.Resources.MainFilterLabel, label?.Name ?? "CUSTOM");
                case GUIModFilter.Tag:
                    return tag == null
                        ? Properties.Resources.MainFilterUntagged
                        : string.Format(Properties.Resources.MainFilterTag, tag.Name);
            }
            return "";
        }

        public static SavedSearch FilterToSavedSearch(GUIModFilter filter, ModuleTag tag = null, ModuleLabel label = null)
        {
            return new SavedSearch()
            {
                Name   = FilterName(filter, tag, label),
                Values = new List<string>() { new ModSearch(filter, tag, label).Combined },
            };
        }

        private static readonly RelationshipResolverOptions conflictOptions = new RelationshipResolverOptions()
        {
            without_toomanyprovides_kraken = true,
            proceed_with_inconsistencies   = true,
            without_enforce_consistency    = true,
            with_recommends                = false
        };

        /// <summary>
        /// Returns a changeset and conflicts based on the selections of the user.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="changeSet"></param>
        /// <param name="version">The version of the current game instance</param>
        public Tuple<IEnumerable<ModChange>, Dictionary<CkanModule, string>> ComputeFullChangeSetFromUserChangeSet(
            IRegistryQuerier registry, HashSet<ModChange> changeSet, GameVersionCriteria version)
        {
            var modules_to_install = new List<CkanModule>();
            var modules_to_remove = new HashSet<CkanModule>();
            var upgrading = new HashSet<CkanModule>();

            foreach (var change in changeSet)
            {
                switch (change.ChangeType)
                {
                    case GUIModChangeType.None:
                        break;
                    case GUIModChangeType.Update:
                        var mod = (change as ModUpgrade)?.targetMod ?? change.Mod;
                        modules_to_install.Add(mod);
                        upgrading.Add(mod);
                        break;
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

            var installed_modules = registry.InstalledModules.ToDictionary(
                imod => imod.Module.identifier,
                imod => imod.Module);

            foreach (var dependent in registry.FindReverseDependencies(
                modules_to_remove
                    .Select(mod => mod.identifier)
                    .Except(modules_to_install.Select(m => m.identifier))
                    .ToList(),
                modules_to_install))
            {
                //TODO This would be a good place to have an event that alters the row's graphics to show it will be removed
                CkanModule depMod;
                if (installed_modules.TryGetValue(dependent, out depMod))
                {
                    CkanModule module_by_version = registry.GetModuleByVersion(depMod.identifier,
                    depMod.version)
                        ?? registry.InstalledModule(dependent).Module;
                    changeSet.Add(new ModChange(module_by_version, GUIModChangeType.Remove));
                    modules_to_remove.Add(module_by_version);
                }
            }

            foreach (var im in registry.FindRemovableAutoInstalled(
                InstalledAfterChanges(registry, changeSet, version).ToList(), version))
            {
                changeSet.Add(new ModChange(im.Module, GUIModChangeType.Remove, new SelectionReason.NoLongerUsed()));
                modules_to_remove.Add(im.Module);
            }

            // Get as many dependencies as we can, but leave decisions and prompts for installation time
            var resolver = new RelationshipResolver(
                modules_to_install, modules_to_remove,
                conflictOptions, registry, version);

            // Replace Install entries in changeset with the ones from resolver to get all the reasons
            return new Tuple<IEnumerable<ModChange>, Dictionary<CkanModule, string>>(
                changeSet.Where(ch => !(ch.ChangeType is GUIModChangeType.Install))
                         .OrderBy(ch => ch.Mod.identifier)
                         .Union(resolver.ModList()
                                        // Changeset already contains Update changes for these
                                        .Except(upgrading)
                                        .Where(m => !m.IsMetapackage)
                                        .Select(m => new ModChange(m, GUIModChangeType.Install, resolver.ReasonsFor(m)))),
                resolver.ConflictList);
        }

        /// <summary>
        /// Get the InstalledModules that we'll have after the changeset,
        /// not including dependencies
        /// </summary>
        /// <param name="registry">Registry with currently installed modules</param>
        /// <param name="changeSet">Changes to be made to the installed modules</param>
        /// <param name="crit">Compatible versions of current instance</param>
        /// <returns>Sequence of InstalledModules after the changes are applied, not including dependencies</returns>
        private IEnumerable<InstalledModule> InstalledAfterChanges(
            IRegistryQuerier registry, HashSet<ModChange> changeSet, GameVersionCriteria crit)
        {
            var removingIdents = changeSet
                .Where(ch => ch.ChangeType != GUIModChangeType.Install)
                .Select(ch => ch.Mod.identifier)
                .ToHashSet();
            return registry.InstalledModules
                .Where(im => !removingIdents.Contains(im.identifier))
                .Concat(changeSet
                    .Where(ch => ch.ChangeType != GUIModChangeType.Remove)
                    .Select(ch => new InstalledModule(
                        null,
                        ch.ChangeType == GUIModChangeType.Replace
                            ? registry.GetReplacement(ch.Mod, crit)?.ReplaceWith
                            : (ch as ModUpgrade)?.targetMod ?? ch.Mod,
                        new string[0],
                        false)));
        }

        public bool IsVisible(GUIMod mod, string instanceName)
        {
            return (activeSearches?.Any(s => s?.Matches(mod) ?? true) ?? true)
                && !HiddenByTagsOrLabels(mod, instanceName);
        }

        private bool TagInSearches(ModuleTag tag)
        {
            return activeSearches?.Any(s => s?.TagNames.Contains(tag.Name) ?? false) ?? false;
        }

        private bool LabelInSearches(ModuleLabel label)
        {
            return activeSearches?.Any(s => s?.Labels.Contains(label) ?? false) ?? false;
        }

        private bool HiddenByTagsOrLabels(GUIMod m, string instanceName)
        {
            // "Hide" labels apply to all non-custom filters
            if (ModuleLabels?.LabelsFor(instanceName)
                .Where(l => !LabelInSearches(l) && l.Hide)
                .Any(l => l.ModuleIdentifiers.Contains(m.Identifier))
                ?? false)
            {
                return true;
            }
            if (ModuleTags?.Tags?.Values
                .Where(t => !TagInSearches(t) && t.Visible == false)
                .Any(t => t.ModuleIdentifiers.Contains(m.Identifier))
                ?? false)
            {
                return true;
            }
            return false;
        }

        public int CountModsBySearches(List<ModSearch> searches)
        {
            return Modules.Count(mod => searches?.Any(s => s?.Matches(mod) ?? true) ?? true);
        }

        public int CountModsByFilter(GUIModFilter filter)
        {
            return CountModsBySearches(new List<ModSearch>() { new ModSearch(filter, null, null) });
        }

        /// <summary>
        /// Constructs the mod list suitable for display to the user.
        /// Manipulates <c>full_list_of_mod_rows</c>.
        /// </summary>
        /// <param name="modules">A list of modules that may require updating</param>
        /// <param name="mc">Changes the user has made</param>
        /// <returns>The mod list</returns>
        public IEnumerable<DataGridViewRow> ConstructModList(
            IEnumerable<GUIMod> modules, string instanceName, IEnumerable<ModChange> mc = null)
        {
            List<ModChange> changes = mc?.ToList();
            full_list_of_mod_rows = modules.ToDictionary(
                gm => gm.Identifier,
                gm => MakeRow(gm, changes, instanceName)
            );
            return full_list_of_mod_rows.Values;
        }

        private DataGridViewRow MakeRow(GUIMod mod, List<ModChange> changes, string instanceName)
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
                    Value = mod.IsAutoInstalled,
                    ToolTipText = Properties.Resources.MainModListAutoInstalledToolTip,
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

            var replacing = (mod.IsInstalled && mod.HasReplacement)
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

            var name   = new DataGridViewTextBoxCell { Value = ToGridText(mod.Name)                       };
            var author = new DataGridViewTextBoxCell { Value = ToGridText(string.Join(", ", mod.Authors)) };

            var installVersion = new DataGridViewTextBoxCell()
            {
                Value = mod.InstalledVersion
            };

            var latestVersion = new DataGridViewTextBoxCell()
            {
                Value = mod.LatestVersion
            };

            var downloadCount = new DataGridViewTextBoxCell { Value = $"{mod.DownloadCount:N0}"       };
            var compat        = new DataGridViewTextBoxCell { Value = mod.GameCompatibility           };
            var downloadSize  = new DataGridViewTextBoxCell { Value = mod.DownloadSize                };
            var installSize   = new DataGridViewTextBoxCell { Value = mod.InstallSize                 };
            var releaseDate   = new DataGridViewTextBoxCell { Value = mod.ToModule().release_date     };
            var installDate   = new DataGridViewTextBoxCell { Value = mod.InstallDate                 };
            var desc          = new DataGridViewTextBoxCell { Value = ToGridText(mod.Abstract)        };

            item.Cells.AddRange(selecting, autoInstalled, updating, replacing, name, author, installVersion, latestVersion, compat, downloadSize, installSize, releaseDate, installDate, downloadCount, desc);

            selecting.ReadOnly     = selecting     is DataGridViewTextBoxCell;
            autoInstalled.ReadOnly = autoInstalled is DataGridViewTextBoxCell;
            updating.ReadOnly      = updating      is DataGridViewTextBoxCell;

            return item;
        }

        private static string ToGridText(string text)
            => Platform.IsMono ? text.Replace("&", "&&") : text;

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

        public HashSet<ModChange> ComputeUserChangeSet(IRegistryQuerier registry, GameVersionCriteria crit)
        {
            log.Debug("Computing user changeset");
            var modChanges = Modules.SelectMany(mod => mod.GetModChanges());
            return (registry == null
                ? modChanges
                : modChanges.Union(
                    registry.FindRemovableAutoInstalled(registry.InstalledModules.ToList(), crit)
                        .Select(im => new ModChange(
                            im.Module, GUIModChangeType.Remove,
                            new SelectionReason.NoLongerUsed()))))
                .ToHashSet();
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(ModList));
    }
}
