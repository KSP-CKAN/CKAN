using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
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
            SuspendLayout();

            label = new TransparentLabel()
            {
                ForeColor   = SystemColors.ControlText,
                Dock        = DockStyle.Fill,
                TextAlign   = ContentAlignment.MiddleCenter,
                Text        = "",
            };
            Controls.Add(label);

            ResumeLayout(false);
            PerformLayout();
        }

        [Bindable(false)]
        [Browsable(true)]
        [DefaultValue("")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        // If we use override instead of new, the nullability never matches (!)
        public new string? Text
        {
            get => label.Text;
            set => label.Text = value;
        }

        // If we use override instead of new, the nullability never matches (!)
        public new Font Font
        {
            get => label.Font;
            set => label.Font = value;
        }

        public ContentAlignment TextAlign
        {
            get => label.TextAlign;
            set => label.TextAlign = value;
        }

        private readonly TransparentLabel label;
    }
}
