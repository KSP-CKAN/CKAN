using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace CKAN
{

    public class Configuration
    {
        public string CommandLineArguments = "";
        public bool AutoCloseWaitDialog = false;
        public bool URLHandlerNoNag = false;

        public bool CheckForUpdatesOnLaunch = false;
        public bool CheckForUpdatesOnLaunchNoNag = false;

        public bool EnableTrayIcon = false;
        public bool MinimizeToTray = false;

        public bool HideEpochs = true;
        public bool HideV = false;

        public bool RefreshOnStartup = true; // Defaults to true, so everyone is forced to refresh on first start
        public bool RefreshOnStartupNoNag = false;
        public bool RefreshPaused = false;

        public bool AutoSortByUpdate = true;

        public int ActiveFilter = 0;

        // Sort by the mod name (index = 2) column by default
        public int SortByColumnIndex = 2;
        public bool SortDescending = false;

        public bool[] VisibleColumns = { true, true, true, true, true, true, true, true, true };

        private string path = "";

        /// <summary>
        /// Stores whether main window was maximised or not
        /// <para> Value is the default - window not maximised</para>
        /// </summary>
        public bool IsWindowMaximised = false;

        private Point windowLocation = new Point(-1, -1);

        public Point WindowLoc
        {
            get { return windowLocation;  }
            set { windowLocation = value; }
        }

        /// <summary>
        /// Stores size of unmaximised main window
        /// <para> value is the default window size used where there is no GUIConfig.xml file</para>
        /// </summary>
        public Size WindowSize = new Size(1024, 500);

        /// <summary>
        /// Stores distance from left of the split between the Main Mod List and the metadata panels
        /// <para> value is the default position used where there is no GUIConfig.xml file</para>
        /// </summary>
        public int PanelPosition = 650;

        /// <summary>
        /// Stores distance from top of the split in the first metadata panel between the Name/description, and the mod details
        /// <para> value is the default position used where there is no GUIConfig.xml file</para>
        /// </summary>
        public int ModInfoPosition = 235;

        public void Save()
        {
            SaveConfiguration(this);
        }

        public static Configuration LoadOrCreateConfiguration(string path)
        {
            if (!File.Exists(path))
            {
                var configuration = new Configuration
                {
                    path = path,
                    CommandLineArguments = Platform.IsUnix ? "./KSP.x86_64 -single-instance" :
                            Platform.IsMac  ? "./KSP.app/Contents/MacOS/KSP" :
                            "KSP_x64.exe -single-instance"
                };

                SaveConfiguration(configuration);
            }

            return LoadConfiguration(path);
        }

        private static Configuration LoadConfiguration(string path)
        {
            var serializer = new XmlSerializer(typeof (Configuration));

            Configuration configuration;
            using (var stream = new StreamReader(path))
            {
                try
                {
                    configuration = (Configuration) serializer.Deserialize(stream);
                }
                catch (System.Exception e)
                {
                    string additionalErrorData = "";

                    if (e is System.InvalidOperationException) // Exception thrown in Windows / .NET
                    {
                        if (e.InnerException != null)
                            additionalErrorData = ": " + e.InnerException.Message;
                    }
                    else if (e is System.Xml.XmlException) // Exception thrown in Mono
                    {
                        additionalErrorData = ": " + e.Message;
                    }
                    else
                    {
                        throw;
                    }

                    string message = string.Format("Error trying to parse \"{0}\"{1}. Try to move it out of the folder and restart CKAN.", path, additionalErrorData);
                    throw new Kraken(message);
                }
            }

            configuration.path = path;
            return configuration;
        }

        private static void SaveConfiguration(Configuration configuration)
        {
            var serializer = new XmlSerializer(typeof (Configuration));

            using (var writer = new StreamWriter(configuration.path))
            {
                serializer.Serialize(writer, configuration);
                writer.Close();
            }
        }
    }
}
