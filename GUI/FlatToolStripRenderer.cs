using System.Drawing;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// Custom toolstrip renderer that just fills the background with the BackColor.
    ///
    /// The default Mono one uses a gradient between a hardcoded light color and a system color that Mono doesn't load correctly.
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class FlatToolStripRenderer : ToolStripProfessionalRenderer
    {

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            // Brushes need to be disposed
            using (SolidBrush bgBrush = new SolidBrush(e.BackColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.AffectedBounds);
            }
        }

    }
}
