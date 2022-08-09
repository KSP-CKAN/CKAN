namespace CKAN.GUI
{
    /// <summary>
    /// Create a new TransparentTextBox control that allows the backcolor of textboxes to be transparent.
    /// <para>
    /// Either set the BackColor to Color.Transparent or the color of the parent container.
    /// Multiline is set to true.
    /// Used in <see cref="MainModInfo"/>.</para>
    /// </summary>
    public class TransparentTextBox : System.Windows.Forms.TextBox
    {
        public TransparentTextBox()
        {
            SetStyle(
                System.Windows.Forms.ControlStyles.SupportsTransparentBackColor |
                System.Windows.Forms.ControlStyles.ResizeRedraw |
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer |
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
            Multiline = true;
        }
    }
}
