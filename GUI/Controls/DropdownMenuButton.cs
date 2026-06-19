using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// Button with a Menu property that displays when you click
    /// Also shows a down-pointing triangle on the button
    ///
    /// Based on https://stackoverflow.com/a/24087828/2422988
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class DropdownMenuButton : Button
    {
        /// <summary>
        /// The menu to use for the dropdown
        /// </summary>
        [
            DefaultValue(null),
            Browsable(true),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)
        ]
        public ContextMenuStrip? Menu { get; set; }

        /// <summary>
        /// Draw the triangle on the button
        /// </summary>
        /// <param name="pevent">The paint event details</param>
        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            if (Menu != null)
            {
                var factor = Util.TextScaleFactor;
                int arrowX = ClientRectangle.Width - (int)(14 * factor),
                    arrowY = (ClientRectangle.Height / 2) - 1;

                pevent.Graphics.FillPolygon(
                    Enabled ? SystemBrushes.ControlText : SystemBrushes.ButtonShadow,
                    new Point[]
                    {
                        new Point(arrowX, arrowY),
                        new Point(arrowX + (int)(7 * factor), arrowY),
                        new Point(arrowX + (int)(3 * factor),
                                  arrowY + (int)(4 * factor))
                    }
                );
            }
        }

        /// <summary>
        /// Show the Menu on click
        /// </summary>
        /// <param name="mevent">The mouse event details</param>
        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);

            if (Menu != null && mevent.Button == MouseButtons.Left)
            {
                Menu.Show(this, new Point(0, Height));
            }
        }
    }
}
