using System.Drawing;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// A ListView that obeys system colors to look less awful in a dark theme
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class ThemedListView : ListView
    {
        public ThemedListView() : base()
        {
            // Tell the base class that we want to draw things ourselves
            OwnerDraw = true;

            // Don't flicker the entire listview when we change one row's background
            DoubleBuffered = true;
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            // Background
            e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);
            // Borders at the bottom and between header cells
            Rectangle rect = e.Bounds;
            rect.Inflate(1, 0);
            rect.Offset(0, -1);
            e.Graphics.DrawRectangle(SystemPens.ControlDark, rect);
            // Text
            e.DrawText(TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            // Alert event subscribers
            base.OnDrawColumnHeader(e);
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            // Tell the base class that we changed our mind, it can draw the data rows
            e.DrawDefault = true;
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            // Tell the base class that we changed our mind, it can draw the data rows
            e.DrawDefault = true;
        }
    }
}
