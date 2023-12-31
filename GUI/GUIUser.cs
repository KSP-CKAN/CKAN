using System;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    /// <summary>
    /// The GUI implementation of the IUser interface.
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class GUIUser : IUser
    {
        public GUIUser(Main main,
                       Wait wait,
                       ToolStripStatusLabel statBarLabel,
                       ToolStripProgressBar statBarProgBar)
        {
            this.main           = main;
            this.wait           = wait;
            this.statBarLabel   = statBarLabel;
            this.statBarProgBar = statBarProgBar;
        }

        private readonly Main main;
        private readonly Wait wait;
        private readonly ToolStripStatusLabel statBarLabel;
        private readonly ToolStripProgressBar statBarProgBar;

        /// <summary>
        /// A GUIUser is obviously not headless. Returns false.
        /// </summary>
        [ForbidGUICalls]
        public bool Headless => false;

        /// <summary>
        /// Shows a small form with the question.
        /// User can select yes or no (ya dont say).
        /// </summary>
        /// <returns><c>true</c> if user pressed yes, <c>false</c> if no.</returns>
        /// <param name="question">Question.</param>
        [ForbidGUICalls]
        public bool RaiseYesNoDialog(string question)
            => main.YesNoDialog(question);

        /// <summary>
        /// Will show a small form with the message and a list to choose from.
        /// </summary>
        /// <returns>The index of the selection in the args array. 0-based!</returns>
        /// <param name="message">Message.</param>
        /// <param name="args">Array of offered options.</param>
        [ForbidGUICalls]
        public int RaiseSelectionDialog(string message, params object[] args)
            => main.SelectionDialog(message, args);

        /// <summary>
        /// Shows a message box containing the formatted error message.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="args">Arguments to format the message.</param>
        [ForbidGUICalls]
        public void RaiseError(string message, params object[] args)
        {
            main.ErrorDialog(message, args);
        }

        /// <summary>
        /// Sets the progress bars and the message box of the current WaitTabPage.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="percent">Progress in percent.</param>
        [ForbidGUICalls]
        public void RaiseProgress(string message, int percent)
        {
            Util.Invoke(main, () =>
            {
                statBarLabel.ToolTipText = message;
                // No newlines in status bar
                statBarLabel.Text = message.Replace("\r\n", " ")
                                           .Replace("\n",   " ");
                statBarProgBar.Value =
                    Math.Max(statBarProgBar.Minimum,
                        Math.Min(statBarProgBar.Maximum, percent));
                statBarProgBar.Style = ProgressBarStyle.Continuous;
                wait.SetMainProgress(message, percent);
            });
        }

        [ForbidGUICalls]
        public void RaiseProgress(int percent,
                                  long bytesPerSecond, long bytesLeft)
        {
            Util.Invoke(main, () =>
            {
                wait.SetMainProgress(percent, bytesPerSecond, bytesLeft);
                statBarProgBar.Value =
                    Math.Max(statBarProgBar.Minimum,
                        Math.Min(statBarProgBar.Maximum, percent));
                statBarProgBar.Style = ProgressBarStyle.Continuous;
            });
        }

        /// <summary>
        /// Displays the formatted message in the lower StatusStrip.
        /// Removes any newline strings.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="args">Arguments to fromat the message.</param>
        [ForbidGUICalls]
        public void RaiseMessage(string message, params object[] args)
        {
            var fullMsg = string.Format(message, args);
            Util.Invoke(main, () =>
            {
                statBarLabel.ToolTipText = fullMsg;
                // No newlines in status bar
                statBarLabel.Text = fullMsg.Replace("\r\n", " ")
                                           .Replace("\n",   " ");

                wait.AddLogMessage(fullMsg);
            });
        }
    }
}
