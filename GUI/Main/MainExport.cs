using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using CKAN.Exporters;
using CKAN.Types;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    public partial class Main
    {
        /// <summary>
        /// Exports installed mods to a .ckan file.
        /// </summary>
        private void ExportModPackToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null)
            {
                // Background thread so GUI thread can work with the controls
                Task.Run(() =>
                {
                    currentUser.RaiseMessage("");
                    tabController.ShowTab(EditModpackTabPage.Name, 2);
                    DisableMainWindow();
                    var mgr = RegistryManager.Instance(CurrentInstance, repoData);
                    EditModpack.LoadModule(mgr.GenerateModpack(false, true), mgr.registry);
                    // This will block till the user is done
                    EditModpack.Wait(currentUser);
                    EnableMainWindow();
                    tabController.ShowTab(ManageModsTabPage.Name);
                    tabController.HideTab(EditModpackTabPage.Name);
                });
            }
        }

        private void EditModpack_OnSelectedItemsChanged(ListView.SelectedListViewItemCollection items)
        {
            if (items.OfType<ListViewItem>()
                             .FirstOrDefault()
                             ?.Tag
                     is ModuleRelationshipDescriptor first)
            {
                var ident = first.name;
                if (!string.IsNullOrEmpty(ident)
                    && ManageMods.mainModList != null
                    && ManageMods.mainModList.full_list_of_mod_rows.TryGetValue(ident, out DataGridViewRow? row))
                {
                    ActiveModInfo = row.Tag as GUIMod;
                }
                else
                {
                    ActiveModInfo = null;
                }
            }
        }

        private static readonly List<ExportOption> specialExportOptions = new List<ExportOption>
        {
            new ExportOption(ExportFileType.PlainText, Properties.Resources.MainPlainText, "txt"),
            new ExportOption(ExportFileType.Markdown,  Properties.Resources.MainMarkdown,  "md"),
            new ExportOption(ExportFileType.BbCode,    Properties.Resources.MainBBCode,    "txt"),
            new ExportOption(ExportFileType.Csv,       Properties.Resources.MainCSV,       "csv"),
            new ExportOption(ExportFileType.Tsv,       Properties.Resources.MainTSV,       "tsv"),
        };

        /// <summary>
        /// Exports installed mods to a non-.ckan file.
        /// </summary>
        private void ExportModListToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            if (CurrentInstance != null)
            {
                var dlg = new SaveFileDialog()
                {
                    Filter = string.Join("|", specialExportOptions.Select(i => i.ToString()).ToArray()),
                    Title  = Properties.Resources.ExportInstalledModsDialogTitle
                };
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var fileMode = File.Exists(dlg.FileName) ? FileMode.Truncate : FileMode.CreateNew;
                    using (var stream = new FileStream(dlg.FileName, fileMode))
                    {
                        var regMgr = RegistryManager.Instance(CurrentInstance, repoData);
                        new Exporter(specialExportOptions[dlg.FilterIndex - 1].ExportFileType).Export(regMgr, regMgr.registry, stream);
                    }
                }
            }
        }

    }
}
