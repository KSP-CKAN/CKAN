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
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="inst">Game instance</param>
        /// <param name="possibleConfigOnlyDirs">Deletable stuff the user should see</param>
        public DeleteDirectoriesScreen(ConsoleTheme    theme,
                                       GameInstance    inst,
                                       HashSet<string> possibleConfigOnlyDirs)
            : base(theme)
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
                    new ConsoleListBoxColumn<string>(
                        "",
                        s => toDelete.Contains(s) ? Symbols.checkmark : "",
                        null,
                        1),
                    new ConsoleListBoxColumn<string>(
                        Properties.Resources.DeleteDirectoriesFoldersHeader,
                        // The data model holds absolute paths, but the UI shows relative
                        p => Platform.FormatPath(instance.ToRelativeGameDir(p)),
                        null,
                        null),
                },
                0, -1);
            directories.AddTip("D", Properties.Resources.DeleteDirectoriesDeleteDirTip,
                               () => directories.Selection is not null
                                     && !toDelete.Contains(directories.Selection));
            directories.AddBinding(Keys.D, (object sender) => {
                if (directories.Selection is not null)
                {
                    toDelete.Add(directories.Selection);
                }
                return true;
            });
            directories.AddTip("K", Properties.Resources.DeleteDirectoriesKeepDirTip,
                               () => directories.Selection is not null
                                     && toDelete.Contains(directories.Selection));
            directories.AddBinding(Keys.K, (object sender) => {
                if (directories.Selection is not null)
                {
                    toDelete.Remove(directories.Selection);
                }
                return true;
            });
            directories.SelectionChanged += PopulateFiles;
            AddObject(directories);

            files = new ConsoleListBox<string>(
                listWidth + 1, 6, -1, -2,
                new List<string>() { "" },
                new List<ConsoleListBoxColumn<string>>() {
                    new ConsoleListBoxColumn<string>(
                        Properties.Resources.DeleteDirectoriesFilesHeader,
                        p => directories.Selection is null
                            ? ""
                            : Platform.FormatPath(CKANPathUtils.ToRelative(p, directories.Selection)),
                        null,
                        null),
                },
                0, -1);
            AddObject(files);

            AddTip(Properties.Resources.Esc,
                   Properties.Resources.DeleteDirectoriesCancelTip);
            AddBinding(Keys.Escape,
                       // Discard changes
                       (object sender) => false);

            AddTip("F9", Properties.Resources.DeleteDirectoriesApplyTip);
            AddBinding(Keys.F9, (object sender) => {
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
                directories.Selection is null
                    ? new List<string>()
                    : Directory.EnumerateFileSystemEntries(directories.Selection,
                                                           "*",
                                                           SearchOption.AllDirectories)
                               .OrderBy(f => f)
                               .ToList(),
                true);
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader() => $"{Meta.ProductName} {Meta.GetVersion()}";

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
