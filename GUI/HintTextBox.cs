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
        public HintTextBox()
        {
            InitializeComponent();
        }

        private void HintTextBox_TextChanged(object sender, EventArgs e)
        {
            // sanity checks
            if (!Visible || ReadOnly)
            {
                return;
            }

            if (TextLength <= 0)
            {
                return;
            }

        }
    }
}
