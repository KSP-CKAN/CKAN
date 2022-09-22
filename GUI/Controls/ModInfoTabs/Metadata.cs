using System;
using System.Drawing;
using System.Windows.Forms;

namespace CKAN.GUI
{
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

            Util.Invoke(MetadataModuleVersionTextBox, () => MetadataModuleVersionTextBox.Text = gui_module.LatestVersion.ToString());
            Util.Invoke(MetadataModuleLicenseTextBox, () => MetadataModuleLicenseTextBox.Text = string.Join(", ", module.license));
            Util.Invoke(MetadataModuleAuthorTextBox, () => MetadataModuleAuthorTextBox.Text = gui_module.Authors);
            Util.Invoke(MetadataIdentifierTextBox, () => MetadataIdentifierTextBox.Text = module.identifier);

            Util.Invoke(MetadataModuleReleaseStatusTextBox, () =>
            {
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
            });
            Util.Invoke(MetadataModuleGameCompatibilityTextBox, () => MetadataModuleGameCompatibilityTextBox.Text = gui_module.GameCompatibilityLong);

            Util.Invoke(ReplacementTextBox, () =>
            {
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
            });

            Util.Invoke(MetadataTable, () =>
            {
                ClearResourceLinks();
                var res = module.resources;
                if (res != null)
                {
                    AddResourceLink(Properties.Resources.ModInfoHomepageLabel,              res.homepage);
                    AddResourceLink(Properties.Resources.ModInfoSpaceDockLabel,             res.spacedock);
                    AddResourceLink(Properties.Resources.ModInfoCurseLabel,                 res.curse);
                    AddResourceLink(Properties.Resources.ModInfoRepositoryLabel,            res.repository);
                    AddResourceLink(Properties.Resources.ModInfoBugTrackerLabel,            res.bugtracker);
                    AddResourceLink(Properties.Resources.ModInfoContinuousIntegrationLabel, res.ci);
                    AddResourceLink(Properties.Resources.ModInfoLicenseLabel,               res.license);
                    AddResourceLink(Properties.Resources.ModInfoManualLabel,                res.manual);
                    AddResourceLink(Properties.Resources.ModInfoMetanetkanLabel,            res.metanetkan);
                    AddResourceLink(Properties.Resources.ModInfoRemoteAvcLabel,             res.remoteAvc);
                    AddResourceLink(Properties.Resources.ModInfoStoreLabel,                 res.store);
                    AddResourceLink(Properties.Resources.ModInfoSteamStoreLabel,            res.steamstore);
                }
            });
        }

        private GameInstanceManager manager => Main.Instance.manager;

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

        private int StringHeight(string text, Font font, int maxWidth)
            => (int)CreateGraphics().MeasureString(text, font, maxWidth).Height;

        private int LinkLabelStringHeight(LinkLabel lb, int fitWidth)
            => lb.Padding.Vertical + lb.Margin.Vertical + 10
                + StringHeight(lb.Text, lb.Font, fitWidth);

        private int LabelStringHeight(Label lb)
            => lb.Padding.Vertical + lb.Margin.Vertical + 10
                + StringHeight(lb.Text, lb.Font, lb.Width);

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
                        LabelStringHeight(lbl),
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
                            LabelStringHeight(lab),
                            LinkLabelStringHeight(link, rWidth));
                    }
                }
            }
        }

        private readonly int staticRowCount;
    }
}
