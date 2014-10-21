using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CKAN
{

    public enum RelationshipType
    {
        Depends = 0,
        PreDepends = 1,
        Recommends = 2,
        Suggests = 3
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
                    () => MetadataModuleHomePageLinkLabel.Text = module.resources.homepage);     
            }

            if (module.resources != null && module.resources.github != null && module.resources.github.url != null)
            {
                Util.Invoke(MetadataModuleGitHubLinkLabel,
                    () => MetadataModuleGitHubLinkLabel.Text = module.resources.github.url);     
            }

            Util.Invoke(MetadataModuleReleaseStatusLabel, () => MetadataModuleReleaseStatusLabel.Text = module.release_status);
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
       
        private void UpdateModDependencyGraphRecursively(TreeNode node, CkanModule module, RelationshipType relationship)
        {
            int i = 0;

            node.Text = module.name;
            node.Nodes.Clear();
           
            RelationshipDescriptor[] relationships = null;
            switch (relationship)
            {
                case RelationshipType.Depends:
                    relationships = module.depends;
                    break;
                case RelationshipType.PreDepends:
                    relationships = module.pre_depends;
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
                return;
            }

            foreach (RelationshipDescriptor dependency in relationships)
            {
                Registry registry = RegistryManager.Instance().registry;

                try
                {
                    CkanModule dependencyModule = registry.LatestAvailable
                        (dependency.name.ToString(), KSP.CurrentInstance.Version());

                    if (dependencyModule == null)
                    {
                        continue;
                    }

                    node.Nodes.Add("");
                    UpdateModDependencyGraphRecursively(node.Nodes[i], dependencyModule, relationship);
                    i++;
                }
                catch (Exception)
                {
                }
            }
        }

        private void UpdateModDependencyGraph(CkanModule module)
        {
            Util.Invoke(DependsGraphTree, () => _UpdateModDependencyGraph(module));
        }

        private void _UpdateModDependencyGraph(CkanModule module)
        {
            if (ModuleRelationshipType.SelectedIndex == -1)
            {
                ModuleRelationshipType.SelectedIndex = 0;
            }

            var relationshipType = (RelationshipType) ModuleRelationshipType.SelectedIndex;

            DependsGraphTree.Nodes.Clear();
            DependsGraphTree.Nodes.Add("");
            UpdateModDependencyGraphRecursively(DependsGraphTree.Nodes[0], module, relationshipType);
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
            if (ModuleInstaller.IsCached(module))
            {
                NotCachedLabel.Text = "Module is cached, preview available";
                ContentsDownloadButton.Enabled = false;
                ContentsPreviewTree.Enabled = true;
            }
            else
            {
                NotCachedLabel.Text = "This mod is not in the cache, click 'Download' to preview contents";
                ContentsDownloadButton.Enabled = true;
                ContentsPreviewTree.Enabled = false; 
            }

            ContentsPreviewTree.Nodes.Clear();
            ContentsPreviewTree.Nodes.Add(module.name);

            var contents = ModuleInstaller.Instance.GetModuleContentsList(module);
            if (contents == null)
            {
                return;
            }

            for (int i = 0; i < contents.Count; i++)
            {
                contents[i] = contents[i].Replace("\\", "/");
                if (contents[i][contents[i].Length - 1] == '/')
                {
                    contents[i] = contents[i].Substring(0, contents[i].Length - 1);
                }
            }

            foreach (var item in contents)
            {
                ContentsPreviewTree.Nodes[0].Nodes.Add(item);
            }

            ContentsPreviewTree.Nodes[0].ExpandAll();
        }

    }

}
