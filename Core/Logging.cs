using System;
using System.IO;
using log4net;
using log4net.Config;

namespace CKAN
{
    public class Logging
    {
        public static void Initialize()
        {
            if (LogManager.GetRepository().Configured)
            {
                return;
            }

            // if the log4net.xml file does not exist, then fall back to the existing
            // configuration process
            var logConfig = new FileInfo(Path.Combine(Environment.CurrentDirectory, "log4net.xml"));
            if (!logConfig.Exists)
            {
                BasicConfigurator.Configure();
            }
            else
            {
                // when the XML file exists, attempt to set up log4net using it
                try
                {
                    XmlConfigurator.ConfigureAndWatch(logConfig);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception trying to configure logging!", e);

                    // attempt to fall back to basic
                    BasicConfigurator.Configure();
                }
            }
        }
    }
}
