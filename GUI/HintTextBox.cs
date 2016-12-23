using System;
using System.Drawing;
using System.Windows.Forms;

namespace CKAN
{
    /// <summary>
    /// A textbox which shows a "clear text" icon on the right side
    /// whenever data is present.
    /// </summary>
    public partial class HintTextBox : TextBox
    {
        private readonly PictureBox _clearIcon;

        /// <summary>
        /// Creates a HintTextBox object.
        /// </summary>
        public HintTextBox()
        {
            InitializeComponent();

            // set up clear icon state and handlers
            _clearIcon = new PictureBox()
            {
                BackColor = Color.Transparent,
                Visible = false,
                Cursor = Cursors.Hand,
                Image = global::CKAN.Properties.Resources.textClear,
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            // post-instantiation setup
            _clearIcon.Size = _clearIcon.Image.Size;
            _clearIcon.Click += HintClearIcon_Click;

            // add icon and show form
            Controls.Add(_clearIcon);
            _clearIcon.Parent = this;
            _clearIcon.BringToFront();
        }

        /// <summary>
        /// When the icon is clicked, reset the textbox value.
        /// </summary>
        /// <param name="sender">The control sending the event</param>
        /// <param name="e">The event arguments</param>
        private void HintClearIcon_Click(object sender, EventArgs e)
        {
            Text = string.Empty;
        }

        /// <summary>
        /// Show the clear icon when the textbox has data
        /// </summary>
        /// <param name="sender">The control sending the event</param>
        /// <param name="e">The event arguments</param>
        private void HintTextBox_TextChanged(object sender, EventArgs e)
        {
            // sanity checks
            if (!Visible || ReadOnly)
            {
                return;
            }

            _clearIcon.Visible = (TextLength > 0);
        }

        /// <summary>
        /// Adjust the position of the clear icon regardless of control size.
        /// </summary>
        /// <param name="sender">The control sending the event</param>
        /// <param name="e">The event arguments</param>
        private void HintTextBox_SizeChanged(object sender, EventArgs e)
        {
            if (_clearIcon.Image == null)
            {
                return;
            }

            _clearIcon.Location = new Point(
                // align with right edge of textbox minus 5px
                Width - _clearIcon.Width - 5,
                // need to divide these as decimals and drop back to int at the end
                (int)Math.Ceiling(Height / 2d - (_clearIcon.Height / 2d))
            );
        }

        /// <summary>
        /// Intercept the low-level key press messages to catch Esc,
        /// which causes the textbox to clear
        /// </summary>
        /// <param name="msg">Win32 Message object</param>
        /// <param name="keyData">Which keys are being pressed</param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Text = "";
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
