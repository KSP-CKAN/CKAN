using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using log4net;

namespace CKAN
{
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

            this.ToolTip.SetToolTip(NameTextBox, Properties.Resources.EditLabelsToolTipName);
            this.ToolTip.SetToolTip(ColorButton, Properties.Resources.EditLabelsToolTipColor);
            this.ToolTip.SetToolTip(InstanceNameComboBox, Properties.Resources.EditLabelsToolTipInstance);
            this.ToolTip.SetToolTip(HideFromOtherFiltersCheckBox, Properties.Resources.EditLabelsToolTipHide);
            this.ToolTip.SetToolTip(NotifyOnChangesCheckBox, Properties.Resources.EditLabelsToolTipNotifyOnChanges);
            this.ToolTip.SetToolTip(RemoveOnChangesCheckBox, Properties.Resources.EditLabelsToolTipRemoveOnChanges);
            this.ToolTip.SetToolTip(AlertOnInstallCheckBox, Properties.Resources.EditLabelsToolTipAlertOnInstall);
            this.ToolTip.SetToolTip(RemoveOnInstallCheckBox, Properties.Resources.EditLabelsToolTipRemoveOnInstall);
        }

        private void LoadTree()
        {
            LabelSelectionTree.BeginUpdate();
            LabelSelectionTree.Nodes.Clear();
            var groups = this.labels.Labels
                .GroupBy(l => l.InstanceName)
                .OrderBy(g => g.Key);
            foreach (var group in groups)
            {
                string groupName = string.IsNullOrEmpty(group.Key)
                    ? Properties.Resources.ModuleLabelListGlobal
                    : group.Key;
                var gnd = LabelSelectionTree.Nodes.Add(groupName);
                gnd.NodeFont = new Font(LabelSelectionTree.Font, FontStyle.Bold);
                foreach (ModuleLabel mlbl in group.OrderBy(l => l.Name))
                {
                    var lblnd = gnd.Nodes.Add(mlbl.Name);
                    lblnd.Tag = mlbl;
                }
            }
            LabelSelectionTree.ExpandAll();
            LabelSelectionTree.EndUpdate();
        }

        private void LabelSelectionTree_BeforeSelect(Object sender, TreeViewCancelEventArgs e)
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
            string errMsg;
            if (TrySave(out errMsg))
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

            DeleteButton.Enabled = labels.Labels.Contains(lbl);

            EditDetailsPanel.Visible = true;
            EditDetailsPanel.BringToFront();
            NameTextBox.Focus();
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
                    string errMsg;
                    if (!TrySave(out errMsg))
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
                if (!labels.Labels.Contains(currentlyEditing))
                {
                    labels.Labels = labels.Labels
                        .Concat(new ModuleLabel[] { currentlyEditing })
                        .ToArray();
                }
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
                );
        }

        private ModuleLabel     currentlyEditing;

        private readonly IUser           user;
        private readonly ModuleLabelList labels;

        private static readonly ILog log = LogManager.GetLogger(typeof(EditLabelsDialog));
    }
}
