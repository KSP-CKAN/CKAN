using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class DeleteDirectories : UserControl
    {
        public DeleteDirectories()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set up the display for interaction.
        /// This is separate from Wait so we can set up
        /// before the calling code switches to the tab.
        /// </summary>
        /// <param name="possibleConfigOnlyDirs">Directories that the user may want to delete</param>
        [ForbidGUICalls]
        public void LoadDirs(GameInstance ksp, HashSet<string> possibleConfigOnlyDirs)
        {
            instance = ksp;
            var items = possibleConfigOnlyDirs
                .OrderBy(d => d)
                .Select(d => new ListViewItem(Platform.FormatPath(instance.ToRelativeGameDir(d)))
                    {
                        Tag     = d,
                        Checked = true
                    })
                .ToArray();
            Util.Invoke(this, () =>
            {
                DeleteButton.Focus();
                DirectoriesListView.Items.Clear();
                DirectoriesListView.Items.AddRange(items);
                DirectoriesListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                DirectoriesListView_ItemSelectionChanged(null, null);
            });
        }

        /// <summary>
        /// Allow the user to interact with the control,
        /// return after they click one of the action buttons.
        /// </summary>
        /// <param name="toDelete">The directories to delete if the return is true</param>
        /// <returns>
        /// true if user chose to delete, false otherwise
        /// </returns>
        [ForbidGUICalls]
        public bool Wait(out HashSet<string> toDelete)
        {
            if (Platform.IsMono)
            {
                // Workaround: make sure the ListView headers are drawn
                Util.Invoke(DirectoriesListView, () =>
                {
                    DirectoriesListView.EndUpdate();
                    ContentsListView.EndUpdate();
                });
            }

            // Reset the task each time
            task = new TaskCompletionSource<bool>();
            // This will block until one of the buttons calls SetResult
            if (task.Task.Result)
            {
                toDelete = DirectoriesListView.CheckedItems.Cast<ListViewItem>()
                    .Select(lvi => lvi.Tag as string)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToHashSet();
                return true;
            }
            else
            {
                toDelete = null;
                return false;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ExplanationLabel.Height = Util.LabelStringHeight(CreateGraphics(), ExplanationLabel);
        }

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.DeleteDirectories);
        }

        private void DirectoriesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ContentsListView.Items.Clear();
            ContentsListView.Items.AddRange(
                DirectoriesListView.SelectedItems.Cast<ListViewItem>()
                    .SelectMany(lvi => Directory.EnumerateFileSystemEntries(
                            lvi.Tag as string,
                            "*",
                            SearchOption.AllDirectories)
                        .Select(f => new ListViewItem(Platform.FormatPath(CKANPathUtils.ToRelative(f, lvi.Tag as string)))))
                    .ToArray());
            if (DirectoriesListView.SelectedItems.Count == 0)
            {
                ContentsListView.Items.Add(SelectDirPrompt);
            }
            ContentsListView.AutoResizeColumns(
                ContentsListView.Items.Count > 0
                    ? ColumnHeaderAutoResizeStyle.ColumnContent
                    : ColumnHeaderAutoResizeStyle.HeaderSize);
            OpenDirectoryButton.Enabled = DirectoriesListView.SelectedItems.Count > 0;
        }

        private void OpenDirectoryButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in DirectoriesListView.SelectedItems)
            {
                Utilities.ProcessStartURL(lvi.Tag as string);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            task?.SetResult(true);
        }

        private void KeepAllButton_Click(object sender, EventArgs e)
        {
            task?.SetResult(false);
        }

        private TaskCompletionSource<bool> task = null;
        private GameInstance instance = null;
    }
}
