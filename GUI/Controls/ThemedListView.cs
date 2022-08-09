using System.Drawing;
using System.Windows.Forms;

namespace CKAN.GUI
{
    /// <summary>
    /// A ListView that obeys system colors to look less awful in a dark theme
    /// </summary>
    public class ThemedListView : ListView
    {
        public ThemedListView() : base()
        {
            // Tell the base class that we want to draw things ourselves
            OwnerDraw = true;
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
