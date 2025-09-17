using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;

using CKAN.Configuration;
using CKAN.Versioning;
using CKAN.Extensions;
using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    public enum RelationshipType
    {
        [Display(Name         = "RelationshipTypeProvides",
                 Description  = "RelationshipTypeProvides",
                 ResourceType = typeof(Properties.Resources))]
        Provides   = 0,

        [Display(Name         = "RelationshipTypeDepends",
                 Description  = "RelationshipTypeDepends",
                 ResourceType = typeof(Properties.Resources))]
        Depends    = 1,

        [Display(Name         = "RelationshipTypeRecommends",
                 Description  = "RelationshipTypeRecommends",
                 ResourceType = typeof(Properties.Resources))]
        Recommends = 2,

        [Display(Name         = "RelationshipTypeSuggests",
                 Description  = "RelationshipTypeSuggests",
                 ResourceType = typeof(Properties.Resources))]
        Suggests   = 3,

        [Display(Name         = "RelationshipTypeSupports",
                 Description  = "RelationshipTypeSupports",
                 ResourceType = typeof(Properties.Resources))]
        Supports   = 4,

        [Display(Name         = "RelationshipTypeConflicts",
                 Description  = "RelationshipTypeConflicts",
                 ResourceType = typeof(Properties.Resources))]
        Conflicts  = 5,
    }

    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class Relationships : UserControl
    {
        public Relationships()
        {
            InitializeComponent();
            repoData = ServiceLocator.Container.Resolve<RepositoryDataManager>();

            ToolTip.SetToolTip(ReverseRelationshipsCheckbox, Properties.Resources.ModInfoToolTipReverseRelationships);

            DependsGraphTree.BeforeExpand += BeforeExpand;
        }

        public GUIMod? SelectedModule
        {
            set
            {
                if (value != selectedModule)
                {
                    if (ReverseRelationshipsCheckbox.CheckState == CheckState.Checked)
                    {
                        ReverseRelationshipsCheckbox.CheckState = CheckState.Unchecked;
                    }
                    selectedModule = value;
                    UpdateModDependencyGraph(selectedModule?.Module);
                }
            }
            get => selectedModule;
        }

        public event Action<CkanModule>? ModuleDoubleClicked;

        private void UpdateModDependencyGraph(CkanModule? module)
        {
            if (module != null)
            {
                Util.Invoke(DependsGraphTree, () => _UpdateModDependencyGraph(module));
            }
        }

        private GUIMod?                        selectedModule;
        private static GameInstanceManager?    Manager => Main.Instance?.Manager;
        private readonly RepositoryDataManager repoData;

        private void DependsGraphTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is CkanModule module)
            {
                ModuleDoubleClicked?.Invoke(module);
            }
        }

        private static bool ImMyOwnGrandpa(TreeNode node)
            => node.Tag is CkanModule module
               && (node.Parent?.TraverseNodes(nd => nd.Parent)
                               .Any(other => other.Tag == module)
                              ?? false);

        private void ReverseRelationshipsCheckbox_Click(object? sender, EventArgs? e)
        {
            ReverseRelationshipsCheckbox.CheckState =
                ReverseRelationshipsCheckbox.CheckState == CheckState.Unchecked
                    // If user holds ctrl or shift, go to "sticky" indeterminate state,
                    // else normal checked
                    ? ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift)
                        ? CheckState.Indeterminate
                        : CheckState.Checked
                    : CheckState.Unchecked;
        }

        private void ReverseRelationshipsCheckbox_CheckedChanged(object? sender, EventArgs? e)
        {
            UpdateModDependencyGraph(SelectedModule?.Module);
        }

        private void _UpdateModDependencyGraph(CkanModule module)
        {
            if (Manager?.CurrentInstance != null)
            {
                DependsGraphTree.BeginUpdate();
                DependsGraphTree.BackColor = SystemColors.Window;
                DependsGraphTree.LineColor = SystemColors.WindowText;
                DependsGraphTree.Nodes.Clear();
                IRegistryQuerier registry = RegistryManager.Instance(Manager.CurrentInstance, repoData).registry;
                TreeNode root = new TreeNode($"{module.name} {module.version}", 0, 0)
                {
                    Name = module.identifier,
                    Tag  = module
                };
                DependsGraphTree.Nodes.Add(root);
                AddChildren(registry, Manager.CurrentInstance.StabilityToleranceConfig, root);
                root.Expand();
                // Expand virtual depends nodes
                foreach (var node in root.Nodes.OfType<TreeNode>()
                                               .Where(nd => nd.Nodes.Count > 0
                                                            && nd.ImageIndex == (int)RelationshipType.Depends + 1))
                {
                    node.Expand();
                }
                DependsGraphTree.EndUpdate();
            }
        }

        private void BeforeExpand(object? sender, TreeViewCancelEventArgs? args)
        {
            if (Manager?.CurrentInstance != null && args?.Node is TreeNode node)
            {
                IRegistryQuerier registry      = RegistryManager.Instance(Manager.CurrentInstance, repoData).registry;
                const int        modsPerUpdate = 10;

                // Load in groups to reduce flickering
                UseWaitCursor = true;
                int lastStart = Math.Max(0, node.Nodes.Count - modsPerUpdate);
                for (int start = 0; start <= lastStart; start += modsPerUpdate)
                {
                    // Copy start's value to a variable that won't change as we loop
                    int threadStart = start;
                    int nodesLeft   = node.Nodes.Count - start;
                    Task.Run(() =>
                        ExpandOnePage(
                            registry, Manager.CurrentInstance.StabilityToleranceConfig,
                            node, threadStart,
                            // If next page is small (last), add it to this one,
                            // so the final page will be slower rather than faster
                            nodesLeft >= 2 * modsPerUpdate ? modsPerUpdate : nodesLeft));
                }
            }
        }

        [ForbidGUICalls]
        private void ExpandOnePage(IRegistryQuerier         registry,
                                   StabilityToleranceConfig stabilityTolerance,
                                   TreeNode                 parent,
                                   int                      start,
                                   int                      length)
        {
            // Should already have children, since the user is expanding it
            var nodesAndChildren = parent.Nodes.Cast<TreeNode>()
                .Skip(start)
                .Take(length)
                // If there are grandchildren, then this child has been loaded before
                .Where(child => child.Nodes.Count == 0
                    // If user switched to another mod, stop loading
                    && child.TreeView != null)
                .Select(child => new KeyValuePair<TreeNode, TreeNode[]>(
                    child,
                    GetChildren(registry, stabilityTolerance, child).ToArray()))
                .ToArray();
            // If user switched to another mod, stop loading
            if (parent.TreeView != null)
            {
                // Refresh the UI
                Util.Invoke(this, () =>
                {
                    if (nodesAndChildren.Length > 0)
                    {
                        DependsGraphTree.BeginUpdate();
                        foreach (var kvp in nodesAndChildren)
                        {
                            kvp.Key.Nodes.AddRange(kvp.Value);
                        }
                        DependsGraphTree.EndUpdate();
                    }
                    if (start + length >= parent.Nodes.Count)
                    {
                        // Reset the cursor when the final group finishes
                        UseWaitCursor = false;
                    }
                });
            }
        }

        private static readonly RelationshipType[] kindsOfRelationships = new RelationshipType[]
        {
            RelationshipType.Depends,
            RelationshipType.Recommends,
            RelationshipType.Suggests,
            RelationshipType.Supports,
            RelationshipType.Conflicts,
        };

        private void AddChildren(IRegistryQuerier         registry,
                                 StabilityToleranceConfig stabilityTolerance,
                                 TreeNode                 node)
        {
            var nodes = GetChildren(registry, stabilityTolerance, node).ToArray();
            Util.Invoke(this, () => node.Nodes.AddRange(nodes));
        }

        // Load one layer of grandchildren on demand
        private IEnumerable<TreeNode> GetChildren(IRegistryQuerier         registry,
                                                  StabilityToleranceConfig stabilityTolerance,
                                                  TreeNode                 node)
            // Skip children of nodes from circular dependencies
            // Tag is null for non-indexed nodes
            => !ImMyOwnGrandpa(node)
               && node.Tag is CkanModule module
               && Manager?.CurrentInstance?.VersionCriteria() is GameVersionCriteria crit
                   ? ReverseRelationshipsCheckbox.CheckState == CheckState.Unchecked
                       ? ForwardRelationships(registry, module, stabilityTolerance, crit)
                       : ReverseRelationships(registry, module, stabilityTolerance, crit)
                   : Enumerable.Empty<TreeNode>();

        private static IEnumerable<RelationshipDescriptor> GetModRelationships(CkanModule       module,
                                                                               RelationshipType which)
            => which switch {
                RelationshipType.Depends       => module.depends
                                                  ?? Enumerable.Empty<RelationshipDescriptor>(),
                RelationshipType.Recommends    => module.recommends
                                                  ?? Enumerable.Empty<RelationshipDescriptor>(),
                RelationshipType.Suggests      => module.suggests
                                                  ?? Enumerable.Empty<RelationshipDescriptor>(),
                RelationshipType.Supports      => module.supports
                                                  ?? Enumerable.Empty<RelationshipDescriptor>(),
                RelationshipType.Conflicts     => module.conflicts
                                                  ?? Enumerable.Empty<RelationshipDescriptor>(),
                RelationshipType.Provides or _ => Enumerable.Empty<RelationshipDescriptor>(),
            };

        private IEnumerable<TreeNode> ForwardRelationships(IRegistryQuerier         registry,
                                                           CkanModule               module,
                                                           StabilityToleranceConfig stabilityTolerance,
                                                           GameVersionCriteria      crit)
            => (module.provides?.Select(ProvidedNode)
                    ?? Enumerable.Empty<TreeNode>())
                .Concat(kindsOfRelationships.SelectMany(relationship =>
                    GetModRelationships(module, relationship).Select(dependency =>
                        // Look for compatible mods
                        FindDependencyShallow(registry, dependency, relationship, stabilityTolerance, crit)
                        // Then incompatible mods
                        ?? FindDependencyShallow(registry, dependency, relationship, stabilityTolerance, null)
                        // Then give up and note the name without a module
                        ?? NonindexedNode(dependency, relationship))));

        private TreeNode? FindDependencyShallow(IRegistryQuerier         registry,
                                                RelationshipDescriptor   relDescr,
                                                RelationshipType         relationship,
                                                StabilityToleranceConfig stabilityTolerance,
                                                GameVersionCriteria?     crit)
        {
            var childNodes = relDescr.LatestAvailableWithProvides(
                                          registry, stabilityTolerance, crit,
                                          // Ignore conflicts with installed mods
                                          new List<CkanModule>())
                                     .Select(dep => IndexedNode(registry, dep, relationship, relDescr, stabilityTolerance, crit))
                                     .ToList();

            // Check if this dependency is installed
            if (relDescr.MatchesAny(registry.InstalledModules.Select(im => im.Module).ToList(),
                                    registry.InstalledDlls,
                                    // Maybe it's a DLC?
                                    registry.InstalledDlc,
                                    out CkanModule? matched))
            {
                if (matched == null)
                {
                    childNodes.Add(NonModuleNode(relDescr, null, relationship));
                }
                else
                {
                    var newNode = IndexedNode(registry, matched, relationship, relDescr, stabilityTolerance, crit);
                    if (childNodes.FindIndex(nd => (nd.Tag as CkanModule)?.identifier == matched.identifier)
                        is int index && index != -1)
                    {
                        // Replace the latest provider with the installed version
                        childNodes[index] = newNode;
                    }
                    else
                    {
                        childNodes.Add(newNode);
                    }
                }
            }

            if (childNodes.Count == 0)
            {
                // Nothing found, don't return a node
                return null;
            }
            else if (//childNodes is [var node and {Tag: CkanModule module}]
                     childNodes.Count == 1
                     && childNodes[0] is var node and {Tag: CkanModule module}
                     && relDescr.ContainsAny(new string[] { module.identifier }))
            {
                // Only one exact match module, return a simple node
                return node;
            }
            else
            {
                // Several found or not same id, return a "provides" node
                return providesNode(relDescr.ToString() ?? "", relationship,
                                    childNodes.ToArray());
            }
        }

        private IEnumerable<TreeNode> ReverseRelationships(IRegistryQuerier         registry,
                                                           CkanModule               module,
                                                           StabilityToleranceConfig stabilityTolerance,
                                                           GameVersionCriteria      crit)
            => ReverseRelationships(registry, new CkanModule[] { module },
                                    stabilityTolerance, crit);

        private IEnumerable<TreeNode> ReverseRelationships(IRegistryQuerier         registry,
                                                           CkanModule[]             modules,
                                                           StabilityToleranceConfig stabilityTolerance,
                                                           GameVersionCriteria      crit)
            => kindsOfRelationships.SelectMany(relationship =>
                registry.CompatibleModules(stabilityTolerance, crit)
                        .SelectMany(otherMod =>
                            GetModRelationships(otherMod, relationship)
                                .Where(r => r.MatchesAny(modules, null, null))
                                .Select(r => IndexedNode(registry, otherMod, relationship,
                                                         r, stabilityTolerance, crit)))
                        .OrderByDescending(r => registry.IsInstalled(r.Name, false))
                        .ThenBy(r => r.Name)
                        .Concat(registry.IncompatibleModules(stabilityTolerance, crit)
                                        .SelectMany(otherMod =>
                                            GetModRelationships(otherMod, relationship)
                                                .Where(r => r.MatchesAny(modules, null, null))
                                                .Select(r => IndexedNode(registry, otherMod, relationship,
                                                                         r, stabilityTolerance, crit)))
                                        .OrderByDescending(r => registry.IsInstalled(r.Name, false))
                                        .ThenBy(r => r.Name)));

        private static TreeNode providesNode(string           identifier,
                                             RelationshipType relationship,
                                             TreeNode[]       children)
        {
            int icon = (int)relationship + 1;
            return new TreeNode(string.Format(Properties.Resources.ModInfoVirtual,
                                              identifier),
                                icon, icon, children)
            {
                Name        = identifier,
                ToolTipText = $"{relationship.LocalizeDescription()} {identifier}",
                ForeColor   = SystemColors.GrayText,
            };
        }

        private TreeNode IndexedNode(IRegistryQuerier         registry,
                                     CkanModule               module,
                                     RelationshipType         relationship,
                                     RelationshipDescriptor   relDescr,
                                     StabilityToleranceConfig stabilityTolerance,
                                     GameVersionCriteria?     crit)
        {
            int icon = (int)relationship + 1;
            bool missingDLC = module.IsDLC && !registry.InstalledDlc.ContainsKey(module.identifier);
            bool compatible = crit != null && registry.IdentifierCompatible(module.identifier, stabilityTolerance, crit);
            string suffix = compatible || Manager?.CurrentInstance == null
                ? ""
                : $" ({registry.CompatibleGameVersions(Manager.CurrentInstance.game, module.identifier)})";
            return new TreeNode($"{module.name} {module.version}{suffix}", icon, icon)
            {
                Name        = module.identifier,
                ToolTipText = $"{relationship.LocalizeDescription()} {relDescr}",
                Tag         = module,
                ForeColor   = (compatible && !missingDLC)
                                  ? SystemColors.WindowText
                                  : Color.Red,
                NodeFont    = new Font(DependsGraphTree.Font,
                                       registry.IsInstalled(module.identifier, false)
                                           ? FontStyle.Bold
                                           : FontStyle.Regular),
            };
        }

        private TreeNode NonModuleNode(RelationshipDescriptor relDescr,
                                       ModuleVersion?         version,
                                       RelationshipType       relationship)
        {
            int icon = (int)relationship + 1;
            return new TreeNode($"{relDescr} {version}", icon, icon)
            {
                Name        = relDescr.ToString(),
                ToolTipText = relationship.LocalizeDescription(),
                NodeFont    = new Font(DependsGraphTree.Font,
                                       FontStyle.Bold),
            };
        }

        private static TreeNode NonindexedNode(RelationshipDescriptor relDescr,
                                               RelationshipType       relationship)
        {
            // Completely nonexistent dependency, e.g. "AJE"
            int icon = (int)relationship + 1;
            return new TreeNode(string.Format(Properties.Resources.ModInfoNotIndexed,
                                              relDescr.ToString()),
                                icon, icon)
            {
                Name        = relDescr.ToString(),
                ToolTipText = relationship.LocalizeDescription(),
                ForeColor   = Color.Red,
            };
        }

        private TreeNode ProvidedNode(string identifier)
            => new TreeNode(identifier, 1, 1)
            {
                Name        = identifier,
                ToolTipText = $"{RelationshipType.Provides.LocalizeDescription()} {identifier}",
            };

    }
}
