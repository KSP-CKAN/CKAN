using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Diagnostics.CodeAnalysis;

using log4net;

using CKAN.Games;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class UnmanagedFiles : UserControl
    {
        public UnmanagedFiles()
        {
            InitializeComponent();
            GameFolderTree.TreeViewNodeSorter = new DirsFirstSorter();
        }

        [MemberNotNull(nameof(inst), nameof(user), nameof(registry))]
        public void LoadFiles(GameInstance          inst,
                              RepositoryDataManager repoData,
                              IUser                 user)
        {
            this.inst = inst;
            this.user = user;
            registry = RegistryManager.Instance(inst, repoData).registry;
            Util.Invoke(this, _UpdateGameFolderTree);
        }

        /// <summary>
        /// Invoked when the user clicks OK
        /// </summary>
        public event Action? Done;

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.UnmanagedFiles);
        }

        private void _UpdateGameFolderTree()
        {
            if (inst != null && registry != null)
            {
                GameFolderTree.BackColor = SystemColors.Window;
                GameFolderTree.LineColor = SystemColors.WindowText;

                GameFolderTree.Nodes.Clear();
                var rootNode = GameFolderTree.Nodes.Add(
                    "",
                    Platform.FormatPath(inst.GameDir()),
                    "folder", "folder");

                UseWaitCursor = true;
                Task.Factory.StartNew(() =>
                {
                    var paths = inst.UnmanagedFiles(registry).ToArray()
                        ?? Array.Empty<string>();
                    Util.Invoke(this, () =>
                    {
                        GameFolderTree.BeginUpdate();
                        foreach (string path in paths)
                        {
                            AddContentPieces(rootNode, path.Split(new char[] {'/'}));
                        }
                        rootNode.Expand();
                        rootNode.EnsureVisible();
                        ExpandDefaultModDir(inst.game);
                        // The nodes don't have children at first, so the sort needs to be re-applied after they're added
                        GameFolderTree.Sort();
                        GameFolderTree.EndUpdate();
                        UseWaitCursor = false;
                    });
                });
            }
        }

        private IEnumerable<string> ParentPaths(string[] pathPieces)
            => Enumerable.Range(1, pathPieces.Length)
                         .Select(numPieces => string.Join("/", pathPieces.Take(numPieces)));

        private void ExpandDefaultModDir(IGame game)
        {
            foreach (string path in ParentPaths(game.PrimaryModDirectoryRelative.Split(new char[] {'/'})))
            {
                foreach (var node in GameFolderTree.Nodes.Find(path, true))
                {
                    node.Expand();
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

        private void RefreshButton_Click(object? sender, EventArgs? e)
        {
            Refresh();
        }

        private void ExpandAllButton_Click(object? sender, EventArgs? e)
        {
            GameFolderTree.BeginUpdate();
            GameFolderTree.ExpandAll();
            GameFolderTree.Nodes[0].EnsureVisible();
            GameFolderTree.EndUpdate();
            GameFolderTree.Focus();
        }

        private void CollapseAllButton_Click(object? sender, EventArgs? e)
        {
            GameFolderTree.BeginUpdate();
            GameFolderTree.CollapseAll();
            GameFolderTree.Nodes[0].Expand();
            GameFolderTree.EndUpdate();
            GameFolderTree.Focus();
        }

        private void ResetCollapseButton_Click(object? sender, EventArgs? e)
        {
            if (inst != null)
            {
                GameFolderTree.BeginUpdate();
                GameFolderTree.CollapseAll();
                GameFolderTree.Nodes[0].Expand();
                ExpandDefaultModDir(inst.game);
                GameFolderTree.Nodes[0].EnsureVisible();
                GameFolderTree.EndUpdate();
                GameFolderTree.Focus();
            }
        }

        private void ShowInFolderButton_Click(object? sender, EventArgs? e)
        {
            Utilities.OpenFileBrowser(GameFolderTree.SelectedNode.Name);
            GameFolderTree.Focus();
        }

        private void DeleteButton_Click(object? sender, EventArgs? e)
        {
            if (inst != null && registry != null
                && GameFolderTree.SelectedNode?.Name is string relPath)
            {
                var absPath = inst.ToAbsoluteGameDir(relPath);
                log.DebugFormat("Trying to delete {0}", absPath);
                if (inst.HasManagedFiles(registry, absPath))
                {
                    Main.Instance?.ErrorDialog(Properties.Resources.FolderContainsManagedFiles, relPath);
                }
                else if (!string.IsNullOrEmpty(relPath)
                         && (Main.Instance?.YesNoDialog(string.Format(Properties.Resources.DeleteUnmanagedFileConfirmation,
                                                                    Platform.FormatPath(relPath)),
                                                        Properties.Resources.DeleteUnmanagedFileDelete,
                                                        Properties.Resources.DeleteUnmanagedFileCancel) ?? false))
                {
                    try
                    {
                        if (File.Exists(absPath))
                        {
                            File.Delete(absPath);
                        }
                        else if (Directory.Exists(absPath))
                        {
                            Directory.Delete(absPath, true);
                        }
                        GameFolderTree.Nodes.Remove(GameFolderTree.SelectedNode);
                    }
                    catch (Exception exc)
                    {
                        user?.RaiseError(exc.Message);
                    }
                }
            }
        }

        private void GameFolderTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Utilities.OpenFileBrowser(e.Node.Name);
        }

        private void OKButton_Click(object? sender, EventArgs? e)
        {
            Done?.Invoke();
        }

        private GameInstance? inst;
        private IUser?        user;
        private Registry?     registry;
        private static readonly ILog log = LogManager.GetLogger(typeof(UnmanagedFiles));
    }

    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    internal class DirsFirstSorter : IComparer, IComparer<TreeNode>
    {
        public int Compare(object? a, object? b)
            => Compare(a as TreeNode, b as TreeNode);

        public int Compare(TreeNode? a, TreeNode? b)
            => a == null ? b == null ? 0
                                     : -1
                         : b == null ? 1
                                     : a.Nodes.Count > 0
                                         ? b.Nodes.Count > 0
                                             ? string.Compare(a.Text, b.Text)
                                             : -1
                                         : b.Nodes.Count > 0
                                             ? 1
                                             : string.Compare(a.Text, b.Text);
    }
}
