using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using log4net;
using log4net.Config;
using log4net.Core;

namespace CKAN
{
    public class Logging
    {
        public static void Initialize()
        {
            if (LogManager.GetRepository(Assembly.GetExecutingAssembly()).Configured)
            {
                return;
            }

            // if the log4net.xml file does not exist, then fall back to the existing
            // configuration process
            var logConfig = new FileInfo(Path.Combine(Environment.CurrentDirectory, "log4net.xml"));
            if (!logConfig.Exists)
            {
                BasicConfigurator.Configure(LogManager.GetRepository(Assembly.GetExecutingAssembly()));
                LogManager.GetRepository(Assembly.GetExecutingAssembly()).Threshold = Level.Warn;
            }
            else
            {
                // when the XML file exists, attempt to set up log4net using it
                try
                {
                    XmlConfigurator.ConfigureAndWatch(LogManager.GetRepository(Assembly.GetExecutingAssembly()), logConfig);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception trying to configure logging!", e);

                    // attempt to fall back to basic
                    BasicConfigurator.Configure(LogManager.GetRepository(Assembly.GetExecutingAssembly()));
                }
            }
        }

#if DEBUG

        public static void WithTimeElapsed(Action<TimeSpan> elapsedCallback,
                                           Action           toMeasure)
        {
            var sw = new Stopwatch();
            sw.Start();

            toMeasure();

            sw.Stop();
            elapsedCallback(sw.Elapsed);
        }

        public static T WithTimeElapsed<T>(Action<TimeSpan> elapsedCallback,
                                           Func<T>          toMeasure)
        {
            var sw = new Stopwatch();
            sw.Start();

            T val = toMeasure();

            sw.Stop();
            elapsedCallback(sw.Elapsed);

            return val;
        }

#endif

    }
}
