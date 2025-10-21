using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

using Newtonsoft.Json;

using CKAN.IO;
using CKAN.Extensions;

namespace CKAN.GUI
{
    [XmlRoot("Configuration")]
    [JsonObject(MemberSerialization   = MemberSerialization.OptOut,
                ItemNullValueHandling = NullValueHandling.Ignore)]
    public class GUIConfiguration
    {
        public string? CommandLineArguments = null;

        [XmlArray, XmlArrayItem(ElementName = "CommandLine")]
        public List<string> CommandLines = new List<string>();

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool URLHandlerNoNag = false;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool CheckForUpdatesOnLaunch = false;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool CheckForUpdatesOnLaunchNoNag = false;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool EnableTrayIcon = false;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool MinimizeToTray = false;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(true)]
        public bool HideEpochs = true;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool HideV = false;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        // Defaults to true, so everyone is forced to refresh on first start
        [DefaultValue(true)]
        public bool RefreshOnStartup = true;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool RefreshOnStartupNoNag = false;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool RefreshPaused = false;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(true)]
        public bool AutoSortByUpdate = true;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool SuppressRecommendations = false;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(0)]
        public int ActiveFilter = 0;

        /// <summary>
        /// Name of the tag filter the user chose, if any
        /// </summary>
        public string? TagFilter = null;

        /// <summary>
        /// Name of the label filter the user chose, if any
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? CustomLabelFilter = null;

        [XmlArray, XmlArrayItem(ElementName = "Search")]
        public List<string>? DefaultSearches = null;

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

        /// <summary>
        /// Stores whether main window was maximised or not
        /// <para> Value is the default - window not maximised</para>
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
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
        [DefaultValue(650)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int PanelPosition = 650;

        public void Save(GameInstance instance)
        {
            JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented)
                       .WriteThroughTo(ConfigPath(instance));
        }

        public static DateTime LastWriteTime(GameInstance instance)
            => File.GetLastWriteTime(ConfigPath(instance));

        public static GUIConfiguration LoadOrCreateConfiguration(GameInstance instance,
                                                                 SteamLibrary steamLib)
            => LoadOrCreateConfiguration(instance,
                                         instance.Game
                                                 .DefaultCommandLines(steamLib, new DirectoryInfo(instance.GameDir))
                                                 .ToList());

        public static GUIConfiguration LoadOrCreateConfiguration(GameInstance instance,
                                                                 List<string> defaultCommandLines)
            => LoadJSON(ConfigPath(instance))
               ?? LoadXML(instance, defaultCommandLines)
               ?? new GUIConfiguration { CommandLines = defaultCommandLines };

        private const string filename       = "GUIConfig.json";
        private const string legacyFilename = "GUIConfig.xml";

        private static string ConfigPath(GameInstance inst)
            => Path.Combine(inst.CkanDir, filename);

        private static string LegacyConfigPath(GameInstance inst)
            => Path.Combine(inst.CkanDir, legacyFilename);

        private static GUIConfiguration? LoadJSON(string path)
            => Utilities.DefaultIfThrows(() =>
                   JsonConvert.DeserializeObject<GUIConfiguration>(
                       File.ReadAllText(path)));

        private static GUIConfiguration? LoadXML(GameInstance instance,
                                                 List<string> defaultCommandLines)
        {
            var serializer = new XmlSerializer(typeof(GUIConfiguration));
            var xmlFI = new FileInfo(LegacyConfigPath(instance));
            GUIConfiguration? configuration = null;
            try
            {
                using (var stream = new StreamReader(xmlFI.OpenRead()))
                {
                    configuration = serializer.Deserialize(stream) as GUIConfiguration;
                }
            }
            catch (Exception e) when (e is InvalidOperationException or XmlException)
            {
                throw new Kraken(
                    string.Format(Properties.Resources.ConfigurationParseError,
                                  xmlFI.FullName,
                                  e switch
                                  {
                                      // Exception thrown in Windows / .NET
                                      InvalidOperationException { InnerException: Exception inner }
                                                      => inner.Message,
                                      // Exception thrown in Mono
                                      XmlException xe => xe.Message,
                                      _               => "",
                                  },
                                  xmlFI.Name, xmlFI.DirectoryName),
                    e);
            }
            catch
            {
                return null;
            }
            if (configuration != null)
            {
                // KSPCompatibility column got renamed to GameCompatibility
                configuration.FixColumnName("KSPCompatibility", "GameCompatibility");

                // SizeCol column got renamed to DownloadSize
                configuration.FixColumnName("SizeCol", "DownloadSize");

                if (!string.IsNullOrEmpty(configuration.CommandLineArguments))
                {
                    configuration.CommandLines.AddRange(
                        Enumerable.Repeat(configuration.CommandLineArguments, 1)
                                  .Concat(defaultCommandLines)
                                  .OfType<string>()
                                  .Distinct());
                    configuration.CommandLineArguments = null;
                }
                else if (configuration.CommandLines.Count < 1)
                {
                    // Don't leave the list empty if user switches CKAN versions
                    configuration.CommandLines.AddRange(defaultCommandLines);
                }
                // Convert to JSON
                configuration.Save(instance);
                // Delete XML
                xmlFI.Delete();
            }
            return configuration;
        }

        private void FixColumnName(string oldName, string newName)
        {
            FixColumnName(SortColumns,       oldName, newName);
            FixColumnName(HiddenColumnNames, oldName, newName);
        }

        private static void FixColumnName(List<string> columnNames, string oldName, string newName)
        {
            int columnIndex = columnNames.IndexOf(oldName);
            if (columnIndex > -1)
            {
                columnNames[columnIndex] = newName;
            }
        }
    }

    [XmlRoot("SavedSearch")]
    [JsonObject(MemberSerialization   = MemberSerialization.OptOut,
                ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SavedSearch
    {
        public string       Name   = "";
        [XmlArray, XmlArrayItem(ElementName = "Search")]
        public List<string> Values = new List<string>();
    }
}
