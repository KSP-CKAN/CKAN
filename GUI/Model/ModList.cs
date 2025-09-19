using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;
using log4net;

using CKAN.Configuration;
using CKAN.Versioning;
using CKAN.Games;
using CKAN.Extensions;

namespace CKAN.GUI
{
    /// <summary>
    /// The holder of the list of mods to be shown.
    /// Should be a pure data model and avoid UI stuff, but it's not there yet.
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class ModList
    {
        /// <summary>
        /// Constructs the mod list suitable for display to the user.
        /// </summary>
        /// <param name="modules">A list of modules that may require updating</param>
        /// <param name="instance">Game instance for getting labels</param>
        /// <param name="allLabels">All label definitions</param>
        /// <param name="coreConfig">Core configuration</param>
        /// <param name="guiConfig">GUI configuration</param>
        /// <param name="mc">Changes the user has made</param>
        /// <returns>The mod list</returns>
        public ModList(IReadOnlyCollection<GUIMod> modules,
                       GameInstance                instance,
                       ModuleLabelList             allLabels,
                       IConfiguration              coreConfig,
                       GUIConfiguration            guiConfig,
                       List<ModChange>?            mc = null)
        {
            this.allLabels        = allLabels;
            this.coreConfig       = coreConfig;
            this.guiConfig        = guiConfig;
            activeSearches        = guiConfig.DefaultSearches
                                             ?.Select(s => ModSearch.Parse(allLabels, instance, s))
                                              .OfType<ModSearch>()
                                              .ToArray()
                                             ?? Array.Empty<ModSearch>();
            Modules               = modules;
            HasAnyInstalled       = Modules.Any(m => m.IsInstalled);
            full_list_of_mod_rows = Modules.AsParallel()
                                           .ToDictionary(gm => gm.Identifier,
                                                         gm => MakeRow(gm, mc, instance));
        }

        // identifier => row
        public readonly IReadOnlyCollection<GUIMod>         Modules;
        public readonly bool                                HasAnyInstalled;
        public readonly Dictionary<string, DataGridViewRow> full_list_of_mod_rows;
        public event    Action?                             ModFiltersUpdated;

        // Unlike GUIMod.IsInstalled, DataGridViewRow.Visible can change on the fly without notifying us
        public bool HasVisibleInstalled()
            => full_list_of_mod_rows.Values.Any(row => ((row.Tag as GUIMod)?.IsInstalled ?? false)
                                                       && row.Visible);

        public void SetSearches(List<ModSearch> newSearches)
        {
            if (!SearchesEqual(activeSearches, newSearches))
            {
                activeSearches = newSearches;
                guiConfig.DefaultSearches = activeSearches.Select(s => s.Combined ?? "")
                                                          .ToList();
                ModFiltersUpdated?.Invoke();
            }
        }

        public static SavedSearch FilterToSavedSearch(GameInstance    instance,
                                                      GUIModFilter    filter,
                                                      ModuleLabelList allLabels,
                                                      ModuleTag?      tag   = null,
                                                      ModuleLabel?    label = null)
            => new SavedSearch()
            {
                Name   = FilterName(filter, tag, label),
                Values = new List<string>()
                         {
                             new ModSearch(allLabels, instance,
                                           filter, tag, label)
                                 .Combined
                             ?? ""
                         },
            };

        public bool IsVisible(GUIMod mod, GameInstance instance, Registry registry)
            => activeSearches.IsEmptyOrAny(s => s.Matches(mod))
               && !HiddenByTagsOrLabels(mod, instance, registry);

        public int CountModsBySearches(List<ModSearch> searches)
            => Modules.Count(mod => searches?.Any(s => s?.Matches(mod) ?? true) ?? true);

        public int CountModsByFilter(GameInstance inst, GUIModFilter filter)
            => CountModsBySearches(new List<ModSearch>() { new ModSearch(allLabels, inst, filter, null, null) });

        private Color GetRowBackground(GUIMod mod, bool conflicted, GameInstance instance)
            => conflicted
                   ? conflictColor
                   : Util.BlendColors(allLabels.LabelsFor(instance.Name)
                                               .Where(l => l.ContainsModule(instance.game,
                                                                            mod.Identifier))
                                               .Select(l => l.Color)
                                               .OfType<Color>()
                                               // No transparent blending
                                               .Where(c => c.A == byte.MaxValue)
                                               .ToArray());

        /// <summary>
        /// Update the color and visible state of the given row
        /// after it has been added to or removed from a label group
        /// </summary>
        /// <param name="mod">The mod that needs an update</param>
        /// <param name="conflicted">True if mod should have a red background</param>
        /// <param name="instance">Game instance for finding labels</param>
        /// <param name="registry">Registry for finding mods</param>
        public DataGridViewRow? ReapplyLabels(GUIMod mod, bool conflicted,
                                              GameInstance instance, Registry registry)
        {
            if (full_list_of_mod_rows.TryGetValue(mod.Identifier, out DataGridViewRow? row))
            {
                row.DefaultCellStyle.BackColor = GetRowBackground(mod, conflicted, instance);
                row.DefaultCellStyle.ForeColor = row.DefaultCellStyle.BackColor.ForeColorForBackColor()
                                                 ?? SystemColors.WindowText;
                row.DefaultCellStyle.SelectionBackColor = SelectionBlend(row.DefaultCellStyle.BackColor);
                row.DefaultCellStyle.SelectionForeColor = row.DefaultCellStyle.SelectionBackColor.ForeColorForBackColor()
                                                          ?? SystemColors.HighlightText;
                row.Visible = IsVisible(mod, instance, registry);
                return row;
            }
            return null;
        }

        public HashSet<ModChange> ComputeUserChangeSet(IRegistryQuerier     registry,
                                                       GameInstance         instance,
                                                       DataGridViewColumn?  upgradeCol,
                                                       DataGridViewColumn?  replaceCol)
        {
            log.Debug("Computing user changeset");
            var modChanges = (full_list_of_mod_rows?.Values
                                                    .SelectMany(row => rowChanges(registry, row, upgradeCol, replaceCol))
                                                   ?? Enumerable.Empty<ModChange>())
                                                   .ToList();

            // Inter-mod dependencies can block some upgrades, which can sometimes but not always
            // be overcome by upgrading both mods. Try to pick the right target versions.
            var upgrades = modChanges.OfType<ModUpgrade>()
                                     // Skip reinstalls
                                     .Where(upg => upg.Mod != upg.targetMod)
                                     .ToArray();
            if (upgrades.Length > 0)
            {
                var upgradeable = registry.CheckUpgradeable(instance,
                                                            // Hold identifiers not chosen for upgrading
                                                            registry.Installed(false)
                                                                    .Select(kvp => kvp.Key)
                                                                    .Except(upgrades.Select(ch => ch.Mod.identifier))
                                                                    .ToHashSet(),
                                                            allLabels.IgnoreMissingIdentifiers(instance)
                                                                     .ToHashSet())
                                          [true]
                                          .ToDictionary(m => m.identifier,
                                                        m => m);
                foreach (var change in upgrades)
                {
                    change.targetMod = upgradeable.TryGetValue(change.Mod.identifier,
                                                               out CkanModule? allowedMod)
                        // Upgrade to the version the registry says we should
                        ? allowedMod
                        // Not upgradeable!
                        : change.Mod;
                    if (change.Mod == change.targetMod)
                    {
                        // This upgrade was voided by dependencies or conflicts
                        modChanges.Remove(change);
                    }
                }
            }

            return modChanges.Union(
                    registry.FindRemovableAutoInstalled(InstalledAfterChanges(registry, modChanges).ToArray(),
                                                        Array.Empty<CkanModule>(),
                                                        instance.game, instance.StabilityToleranceConfig,
                                                        instance.VersionCriteria())
                        .Select(im => new ModChange(
                            im.Module, GUIModChangeType.Remove,
                            new SelectionReason.NoLongerUsed(),
                            coreConfig)))
                .ToHashSet();
        }

        /// <summary>
        /// Returns a changeset and conflicts based on the selections of the user.
        /// </summary>
        /// <param name="registry">The registry for getting available mods</param>
        /// <param name="changeSet">User's choices of installation and removal</param>
        /// <param name="game">Game of the game instance</param>
        /// <param name="stabilityTolerance">Prerelease configuration</param>
        /// <param name="version">The version of the current game instance</param>
        public Tuple<ICollection<ModChange>, Dictionary<CkanModule, string>, List<string>> ComputeFullChangeSetFromUserChangeSet(
            IRegistryQuerier         registry,
            HashSet<ModChange>       changeSet,
            IGame                    game,
            StabilityToleranceConfig stabilityTolerance,
            GameVersionCriteria      version)
        {
            var modules_to_install = new List<CkanModule>();
            var modules_to_remove = new HashSet<CkanModule>();
            var extraInstalls = new HashSet<CkanModule>();

            changeSet.UnionWith(changeSet.Where(ch => ch.ChangeType == GUIModChangeType.Replace)
                                         .Select(ch => registry.GetReplacement(ch.Mod, stabilityTolerance, version))
                                         .OfType<ModuleReplacement>()
                                         .GroupBy(repl => repl.ReplaceWith)
                                         .Select(grp => new ModChange(grp.Key, GUIModChangeType.Install,
                                                                      grp.Select(repl => new SelectionReason.Replacement(repl.ToReplace)),
                                                                      coreConfig))
                                         .ToHashSet());

            foreach (var change in changeSet)
            {
                switch (change.ChangeType)
                {
                    case GUIModChangeType.None:
                        break;
                    case GUIModChangeType.Update:
                        var mod = (change as ModUpgrade)?.targetMod ?? change.Mod;
                        modules_to_install.Add(mod);
                        extraInstalls.Add(mod);
                        break;
                    case GUIModChangeType.Install:
                        modules_to_install.Add(change.Mod);
                        break;
                    case GUIModChangeType.Remove:
                        modules_to_remove.Add(change.Mod);
                        break;
                    case GUIModChangeType.Replace:
                        if (registry.GetReplacement(change.Mod, stabilityTolerance, version) is ModuleReplacement repl)
                        {
                            modules_to_remove.Add(repl.ToReplace);
                            extraInstalls.Add(repl.ReplaceWith);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(change),
                                                              change.ChangeType.ToString());
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
                if (!changeSet.Any(ch => ch.ChangeType == GUIModChangeType.Replace
                                      && ch.Mod.identifier == dependent)
                    && installed_modules.TryGetValue(dependent, out CkanModule? depMod)
                    && (registry.GetModuleByVersion(depMod.identifier, depMod.version)
                        ?? registry.InstalledModule(dependent)?.Module)
                        is CkanModule modByVer)
                {
                    changeSet.Add(new ModChange(modByVer, GUIModChangeType.Remove,
                                                new SelectionReason.DependencyRemoved(),
                                                coreConfig));
                    modules_to_remove.Add(modByVer);
                }
            }

            foreach (var im in registry.FindRemovableAutoInstalled(
                InstalledAfterChanges(registry, changeSet).ToArray(),
                Array.Empty<CkanModule>(), game, stabilityTolerance, version))
            {
                changeSet.Add(new ModChange(im.Module, GUIModChangeType.Remove, new SelectionReason.NoLongerUsed(),
                                            coreConfig));
                modules_to_remove.Add(im.Module);
            }

            // Get as many dependencies as we can, but leave decisions and prompts for installation time
            var resolver = new RelationshipResolver(
                modules_to_install, modules_to_remove,
                conflictOptions(stabilityTolerance), registry, game, version);

            // Replace Install entries in changeset with the ones from resolver to get all the reasons
            return new Tuple<ICollection<ModChange>, Dictionary<CkanModule, string>, List<string>>(
                changeSet.Where(ch => !(ch.ChangeType is GUIModChangeType.Install
                                        // Leave in replacements
                                        && !ch.Reasons.Any(r => r is SelectionReason.Replacement)))
                         .OrderBy(ch => ch.Mod.identifier)
                         .Union(resolver.ModList()
                                        // Changeset already contains changes for these
                                        .Except(extraInstalls)
                                        .Select(m => new ModChange(m, GUIModChangeType.Install, resolver.ReasonsFor(m),
                                                                   coreConfig)))
                         .ToArray(),
                resolver.ConflictList,
                resolver.ConflictDescriptions.ToList());
        }

        /// <summary>
        /// Check upgradeability of all rows and set GUIMod.HasUpdate appropriately
        /// </summary>
        /// <param name="inst">Current game instance</param>
        /// <param name="registry">Current instance's registry</param>
        /// <param name="ChangeSet">Currently pending changeset</param>
        /// <param name="rows">The grid rows in case we need to replace some</param>
        /// <returns>true if any mod can be updated, false otherwise</returns>
        public bool ResetHasUpdate(GameInstance              inst,
                                   IRegistryQuerier          registry,
                                   List<ModChange>?          ChangeSet,
                                   DataGridViewRowCollection rows)
        {
            var upgGroups = registry.CheckUpgradeable(inst,
                                                      allLabels.HeldIdentifiers(inst)
                                                               .ToHashSet(),
                                                      allLabels.IgnoreMissingIdentifiers(inst)
                                                               .ToHashSet());
            var dlls = registry.InstalledDlls.ToList();
            foreach ((var upgradeable, var mods) in upgGroups)
            {
                foreach (var ident in mods.Select(m => m.identifier))
                {
                    dlls.Remove(ident);
                    CheckRowUpgradeable(inst, ChangeSet, rows, ident, upgradeable);
                }
            }
            // AD mods don't have CkanModules in the return value of CheckUpgradeable
            foreach (var ident in dlls)
            {
                CheckRowUpgradeable(inst, ChangeSet, rows, ident, false);
            }
            return upgGroups[true].Count > 0;
        }

        /// <summary>
        /// Get all the GUI mods for the given instance.
        /// </summary>
        /// <param name="registry">Registry of the instance</param>
        /// <param name="repoData">Repo data of the instance</param>
        /// <param name="inst">Game instance</param>
        /// <param name="allLabels">All label definitions</param>
        /// <param name="cache">Cache for checking if mods are cached</param>
        /// <param name="config">GUI config to use</param>
        /// <returns>Sequence of GUIMods</returns>
        public static IEnumerable<GUIMod> GetGUIMods(IRegistryQuerier      registry,
                                                     RepositoryDataManager repoData,
                                                     GameInstance          inst,
                                                     ModuleLabelList       allLabels,
                                                     NetModuleCache        cache,
                                                     GUIConfiguration?     config)
            => GetGUIMods(registry, repoData, inst,
                          registry.InstalledModules.Select(im => im.identifier)
                                                   .ToHashSet(),
                          allLabels,
                          cache,
                          config?.HideEpochs ?? false, config?.HideV ?? false);

        private static IEnumerable<GUIMod> GetGUIMods(IRegistryQuerier      registry,
                                                      RepositoryDataManager repoData,
                                                      GameInstance          inst,
                                                      HashSet<string>       installedIdents,
                                                      ModuleLabelList       allLabels,
                                                      NetModuleCache        cache,
                                                      bool                  hideEpochs,
                                                      bool                  hideV)
            => registry.CheckUpgradeable(inst,
                                         allLabels.HeldIdentifiers(inst)
                                                  .ToHashSet(),
                                         allLabels.IgnoreMissingIdentifiers(inst)
                                                  .ToHashSet())
                       .SelectMany(kvp => kvp.Value
                                             .Select(mod => registry.IsAutodetected(mod.identifier)
                                                            ? new GUIMod(mod, repoData, registry,
                                                                         inst.StabilityToleranceConfig,
                                                                         inst, cache, null,
                                                                         hideEpochs, hideV)
                                                              {
                                                                  HasUpdate = kvp.Key,
                                                              }
                                                            : registry.InstalledModule(mod.identifier)
                                                              is InstalledModule found
                                                                ? new GUIMod(found,
                                                                         repoData, registry,
                                                                         inst.StabilityToleranceConfig,
                                                                         inst, cache, null,
                                                                         hideEpochs, hideV)
                                                              {
                                                                  HasUpdate = kvp.Key,
                                                              }
                                                              : null))
                       .OfType<GUIMod>()
                       .Concat(registry.CompatibleModules(inst.StabilityToleranceConfig, inst.VersionCriteria())
                                       .Where(m => !installedIdents.Contains(m.identifier))
                                       .AsParallel()
                                       .Where(m => !m.IsDLC)
                                       .Select(m => new GUIMod(m, repoData, registry,
                                                               inst.StabilityToleranceConfig,
                                                               inst, cache, null,
                                                               hideEpochs, hideV)))
                       .Concat(registry.IncompatibleModules(inst.StabilityToleranceConfig, inst.VersionCriteria())
                                       .Where(m => !installedIdents.Contains(m.identifier))
                                       .AsParallel()
                                       .Where(m => !m.IsDLC)
                                       .Select(m => new GUIMod(m, repoData, registry,
                                                               inst.StabilityToleranceConfig,
                                                               inst, cache, true,
                                                               hideEpochs, hideV)));

        private void CheckRowUpgradeable(GameInstance              inst,
                                         List<ModChange>?          ChangeSet,
                                         DataGridViewRowCollection rows,
                                         string                    ident,
                                         bool                      upgradeable)
        {
            if (full_list_of_mod_rows.TryGetValue(ident, out DataGridViewRow? row)
                && row.Tag is GUIMod gmod
                && gmod.HasUpdate != upgradeable)
            {
                gmod.HasUpdate = upgradeable;
                if (row.Visible)
                {
                    // Swap whether the row has an upgrade checkbox
                    var newRow = full_list_of_mod_rows[ident] = MakeRow(gmod, ChangeSet, inst);
                    var rowIndex = row.Index;
                    var selected = row.Selected;
                    rows.Remove(row);
                    rows.Insert(rowIndex, newRow);
                    if (selected)
                    {
                        rows[rowIndex].Selected = true;
                    }
                }
            }
        }

        private static Color SelectionBlend(Color c)
            => c == Color.Empty
                ? SystemColors.Highlight
                : SystemColors.Highlight.AlphaBlendWith(selectionAlpha, c);

        private const float selectionAlpha = 0.4f;

        private static IEnumerable<ModChange> rowChanges(IRegistryQuerier    registry,
                                                         DataGridViewRow     row,
                                                         DataGridViewColumn? upgradeCol,
                                                         DataGridViewColumn? replaceCol)
            => row.Tag is GUIMod gmod ? gmod.GetModChanges(
                   upgradeCol != null && upgradeCol.Visible
                   && row.Cells[upgradeCol.Index] is DataGridViewCheckBoxCell upgradeCell
                   && (bool)upgradeCell.Value,
                   replaceCol != null && replaceCol.Visible
                   && row.Cells[replaceCol.Index] is DataGridViewCheckBoxCell replaceCell
                   && (bool)replaceCell.Value,
                   registry.MetadataChanged(gmod.Identifier))
               : Enumerable.Empty<ModChange>();

        private DataGridViewRow MakeRow(GUIMod           mod,
                                        List<ModChange>? changes,
                                        GameInstance     instance)
        {
            DataGridViewRow item = new DataGridViewRow() { Tag = mod };

            item.DefaultCellStyle.BackColor = GetRowBackground(mod, false, instance);
            item.DefaultCellStyle.ForeColor = item.DefaultCellStyle.BackColor.ForeColorForBackColor()
                                              ?? SystemColors.WindowText;
            item.DefaultCellStyle.SelectionBackColor = SelectionBlend(item.DefaultCellStyle.BackColor);
            item.DefaultCellStyle.SelectionForeColor = item.DefaultCellStyle.SelectionBackColor.ForeColorForBackColor()
                                                       ?? SystemColors.HighlightText;

            var myChange = changes?.FindLast(ch => ch.Mod.Equals(mod));

            var selecting = mod.IsAutodetected
                ? new DataGridViewTextBoxCell()
                {
                    Value = Properties.Resources.MainModListAutoDetected
                }
                : mod.IsInstallable()
                ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                {
                    Value = myChange == null
                        ? mod.IsInstalled
                        : myChange.ChangeType == GUIModChangeType.Install
                          || (myChange.ChangeType != GUIModChangeType.Remove && mod.IsInstalled)
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
                    Value = myChange?.ChangeType == GUIModChangeType.Update
                }
                : new DataGridViewTextBoxCell()
                {
                    Value = "-"
                };

            var replacing = (mod.IsInstalled && mod.HasReplacement)
                ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                {
                    Value = myChange?.ChangeType == GUIModChangeType.Replace
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

            var downloadCount = new DataGridViewTextBoxCell { Value = $"{mod.DownloadCount:N0}"   };
            var compat        = new DataGridViewTextBoxCell { Value = mod.GameCompatibility       };
            var downloadSize  = new DataGridViewTextBoxCell { Value = mod.DownloadSize            };
            var installSize   = new DataGridViewTextBoxCell { Value = mod.InstallSize             };
            var releaseDate   = new DataGridViewTextBoxCell { Value = mod.Module.release_date     };
            var installDate   = new DataGridViewTextBoxCell { Value = mod.InstallDate             };
            var desc          = new DataGridViewTextBoxCell { Value = ToGridText(mod.Abstract)    };

            item.Cells.AddRange(selecting, autoInstalled, updating, replacing, name, author, installVersion, latestVersion, compat, downloadSize, installSize, releaseDate, installDate, downloadCount, desc);

            selecting.ReadOnly     = selecting     is DataGridViewTextBoxCell;
            autoInstalled.ReadOnly = autoInstalled is DataGridViewTextBoxCell;
            updating.ReadOnly      = updating      is DataGridViewTextBoxCell;

            return item;
        }

        private static string ToGridText(string text)
            => Platform.IsMono ? text.Replace("&", "&&") : text;

        private bool TagInSearches(ModuleTag tag)
            => activeSearches.Any(s => s.TagNames.Contains(tag.Name));

        private bool LabelInSearches(ModuleLabel label)
            => activeSearches.Any(s => s.LabelNames.Contains(label.Name));

        private bool HiddenByTagsOrLabels(GUIMod m, GameInstance instance, Registry registry)
            // "Hide" labels apply to all non-custom filters
            => (allLabels?.LabelsFor(instance.Name)
                                             .Where(l => !LabelInSearches(l) && l.Hide)
                                             .Any(l => l.ContainsModule(instance.game, m.Identifier))
                                            ?? false)
               || (registry.Tags?.Values
                                 .Where(t => !TagInSearches(t) && ModuleTagList.ModuleTags.HiddenTags.Contains(t.Name))
                                 .Any(t => t.ModuleIdentifiers.Contains(m.Identifier))
                                ?? false);

        private static RelationshipResolverOptions conflictOptions(StabilityToleranceConfig stabilityTolerance)
            => new RelationshipResolverOptions(stabilityTolerance)
            {
                without_toomanyprovides_kraken = true,
                proceed_with_inconsistencies   = true,
                without_enforce_consistency    = true,
                with_recommends                = false
            };

        /// <summary>
        /// Get the InstalledModules that we'll have after the changeset,
        /// not including dependencies
        /// </summary>
        /// <param name="registry">Registry with currently installed modules</param>
        /// <param name="changeSet">Changes to be made to the installed modules</param>
        private static IEnumerable<InstalledModule> InstalledAfterChanges(
            IRegistryQuerier               registry,
            IReadOnlyCollection<ModChange> changeSet)
        {
            var removingIdents = changeSet
                .Where(ch => ch.ChangeType != GUIModChangeType.Install)
                .Select(ch => ch.Mod.identifier)
                .ToHashSet();
            return registry.InstalledModules
                .Where(im => !removingIdents.Contains(im.identifier))
                .Concat(changeSet
                    .Where(ch => ch.ChangeType is not GUIModChangeType.Remove
                                              and not GUIModChangeType.Replace)
                    .Select(ch => new InstalledModule(
                        null,
                        (ch as ModUpgrade)?.targetMod ?? ch.Mod,
                        Enumerable.Empty<string>(),
                        false)));
        }

        private static bool SearchesEqual(IReadOnlyCollection<ModSearch> a,
                                          IReadOnlyCollection<ModSearch> b)
            => a.SequenceEqual(b);

        private static string FilterName(GUIModFilter filter,
                                         ModuleTag?   tag   = null,
                                         ModuleLabel? label = null)
            => filter switch
               {
                   GUIModFilter.Compatible               => Properties.Resources.MainFilterCompatible,
                   GUIModFilter.Incompatible             => Properties.Resources.MainFilterIncompatible,
                   GUIModFilter.Installed                => Properties.Resources.MainFilterInstalled,
                   GUIModFilter.NotInstalled             => Properties.Resources.MainFilterNotInstalled,
                   GUIModFilter.InstalledUpdateAvailable => Properties.Resources.MainFilterUpgradeable,
                   GUIModFilter.Replaceable              => Properties.Resources.MainFilterReplaceable,
                   GUIModFilter.Cached                   => Properties.Resources.MainFilterCached,
                   GUIModFilter.Uncached                 => Properties.Resources.MainFilterUncached,
                   GUIModFilter.NewInRepository          => Properties.Resources.MainFilterNew,
                   GUIModFilter.All                      => Properties.Resources.MainFilterAll,
                   GUIModFilter.CustomLabel              => string.Format(Properties.Resources.MainFilterLabel,
                                                                          label?.Name ?? "CUSTOM"),
                   GUIModFilter.Tag                      => tag == null
                                                                ? Properties.Resources.MainFilterUntagged
                                                                : string.Format(Properties.Resources.MainFilterTag,
                                                                                tag.Name),
                   _                                     => "",
               };

        private readonly ModuleLabelList                allLabels;
        private readonly IConfiguration                 coreConfig;
        private readonly GUIConfiguration               guiConfig;
        private          IReadOnlyCollection<ModSearch> activeSearches;

        private static readonly Color conflictColor = Color.FromArgb(255, 64, 64);

        private static readonly ILog log = LogManager.GetLogger(typeof(ModList));
    }
}
