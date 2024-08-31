using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// Create a new TransparentTextBox control that allows the backcolor of textboxes to be transparent.
    /// <para>
    /// Either set the BackColor to Color.Transparent or the color of the parent container.
    /// Multiline is set to true.
    /// Used in <see cref="MainModInfo"/>.</para>
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class TransparentTextBox : TextBox
    {
        public TransparentTextBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.AllPaintingInWmPaint,
                     true);
            Multiline = true;
        }
    }
}
