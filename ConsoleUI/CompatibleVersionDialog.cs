using System;
using System.Collections.Generic;
using System.ComponentModel;
using CKAN.Versioning;
using CKAN.GameVersionProviders;
using CKAN.ConsoleUI.Toolkit;
using Autofac;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Popup letting user pick a game version from a list or enter it manually
    /// </summary>
    public class CompatibleVersionDialog : ConsoleDialog {

        /// <summary>
        /// Initialize the popup
        /// </summary>
        public CompatibleVersionDialog() : base()
        {
            int l = GetLeft(),
                r = GetRight();
            int t = GetTop(),
                b = GetBottom();

            choices = new ConsoleListBox<KspVersion>(
                l + 2, t + 2, r - 2, b - 4,
                options,
                new List<ConsoleListBoxColumn<KspVersion>>() {
                    new ConsoleListBoxColumn<KspVersion>() {
                        Header   = "Predefined Version",
                        Width    = r - l - 5,
                        Renderer = v => v.ToString(),
                        Comparer = (v1, v2) => v1.CompareTo(v2)
                    }
                },
                0, 0, ListSortDirection.Descending
            );
            AddObject(choices);
            choices.AddTip("Enter", "Select version");
            choices.AddBinding(Keys.Enter, (object sender) => {
                choice = choices.Selection;
                return false;
            });

            manualEntry = new ConsoleField(
                l + 2, b - 2, r - 2
            ) {
                GhostText = () => "<Enter a version>"
            };
            AddObject(manualEntry);
            manualEntry.AddTip("Enter", "Accept value", () => KspVersion.TryParse(manualEntry.Value, out choice));
            manualEntry.AddBinding(Keys.Enter, (object sender) => {
                if (KspVersion.TryParse(manualEntry.Value, out choice)) {
                    // Good value, done running
                    return false;
                } else {
                    // Not valid, so they can't even see the key binding
                    return true;
                }
            });

            AddTip("Esc", "Cancel");
            AddBinding(Keys.Escape, (object sender) => {
                choice = null;
                return false;
            });

            CenterHeader = () => "Select Compatible Version";
        }

        /// <summary>
        /// Display the dialog and handle its interaction
        /// </summary>
        /// <param name="process">Function to control the dialog, default is normal user interaction</param>
        /// <returns>
        /// Row user selected
        /// </returns>
        public new KspVersion Run(Action process = null)
        {
            base.Run(process);
            return choice;
        }

        static CompatibleVersionDialog()
        {
            options = ServiceLocator.Container.Resolve<IKspBuildMap>().KnownVersions;
            // C# won't let us foreach over an array while modifying it
            for (int i = options.Count - 1; i >= 0; --i) {
                KspVersion v = options[i];
                // From GUI/CompatibleKspVersionsDialog.cs
                KspVersion fullKnownVersion = v.ToVersionRange().Lower.Value;
                KspVersion toAdd = new KspVersion(fullKnownVersion.Major, fullKnownVersion.Minor);
                if (!options.Contains(toAdd)) {
                    options.Add(toAdd);
                }
            }
        }

        private static List<KspVersion> options;

        private ConsoleListBox<KspVersion> choices;
        private ConsoleField               manualEntry;
        private KspVersion                 choice;
    }
}
