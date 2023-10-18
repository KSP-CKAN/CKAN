using System.Windows.Forms;

namespace CKAN.GUI
{
    /// <summary>
    /// Create a new TransparentTextBox control that allows the backcolor of textboxes to be transparent.
    /// <para>
    /// Either set the BackColor to Color.Transparent or the color of the parent container.
    /// Multiline is set to true.
    /// Used in <see cref="MainModInfo"/>.</para>
    /// </summary>
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
