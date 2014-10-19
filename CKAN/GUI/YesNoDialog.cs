using System.Windows.Forms;

namespace CKAN
{
    public partial class YesNoDialog : Form
    {
        public YesNoDialog()
        {
            InitializeComponent();
        }

        public DialogResult ShowYesNoDialog(string text)
        {
            if (DescriptionLabel.InvokeRequired)
            {
                DescriptionLabel.Invoke(new MethodInvoker(delegate { DescriptionLabel.Text = text; }));
            }
            else
            {
                DescriptionLabel.Text = text;
            }

            StartPosition = FormStartPosition.CenterScreen;
            return ShowDialog();
        }

        public void HideYesNoDialog()
        {
            Close();
        }
    }
}