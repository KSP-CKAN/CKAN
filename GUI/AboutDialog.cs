using System.Windows.Forms;

namespace CKAN
{
    public partial class AboutDialog : FormCompatibility
    {
        public AboutDialog()
        {
            InitializeComponent();
            ApplyFormCompatibilityFixes();
            StartPosition = FormStartPosition.CenterScreen;
            versionLabel.Text = string.Format(Properties.Resources.AboutDialogLabel2Text, Meta.GetVersion());
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = (sender as LinkLabel).Text;
            Util.HandleLinkClicked(url, e);
        }
    }
}
