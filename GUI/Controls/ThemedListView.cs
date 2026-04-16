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
            #if !NET10_0_OR_GREATER
            // Tell the base class that we want to draw things ourselves
            OwnerDraw = true;
            #endif

            // Don't flicker the entire listview when we change one row's background
            DoubleBuffered = true;
        }

        public void EnsureReadableGroupHeaders()
        {
            if (Platform.IsWindows && !BackColor.IsLight()
                && Groups.Count > 0)
            {
                // Windows forces ListViewGroup headers to use blue text, which is not readable on a dark background
                // 117,117,117 is the lightest dark gray, so it fits white text while also making the blue readable
                BackColor = Color.FromArgb(117, 117, 117);
            }
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
            if (Platform.IsMono
                && (int)e.Graphics.DpiX is int dpi
                && dpi != 96
                && e.Font is Font f)
            {
                var replacementEventArgs = new DrawListViewColumnHeaderEventArgs(
                                               e.Graphics, e.Bounds, e.ColumnIndex, e.Header,
                                               e.State, Util.ForeColorForBackColor(BackColor) ?? ForeColor, e.BackColor,
                                               f.Scale(dpi));
                replacementEventArgs.DrawText(TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
            else
            {
                var replacementEventArgs = new DrawListViewColumnHeaderEventArgs(
                                               e.Graphics, e.Bounds, e.ColumnIndex, e.Header,
                                               e.State, Util.ForeColorForBackColor(BackColor) ?? ForeColor, e.BackColor,
                                               e.Font);
                replacementEventArgs.DrawText(TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
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
