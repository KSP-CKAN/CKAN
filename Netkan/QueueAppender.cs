using System.Collections.Generic;
using log4net.Core;
using log4net.Appender;

namespace CKAN.NetKAN
{
    public class QueueAppender : AppenderSkeleton
    {
        public QueueAppender() { }

        protected override void Append(LoggingEvent evt)
        {
            Warnings.Add(evt.RenderedMessage);
        }

        public List<string> Warnings = new List<string>();
    }
}
