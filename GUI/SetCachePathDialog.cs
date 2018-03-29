using System;
using System.Windows.Forms;

namespace CKAN
{
    public partial class SetCachePathDialog : FormCompatibility
    {
        public SetCachePathDialog()
        {
            InitializeComponent();
            ApplyFormCompatibilityFixes();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private FolderBrowserDialog browseDialog = new FolderBrowserDialog();

        private void SetCachePathDialog_Load(object sender, EventArgs e)
        {
            browseDialog.ShowNewFolderButton = true;
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            if (browseDialog.ShowDialog() == DialogResult.OK)
            {
                var path = browseDialog.SelectedPath;
                PathTextBox.Text = path;
            }
        }

        public DialogResult ShowSetCachePathDialog(string path)
        {
            PathTextBox.Text = path;
            return ShowDialog();
        }

        public string GetPath()
        {
            return PathTextBox.Text;
        }
    }
}
