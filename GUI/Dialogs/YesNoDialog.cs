using System.Drawing;
using System.Windows.Forms;

namespace CKAN
{
    public partial class YesNoDialog : Form
    {
        public YesNoDialog()
        {
            InitializeComponent();
        }

        public DialogResult ShowYesNoDialog(string text, string yesText = null, string noText = null)
        {
            Util.Invoke(DescriptionLabel, () =>
            {
                DescriptionLabel.Text = text;
                if (yesText != null)
                {
                    YesButton.Text = yesText;
                }
                if (noText != null)
                {
                    NoButton.Text = noText;
                }
                ClientSize = new Size(ClientSize.Width, StringHeight(text, ClientSize.Width - 25) + 2 * 54);
            });

            return ShowDialog();
        }

        /// <summary>
        /// Simple syntactic sugar around Graphics.MeasureString
        /// </summary>
        /// <param name="text">String to measure size of</param>
        /// <param name="maxWidth">Number of pixels allowed horizontally</param>
        /// <returns>
        /// Number of pixels needed vertically to fit the string
        /// </returns>
        private int StringHeight(string text, int maxWidth)
        {
            return (int)CreateGraphics().MeasureString(text, DescriptionLabel.Font, maxWidth).Height;
        }

        public void HideYesNoDialog()
        {
            Util.Invoke(this, Close);
        }
    }
}
