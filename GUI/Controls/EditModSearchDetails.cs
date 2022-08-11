using System;
using System.Drawing;
using System.Windows.Forms;
using log4net;

namespace CKAN.GUI
{
    /// <summary>
    /// A control for displaying and editing a search of mods.
    /// Contains several separate fields for searching different properties,
    /// plus a combined field that represents them all in a special syntax.
    /// </summary>
    public partial class EditModSearchDetails : UserControl
    {
        /// <summary>
        /// Initialize a mod search editing control
        /// </summary>
        public EditModSearchDetails()
        {
            InitializeComponent();
            // Float over other controls
            SetTopLevel(true);
        }

        /// <summary>
        /// Event fired when a search needs to be executed.
        /// The parameter is true if the search should be executed immediately,
        /// or false if the keystroke timer should be used.
        /// </summary>
        public event Action<bool> ApplySearch;

        /// <summary>
        /// Event fired when user wants to switch focus away from this control.
        /// </summary>
        public event Action SurrenderFocus;

        /// <summary>
        /// Override special settings to make this control behave like a dropdown.
        /// The "|=" lines are turning ON those flags.
        /// The "&amp;= ~" lines are turning OFF those flags.
        /// https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles
        /// https://docs.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                //  The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.
                cp.Style   &= ~(int)WindowStyles.WS_CHILD;
                // The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.
                cp.Style   &= ~(int)WindowStyles.WS_VISIBLE;
                // The window is a pop-up window. This style cannot be used with the WS_CHILD style.
                cp.Style   |= unchecked((int)WindowStyles.WS_POPUP);
                // The window is intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE.
                cp.ExStyle |= (int)WindowExStyles.WS_EX_TOOLWINDOW;
                // The window itself contains child windows that should take part in dialog box navigation. If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
                cp.ExStyle |= (int)WindowExStyles.WS_EX_CONTROLPARENT;
                // A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window. The window should not be activated through programmatic access or via keyboard navigation by accessible technology, such as Narrator. To activate the window, use the SetActiveWindow or SetForegroundWindow function. The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
                cp.ExStyle |= (int)WindowExStyles.WS_EX_NOACTIVATE;
                return cp;
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            if (SurrenderFocus != null)
            {
                SurrenderFocus();
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(EditModSearchDetails));

        private void FilterTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ApplySearch != null)
            {
                ApplySearch(string.IsNullOrEmpty((sender as TextBox)?.Text));
            }
        }

        private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Switch focus from filters to mod list on enter, down, or pgdn
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (ApplySearch != null)
                    {
                        ApplySearch(true);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    break;

                case Keys.Escape:
                    if (SurrenderFocus != null
                        && string.IsNullOrEmpty((sender as HintTextBox)?.Text ?? null))
                    {
                        SurrenderFocus();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    break;

                case Keys.Up:
                case Keys.Down:
                case Keys.PageUp:
                case Keys.PageDown:
                    if (SurrenderFocus != null)
                    {
                        SurrenderFocus();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void TriStateChanged(bool? val)
        {
            ApplySearch?.Invoke(true);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.Shift | Keys.F:
                    SurrenderFocus();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles
        private enum WindowStyles : uint
        {
            WS_VISIBLE = 0x10000000,
            WS_CHILD   = 0x40000000,
            WS_POPUP   = 0x80000000,
        }

        // https://docs.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles
        private enum WindowExStyles : uint
        {
            WS_EX_TOOLWINDOW    =       0x80,
            WS_EX_CONTROLPARENT =    0x10000,
            WS_EX_NOACTIVATE    = 0x08000000,
        }

    }
}
