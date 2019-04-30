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
            label2.Text = string.Format(Properties.Resources.AboutDialogLabel2Text, Meta.GetVersion());
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.HandleLinkClicked("https://github.com/KSP-CKAN/CKAN/blob/master/LICENSE.md", e);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.HandleLinkClicked("https://github.com/KSP-CKAN/CKAN/graphs/contributors", e);
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.HandleLinkClicked("https://github.com/KSP-CKAN/CKAN/", e);
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.HandleLinkClicked("http://forum.kerbalspaceprogram.com/index.php?/topic/154922-ckan", e);
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.HandleLinkClicked("http://ksp-ckan.space", e);
        }
    }
}
