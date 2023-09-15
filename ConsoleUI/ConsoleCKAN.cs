using System;
using System.Linq;

using Autofac;

using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Class that runs the console UI in its constructor
    /// </summary>
    public class ConsoleCKAN {

        /// <summary>
        /// Run the console UI.
        /// Starts with a splash screen, then instance selection if no default,
        /// then list of mods.
        /// </summary>
        public ConsoleCKAN(GameInstanceManager mgr, string themeName, bool debug)
        {
            if (ConsoleTheme.Themes.TryGetValue(themeName ?? "default", out ConsoleTheme theme))
            {
                var repoData = ServiceLocator.Container.Resolve<RepositoryDataManager>();
                // GameInstanceManager only uses its IUser object to construct game instance objects,
                // which only use it to inform the user about the creation of the CKAN/ folder.
                // These aren't really intended to be displayed, so the manager
                // can keep a NullUser reference forever.
                GameInstanceManager manager = mgr ?? new GameInstanceManager(new NullUser());

                // The splash screen returns true when it's safe to run the rest of the app.
                // This can be blocked by a lock file, for example.
                if (new SplashScreen(manager, repoData).Run(theme)) {

                    if (manager.CurrentInstance == null) {
                        if (manager.Instances.Count == 0) {
                            // No instances, add one
                            new GameInstanceAddScreen(manager).Run(theme);
                            // Set instance to current if they added one
                            manager.GetPreferredInstance();
                        } else {
                            // Multiple instances, no default, pick one
                            new GameInstanceListScreen(manager, repoData).Run(theme);
                        }
                    }
                    if (manager.CurrentInstance != null) {
                        new ModListScreen(manager, repoData,
                                          RegistryManager.Instance(manager.CurrentInstance, repoData),
                                          manager.CurrentInstance.game,
                                          debug, theme).Run(theme);
                    }

                    new ExitScreen().Run(theme);
                }
            }
            else
            {
                Console.WriteLine(Properties.Resources.ThemeNotFound, themeName);
                Console.WriteLine(Properties.Resources.ThemeList, string.Join(", ",
                    ConsoleTheme.Themes.Keys.OrderBy(th => th)));
            }
        }

    }
}
