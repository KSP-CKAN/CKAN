using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// A popup to let the user select multiple files.
    /// </summary>
    public class ConsoleFileMultiSelectDialog : ConsoleDialog {

        /// <summary>
        /// Initialize the popup.
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="title">String to be shown in the top center of the popup</param>
        /// <param name="startPath">Path of directory to start in</param>
        /// <param name="filPat">Glob-style wildcard string for matching files to show</param>
        /// <param name="toggleHeader">Header for the column with checkmarks for selected files</param>
        /// <param name="acceptTip">Description of the F9 action to accept selections</param>
        public ConsoleFileMultiSelectDialog(ConsoleTheme theme,
                                            string title,
                                            string startPath,
                                            string filPat,
                                            string toggleHeader,
                                            string acceptTip)
            : base(theme)
        {
            CenterHeader = () => title;
            curDir       = new DirectoryInfo(startPath);
            filePattern  = filPat;

            int w = (Console.WindowWidth > idealW + (2 * hPad))
                ? idealW
                : Console.WindowWidth - (2 * hPad);
            int left  = (Console.WindowWidth - w) / 2;
            int right = -left;

            SetDimensions(left, top, right, bottom);

            AddObject(new ConsoleLabel(
                left + 3, top + 2, left + 2 + labelW - 1,
                () => Properties.Resources.FileSelectDirectory,
                th => th.PopupBg,
                th => th.PopupFg
            ));

            pathField = new ConsoleField(
                left + 3 + labelW + 1, top + 2, right - 3,
                curDir.FullName
            );
            pathField.OnChange += pathFieldChanged;
            AddObject(pathField);

            AddObject(new ConsoleLabel(
                left + 3, bottom - 1, right - 3,
                () => string.Format(Properties.Resources.FileSelectCountFooter, chosenFiles.Count, CkanModule.FmtSize(totalChosenSize())),
                th => th.PopupBg,
                th => th.PopupFg
            ));

            // ListBox showing matching files in current dir
            fileList = new ConsoleListBox<FileSystemInfo>(
                left + 3, top + 4, right - 3, bottom - 2,
                getFileList(),
                new List<ConsoleListBoxColumn<FileSystemInfo>>() {
                    new ConsoleListBoxColumn<FileSystemInfo>(
                        toggleHeader, getRowSymbol, null, 8),
                    new ConsoleListBoxColumn<FileSystemInfo>(
                        Properties.Resources.FileSelectNameHeader,
                        getRowName, compareNames, null),
                    new ConsoleListBoxColumn<FileSystemInfo>(
                        Properties.Resources.FileSelectSizeHeader,
                        getLength,
                        (a, b) => {
                            var fb = b as FileInfo;
                            return a is not FileInfo fa
                                ? (fb == null ? 0 : -1)
                                : (fb == null ? 1 : fa.Length.CompareTo(fb.Length));
                        },
                        9),
                    new ConsoleListBoxColumn<FileSystemInfo>(
                        Properties.Resources.FileSelectTimestampHeader,
                        fi => fi.LastWriteTime.ToString("yyyy-MM-dd"),
                        (a, b) => a.LastWriteTime.CompareTo(b.LastWriteTime),
                        10)
                },
                1, 1, ListSortDirection.Ascending
            );
            AddObject(fileList);

            AddTip(Properties.Resources.Esc, Properties.Resources.Cancel);
            AddBinding(Keys.Escape, sender =>
            {
                chosenFiles.Clear();
                return false;
            });

            AddTip(ConsoleScreen.MainMenuKeyTip, Properties.Resources.Sort);
            AddBinding(ConsoleScreen.MainMenuKeys, sender =>
            {
                fileList.SortMenu().Run(theme, right - 2, top + 2);
                DrawBackground();
                return true;
            });

            AddTip(Properties.Resources.Enter, Properties.Resources.FileSelectChangeDirectory, () => fileList.Selection != null &&  isDir(fileList.Selection));
            AddTip(Properties.Resources.Enter, Properties.Resources.FileSelectSelect,          () => fileList.Selection != null && !isDir(fileList.Selection));
            AddBinding(Keys.Enter, sender => selectRow());
            AddBinding(Keys.Space, sender => selectRow());

            AddTip($"{Properties.Resources.Ctrl}+A", Properties.Resources.SelectAll);
            AddBinding(Keys.CtrlA, sender =>
            {
                foreach (FileSystemInfo fi in contents) {
                    if (!isDir(fi)) {
                        if (fi is FileInfo file)
                        {
                            chosenFiles.Add(file);
                        }
                    }
                }
                return true;
            });

            AddTip($"{Properties.Resources.Ctrl}+D", Properties.Resources.DeselectAll, () => chosenFiles.Count > 0);
            AddBinding(Keys.CtrlD, sender =>
            {
                if (chosenFiles.Count > 0) {
                    chosenFiles.Clear();
                }
                return true;
            });

            AddTip("F9", acceptTip, () => chosenFiles.Count > 0);
            AddBinding(Keys.F9, sender => false);
        }

        private bool selectRow()
        {
            if (fileList.Selection != null && isDir(fileList.Selection)) {
                if (fileList.Selection is DirectoryInfo di)
                {
                    curDir = di;
                    pathField.Value = curDir.FullName;
                    fileList.SetData(getFileList(), true);
                }
            } else {
                if (fileList.Selection is FileInfo fi)
                {
                    if (!chosenFiles.Remove(fi))
                    {
                        chosenFiles.Add(fi);
                    }
                }
            }
            return true;
        }

        private void pathFieldChanged(ConsoleField sender, string newValue)
        {
            // Validate and update path
            if (Directory.Exists(newValue)) {
                try {
                    curDir = new DirectoryInfo(newValue);
                    fileList.SetData(getFileList());
                } catch { }
            }
        }

        /// <summary>
        /// Display the dialog and handle its interaction
        /// </summary>
        /// <param name="process">Function to control the dialog, default is normal user interaction</param>
        /// <returns>
        /// Files user selected
        /// </returns>
        public new HashSet<FileInfo> Run(Action? process = null)
        {
            base.Run(process);
            return chosenFiles;
        }

        [MemberNotNull(nameof(contents))]
        private IList<FileSystemInfo> getFileList()
        {
            contents = new List<FileSystemInfo>();
            if (curDir.Parent != null) {
                contents.Add(curDir.Parent);
            }
            contents.AddRange(curDir.EnumerateDirectories());
            contents.AddRange(curDir.EnumerateFiles(filePattern));
            return contents;
        }

        private static bool isDir(FileSystemInfo fi)
            => fi.Attributes.HasFlag(FileAttributes.Directory);

        /// <summary>
        /// Check whether two file system references are equal.
        /// Amazingly and shockingly, this seems not to exist in System.IO.
        /// </summary>
        /// <param name="a">First  object to compare</param>
        /// <param name="b">Second object to compare</param>
        /// <returns>
        /// True if they're the same file/directory, false otherwise.
        /// </returns>
        private static bool pathEquals(FileSystemInfo? a, FileSystemInfo? b)
        {
            if (a == null || b == null) {
                return false;
            }
            if (a == b) {
                // If the references are the same, we're done
                return true;
            }
            string pathA = a.FullName;
            string pathB = b.FullName;
            if (Platform.IsWindows) {
                // Case insensitive
                pathA = pathA.ToLower();
                pathB = pathB.ToLower();
            }
            return pathA == pathB;
        }

        private string getLength(FileSystemInfo fi)
            => isDir(fi)
                ? Properties.Resources.FileSelectDirSize
                : fi is FileInfo file
                    ? CkanModule.FmtSize(file.Length)
                    : Properties.Resources.FileSelectDirSize;

        private long totalChosenSize()
        {
            // Add up sizes of chosen files
            long val = 0;
            foreach (FileInfo f in chosenFiles) {
                val += f.Length;
            }
            return val;
        }

        private string getRowSymbol(FileSystemInfo fi)
            => !isDir(fi)
                && fi is FileInfo file
                && chosenFiles.Contains(file)
                ? chosen
                : "";

        private string getRowName(FileSystemInfo fi)
        {
            if (isDir(fi)) {
                // Return /path/ or \path\ to show it's a dir
                if (pathEquals(curDir.Parent, fi)) {
                    // Treat parent as a special case
                    return $"..{Path.DirectorySeparatorChar}";
                } else {
                    return $"{fi.Name}{Path.DirectorySeparatorChar}";
                }
            } else {
                return fi.Name;
            }
        }

        private int compareNames(FileSystemInfo a, FileSystemInfo b)
        {
            if (isDir(a)) {
                if (isDir(b)) {
                    if (pathEquals(curDir.Parent, a)) {
                        // Sort .. to the top
                        return -1;
                    } else if (pathEquals(curDir.Parent, b)) {
                        return 1;
                    } else {
                        // Both regular directories, just compare the names
                        return a.Name.CompareTo(b.Name);
                    }
                } else {
                    return -1;
                }
            } else {
                if (isDir(b)) {
                    return 1;
                } else {
                    // Both files, just compare the names
                    return a.Name.CompareTo(b.Name);
                }
            }
        }

        private          List<FileSystemInfo>           contents;
        private readonly ConsoleField                   pathField;
        private readonly ConsoleListBox<FileSystemInfo> fileList;
        private          DirectoryInfo                  curDir;

        private readonly HashSet<FileInfo> chosenFiles = new HashSet<FileInfo>();

        private readonly string filePattern;

        private static readonly string chosen  = Symbols.checkmark;

        private const  int idealW = 76;
        private static int labelW => Properties.Resources.FileSelectDirectory.Length;
        private const  int hPad   = 2;
        private const  int top    =  2;
        private const  int bottom = -3;
    }

}
