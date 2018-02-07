using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// A popup to let the user select multiple files.
    /// </summary>
    public class ConsoleFileMultiSelectDialog : ConsoleDialog {

        /// <summary>
        /// Initialize the popup.
        /// </summary>
        /// <param name="title">String to be shown in the top center of the popup</param>
        /// <param name="startPath">Path of directory to start in</param>
        /// <param name="filPat">Glob-style wildcard string for matching files to show</param>
        /// <param name="toggleHeader">Header for the column with checkmarks for selected files</param>
        public ConsoleFileMultiSelectDialog(string title, string startPath, string filPat, string toggleHeader)
            : base()
        {
            CenterHeader = () => title;
            curDir       = new DirectoryInfo(startPath);
            filePattern  = filPat;

            int w = (Console.WindowWidth > idealW + 2 * hPad)
                ? idealW
                : Console.WindowWidth - 2 * hPad;
            int left  = (Console.WindowWidth - w) / 2;
            int right = -left;

            SetDimensions(left, top, right, bottom);

            AddObject(new ConsoleLabel(
                left + 2, top + 2, left + 2 + labelW - 1,
                () => $"Directory:",
                () => ConsoleTheme.Current.PopupBg,
                () => ConsoleTheme.Current.PopupFg
            ));

            pathField = new ConsoleField(
                left + 2 + labelW, top + 2, right - 2,
                curDir.FullName
            );
            pathField.OnChange += pathFieldChanged;
            AddObject(pathField);

            AddObject(new ConsoleLabel(
                left + 2, bottom - 1, right - 2,
                () => $"{chosenFiles.Count} selected, {CkanModule.FmtSize(totalChosenSize())}",
                () => ConsoleTheme.Current.PopupBg,
                () => ConsoleTheme.Current.PopupFg
            ));

            // ListBox showing zip files in current dir
            fileList = new ConsoleListBox<FileSystemInfo>(
                left + 2, top + 4, right - 2, bottom - 2,
                getFileList(),
                new List<ConsoleListBoxColumn<FileSystemInfo>>() {
                    new ConsoleListBoxColumn<FileSystemInfo>() {
                        Header   = toggleHeader,
                        Width    = 8,
                        Renderer = getRowSymbol
                    }, new ConsoleListBoxColumn<FileSystemInfo>() {
                        Header   = "Name",
                        Width    = 36,
                        Renderer = getRowName,
                        Comparer = compareNames
                    }, new ConsoleListBoxColumn<FileSystemInfo>() {
                        Header   = "Size",
                        // Longest: "1023.1 KB"
                        Width    = 9,
                        Renderer = (FileSystemInfo fi) => getLength(fi),
                        Comparer = (a, b) => {
                            FileInfo fa = a as FileInfo, fb = b as FileInfo;
                            return fa == null
                                ? (fb == null ? 0 : -1)
                                : (fb == null ? 1 : fa.Length.CompareTo(fb.Length));
                        }
                    }, new ConsoleListBoxColumn<FileSystemInfo>() {
                        Header   = "Accessed",
                        Width    = 10,
                        Renderer = (FileSystemInfo fi) => fi.LastWriteTime.ToString("yyyy-MM-dd"),
                        Comparer = (a, b) => a.LastWriteTime.CompareTo(b.LastWriteTime)
                    }
                },
                1, 1, ListSortDirection.Ascending
            );
            AddObject(fileList);

            AddTip("Esc", "Cancel");
            AddBinding(Keys.Escape, (object sender) => {
                chosenFiles.Clear();
                return false;
            });

            AddTip("F10", "Sort");
            AddBinding(Keys.F10, (object sender) => {
                fileList.SortMenu().Run(right - 2, top + 2);
                DrawBackground();
                return true;
            });

            AddTip("Enter", "Change directory", () => fileList.Selection != null &&  isDir(fileList.Selection));
            AddTip("Enter", "Select",           () => fileList.Selection != null && !isDir(fileList.Selection));
            AddBinding(Keys.Enter, (object sender) => selectRow());
            AddBinding(Keys.Space, (object sender) => selectRow());

            AddTip("Ctrl+A", "Select all");
            AddBinding(Keys.CtrlA, (object sender) => {
                foreach (FileSystemInfo fi in contents) {
                    if (!isDir(fi)) {
                        FileInfo file = fi as FileInfo;
                        if (file != null) {
                            chosenFiles.Add(file);
                        }
                    }
                }
                return true;
            });

            AddTip("Ctrl+D", "Deselect all", () => chosenFiles.Count > 0);
            AddBinding(Keys.CtrlD, (object sender) => {
                if (chosenFiles.Count > 0) {
                    chosenFiles.Clear();
                }
                return true;
            });

            AddTip("F9", "Import", () => chosenFiles.Count > 0);
            AddBinding(Keys.F9, (object sender) => {
                return false;
            });
        }

        private bool selectRow()
        {
            if (isDir(fileList.Selection)) {
                DirectoryInfo di = fileList.Selection as DirectoryInfo;
                if (di != null) {
                    curDir = di;
                    pathField.Value = curDir.FullName;
                    fileList.SetData(getFileList());
                }
            } else {
                FileInfo fi = fileList.Selection as FileInfo;
                if (fi != null) {
                    if (chosenFiles.Contains(fi)) {
                        chosenFiles.Remove(fi);
                    } else {
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
        public new HashSet<FileInfo> Run(Action process = null)
        {
            base.Run(process);
            return chosenFiles;
        }

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
        {
            return (fi.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }

        /// <summary>
        /// Check whether two file system references are equal.
        /// Amazingly and shockingly, this seems not to exist in System.IO.
        /// </summary>
        /// <param name="a">First  object to compare</param>
        /// <param name="b">Second object to compare</param>
        /// <returns>
        /// True if they're the same file/directory, false otherwise.
        /// </returns>
        private static bool pathEquals(FileSystemInfo a, FileSystemInfo b)
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
        {
            if (isDir(fi)) {
                return dirSize;
            } else {
                FileInfo file = fi as FileInfo;
                if (file != null) {
                    return CkanModule.FmtSize(file.Length);
                } else {
                    return dirSize;
                }
            }
        }

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
        {
            if (!isDir(fi) && chosenFiles.Contains(fi as FileInfo)) {
                return chosen;
            }
            return "";
        }

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

        private List<FileSystemInfo>           contents;
        private ConsoleField                   pathField;
        private ConsoleListBox<FileSystemInfo> fileList;
        private DirectoryInfo                  curDir;

        private HashSet<FileInfo> chosenFiles = new HashSet<FileInfo>();

        private string filePattern;

        private static readonly string chosen  = Symbols.checkmark;
        private const           string dirSize = "<DIR>";

        private const int idealW = 76;
        private const int labelW = 12;
        private const int hPad   = 2;
        private const int top    =  2;
        private const int bottom = -2;
    }

}
