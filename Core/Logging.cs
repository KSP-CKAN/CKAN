using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using log4net;
using log4net.Config;
using log4net.Core;

namespace CKAN
{
    [ExcludeFromCodeCoverage]
    public static class Logging
    {
        public static void Initialize()
        {
            if (LogManager.GetRepository(Assembly.GetExecutingAssembly())
                is { Configured: false } repo)
            {
                // if the log4net.xml file does not exist, then fall back to the existing
                // configuration process
                var logConfig = new FileInfo(Path.Combine(Environment.CurrentDirectory, "log4net.xml"));
                if (!logConfig.Exists)
                {
                    BasicConfigurator.Configure(repo);
                    repo.Threshold = Level.Warn;
                }
                else
                {
                    // when the XML file exists, attempt to set up log4net using it
                    try
                    {
                        XmlConfigurator.ConfigureAndWatch(repo, logConfig);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception trying to configure logging!", e);

                        // attempt to fall back to basic
                        BasicConfigurator.Configure(repo);
                    }
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
