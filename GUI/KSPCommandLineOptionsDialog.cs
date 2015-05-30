using System.Windows.Forms;

namespace CKAN
{

    public partial class KSPCommandLineOptionsDialog : FormCompatibility
    {
        public KSPCommandLineOptionsDialog()
        {
            InitializeComponent();
            ApplyFormCompatibilityFixes();

            StartPosition = FormStartPosition.CenterScreen;
        }

        public void SetCommandLine(string commandLine)
        {
            AdditionalArguments.Text = commandLine;
        }
    }

}
