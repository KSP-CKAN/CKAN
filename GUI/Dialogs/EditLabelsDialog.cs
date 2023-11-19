using System;
using System.Linq;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class EditLabelsDialog : Form
    {
        public EditLabelsDialog(IUser user, GameInstanceManager manager, ModuleLabelList labels)
        {
            InitializeComponent();
            this.user    = user;
            this.labels  = labels;
            InstanceNameComboBox.DataSource = new string[] { "" }
                .Concat(manager.Instances.Keys).ToArray();
            LoadTree();

            ToolTip.SetToolTip(NameTextBox, Properties.Resources.EditLabelsToolTipName);
            ToolTip.SetToolTip(ColorButton, Properties.Resources.EditLabelsToolTipColor);
            ToolTip.SetToolTip(InstanceNameComboBox, Properties.Resources.EditLabelsToolTipInstance);
            ToolTip.SetToolTip(HideFromOtherFiltersCheckBox, Properties.Resources.EditLabelsToolTipHide);
            ToolTip.SetToolTip(NotifyOnChangesCheckBox, Properties.Resources.EditLabelsToolTipNotifyOnChanges);
            ToolTip.SetToolTip(RemoveOnChangesCheckBox, Properties.Resources.EditLabelsToolTipRemoveOnChanges);
            ToolTip.SetToolTip(AlertOnInstallCheckBox, Properties.Resources.EditLabelsToolTipAlertOnInstall);
            ToolTip.SetToolTip(RemoveOnInstallCheckBox, Properties.Resources.EditLabelsToolTipRemoveOnInstall);
            ToolTip.SetToolTip(HoldVersionCheckBox, Properties.Resources.EditLabelsToolTipHoldVersion);
            ToolTip.SetToolTip(MoveUpButton, Properties.Resources.EditLabelsToolTipMoveUp);
            ToolTip.SetToolTip(MoveDownButton, Properties.Resources.EditLabelsToolTipMoveDown);
        }

        private void LoadTree()
        {
            LabelSelectionTree.BeginUpdate();
            LabelSelectionTree.Nodes.Clear();
            var groups = labels.Labels
                .GroupBy(l => l.InstanceName)
                .OrderBy(g => g.Key == null)
                .ThenBy(g => g.Key);
            foreach (var group in groups)
            {
                string groupName = string.IsNullOrEmpty(group.Key)
                    ? Properties.Resources.ModuleLabelListGlobal
                    : group.Key;
                LabelSelectionTree.Nodes.Add(new TreeNode(
                    groupName,
                    group.Select(mlbl => new TreeNode(mlbl.Name)
                        {
                            // Windows's TreeView has a bug where the node's visual
                            // width is based on the owning TreeView.Font rather
                            // than TreeNode.Font, so to ensure there's enough space,
                            // we have to make the default bold and then override it
                            // for non-bold nodes.
                            NodeFont = new Font(LabelSelectionTree.Font, FontStyle.Regular),
                            Tag      = mlbl
                        })
                        .ToArray()
                ));
            }
            EnableDisableUpDownButtons();
            if (currentlyEditing != null)
            {
                LabelSelectionTree.BeforeSelect -= LabelSelectionTree_BeforeSelect;
                // Select the new node representing the label we're editing
                LabelSelectionTree.SelectedNode = LabelSelectionTree.Nodes.Cast<TreeNode>()
                    .SelectMany(nd => nd.Nodes.Cast<TreeNode>())
                    .FirstOrDefault(nd => (nd.Tag as ModuleLabel) == currentlyEditing);
                LabelSelectionTree.BeforeSelect += LabelSelectionTree_BeforeSelect;
            }
            LabelSelectionTree.ExpandAll();
            LabelSelectionTree.EndUpdate();
        }

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.Labels);
        }

        /// <summary>
        /// Open the user guide when the user clicks the help button
        /// </summary>
        protected override void OnHelpButtonClicked(CancelEventArgs evt)
        {
            evt.Cancel = Util.TryOpenWebPage(HelpURLs.Labels);
        }

        private void LabelSelectionTree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null)
            {
                e.Cancel = false;
            }
            else if (e.Node.Tag == null)
            {
                e.Cancel = true;
            }
            else if (!TryCloseEdit())
            {
                e.Cancel = true;
            }
            else
            {
                StartEdit(e.Node.Tag as ModuleLabel);
                e.Cancel = false;
            }
        }

        private void LabelSelectionTree_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            LabelSelectionTree.SelectedNode = null;
            StartEdit(new ModuleLabel());
        }

        private void ColorButton_Click(object sender, EventArgs e)
        {
            var dlg = new ColorDialog()
            {
                AnyColor       = true,
                AllowFullOpen  = true,
                ShowHelp       = true,
                SolidColorOnly = true,
                Color          = ColorButton.BackColor,
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                ColorButton.BackColor = dlg.Color;
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (TrySave(out string errMsg))
            {
                LabelSelectionTree.SelectedNode = null;
            }
            else
            {
                user.RaiseError(errMsg);
            }
        }

        private void CancelEditButton_Click(object sender, EventArgs e)
        {
            EditDetailsPanel.Visible = false;
            currentlyEditing = null;
            LabelSelectionTree.SelectedNode = null;
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (currentlyEditing != null && Main.Instance.YesNoDialog(
                string.Format(
                    Properties.Resources.EditLabelsDialogConfirmDelete,
                    currentlyEditing.Name
                ),
                Properties.Resources.EditLabelsDialogDelete,
                Properties.Resources.EditLabelsDialogCancel
            ))
            {
                labels.Labels = labels.Labels
                    .Except(new ModuleLabel[] { currentlyEditing })
                    .ToArray();
                EditDetailsPanel.Visible = false;
                currentlyEditing = null;
                LoadTree();
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (TryCloseEdit())
            {
                Close();
            }
        }

        private void StartEdit(ModuleLabel lbl)
        {
            currentlyEditing = lbl;

            NameTextBox.Text                     = lbl.Name;
            ColorButton.BackColor                = lbl.Color;
            InstanceNameComboBox.SelectedItem    = lbl.InstanceName;
            HideFromOtherFiltersCheckBox.Checked = lbl.Hide;
            NotifyOnChangesCheckBox.Checked      = lbl.NotifyOnChange;
            RemoveOnChangesCheckBox.Checked      = lbl.RemoveOnChange;
            AlertOnInstallCheckBox.Checked       = lbl.AlertOnInstall;
            RemoveOnInstallCheckBox.Checked      = lbl.RemoveOnInstall;
            HoldVersionCheckBox.Checked          = lbl.HoldVersion;

            DeleteButton.Enabled = labels.Labels.Contains(lbl);
            EnableDisableUpDownButtons();

            EditDetailsPanel.Visible = true;
            EditDetailsPanel.BringToFront();
            NameTextBox.Focus();
        }

        private void EnableDisableUpDownButtons()
        {
            if (currentlyEditing == null)
            {
                MoveUpButton.Enabled = MoveDownButton.Enabled = false;
            }
            else
            {
                var group = labels.Labels
                    .Where(lbl => lbl.InstanceName == currentlyEditing.InstanceName)
                    .ToList();
                int groupIndex = group.IndexOf(currentlyEditing);
                MoveUpButton.Enabled   = groupIndex >  0;
                MoveDownButton.Enabled = groupIndex >= 0 && groupIndex < group.Count - 1;
            }
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            if (currentlyEditing != null)
            {
                var group = labels.Labels
                    .Where(lbl => lbl.InstanceName == currentlyEditing.InstanceName)
                    .ToList();
                int groupIndex = group.IndexOf(currentlyEditing);
                if (groupIndex > 0)
                {
                    // Swap with previous node
                    int mainIndex = Array.IndexOf(labels.Labels, currentlyEditing);
                    int prevIndex = Array.IndexOf(labels.Labels, group[groupIndex - 1]);
                    labels.Labels[mainIndex] = labels.Labels[prevIndex];
                    labels.Labels[prevIndex] = currentlyEditing;
                    LoadTree();
                }
            }
        }

        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            if (currentlyEditing != null)
            {
                var group = labels.Labels
                    .Where(lbl => lbl.InstanceName == currentlyEditing.InstanceName)
                    .ToList();
                int groupIndex = group.IndexOf(currentlyEditing);
                if (groupIndex >= 0 && groupIndex < group.Count - 1)
                {
                    // Swap with next node
                    int mainIndex = Array.IndexOf(labels.Labels, currentlyEditing);
                    int nextIndex = Array.IndexOf(labels.Labels, group[groupIndex + 1]);
                    labels.Labels[mainIndex] = labels.Labels[nextIndex];
                    labels.Labels[nextIndex] = currentlyEditing;
                    LoadTree();
                }
            }
        }

        private bool TryCloseEdit()
        {
            if (HasChanges())
            {
                if (Main.Instance.YesNoDialog(
                    Properties.Resources.EditLabelsDialogSavePrompt,
                    Properties.Resources.EditLabelsDialogSave,
                    Properties.Resources.EditLabelsDialogDiscard
                ))
                {
                    if (!TrySave(out string errMsg))
                    {
                        user.RaiseError(errMsg);
                        return false;
                    }
                }
            }
            return true;
        }

        private bool TrySave(out string errMsg)
        {
            if (EditingValid(out errMsg))
            {
                currentlyEditing.Name         = NameTextBox.Text;
                currentlyEditing.Color        = ColorButton.BackColor;
                currentlyEditing.InstanceName =
                    string.IsNullOrWhiteSpace(InstanceNameComboBox.SelectedItem?.ToString())
                        ? null
                        : InstanceNameComboBox.SelectedItem.ToString();
                currentlyEditing.Hide            = HideFromOtherFiltersCheckBox.Checked;
                currentlyEditing.NotifyOnChange  = NotifyOnChangesCheckBox.Checked;
                currentlyEditing.RemoveOnChange  = RemoveOnChangesCheckBox.Checked;
                currentlyEditing.AlertOnInstall  = AlertOnInstallCheckBox.Checked;
                currentlyEditing.RemoveOnInstall = RemoveOnInstallCheckBox.Checked;
                currentlyEditing.HoldVersion     = HoldVersionCheckBox.Checked;
                if (!labels.Labels.Contains(currentlyEditing))
                {
                    labels.Labels = labels.Labels
                        .Concat(new ModuleLabel[] { currentlyEditing })
                        .OrderBy(l => l.InstanceName == null)
                        .ThenBy(l => l.InstanceName)
                        .ToArray();
                }

                EditDetailsPanel.Visible = false;
                currentlyEditing = null;
                LoadTree();
                return true;
            }
            return false;
        }

        private bool EditingValid(out string errMsg)
        {
            if (currentlyEditing == null)
            {
                errMsg = Properties.Resources.EditLabelsDialogNoRecord;
                return false;
            }
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                errMsg = Properties.Resources.EditLabelsDialogNameRequired;
                return false;
            }
            var newInst = string.IsNullOrWhiteSpace(InstanceNameComboBox.SelectedItem?.ToString())
                ? null
                : InstanceNameComboBox.SelectedItem.ToString();
            var found = labels.Labels.FirstOrDefault(l =>
                   l              != currentlyEditing
                && l.Name         == NameTextBox.Text
                && (l.InstanceName == newInst
                    || newInst == null
                    || l.InstanceName == null)
            );
            if (found != null)
            {
                errMsg = string.Format(
                    Properties.Resources.EditLabelsDialogAlreadyExists,
                    NameTextBox.Text,
                    found.InstanceName ?? Properties.Resources.ModuleLabelListGlobal
                );
                return false;
            }
            errMsg = "";
            return true;
        }

        private bool HasChanges()
        {
            var newInst = string.IsNullOrWhiteSpace(InstanceNameComboBox.SelectedItem?.ToString())
                ? null
                : InstanceNameComboBox.SelectedItem.ToString();
            return EditDetailsPanel.Visible && currentlyEditing != null
                && (   currentlyEditing.Name            != NameTextBox.Text
                    || currentlyEditing.Color           != ColorButton.BackColor
                    || currentlyEditing.InstanceName    != newInst
                    || currentlyEditing.Hide            != HideFromOtherFiltersCheckBox.Checked
                    || currentlyEditing.NotifyOnChange  != NotifyOnChangesCheckBox.Checked
                    || currentlyEditing.RemoveOnChange  != RemoveOnChangesCheckBox.Checked
                    || currentlyEditing.AlertOnInstall  != AlertOnInstallCheckBox.Checked
                    || currentlyEditing.RemoveOnInstall != RemoveOnInstallCheckBox.Checked
                    || currentlyEditing.HoldVersion     != HoldVersionCheckBox.Checked
                );
        }

        private ModuleLabel     currentlyEditing;

        private readonly IUser           user;
        private readonly ModuleLabelList labels;
    }
}
