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
    public class TriStateToggle : UserControl
    {
        public TriStateToggle()
        {
            SuspendLayout();

            ToolTip = new ToolTip()
            {
                AutoPopDelay = 10000,
                InitialDelay = 250,
                ReshowDelay  = 250,
                ShowAlways   = true,
            };

            YesRadioButton  = MakeRadioButton(0, EmbeddedImages.triToggleYes,
                Properties.Resources.TriStateToggleYesTooltip);
            BothRadioButton = MakeRadioButton(1, EmbeddedImages.triToggleBoth,
                Properties.Resources.TriStateToggleBothTooltip, true);
            NoRadioButton   = MakeRadioButton(2, EmbeddedImages.triToggleNo,
                Properties.Resources.TriStateToggleNoTooltip);
            Controls.Add(YesRadioButton);
            Controls.Add(BothRadioButton);
            Controls.Add(NoRadioButton);

            Size = new Size((3 * buttonXOffset) + 1, 20);

            ResumeLayout(false);
            PerformLayout();
        }

        public bool? Value
        {
            get => YesRadioButton.Checked ? true
                 : NoRadioButton.Checked ? (bool?)false
                 : null;
            set
            {
                if (!value.HasValue)
                {
                    BothRadioButton.Checked = true;
                }
                else if (value.Value)
                {
                    YesRadioButton.Checked = true;
                }
                else
                {
                    NoRadioButton.Checked = true;
                }
            }
        }

        public event Action<bool?> Changed;

        private RadioButton MakeRadioButton(int index, Bitmap icon, string tooltip, bool check = false)
        {
            var rb = new RadioButton()
            {
                Appearance = Appearance.Button,
                BackColor  = check ? SystemColors.Highlight : DefaultBackColor,
                FlatStyle  = FlatStyle.Flat,
                Location   = new Point(index * buttonXOffset, 0),
                Size       = new Size(buttonWidth, 20),
                Image      = icon,
                Checked    = check,
                UseVisualStyleBackColor = false,
            };
            rb.CheckedChanged += RadioButtonChanged;
            ToolTip.SetToolTip(rb, tooltip);
            return rb;
        }

        private void RadioButtonChanged(object sender, EventArgs e)
        {
            var butt = sender as RadioButton;
            // Will probably fire 3 times per click, only pass along one of them
            if (butt.Checked)
            {
                Changed?.Invoke(Value);
                butt.BackColor = SystemColors.Highlight;
            }
            else
            {
                butt.BackColor = DefaultBackColor;
            }
        }

        private const int buttonWidth   = 33;
        private const int buttonXOffset = buttonWidth - 1;

        private readonly ToolTip     ToolTip;
        private readonly RadioButton YesRadioButton;
        private readonly RadioButton BothRadioButton;
        private readonly RadioButton NoRadioButton;
    }
}
