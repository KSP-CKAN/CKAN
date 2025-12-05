using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class RenameInstanceDialog : Forms.Form
    {
        public RenameInstanceDialog()
        {
            InitializeComponent();

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
