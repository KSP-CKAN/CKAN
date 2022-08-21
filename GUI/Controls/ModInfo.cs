using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;

using CKAN.Versioning;

namespace CKAN.GUI
{
    public enum RelationshipType
    {
        Depends    = 0,
        Recommends = 1,
        Suggests   = 2,
        Supports   = 3,
        Conflicts  = 4
    }

    public partial class ModInfo : UserControl
    {
        private GUIMod                    selectedModule;
        private CkanModule                currentModContentsModule;
        private readonly int staticRowCount;

        public ModInfo()
        {
            InitializeComponent();
            staticRowCount = MetaDataLowerLayoutPanel.RowCount;

            DependsGraphTree.BeforeExpand += BeforeExpand;
        }

        public GUIMod SelectedModule
        {
            set
            {
                var module = value?.ToModule();
                if (value != selectedModule)
                {
                    if (module == null)
                    {
                        ModInfoTabControl.Enabled = false;
                    }
                    else
                    {
                        ModInfoTabControl.Enabled = true;
                        if (ReverseRelationshipsCheckbox.CheckState == CheckState.Checked)
                        {
                            ReverseRelationshipsCheckbox.CheckState = CheckState.Unchecked;
                        }
                        LoadTab(ModInfoTabControl.SelectedTab.Name, value);
                    }
                    selectedModule = value;
                }
            }
            get
            {
                return selectedModule;
            }
        }

        private void LoadTab(string name, GUIMod gm)
        {
            switch (ModInfoTabControl.SelectedTab.Name)
            {
                case "MetadataTabPage":
                    UpdateModInfo(gm);
                    break;

                case "ContentTabPage":
                    UpdateModContentsTree(gm.ToModule());
                    break;

                case "RelationshipTabPage":
                    UpdateModDependencyGraph(gm.ToModule());
                    break;

                case "AllModVersionsTabPage":
                    AllModVersions.SelectedModule = gm;
                    if (Platform.IsMono)
                    {
                        // Workaround: make sure the ListView headers are drawn
                        AllModVersions.ForceRedraw();
                    }
                    break;
            }
        }

        // When switching tabs ensure that the resulting tab is updated.
        private void ModInfoTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTab(ModInfoTabControl.SelectedTab.Name, selectedModule);
        }

        public int ModMetaSplitPosition
        {
            get
            {
                return splitContainer2.SplitterDistance;
            }
            set
            {
                try
                {
                    this.splitContainer2.SplitterDistance = value;
                }
                catch
                {
                    // SplitContainer is mis-designed to throw exceptions
                    // if the min/max limits are exceeded rather than simply obeying them.
                }
            }
        }

        public event Action<GUIMod> OnDownloadClick;

        private GameInstanceManager manager
        {
            get
            {
                return Main.Instance.manager;
            }
        }

