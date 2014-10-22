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
            Util.Invoke(DescriptionLabel, () => DescriptionLabel.Text = text);
            return ShowDialog();
        }

        public void HideYesNoDialog()
        {
            Util.Invoke(this, Close);
        }
    }
}