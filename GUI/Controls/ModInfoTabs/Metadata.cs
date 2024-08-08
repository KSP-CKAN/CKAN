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
            staticRowCount = MetadataTable.RowCount;
        }

        public void UpdateModInfo(GUIMod gui_module)
        {
            CkanModule module = gui_module.ToModule();

            Util.Invoke(this, () =>
            {
                MetadataModuleVersionTextBox.Text = gui_module.LatestVersion.ToString();
                MetadataModuleLicenseTextBox.Text = string.Join(", ", module.license);

                UpdateAuthorLinks(gui_module.Authors);

                MetadataIdentifierTextBox.Text = module.identifier;

                if (module.release_status == null)
                {
                    ReleaseLabel.Visible = false;
                    MetadataModuleReleaseStatusTextBox.Visible = false;
                    MetadataTable.LayoutSettings.RowStyles[3].Height = 0;
                }
                else
                {
                    ReleaseLabel.Visible = true;
                    MetadataModuleReleaseStatusTextBox.Visible = true;
                    MetadataTable.LayoutSettings.RowStyles[3].Height = 30;
                    MetadataModuleReleaseStatusTextBox.Text = module.release_status.ToString();
                }

                var compatMod = gui_module.LatestCompatibleMod
                                ?? gui_module.LatestAvailableMod
                                ?? gui_module.ToModule();
                MetadataModuleGameCompatibilityTextBox.Text = string.Format(
                    Properties.Resources.GUIModGameCompatibilityLong,
                    gui_module.GameCompatibility,
                    compatMod.version);

                if (module.replaced_by == null)
                {
                    ReplacementLabel.Visible = false;
                    ReplacementTextBox.Visible = false;
                    MetadataTable.LayoutSettings.RowStyles[6].Height = 0;
                }
                else
                {
                    ReplacementLabel.Visible = true;
                    ReplacementTextBox.Visible = true;
                    MetadataTable.LayoutSettings.RowStyles[6].Height = 30;
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
                }
            });
        }

        public event Action<SavedSearch, bool> OnChangeFilter;

        private void UpdateAuthorLinks(List<string> authors)
        {
            AuthorsPanel.SuspendLayout();
            AuthorsPanel.Controls.Clear();
            AuthorsPanel.Controls.AddRange(
                authors.Select(a => AuthorLink(a)).ToArray());
            AuthorsPanel.ResumeLayout();
        }

        private LinkLabel AuthorLink(string name)
        {
            var link = new LinkLabel()
            {
                AutoSize     = true,
                LinkColor    = SystemColors.GrayText,
                LinkBehavior = LinkBehavior.HoverUnderline,
                Margin       = new Padding(0, 0, 4, 4),
                Text         = name,
                Tag          = name,
            };
            link.LinkClicked += OnAuthorClick;
            ToolTip.SetToolTip(link, Properties.Resources.FilterLinkToolTip);
            return link;
        }

        private void OnAuthorClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var link   = sender as LinkLabel;
            var author = link.Text;
            var merge  = ModifierKeys.HasAnyFlag(Keys.Control, Keys.Shift);
            OnChangeFilter?.Invoke(
                new SavedSearch()
                {
                    Name   = string.Format(Properties.Resources.AuthorSearchName, author),
                    Values = new List<string>()
                    {
                        ModSearch.FromAuthors(Enumerable.Repeat(author, 1)).Combined
                    },
                },
                merge);
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.HandleLinkClicked((sender as LinkLabel).Text, e);
        }

        private void LinkLabel_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Apps:
                    Util.LinkContextMenu((sender as LinkLabel).Text);
                    e.Handled = true;
                    break;
            }
        }

        private int LinkLabelStringHeight(LinkLabel lb, int fitWidth)
            => lb.Padding.Vertical + lb.Margin.Vertical + 10
                + Util.StringHeight(CreateGraphics(), lb.Text, lb.Font, fitWidth);

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeResourceRows();
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

        private int RightColumnWidth
            => MetadataTable.Width
                - MetadataTable.Padding.Horizontal
                - MetadataTable.Margin.Horizontal
                - (int)MetadataTable.ColumnStyles[0].Width;

        private void AddResourceLink(string label, Uri link)
        {
            const int vPadding = 5;
            if (link != null)
            {
                Label lbl = new Label()
                {
                    AutoSize  = true,
                    Dock      = DockStyle.Fill,
                    ForeColor = SystemColors.GrayText,
                    Padding   = new Padding(0, vPadding, 0, vPadding),
                    Text      = label,
                };
                LinkLabel llbl = new LinkLabel()
                {
                    AutoSize = false,
                    Dock     = DockStyle.Fill,
                    Padding  = new Padding(0, vPadding, 0, vPadding),
                    TabStop  = true,
                    Text     = link.ToString(),
                };
                llbl.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabel_LinkClicked);
                llbl.KeyDown += new KeyEventHandler(LinkLabel_KeyDown);
                int row = MetadataTable.RowCount;
                MetadataTable.Controls.Add(lbl,  0, row);
                MetadataTable.Controls.Add(llbl, 1, row);
                MetadataTable.RowCount = row + 1;
                MetadataTable.RowStyles.Add(
                    new RowStyle(SizeType.Absolute, Math.Max(
                        // "Remote version file" wraps
                        Util.LabelStringHeight(CreateGraphics(), lbl),
                        LinkLabelStringHeight(llbl, RightColumnWidth))));
            }
        }

        private void ResizeResourceRows()
        {
            if (staticRowCount > 0)
            {
                var rWidth = RightColumnWidth;
                for (int row = staticRowCount; row < MetadataTable.RowStyles.Count; ++row)
                {
                    if (MetadataTable.GetControlFromPosition(0, row) is Label lab
                        && MetadataTable.GetControlFromPosition(1, row) is LinkLabel link)
                    {
                        MetadataTable.RowStyles[row].Height = Math.Max(
                            // "Remote version file" wraps
                            Util.LabelStringHeight(CreateGraphics(), lab),
                            LinkLabelStringHeight(link, rWidth));
                    }
                }
            }
        }

        private readonly int staticRowCount;
    }
}
