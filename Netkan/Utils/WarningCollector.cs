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
    /// Collects the warnings logged by the current thread (or async flow)
    /// while the collector is open:
    ///
    /// <code>
    /// using (var collector = new WarningCollector())
    /// {
    ///     ...
    ///     var warnings = collector.Warnings;
    /// }
    /// </code>
    /// </summary>
    internal sealed class WarningCollector : IDisposable
    {
        public WarningCollector()
        {
            EnsureAppenderAttached();
            parent        = current.Value;
            current.Value = this;
        }

        /// <summary>
        /// Snapshot of the warnings collected so far, in the order they were logged
        /// </summary>
        public List<string> Warnings
        {
            get
            {
                lock (warnings)
                {
                    return warnings.ToList();
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

        private void Add(string message)
        {
            lock (warnings)
            {
                warnings.Add(message);
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
                if (evt.Level != Level.Warn || evt.RenderedMessage is not string message)
                {
                    return;
                }

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

        private readonly List<string>      warnings = new List<string>();
        private readonly WarningCollector? parent;
        private volatile bool              disposed;

        private const string AppenderName = "WarningCollector";

        private static readonly AsyncLocal<WarningCollector?> current    = new AsyncLocal<WarningCollector?>();
        private static readonly CollectorAppender             appender   = new CollectorAppender();
        private static readonly object                        attachLock = new object();
    }
}
