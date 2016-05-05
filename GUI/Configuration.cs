﻿using System.Drawing;
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

        public bool RefreshOnStartup = true; // Defaults to true, so everyone is forced to refresh on first start
        public bool RefreshOnStartupNoNag = false;

        public int ActiveFilter = 0;

        // Sort by the mod name (index = 2) column by default
        public int SortByColumnIndex = 2;
        public bool SortDescending = false;

        private string m_Path = "";
        private Point m_window_loc = new Point(0,0);
        //Workaround for bug which miss-sets the window location.
        // Here instead of in Main_FormClosing due to the misstaken 
        // value possibly being written out to config file. After some time
        // it should be save to move. RLake 2015/05
        public Point WindowLoc
        {
            get
            {
                if (m_window_loc.X < 0 && m_window_loc.Y < 0)
                {
                    m_window_loc = new Point(60, 30);
                }
                return m_window_loc;
            }
            set { m_window_loc = value; }
        }
        
        public Size WindowSize = new Size(1024, 500);

        public void Save()
        {
            SaveConfiguration(this, m_Path);
        }

        public static Configuration LoadOrCreateConfiguration(string path, string defaultRepo)
        {
            if (!File.Exists(path))
            {
                var configuration = new Configuration
                {
                    m_Path = path,
                        CommandLineArguments = Platform.IsUnix ? "./KSP.x86_64 -single-instance" :
                            Platform.IsMac  ? "./KSP.app/Contents/MacOS/KSP" :
                            "KSP_x64.exe -single-instance"
                };

                SaveConfiguration(configuration, path);
            }

            return LoadConfiguration(path);
        }

        public static Configuration LoadConfiguration(string path)
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

                    if(e is System.InvalidOperationException) // Exception thrown in Windows / .NET
                    {
                        if(e.InnerException != null)
                            additionalErrorData = ": " + e.InnerException.Message;
                    }
                    else if(e is System.Xml.XmlException) // Exception thrown in Mono
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

            configuration.m_Path = path;
            return configuration;
        }

        public static void SaveConfiguration(Configuration configuration, string path)
        {
            var serializer = new XmlSerializer(typeof (Configuration));

            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, configuration);
                writer.Close();
            }
        }
    }
}
