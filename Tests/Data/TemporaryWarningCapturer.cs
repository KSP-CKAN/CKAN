using System;
using System.Collections.Generic;

using log4net;
using log4net.Core;
using log4net.Filter;
using log4net.Repository.Hierarchy;

using CKAN.NetKAN;

namespace Tests.Data
{
    public class TemporaryWarningCapturer : IDisposable
    {
        public TemporaryWarningCapturer(string loggerName)
        {
            logRoot  = (LogManager.GetRepository() as Hierarchy)?.Root!;
            appender = GetWarningCapturer(loggerName);
            logRoot.AddAppender(appender);
        }

        public void Dispose()
        {
            logRoot.RemoveAppender(appender);
            GC.SuppressFinalize(this);
        }

        public IReadOnlyCollection<string> Warnings => appender.Warnings;

        private static QueueAppender GetWarningCapturer(string loggerName)
        {
            var qap = new QueueAppender() { Name = "TestWarningCapturer" };
            qap.AddFilter(new LevelMatchFilter()
            {
                LevelToMatch  = Level.Warn,
                AcceptOnMatch = true,
            });
            qap.AddFilter(new LoggerMatchFilter() {
                LoggerToMatch = loggerName,
                AcceptOnMatch = true,
            });
            qap.AddFilter(new DenyAllFilter());
            return qap;
        }

        private readonly QueueAppender appender;
        private readonly Logger        logRoot;
    }
}
