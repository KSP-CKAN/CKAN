using System;

using Autofac;

using CKAN.ConsoleUI.Toolkit;
using CKAN.Configuration;

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
            CenterHeader = () => Properties.Resources.AuthTokenAddTitle;

            int top = (Console.WindowHeight - height) / 2;
            SetDimensions(6, top, -6, top + height - 1);

            int l = GetLeft(),
                r = GetRight(),
                t = GetTop(),
                b = GetBottom();

            AddObject(new ConsoleLabel(
                l + 2, t + 2, l + 2 + labelW,
                () => Properties.Resources.AuthTokenAddHost,
                th => th.PopupBg,
                th => th.PopupFg
            ));

            hostEntry = new ConsoleField(
                l + 2 + labelW + wPad, t + 2, r - 3
            ) {
                GhostText = () => Properties.Resources.AuthTokenAddHostGhostText
            };
            AddObject(hostEntry);

            AddObject(new ConsoleLabel(
                l + 2, t + 4, l + 2 + labelW,
                () => Properties.Resources.AuthTokenAddToken,
                th => th.PopupBg,
                th => th.PopupFg
            ));

            tokenEntry = new ConsoleField(
                l + 2 + labelW + wPad, t + 4, r - 3
            ) {
                GhostText = () => Properties.Resources.AuthTokenAddTokenGhostText
            };
            AddObject(tokenEntry);

            AddTip(Properties.Resources.Esc, Properties.Resources.Cancel);
            AddBinding(Keys.Escape, (object sender, ConsoleTheme theme) => false);

            AddTip(Properties.Resources.Enter, Properties.Resources.Accept, validKey);
            AddBinding(Keys.Enter, (object sender, ConsoleTheme theme) => {
                if (validKey()) {
                    ServiceLocator.Container.Resolve<IConfiguration>().SetAuthToken(hostEntry.Value, tokenEntry.Value);
                    return false;
                } else {
                    // Don't close window on Enter unless adding a key
                    return true;
                }
            });
        }

        private bool validKey()
        {
            return hostEntry.Value.Length > 0
                && tokenEntry.Value.Length > 0
                && Uri.CheckHostName(hostEntry.Value) != UriHostNameType.Unknown
                && !ServiceLocator.Container.Resolve<IConfiguration>().TryGetAuthToken(hostEntry.Value, out _);
        }

        private readonly ConsoleField hostEntry;
        private readonly ConsoleField tokenEntry;

        private const int wPad   = 2;
        private int labelW => Math.Max(6, Math.Max(
            Properties.Resources.AuthTokenAddHost.Length,
            Properties.Resources.AuthTokenAddToken.Length
        ));
        private const int height = 7;
    }

}
