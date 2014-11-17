using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CKAN
{

    public enum RelationshipType
    {
        Depends = 0,
        Recommends = 1,
        Suggests = 2
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

            Util.Invoke(MetadataModuleReleaseStatusLabel, () => MetadataModuleReleaseStatusLabel.Text = module.release_status.ToString());
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
            TreeNode node = null;
            
            if (module == null)
            {
                return node;
            }

            if (depth > 0 && dependencyGraphRootModule == module)
            {
                return node;
            }

            if (alreadyVisited.Contains(module))
            {
                return node;
            }

            alreadyVisited.Add(module);

            if (parentNode == null)
            {
                node = new TreeNode(module.name);
            }
            else
            {
                node = parentNode.Nodes.Add(module.name);
            }

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
            }

            if (relationships == null)
            {
                return node;
            }

            int i = 0;
            foreach (RelationshipDescriptor dependency in relationships)
            {
                Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;

                try
                {
                    CkanModule dependencyModule = null;

                    try
                    {
                        dependencyModule = registry.LatestAvailable
                        (dependency.name.ToString(), KSPManager.CurrentInstance.Version());
                        UpdateModDependencyGraphRecursively(node, dependencyModule, relationship, depth + 1);
                    }
                    catch (ModuleNotFoundKraken)
                    {
                        List<CkanModule> dependencyModules = registry.LatestAvailableWithProvides
                        (dependency.name.ToString(), KSPManager.CurrentInstance.Version());

                        if (dependencyModules == null)
                        {
                            continue;
                        }

                        var newNode = node.Nodes.Add(dependency.name + " (virtual)");
                        newNode.ForeColor = Color.Gray;

                        foreach (var dep in dependencyModules)
                        {
                            UpdateModDependencyGraphRecursively(newNode, dep, relationship, depth + 1, true);
                            i++;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            return node;
        }

        private void UpdateModDependencyGraph(CkanModule module)
        {
            Util.Invoke(DependsGraphTree, () => _UpdateModDependencyGraph(module));
        }

        private CkanModule dependencyGraphRootModule = null;

        private void _UpdateModDependencyGraph(CkanModule module)
        {
            if (ModuleRelationshipType.SelectedIndex == -1)
            {
                ModuleRelationshipType.SelectedIndex = 0;
            }

            var relationshipType = (RelationshipType) ModuleRelationshipType.SelectedIndex;

            dependencyGraphRootModule = module;
            alreadyVisited.Clear();

            DependsGraphTree.Nodes.Clear();
            DependsGraphTree.Nodes.Add(UpdateModDependencyGraphRecursively(null, module, relationshipType, 0));
            DependsGraphTree.Nodes[0].ExpandAll();
        }

        private void UpdateModContentsTree(CkanModule module)
        {
            Util.Invoke(ContentsPreviewTree, () => _UpdateModContentsTree(module));
        }

        private void _UpdateModContentsTreeRecursively(CkanModule module, string parentFolder, TreeNode node, List<string> items)
        {

        }

        private void _UpdateModContentsTree(CkanModule module)
        {
            if (!KSPManager.CurrentInstance.Cache.IsCachedZip(module.download))
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

            IEnumerable<string> contents = ModuleInstaller.Instance.GetModuleContentsList(module);
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
