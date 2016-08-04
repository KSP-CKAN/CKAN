using System.Windows.Forms;

namespace CKAN
{
    public partial class NewUpdateDialog : Form
    {
        public NewUpdateDialog(string version, string releaseNotes)
        {
            InitializeComponent();

            VersionLabel.Text = version;
            ReleaseNotesTextbox.Text = releaseNotes;
        }
    }
}