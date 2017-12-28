using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using CKAN.Versioning;

namespace CKAN
{
    public enum RelationshipType
    {
        Depends    = 0,
        Recommends = 1,
        Suggests   = 2,
        Supports   = 3,
        Conflicts  = 4
    }

    public partial class MainModInfo : UserControl
    {
        private BackgroundWorker m_CacheWorker;
        private GUIMod _selectedModule;

        public MainModInfo()
        {
            InitializeComponent();

            m_CacheWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            m_CacheWorker.RunWorkerCompleted += PostModCaching;
            m_CacheWorker.DoWork += CacheMod;

            DependsGraphTree.BeforeExpand += BeforeExpand;
        }

        public GUIMod SelectedModule
        {
            set
            {
                this._selectedModule = value;
                if (value == null)
                {
                    ModInfoTabControl.Enabled = false;
                }
                else
                {
                    var module = value;
                    ModInfoTabControl.Enabled = module != null;
                    if (module == null) return;

                    UpdateModInfo(module);
                    UpdateModDependencyGraph(module);
                    UpdateModContentsTree(module);
                    AllModVersions.SelectedModule = module;
                }
            }
            get
            {
                return _selectedModule;
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
                this.splitContainer2.SplitterDistance = value;
            }
        }

        private KSPManager manager
        {
            get
            {
                return Main.Instance.manager;
            }
        }

        public BackgroundWorker CacheWorker
        {
            get
            {
                return m_CacheWorker;
            }
            set
            {
                m_CacheWorker = value;
            }
        }

        private void DependsGraphTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Main.Instance.ResetFilterAndSelectModOnList(e.Node.Name);
        }

        private void ModuleRelationshipType_SelectedIndexChanged(object sender, EventArgs e)
        {
            GUIMod module = SelectedModule;
            if (module == null) return;
            UpdateModDependencyGraph(module);
        }

