using System;
using System.Drawing;
using System.Windows.Forms;

namespace CKAN.GUI
{
    /// <summary>
    /// Custom toolstrip renderer that just fills the background with the BackColor.
    ///
    /// The default Mono one uses a gradient between a hardcoded light color and a system color that Mono doesn't load correctly.
    /// </summary>
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
