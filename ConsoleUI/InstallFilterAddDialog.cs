using System;
using System.Collections.Generic;
using System.ComponentModel;
using CKAN.Versioning;
using CKAN.Games;
using CKAN.GameVersionProviders;
using CKAN.ConsoleUI.Toolkit;
using Autofac;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Popup letting user enter an installation filter
    /// </summary>
    public class InstallFilterAddDialog : ConsoleDialog {

        /// <summary>
        /// Initialize the popup
        /// </summary>
        public InstallFilterAddDialog() : base()
        {
            int l = GetLeft(),
                r = GetRight();
            int t = GetTop(),
                b = t + 4;
            SetDimensions(l, t, r, b);

            manualEntry = new ConsoleField(
                l + 2, b - 2, r - 2
            ) {
                GhostText = () => "<Enter a filter>"
            };
            AddObject(manualEntry);
            manualEntry.AddTip("Enter", "Accept value");
            manualEntry.AddBinding(Keys.Enter, (object sender, ConsoleTheme theme) => {
                choice = manualEntry.Value;
                return false;
            });

            AddTip("Esc", "Cancel");
            AddBinding(Keys.Escape, (object sender, ConsoleTheme theme) => {
                choice = null;
                return false;
            });

            CenterHeader = () => "Add Filter";
        }

        /// <summary>
        /// Display the dialog and handle its interaction
        /// </summary>
        /// <param name="process">Function to control the dialog, default is normal user interaction</param>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <returns>
        /// User input
        /// </returns>
        public new string Run(ConsoleTheme theme, Action<ConsoleTheme> process = null)
        {
            base.Run(theme, process);
            return choice;
        }

        private ConsoleField manualEntry;
        private string       choice;
    }
}
