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
    public partial class PreferredHostsDialog : Form
    {
        public PreferredHostsDialog(IConfiguration config, Registry registry)
        {
            InitializeComponent();
            this.config = config;
            allHosts    = registry.GetAllHosts().ToArray();
            placeholder = Properties.Resources.PreferredHostsPlaceholder;

            ToolTip.SetToolTip(MoveRightButton, Properties.Resources.PreferredHostsTooltipMoveRight);
            ToolTip.SetToolTip(MoveLeftButton,  Properties.Resources.PreferredHostsTooltipMoveLeft);
            ToolTip.SetToolTip(MoveUpButton,    Properties.Resources.PreferredHostsTooltipMoveUp);
            ToolTip.SetToolTip(MoveDownButton,  Properties.Resources.PreferredHostsTooltipMoveDown);
        }

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.PreferredHosts);
        }

        /// <summary>
        /// Open the user guide when the user clicks the help button
        /// </summary>
        protected override void OnHelpButtonClicked(CancelEventArgs evt)
        {
            evt.Cancel = Util.TryOpenWebPage(HelpURLs.PreferredHosts);
        }

        private void PreferredHostsDialog_Load(object sender, EventArgs e)
        {
            AvailableHostsListBox.Items.AddRange(allHosts
                .Except(config.PreferredHosts)
                .ToArray());
            PreferredHostsListBox.Items.AddRange(config.PreferredHosts
                .Select(host => host ?? placeholder)
                .ToArray());
            AvailableHostsListBox_SelectedIndexChanged(null, null);
            PreferredHostsListBox_SelectedIndexChanged(null, null);
        }

        private void PreferredHostsDialog_Closing(object sender, CancelEventArgs e)
        {
            config.PreferredHosts = PreferredHostsListBox.Items.Cast<string>()
                .Select(h => h == placeholder ? null : h)
                .ToArray();
        }

        private void AvailableHostsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            MoveRightButton.Enabled = AvailableHostsListBox.SelectedIndex > -1;
        }

        private void PreferredHostsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var haveSelection      = PreferredHostsListBox.SelectedIndex > -1;
            MoveLeftButton.Enabled = haveSelection
                                     && (string)PreferredHostsListBox.SelectedItem != placeholder;
            MoveUpButton.Enabled   = PreferredHostsListBox.SelectedIndex > 0;
            MoveDownButton.Enabled = haveSelection
                                     && PreferredHostsListBox.SelectedIndex < PreferredHostsListBox.Items.Count - 1;
        }

        private void AvailableHostsListBox_DoubleClick(object sender, EventArgs e)
        {
            MoveRightButton_Click(null, null);
        }

        private void PreferredHostsListBox_DoubleClick(object sender, EventArgs e)
        {
            MoveLeftButton_Click(null, null);
        }

        private void MoveRightButton_Click(object sender, EventArgs e)
        {
            if (AvailableHostsListBox.SelectedIndex > -1)
            {
                if (PreferredHostsListBox.Items.Count == 0)
                {
                    PreferredHostsListBox.Items.Add(placeholder);
                }
                var fromWhere = AvailableHostsListBox.SelectedIndex;
                var selected = AvailableHostsListBox.SelectedItem;
                var toWhere = PreferredHostsListBox.Items.IndexOf(placeholder);
                AvailableHostsListBox.Items.Remove(selected);
                PreferredHostsListBox.Items.Insert(toWhere, selected);
                // Preserve selection on same line
                if (AvailableHostsListBox.Items.Count > 0)
                {
                    AvailableHostsListBox.SetSelected(Math.Min(fromWhere,
                                                               AvailableHostsListBox.Items.Count - 1),
                                                      true);
                }
                else
                {
                    // ListBox doesn't notify of selection changes that happen via removal
                    AvailableHostsListBox_SelectedIndexChanged(null, null);
                }
            }
        }

        private void MoveLeftButton_Click(object sender, EventArgs e)
        {
            if (PreferredHostsListBox.SelectedIndex > -1)
            {
                var fromWhere = PreferredHostsListBox.SelectedIndex;
                var selected  = (string)PreferredHostsListBox.SelectedItem;
                if (selected != placeholder)
                {
                    PreferredHostsListBox.Items.Remove(selected);
                    // Regenerate the list to put the item back in the original order
                    AvailableHostsListBox.Items.Clear();
                    AvailableHostsListBox.Items.AddRange(allHosts
                        .Except(PreferredHostsListBox.Items.Cast<string>())
                        .ToArray());
                    if (PreferredHostsListBox.Items.Count == 1)
                    {
                        PreferredHostsListBox.Items.Remove(placeholder);
                    }
                    // Preserve selection on same line
                    if (PreferredHostsListBox.Items.Count > 0)
                    {
                        PreferredHostsListBox.SetSelected(Math.Min(fromWhere,
                                                                   PreferredHostsListBox.Items.Count - 1),
                                                          true);
                    }
                }
            }
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            if (PreferredHostsListBox.SelectedIndex > 0)
            {
                MoveItem(PreferredHostsListBox.SelectedIndex - 1,
                         PreferredHostsListBox.SelectedIndex);
                // ListBox doesn't notify of selection changes that happen via removal
                PreferredHostsListBox_SelectedIndexChanged(null, null);
            }
        }

        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            if (PreferredHostsListBox.SelectedIndex > -1
                && PreferredHostsListBox.SelectedIndex < PreferredHostsListBox.Items.Count - 1)
            {
                MoveItem(PreferredHostsListBox.SelectedIndex + 1,
                         PreferredHostsListBox.SelectedIndex);
                // ListBox doesn't notify of selection changes that happen via insertion
                PreferredHostsListBox_SelectedIndexChanged(null, null);
            }
        }

        private void MoveItem(int from, int to)
        {
            var item = PreferredHostsListBox.Items[from];
            PreferredHostsListBox.Items.RemoveAt(from);
            PreferredHostsListBox.Items.Insert(to, item);
        }

        private readonly IConfiguration config;
        private readonly string[]       allHosts;
        private readonly string         placeholder;
    }
}
