using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CKAN
{
    public enum RelationshipType
    {
        Depends = 0,
        Recommends = 1,
        Suggests = 2,
        Supports = 3
    }

    public partial class Main : Form
    {
        private void UpdateModInfo(CkanModule module)
        {
            Util.Invoke(MetadataModuleNameLabel, () => MetadataModuleNameLabel.Text = module.name);
            Util.Invoke(MetadataModuleVersionLabel, () => MetadataModuleVersionLabel.Text = module.version.ToString());
            Util.Invoke(MetadataModuleLicenseLabel, () => MetadataModuleLicenseLabel.Text = module.license.ToString());
            Util.Invoke(MetadataModuleAuthorLabel, () => UpdateModInfoAuthor(module));
            Util.Invoke(MetadataModuleAbstractLabel, () => MetadataModuleAbstractLabel.Text = module.@abstract);

            if (module.resources != null && module.resources.homepage != null)
            {
                Util.Invoke(MetadataModuleHomePageLinkLabel,
                    () => MetadataModuleHomePageLinkLabel.Text = module.resources.homepage.ToString());
            }
            else
            {
                Util.Invoke(MetadataModuleHomePageLinkLabel,
                    () => MetadataModuleHomePageLinkLabel.Text = "N/A");
            }

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
        }

        private void UpdateModInfoAuthor(CkanModule module)
        {
            var authors = module.author != null
                ? String.Join(", ", module.author)
                : String.Empty;

            MetadataModuleAuthorLabel.Text = authors;
        }

        private readonly HashSet<CkanModule> alreadyVisited = new HashSet<CkanModule>();
        private readonly Queue<QueueItem> to_visit = new Queue<QueueItem>();

        private struct QueueItem
        {
            public CkanModule Module;
            public TreeNode ParentNode;
            public bool VirtualProvides;
        }

        private TreeNode UpdateModDependencyGraph(TreeNode parentNode, CkanModule module, RelationshipType relationship, bool isHeadNode, bool virtualProvides = false)
        {

            if (module == null
                || (!isHeadNode && dependencyGraphRootModule == module)
                || alreadyVisited.Contains(module))
            {
                return null;
            }



            alreadyVisited.Add(module);

            var node = parentNode == null
                ? new TreeNode(module.name)
                : parentNode.Nodes.Add(module.name);

            var relationships = GetRelationshipDescriptorsFromType(module, relationship);

            if (relationships == null)
            {
                return node;
            }

            var current_instance = Instance.CurrentInstance;
            var registry = RegistryManager.Instance(current_instance).registry;

            foreach (RelationshipDescriptor dependency in relationships)
            {
                try
                {
                    var dependency_module = registry.LatestAvailable(dependency.name, current_instance.Version());
                    var queue_items = new QueueItem
                    {
                        Module = dependency_module,
                        ParentNode = node,
                        VirtualProvides = false,

                    };
                    to_visit.Enqueue(queue_items);
                }
                catch (ModuleNotFoundKraken)
                {
                    //Can not find referanced module. Check for a module that provides it. 
                    try
                    {
                        List<CkanModule> dependency_modules = registry.LatestAvailableWithProvides(dependency.name, current_instance.Version());

                        if (dependency_modules == null)
                        {
                            continue;
                        }

                        var new_node = node.Nodes.Add(dependency.name + " (virtual)");
                        new_node.ForeColor = Color.Gray;

                        foreach (var dep in dependency_modules)
                        {
                            var queue_items = new QueueItem
                            {
                                Module = dep,
                                ParentNode = new_node,
                                VirtualProvides = true
                            };
                            to_visit.Enqueue(queue_items);
                        }
                    }
                    catch (ModuleNotFoundKraken)
                    {
                        //No need to display mods we can not provide. 
                    }
                }

            }

            while (to_visit.Count > 0)
            {
                var item = to_visit.Dequeue();
                UpdateModDependencyGraph(item, relationship);
            }

            if (virtualProvides)
            {
                node.Collapse(true);
            }
            else
            {
                node.ExpandAll();
            }

            return node;
        }

        private void UpdateModDependencyGraph(QueueItem item, RelationshipType relationship)
        {
            UpdateModDependencyGraph(item.ParentNode, item.Module, relationship, false, item.VirtualProvides);
        }

        private static IEnumerable<RelationshipDescriptor> GetRelationshipDescriptorsFromType(Module module, RelationshipType relationship)
        {
            IEnumerable<RelationshipDescriptor> relationships;
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
                default: throw new ArgumentException("Unknown type of relationship");
            }
            return relationships;
        }

        private void UpdateModDependencyGraph(CkanModule module)
        {
            ModInfoTabControl.Tag = module ?? ModInfoTabControl.Tag;
            //Can be costly. For now only update when visible. 
            if (ModInfoTabControl.SelectedIndex != RelationshipTabPage.TabIndex)
            {
                return;
            }
            Util.Invoke(DependsGraphTree, _UpdateModDependencyGraph);
        }

        private CkanModule dependencyGraphRootModule;

        private void _UpdateModDependencyGraph()
        {
            var module = (CkanModule) ModInfoTabControl.Tag;
            if (module == dependencyGraphRootModule)
            {
                return;
            }
            else
            {
                dependencyGraphRootModule = module;
            }


            if (ModuleRelationshipType.SelectedIndex == -1)
            {
                ModuleRelationshipType.SelectedIndex = 0;
            }

            var relationshipType = (RelationshipType)ModuleRelationshipType.SelectedIndex;


            alreadyVisited.Clear();

            DependsGraphTree.Nodes.Clear();
            DependsGraphTree.Nodes.Add(UpdateModDependencyGraph(null, module, relationshipType, true));
            DependsGraphTree.Nodes[0].ExpandAll();

        }

        // When switching tabs ensure that the resulting tab is updated. 
        private void ModInfoIndexChanged(object sender, EventArgs e)
        {
            if (ModInfoTabControl.SelectedIndex == ContentTabPage.TabIndex)
                UpdateModContentsTree(null);
            if (ModInfoTabControl.SelectedIndex == RelationshipTabPage.TabIndex)
                UpdateModDependencyGraph(null);
        }

        private void UpdateModContentsTree(CkanModule module)
        {
            ModInfoTabControl.Tag = module ?? ModInfoTabControl.Tag;
            //Can be costly. For now only update when visible. 
            if (ModInfoTabControl.SelectedIndex != ContentTabPage.TabIndex)
            {
                return;
            }
            Util.Invoke(ContentsPreviewTree, ()=>_UpdateModContentsTree(module));
        }

        private CkanModule current_mod_contents_module;

        private void _UpdateModContentsTree(CkanModule module)
        {            
            if (module == current_mod_contents_module)
            {
                return;
            }
            else
            {
                current_mod_contents_module = module;
            }
            if (!manager.CurrentInstance.Cache.IsCachedZip(module.download))
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
    }
}
