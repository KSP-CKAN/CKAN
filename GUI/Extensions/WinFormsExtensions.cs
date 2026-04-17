using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [ExcludeFromCodeCoverage]
    public static class WinFormsExtensions
    {
        /// <summary>
        /// GetControlFromPosition returns null if Control.Visible == false.
        /// This doesn't.
        /// </summary>
        /// <param name="panel">The table to inspect</param>
        /// <param name="column">The column of the control to retrieve</param>
        /// <param name="row">the row of the control to retrieve</param>
        /// <returns>The control at the given cell of the table</returns>
        public static Control? GetControlFromPositionEvenIfInvisible(this TableLayoutPanel panel,
                                                                     int                   column,
                                                                     int                   row)
            => panel.Controls.OfType<Control>()
                             .FirstOrDefault(ctl => panel.GetPositionFromControl(ctl)
                                                    is TableLayoutPanelCellPosition pos
                                                    && pos.Column == column
                                                    && pos.Row    == row);

        #region Icon sizing

        public static int ScaledWidth(this Graphics g, int w)
            => w * (int)g.DpiX / 96;

        public static int ScaledHeight(this Graphics g, int h)
            => h * (int)g.DpiY / 96;

        public static Size ScaledSize(this Graphics g, int w, int h)
            => new Size(g.ScaledWidth(w), g.ScaledHeight(h));

        public static Size ScaledSize(this Graphics g, Size size)
            => g.ScaledSize(size.Width, size.Height);

        #endregion

        public static Font Scale(this Font f, int dpi)
            => new Font(f.FontFamily, f.Size * dpi / 96, f.Style);

        public static void ScaleFonts(this Control control)
        {
            if (Platform.IsMono
                && (int)control.CreateGraphics().DpiX is int dpi
                && dpi != 96)
            {
                control.Font = control.Font.Scale(dpi);
                if (control is DataGridView grid)
                {
                    grid.DefaultCellStyle.Font = grid.DefaultCellStyle.Font.Scale(dpi);
                    grid.ColumnHeadersDefaultCellStyle.Font = grid.ColumnHeadersDefaultCellStyle.Font.Scale(dpi);
                }
            }
        }

        public static void ScaleFonts(this ToolStripItem item)
        {
            if (Platform.IsMono
                && (int)Graphics.FromImage(new Bitmap(1, 1)).DpiX is int dpi
                && dpi != 96)
            {
                item.Font = item.Font.Scale(dpi);
            }
        }

        public static IEnumerable<string> WordWrap(this Graphics g,
                                                   string        orig,
                                                   float         maxPixelWidth,
                                                   Font?         font = null,
                                                   string        delim = " ")
        {
            font ??= SystemFonts.DefaultFont;
            var delims = new string[] { delim };
            var delimWidth = g.MeasureString(delim, font).Width;
            foreach (var line in orig.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None))
            {
                var piece      = "";
                var pieceWidth = 0f;
                foreach (var word in line.Split(delims, StringSplitOptions.None))
                {
                    var wordWidth = g.MeasureString(word, font).Width;
                    if (pieceWidth + (pieceWidth > 0 ? delimWidth : 0) + g.MeasureString(word, font).Width < maxPixelWidth)
                    {
                        piece      += (pieceWidth > 0 ? delim      : "") + word;
                        pieceWidth += (pieceWidth > 0 ? delimWidth :  0) + wordWidth;
                    }
                    else
                    {
                        yield return piece;
                        piece      = word;
                        pieceWidth = wordWidth;
                    }
                }
                if (piece is { Length: > 0 })
                {
                    yield return piece;
                }
            }
        }
    }
}
