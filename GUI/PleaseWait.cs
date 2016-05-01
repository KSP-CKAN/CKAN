using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.String;

namespace CKAN
{
    public partial class PleaseWait : Form
    {
        internal class AutoUpdateUser : NullUser
        {
        }

        internal static AutoUpdateUser User = new AutoUpdateUser();

        public PleaseWait()
        {
            InitializeComponent();
            User.Progress += (f, p) => Util.Invoke(this, () => User_Progress(f, p));
            User.Message += (s, o) => Util.Invoke(this, ClearProgress);
            User.DownloadsComplete += (a, b, c) => Util.Invoke(this, ClearProgress);
        }

        private void User_Progress(string format, int percent)
        {
            progressBar1.Value = percent;
            progressBar1.Style = ProgressBarStyle.Continuous;
            label1.Text = Format(format, percent);
        }

        private void ClearProgress()
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            label1.Text = "Updating...";
        }
    }
}