        private void ContentsPreviewTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenFileBrowser(e.Node);
        }

        private void ContentsDownloadButton_Click(object sender, EventArgs e)
        {
            var module = SelectedModule;
            if (module == null || !module.IsCKAN) return;

            Main.Instance.ResetProgress();
            Main.Instance.ShowWaitDialog(false);
            m_CacheWorker.RunWorkerAsync(module.ToCkanModule());
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.OpenLinkFromLinkLabel(sender as LinkLabel);
        }

        private void UpdateModInfo(GUIMod gui_module)
        {
            CkanModule module = gui_module.ToModule();

            Util.Invoke(MetadataModuleNameLabel, () => MetadataModuleNameLabel.Text = gui_module.Name);
            Util.Invoke(MetadataModuleVersionLabel, () => MetadataModuleVersionLabel.Text = gui_module.LatestVersion.ToString());
            Util.Invoke(MetadataModuleLicenseLabel, () => MetadataModuleLicenseLabel.Text = string.Join(", ", module.license));
            Util.Invoke(MetadataModuleAuthorLabel, () => MetadataModuleAuthorLabel.Text = gui_module.Authors);
            Util.Invoke(MetadataModuleAbstractLabel, () => MetadataModuleAbstractLabel.Text = module.@abstract);
            Util.Invoke(MetadataIdentifierLabel, () => MetadataIdentifierLabel.Text = module.identifier);

            // If we have a homepage provided, use that; otherwise use the spacedock page, curse page or the github repo so that users have somewhere to get more info than just the abstract.
            Util.Invoke(MetadataModuleHomePageLinkLabel,
                       () => MetadataModuleHomePageLinkLabel.Text = gui_module.Homepage.ToString());

            if (module.resources != null && module.resources.repository != null)
            {
                Util.Invoke(MetadataModuleGitHubLinkLabel,
                    () => MetadataModuleGitHubLinkLabel.Text = module.resources.repository.ToString());
            }
            else
            {
                Util.Invoke(MetadataModuleGitHubLinkLabel,
                    () => MetadataModuleGitHubLinkLabel.Text = "N/A");
            }

            if (module.release_status != null)
            {
                Util.Invoke(MetadataModuleReleaseStatusLabel, () => MetadataModuleReleaseStatusLabel.Text = module.release_status.ToString());
            }

            Util.Invoke(MetadataModuleKSPCompatibilityLabel, () => MetadataModuleKSPCompatibilityLabel.Text = gui_module.KSPCompatibilityLong);
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

        private void UpdateModDependencyGraph(CkanModule module)
        {
            ModInfoTabControl.Tag = module ?? ModInfoTabControl.Tag;
            // Can be costly. For now only update when visible.
            if (ModInfoTabControl.SelectedIndex != RelationshipTabPage.TabIndex)
            {
                return;
            }
            Util.Invoke(DependsGraphTree, _UpdateModDependencyGraph);
        }

        private void _UpdateModDependencyGraph()
        {
            CkanModule module = (CkanModule)ModInfoTabControl.Tag;

            DependsGraphTree.BeginUpdate();
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
                                    registry, dependency.name, relationship,
                                    manager.CurrentInstance.VersionCriteria())
                                // Then incompatible mods
                                ?? findDependencyShallow(
                                    registry, dependency.name, relationship, null)
                                // Then give up and note the name without a module
                                ?? nonindexedNode(dependency.name, relationship);
                            node.Nodes.Add(child);
                        }
                    }
                }
            }
        }

        private TreeNode findDependencyShallow(IRegistryQuerier registry, string identifier, RelationshipType relationship, KspVersionCriteria crit)
        {
            try
            {
                CkanModule dependencyModule = registry.LatestAvailable(identifier, crit);
                if (dependencyModule != null)
                {
                    return indexedNode(registry, dependencyModule, relationship, crit != null);
                }
            }
            catch (ModuleNotFoundKraken)
            {
                // If we don't find a module by this name, look for other modules that provide it.
                List<CkanModule> dependencyModules = registry.LatestAvailableWithProvides(identifier, crit);
                if (dependencyModules != null && dependencyModules.Count > 0)
                {
                    List<TreeNode> children = new List<TreeNode>();
                    foreach (CkanModule dep in dependencyModules)
                    {
                        children.Add(indexedNode(registry, dep, relationship, crit != null));
                    }
                    return providesNode(identifier, relationship, children);
                }
            }
            return null;
        }

        private TreeNode providesNode(string identifier, RelationshipType relationship, List<TreeNode> children)
        {
            int icon = (int)relationship + 1;
            return new TreeNode(identifier + " (virtual)", icon, icon, children.ToArray())
            {
                Name        = identifier,
                ToolTipText = relationship.ToString(),
                ForeColor   = Color.Gray
            };
        }

        private TreeNode indexedNode(IRegistryQuerier registry, CkanModule module, RelationshipType relationship, bool compatible)
        {
            int icon = (int)relationship + 1;
            string suffix = compatible ? ""
                : $" ({registry.CompatibleGameVersions(module.identifier)})";
            return new TreeNode($"{module.name} {module.version}{suffix}", icon, icon)
            {
                Name        = module.identifier,
                ToolTipText = relationship.ToString(),
                Tag         = module,
                ForeColor   = compatible ? Color.Empty : Color.Red
            };
        }

        private TreeNode nonindexedNode(string identifier, RelationshipType relationship)
        {
            // Completely nonexistent dependency, e.g. "AJE"
            int icon = (int)relationship + 1;
            return new TreeNode(identifier + " (not indexed)", icon, icon)
            {
                Name        = identifier,
                ToolTipText = relationship.ToString(),
                ForeColor   = Color.Red
            };
        }

        // When switching tabs ensure that the resulting tab is updated.
        private void ModInfoIndexChanged(object sender, EventArgs e)
        {
            if (ModInfoTabControl.SelectedIndex == ContentTabPage.TabIndex)
                UpdateModContentsTree(null);
            if (ModInfoTabControl.SelectedIndex == RelationshipTabPage.TabIndex)
                UpdateModDependencyGraph(null);
        }

        public void UpdateModContentsTree(CkanModule module, bool force = false)
        {
            ModInfoTabControl.Tag = module ?? ModInfoTabControl.Tag;
            //Can be costly. For now only update when visible.
            if (ModInfoTabControl.SelectedIndex != ContentTabPage.TabIndex && !force)
            {
                return;
            }
            Util.Invoke(ContentsPreviewTree, () => _UpdateModContentsTree(force));
        }

        private CkanModule current_mod_contents_module;

        private void _UpdateModContentsTree(bool force = false)
        {
            GUIMod guiMod = SelectedModule;
            if (!guiMod.IsCKAN)
            {
                return;
            }
            CkanModule module = guiMod.ToCkanModule();
            if (Equals(module, current_mod_contents_module) && !force)
            {
                return;
            }
            else
            {
                current_mod_contents_module = module;
            }
            if (!guiMod.IsCached)
            {
                NotCachedLabel.Text = "This mod is not in the cache, click 'Download' to preview contents";
                ContentsDownloadButton.Enabled = true;
                ContentsPreviewTree.Enabled = false;
            }
            else
            {
                NotCachedLabel.Text = "Module is cached, preview available";
                ContentsDownloadButton.Enabled = false;
                ContentsPreviewTree.Enabled = true;
            }

            ContentsPreviewTree.Nodes.Clear();
            ContentsPreviewTree.Nodes.Add(module.name);

            IEnumerable<string> contents = ModuleInstaller.GetInstance(manager.CurrentInstance, GUI.user).GetModuleContentsList(module);
            if (contents == null)
            {
                return;
            }

            foreach (string item in contents)
            {
                ContentsPreviewTree.Nodes[0].Nodes.Add(item);
            }

            ContentsPreviewTree.Nodes[0].ExpandAll();
        }

        private void CacheMod(object sender, DoWorkEventArgs e)
        {
            Main.Instance.ResetProgress();
            Main.Instance.ClearLog();

            NetAsyncModulesDownloader dowloader = new NetAsyncModulesDownloader(Main.Instance.currentUser);

            dowloader.DownloadModules(Main.Instance.CurrentInstance.Cache, new List<CkanModule> { (CkanModule)e.Argument });
            e.Result = e.Argument;
        }

        public void PostModCaching(object sender, RunWorkerCompletedEventArgs e)
        {
            Util.Invoke(this, () => _PostModCaching((CkanModule)e.Result));
        }

        private void _PostModCaching(CkanModule module)
        {
            Main.Instance.HideWaitDialog(true);

            SelectedModule?.UpdateIsCached(); ;
            UpdateModContentsTree(module, true);
            Main.Instance.RecreateDialogs();
        }

        /// <summary>
        /// Opens the file browser of the users system
        /// with the folder of the clicked node opened
        /// TODO: Open a file broweser with the file selected
        /// </summary>
        /// <param name="node">A node of the ContentsPreviewTree</param>
        internal void OpenFileBrowser(TreeNode node)
        {
            string location = node.Text;

            if (File.Exists(location))
            {
                //We need the Folder of the file
                //Otherwise the OS would try to open the file in its default application
                location = Path.GetDirectoryName(location);
            }

            if (!Directory.Exists(location))
            {
                //User either selected the parent node
                //or he clicked on the tree node of a cached, but not installed mod
                return;
            }

            Process.Start(location);
        }
    }
}
