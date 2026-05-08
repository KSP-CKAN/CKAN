using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class AboutDialog : FormCompatibility
    {
        public AboutDialog()
        {
            InitializeComponent();
            projectNameLabel.ScaleFonts();
            this.ScaleFonts();
            ApplyFormCompatibilityFixes();
            licenseLinkLabel.LinkColor     = BackColor.LinkColorForBackColor();
            authorsLinkLabel.LinkColor     = BackColor.LinkColorForBackColor();
            sourceLinkLabel.LinkColor      = BackColor.LinkColorForBackColor();
            forumthreadLinkLabel.LinkColor = BackColor.LinkColorForBackColor();
            homepageLinkLabel.LinkColor    = BackColor.LinkColorForBackColor();
            versionLabel.Text = string.Format(Properties.Resources.AboutDialogLabel2Text, Meta.GetVersion());
        }

        private void linkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs? e)
        {
            if (sender is LinkLabel l)
            {
                Util.HandleLinkClicked(l.Text, e);
            }
        }

        private void linkLabel_KeyDown(object? sender, KeyEventArgs? e)
        {
            if (sender is LinkLabel l)
            {
                switch (e)
                {
                    case {KeyCode: Keys.Apps}:
                        Util.LinkContextMenu(l.Text, l);
                        e.Handled = true;
                        break;
                }
            }
        }

    }
}
