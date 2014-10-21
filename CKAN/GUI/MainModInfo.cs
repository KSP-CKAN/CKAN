using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CKAN
{

    public partial class Main : Form
    {

        private void UpdateModInfo(CkanModule module)
        {
            Util.Invoke(MetadataModuleNameLabel, () => MetadataModuleNameLabel.Text = module.name);
            Util.Invoke(MetadataModuleVersionLabel, () => MetadataModuleVersionLabel.Text = module.version.ToString());
            Util.Invoke(MetadataModuleLicenseLabel, () => MetadataModuleLicenseLabel.Text = module.license.ToString());
            Util.Invoke(MetadataModuleAuthorLabel, () => UpdateModInfoAuthor(module));
            
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
        
        /*
        private void _UpdateModInfo(CkanModule module)
        {
            if (module == null)
            {
                return;
            }

            //ModInfo.Text = "";

            

            if (module.name != null && module.version != null)
            {
                ModInfo.AppendText(String.Format("\"{0}\" - version {1}\r\n", module.name, module.version));
            }

            if (module.@abstract != null)
            {
                ModInfo.AppendText(String.Format("Abstract: {0}\r\n", module.@abstract));
            }

            if (module.author != null)
            {
                string authors = "";
                foreach (string auth in module.author)
                {
                    authors += auth + ", ";
                }

                ModInfo.AppendText(String.Format("Author: {0}\r\n", authors));
            }

            if (module.comment != null)
            {
                ModInfo.AppendText(String.Format("Comment: {0}\r\n", module.comment));
            }

            if (module.download != null)
            {
                ModInfo.AppendText(String.Format("Download: {0}\r\n", module.download));
            }

            if (module.identifier != null)
            {
                ModInfo.AppendText(String.Format("Identifier: {0}\r\n", module.identifier));
            }

            if (module.ksp_version != null)
            {
                ModInfo.AppendText(String.Format("KSP Version: {0}\r\n", module.ksp_version.ToString()));
            }

            if (module.license != null)
            {
                ModInfo.AppendText(String.Format("License: {0}\r\n", module.license.ToString()));
            }

            if (module.release_status != null)
            {
                ModInfo.AppendText(String.Format("Release status: {0}\r\n", module.release_status));
            }

            ModInfo.AppendText("\r\n");

            string dependencies = "";
            if (module.depends != null)
            {
                for (int i = 0; i < module.depends.Count(); i++)
                {
                    dependencies += module.depends[i].name;
                    if (i != module.depends.Count() - 1)
                    {
                        dependencies += ", ";
                    }
                }
            }

            ModInfo.AppendText(String.Format("Dependencies: {0}\r\n", dependencies));
            ModInfo.AppendText("\r\n");

            string recommended = "";
            if (module.recommends != null)
            {
                for (int i = 0; i < module.recommends.Count(); i++)
                {
                    recommended += module.recommends[i].name;
                    if (i != module.recommends.Count() - 1)
                    {
                        recommended += ", ";
                    }
                }
            }

            ModInfo.AppendText(String.Format("Recommends: {0}\r\n", recommended));
            ModInfo.AppendText("\r\n");

            string suggested = "";
            if (module.suggests != null)
            {
                for (int i = 0; i < module.suggests.Count(); i++)
                {
                    suggested += module.suggests[i].name;
                    if (i != module.suggests.Count() - 1)
                    {
                        suggested += ", ";
                    }
                }
            }

            ModInfo.AppendText(String.Format("Suggested: {0}\r\n", suggested));
            ModInfo.AppendText("\r\n");
        }*/

        private void UpdateModDependencyGraphRecursively(TreeNode node, CkanModule module)
        {
            int i = 0;

            node.Text = module.name;
            node.Nodes.Clear();

            if (module.depends != null)
            {
                foreach (RelationshipDescriptor dependency in module.depends)
                {
                    Registry registry = RegistryManager.Instance().registry;

                    try
                    {
                        CkanModule dependencyModule = registry.LatestAvailable(dependency.name.ToString(), KSP.Version());

                        node.Nodes.Add("");
                        UpdateModDependencyGraphRecursively(node.Nodes[i], dependencyModule);
                        i++;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void UpdateModDependencyGraph(CkanModule module)
        {
            Util.Invoke(DependsGraphTree, () => _UpdateModDependencyGraph(module));
        }

        private void _UpdateModDependencyGraph(CkanModule module)
        {
            DependsGraphTree.Nodes.Clear();
            DependsGraphTree.Nodes.Add("");
            UpdateModDependencyGraphRecursively(DependsGraphTree.Nodes[0], module);
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
