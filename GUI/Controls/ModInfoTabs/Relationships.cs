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

using CKAN.Versioning;
using CKAN.Extensions;
using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    public enum RelationshipType
    {
        [Display(Description  = "RelationshipTypeProvides",
                 ResourceType = typeof(Properties.Resources))]
        Provides   = 0,

        [Display(Description  = "RelationshipTypeDepends",
                 ResourceType = typeof(Properties.Resources))]
        Depends    = 1,

        [Display(Description  = "RelationshipTypeRecommends",
                 ResourceType = typeof(Properties.Resources))]
        Recommends = 2,

        [Display(Description  = "RelationshipTypeSuggests",
                 ResourceType = typeof(Properties.Resources))]
        Suggests   = 3,

        [Display(Description  = "RelationshipTypeSupports",
                 ResourceType = typeof(Properties.Resources))]
        Supports   = 4,

        [Display(Description  = "RelationshipTypeConflicts",
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

        public GUIMod SelectedModule
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
                    UpdateModDependencyGraph(selectedModule.ToModule());
                }
            }
            get => selectedModule;
        }

        public event Action<CkanModule> ModuleDoubleClicked;

        private void UpdateModDependencyGraph(CkanModule module)
        {
            Util.Invoke(DependsGraphTree, () => _UpdateModDependencyGraph(module));
        }

        private GUIMod                selectedModule;
        private GameInstanceManager   manager => Main.Instance.Manager;
        private readonly RepositoryDataManager repoData;

        private void DependsGraphTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is CkanModule module)
            {
                ModuleDoubleClicked?.Invoke(module);
            }
        }

        private bool ImMyOwnGrandpa(TreeNode node)
        {
            if (node.Tag is CkanModule module)
            {
                for (TreeNode other = node.Parent; other != null; other = other.Parent)
                {
                    if (module == other.Tag)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ReverseRelationshipsCheckbox_Click(object sender, EventArgs e)
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

        private void ReverseRelationshipsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateModDependencyGraph(SelectedModule.ToModule());
        }

        private void _UpdateModDependencyGraph(CkanModule module)
        {
            DependsGraphTree.BeginUpdate();
            DependsGraphTree.BackColor = SystemColors.Window;
            DependsGraphTree.LineColor = SystemColors.WindowText;
            DependsGraphTree.Nodes.Clear();
            IRegistryQuerier registry = RegistryManager.Instance(manager.CurrentInstance, repoData).registry;
            TreeNode root = new TreeNode($"{module.name} {module.version}", 0, 0)
            {
                Name = module.identifier,
                Tag  = module
            };
            DependsGraphTree.Nodes.Add(root);
            AddChildren(registry, root);
            DependsGraphTree.EndUpdate();
            root.Expand();
        }

        private void BeforeExpand(object sender, TreeViewCancelEventArgs args)
        {
            IRegistryQuerier registry      = RegistryManager.Instance(manager.CurrentInstance, repoData).registry;
            TreeNode         node          = args.Node;
            const int        modsPerUpdate = 10;

            // Load in groups to reduce flickering
            UseWaitCursor = true;
            int lastStart = Math.Max(0, node.Nodes.Count - modsPerUpdate);
            for (int start = 0; start <= lastStart; start += modsPerUpdate)
            {
                // Copy start's value to a variable that won't change as we loop
                int threadStart = start;
                int nodesLeft   = node.Nodes.Count - start;
                Task.Factory.StartNew(() =>
                    ExpandOnePage(
                        registry, node, threadStart,
                        // If next page is small (last), add it to this one,
                        // so the final page will be slower rather than faster
                        nodesLeft >= 2 * modsPerUpdate ? modsPerUpdate : nodesLeft));
            }
        }

        [ForbidGUICalls]
        private void ExpandOnePage(IRegistryQuerier registry, TreeNode parent, int start, int length)
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
                    GetChildren(registry, child).ToArray()))
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
            RelationshipType.Conflicts
        };

        private void AddChildren(IRegistryQuerier registry, TreeNode node)
        {
            var nodes = GetChildren(registry, node).ToArray();
            Util.Invoke(this, () => node.Nodes.AddRange(nodes));
        }

        // Load one layer of grandchildren on demand
        private IEnumerable<TreeNode> GetChildren(IRegistryQuerier registry, TreeNode node)
        {
            var crit = manager.CurrentInstance.VersionCriteria();
            // Skip children of nodes from circular dependencies
            // Tag is null for non-indexed nodes
            return !ImMyOwnGrandpa(node) && node.Tag is CkanModule module
                ? ReverseRelationshipsCheckbox.CheckState == CheckState.Unchecked
                    ? ForwardRelationships(registry, module, crit)
                    : ReverseRelationships(registry, module, crit)
                : Enumerable.Empty<TreeNode>();
        }

        private IEnumerable<RelationshipDescriptor> GetModRelationships(CkanModule module, RelationshipType which)
        {
            switch (which)
            {
                case RelationshipType.Depends:
                    return module.depends
                        ?? Enumerable.Empty<RelationshipDescriptor>();
                case RelationshipType.Recommends:
                    return module.recommends
                        ?? Enumerable.Empty<RelationshipDescriptor>();
                case RelationshipType.Suggests:
                    return module.suggests
                        ?? Enumerable.Empty<RelationshipDescriptor>();
                case RelationshipType.Supports:
                    return module.supports
                        ?? Enumerable.Empty<RelationshipDescriptor>();
                case RelationshipType.Conflicts:
                    return module.conflicts
                        ?? Enumerable.Empty<RelationshipDescriptor>();
            }
            return Enumerable.Empty<RelationshipDescriptor>();
        }

        private IEnumerable<TreeNode> ForwardRelationships(IRegistryQuerier registry, CkanModule module, GameVersionCriteria crit)
            => (module.provides?.Select(p => providedNode(p))
                    ?? Enumerable.Empty<TreeNode>())
                .Concat(kindsOfRelationships.SelectMany(relationship =>
                    GetModRelationships(module, relationship).Select(dependency =>
                        // Look for compatible mods
                        findDependencyShallow(registry, dependency, relationship, crit)
                        // Then incompatible mods
                        ?? findDependencyShallow(registry, dependency, relationship, null)
                        // Then give up and note the name without a module
                        ?? nonindexedNode(dependency, relationship))));

        private TreeNode findDependencyShallow(IRegistryQuerier registry, RelationshipDescriptor relDescr, RelationshipType relationship, GameVersionCriteria crit)
        {
            // Check if this dependency is installed
            if (relDescr.MatchesAny(registry.InstalledModules.Select(im => im.Module).ToList(),
                                    registry.InstalledDlls.ToHashSet(),
                                    // Maybe it's a DLC?
                                    registry.InstalledDlc,
                                    out CkanModule matched))
            {
                return matched != null
                    ? indexedNode(registry, matched, relationship, relDescr, crit)
                    : nonModuleNode(relDescr, null, relationship);
            }

            // Find modules that satisfy this dependency
            List<CkanModule> dependencyModules = relDescr.LatestAvailableWithProvides(
                registry, crit,
                // Ignore conflicts with installed mods
                new List<CkanModule>());
            if (dependencyModules.Count == 0)
            {
                // Nothing found, don't return a node
                return null;
            }
            else if (dependencyModules.Count == 1
                && relDescr.ContainsAny(new string[] { dependencyModules[0].identifier }))
            {
                // Only one exact match module, return a simple node
                return indexedNode(registry, dependencyModules[0], relationship, relDescr, crit);
            }
            else
            {
                // Several found or not same id, return a "provides" node
                return providesNode(relDescr.ToString(),
                                    relationship,
                                    dependencyModules.Select(dep => indexedNode(
                                        registry, dep, relationship, relDescr, crit)));
            }
        }

        private IEnumerable<TreeNode> ReverseRelationships(IRegistryQuerier registry, CkanModule module, GameVersionCriteria crit)
        {
            var compat   = registry.CompatibleModules(crit).ToArray();
            var incompat = registry.IncompatibleModules(crit).ToArray();
            var toFind   = new CkanModule[] { module };
            return kindsOfRelationships.SelectMany(relationship =>
                compat.SelectMany(otherMod =>
                    GetModRelationships(otherMod, relationship)
                        .Where(r => r.MatchesAny(toFind, null, null))
                        .Select(r => indexedNode(registry, otherMod, relationship, r, crit)))
                .Concat(incompat.SelectMany(otherMod =>
                    GetModRelationships(otherMod, relationship)
                        .Where(r => r.MatchesAny(toFind, null, null))
                        .Select(r => indexedNode(registry, otherMod, relationship, r, crit)))));
        }

        private TreeNode providesNode(string identifier, RelationshipType relationship, IEnumerable<TreeNode> children)
        {
            int icon = (int)relationship + 1;
            return new TreeNode(string.Format(Properties.Resources.ModInfoVirtual, identifier), icon, icon, children.ToArray())
            {
                Name        = identifier,
                ToolTipText = relationship.Localize(),
                ForeColor   = SystemColors.GrayText,
            };
        }

        private TreeNode indexedNode(IRegistryQuerier registry, CkanModule module, RelationshipType relationship, RelationshipDescriptor relDescr, GameVersionCriteria crit)
        {
            int icon = (int)relationship + 1;
            bool missingDLC = module.IsDLC && !registry.InstalledDlc.ContainsKey(module.identifier);
            bool compatible = crit != null && registry.IdentifierCompatible(module.identifier, crit);
            string suffix = compatible ? ""
                : $" ({registry.CompatibleGameVersions(manager.CurrentInstance.game, module.identifier)})";
            return new TreeNode($"{module.name} {module.version}{suffix}", icon, icon)
            {
                Name        = module.identifier,
                ToolTipText = $"{relationship.Localize()} {relDescr}",
                Tag         = module,
                ForeColor   = (compatible && !missingDLC)
                    ? SystemColors.WindowText
                    : Color.Red,
            };
        }

        private TreeNode nonModuleNode(RelationshipDescriptor relDescr, ModuleVersion version, RelationshipType relationship)
        {
            int icon = (int)relationship + 1;
            return new TreeNode($"{relDescr} {version}", icon, icon)
            {
                Name        = relDescr.ToString(),
                ToolTipText = relationship.Localize()
            };
        }

        private TreeNode nonindexedNode(RelationshipDescriptor relDescr, RelationshipType relationship)
        {
            // Completely nonexistent dependency, e.g. "AJE"
            int icon = (int)relationship + 1;
            return new TreeNode(string.Format(Properties.Resources.ModInfoNotIndexed, relDescr.ToString()), icon, icon)
            {
                Name        = relDescr.ToString(),
                ToolTipText = relationship.Localize(),
                ForeColor   = Color.Red
            };
        }

        private TreeNode providedNode(string identifier)
            => new TreeNode(identifier, 1, 1)
            {
                Name        = identifier,
                ToolTipText = $"{RelationshipType.Provides.Localize()} {identifier}",
            };

    }
}
