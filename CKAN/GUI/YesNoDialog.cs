using System.Windows.Forms;

namespace CKAN
{
    public partial class YesNoDialog : Form
    {
        public YesNoDialog()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
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

            return ShowDialog();
        }

        public void HideYesNoDialog()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { Close(); }));
            }
            else
            {
                Close();
            }
        }
    }
}