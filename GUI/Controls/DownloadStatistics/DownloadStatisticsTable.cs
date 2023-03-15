using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class DownloadStatisticsTable : TableLayoutPanel
    {
        public DownloadStatisticsTable()
        {
            SuspendLayout();

            ColumnCount = 3;
            ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            ResumeLayout();

            tooltip = new ToolTip()
                      {
                          AutoPopDelay = 10000,
                          InitialDelay = 250,
                          ReshowDelay  = 250,
                          ShowAlways   = true,
                      };
        }

        public void SetData(IReadOnlyDictionary<string, long> bytesPerHost)
        {
            SuspendLayout();
            Controls.Clear();

            RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            int row = 1;
            foreach (var kvp in bytesPerHost.OrderByDescending(kvp => kvp.Value))
            {
                Controls.Add(new Label()
                             {
                                 AutoSize  = true,
                                 Dock      = DockStyle.Fill,
                                 Text      = kvp.Key,
                             },
                             0, row);
                Controls.Add(new Label()
                             {
                                 AutoSize  = true,
                                 Dock      = DockStyle.Fill,
                                 Margin    = new Padding(15, 0, 15, 5),
                                 Text      = CkanModule.FmtSize(kvp.Value),
                                 TextAlign = ContentAlignment.TopRight,
                             },
                             1, row);
                if (DonationLinksByHost.TryGetValue(kvp.Key,
                                                    out string? url))
                {
                    Controls.Add(MakeLink(Properties.Resources.DownloadStatisticsTableDonateLinkText,
                                          string.Format(Properties.Resources.DownloadStatisticsTableDonateLinkToolTip,
                                                        kvp.Key),
                                          url),
                                 2, row);
                }
                RowStyles.Add(new RowStyle(SizeType.AutoSize));
                RowCount = ++row;
            }

            RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            ++RowCount;

            ResumeLayout();
        }

        private LinkLabel MakeLink(string text,
                                   string tooltipText,
                                   string url)
        {
            var link = new LinkLabel()
                       {
                           AutoSize     = true,
                           Dock         = DockStyle.Fill,
                           LinkBehavior = LinkBehavior.HoverUnderline,
                           LinkVisited  = false,
                           Text         = text,
                           TabStop      = true,
                       };
            tooltip.SetToolTip(link, tooltipText);
            link.LinkClicked += (sender, e) => Util.HandleLinkClicked(url, e);
            link.KeyDown     += (sender, e) =>
                                {
                                    switch (e)
                                    {
                                        case { KeyCode: Keys.Enter or Keys.Space }:
                                            Util.OpenLinkFromLinkLabel(url);
                                            link.LinkVisited = true;
                                            e.Handled = true;
                                            break;

                                        case { KeyCode: Keys.Apps or Keys.Menu }:
                                            Util.LinkContextMenu(url, link);
                                            e.Handled = true;
                                            break;
                                    }
                                };
            return link;
        }

        private static readonly IReadOnlyDictionary<string, string> DonationLinksByHost = new Dictionary<string, string>
        {
            { "spacedock.info", "https://www.patreon.com/spacedock" },
            { "archive.org",    "https://archive.org/donate"        },
        };

        private readonly ToolTip tooltip;
    }
}
