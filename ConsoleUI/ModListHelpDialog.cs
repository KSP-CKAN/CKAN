using System;
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
            SetDimensions(9, 4, -9, -3);

            int btnW = 10;
            int btnL = (Console.WindowWidth - btnW) / 2;

            ConsoleTextBox symbolTb = new ConsoleTextBox(
                GetLeft() + 2, GetTop() + 2, Console.WindowWidth / 2 - 1, GetBottom() - 4,
                false,
                TextAlign.Center,
                () => ConsoleTheme.Current.PopupBg,
                () => ConsoleTheme.Current.PopupFg
            );
            AddObject(symbolTb);
            symbolTb.AddLine("Status Symbols");
            symbolTb.AddLine("==============");
            symbolTb.AddLine($"{installed}    Installed");
            symbolTb.AddLine($"{upgradable}  Upgradeable");
            symbolTb.AddLine($"!  Unavailable");
            symbolTb.AddLine(" ");
            symbolTb.AddLine("Basic Keys");
            symbolTb.AddLine("==========");
            symbolTb.AddLine("Tab            Move focus");
            symbolTb.AddLine("Cursor keys    Select row");
            symbolTb.AddLine("Escape       Clear search");

            ConsoleTextBox searchTb = new ConsoleTextBox(
                Console.WindowWidth / 2 + 1, GetTop() + 3, GetRight() - 2, GetBottom() - 4,
                false,
                TextAlign.Center,
                () => ConsoleTheme.Current.PopupBg,
                () => ConsoleTheme.Current.PopupFg
            );
            AddObject(searchTb);
            searchTb.AddLine("Special Searches");
            searchTb.AddLine("================");
            searchTb.AddLine("@author    Mods by author");
            searchTb.AddLine("~i         Installed mods");
            searchTb.AddLine("~u       Upgradeable mods");
            searchTb.AddLine("~dname     Depend on name");
            searchTb.AddLine("~cname   Conflict w/ name");
            searchTb.AddLine("~n               New mods");

            AddObject(new ConsoleButton(
                btnL, GetBottom() - 2, btnL + btnW - 1,
                "OK",
                Quit
            ));
        }

        private static readonly string installed    = Symbols.checkmark;
        private static readonly string upgradable   = Symbols.greaterEquals;
    }

}
