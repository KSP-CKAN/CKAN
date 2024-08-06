using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;

using CKAN.Versioning;
using CKAN.Types;
using CKAN.GUI.Attributes;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class EditModpack : UserControl
    {
        public EditModpack()
        {
            InitializeComponent();

            ToolTip.SetToolTip(IdentifierTextBox,       Properties.Resources.EditModpackTooltipIdentifier);
            ToolTip.SetToolTip(NameTextBox,             Properties.Resources.EditModpackTooltipName);
            ToolTip.SetToolTip(AbstractTextBox,         Properties.Resources.EditModpackTooltipAbstract);
            ToolTip.SetToolTip(VersionTextBox,          Properties.Resources.EditModpackTooltipVersion);
            ToolTip.SetToolTip(GameVersionMinComboBox,  Properties.Resources.EditModpackTooltipGameVersionMin);
            ToolTip.SetToolTip(GameVersionMaxComboBox,  Properties.Resources.EditModpackTooltipGameVersionMax);
            ToolTip.SetToolTip(LicenseComboBox,         Properties.Resources.EditModpackTooltipLicense);
            ToolTip.SetToolTip(IncludeVersionsCheckbox, Properties.Resources.EditModpackTooltipIncludeVersions);
            ToolTip.SetToolTip(IncludeOptRelsCheckbox,  Properties.Resources.EditModpackTooltipIncludeOptRels);
            ToolTip.SetToolTip(DependsRadioButton,      Properties.Resources.EditModpackTooltipDepends);
            ToolTip.SetToolTip(RecommendsRadioButton,   Properties.Resources.EditModpackTooltipRecommends);
            ToolTip.SetToolTip(SuggestsRadioButton,     Properties.Resources.EditModpackTooltipSuggests);
            ToolTip.SetToolTip(IgnoreRadioButton,       Properties.Resources.EditModpackTooltipIgnore);
            ToolTip.SetToolTip(CancelExportButton,      Properties.Resources.EditModpackTooltipCancel);
            ToolTip.SetToolTip(ExportModpackButton,     Properties.Resources.EditModpackTooltipExport);
        }

        [ForbidGUICalls]
        public void LoadModule(CkanModule module, IRegistryQuerier registry)
        {
            this.module = module;
            Util.Invoke(this, () =>
            {
                IdentifierTextBox.Text = module.identifier;
                NameTextBox.Text       = module.name;
                AbstractTextBox.Text   = module.@abstract;
                AuthorTextBox.Text     = string.Join(", ", module.author);
                VersionTextBox.Text    = module.version.ToString();
                var options = new string[] { "" }.Concat(Main.Instance.CurrentInstance.game.KnownVersions
                    .SelectMany(v => new GameVersion[] {
                            new GameVersion(v.Major, v.Minor, v.Patch),
                            new GameVersion(v.Major, v.Minor)
                        })
                    .Distinct()
                    .OrderByDescending(v => v)
                    .Select(v => v.ToString())
                );
                GameVersionMinComboBox.DataSource = options.ToArray();
                GameVersionMinComboBox.Text = (module.ksp_version_min ?? module.ksp_version)?.ToString();
                GameVersionMaxComboBox.DataSource = options.ToArray();
                GameVersionMaxComboBox.Text = (module.ksp_version_max ?? module.ksp_version)?.ToString();
                LicenseComboBox.DataSource = License.valid_licenses.OrderBy(l => l).ToArray();
                LicenseComboBox.Text = module.license?.FirstOrDefault()?.ToString();
                LoadRelationships(registry);
            });
        }

        public event Action<ListView.SelectedListViewItemCollection> OnSelectedItemsChanged;

        [ForbidGUICalls]
        public bool Wait(IUser user)
        {
            if (Platform.IsMono)
            {
                // Workaround: make sure the ListView headers are drawn
                Util.Invoke(this, () => RelationshipsListView.EndUpdate());
            }
            this.user = user;
            task = new TaskCompletionSource<bool>();
            return task.Task.Result;
        }

        private void LoadRelationships(IRegistryQuerier registry)
        {
            if (module.depends == null)
            {
                module.depends = new List<RelationshipDescriptor>();
            }
            if (module.recommends == null)
            {
                module.recommends = new List<RelationshipDescriptor>();
            }
            if (module.suggests == null)
            {
                module.suggests = new List<RelationshipDescriptor>();
            }

            ignored.Clear();
            // Find installed modules that aren't in the module's relationships
            ignored.AddRange(registry.Installed(false, false)
                .Where(kvp => {
                    var ids = new string[] { kvp.Key };
                    return !module.depends.Any(rel => rel.ContainsAny(ids))
                        && !module.recommends.Any(rel => rel.ContainsAny(ids))
                        && !module.suggests.Any(rel => rel.ContainsAny(ids));
                })
                .Select(kvp => (RelationshipDescriptor) new ModuleRelationshipDescriptor()
                    {
                        name    = kvp.Key,
                        version = kvp.Value,
                    })
            );
            RelationshipsListView.Items.Clear();
            AddGroup(module.depends,    DependsGroup,         registry);
            AddGroup(module.recommends, RecommendationsGroup, registry);
            AddGroup(module.suggests,   SuggestionsGroup,     registry);
            AddGroup(ignored,           IgnoredGroup,         registry);
            RelationshipsListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            GroupToRelationships.Clear();
            GroupToRelationships.Add(DependsGroup,         module.depends);
            GroupToRelationships.Add(RecommendationsGroup, module.recommends);
            GroupToRelationships.Add(SuggestionsGroup,     module.suggests);
            GroupToRelationships.Add(IgnoredGroup,         ignored);

            RelationshipsListView_ItemSelectionChanged(null, null);
        }

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.ModPacks);
        }

        private void AddGroup(List<RelationshipDescriptor> relationships, ListViewGroup group, IRegistryQuerier registry)
        {
            if (relationships != null)
            {
                RelationshipsListView.Items.AddRange(relationships
                    .OrderBy(r => (r as ModuleRelationshipDescriptor)?.name)
                    .Select(r => new ListViewItem(new string[]
                        {
                            (r as ModuleRelationshipDescriptor)?.name,
                            (r as ModuleRelationshipDescriptor)?.version?.ToString(),
                            registry.InstalledModules.First(
                                im => im.identifier == (r as ModuleRelationshipDescriptor)?.name
                            )?.Module.@abstract
                        })
                        {
                            Tag   = r,
                            Group = group,
                        })
                    .ToArray());
            }
        }

        private bool TryFieldsToModule(out string error, out Control badField)
        {
            if (!Identifier.ValidIdentifierPattern.IsMatch(IdentifierTextBox.Text))
            {
                error = Properties.Resources.EditModpackBadIdentifier;
                badField = IdentifierTextBox;
                return false;
            }
            if (string.IsNullOrEmpty(NameTextBox.Text))
            {
                error = Properties.Resources.EditModpackBadName;
                badField = NameTextBox;
                return false;
            }
            if (string.IsNullOrEmpty(VersionTextBox.Text))
            {
                error = Properties.Resources.EditModpackBadVersion;
                badField = VersionTextBox;
                return false;
            }
            if (!string.IsNullOrEmpty(GameVersionMinComboBox.Text) && !string.IsNullOrEmpty(GameVersionMaxComboBox.Text)
                && GameVersion.Parse(GameVersionMinComboBox.Text) > GameVersion.Parse(GameVersionMaxComboBox.Text))
            {
                error = Properties.Resources.EditModpackBadGameVersions;
                badField = GameVersionMinComboBox;
                return false;
            }

            error = null;
            badField = null;
            module.identifier = IdentifierTextBox.Text;
            module.name       = NameTextBox.Text;
            module.@abstract  = AbstractTextBox.Text;
            module.author     = AuthorTextBox.Text
                .Split(',').Select(a => a.Trim()).ToList();
            module.version    = new ModuleVersion(VersionTextBox.Text);
            module.license    = new List<License>() { new License(LicenseComboBox.Text) };
            module.ksp_version_min = string.IsNullOrEmpty(GameVersionMinComboBox.Text)
                ? null
                : GameVersion.Parse(GameVersionMinComboBox.Text);
            module.ksp_version_max = string.IsNullOrEmpty(GameVersionMaxComboBox.Text)
                ? null
                : GameVersion.Parse(GameVersionMaxComboBox.Text);
            return true;
        }

        private void RelationshipsListView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // Select all on ctrl-A
                case Keys.A:
                    if (e.Control)
                    {
                        foreach (ListViewItem lvi in RelationshipsListView.Items)
                        {
                            lvi.Selected = true;
                        }
                    }
                    break;

                // Deselect all on Esc
                case Keys.Escape:
                    foreach (ListViewItem lvi in RelationshipsListView.Items)
                    {
                        lvi.Selected = false;
                    }
                    break;
            }
        }

        private void RelationshipsListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            OnSelectedItemsChanged?.Invoke(RelationshipsListView.SelectedItems);
            var kinds = RelationshipsListView.SelectedItems.Cast<ListViewItem>()
                .Select(lvi => lvi.Group)
                .Distinct()
                .ToList();
            if (kinds.Count == 1)
            {
                switch (kinds.First().Name)
                {
                    case "DependsGroup":         DependsRadioButton.Checked    = true; break;
                    case "RecommendationsGroup": RecommendsRadioButton.Checked = true; break;
                    case "SuggestionsGroup":     SuggestsRadioButton.Checked   = true; break;
                    case "IgnoredGroup":         IgnoreRadioButton.Checked     = true; break;
                }
            }
            else
            {
                DependsRadioButton.Checked    = false;
                RecommendsRadioButton.Checked = false;
                SuggestsRadioButton.Checked   = false;
                IgnoreRadioButton.Checked     = false;
            }
            if (RelationshipsListView.SelectedItems.Count > 0)
            {
                DependsRadioButton.Enabled    = true;
                RecommendsRadioButton.Enabled = true;
                SuggestsRadioButton.Enabled   = true;
                IgnoreRadioButton.Enabled     = true;
            }
            else
            {
                DependsRadioButton.Enabled    = false;
                RecommendsRadioButton.Enabled = false;
                SuggestsRadioButton.Enabled   = false;
                IgnoreRadioButton.Enabled     = false;
            }
        }

        private void DependsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            MoveItemsTo(RelationshipsListView.SelectedItems.Cast<ListViewItem>(), DependsGroup, module.depends);
            RelationshipsListView.Focus();
        }

        private void RecommendsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            MoveItemsTo(RelationshipsListView.SelectedItems.Cast<ListViewItem>(), RecommendationsGroup, module.recommends);
            RelationshipsListView.Focus();
        }

        private void SuggestsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            MoveItemsTo(RelationshipsListView.SelectedItems.Cast<ListViewItem>(), SuggestionsGroup, module.suggests);
            RelationshipsListView.Focus();
        }

        private void IgnoreRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            MoveItemsTo(RelationshipsListView.SelectedItems.Cast<ListViewItem>(), IgnoredGroup, ignored);
            RelationshipsListView.Focus();
        }

        private void MoveItemsTo(IEnumerable<ListViewItem> items, ListViewGroup group, List<RelationshipDescriptor> relationships)
        {
            foreach (ListViewItem lvi in items.Where(lvi => lvi.Group != group))
            {
                // UI
                var rel = lvi.Tag as RelationshipDescriptor;
                var fromRel = GroupToRelationships[lvi.Group];
                fromRel.Remove(rel);
                relationships.Add(rel);
                // Model
                lvi.Group = group;
            }
        }

        private void CancelExportButton_Click(object sender, EventArgs e)
        {
            task?.SetResult(false);
        }

        private void ExportModpackButton_Click(object sender, EventArgs e)
        {
            if (!TryFieldsToModule(out string error, out Control badField))
            {
                badField.Focus();
                user.RaiseError(error);
            }
            else if (TrySavePrompt(modpackExportOptions, out string filename))
            {
                if (module.depends.Count == 0)
                {
                    module.depends = null;
                }
                if (module.recommends.Count == 0)
                {
                    module.recommends = null;
                }
                if (module.suggests.Count == 0)
                {
                    module.suggests = null;
                }
                CkanModule.ToFile(ApplyCheckboxes(module), filename);
                OpenFileBrowser(filename);
                task?.SetResult(true);
            }
        }

        private CkanModule ApplyCheckboxes(CkanModule input)
            => ApplyIncludeOptRelsCheckbox(ApplyVersionsCheckbox(input));

        private CkanModule ApplyVersionsCheckbox(CkanModule input)
        {
            if (IncludeVersionsCheckbox.Checked)
            {
                return input;
            }
            else
            {
                // We want to return the relationships without the version properties,
                // BUT we don't want to purge them from the main module object
                // in case the user changes the checkbox after cancelling out of the
                // save popup. So we create a new CkanModule instead.
                var newMod = CkanModule.FromJson(CkanModule.ToJson(input));
                foreach (var rels in new List<List<RelationshipDescriptor>>()
                    {
                        newMod.depends,
                        newMod.recommends,
                        newMod.suggests
                    })
                {
                    if (rels != null)
                    {
                        foreach (var rel in rels.OfType<ModuleRelationshipDescriptor>())
                        {
                            rel.version     = null;
                            rel.min_version = null;
                            rel.max_version = null;
                        }
                    }
                }
                return newMod;
            }
        }

        private CkanModule ApplyIncludeOptRelsCheckbox(CkanModule input)
        {
            if (IncludeOptRelsCheckbox.Checked)
            {
                return input;
            }
            else
            {
                var newMod = CkanModule.FromJson(CkanModule.ToJson(input));
                foreach (var rel in newMod.depends)
                {
                    rel.suppress_recommendations = true;
                }
                return newMod;
            }
        }

        private void OpenFileBrowser(string location)
        {
            if (File.Exists(location))
            {
                // We need the folder of the file
                // Otherwise the OS would try to open the file in its default application
                location = Path.GetDirectoryName(location);
            }
            if (Directory.Exists(location))
            {
                Utilities.ProcessStartURL(location);
            }
        }

        private bool TrySavePrompt(List<ExportOption> exportOptions, out string filename)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = string.Join("|", exportOptions.Select(i => i.ToString()).ToArray()),
                Title  = Properties.Resources.ExportInstalledModsDialogTitle
            };
            if (dlg.ShowDialog(ParentForm) == DialogResult.OK)
            {
                filename       = dlg.FileName;
                return true;
            }
            else
            {
                filename       = null;
                return false;
            }
        }

        private static readonly List<ExportOption> modpackExportOptions = new List<ExportOption>()
        {
            new ExportOption(ExportFileType.Ckan, Properties.Resources.MainModPack, "ckan"),
        };

        private CkanModule                   module;
        private IUser                        user;
        private TaskCompletionSource<bool>   task;
        private readonly List<RelationshipDescriptor> ignored = new List<RelationshipDescriptor>();
        private readonly Dictionary<ListViewGroup, List<RelationshipDescriptor>> GroupToRelationships =
            new Dictionary<ListViewGroup, List<RelationshipDescriptor>>();
    }
}
