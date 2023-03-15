using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class DownloadStatisticsPieChart : PlotView
    {
        public DownloadStatisticsPieChart() : base()
        {
            Model = new PlotModel()
            {
                Title               = Properties.Resources.DownloadStatisticsPieChartTitle,
                TitleColor          = SystemColors.ControlText.ToOxyColor(),
                TextColor           = SystemColors.ControlText.ToOxyColor(),
                PlotAreaBorderColor = SystemColors.Control.ToOxyColor(),
            };
            Controller = new PlotController();
            // Get rid of the weird click-toooltip thing
            Controller.UnbindMouseDown(OxyMouseButton.Left);
        }

        public void SetData(IReadOnlyDictionary<string, long> bytesPerHost)
        {
            var totalSize = bytesPerHost.Values.Sum();
            var numLabels = bytesPerHost.Values.Count(size => size >= totalSize * 5 / 100) + 1;
            Model.Series.Clear();
            Model.Series.Add(new PieSeries()
            {
                Title               = Properties.Resources.DownloadStatisticsPieChartTitle,
                Diameter            = 0.75,
                TickRadialLength    = 25,
                StartAngle          = 180,
                InsideLabelFormat   = null,
                InsideLabelColor    = SystemColors.ControlText.ToOxyColor(),
                Stroke              = SystemColors.Control.ToOxyColor(),
                StrokeThickness     = 0,
                OutsideLabelFormat  = "{1}",
                Slices              = bytesPerHost.OrderByDescending(kvp => kvp.Value)
                                                  .Select((kvp, i) => (kvp, big: i < numLabels))
                                                  .GroupBy(tuple => tuple.big,
                                                           tuple => tuple.kvp)
                                                  .SelectMany(grp => grp.Key
                                                                     ? grp.Select(HostSlice)
                                                                     : Enumerable.Repeat(OtherSlice(grp), 1))
                                                  .ToList(),
            });
        }

        private static PieSlice HostSlice(KeyValuePair<string, long> kvp)
            => new PieSlice($"{Environment.NewLine}{kvp.Key}{Environment.NewLine}({CkanModule.FmtSize(kvp.Value)})",
                            kvp.Value);

        private static PieSlice OtherSlice(IEnumerable<KeyValuePair<string, long>> smallSlices)
            => new PieSlice("Other", smallSlices.Sum(kvp => kvp.Value));
    }
}
