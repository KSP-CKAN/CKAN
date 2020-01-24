using System.Drawing;
using System.Windows.Forms;

namespace CKAN
{
    /// <summary>
    /// A TabControl that obeys system colors to look less awful in a dark theme
    /// </summary>
    public class ThemedTabControl : TabControl
    {
        public ThemedTabControl() : base()
        {
            // Tell the base class that we want to draw things ourselves
            DrawMode = TabDrawMode.OwnerDrawFixed;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // Background
            Rectangle bgRect = e.Bounds;
            bgRect.Inflate(-2, -1);
            bgRect.Offset(0, 1);
            e.Graphics.FillRectangle(new SolidBrush(BackColor), bgRect);
            // Text
            var tabPage = TabPages[e.Index];
            Rectangle rect = e.Bounds;
            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font,
                rect, tabPage.ForeColor);
            // Alert event subscribers
            base.OnDrawItem(e);
        }
    }
}
