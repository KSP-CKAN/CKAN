using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

using CKAN.Games;

namespace CKAN
{
    [XmlRootAttribute("Configuration")]
    public class GUIConfiguration
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

        /// <summary>
        /// Name of the tag filter the user chose, if any
        /// </summary>
        public string TagFilter = null;

        /// <summary>
        /// Name of the label filter the user chose, if any
        /// </summary>
        public string CustomLabelFilter = null;

        [XmlArray, XmlArrayItem(ElementName = "Search")]
        public List<string> DefaultSearches = null;

        public List<string> SortColumns = new List<string>();
        public List<bool> MultiSortDescending = new List<bool>();

        [XmlArray, XmlArrayItem(ElementName = "ColumnName")]
        public List<string> HiddenColumnNames = new List<string>();

        /// <summary>
        /// Set whether a column name is in the visible list
        /// </summary>
        /// <param name="name">Name property of the column</param>
        /// <param name="vis">true if visible, false if hidden</param>
        public void SetColumnVisibility(string name, bool vis)
        {
            if (vis)
            {
                HiddenColumnNames.RemoveAll(n => n == name);
            }
            else if (!HiddenColumnNames.Contains(name))
            {
                HiddenColumnNames.Add(name);
            }
        }

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

        public static GUIConfiguration LoadOrCreateConfiguration(string path)
        {
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                var configuration = new GUIConfiguration
                {
                    path = path,
                    CommandLineArguments = new GameInstanceManager(new NullUser()).CurrentInstance.game.DefaultCommandLine
                };

                SaveConfiguration(configuration);
            }

            return LoadConfiguration(path);
        }

        private static GUIConfiguration LoadConfiguration(string path)
        {
            var serializer = new XmlSerializer(typeof (GUIConfiguration));

            GUIConfiguration configuration;
            using (var stream = new StreamReader(path))
            {
                try
                {
                    configuration = (GUIConfiguration) serializer.Deserialize(stream);
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

                    var fi = new FileInfo(path);
                    string message = string.Format(
                        "Error trying to parse \"{0}\"{1} Try to move {2} out of {3} and restart CKAN.",
                        fi.FullName, additionalErrorData, fi.Name, fi.DirectoryName);
                    throw new Kraken(message);
                }
            }

            configuration.path = path;
            if (DeserializationFixes(configuration))
            {
                SaveConfiguration(configuration);
            }
            return configuration;
        }

        /// <summary>
        /// Apply fixes and migrations after deserialization.
        /// </summary>
        /// <param name="configuration">The current configuration to apply the fixes on</param>
        /// <returns>A bool indicating whether something changed and the configuration should be saved to disk</returns>
        private static bool DeserializationFixes(GUIConfiguration configuration)
        {
            bool needsSave = false;

            // KSPCompatibility column got renamed to GameCompatibility
            needsSave = FixColumnName(configuration.SortColumns,       "KSPCompatibility", "GameCompatibility") || needsSave;
            needsSave = FixColumnName(configuration.HiddenColumnNames, "KSPCompatibility", "GameCompatibility") || needsSave;

            // SizeCol column got renamed to DownloadSize
            needsSave = FixColumnName(configuration.SortColumns,       "SizeCol", "DownloadSize") || needsSave;
            needsSave = FixColumnName(configuration.HiddenColumnNames, "SizeCol", "DownloadSize") || needsSave;

            return needsSave;
        }

        private static bool FixColumnName(List<string> columnNames, string oldName, string newName)
        {
            int columnIndex = columnNames.IndexOf("KSPCompatibility");
            if (columnIndex > -1)
            {
                columnNames[columnIndex] = "GameCompatibility";
                return true;
            }
            return false;
        }

        private static void SaveConfiguration(GUIConfiguration configuration)
        {
            var serializer = new XmlSerializer(typeof (GUIConfiguration));

            using (var writer = new StreamWriter(configuration.path))
            {
                serializer.Serialize(writer, configuration);
                writer.Close();
            }
        }
    }

    [XmlRootAttribute("SavedSearch")]
    public class SavedSearch
    {
        public string Name;
        [XmlArray, XmlArrayItem(ElementName = "Search")]
        public List<string> Values;
    }

}
