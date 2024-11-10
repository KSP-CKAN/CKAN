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
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Font = SystemFonts.DefaultFont;
            Text = "";
        }

        [Bindable(false)]
        [Browsable(true)]
        [DefaultValue("")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        // If we use override instead of new, the nullability never matches (!)
        public new string Text {
            get => text;
            [MemberNotNull(nameof(text), nameof(textSize))]
            set
            {
                text     = value;
                textSize = TextRenderer.MeasureText(text, Font);
            }
        }

        [Bindable(false)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        // If we use override instead of new, the nullability never matches (!)
        public new Font Font { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            TextRenderer.DrawText(e.Graphics, Text, Font,
                                  new Point((Width  - textSize.Width)  / 2,
                                            (Height - textSize.Height) / 2),
                                  SystemColors.ControlText);
        }

        private string text;
        private Size   textSize;
    }
}
