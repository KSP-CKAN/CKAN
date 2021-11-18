using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace CKAN.GUI
{
    public partial class YesNoDialog : Form
    {
        public YesNoDialog()
        {
            InitializeComponent();
            defaultYes = YesButton.Text;
            defaultNo  = NoButton.Text;
        }

        public DialogResult ShowYesNoDialog(Form parentForm, string text, string yesText = null, string noText = null)
        {
            task = new TaskCompletionSource<Tuple<DialogResult, bool>>();

            Util.Invoke(parentForm, () =>
            {
                Setup(text, yesText, noText);
                task.SetResult(new Tuple<DialogResult, bool>(ShowDialog(parentForm), SuppressCheckbox.Checked));
            });

            return task.Task.Result.Item1;
        }

        public Tuple<DialogResult, bool> ShowSuppressableYesNoDialog(Form parentForm, string text, string suppressText, string yesText = null, string noText = null)
        {
            task = new TaskCompletionSource<Tuple<DialogResult, bool>>();

            Util.Invoke(parentForm, () =>
            {
                SetupSuppressable(text, yesText, noText, suppressText);
                task.SetResult(new Tuple<DialogResult, bool>(ShowDialog(parentForm), SuppressCheckbox.Checked));
            });

            return task.Task.Result;
        }

        private void Setup(string text, string yesText, string noText)
        {
            var height = StringHeight(text, ClientSize.Width - 25) + 2 * 54;
            DescriptionLabel.Text = text;
            DescriptionLabel.TextAlign = text.Contains("\n")
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Center;
            DescriptionLabel.ScrollBars = height < maxHeight
                ? ScrollBars.None
                : ScrollBars.Vertical;
            YesButton.Text = yesText ?? defaultYes;
            NoButton.Text  = noText  ?? defaultNo;
            SuppressCheckbox.Visible = false;
            ClientSize = new Size(
                ClientSize.Width,
                Math.Min(maxHeight, height)
            );
        }

        private void SetupSuppressable(string text, string yesText, string noText, string suppressText)
        {
            Setup(text, yesText, noText);
            SuppressCheckbox.Checked = false;
            SuppressCheckbox.Text = suppressText;
            SuppressCheckbox.Visible = true;
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

        private const int maxHeight = 600;
        private TaskCompletionSource<Tuple<DialogResult, bool>> task;
        private string defaultYes;
        private string defaultNo;
    }
}
