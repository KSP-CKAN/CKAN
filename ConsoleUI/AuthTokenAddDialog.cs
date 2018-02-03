using System;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Popup for adding a new authentication token.
    /// </summary>
    public class AuthTokenAddDialog : ConsoleDialog {

        /// <summary>
        /// Initialize the popup.
        /// </summary>
        public AuthTokenAddDialog() : base()
        {
            CenterHeader = () => "Create Authentication Key";

            int top = (Console.WindowHeight - height) / 2;
            SetDimensions(6, top, -6, top + height - 1);

            int l = GetLeft(),
                r = GetRight(),
                t = GetTop(),
                b = GetBottom();

            AddObject(new ConsoleLabel(
                l + 2, t + 2, l + 2 + labelW,
                () => "Host:",
                () => ConsoleTheme.Current.PopupBg,
                () => ConsoleTheme.Current.PopupFg
            ));

            hostEntry = new ConsoleField(
                l + 2 + labelW + wPad, t + 2, r - 3
            ) {
                GhostText = () => "<Enter a host name>"
            };
            AddObject(hostEntry);

            AddObject(new ConsoleLabel(
                l + 2, t + 4, l + 2 + labelW,
                () => "Token:",
                () => ConsoleTheme.Current.PopupBg,
                () => ConsoleTheme.Current.PopupFg
            ));

            tokenEntry = new ConsoleField(
                l + 2 + labelW + wPad, t + 4, r - 3
            ) {
                GhostText = () => "<Enter an authentication token>"
            };
            AddObject(tokenEntry);

            AddTip("Esc", "Cancel");
            AddBinding(Keys.Escape, (object sender) => false);

            AddTip("Enter", "Accept", validKey);
            AddBinding(Keys.Enter, (object sender) => {
                if (validKey()) {
                    Win32Registry.SetAuthToken(hostEntry.Value, tokenEntry.Value);
                    return false;
                } else {
                    // Don't close window on Enter unless adding a key
                    return true;
                }
            });
        }

        private bool validKey()
        {
            string token;
            return hostEntry.Value.Length  > 0
                && tokenEntry.Value.Length > 0
                && Uri.CheckHostName(hostEntry.Value) != UriHostNameType.Unknown
                && !Win32Registry.TryGetAuthToken(hostEntry.Value, out token);
        }

        private ConsoleField hostEntry;
        private ConsoleField tokenEntry;

        private const int wPad   = 2;
        private const int labelW = 6;
        private const int height = 7;
    }

}
