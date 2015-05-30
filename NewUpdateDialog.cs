using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CKAN
{
    public partial class NewUpdateDialog : Form
    {
        public NewUpdateDialog(string version, string releaseNotes)
        {
            InitializeComponent();

            VersionLabel.Text = version;
            ReleaseNotesTextbox.Text = releaseNotes;
        }
    }
}
