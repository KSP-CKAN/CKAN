using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class NewRepoDialog : Form
    {
        public NewRepoDialog(Repository[] repos)
        {
            this.repos = repos;
            InitializeComponent();
            this.ScaleFonts();
        }

        public Repository Selection
            => new Repository(RepoNameTextBox.Text, RepoUrlTextBox.Text);

        private void NewRepoDialog_Load(object? sender, EventArgs? e)
        {
            ReposListBox.Items.Clear();
            ReposListBox.Items.AddRange(
                repos.Select(r => new ListViewItem(new string[] { r.name,
                                                                  r.uri.ToString() })
                                  {
                                      Tag = r
                                  })
                     .ToArray());
            ClientSize = new Size(ClientSize.Width,
                                  Math.Max(ClientSize.Height,
                                           ClientSize.Height
                                               - ReposListBox.Height
                                               // Use the horizontal scrollbar as a proxy for the unknowable header height
                                               + 3 * SystemInformation.HorizontalScrollBarHeight
                                               + ReposListBox.Items.OfType<ListViewItem>()
                                                                   .Sum(lvi => lvi.Bounds.Height)));
            ReposListBox.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            ReposListBox.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ReposListBox.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            ReposListBox.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void ReposListBox_SelectedIndexChanged(object? sender, EventArgs? e)
        {
            if (//ReposListBox.SelectedItems is [{Tag: Repository r}, ..]
                ReposListBox.SelectedItems.Count > 0
                && ReposListBox.SelectedItems[0] is {Tag: Repository r})
            {
                RepoNameTextBox.Text = r.name;
                RepoUrlTextBox.Text = r.uri.ToString();
            }
        }

        private void ReposListBox_DoubleClick(object sender, EventArgs r)
        {
            if (ReposListBox.SelectedItems.Count == 0)
            {
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void RepoUrlTextBox_TextChanged(object? sender, EventArgs? e)
        {
            RepoOK.Enabled = RepoNameTextBox.Text.Length > 0
                && RepoUrlTextBox.Text.Length > 0;
        }

        private readonly Repository[] repos;
    }
}
