using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class NewUpdateDialog : Forms.Form
    {
        /// <summary>
        /// Iniitialize the update info form with version and release notes
        /// </summary>
        /// <param name="version">Version number of new release</param>
        /// <param name="releaseNotes">Markdown formatted description of the new release</param>
        public NewUpdateDialog(string version, string? releaseNotes)
        {
            InitializeComponent();
            VersionLabel.Text = version;
            ReleaseNotesTextbox.Text = releaseNotes?.Trim() ?? "";
        }
    }
}
