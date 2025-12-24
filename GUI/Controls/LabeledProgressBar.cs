using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// https://stackoverflow.com/a/40824778
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class LabeledProgressBar : ProgressBar
    {
        public LabeledProgressBar()
            : base()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.UserPaint,
                     true);
            Text = "";
        }

        [Bindable(false)]
        [Browsable(true)]
        [DefaultValue("")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        // If we use override instead of new, the nullability never matches (!)
        public new string Text {
            get => base.Text;
            [MemberNotNull(nameof(textSize))]
            set
            {
                base.Text = value;
                textSize  = TextRenderer.MeasureText(Text, Font);
            }
        }

        [Bindable(false)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        // If we use override instead of new, the nullability never matches (!)
        public new Font Font
        {
            get => base.Font;
            [MemberNotNull(nameof(textSize))]
            set
            {
                base.Font = value;
                textSize  = TextRenderer.MeasureText(Text, Font);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (ProgressBarRenderer.IsSupported)
            {
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, ClientRectangle);
                ProgressBarRenderer.DrawHorizontalChunks(e.Graphics,
                                                         new Rectangle(ClientRectangle.X,
                                                                       ClientRectangle.Y,
                                                                       ClientRectangle.Width * (Value   - Minimum)
                                                                                             / (Maximum - Minimum),
                                                                       ClientRectangle.Height));
            }
            else
            {
                const int borderWidth = 1;
                var innerRect = Rectangle.Inflate(ClientRectangle, -2 * borderWidth,
                                                                   -2 * borderWidth);
                innerRect.Offset(borderWidth, borderWidth);
                e.Graphics.DrawRectangle(SystemPens.ControlDark, ClientRectangle);
                e.Graphics.FillRectangle(SystemBrushes.Control, innerRect);
                e.Graphics.FillRectangle(SystemBrushes.Highlight,
                                         new Rectangle(innerRect.X,
                                                       innerRect.Y,
                                                       innerRect.Width * (Value   - Minimum)
                                                                       / (Maximum - Minimum),
                                                       innerRect.Height));
            }
            TextRenderer.DrawText(e.Graphics, Text, Font,
                                  new Point((Width  - textSize.Width)  / 2,
                                            (Height - textSize.Height) / 2),
                                  SystemColors.ControlText);
        }

        private Size textSize;
    }
}
