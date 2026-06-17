using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.Extensions;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class Metadata : UserControl
    {
        public Metadata()
        {
            InitializeComponent();
            ToolTip.ScaleFonts();
            staticRowCount = MetadataTable.RowCount;
        }

        public void UpdateModInfo(GUIMod gui_module)
        {
            CkanModule module = gui_module.Module;

            Util.Invoke(this, () =>
            {
                MetadataTable.SuspendLayout();
                VersionTextBox.Text = gui_module.LatestVersion.ToString();
                LicenseTextBox.Text = string.Join(", ", module.license);
                UpdateAuthorLinks(gui_module.Authors);

                IdentifierTextBox.Text = module.identifier;

                if (gui_module.DownloadCount is null or 0)
                {
                    DownloadCountLabel.Visible = false;
                    DownloadCountTextBox.Visible = false;
                }
                else
                {
                    DownloadCountLabel.Visible = true;
                    DownloadCountTextBox.Visible = true;
                    DownloadCountTextBox.Text = $"{gui_module.DownloadCount:N0}";
                }

                if (module.release_status is null or ReleaseStatus.stable)
                {
                    ReleaseLabel.Visible = false;
                    ReleaseStatusTextBox.Visible = false;
                }
                else
                {
                    ReleaseLabel.Visible = true;
                    ReleaseStatusTextBox.Visible = true;
                    ReleaseStatusTextBox.Text = module.release_status.LocalizeName();
                }

                var compatMod = gui_module.LatestCompatibleMod
                                ?? gui_module.LatestAvailableMod
                                ?? gui_module.Module;
                GameCompatibilityTextBox.Text = string.Format(
                    Properties.Resources.GUIModGameCompatibilityLong,
                    gui_module.GameCompatibility,
                    compatMod.version);

                if (module.replaced_by == null)
                {
                    ReplacementLabel.Visible = false;
                    ReplacementTextBox.Visible = false;
                }
                else
                {
                    ReplacementLabel.Visible = true;
                    ReplacementTextBox.Visible = true;
                    ReplacementTextBox.Text = module.replaced_by.ToString();
                }

                ClearResourceLinks();
                var res = module.resources;
                if (res != null)
                {
                    AddResourceLink(Properties.Resources.ModInfoHomepageLabel,              res.homepage);
                    AddResourceLink(Properties.Resources.ModInfoSpaceDockLabel,             res.spacedock);
                    AddResourceLink(Properties.Resources.ModInfoCurseLabel,                 res.curse);
                    AddResourceLink(Properties.Resources.ModInfoRepositoryLabel,            res.repository);
                    AddResourceLink(Properties.Resources.ModInfoBugTrackerLabel,            res.bugtracker);
                    AddResourceLink(Properties.Resources.ModInfoDiscussionsLabel,           res.discussions);
                    AddResourceLink(Properties.Resources.ModInfoContinuousIntegrationLabel, res.ci);
                    AddResourceLink(Properties.Resources.ModInfoLicenseLabel,               res.license);
                    AddResourceLink(Properties.Resources.ModInfoManualLabel,                res.manual);
                    AddResourceLink(Properties.Resources.ModInfoMetanetkanLabel,            res.metanetkan);
                    AddResourceLink(Properties.Resources.ModInfoRemoteAvcLabel,             res.remoteAvc);
                    AddResourceLink(Properties.Resources.ModInfoRemoteSWInfoLabel,          res.remoteSWInfo);
                    AddResourceLink(Properties.Resources.ModInfoStoreLabel,                 res.store);
                    AddResourceLink(Properties.Resources.ModInfoSteamStoreLabel,            res.steamstore);
                    AddResourceLink(Properties.Resources.ModInfoGogStoreLabel,              res.gogstore);
                    AddResourceLink(Properties.Resources.ModInfoEpicStoreLabel,             res.epicstore);
                }
                WrapRows();
                MetadataTable.ResumeLayout(true);
            });
        }

        public event Action<SavedSearch, bool>? OnChangeFilter;

        private void UpdateAuthorLinks(List<string> authors)
        {
            AuthorsPanel.SuspendLayout();
            AuthorsPanel.Controls.Clear();
            AuthorsPanel.Controls.AddRange(
                authors.Select((a, i) => AuthorLink(a, i < authors.Count - 1)).ToArray());
            AuthorsPanel.ResumeLayout(true);
        }

        private LinkLabel AuthorLink(string name, bool withComma)
        {
            var link = new LinkLabel()
            {
                AutoSize     = true,
                ForeColor    = SystemColors.GrayText,
                LinkColor    = SystemColors.GrayText,
                LinkBehavior = LinkBehavior.HoverUnderline,
                Margin       = new Padding(0, 0, 2, 2),
                Text         = withComma ? $"{name}," : name,
                LinkArea     = new LinkArea(0, name.Length),
                Tag          = name,
            };
            link.LinkClicked += OnAuthorClick;
            ToolTip.SetToolTip(link, Properties.Resources.FilterLinkToolTip);
            return link;
        }

        private void OnAuthorClick(object? sender, LinkLabelLinkClickedEventArgs? e)
        {
            if (sender is LinkLabel link
                && link.Text is string author)
            {
                var merge  = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
                OnChangeFilter?.Invoke(
                    new SavedSearch()
                    {
                        Name   = string.Format(Properties.Resources.AuthorSearchName, author),
                        Values = Enumerable.Repeat(ModSearch.FromAuthors(ModuleLabelList.ModuleLabels,
                                                                         Main.Instance!.CurrentInstance!,
                                                                         Enumerable.Repeat(author, 1)).Combined,
                                                   1)
                                           .OfType<string>()
                                           .ToList(),
                    },
                    merge);
            }
        }

        private void LinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs? e)
        {
            if (sender is LinkLabel lbl)
            {
                Util.HandleLinkClicked(lbl.Text, e);
            }
        }

        private void LinkLabel_KeyDown(object? sender, KeyEventArgs? e)
        {
            if (sender is LinkLabel lbl)
            {
                switch (e)
                {
                    case {KeyCode: Keys.Apps}:
                        Util.LinkContextMenu(lbl.Text, lbl);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void MetadataTable_Resize(object sender, EventArgs args)
        {
            MetadataTable.SuspendLayout();
            WrapRows();
            MetadataTable.ResumeLayout(true);
        }

        private void ClearResourceLinks()
        {
            for (int row = MetadataTable.RowCount - 1; row >= staticRowCount; --row)
            {
                RemovePanelControl(MetadataTable, 0, row);
                RemovePanelControl(MetadataTable, 1, row);
                MetadataTable.RowStyles.RemoveAt(row);
            }
            MetadataTable.RowCount = staticRowCount;
        }

        private static void RemovePanelControl(TableLayoutPanel panel, int col, int row)
        {
            var ctl = panel.GetControlFromPosition(col, row);
            if (ctl != null)
            {
                panel.Controls.Remove(ctl);
            }
        }

        private void AddResourceLink(string label, Uri? link)
        {
            if (link != null)
            {
                Label lbl = new Label()
                {
                    AutoSize  = true,
                    ForeColor = SystemColors.GrayText,
                    Text      = label,
                };
                LinkLabel llbl = new LinkLabel()
                {
                    AutoSize = true,
                    Margin   = new Padding(4),
                    TabStop  = true,
                    Text     = link.OriginalString,
                    Tag      = link,
                    // Lighter colors for dark mode
                    LinkColor = BackColor.LinkColorForBackColor(),
                };
                llbl.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabel_LinkClicked);
                llbl.KeyDown += new KeyEventHandler(LinkLabel_KeyDown);
                int row = MetadataTable.RowCount;
                MetadataTable.Controls.Add(lbl,  0, row);
                MetadataTable.Controls.Add(llbl, 1, row);
                MetadataTable.RowCount = row + 1;
                MetadataTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
        }

        private void WrapRows()
        {
            const int bottomMargin = 10;
            if (MetadataTable.GetColumnWidths() is { Length: >= 2 } colWidths)
            {
                for (int col = 0; col < MetadataTable.ColumnCount; ++col)
                {
                    for (int row = 0; row < MetadataTable.RowCount; ++row)
                    {
                        switch (MetadataTable.GetControlFromPosition(col, row))
                        {
                            case TextBox tb:
                                // Text boxes have internal padding that is about the size of the default label margin
                                tb.Margin = new Padding(tb.Margin.Left, 0, tb.Margin.Right, bottomMargin);
                                // Text boxes don't like using MaximumSize
                                tb.Size   = new Size(colWidths[col] - tb.Margin.Horizontal,
                                                     tb.StringHeight(colWidths[col] - tb.Margin.Horizontal));
                                break;
                            case FlowLayoutPanel flp:
                                flp.Margin      = new Padding(flp.Margin.Left, 0, 0, bottomMargin);
                                flp.MaximumSize = new Size(colWidths[col] - flp.Margin.Horizontal, 1000);
                                break;
                            case Control c:
                                c.Margin      = new Padding(c.Margin.Left, c.Margin.Top, c.Margin.Right, bottomMargin);
                                c.MaximumSize = new Size(colWidths[col] - c.Margin.Horizontal,
                                                         c.StringHeight(colWidths[col] - c.Margin.Horizontal) + c.Margin.Vertical);
                                break;
                        }
                    }
                }
            }
        }

        private readonly int staticRowCount;
    }
}
