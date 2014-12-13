using System.Windows.Forms;

namespace CKAN
{
    public partial class RenameInstanceDialog : FormCompatibility
    {
        public RenameInstanceDialog()
        {
            InitializeComponent();
            ApplyFormCompatibilityFixes();
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
