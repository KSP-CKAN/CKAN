using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace CKAN.GUI
{
    [XmlRoot("Configuration")]
    public class GUIConfiguration
    {
        public string CommandLineArguments = null;

        [XmlArray, XmlArrayItem(ElementName = "CommandLine")]
        public List<string> CommandLines = new List<string>();

        public bool URLHandlerNoNag = false;

        public bool CheckForUpdatesOnLaunch = false;
        public bool CheckForUpdatesOnLaunchNoNag = false;

        public bool EnableTrayIcon = false;
        public bool MinimizeToTray = false;

        public bool HideEpochs = true;
        public bool HideV = false;

        // Defaults to true, so everyone is forced to refresh on first start
        public bool RefreshOnStartup = true;
        public bool RefreshOnStartupNoNag = false;
        public bool RefreshPaused = false;

        public bool AutoSortByUpdate = true;

        public bool SuppressRecommendations = false;

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
            get => windowLocation;
            #pragma warning disable IDE0027
            set { windowLocation = value; }
            #pragma warning restore IDE0027
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

        public void Save()
        {
            if (!string.IsNullOrEmpty(path))
            {
                SaveConfiguration(this);
            }
        }

        public static GUIConfiguration LoadOrCreateConfiguration(string       path,
                                                                 List<string> defaultCommandLines)
        {
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                var configuration = new GUIConfiguration
                {
                    path         = path,
                    CommandLines = defaultCommandLines,
                };

                SaveConfiguration(configuration);
            }

            return LoadConfiguration(path, defaultCommandLines);
        }

        private static GUIConfiguration LoadConfiguration(string       path,
                                                          List<string> defaultCommandLines)
        {
            var serializer = new XmlSerializer(typeof(GUIConfiguration));

            GUIConfiguration configuration;
            using (var stream = new StreamReader(path))
            {
                try
                {
                    configuration = (GUIConfiguration) serializer.Deserialize(stream);
                }
                catch (Exception e)
                {
                    string additionalErrorData = "";

                    if (e is InvalidOperationException) // Exception thrown in Windows / .NET
                    {
                        if (e.InnerException != null)
                        {
                            additionalErrorData = ": " + e.InnerException.Message;
                        }
                    }
                    else if (e is XmlException) // Exception thrown in Mono
                    {
                        additionalErrorData = ": " + e.Message;
                    }
                    else
                    {
                        throw;
                    }

                    var fi = new FileInfo(path);
                    string message = string.Format(
                        Properties.Resources.ConfigurationParseError,
                        fi.FullName, additionalErrorData, fi.Name, fi.DirectoryName);
                    throw new Kraken(message);
                }
            }

            configuration.path = path;
            if (DeserializationFixes(configuration, defaultCommandLines))
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
        private static bool DeserializationFixes(GUIConfiguration configuration,
                                                 List<string>     defaultCommandLines)
        {
            bool needsSave = false;

            // KSPCompatibility column got renamed to GameCompatibility
            needsSave = FixColumnName(configuration.SortColumns,       "KSPCompatibility", "GameCompatibility") || needsSave;
            needsSave = FixColumnName(configuration.HiddenColumnNames, "KSPCompatibility", "GameCompatibility") || needsSave;

            // SizeCol column got renamed to DownloadSize
            needsSave = FixColumnName(configuration.SortColumns,       "SizeCol", "DownloadSize") || needsSave;
            needsSave = FixColumnName(configuration.HiddenColumnNames, "SizeCol", "DownloadSize") || needsSave;

            if (!string.IsNullOrEmpty(configuration.CommandLineArguments))
            {
                configuration.CommandLines.AddRange(
                    Enumerable.Repeat(configuration.CommandLineArguments, 1)
                              .Concat(defaultCommandLines)
                              .Distinct());
                configuration.CommandLineArguments = null;
                needsSave = true;
            }
            else if (configuration.CommandLines.Count < 1)
            {
                // Don't leave the list empty if user switches CKAN versions
                configuration.CommandLines.AddRange(defaultCommandLines);
                needsSave = true;
            }

            return needsSave;
        }

        private static bool FixColumnName(List<string> columnNames, string oldName, string newName)
        {
            int columnIndex = columnNames.IndexOf(oldName);
            if (columnIndex > -1)
            {
                columnNames[columnIndex] = newName;
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

    [XmlRoot("SavedSearch")]
    public class SavedSearch
    {
        public string Name;
        [XmlArray, XmlArrayItem(ElementName = "Search")]
        public List<string> Values;
    }

}
