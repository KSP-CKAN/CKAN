using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class Contents : UserControl
    {
        public Contents()
        {
            InitializeComponent();
        }

        public GUIMod SelectedModule
        {
            set
            {
                if (value != selectedModule)
                {
                    selectedModule = value;
                    Util.Invoke(ContentsPreviewTree, () => _UpdateModContentsTree(selectedModule.ToModule()));
                }
            }
            get => selectedModule;
        }

        [ForbidGUICalls]
        public void RefreshModContentsTree()
        {
            if (currentModContentsModule != null)
            {
                Util.Invoke(ContentsPreviewTree, () => _UpdateModContentsTree(currentModContentsModule, true));
            }
        }

        public event Action<GUIMod> OnDownloadClick;

        private GUIMod              selectedModule;
        private CkanModule          currentModContentsModule;
        private GameInstanceManager manager => Main.Instance.Manager;

        private void ContentsPreviewTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenFileBrowser(e.Node);
        }

        private void ContentsDownloadButton_Click(object sender, EventArgs e)
        {
            OnDownloadClick?.Invoke(SelectedModule);
        }

        private void ContentsOpenButton_Click(object sender, EventArgs e)
        {
            Utilities.ProcessStartURL(manager.Cache.GetCachedFilename(SelectedModule.ToModule()));
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
                if (!manager.Cache.IsMaybeCachedZip(module))
                {
                    NotCachedLabel.Text = Properties.Resources.ModInfoNotCached;
                    ContentsDownloadButton.Enabled = true;
                    ContentsOpenButton.Enabled = false;
                    ContentsPreviewTree.Enabled = false;
                }
                else
                {
                    rootNode.Text = Path.GetFileName(
                        manager.Cache.GetCachedFilename(module));
                    NotCachedLabel.Text = Properties.Resources.ModInfoCached;
                    ContentsDownloadButton.Enabled = false;
                    ContentsOpenButton.Enabled = true;
                    ContentsPreviewTree.Enabled = true;

                    UseWaitCursor = true;
                    Task.Factory.StartNew(() =>
                    {
                        var paths = new ModuleInstaller(
                                manager.CurrentInstance,
                                manager.Cache,
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
        private void OpenFileBrowser(TreeNode node)
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
                // or clicked on the tree node of a cached, but not installed mod
                return;
            }

            Utilities.ProcessStartURL(location);
        }
    }
}
