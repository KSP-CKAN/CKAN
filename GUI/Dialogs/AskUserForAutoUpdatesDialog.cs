using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [ExcludeFromCodeCoverage]
    public partial class AskUserForAutoUpdatesDialog : Form
    {
        public AskUserForAutoUpdatesDialog()
        {
            InitializeComponent();
            this.ScaleFonts();
        }
    }
}
