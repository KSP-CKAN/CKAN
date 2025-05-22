using System;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;

using CKAN.IO;
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
            var coreCfg = ServiceLocator.Container.Resolve<IConfiguration>();
            coreCfg.PropertyChanged += Configuration_PropertyChanged;
        }

        public GUIMod? SelectedModule
        {
            set
            {
                if (value != selectedModule)
                {
                    if (selectedModule != null)
                    {
                        selectedModule.PropertyChanged -= SelectedMod_PropertyChanged;
                    }
                    selectedModule = value;
                    if (selectedModule != null)
                    {
                        ContentsDownloadButton.Text = string.Format(Properties.Resources.ModInfoDownload,
                                                                    CkanModule.FmtSize(selectedModule.ToModule().download_size));
                        selectedModule.PropertyChanged += SelectedMod_PropertyChanged;
                    }
                    Util.Invoke(ContentsPreviewTree,
                                () => _UpdateModContentsTree(selectedModule?.InstalledMod,
                                                             selectedModule?.ToModule()));
                }
            }
            get => selectedModule;
        }

        [ForbidGUICalls]
        private void RefreshModContentsTree()
        {
            if (currentModContentsInstalledModule != null
                || currentModContentsModule != null)
            {
                Util.Invoke(ContentsPreviewTree,
                            () => _UpdateModContentsTree(currentModContentsInstalledModule,
                                                         currentModContentsModule, true));
            }
        }

        public event Action<GUIMod>? OnDownloadClick;

        private static GameInstanceManager? manager => Main.Instance?.Manager;

        private GUIMod?          selectedModule;
        private InstalledModule? currentModContentsInstalledModule;
        private CkanModule?      currentModContentsModule;
        private bool             cancelExpandCollapse;

        private void ContentsPreviewTree_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs? e)
        {
            if (e != null && manager?.CurrentInstance is GameInstance inst)
            {
                Utilities.OpenFileBrowser(inst.ToAbsoluteGameDir(e.Node.Name));
            }
        }

        private void ContentsPreviewTree_MouseDown(object sender, MouseEventArgs e)
        {
            // Double click itself isn't cancellable, and it happens after expand/collapse anyway,
            // so we have to detect it here and then cancel the main expand/collapse event
            switch (e)
            {
                case {Clicks: > 1}:
                    cancelExpandCollapse = true;
                    break;
            }
        }

        private void ContentsPreviewTree_BeforeExpandCollapse(object sender, TreeViewCancelEventArgs e)
        {
            switch (e)
            {
                case {Action: TreeViewAction.Expand
                              or TreeViewAction.Collapse} when cancelExpandCollapse:
                    e.Cancel = true;
                    cancelExpandCollapse = false;
                    break;
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

        private void SelectedMod_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
        {
            switch (e?.PropertyName)
            {
                case nameof(GUIMod.IsCached):
                    RefreshModContentsTree();
                    break;
            }
        }

        private void Configuration_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
        {
            switch (e?.PropertyName)
            {
                case nameof(IConfiguration.GetGlobalInstallFilters):
                    RefreshModContentsTree();
                    break;
            }
        }

        private void _UpdateModContentsTree(InstalledModule? instMod, CkanModule? module,
                                            bool force = false)
        {
            if (module == null)
            {
                currentModContentsModule = null;
                currentModContentsInstalledModule = null;
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
                    currentModContentsInstalledModule = instMod;
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
                            var filters = ServiceLocator.Container.Resolve<IConfiguration>()
                                                                  .GetGlobalInstallFilters(inst.game)
                                                                  .Concat(inst.InstallFilters)
                                                                  .ToHashSet();
                            var tuples = (instMod != null
                                              ? ModuleInstaller.GetModuleContents(inst, instMod.Files, filters)
                                              : ModuleInstaller.GetModuleContents(manager.Cache, inst,
                                                                                  module, filters))
                                         // Load fully in bg
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
                                    // First scroll to the top
                                    rootNode.EnsureVisible();
                                    // Then scroll down to the first red node
                                    FirstMatching(rootNode, n => n.ForeColor == Color.Red)
                                        ?.EnsureVisible();
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
