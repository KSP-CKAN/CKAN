using System.Windows.Forms;

namespace CKAN.GUI
{
    public partial class NewUpdateDialog : Form
    {
        /// <summary>
        /// Iniitialize the update info form with version and release notes
        /// </summary>
        /// <param name="version">Version number of new release</param>
        /// <param name="releaseNotes">Markdown formatted description of the new release</param>
        public NewUpdateDialog(string version, string releaseNotes)
        {
            InitializeComponent();
            VersionLabel.Text = version;
            ReleaseNotesTextbox.Text = releaseNotes.Trim();
        }
    }
}
