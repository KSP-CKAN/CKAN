using System;
using System.Linq;
using System.Text;

using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Dialog with help info for the main screen.
    /// Lists meaning of symbols, non-obvious key strokes, and special search syntaxes
    /// </summary>
    public class ModListHelpDialog : ConsoleDialog {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        public ModListHelpDialog() : base()
        {
            SetDimensions(9, 3, -9, -3);

            int btnW = 10;
            int btnL = (Console.WindowWidth - btnW) / 2;

            ConsoleTextBox symbolTb = new ConsoleTextBox(
                GetLeft() + 2, GetTop() + 2, (Console.WindowWidth / 2) - 1, GetBottom() - 4,
                false,
                TextAlign.Center,
                th => th.PopupBg,
                th => th.PopupFg
            );
            AddObject(symbolTb);
            symbolTb.AddLine(LeftRightTable(
                Properties.Resources.ModListHelpSymbolHeader,
                new Tuple<string, string>[] {
                    new Tuple<string, string>(installed,     Properties.Resources.ModListHelpInstalled),
                    new Tuple<string, string>(autoInstalled, Properties.Resources.ModListHelpAutoInstalled),
                    new Tuple<string, string>(upgradable,    Properties.Resources.ModListHelpUpgradeable),
                    new Tuple<string, string>(autodetected,  Properties.Resources.ModListHelpManuallyInstalled),
                    new Tuple<string, string>(replaceable,   Properties.Resources.ModListHelpReplaceable),
                    new Tuple<string, string>("!",           Properties.Resources.ModListHelpUnavailable),
                }
            ));
            symbolTb.AddLine(" ");
            symbolTb.AddLine(LeftRightTable(
                Properties.Resources.ModListHelpBasicKeysHeader,
                new Tuple<string, string>[] {
                    new Tuple<string, string>(Properties.Resources.Tab,        Properties.Resources.ModListHelpMoveFocus),
                    new Tuple<string, string>(Properties.Resources.CursorKeys, Properties.Resources.ModListHelpSelectRow),
                    new Tuple<string, string>(Properties.Resources.Esc,        Properties.Resources.ModListHelpClearSearch),
                }
            ));

            ConsoleTextBox searchTb = new ConsoleTextBox(
                (Console.WindowWidth / 2) + 1, GetTop() + 3, GetRight() - 2, GetBottom() - 4,
                false,
                TextAlign.Center,
                th => th.PopupBg,
                th => th.PopupFg
            );
            AddObject(searchTb);
            searchTb.AddLine(LeftRightTable(
                Properties.Resources.ModListHelpSpecialSearchesHeader,
                new Tuple<string, string>[] {
                    new Tuple<string, string>($"@{Properties.Resources.ModListHelpAuthor}", Properties.Resources.ModListHelpSearchAuthor),
                    new Tuple<string, string>("~i", Properties.Resources.ModListHelpSearchInstalled),
                    new Tuple<string, string>("~u", Properties.Resources.ModListHelpSearchUpgradeable),
                    new Tuple<string, string>($"~d{Properties.Resources.ModListHelpName}", Properties.Resources.ModListHelpSearchDepends),
                    new Tuple<string, string>($"~c{Properties.Resources.ModListHelpName}", Properties.Resources.ModListHelpSearchConflicts),
                    new Tuple<string, string>("~n", Properties.Resources.ModListHelpSearchNew),
                }
            ));

            AddObject(new ConsoleButton(
                btnL, GetBottom() - 2, btnL + btnW - 1,
                Properties.Resources.OK,
                Quit
            ));
        }

        private string LeftRightTable(string header, Tuple<string, string>[] rows)
        {
            int leftW  = rows.Max(r => r.Item1.Length);
            int rightW = rows.Max(r => r.Item2.Length);
            int fullW  = Math.Max(leftW + rightW + tableSpacing, header.Length);
            int midW   = fullW - leftW - rightW;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header);
            sb.AppendLine(new string('=', fullW));
            string mid = new string(' ', midW);
            foreach (var row in rows) {
                sb.AppendLine(row.Item1.PadRight(leftW) + mid + row.Item2.PadLeft(rightW));
            }
            return sb.ToString();
        }

        private const int tableSpacing = 2;

        private static readonly string installed     = Symbols.checkmark;
        private static readonly string autoInstalled = Symbols.feminineOrdinal;
        private static readonly string upgradable    = Symbols.greaterEquals;
        private static readonly string autodetected  = Symbols.infinity;
        private static readonly string replaceable   = Symbols.doubleGreater;
    }

}
