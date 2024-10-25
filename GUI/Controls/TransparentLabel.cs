using System;
using System.Drawing;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class TransparentLabel : Label
    {
        public TransparentLabel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.Opaque
                     | ControlStyles.AllPaintingInWmPaint,
                     true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            BackColor = Color.Transparent;
        }

        // If we use override instead of new, the nullability never matches (!)
        public new string? Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                Parent?.Invalidate(Bounds, false);
            }
        }

        // If we use override instead of new, the nullability never matches (!)
        public new ContentAlignment TextAlign
        {
            get => base.TextAlign;
            set
            {
                base.TextAlign = value;
                Parent?.Invalidate(Bounds, false);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= (int)WindowExStyles.WS_EX_TRANSPARENT;
                return cp;
            }
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            RecreateHandle();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Do nothing
        }
    }
}
