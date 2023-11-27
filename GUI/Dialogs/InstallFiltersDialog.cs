using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.Configuration;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class InstallFiltersDialog : Form
    {
        public InstallFiltersDialog(IConfiguration globalConfig, GameInstance instance)
        {
            InitializeComponent();
            this.globalConfig = globalConfig;
            this.instance     = instance;
        }

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.Filters);
        }

        /// <summary>
        /// Open the user guide when the user clicks the help button
        /// </summary>
        protected override void OnHelpButtonClicked(CancelEventArgs evt)
        {
            evt.Cancel = Util.TryOpenWebPage(HelpURLs.Filters);
        }

        private void InstallFiltersDialog_Load(object sender, EventArgs e)
        {
            GlobalFiltersTextBox.Text = string.Join(Environment.NewLine, globalConfig.GlobalInstallFilters);
            InstanceFiltersTextBox.Text = string.Join(Environment.NewLine, instance.InstallFilters);
            GlobalFiltersTextBox.DeselectAll();
            InstanceFiltersTextBox.DeselectAll();
        }

        private void InstallFiltersDialog_Closing(object sender, CancelEventArgs e)
        {
            globalConfig.GlobalInstallFilters = GlobalFiltersTextBox.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            instance.InstallFilters = InstanceFiltersTextBox.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        }

        private void AddMiniAVCButton_Click(object sender, EventArgs e)
        {
            GlobalFiltersTextBox.Text = string.Join(Environment.NewLine,
                GlobalFiltersTextBox.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                    .Concat(miniAVC)
                    .Distinct()
            );
        }

        private readonly IConfiguration globalConfig;
        private readonly GameInstance   instance;

        private static readonly string[] delimiters = new string[]
        {
            Environment.NewLine
        };

        private static readonly string[] miniAVC = new string[]
        {
            "MiniAVC.dll",
            "MiniAVC.xml",
            "LICENSE-MiniAVC.txt",
            "MiniAVC-V2.dll",
            "MiniAVC-V2.dll.mdb",
        };
    }
}
