using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace CKAN.NetKAN.Utils
{
    /// <summary>
    /// A message logged at Info level or above
    /// </summary>
    internal sealed class LogMessage
    {
        public LogMessage(string level, string logger, string message)
        {
            Level   = level;
            Logger  = logger;
            Message = message;
        }

        public string Level   { get; }
        public string Logger  { get; }
        public string Message { get; }

        /// <summary>
        /// Log this message again, from its original logger and at its original level
        /// </summary>
        public void Replay(Type callerStackBoundary)
        {
            var level = LogManager.GetRepository().LevelMap[Level] ?? log4net.Core.Level.Info;
            LogManager.GetLogger(Logger)
                      .Logger
                      .Log(callerStackBoundary, level, Message, null);
        }
    }

    /// <summary>
    /// Collects the messages at Info level or above logged by the current
    /// thread (or async flow) while the collector is open:
    ///
    /// <code>
    /// using (var collector = new LogCollector())
    /// {
    ///     ...
    ///     var messages = collector.Messages;
    /// }
    /// </code>
    /// </summary>
    internal sealed class LogCollector : IDisposable
    {
        public LogCollector()
        {
            EnsureAppenderAttached();
            parent        = current.Value;
            current.Value = this;
        }

        /// <summary>
        /// Snapshot of the messages collected so far, in the order they were logged
        /// </summary>
        public List<LogMessage> Messages
        {
            get
            {
                lock (messages)
                {
                    return messages.ToList();
                }
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;

            // Remove all parent entries that have already been disposed of.
            // This way if disposes happen out of order then older values will
            // eventually get cleaned up.
            var entry = current.Value;
            while (true)
            {
                if (entry is null || !entry.disposed)
                {
                    break;
                }

                entry = entry.parent;
            }

            current.Value = entry;
        }

        private void Add(LogMessage message)
        {
            lock (messages)
            {
                messages.Add(message);
            }
        }

        private static void EnsureAppenderAttached()
        {
            // Reconfiguring log4net can remove the root logger's appenders
            // so we need to recheck this every time.
            if (LogManager.GetRepository() is Hierarchy hierarchy
                && hierarchy.Root.GetAppender(AppenderName) == null)
            {
                lock (attachLock)
                {
                    if (hierarchy.Root.GetAppender(AppenderName) == null)
                    {
                        hierarchy.Root.AddAppender(appender);
                    }
                }
            }
        }

        private sealed class CollectorAppender : AppenderSkeleton
        {
            public CollectorAppender()
            {
                Name = AppenderName;
            }

            protected override void Append(LoggingEvent evt)
            {
                if (evt.Level is not Level level
                    || level < Level.Info
                    || evt.RenderedMessage is not string text)
                {
                    return;
                }

                var message = new LogMessage(level.Name, evt.LoggerName ?? "", text);
                for (var collector = current.Value; collector != null; collector = collector.parent)
                {
                    if (collector.disposed)
                    {
                        continue;
                    }

                    collector.Add(message);
                }
            }
        }

        private readonly List<LogMessage> messages = new List<LogMessage>();
        private readonly LogCollector?    parent;
        private volatile bool             disposed;

        private const string AppenderName = "LogCollector";

        private static readonly AsyncLocal<LogCollector?> current    = new AsyncLocal<LogCollector?>();
        private static readonly CollectorAppender         appender   = new CollectorAppender();
        private static readonly object                    attachLock = new object();
    }
}
