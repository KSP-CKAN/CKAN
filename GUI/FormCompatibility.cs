using System.Drawing;
using System.Windows.Forms;

namespace CKAN.GUI
{
    /// <summary>
    /// Inheriting from this class ensures that forms are equally sized on Windows and on Linux/MacOSX
    /// Choose the form size so that it is the right one for Windows.
    /// </summary>
    public class FormCompatibility : Form
    {
        private const int formHeightDifference = 24;

        public void ApplyFormCompatibilityFixes()
        {
            if (!Platform.IsWindows)
            {
                ClientSize = new Size(ClientSize.Width, ClientSize.Height + formHeightDifference);
            }
        }
    }
}
