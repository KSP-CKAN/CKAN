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
            var authors = "";

            if (module.author != null)
            {
                for (int i = 0; i < module.author.Count; i++)
                {
                    authors += module.author[i];

                    if (i != module.author.Count - 1)
                    {
                        authors += ", ";
                    }
                }
            }

            MetadataModuleAuthorLabel.Text = authors;
        }

        private HashSet<CkanModule> alreadyVisited = new HashSet<CkanModule>();

        private TreeNode UpdateModDependencyGraphRecursively(TreeNode parentNode, CkanModule module, RelationshipType relationship, int depth, bool virtualProvides = false)
        {
            if (module == null 
                || (depth > 0 && dependencyGraphRootModule == module)
                || (alreadyVisited.Contains(module)))
            {
                return null;
            }

            alreadyVisited.Add(module);

            string nodeText = module.name;
            if (virtualProvides)
            {
                nodeText = String.Format("provided by - {0}", module.name);
            }

            var node = parentNode == null ? new TreeNode(nodeText) : parentNode.Nodes.Add(nodeText);

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
            }

            if (relationships == null)
            {
                return node;
            }

            foreach (RelationshipDescriptor dependency in relationships)
            {
                Registry registry = RegistryManager.Instance(manager.CurrentInstance).registry;

                try
                {
                    try
                    {
                        var dependencyModule = registry.LatestAvailable
                            (dependency.name, manager.CurrentInstance.Version());
                        UpdateModDependencyGraphRecursively(node, dependencyModule, relationship, depth + 1);
                    }
                    catch (ModuleNotFoundKraken)
                    {
                        List<CkanModule> dependencyModules = registry.LatestAvailableWithProvides
                            (dependency.name, manager.CurrentInstance.Version());

                        if (dependencyModules == null)
                        {
                            continue;
                        }

                        var newNode = node.Nodes.Add(dependency.name + " (virtual)");
                        newNode.ForeColor = Color.Gray;

                        foreach (var dep in dependencyModules)
                        {
                            UpdateModDependencyGraphRecursively(newNode, dep, relationship, depth + 1, true);                            
                        }
                    }
                }
                catch (Exception)
                {
                }
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
            dependencyGraphRootModule = module;


            if (ModuleRelationshipType.SelectedIndex == -1)
            {
                ModuleRelationshipType.SelectedIndex = 0;
            }

            var relationshipType = (RelationshipType) ModuleRelationshipType.SelectedIndex;


            alreadyVisited.Clear();

            DependsGraphTree.Nodes.Clear();
            DependsGraphTree.Nodes.Add(UpdateModDependencyGraphRecursively(null, module, relationshipType, 0));
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
            Util.Invoke(ContentsPreviewTree, _UpdateModContentsTree);
        }

        private CkanModule current_mod_contents_module;

        private void _UpdateModContentsTree()
        {
            var module = (CkanModule) ModInfoTabControl.Tag;
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