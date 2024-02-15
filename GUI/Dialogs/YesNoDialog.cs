using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class YesNoDialog : Form
    {
        public YesNoDialog()
        {
            InitializeComponent();
            defaultYes = YesButton.Text;
            defaultNo  = NoButton.Text;
        }

        [ForbidGUICalls]
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

        [ForbidGUICalls]
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
            var height = Util.StringHeight(CreateGraphics(), text, DescriptionLabel.Font, ClientSize.Width - 25) + (2 * 54);
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
            ActiveControl = YesButton;
        }

        private void SetupSuppressable(string text, string yesText, string noText, string suppressText)
        {
            Setup(text, yesText, noText);
            SuppressCheckbox.Checked = false;
            SuppressCheckbox.Text = suppressText;
            SuppressCheckbox.Visible = true;
        }

        public void HideYesNoDialog()
        {
            Util.Invoke(this, Close);
        }

        private const int maxHeight = 600;
        private TaskCompletionSource<Tuple<DialogResult, bool>> task;
        private readonly string defaultYes;
        private readonly string defaultNo;
    }
}
