using System;
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
        public ConsoleCKAN(bool debug)
        {
            // KSPManager only uses its IUser object to construct KSP objects,
            // which only use it to inform the user about the creation of the CKAN/ folder.
            // These aren't really intended to be displayed, so the manager
            // can keep a NullUser reference forever.
            KSPManager manager = new KSPManager(new NullUser());

            // The splash screen returns true when it's safe to run the rest of the app.
            // This can be blocked by a lock file, for example.
            if (new SplashScreen(manager).Run()) {

                if (manager.CurrentInstance == null) {
                    new KSPListScreen(manager, true).Run();
                }
                if (manager.CurrentInstance != null) {
                    new ModListScreen(manager, debug).Run();
                }

                new ExitScreen().Run();
            }
        }

    }
}
