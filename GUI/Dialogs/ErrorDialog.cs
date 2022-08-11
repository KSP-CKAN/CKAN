using System;
using System.Drawing;
using System.Windows.Forms;
using log4net;

namespace CKAN.GUI
{
    public partial class ErrorDialog : Form
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }

        public void ShowErrorDialog(string text, params object[] args)
        {
            Util.Invoke(Main.Instance, () =>
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
                        padding + StringHeight(ErrorMessage.Text, ErrorMessage.Width - 4)
                    )
                );
                if (!Visible)
                {
                    StartPosition = Main.Instance.actuallyVisible
                        ? FormStartPosition.CenterParent
                        : FormStartPosition.CenterScreen;
                    ShowDialog(Main.Instance);
                }
            });
        }

        public void HideErrorDialog()
        {
            Util.Invoke(this, () => Close());
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

        private int StringHeight(string text, int maxWidth)
        {
            return (int)CreateGraphics().MeasureString(text, ErrorMessage.Font, maxWidth).Height;
        }

        private const           int  maxHeight = 600;
        private static readonly ILog log       = LogManager.GetLogger(typeof(ErrorDialog));
    }
}
