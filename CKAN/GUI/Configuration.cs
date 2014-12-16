﻿using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace CKAN
{

    public class Configuration
    {
        public string Repository = "";
        public string CommandLineArguments = "";
        public bool AutoCloseWaitDialog = false;

        // use set of ProfilesEntry instead of dictionary because of lack of serialization support
        public HashSet<ProfilesEntry> Profiles = new HashSet<ProfilesEntry>()
        {
            new ProfilesEntry { Name="default", ModIdentifiers=new HashSet<string>() }
        };
        public string ActiveProfileName = "default";

        private string m_Path = "";

        public void Save()
        {
            SaveConfiguration(this, m_Path);
        }

        public static Configuration LoadOrCreateConfiguration(string path, string defaultRepo)
        {
            if (!File.Exists(path))
            {
                var configuration = new Configuration {Repository = defaultRepo, m_Path = path};

                configuration.CommandLineArguments = Util.IsLinux ? "./KSP.x86_64" : "KSP.exe -force-opengl";

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
                configuration = (Configuration) serializer.Deserialize(stream);
                stream.Close();
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