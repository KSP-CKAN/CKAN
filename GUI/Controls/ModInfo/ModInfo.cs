using System;
using System.Linq;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;

using CKAN.Configuration;
using CKAN.Versioning;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class ModInfo : UserControl
    {
        public ModInfo()
        {
            InitializeComponent();
            Contents.OnDownloadClick += gmod => OnDownloadClick?.Invoke(gmod);
            Relationships.ModuleDoubleClicked += mod => ModuleDoubleClicked?.Invoke(mod);
            tagsLabelsLinkList.ShowHideTag += t => ShowHideTag?.Invoke(t);
            tagsLabelsLinkList.AddRemoveModuleLabel += l => AddRemoveModuleLabel?.Invoke(l);
        }

        public GUIMod? SelectedModule
        {
            set
            {
                if (value != null
                    && manager?.CurrentInstance?.VersionCriteria()
                       is GameVersionCriteria crit)
                {
                    ModInfoTabControl.Enabled = true;
                    UpdateHeaderInfo(value, crit);
                }
                else
                {
                    ModInfoTabControl.Enabled = false;
                }
                if (value != selectedModule)
                {
                    selectedModule = value;
                    LoadTab(value);
                }
            }
            get => selectedModule;
        }

        public void SwitchTab(string name)
        {
            ModInfoTabControl.SelectedTab = ModInfoTabControl.TabPages[name];
        }

        public event Action<GUIMod>?            OnDownloadClick;
        public event Action<SavedSearch, bool>? OnChangeFilter;
        public event Action<CkanModule>?        ModuleDoubleClicked;
        public event Action<ModuleTag>?         ShowHideTag;
        public event Action<ModuleLabel>?       AddRemoveModuleLabel;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ModInfoTable.RowStyles[1].Height = ModInfoTable.Padding.Vertical
                                               + ModInfoTable.Margin.Vertical
                                               + tagsLabelsLinkList.TagsHeight;
            if (MetadataModuleDescriptionTextBox != null
                && !string.IsNullOrEmpty(MetadataModuleDescriptionTextBox.Text))
            {
                MetadataModuleDescriptionTextBox.Height = DescriptionHeight;
            }
        }

        private GUIMod? selectedModule;

        private void LoadTab(GUIMod? gm)
        {
            switch (ModInfoTabControl.SelectedTab?.Name)
            {
                case "MetadataTabPage":
                    if (gm != null)
                    {
                        Metadata.UpdateModInfo(gm);
                    }
                    break;

                case "ContentTabPage":
                    Contents.SelectedModule = gm;
                    break;

                case "RelationshipTabPage":
                    Relationships.SelectedModule = gm;
                    break;

                case "VersionsTabPage":
                    Versions.SelectedModule = gm;
                    if (Platform.IsMono)
                    {
                        // Workaround: make sure the ListView headers are drawn
                        Versions.ForceRedraw();
                    }
                    break;
            }
        }

        // When switching tabs ensure that the resulting tab is updated.
        private void ModInfoTabControl_SelectedIndexChanged(object? sender, EventArgs? e)
        {
            if (SelectedModule != null)
            {
                LoadTab(SelectedModule);
            }
        }

        private static GameInstanceManager? manager => Main.Instance?.Manager;

        private int TextBoxStringHeight(TextBox tb)
            => tb.Padding.Vertical + tb.Margin.Vertical
                + Util.StringHeight(CreateGraphics(), tb.Text, tb.Font,
                                    tb.Width - tb.Padding.Horizontal - tb.Margin.Horizontal);

        private int DescriptionHeight => TextBoxStringHeight(MetadataModuleDescriptionTextBox);

        private void UpdateHeaderInfo(GUIMod gmod, GameVersionCriteria crit)
        {
            var module = gmod.Module;
            Util.Invoke(this, () =>
            {
                ModInfoTabControl.SuspendLayout();

                MetadataModuleNameTextBox.Text = module.name;
                UpdateTagsAndLabels(module);
                MetadataModuleAbstractLabel.Text = module.@abstract.Replace("&", "&&");
                MetadataModuleDescriptionTextBox.Text = module.description
                    ?.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
                MetadataModuleDescriptionTextBox.Height = DescriptionHeight;

                // Set/reset alert icons to show a user why a mod is incompatible
                RelationshipTabPage.ImageKey = "";
                VersionsTabPage.ImageKey     = "";
                if (gmod.IsIncompatible)
                {
                    var pageToAlert = module.IsCompatible(crit) ? RelationshipTabPage : VersionsTabPage;
                    pageToAlert.ImageKey = "Stop";
                }
                if (manager?.CurrentInstance is GameInstance inst)
                {
                    var filters = ServiceLocator.Container.Resolve<IConfiguration>()
                                                          .GetGlobalInstallFilters(inst.Game)
                                                          .Concat(inst.InstallFilters)
                                                          .ToHashSet();
                    ContentTabPage.ImageKey = ModuleLabels.IgnoreMissingIdentifiers(inst)
                                                          .Contains(gmod.Identifier)
                                              || (gmod.InstalledMod?.AllFilesExist(inst, filters)
                                                                   ?? true)
                                                  ? ""
                                                  : "Stop";
                }

                ModInfoTabControl.ResumeLayout();
            });
        }

        private static ModuleLabelList ModuleLabels => ModuleLabelList.ModuleLabels;

        private void UpdateTagsAndLabels(CkanModule mod)
        {
            if (manager?.CurrentInstance is GameInstance inst)
            {
                var registry = RegistryManager.Instance(inst, ServiceLocator.Container.Resolve<RepositoryDataManager>())
                                              .registry;
                tagsLabelsLinkList.UpdateTagsAndLabels(
                    registry?.Tags
                             .Where(t => t.Value.ModuleIdentifiers.Contains(mod.identifier))
                             .OrderBy(t => t.Key)
                             .Select(t => t.Value),
                    ModuleLabels?.LabelsFor(inst.Name)
                                 .Where(l => l.ContainsModule(inst.Game, mod.identifier))
                                 .OrderBy(l => l.Name));
                Util.Invoke(tagsLabelsLinkList, () =>
                {
                    ModInfoTable.RowStyles[1].Height = ModInfoTable.Padding.Vertical
                                                       + ModInfoTable.Margin.Vertical
                                                       + tagsLabelsLinkList.TagsHeight;
                });
            }
        }


        private void tagsLabelsLinkList_TagClicked(ModuleTag tag, bool merge)
            => OnChangeFilter?.Invoke(ModList.FilterToSavedSearch(Main.Instance!.CurrentInstance!,
                                                                  GUIModFilter.Tag, ModuleLabelList.ModuleLabels,
                                                                  tag, null),
                                      merge);

        private void tagsLabelsLinkList_LabelClicked(ModuleLabel label, bool merge)
            => OnChangeFilter?.Invoke(ModList.FilterToSavedSearch(Main.Instance!.CurrentInstance!,
                                                                  GUIModFilter.CustomLabel, ModuleLabelList.ModuleLabels,
                                                                  null, label),
                                      merge);

        private void Metadata_OnChangeFilter(SavedSearch search, bool merge)
        {
            OnChangeFilter?.Invoke(search, merge);
        }
    }
}
