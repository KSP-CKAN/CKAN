using System;
using System.Linq;
using System.Windows.Forms;

using CKAN.Configuration;

namespace CKAN
{
    public partial class InstallFiltersDialog : Form
    {
        public InstallFiltersDialog(IConfiguration globalConfig, GameInstance instance)
        {
            InitializeComponent();
            this.globalConfig = globalConfig;
            this.instance     = instance;
        }

        private void InstallFiltersDialog_Load(object sender, EventArgs e)
        {
            GlobalFiltersTextBox.Text = string.Join(Environment.NewLine, globalConfig.GlobalInstallFilters);
            InstanceFiltersTextBox.Text = string.Join(Environment.NewLine, instance.InstallFilters);
            GlobalFiltersTextBox.DeselectAll();
            InstanceFiltersTextBox.DeselectAll();
        }

        private void InstallFiltersDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

        private IConfiguration globalConfig;
        private GameInstance   instance;

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
