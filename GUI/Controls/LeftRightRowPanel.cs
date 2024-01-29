using System.Windows.Forms;
using System.Drawing;

namespace CKAN.GUI
{
    /// <summary>
    /// A panel containing two groups of controls in flow layouts,
    /// one on the right side and one on the left.
    /// Intended to allow autosizing of Buttons.
    /// </summary>
    public class LeftRightRowPanel : TableLayoutPanel
    {
        /// <summary>
        /// Initialize the control.
        /// </summary>
        public LeftRightRowPanel()
        {
            LeftPanel = new FlowLayoutPanel()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                // Bottom-align the groups if one wraps and the other doesn't
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            };
            RightPanel = new FlowLayoutPanel()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                // Bottom-align the groups if one wraps and the other doesn't
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                // Right align the controls on the right
                FlowDirection = FlowDirection.RightToLeft,
            };

            // Let the outer control handle horizontal padding
            LeftPanel.Margin = new Padding(0, LeftPanel.Margin.Top,
                                           0, LeftPanel.Margin.Bottom);
            RightPanel.Margin = new Padding(0, RightPanel.Margin.Top,
                                            0, RightPanel.Margin.Bottom);

            // Don't overwrite graphics drawn on parent
            BackColor = LeftPanel.BackColor = RightPanel.BackColor = Color.Transparent;

            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            // Throw exceptions if the table gets bigger than a 2x1 layout
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize;

            ColumnCount = 2;
            ColumnStyles.Add(new ColumnStyle());
            ColumnStyles.Add(new ColumnStyle());

            RowCount = 1;
            RowStyles.Add(new RowStyle());

            Controls.Add(LeftPanel);
            Controls.Add(RightPanel);
        }

        /// <summary>
        /// Controls to be placed in the left half,
        /// first added will be at the left/top.
        /// </summary>
        public ControlCollection LeftControls  => LeftPanel.Controls;
        /// <summary>
        /// Controls to be placed in the right half,
        /// first added will be at the right/top.
        /// </summary>
        public ControlCollection RightControls => RightPanel.Controls;

        /// <summary>
        /// true if the borders of the panels should be shown, false to hide them.
        /// Useful for debugging.
        /// </summary>
        #pragma warning disable IDE0051
        private bool BordersVisible
        #pragma warning restore IDE0051
        {
            get => BorderStyle == BorderStyle.FixedSingle;
            set
            {
                if (value)
                {
                    BorderStyle = BorderStyle.FixedSingle;
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                    LeftPanel.BorderStyle = BorderStyle.FixedSingle;
                    RightPanel.BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    BorderStyle = BorderStyle.None;
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
                    LeftPanel.BorderStyle = BorderStyle.None;
                    RightPanel.BorderStyle = BorderStyle.None;
                }
            }
        }

        private readonly FlowLayoutPanel LeftPanel;
        private readonly FlowLayoutPanel RightPanel;
    }
}
