using System;
using System.Drawing;
using System.Windows.Forms;

namespace CKAN
{
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

            YesRadioButton = new RadioButton()
            {
                Appearance = Appearance.Button,
                FlatStyle  = FlatStyle.Flat,
                Location   = new Point(0, 0),
                Size       = new Size(33, 20),
                Image      = Properties.Resources.triToggleYes,
                UseVisualStyleBackColor = false,
            };
            YesRadioButton.CheckedChanged += RadioButtonChanged;
            ToolTip.SetToolTip(YesRadioButton, Properties.Resources.TriStateToggleYesTooltip);

            BothRadioButton = new RadioButton()
            {
                Appearance = Appearance.Button,
                BackColor  = SystemColors.Highlight,
                FlatStyle  = FlatStyle.Flat,
                Location   = new Point(33, 0),
                Size       = new Size(33, 20),
                Image      = Properties.Resources.triToggleBoth,
                Checked    = true,
                UseVisualStyleBackColor = false,
            };
            BothRadioButton.CheckedChanged += RadioButtonChanged;
            ToolTip.SetToolTip(BothRadioButton, Properties.Resources.TriStateToggleBothTooltip);

            NoRadioButton = new RadioButton()
            {
                Appearance = Appearance.Button,
                FlatStyle  = FlatStyle.Flat,
                Location   = new Point(66, 0),
                Size       = new Size(33, 20),
                Image      = Properties.Resources.triToggleNo,
                UseVisualStyleBackColor = false,
            };
            NoRadioButton.CheckedChanged += RadioButtonChanged;
            ToolTip.SetToolTip(NoRadioButton, Properties.Resources.TriStateToggleNoTooltip);

            Controls.Add(YesRadioButton);
            Controls.Add(BothRadioButton);
            Controls.Add(NoRadioButton);

            Size = new Size(99, 20);

            ResumeLayout(false);
            PerformLayout();
        }

        public bool? Value
        {
            get 
            {
                return YesRadioButton.Checked ? (bool?)true
                     : NoRadioButton.Checked  ? (bool?)false
                     :                          null;
            }
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
                butt.BackColor = RadioButton.DefaultBackColor;
            }
        }

        private ToolTip     ToolTip;
        private RadioButton YesRadioButton;
        private RadioButton BothRadioButton;
        private RadioButton NoRadioButton;
    }
}
