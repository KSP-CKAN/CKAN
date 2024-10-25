using System;
using System.Timers;

namespace CKAN
{
    public class ByteRateCounter
    {
        public ByteRateCounter()
        {
            progressTimer.Elapsed += CalculateByteRate;
        }

        public long Size           { get;         set; }
        public long BytesLeft      { get;         set; }
        public long BytesPerSecond { get; private set; }

        public int  Percent
            => Size > 0 ? (int)(100 * (Size - BytesLeft) / Size) : 0;

        public TimeSpan TimeLeft
            => BytesPerSecond > 0 ? TimeSpan.FromSeconds(BytesLeft / BytesPerSecond)
                                  : TimeSpan.MaxValue;

        public string TimeLeftString
            => TimeLeft switch
            {
                {TotalHours:   >1 and var hours} tls
                    => string.Format(Properties.Resources.ByteRateCounterHoursMinutesSeconds,
                                     (int)hours, tls.Minutes, tls.Seconds),
                {TotalMinutes: >1 and var mins}  tls
                    => string.Format(Properties.Resources.ByteRateCounterMinutesSeconds,
                                     (int)mins, tls.Seconds),
                var tls
                    => string.Format(Properties.Resources.ByteRateCounterSeconds,
                                     tls.Seconds),
            };

        public string Summary =>
            BytesPerSecond > 0
                ? string.Format(Properties.Resources.ByteRateCounterRateSummary,
                                CkanModule.FmtSize(BytesPerSecond),
                                CkanModule.FmtSize(BytesLeft),
                                TimeLeftString,
                                Percent)
                : string.Format(Properties.Resources.ByteRateCounterSummary,
                                CkanModule.FmtSize(BytesLeft),
                                Percent);

        public void Start() => progressTimer.Start();
        public void Stop()  => progressTimer.Stop();

        private void CalculateByteRate(object? sender, ElapsedEventArgs args)
        {
            var now                = DateTime.Now;
            var timerSpan          = now - lastProgressUpdateTime;
            var startSpan          = now - startedAt;
            var bytesDownloaded    = Size - BytesLeft;
            var timerBytesChange   = bytesDownloaded - lastProgressUpdateSize;
            lastProgressUpdateSize = bytesDownloaded;
            lastProgressUpdateTime = now;

            var overallRate = bytesDownloaded  / startSpan.TotalSeconds;
            var timerRate   = timerBytesChange / timerSpan.TotalSeconds;
            BytesPerSecond  = (long)(0.5 * (overallRate + timerRate));
        }

        private readonly DateTime startedAt              = DateTime.Now;
        private          DateTime lastProgressUpdateTime = DateTime.Now;
        private          long     lastProgressUpdateSize = 0;
        private readonly Timer    progressTimer          = new Timer(intervalMs);
        private const    int      intervalMs             = 3000;
    }
}
