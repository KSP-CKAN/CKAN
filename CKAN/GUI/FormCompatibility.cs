using System.Drawing;
using System.Windows.Forms;

namespace CKAN
{
    public class FormCompatibility : Form
    {

        // inheriting from this class ensures that forms are equally sized on windows and on linux/ macosx
        private const int formHeightDifference = 24;

        public void ApplyFormCompatibilityFixes()
        {
            if (Util.IsLinux)
            {
                ClientSize = new Size(ClientSize.Width, ClientSize.Height + formHeightDifference);      
            }
        }

    }

}
