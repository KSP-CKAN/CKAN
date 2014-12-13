using System.Windows.Forms;

namespace CKAN
{

    public partial class KSPCommandLineOptionsDialog : Form
    {
        public KSPCommandLineOptionsDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        public void SetCommandLine(string commandLine)
        {
            AdditionalArguments.Text = commandLine;
        }
    }

}
