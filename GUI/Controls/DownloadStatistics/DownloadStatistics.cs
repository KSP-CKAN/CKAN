using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class DownloadStatistics : UsableSplitContainer
    {
        public DownloadStatistics() : base()
        {
            Panel1.SuspendLayout();
            Panel2.SuspendLayout();
            SuspendLayout();

            BorderStyle      = BorderStyle.Fixed3D;
            Orientation      = Orientation.Vertical;
            SplitterWidth    = 6;
            SplitterDistance = Width * 2 / 3;

            pieChart = new DownloadStatisticsPieChart()
                       {
                           AutoSize = true,
                           Dock     = DockStyle.Fill,
                       };
            table    = new DownloadStatisticsTable()
                       {
                           AutoSize = true,
                           Dock     = DockStyle.Fill,
                           Padding  = new Padding(20, 0, 20, 0),
                       };
            Panel1.Controls.Add(pieChart);
            Panel2.Controls.Add(table);

            Panel1.ResumeLayout();
            Panel2.ResumeLayout();
            ResumeLayout();
        }

        public void SetData(NetModuleCache cache, Registry registry)
        {
            UseWaitCursor    = true;
            pieChart.Visible = false;
            table.Visible    = false;
            Task.Run(() =>
            {
                if (cache.CachedFileSizeByHost(registry.GetDownloadUrlsByHash())
                    is IReadOnlyDictionary<string, long> bytesPerHost)
                {
                    Util.Invoke(this, () =>
                    {
                        pieChart.SetData(bytesPerHost);
                        table.SetData(bytesPerHost);
                        UseWaitCursor    = false;
                        pieChart.Visible = true;
                        table.Visible    = true;
                    });
                }
            });
        }

        private readonly DownloadStatisticsPieChart pieChart;
        private readonly DownloadStatisticsTable    table;
    }
}
