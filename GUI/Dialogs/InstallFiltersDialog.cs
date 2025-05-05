using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
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
            presets           = instance.game.InstallFilterPresets;
            const int hPadding = 6;
            int top = 17;
            GlobalFiltersGroupBox.Text = string.Format(Properties.Resources.InstallFiltersGlobalFiltersForGame,
                                                       instance.game.ShortName);
            foreach ((string name, string[] filters) in presets)
            {
                var btn = new Button()
                {
                    AutoSize                = true,
                    AutoSizeMode            = AutoSizeMode.GrowOnly,
                    Anchor                  = AnchorStyles.Top | AnchorStyles.Right,
                    FlatStyle               = FlatStyle.Flat,
                    Location                = new Point(GlobalFiltersTextBox.Right + hPadding, top),
                    Size                    = new Size(GlobalFiltersGroupBox.Width - GlobalFiltersTextBox.Right - 2 * hPadding, 23),
                    Text                    = string.Format(Properties.Resources.InstallFiltersAddPreset,
                                                            name),
                    Tag                     = name,
                    UseVisualStyleBackColor = true,
                };
                btn.Click += AddMiniAVCButton_Click;
                GlobalFiltersGroupBox.Controls.Add(btn);
                top += btn.Height * 4 / 3;
            }
        }

        public bool Changed { get; private set; } = false;

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

        private void InstallFiltersDialog_Load(object? sender, EventArgs? e)
        {
            GlobalFiltersTextBox.Text = string.Join(Environment.NewLine, globalConfig.GetGlobalInstallFilters(instance.game));
            InstanceFiltersTextBox.Text = string.Join(Environment.NewLine, instance.InstallFilters);
            GlobalFiltersTextBox.DeselectAll();
            InstanceFiltersTextBox.DeselectAll();
        }

        private void InstallFiltersDialog_Closing(object? sender, CancelEventArgs? e)
        {
            var newGlobal   = GlobalFiltersTextBox.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            var newInstance = InstanceFiltersTextBox.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            Changed = !globalConfig.GetGlobalInstallFilters(instance.game).SequenceEqual(newGlobal)
                      || !instance.InstallFilters.SequenceEqual(newInstance);
            if (Changed)
            {
                globalConfig.SetGlobalInstallFilters(instance.game, newGlobal);
                instance.InstallFilters           = newInstance;
            }
        }

        private void AddMiniAVCButton_Click(object? sender, EventArgs? e)
        {
            if (sender is Button b
                && b.Tag is string presetName
                && presets.TryGetValue(presetName, out string[]? filters))
            {
                GlobalFiltersTextBox.Text = string.Join(Environment.NewLine,
                    GlobalFiltersTextBox.Text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                        .Concat(filters)
                        .Distinct());
            }
        }

        private readonly IConfiguration                globalConfig;
        private readonly GameInstance                  instance;
        private readonly IDictionary<string, string[]> presets;

        private static readonly string[] delimiters = new string[]
        {
            Environment.NewLine
        };
    }
}
