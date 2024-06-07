using System;
using System.ComponentModel;
using System.Collections.Generic;

using Autofac;

using CKAN.ConsoleUI.Toolkit;
using CKAN.Configuration;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Screen for display and editing of authentication tokens.
    /// </summary>
    public class AuthTokenScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen.
        /// </summary>
        public AuthTokenScreen() : base()
        {
            mainMenu = new ConsolePopupMenu(new List<ConsoleMenuOption>() {
                new ConsoleMenuOption(Properties.Resources.AuthTokenListGitHubLink, "",
                    Properties.Resources.AuthTokenListGitHubLinkTip,
                    true, openGitHubURL)
            });

            AddObject(new ConsoleLabel(
                1, 2, -1,
                () => Properties.Resources.AuthTokenListLabel
            ));

            tokenList = new ConsoleListBox<string>(
                1, 4, -1, -2,
                new List<string>(ServiceLocator.Container.Resolve<IConfiguration>().GetAuthTokenHosts()),
                new List<ConsoleListBoxColumn<string>>() {
                    new ConsoleListBoxColumn<string>() {
                        Header   = Properties.Resources.AuthTokenListHostHeader,
                        Width    = 20,
                        Renderer = (string s) => s
                    },
                    new ConsoleListBoxColumn<string>() {
                        Header   = Properties.Resources.AuthTokenListTokenHeader,
                        Width    = null,
                        Renderer = (string s) => {
                            return ServiceLocator.Container.Resolve<IConfiguration>().TryGetAuthToken(s, out string token)
                                ? token
                                : Properties.Resources.AuthTokenListMissingToken;
                        }
                    }
                },
                0, 0, ListSortDirection.Descending
            );
            AddObject(tokenList);

            AddObject(new ConsoleLabel(
                3, -1, -1,
                () => Properties.Resources.AuthTokenListWarning,
                null,
                th => th.AlertFrameFg
            ));

            AddTip(Properties.Resources.Esc, Properties.Resources.Back);
            AddBinding(Keys.Escape, (object sender, ConsoleTheme theme) => false);

            tokenList.AddTip("A", Properties.Resources.Add);
            tokenList.AddBinding(Keys.A, (object sender, ConsoleTheme theme) => {
                AuthTokenAddDialog ad = new AuthTokenAddDialog();
                ad.Run(theme);
                DrawBackground(theme);
                tokenList.SetData(new List<string>(ServiceLocator.Container.Resolve<IConfiguration>().GetAuthTokenHosts()));
                return true;
            });

            tokenList.AddTip("R", Properties.Resources.Remove, () => tokenList.Selection != null);
            tokenList.AddBinding(Keys.R, (object sender, ConsoleTheme theme) => {
                if (tokenList.Selection != null) {
                    ServiceLocator.Container.Resolve<IConfiguration>().SetAuthToken(tokenList.Selection, null);
                    tokenList.SetData(new List<string>(ServiceLocator.Container.Resolve<IConfiguration>().GetAuthTokenHosts()));
                }
                return true;
            });
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader()
        {
            return $"{Meta.GetProductName()} {Meta.GetVersion()}";
        }

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
        {
            return Properties.Resources.AuthTokenListTitle;
        }

        private bool openGitHubURL(ConsoleTheme theme)
        {
            ModInfoScreen.LaunchURL(theme, githubTokenURL);
            return true;
        }

        private readonly ConsoleListBox<string> tokenList;

        private static readonly Uri githubTokenURL = new Uri("https://github.com/settings/tokens");
    }

}
