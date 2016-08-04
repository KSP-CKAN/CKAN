using System.Windows.Forms;

namespace CKAN
{
    public partial class RenameInstanceDialog : MonoCompatibleForm
    {
        public RenameInstanceDialog()
        {
            InitializeComponent();
            ApplyFormCompatibilityFixes();

            // Set the default actions for pressing Enter and Escape.
            AcceptButton = OKButton;
            CancelButton = CancelRenameInstanceButton;
        }

        public DialogResult ShowRenameInstanceDialog(string name)
        {
            NameTextBox.Text = name;
            return ShowDialog();
        }

        public string GetResult()
        {
            return NameTextBox.Text;
        }
    }
}