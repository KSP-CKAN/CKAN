using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// Custom toolstrip renderer that just fills the background with the BackColor.
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [ExcludeFromCodeCoverage]
    public class FlatToolStripRenderer : ToolStripProfessionalRenderer
    {
        public FlatToolStripRenderer()
            : base(new FlatToolStripColors())
        {
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            if (Platform.IsMono)
            {
                // The default Mono one uses a gradient between a hardcoded light color
                // and a system color that Mono doesn't load correctly.

                // Brushes need to be disposed
                using (SolidBrush bgBrush = new SolidBrush(e.BackColor))
                {
                    e.Graphics.FillRectangle(bgBrush, e.AffectedBounds);
                }
            }
            else
            {
                base.OnRenderToolStripBackground(e);
            }
        }
    }

    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [ExcludeFromCodeCoverage]
    public class FlatToolStripColors : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin  => base.ToolStripGradientBegin;
        public override Color ToolStripGradientMiddle => base.ToolStripGradientBegin;
        public override Color ToolStripGradientEnd    => base.ToolStripGradientBegin;

        public override Color MenuStripGradientBegin => base.MenuStripGradientBegin;
        public override Color MenuStripGradientEnd   => base.MenuStripGradientBegin;

        public override Color ToolStripPanelGradientBegin => base.ToolStripPanelGradientBegin;
        public override Color ToolStripPanelGradientEnd   => base.ToolStripPanelGradientBegin;

        public override Color ToolStripContentPanelGradientBegin => base.ToolStripContentPanelGradientBegin;
        public override Color ToolStripContentPanelGradientEnd   => base.ToolStripContentPanelGradientBegin;
    }
}
