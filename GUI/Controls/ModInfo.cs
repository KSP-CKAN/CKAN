using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
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
                        UpdateModInfo(value);
                        UpdateModDependencyGraph(module);
                        UpdateModContentsTree(module);
                        AllModVersions.SelectedModule = value;
                    }
                    selectedModule = value;
                }
            }
            get
            {
                return selectedModule;
            }
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

        private void BeforeExpand(object sender, TreeViewCancelEventArgs args)
        {
            // Hourglass cursor
            Cursor prevCur = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            DependsGraphTree.BeginUpdate();

            TreeNode node = args.Node;
            IRegistryQuerier registry = RegistryManager.Instance(manager.CurrentInstance).registry;
            // Should already have children, since the user is expanding it
            foreach (TreeNode child in node.Nodes)
            {
                // If there are grandchildren, then this child has been loaded before
                if (child.Nodes.Count == 0)
                {
                    AddChildren(registry, child);
                }
            }

            DependsGraphTree.EndUpdate();

            Cursor.Current = prevCur;
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
            root.Expand();
            DependsGraphTree.EndUpdate();
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
            // Skip children of nodes from circular dependencies
            if (ImMyOwnGrandpa(node))
                return;

            // Load one layer of grandchildren on demand
            CkanModule module = node.Tag as CkanModule;
            // Tag is null for non-indexed nodes
            if (module != null)
            {
                foreach (RelationshipType relationship in kindsOfRelationships)
                {
                    IEnumerable<RelationshipDescriptor> relationships = null;
                    switch (relationship)
                    {
                        case RelationshipType.Depends:
                            relationships = module.depends;
                            break;
                        case RelationshipType.Recommends:
                            relationships = module.recommends;
                            break;
                        case RelationshipType.Suggests:
                            relationships = module.suggests;
                            break;
                        case RelationshipType.Supports:
                            relationships = module.supports;
                            break;
                        case RelationshipType.Conflicts:
                            relationships = module.conflicts;
                            break;
                    }
                    if (relationships != null)
                    {
                        foreach (RelationshipDescriptor dependency in relationships)
                        {
                            // Look for compatible mods
                            TreeNode child = findDependencyShallow(
                                    registry, dependency, relationship,
                                    manager.CurrentInstance.VersionCriteria())
                                // Then incompatible mods
                                ?? findDependencyShallow(
                                    registry, dependency, relationship, null)
                                // Then give up and note the name without a module
                                ?? nonindexedNode(dependency, relationship);
                            node.Nodes.Add(child);
                        }
                    }
                }
            }
        }

        private TreeNode findDependencyShallow(IRegistryQuerier registry, RelationshipDescriptor relDescr, RelationshipType relationship, GameVersionCriteria crit)
        {
            // Maybe it's a DLC?
            if (relDescr.MatchesAny(
                registry.InstalledModules.Select(im => im.Module),
                new HashSet<string>(registry.InstalledDlls),
                registry.InstalledDlc))
            {
                return nonModuleNode(relDescr, null, relationship);
            }

            // Find modules that satisfy this dependency
            List<CkanModule> dependencyModules = relDescr.LatestAvailableWithProvides(registry, crit);
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

        // When switching tabs ensure that the resulting tab is updated.
        private void ModInfoIndexChanged(object sender, EventArgs e)
        {
            switch (ModInfoTabControl.SelectedTab.Name)
            {

                case "ContentTabPage":
                    UpdateModContentsTree(null);
                    break;

                case "RelationshipTabPage":
                    UpdateModDependencyGraph(null);
                    break;

                case "AllModVersionsTabPage":
                    if (Platform.IsMono)
                    {
                        // Workaround: make sure the ListView headers are drawn
                        AllModVersions.ForceRedraw();
                    }
                    break;

            }
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
                ContentsPreviewTree.Nodes.Add(module.name);
                if (!Main.Instance.Manager.Cache.IsMaybeCachedZip(module))
                {
                    NotCachedLabel.Text = Properties.Resources.ModInfoNotCached;
                    ContentsDownloadButton.Enabled = true;
                    ContentsOpenButton.Enabled = false;
                    ContentsPreviewTree.Enabled = false;
                }
                else
                {
                    NotCachedLabel.Text = Properties.Resources.ModInfoCached;
                    ContentsDownloadButton.Enabled = false;
                    ContentsOpenButton.Enabled = true;
                    ContentsPreviewTree.Enabled = true;

                    // Get all the data; can put this in bg if slow
                    var contents = new ModuleInstaller(
                            manager.CurrentInstance,
                            Main.Instance.Manager.Cache,
                            Main.Instance.currentUser)
                        .GetModuleContentsList(module)?.ToList();

                    // Update UI; must be in fg
                    if (contents != null)
                    {
                        foreach (string item in contents)
                        {
                            ContentsPreviewTree.Nodes[0].Nodes.Add(
                                item.Replace('/', Path.DirectorySeparatorChar));
                        }
                        ContentsPreviewTree.Nodes[0].ExpandAll();
                    }
                }
            }
        }

        /// <summary>
        /// Opens the file browser of the users system
        /// with the folder of the clicked node opened
        /// TODO: Open a file browser with the file selected
        /// </summary>
        /// <param name="node">A node of the ContentsPreviewTree</param>
        internal void OpenFileBrowser(TreeNode node)
        {
            string location = manager.CurrentInstance.ToAbsoluteGameDir(node.Text);

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
