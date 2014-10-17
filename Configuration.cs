using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace CKAN
{

    public class Configuration
    {

        public string Repository = "";

        private string m_Path = "";

        public void Save()
        {
            Configuration.SaveConfiguration(this, m_Path);
        }

        public static Configuration LoadOrCreateConfiguration(string path, string defaultRepo)
        {
            if (!System.IO.File.Exists(path))
            {
                Configuration configuration = new Configuration();
                configuration.Repository = defaultRepo;
                configuration.m_Path = path;
                SaveConfiguration(configuration, path);
            }

            return LoadConfiguration(path);
        }

        public static Configuration LoadConfiguration(string path)
        {
            var serializer = new XmlSerializer(typeof(Configuration));
            var configuration = (Configuration)serializer.Deserialize(new System.IO.StreamReader(path));
            configuration.m_Path = path;
            return configuration;
        }

        public static void SaveConfiguration(Configuration configuration, string path)
        {
            var serializer = new XmlSerializer(typeof(Configuration));
            serializer.Serialize(new System.IO.StreamWriter(path), configuration);
        }
    
    }

}
