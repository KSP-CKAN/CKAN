using System;
using System.Drawing;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using log4net;

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class ErrorDialog : Form
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }

        [ForbidGUICalls]
        public void ShowErrorDialog(Main mainForm, string text, params object[] args)
        {
            Util.Invoke(this, () =>
            {
                log.ErrorFormat(text, args);
                // Append to previous text, if any
                if (!string.IsNullOrEmpty(ErrorMessage.Text))
                {
                    ErrorMessage.Text += Environment.NewLine + Environment.NewLine;
                }
                ErrorMessage.Text += string.Format(text, args);
                // Resize form to fit
                var padding = ClientSize.Height - ErrorMessage.Height + 50;
                ClientSize = new Size(
                    ClientSize.Width,
                    Math.Min(
                        maxHeight,
                        padding + Util.StringHeight(CreateGraphics(),
                                                    ErrorMessage.Text,
                                                    ErrorMessage.Font,
                                                    ErrorMessage.Width - 4)));
                if (!Visible)
                {
                    StartPosition = mainForm.actuallyVisible
                        ? FormStartPosition.CenterParent
                        : FormStartPosition.CenterScreen;
                    ShowDialog(mainForm);
                }
            });
        }

        private void DismissButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clear message on close so we start blank next time
            ErrorMessage.Text = "";
        }

        private const           int  maxHeight = 600;
        private static readonly ILog log       = LogManager.GetLogger(typeof(ErrorDialog));
    }
}