        private void DependsGraphTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Main.Instance.ManageMods.ResetFilterAndSelectModOnList(e.Node.Name);
        }

        private void ContentsPreviewTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenFileBrowser(e.Node);
        }

        private void ContentsDownloadButton_Click(object sender, EventArgs e)
        {
            if (OnDownloadClick != null)
            {
                OnDownloadClick(SelectedModule);
            }
        }

        private void ContentsOpenButton_Click(object sender, EventArgs e)
        {
            Utilities.ProcessStartURL(manager.Cache.GetCachedFilename(SelectedModule.ToModule()));
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.HandleLinkClicked((sender as LinkLabel).Text, e);
        }

        private void LinkLabel_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Apps:
                    Util.LinkContextMenu((sender as LinkLabel).Text);
                    e.Handled = true;
                    break;
            }
        }

        private void UpdateModInfo(GUIMod gui_module)
        {
            CkanModule module = gui_module.ToModule();

            Util.Invoke(MetadataModuleNameTextBox, () => MetadataModuleNameTextBox.Text = module.name);
            UpdateTagsAndLabels(module);
            Util.Invoke(MetadataModuleAbstractLabel, () => MetadataModuleAbstractLabel.Text = module.@abstract.Replace("&", "&&"));
            Util.Invoke(MetadataModuleDescriptionTextBox, () =>
            {
                MetadataModuleDescriptionTextBox.Text = module.description
                    ?.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
                MetadataModuleDescriptionTextBox.ScrollBars =
                    string.IsNullOrWhiteSpace(module.description)
                        ? ScrollBars.None
                        : ScrollBars.Vertical;
            });

            Util.Invoke(MetadataModuleVersionTextBox, () => MetadataModuleVersionTextBox.Text = gui_module.LatestVersion.ToString());
            Util.Invoke(MetadataModuleLicenseTextBox, () => MetadataModuleLicenseTextBox.Text = string.Join(", ", module.license));
            Util.Invoke(MetadataModuleAuthorTextBox, () => MetadataModuleAuthorTextBox.Text = gui_module.Authors);
            Util.Invoke(MetadataIdentifierTextBox, () => MetadataIdentifierTextBox.Text = module.identifier);

            Util.Invoke(MetadataModuleReleaseStatusTextBox, () =>
            {
                if (module.release_status == null)
                {
                    ReleaseLabel.Visible = false;
                    MetadataModuleReleaseStatusTextBox.Visible = false;
                    MetaDataLowerLayoutPanel.LayoutSettings.RowStyles[3].Height = 0;
                }
                else
                {
                    ReleaseLabel.Visible = true;
                    MetadataModuleReleaseStatusTextBox.Visible = true;
                    MetaDataLowerLayoutPanel.LayoutSettings.RowStyles[3].Height = 30;
                    MetadataModuleReleaseStatusTextBox.Text = module.release_status.ToString();
                }
            });
            Util.Invoke(MetadataModuleGameCompatibilityTextBox, () => MetadataModuleGameCompatibilityTextBox.Text = gui_module.GameCompatibilityLong);

            Util.Invoke(ModInfoTabControl, () =>
            {
                // Mono doesn't draw TabPage.ImageIndex, so fake it
                const string fakeStopSign = "<!> ";
                ComponentResourceManager resources = new SingleAssemblyComponentResourceManager(typeof(ModInfo));
                resources.ApplyResources(RelationshipTabPage,   "RelationshipTabPage");
                resources.ApplyResources(AllModVersionsTabPage, "AllModVersionsTabPage");
                if (gui_module.IsIncompatible)
                {
                    if (!module.IsCompatibleKSP(manager.CurrentInstance.VersionCriteria()))
                    {
                        AllModVersionsTabPage.Text = fakeStopSign + AllModVersionsTabPage.Text;
                    }
                    else
                    {
                        RelationshipTabPage.Text = fakeStopSign + RelationshipTabPage.Text;
                    }
                }
            });
            Util.Invoke(ReplacementTextBox, () =>
            {
                if (module.replaced_by == null)
                {
                    ReplacementLabel.Visible = false;
                    ReplacementTextBox.Visible = false;
                    MetaDataLowerLayoutPanel.LayoutSettings.RowStyles[6].Height = 0;
                }
                else
                {
                    ReplacementLabel.Visible = true;
                    ReplacementTextBox.Visible = true;
                    MetaDataLowerLayoutPanel.LayoutSettings.RowStyles[6].Height = 30;
                    ReplacementTextBox.Text = module.replaced_by.ToString();
                }
            });

            Util.Invoke(MetaDataLowerLayoutPanel, () =>
            {
                ClearResourceLinks();
                var res = module.resources;
                if (res != null)
                {
                    AddResourceLink(Properties.Resources.ModInfoHomepageLabel,              res.homepage);
                    AddResourceLink(Properties.Resources.ModInfoSpaceDockLabel,             res.spacedock);
                    AddResourceLink(Properties.Resources.ModInfoCurseLabel,                 res.curse);
                    AddResourceLink(Properties.Resources.ModInfoRepositoryLabel,            res.repository);
                    AddResourceLink(Properties.Resources.ModInfoBugTrackerLabel,            res.bugtracker);
                    AddResourceLink(Properties.Resources.ModInfoContinuousIntegrationLabel, res.ci);
                    AddResourceLink(Properties.Resources.ModInfoLicenseLabel,               res.license);
                    AddResourceLink(Properties.Resources.ModInfoManualLabel,                res.manual);
                    AddResourceLink(Properties.Resources.ModInfoMetanetkanLabel,            res.metanetkan);
                    AddResourceLink(Properties.Resources.ModInfoRemoteAvcLabel,             res.remoteAvc);
                    AddResourceLink(Properties.Resources.ModInfoStoreLabel,                 res.store);
                    AddResourceLink(Properties.Resources.ModInfoSteamStoreLabel,            res.steamstore);
                }
            });
        }

        private void ClearResourceLinks()
        {
            for (int row = MetaDataLowerLayoutPanel.RowCount - 1; row >= staticRowCount; --row)
            {
                RemovePanelControl(MetaDataLowerLayoutPanel, 0, row);
                RemovePanelControl(MetaDataLowerLayoutPanel, 1, row);
                MetaDataLowerLayoutPanel.RowStyles.RemoveAt(row);
            }
            MetaDataLowerLayoutPanel.RowCount = staticRowCount;
        }

        private static void RemovePanelControl(TableLayoutPanel panel, int col, int row)
        {
            var ctl = panel.GetControlFromPosition(col, row);
            if (ctl != null)
            {
                panel.Controls.Remove(ctl);
            }
        }

        private void AddResourceLink(string label, Uri link)
        {
            if (link != null)
            {
                Label lbl = new Label()
                {
                    AutoSize  = true,
                    Dock      = DockStyle.Fill,
                    ForeColor = SystemColors.GrayText,
                    Text      = label,
                };
                LinkLabel llbl = new LinkLabel()
                {
                    AutoSize = true,
                    TabStop  = true,
                    Text     = link.ToString(),
                };
                llbl.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabel_LinkClicked);
                llbl.KeyDown += new KeyEventHandler(LinkLabel_KeyDown);
                int row = MetaDataLowerLayoutPanel.RowCount;
                MetaDataLowerLayoutPanel.Controls.Add(lbl,  0, row);
                MetaDataLowerLayoutPanel.Controls.Add(llbl, 1, row);
                MetaDataLowerLayoutPanel.RowCount = row + 1;
                MetaDataLowerLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            }
        }

        private ModuleLabelList ModuleLabels
        {
            get
            {
                return Main.Instance.ManageMods.mainModList.ModuleLabels;
            }
        }

        private ModuleTagList ModuleTags
        {
            get
            {
                return Main.Instance.ManageMods.mainModList.ModuleTags;
            }
        }

        private void UpdateTagsAndLabels(CkanModule mod)
        {
            Util.Invoke(MetadataTagsLabelsPanel, () =>
            {
                MetadataTagsLabelsPanel.SuspendLayout();
                MetadataTagsLabelsPanel.Controls.Clear();
                var tags = ModuleTags?.Tags
                    .Where(t => t.Value.ModuleIdentifiers.Contains(mod.identifier))
                    .OrderBy(t => t.Key)
                    .Select(t => t.Value);
                if (tags != null)
                {
                    foreach (ModuleTag tag in tags)
                    {
                        MetadataTagsLabelsPanel.Controls.Add(TagLabelLink(
                            tag.Name, tag, new LinkLabelLinkClickedEventHandler(this.TagLinkLabel_LinkClicked)
                        ));
                    }
                }
                var labels = ModuleLabels?.LabelsFor(manager.CurrentInstance.Name)
                    .Where(l => l.ModuleIdentifiers.Contains(mod.identifier))
                    .OrderBy(l => l.Name);
                if (labels != null)
                {
                    foreach (ModuleLabel mlbl in labels)
                    {
                        MetadataTagsLabelsPanel.Controls.Add(TagLabelLink(
                            mlbl.Name, mlbl, new LinkLabelLinkClickedEventHandler(this.LabelLinkLabel_LinkClicked)
                        ));
                    }
                }
                MetadataTagsLabelsPanel.ResumeLayout();
            });
        }

        private LinkLabel TagLabelLink(string name, object tag, LinkLabelLinkClickedEventHandler onClick)
        {
            var link = new LinkLabel()
            {
                AutoSize     = true,
                LinkColor    = SystemColors.GrayText,
                LinkBehavior = LinkBehavior.HoverUnderline,
                Margin       = new Padding(2),
                Text         = name,
                Tag          = tag,
            };
            link.LinkClicked += onClick;
            return link;
        }

        public delegate void ChangeFilter(SavedSearch search);
        public event ChangeFilter OnChangeFilter;

        private void TagLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var link = sender as LinkLabel;
            OnChangeFilter?.Invoke(ModList.FilterToSavedSearch(GUIModFilter.Tag, link.Tag as ModuleTag, null));
        }

        private void LabelLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var link = sender as LinkLabel;
            OnChangeFilter?.Invoke(ModList.FilterToSavedSearch(GUIModFilter.CustomLabel, null, link.Tag as ModuleLabel));
        }

        private bool ImMyOwnGrandpa(TreeNode node)
        {
            CkanModule module = node.Tag as CkanModule;
            if (module != null)
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

        private void ReverseRelationshipsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateModDependencyGraph(null);
        }

        private void UpdateModDependencyGraph(CkanModule module)
        {
            ModInfoTabControl.Tag = module ?? ModInfoTabControl.Tag;
            // Can be costly. For now only update when visible.
            if (ModInfoTabControl.SelectedIndex != RelationshipTabPage.TabIndex)
            {
                return;
            }
            Util.Invoke(DependsGraphTree, () => _UpdateModDependencyGraph(ModInfoTabControl.Tag as CkanModule));
        }

        private void _UpdateModDependencyGraph(CkanModule module)
        {
            DependsGraphTree.BeginUpdate();
            DependsGraphTree.BackColor = SystemColors.Window;
            DependsGraphTree.LineColor = SystemColors.WindowText;
            DependsGraphTree.Nodes.Clear();
            IRegistryQuerier registry = RegistryManager.Instance(manager.CurrentInstance).registry;
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
            IRegistryQuerier registry      = RegistryManager.Instance(manager.CurrentInstance).registry;
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
            var module = node.Tag as CkanModule;
            var crit   = manager.CurrentInstance.VersionCriteria();
            // Skip children of nodes from circular dependencies
            // Tag is null for non-indexed nodes
            return ImMyOwnGrandpa(node) || module == null
                ? Enumerable.Empty<TreeNode>()
                : ReverseRelationshipsCheckbox.CheckState == CheckState.Unchecked
                    ? ForwardRelationships(registry, node, module, crit)
                    : ReverseRelationships(registry, node, module, crit);
        }

        private IEnumerable<RelationshipDescriptor> GetModRelationships(CkanModule module, RelationshipType which)
        {
            switch (which)
            {
                case RelationshipType.Depends:
                    return module.depends
                        ?? Enumerable.Empty<RelationshipDescriptor>();
                    break;
                case RelationshipType.Recommends:
                    return module.recommends
                        ?? Enumerable.Empty<RelationshipDescriptor>();
                    break;
                case RelationshipType.Suggests:
                    return module.suggests
                        ?? Enumerable.Empty<RelationshipDescriptor>();
                    break;
                case RelationshipType.Supports:
                    return module.supports
                        ?? Enumerable.Empty<RelationshipDescriptor>();
                    break;
                case RelationshipType.Conflicts:
                    return module.conflicts
                        ?? Enumerable.Empty<RelationshipDescriptor>();
                    break;
            }
            return Enumerable.Empty<RelationshipDescriptor>();
        }

        private IEnumerable<TreeNode> ForwardRelationships(IRegistryQuerier registry, TreeNode node, CkanModule module, GameVersionCriteria crit)
            => kindsOfRelationships.SelectMany(relationship =>
                GetModRelationships(module, relationship).Select(dependency =>
                    // Look for compatible mods
                    findDependencyShallow(registry, dependency, relationship, crit)
                    // Then incompatible mods
                    ?? findDependencyShallow(registry, dependency, relationship, null)
                    // Then give up and note the name without a module
                    ?? nonindexedNode(dependency, relationship)));

        private TreeNode findDependencyShallow(IRegistryQuerier registry, RelationshipDescriptor relDescr, RelationshipType relationship, GameVersionCriteria crit)
        {
            // Check if this dependency is installed
            if (relDescr.MatchesAny(
                registry.InstalledModules.Select(im => im.Module),
                new HashSet<string>(registry.InstalledDlls),
                // Maybe it's a DLC?
                registry.InstalledDlc,
                out CkanModule matched))
            {
                return matched != null
                    ? indexedNode(registry, matched, relationship, true)
                    : nonModuleNode(relDescr, null, relationship);
            }

            // Find modules that satisfy this dependency
            List<CkanModule> dependencyModules = relDescr.LatestAvailableWithProvides(
                registry, crit,
                // Ignore conflicts with installed mods
                Enumerable.Empty<CkanModule>());
            if (dependencyModules.Count == 0)
            {
                // Nothing found, don't return a node
                return null;
            }
            else if (dependencyModules.Count == 1
                && relDescr.ContainsAny(new string[] { dependencyModules[0].identifier }))
            {
                // Only one exact match module, return a simple node
                return indexedNode(registry, dependencyModules[0], relationship, crit != null);
            }
            else
            {
                // Several found or not same id, return a "provides" node
                return providesNode(relDescr.ToString(), relationship,
                    dependencyModules.Select(dep => indexedNode(registry, dep, relationship, crit != null))
                );
            }
        }

        private IEnumerable<TreeNode> ReverseRelationships(IRegistryQuerier registry, TreeNode node, CkanModule module, GameVersionCriteria crit)
        {
            var compat   = registry.CompatibleModules(crit).ToArray();
            var incompat = registry.IncompatibleModules(crit).ToArray();
            var toFind   = new CkanModule[] { module };
            return kindsOfRelationships.SelectMany(relationship =>
                compat.SelectMany(otherMod =>
                    GetModRelationships(otherMod, relationship)
                        .Where(r => r.MatchesAny(toFind, null, null))
                        .Select(r => indexedNode(registry, otherMod, relationship, true)))
                .Concat(incompat.SelectMany(otherMod =>
                    GetModRelationships(otherMod, relationship)
                        .Where(r => r.MatchesAny(toFind, null, null))
                        .Select(r => indexedNode(registry, otherMod, relationship, false)))));
        }

        private TreeNode providesNode(string identifier, RelationshipType relationship, IEnumerable<TreeNode> children)
        {
            int icon = (int)relationship + 1;
            return new TreeNode(string.Format(Properties.Resources.ModInfoVirtual, identifier), icon, icon, children.ToArray())
            {
                Name        = identifier,
                ToolTipText = relationship.ToString(),
                ForeColor   = SystemColors.GrayText,
            };
        }

        private TreeNode indexedNode(IRegistryQuerier registry, CkanModule module, RelationshipType relationship, bool compatible)
        {
            int icon = (int)relationship + 1;
            string suffix = compatible ? ""
                : $" ({registry.CompatibleGameVersions(manager.CurrentInstance.game, module.identifier)})";
            return new TreeNode($"{module.name} {module.version}{suffix}", icon, icon)
            {
                Name        = module.identifier,
                ToolTipText = relationship.ToString(),
                Tag         = module,
                ForeColor   = compatible ? SystemColors.WindowText : Color.Red,
            };
        }

        private TreeNode nonModuleNode(RelationshipDescriptor relDescr, ModuleVersion version, RelationshipType relationship)
        {
            int icon = (int)relationship + 1;
            return new TreeNode($"{relDescr} {version}", icon, icon)
            {
                Name        = relDescr.ToString(),
                ToolTipText = relationship.ToString()
            };
        }

        private TreeNode nonindexedNode(RelationshipDescriptor relDescr, RelationshipType relationship)
        {
            // Completely nonexistent dependency, e.g. "AJE"
            int icon = (int)relationship + 1;
            return new TreeNode(string.Format(Properties.Resources.ModInfoNotIndexed, relDescr.ToString()), icon, icon)
            {
                Name        = relDescr.ToString(),
                ToolTipText = relationship.ToString(),
                ForeColor   = Color.Red
            };
        }

        public void UpdateModContentsTree(CkanModule module, bool force = false)
        {
            ModInfoTabControl.Tag = module ?? ModInfoTabControl.Tag;
            //Can be costly. For now only update when visible.
            if (ModInfoTabControl.SelectedIndex != ContentTabPage.TabIndex && !force)
            {
                return;
            }
            Util.Invoke(ContentsPreviewTree, () => _UpdateModContentsTree(ModInfoTabControl.Tag as CkanModule, force));
        }

        private void _UpdateModContentsTree(CkanModule module, bool force = false)
        {
            ContentsPreviewTree.BackColor = SystemColors.Window;
            ContentsPreviewTree.LineColor = SystemColors.WindowText;

            if (Equals(module, currentModContentsModule) && !force)
            {
                return;
            }
            else
            {
                currentModContentsModule = module;
            }
            if (module.IsMetapackage)
            {
                NotCachedLabel.Text = Properties.Resources.ModInfoNoDownload;
                ContentsPreviewTree.Enabled = false;
                ContentsDownloadButton.Enabled = false;
                ContentsOpenButton.Enabled = false;
                ContentsPreviewTree.Nodes.Clear();
            }
            else
            {
                ContentsPreviewTree.Enabled = true;
                ContentsPreviewTree.Nodes.Clear();
                var rootNode = ContentsPreviewTree.Nodes.Add("", module.ToString(), "folderZip", "folderZip");
                if (!Main.Instance.Manager.Cache.IsMaybeCachedZip(module))
                {
                    NotCachedLabel.Text = Properties.Resources.ModInfoNotCached;
                    ContentsDownloadButton.Enabled = true;
                    ContentsOpenButton.Enabled = false;
                    ContentsPreviewTree.Enabled = false;
                }
                else
                {
                    rootNode.Text = Path.GetFileName(
                        Main.Instance.Manager.Cache.GetCachedFilename(module));
                    NotCachedLabel.Text = Properties.Resources.ModInfoCached;
                    ContentsDownloadButton.Enabled = false;
                    ContentsOpenButton.Enabled = true;
                    ContentsPreviewTree.Enabled = true;

                    UseWaitCursor = true;
                    Task.Factory.StartNew(() =>
                    {
                        var paths = new ModuleInstaller(
                                manager.CurrentInstance,
                                Main.Instance.Manager.Cache,
                                Main.Instance.currentUser)
                            .GetModuleContentsList(module)
                            // Load fully in bg
                            .ToArray();
                        // Stop if user switched to another mod
                        if (rootNode.TreeView != null)
                        {
                            Util.Invoke(this, () =>
                            {
                                ContentsPreviewTree.BeginUpdate();
                                foreach (string path in paths)
                                {
                                    AddContentPieces(
                                        rootNode,
                                        path.Split(new char[] {'/'}));
                                }
                                rootNode.ExpandAll();
                                rootNode.EnsureVisible();
                                ContentsPreviewTree.EndUpdate();
                                UseWaitCursor = false;
                            });
                        }
                    });
                }
            }
        }

        private void AddContentPieces(TreeNode parent, IEnumerable<string> pieces)
        {
            var firstPiece = pieces.FirstOrDefault();
            if (firstPiece != null)
            {
                if (parent.ImageKey == "file")
                {
                    parent.SelectedImageKey = parent.ImageKey = "folder";
                }
                // Key/Name needs to be the full relative path for double click to work
                var key = string.IsNullOrEmpty(parent.Name)
                    ? firstPiece
                    : $"{parent.Name}/{firstPiece}";
                var node = parent.Nodes[key]
                        ?? parent.Nodes.Add(key, firstPiece, "file", "file");
                AddContentPieces(node, pieces.Skip(1));
            }
        }

        /// <summary>
        /// Opens the folder of the double-clicked node
        /// in the file browser of the user's system
        /// </summary>
        /// <param name="node">A node of the ContentsPreviewTree</param>
        internal void OpenFileBrowser(TreeNode node)
        {
            string location = manager.CurrentInstance.ToAbsoluteGameDir(node.Name);

            if (File.Exists(location))
            {
                // We need the Folder of the file
                // Otherwise the OS would try to open the file in its default application
                location = Path.GetDirectoryName(location);
            }

            if (!Directory.Exists(location))
            {
                // User either selected the parent node
                // or he clicked on the tree node of a cached, but not installed mod
                return;
            }

            Utilities.ProcessStartURL(location);
        }
    }
}
