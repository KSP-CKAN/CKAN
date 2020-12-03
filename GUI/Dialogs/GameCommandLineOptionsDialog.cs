using System.Windows.Forms;

namespace CKAN
{

    public partial class GameCommandLineOptionsDialog : Form
    {
        public GameCommandLineOptionsDialog()
        {
            InitializeComponent();

            StartPosition = FormStartPosition.CenterScreen;
        }

        public DialogResult ShowGameCommandLineOptionsDialog(string arguments)
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
