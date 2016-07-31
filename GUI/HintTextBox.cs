using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKAN
{
    public partial class HintTextBox : TextBox
    {
        private PictureBox _clearIcon;

        public HintTextBox()
        {
            InitializeComponent();

            // set up clear icon state and handlers
            _clearIcon = new PictureBox();
            _clearIcon.BackColor = Color.Transparent;
            _clearIcon.Visible = false;
            _clearIcon.Cursor = Cursors.Hand;
            _clearIcon.Image = global::CKAN.Properties.Resources.textClear;
            _clearIcon.SizeMode = PictureBoxSizeMode.CenterImage;
            _clearIcon.Size = _clearIcon.Image.Size;
            _clearIcon.Click += HintClearIcon_Click;
            _clearIcon.Paint += ClearIcon_Paint;

            Controls.Add(_clearIcon);
            _clearIcon.Parent = this;
            _clearIcon.BringToFront();
        }

        private void ClearIcon_Paint(object sender, PaintEventArgs e)
        {
            var gfx = e.Graphics;
        }

        private void HintClearIcon_Click(object sender, EventArgs e)
        {
            Text = string.Empty;
        }

        private void HintTextBox_TextChanged(object sender, EventArgs e)
        {
            // sanity checks
            if (!Visible || ReadOnly)
            {
                return;
            }
            
            _clearIcon.Visible = (TextLength > 0);
        }

        private void HintTextBox_SizeChanged(object sender, EventArgs e)
        {
            if (_clearIcon.Image != null)
            {
                _clearIcon.Location = new Point(Width - _clearIcon.Image.Width - 1, (Height - _clearIcon.Image.Height) / 2);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Text = "";
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
