using System;
using System.Collections.Generic;
using System.ComponentModel;

using CKAN.Versioning;
using CKAN.Games;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Popup letting user pick a game version from a list or enter it manually
    /// </summary>
    public class CompatibleVersionDialog : ConsoleDialog {

        /// <summary>
        /// Initialize the popup
        /// </summary>
        public CompatibleVersionDialog(IGame game) : base()
        {
            int l = GetLeft(),
                r = GetRight();
            int t = GetTop(),
                b = GetBottom();

            loadOptions(game);
            choices = new ConsoleListBox<GameVersion>(
                l + 2, t + 2, r - 2, b - 4,
                options,
                new List<ConsoleListBoxColumn<GameVersion>>() {
                    new ConsoleListBoxColumn<GameVersion>() {
                        Header   = Properties.Resources.CompatibleVersionsListHeader,
                        Width    = null,
                        Renderer = v => v.ToString(),
                        Comparer = (v1, v2) => v1.CompareTo(v2)
                    }
                },
                0, 0, ListSortDirection.Descending
            );
            AddObject(choices);
            choices.AddTip(Properties.Resources.Enter, Properties.Resources.CompatibleVersionsListAcceptTip);
            choices.AddBinding(Keys.Enter, (object sender, ConsoleTheme theme) => {
                choice = choices.Selection;
                return false;
            });

            manualEntry = new ConsoleField(
                l + 2, b - 2, r - 2
            ) {
                GhostText = () => Properties.Resources.CompatibleVersionsGhostText
            };
            AddObject(manualEntry);
            manualEntry.AddTip(Properties.Resources.Enter, Properties.Resources.CompatibleVersionsEntryAcceptTip, () => GameVersion.TryParse(manualEntry.Value, out choice));
            manualEntry.AddBinding(Keys.Enter, (object sender, ConsoleTheme theme) => {
                if (GameVersion.TryParse(manualEntry.Value, out choice)) {
                    // Good value, done running
                    return false;
                } else {
                    // Not valid, so they can't even see the key binding
                    return true;
                }
            });

            AddTip(Properties.Resources.Esc, Properties.Resources.Cancel);
            AddBinding(Keys.Escape, (object sender, ConsoleTheme theme) => {
                choice = null;
                return false;
            });

            CenterHeader = () => Properties.Resources.CompatibleVersionsTitle;
        }

        /// <summary>
        /// Display the dialog and handle its interaction
        /// </summary>
        /// <param name="process">Function to control the dialog, default is normal user interaction</param>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <returns>
        /// Row user selected
        /// </returns>
        public new GameVersion Run(ConsoleTheme theme, Action<ConsoleTheme> process = null)
        {
            base.Run(theme, process);
            return choice;
        }

        private void loadOptions(IGame game)
        {
            options = game.KnownVersions;
            // C# won't let us foreach over an array while modifying it
            for (int i = options.Count - 1; i >= 0; --i) {
                GameVersion v = options[i];
                // From GUI/CompatibleGameVersionsDialog.cs
                GameVersion fullKnownVersion = v.ToVersionRange().Lower.Value;
                GameVersion toAdd = new GameVersion(fullKnownVersion.Major, fullKnownVersion.Minor);
                if (!options.Contains(toAdd)) {
                    options.Add(toAdd);
                }
            }
        }

        private List<GameVersion> options;

        private readonly ConsoleListBox<GameVersion> choices;
        private readonly ConsoleField                manualEntry;
        private          GameVersion                 choice;
    }
}
