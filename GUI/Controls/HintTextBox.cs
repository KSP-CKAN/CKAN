﻿using System;
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

        /// <summary>
        /// Creates a HintTextBox object.
        /// </summary>
        public HintTextBox()
        {
            InitializeComponent();
            ClearIcon.BringToFront();
        }

        /// <summary>
        /// When the icon is clicked, reset the textbox value.
        /// </summary>
        /// <param name="sender">The control sending the event</param>
        /// <param name="e">The event arguments</param>
        private void HintClearIcon_Click(object sender, EventArgs e)
        {
            Text = "";
        }

        /// <summary>
        /// Show the clear icon when the textbox has data
        /// </summary>
        /// <param name="sender">The control sending the event</param>
        /// <param name="e">The event arguments</param>
        private void HintTextBox_TextChanged(object sender, EventArgs e)
        {
            ClearIcon.Visible = (TextLength > 0) && !ReadOnly;
        }

        /// <summary>
        /// Adjust the position of the clear icon regardless of control size.
        /// </summary>
        /// <param name="sender">The control sending the event</param>
        /// <param name="e">The event arguments</param>
        private void HintTextBox_SizeChanged(object sender, EventArgs e)
        {
            if (ClearIcon.Image != null)
            {
                ClearIcon.Location = new Point(
                    // align with right edge of textbox minus 5px
                    Width - ClearIcon.Width - 5,
                    // need to divide these as decimals and drop back to int at the end
                    (int)Math.Ceiling(Height / 2d - (ClearIcon.Height / 2d))
                );
            }
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
            if (keyData == Keys.Escape && !string.IsNullOrEmpty(Text))
            {
                Text = "";
                return true;
            }

            try
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch
            {
                // The above throws on Mono for a top-level Control
                // (as opposed to a Form)
                return false;
            }
        }
    }
}
