using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using log4net.Core;
using log4net.Appender;

namespace CKAN.NetKAN
{
    [ExcludeFromCodeCoverage]
    public class QueueAppender : AppenderSkeleton
    {
        public QueueAppender() { }

        protected override void Append(LoggingEvent evt)
        {
            // Skip duplicate messages for better multi-kref handling
            if (!Warnings.Contains(evt.RenderedMessage))
            {
                Warnings.Add(evt.RenderedMessage);
            }
        }

        public readonly List<string> Warnings = new List<string>();
    }
}
