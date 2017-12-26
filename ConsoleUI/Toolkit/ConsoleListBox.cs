using System;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections.Generic;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Scrollable list of objects
    /// </summary>
    public class ConsoleListBox<RowT> : ScreenObject {

        /// <summary>
        /// Initialize the list box
        /// </summary>
        /// <param name="l">X coordinate of left edge</param>
        /// <param name="t">Y coordinate of top edge</param>
        /// <param name="r">X coordinate of right edge</param>
        /// <param name="b">Y coordinate of bottom edge</param>
        /// <param name="dataList">List of objects to display</param>
        /// <param name="columnList">List of columns for the list</param>
        /// <param name="dfltSortCol">Index of column to use as fallback sort</param>
        /// <param name="initialSortCol">Index of column to sort by</param>
        /// <param name="initialSortDir">Whether initial sort should be ascending or descending</param>
        /// <param name="filt">Function to use for filtering</param>
        public ConsoleListBox(int l, int t, int r, int b,
                IList<RowT> dataList,
                IList<ConsoleListBoxColumn<RowT>> columnList,
                int dfltSortCol,
                int initialSortCol = 0,
                ListSortDirection initialSortDir = ListSortDirection.Ascending,
                Func<RowT, string, bool> filt = null)
            : base(l, t, r, b)
        {
            data              = dataList;
            columns           = columnList;
            filterCheck       = filt;
            defaultSortColumn = dfltSortCol;

            sortColIndex = initialSortCol;
            sortDir      = initialSortDir;

            filterAndSort();
        }

        /// <summary>
        /// Set which column to sort by
        /// </summary>
        public int SortColumnIndex {
            set {
                if (sortColIndex != value) {
                    sortColIndex = value;
                    filterAndSort();
                }
            }
        }

        /// <summary>
        /// Set whether to sort ascending or descending
        /// </summary>
        public ListSortDirection SortDirection {
            set {
                if (sortDir != value) {
                    sortDir = value;
                    filterAndSort();
                }
            }
        }

        /// <summary>
        /// Set the string for filtering
        /// </summary>
        public string FilterString {
            set {
                if (filterStr != value) {
                    filterStr = value;
                    filterAndSort();
                }
            }
        }

        /// <summary>
        /// Currently selected row's object
        /// </summary>
        public RowT Selection {
            get {
                if (selectedRow >= 0 && selectedRow < (sortedFilteredData?.Count ?? 0)) {
                    return sortedFilteredData[selectedRow];
                } else {
                    return default (RowT);
                }
            }
        }

        /// <returns>
        /// Return the number of rows shown in the box
        /// </returns>
        public int VisibleRowCount() { return sortedFilteredData?.Count ?? 0; }

        /// <summary>
        /// Draw the list box
        /// </summary>
        /// <param name="focused">Framework parameter not relevant to this object</param>
        public override void Draw(bool focused)
        {
            int l = GetLeft(), r = GetRight(),
                t = GetTop(),  b = GetBottom(), h = b - t + 1;

            bool needScrollbar = (sortedFilteredData.Count >= h);
            int  contentR      = needScrollbar ? r - 1 : r;

            // Prevent selection from running off the end of the list
            if (selectedRow > sortedFilteredData.Count - 1) {
                selectedRow = sortedFilteredData.Count - 1;
            }

            // Ensure selection is not before the top of the list
            if (selectedRow < 0) {
                selectedRow = 0;
            }

            // Don't scroll past the bottom of the list
            if (topRow > sortedFilteredData.Count - h + 1) {
                topRow = sortedFilteredData.Count - h + 1;
            }

            // Don't scroll before the top of the list
            if (topRow < 0) {
                topRow = 0;
            }

            if (topRow > selectedRow) {
                // Scroll up to reveal selected row
                topRow = selectedRow;
            } else if (topRow < selectedRow - h + 2) {
                // Scroll down to reveal selected row
                topRow = selectedRow - h + 2;
            }

            for (int y = 0, index = topRow - 1; y < h; ++y, ++index) {
                Console.SetCursorPosition(l, t + y);
                if (y == 0) {
                    Console.BackgroundColor = ConsoleTheme.Current.ListBoxHeaderBg;
                    Console.ForegroundColor = ConsoleTheme.Current.ListBoxHeaderFg;
                    Console.Write(" ");
                    for (int i = 0; i < columns.Count; ++i) {
                        ConsoleListBoxColumn<RowT> col = columns[i];
                        if (i > 0) {
                            Console.Write("  ");
                        }
                        // Truncate to designated size of the ListBox
                        int maxW = r - Console.CursorLeft + 1;
                        if (maxW > 0) {
                            Console.Write(FmtHdr(
                                i,
                                col.Width < maxW ? col.Width : maxW
                            ));
                        }
                    }
                } else if (index >= 0 && index < sortedFilteredData.Count) {
                    if (topRow + y - 1 == selectedRow) {
                        Console.BackgroundColor = ConsoleTheme.Current.ListBoxSelectedBg;
                        Console.ForegroundColor = ConsoleTheme.Current.ListBoxSelectedFg;
                    } else {
                        Console.BackgroundColor = ConsoleTheme.Current.ListBoxUnselectedBg;
                        Console.ForegroundColor = ConsoleTheme.Current.ListBoxUnselectedFg;
                    }
                    Console.Write(" ");
                    for (int i = 0; i < columns.Count; ++i) {
                        ConsoleListBoxColumn<RowT> col = columns[i];
                        if (i > 0) {
                            Console.Write("  ");
                        }
                        // Truncate to designated size of the ListBox
                        int maxW = contentR - Console.CursorLeft + 1;
                        if (maxW > 0) {
                            Console.Write(FormatExactWidth(
                                col.Renderer(sortedFilteredData[index]).Trim(),
                                col.Width < maxW ? col.Width : maxW
                            ));
                        }
                    }
                } else {
                    Console.BackgroundColor = ConsoleTheme.Current.ListBoxUnselectedBg;
                    Console.ForegroundColor = ConsoleTheme.Current.ListBoxUnselectedFg;
                }
                try {
                    if (y == 0) {
                        Console.Write("".PadRight(r - Console.CursorLeft + 1));
                    } else {
                        // Make space for scrollbar for anti-flicker
                        Console.Write("".PadRight(contentR - Console.CursorLeft + 1));
                    }
                } catch { }
            }

            // Now draw the scrollbar
            if (needScrollbar) {
                DrawScrollbar(
                    r, t + scrollTop, b,
                    sortedFilteredData.Count > 0
                        ? t + 1 + scrollTop + (h - 2 - scrollTop) * selectedRow / sortedFilteredData.Count
                        : -1
                );
            }
        }

        /// <summary>
        /// Handle key bindings for the list box.
        /// Mostly moving around wiht cursor keys.
        /// </summary>
        /// <param name="k">Key the user pressed</param>
        public override void OnKeyPress(ConsoleKeyInfo k)
        {
            int h = GetBottom() - GetTop();
            switch (k.Key) {
                case ConsoleKey.UpArrow:
                    if (selectedRow > 0) {
                        --selectedRow;
                    }
                    break;
                case ConsoleKey.DownArrow:
                    if (selectedRow < sortedFilteredData.Count - 1) {
                        ++selectedRow;
                    }
                    break;
                case ConsoleKey.PageUp:
                    if (selectedRow > h) {
                        selectedRow -= h;
                    } else {
                        selectedRow = 0;
                    }
                    break;
                case ConsoleKey.PageDown:
                    if (selectedRow < sortedFilteredData.Count - 1 - h) {
                        selectedRow += h;
                    } else {
                        selectedRow = sortedFilteredData.Count - 1;
                    }
                    break;
                case ConsoleKey.Home:
                    selectedRow = 0;
                    break;
                case ConsoleKey.End:
                    selectedRow = sortedFilteredData.Count - 1;
                    break;
                case ConsoleKey.Tab:
                    Blur((k.Modifiers & ConsoleModifiers.Shift) == 0);
                    break;
                default:
                    // Go backwards if (k.Modifiers & ConsoleModifiers.Shift)
                    if (!Char.IsControl(k.KeyChar)
                            && (k.Modifiers | ConsoleModifiers.Shift) == ConsoleModifiers.Shift) {

                        bool forward = (k.Modifiers & ConsoleModifiers.Shift) == 0;
                        Func<RowT, string> dfltRend = columns[defaultSortColumn].Renderer;
                        // Find first row after current, wrap
                        int startRow = forward
                            ? selectedRow + 1
                            : selectedRow + sortedFilteredData.Count - 1;
                        for (int i = 0; i < sortedFilteredData.Count; ++i) {
                            int candidateRow = (forward
                                ? startRow + i
                                : startRow + sortedFilteredData.Count - i
                            ) % sortedFilteredData.Count;
                            if (nonAlphaNumPrefix.Replace(
                                    dfltRend(sortedFilteredData[candidateRow]), ""
                                ).IndexOf($"{k.KeyChar}", StringComparison.CurrentCultureIgnoreCase) == 0) {

                                selectedRow = candidateRow;
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Move the screen cursor to the left edge of the active row
        /// </summary>
        public override void PlaceCursor()
        {
            Console.SetCursorPosition(GetLeft(), GetTop() + selectedRow - topRow + 1);
        }

        /// <summary>
        /// Return a popup menu representing the sort options for this list box
        /// </summary>
        public ConsolePopupMenu SortMenu()
        {
            if (sortMenu == null) {
                List<ConsoleMenuOption> opts = new List<ConsoleMenuOption>() {
                    new ConsoleMenuOption(
                        "Ascending", "",
                        "Sort the list in ascending order",
                        true,
                        () => { SortDirection  =  ListSortDirection.Ascending; return true; },
                        () => { return sortDir == ListSortDirection.Ascending;              }
                    ),
                    new ConsoleMenuOption(
                        "Descending", "",
                        "Sort the list in descending order",
                        true,
                        () => { SortDirection  =  ListSortDirection.Descending; return true;},
                        () => { return sortDir == ListSortDirection.Descending;             }
                    ),
                    null
                };
                for (int i = 0; i < columns.Count; ++i) {
                    // Our menus' lambas will share 'i' unless we capture it
                    int newIndex = i;
                    opts.Add(new ConsoleMenuOption(
                        string.IsNullOrEmpty(columns[i].Header)
                            ? $"Column #{i+1}"
                            : columns[i].Header,
                        "",
                        string.IsNullOrEmpty(columns[i].Header)
                            ? $"Sort the list by column #{i+1}"
                            : $"Sort the list by the {columns[i].Header} column",
                        true,
                        () => { SortColumnIndex = newIndex; return true; },
                        () => { return sortColIndex == newIndex;         }
                    ));
                }
                sortMenu = new ConsolePopupMenu(opts);
            }
            return sortMenu;
        }

        /// <summary>
        /// Set the data shown in the list
        /// </summary>
        /// <param name="newData">List of objects to show</param>
        public void SetData(IList<RowT> newData)
        {
            data = newData;
            filterAndSort();
        }

        private string FmtHdr(int colIndex, int w)
        {
            ConsoleListBoxColumn<RowT> col = columns[colIndex];
            if (colIndex == sortColIndex) {
                return FormatExactWidth(
                    col.Header + " " + (sortDir == ListSortDirection.Ascending ? sortUp : sortDown),
                    w
                );
            } else {
                return FormatExactWidth(col.Header, w);
            }
        }

        private void filterAndSort()
        {
            // Keep the same row highlighted when the number of rows changes
            RowT oldSelect = Selection;

            if (string.IsNullOrEmpty(filterStr) || filterCheck == null) {
                sortedFilteredData = new List<RowT>(data);
            } else {
                sortedFilteredData = new List<RowT>(data).FindAll(
                    r => filterCheck(r, filterStr)
                );
            }
            // Semantic sort for versions rather than lexicographical
            if (sortColIndex >= 0 && sortColIndex < columns.Count) {

                Comparison<RowT> sortCol = getComparer(
                    columns[sortColIndex],
                    sortDir == ListSortDirection.Ascending
                );
                Comparison<RowT> dfltCol = getComparer(columns[defaultSortColumn], true);

                sortedFilteredData.Sort((a, b) => IntOr(
                    () => sortCol(a, b),
                    () => dfltCol(a, b)
                ));
            }
            int newSelRow = sortedFilteredData.IndexOf(oldSelect);
            if (newSelRow >= 0) {
                selectedRow = newSelRow;
            }
        }

        private Comparison<RowT> getComparer(ConsoleListBoxColumn<RowT> col, bool ascending)
        {
            if (ascending) {
                return col.Comparer
                    ?? ((a, b) => col.Renderer(a).Trim().CompareTo(col.Renderer(b).Trim()));

            } else if (col.Comparer != null) {
                return (a, b) => col.Comparer(b, a);
            } else {
                return (a, b) => col.Renderer(b).Trim().CompareTo(col.Renderer(a).Trim());
            }
        }

        // Sometimes type safety can be a minor hindrance;
        // this would just be "first || second" in C
        private int IntOr(Func<int> first, Func<int> second)
        {
            int a = first();
            if (a != 0) {
                return a;
            } else {
                return second();
            }
        }

        private List<RowT>                 sortedFilteredData;
        private IList<RowT>                data;
        private IList<ConsoleListBoxColumn<RowT>> columns;
        private Func<RowT, string, bool>   filterCheck;
        private ConsolePopupMenu           sortMenu;

        private int               defaultSortColumn = 0;
        private int               sortColIndex;
        private ListSortDirection sortDir;
        private string            filterStr         = "";

        private int topRow      = 0;
        private int selectedRow = 0;

        private const int scrollTop = 1;

        private static readonly Regex nonAlphaNumPrefix = new Regex("^[^a-zA-Z0-9]*", RegexOptions.Compiled);

        private static readonly string sortUp      = "^";
        private static readonly string sortDown    = "v";
    }

    /// <summary>
    /// Object describing a column in a list box
    /// </summary>
    public class ConsoleListBoxColumn<RowT> {

        /// <summary>
        /// Initialize the column
        /// </summary>
        public ConsoleListBoxColumn() { }

        /// <summary>
        /// Text for the header row
        /// </summary>
        public string             Header;
        /// <summary>
        /// Function to translate a row's object into text to display
        /// </summary>
        public Func<RowT, string> Renderer;
        /// <summary>
        /// Function to compare two rows for sorting purposes.
        /// If not defined, the string representation is used.
        /// </summary>
        public Comparison<RowT>   Comparer;
        /// <summary>
        /// Number of screen columns to use for this column
        /// </summary>
        public int                Width;
    }

}
