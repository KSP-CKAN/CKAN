using System.Windows.Forms;

namespace CKAN
{
    public partial class KSPCommandLineOptionsDialog : MonoCompatibleForm
    {
        public KSPCommandLineOptionsDialog()
        {
            InitializeComponent();
            ApplyFormCompatibilityFixes();

            StartPosition = FormStartPosition.CenterScreen;
        }

        public DialogResult ShowKSPCommandLineOptionsDialog(string arguments)
        {
            AdditionalArguments.Text = arguments;
            return ShowDialog();
        }

        public string GetResult()
        {
            return AdditionalArguments.Text;
        }
    }
}