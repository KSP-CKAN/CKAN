using System;

using log4net;
using log4net.Core;

namespace Tests.Data
{
    public class TemporaryLogSuppressor : IDisposable
    {
        public TemporaryLogSuppressor()
        {
            orig = LogManager.GetRepository().Threshold;
            LogManager.GetRepository().Threshold = Level.Off;
        }

        public void Dispose()
        {
            LogManager.GetRepository().Threshold = orig;
            GC.SuppressFinalize(this);
        }

        private readonly Level orig;
    }
}
