using System;

using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Not inheriting from ConsoleScreen because we don't
    /// want the standard header/footer/background.
    /// </summary>
    public class SplashScreen {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        /// <param name="mgr">Game instance manager object for getting instances</param>
        /// <param name="repoData">Repository data manager providing info from repos</param>
        public SplashScreen(GameInstanceManager mgr, RepositoryDataManager repoData)
        {
            manager = mgr;
            this.repoData = repoData;
        }

        /// <summary>
        /// Show the splash screen and wait for a key press.
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        public bool Run(ConsoleTheme theme)
        {
            // If there's a default instance, try to get the lock for it.
            GameInstance ksp = manager.CurrentInstance ?? manager.GetPreferredInstance();
            if (ksp != null
                && !GameInstanceListScreen.TryGetInstance(theme, ksp, repoData,
                                                          (ConsoleTheme th) => Draw(th, false),
                                                          new Progress<int>(p => drawProgressBar(theme, 22, 20, p)))) {
                Console.ResetColor();
                Console.Clear();
                Console.CursorVisible = true;
                return false;
            }
            // Draw screen with press any key
            Draw(theme, true);
            // Wait for a key
            Console.ReadKey(true);
            return true;
        }

        /// <summary>
        /// Draw a cool retro splash screen like IBM used to do.
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="pressAny">If true, ask user to press any key, otherwise say loading</param>
        private void Draw(ConsoleTheme theme, bool pressAny = false)
        {
            Console.CursorVisible = false;
            if (theme.SplashBg.HasValue)
            {
                Console.BackgroundColor = theme.SplashBg.Value;
            }
            else
            {
                Console.ResetColor();
            }
            Console.Clear();

            Console.ForegroundColor = theme.SplashAccentFg;

            string block = $"{Symbols.lowerHalfBlock}";

            drawCentered(1, "  ########  ####  #####     ######     #####    ####".Replace("#", block));
            drawCentered(2, " #########  #### ######   ##########   ######   ####".Replace("#", block));
            drawCentered(3, "####   ###   #######      ###    ###    ######  ### ".Replace("#", block));
            drawCentered(4, "####         ######       ##########    ####### ### ".Replace("#", block));
            drawCentered(5, "####         ######       ##########    ### ####### ".Replace("#", block));
            drawCentered(6, "####   ###   #######      ###    ###    ###  ###### ".Replace("#", block));
            drawCentered(7, " #########  #### ######  ####    ####  ####   ######".Replace("#", block));
            drawCentered(8, "  ########  ####  #####  ####    ####  ####    #####".Replace("#", block));

            drawCentered(10, "Comprehensive Kerbal Archive Network");

            Console.ForegroundColor = theme.SplashNormalFg;

            string horiz = $"{Symbols.horizLineDouble}";
            drawCentered(12, $"{Symbols.upperLeftCornerDouble}##################################################{Symbols.upperRightCornerDouble}".Replace("#", horiz));
            for (int ln = 13; ln <= 15; ++ln) {
                drawCentered(ln, $"{Symbols.vertLineDouble}                                                  {Symbols.vertLineDouble}");
            }
            drawCentered(14, $"Version {Meta.GetVersion()}");
            drawCentered(16, $"{Symbols.lowerLeftCornerDouble}##################################################{Symbols.lowerRightCornerDouble}".Replace("#", horiz));

            drawCentered(18, $"(C) Copyright the CKAN Authors 2014-{DateTime.Now.Year}");
            drawCentered(19, "https://github.com/KSP-CKAN/CKAN/graphs/contributors");

            if (pressAny) {
                drawCentered(21, Properties.Resources.SplashPressAnyKey);
            } else {
                drawCentered(21, Properties.Resources.SplashLoading);
            }
        }

        private void drawProgressBar(ConsoleTheme theme, int y, int w, int percent)
        {
            lock (progBarMutex)
            {
                try {
                    var doubleWidth = percent * (w - 2) / 50;
                    if (doubleWidth > lastProgDblW)
                    {
                        int lp = (Console.WindowWidth - w) / 2;
                        var bar = new string(Symbols.fullBlock, percent * (w - 2) / 100);
                        if ((doubleWidth & 1) == 1)
                        {
                            // "Cheat" an extra half character to make the bar more precise
                            bar += Symbols.leftHalfBlock;
                        }
                        // This can throw if the screen is too small
                        Console.SetCursorPosition(lp, y);
                        Console.ForegroundColor = theme.SplashNormalFg;
                        Console.Write("[");
                        Console.ForegroundColor = theme.SplashAccentFg;
                        Console.Write(bar.PadRight(w - 2, ' '));
                        Console.ForegroundColor = theme.SplashNormalFg;
                        Console.Write("]");
                        lastProgDblW = doubleWidth;
                    }
                } catch { }
            }
        }

        private void drawCentered(int y, string val)
        {
            int lp = (Console.WindowWidth - val.Length) / 2;
            try {
                // This can throw if the screen is too small
                Console.SetCursorPosition(lp, y);
                Console.Write(val);
            } catch { }
        }

        private readonly GameInstanceManager   manager;
        private readonly RepositoryDataManager repoData;
        private          int                   lastProgDblW = -1;
        private readonly object                progBarMutex = new object();
    }

}
