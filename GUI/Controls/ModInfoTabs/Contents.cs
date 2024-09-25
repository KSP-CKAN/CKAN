using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;

using CKAN.Configuration;
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

        public GUIMod? SelectedModule
        {
            set
            {
                if (value != selectedModule)
                {
                    selectedModule = value;
                    Util.Invoke(ContentsPreviewTree, () => _UpdateModContentsTree(selectedModule?.ToModule()));
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

        public event Action<GUIMod>? OnDownloadClick;

        private GUIMod?              selectedModule;
        private CkanModule?          currentModContentsModule;
        private static GameInstanceManager? manager => Main.Instance?.Manager;

        private void ContentsPreviewTree_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs? e)
        {
            if (e != null && manager?.CurrentInstance is GameInstance inst)
            {
                Utilities.OpenFileBrowser(inst.ToAbsoluteGameDir(e.Node.Name));
            }
        }

        private void ContentsDownloadButton_Click(object? sender, EventArgs? e)
        {
            if (SelectedModule != null)
            {
                OnDownloadClick?.Invoke(SelectedModule);
            }
        }

        private void ContentsOpenButton_Click(object? sender, EventArgs? e)
        {
            if (SelectedModule != null
                && manager?.Cache?.GetCachedFilename(SelectedModule.ToModule()) is string s)
            {
                Utilities.ProcessStartURL(s);
            }
        }

        private void _UpdateModContentsTree(CkanModule? module, bool force = false)
        {
            if (module == null)
            {
                NotCachedLabel.Text = "";
                ContentsPreviewTree.Enabled = false;
                ContentsDownloadButton.Enabled = false;
                ContentsOpenButton.Enabled = false;
                ContentsPreviewTree.Nodes.Clear();
            }
            else
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
                    if (!manager?.Cache?.IsMaybeCachedZip(module) ?? false)
                    {
                        NotCachedLabel.Text = Properties.Resources.ModInfoNotCached;
                        ContentsDownloadButton.Enabled = true;
                        ContentsOpenButton.Enabled = false;
                        ContentsPreviewTree.Enabled = false;
                    }
                    else if (manager?.CurrentInstance is GameInstance inst
                             && manager?.Cache != null
                             && selectedModule != null
                             && Main.Instance?.currentUser != null)
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
                            var filters = ServiceLocator.Container.Resolve<IConfiguration>().GlobalInstallFilters
                                                                  .Concat(inst.InstallFilters)
                                                                  .ToHashSet();
                            var tuples = ModuleInstaller.GetModuleContents(manager.Cache, inst, module, filters)
                                                        .Select(f => (path:   inst.ToRelativeGameDir(f.destination),
                                                                      dir:    f.source.IsDirectory,
                                                                      exists: !selectedModule.IsInstalled
                                                                              || File.Exists(f.destination)
                                                                              || Directory.Exists(f.destination)))
                                                        .ToArray();
                            // Stop if user switched to another mod
                            if (rootNode.TreeView != null)
                            {
                                Util.Invoke(this, () =>
                                {
                                    ContentsPreviewTree.BeginUpdate();
                                    foreach ((string path, bool dir, bool exists) in tuples)
                                    {
                                        AddContentPieces(inst, rootNode,
                                                         path.Split(new char[] {'/'}),
                                                         dir, exists);
                                    }
                                    rootNode.ExpandAll();
                                    var initialFocus = FirstMatching(rootNode,
                                                                     n => n.ForeColor == Color.Red)
                                                       ?? rootNode;
                                    initialFocus.EnsureVisible();
                                    ContentsPreviewTree.EndUpdate();
                                    UseWaitCursor = false;
                                });
                            }
                        });
                    }
                }
            }
        }

        private static void AddContentPieces(GameInstance inst,
                                             TreeNode     parent,
                                             string[]     pieces,
                                             bool         dir,
                                             bool         exists)
        {
            var firstPiece = pieces.FirstOrDefault();
            if (firstPiece != null)
            {
                // Key/Name needs to be the full relative path for double click to work
                var key = string.IsNullOrEmpty(parent.Name)
                              ? firstPiece
                              : $"{parent.Name}/{firstPiece}";
                var node = parent.Nodes[key];
                if (node == null)
                {
                    var iconKey = dir || pieces.Length > 1 ? "folder" : "file";
                    node = parent.Nodes.Add(key, firstPiece, iconKey, iconKey);
                    if (!exists && (pieces.Length == 1 || !Directory.Exists(inst.ToAbsoluteGameDir(key))))
                    {
                        node.ForeColor   = Color.Red;
                        node.ToolTipText = iconKey == "folder"
                                               ? Properties.Resources.ModInfoFolderNotFound
                                               : Properties.Resources.ModInfoFileNotFound;
                    }
                }
                if (pieces.Length > 1)
                {
                    AddContentPieces(inst, node, pieces.Skip(1).ToArray(), dir, exists);
                }
            }
        }

        private static TreeNode? FirstMatching(TreeNode root, Func<TreeNode, bool> predicate)
            => predicate(root) ? root
                               : root.Nodes.OfType<TreeNode>()
                                           .Select(n => FirstMatching(n, predicate))
                                           .OfType<TreeNode>()
                                           .FirstOrDefault();

    }
}
