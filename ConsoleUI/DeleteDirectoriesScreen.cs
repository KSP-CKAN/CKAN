using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {
    /// <summary>
    /// Screen prompting user to delete game-created folders
    /// </summary>
    public class DeleteDirectoriesScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        /// <param name="inst">Game instance</param>
        /// <param name="possibleConfigOnlyDirs">Deletable stuff the user should see</param>
        public DeleteDirectoriesScreen(GameInstance    inst,
                                       HashSet<string> possibleConfigOnlyDirs)
        {
            instance = inst;
            toDelete = possibleConfigOnlyDirs.ToHashSet();

            var tb = new ConsoleTextBox(1, 2, -1, 4, false);
            tb.AddLine(Properties.Resources.DeleteDirectoriesHelpLabel);
            AddObject(tb);

            var listWidth = (Console.WindowWidth - 4) / 2;

            directories = new ConsoleListBox<string>(
                1, 6, listWidth - 1, -2,
                possibleConfigOnlyDirs.OrderBy(d => d).ToList(),
                new List<ConsoleListBoxColumn<string>>() {
                    new ConsoleListBoxColumn<string>() {
                        Header   = "",
                        Width    = 1,
                        Renderer = s => toDelete.Contains(s) ? Symbols.checkmark : "",
                    },
                    new ConsoleListBoxColumn<string>() {
                        Header   = Properties.Resources.DeleteDirectoriesFoldersHeader,
                        Width    = null,
                        // The data model holds absolute paths, but the UI shows relative
                        Renderer = p => Platform.FormatPath(instance.ToRelativeGameDir(p)),
                    },
                },
                0, -1);
            directories.AddTip("D", Properties.Resources.DeleteDirectoriesDeleteDirTip,
                               () => !toDelete.Contains(directories.Selection));
            directories.AddBinding(Keys.D, (object sender, ConsoleTheme theme) => {
                toDelete.Add(directories.Selection);
                return true;
            });
            directories.AddTip("K", Properties.Resources.DeleteDirectoriesKeepDirTip,
                               () => toDelete.Contains(directories.Selection));
            directories.AddBinding(Keys.K, (object sender, ConsoleTheme theme) => {
                toDelete.Remove(directories.Selection);
                return true;
            });
            directories.SelectionChanged += PopulateFiles;
            AddObject(directories);

            files = new ConsoleListBox<string>(
                listWidth + 1, 6, -1, -2,
                new List<string>() { "" },
                new List<ConsoleListBoxColumn<string>>() {
                    new ConsoleListBoxColumn<string>() {
                        Header   = Properties.Resources.DeleteDirectoriesFilesHeader,
                        Width    = null,
                        Renderer = p => Platform.FormatPath(CKANPathUtils.ToRelative(p, directories.Selection)),
                    },
                },
                0, -1);
            AddObject(files);

            AddTip(Properties.Resources.Esc,
                   Properties.Resources.DeleteDirectoriesCancelTip);
            AddBinding(Keys.Escape,
                       // Discard changes
                       (object sender, ConsoleTheme theme) => false);

            AddTip("F9", Properties.Resources.DeleteDirectoriesApplyTip);
            AddBinding(Keys.F9, (object sender, ConsoleTheme theme) => {
                foreach (var d in toDelete) {
                    try {
                        Directory.Delete(d, true);
                    } catch {
                    }
                }
                return false;
            });

            PopulateFiles();
        }

        private void PopulateFiles()
        {
            files.SetData(
                Directory.EnumerateFileSystemEntries(directories.Selection,
                                                     "*",
                                                     SearchOption.AllDirectories)
                         .OrderBy(f => f)
                         .ToList(),
                true);
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader() => $"{Meta.GetProductName()} {Meta.GetVersion()}";

        /// <summary>
        /// Show the Delete Directories header
        /// </summary>
        protected override string CenterHeader() => Properties.Resources.DeleteDirectoriesHeader;

        private readonly GameInstance           instance;
        private readonly HashSet<string>        toDelete;
        private readonly ConsoleListBox<string> directories;
        private readonly ConsoleListBox<string> files;
    }
}
