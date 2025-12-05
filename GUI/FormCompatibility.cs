using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// Inheriting from this class ensures that forms are equally sized on Windows and on Linux/MacOSX
    /// Choose the form size so that it is the right one for Windows.
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [ExcludeFromCodeCoverage]
    public abstract class FormCompatibility : Forms.Form
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
