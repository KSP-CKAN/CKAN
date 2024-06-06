using System;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

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
        /// Fired when the user changes the selection with the arrow keys
        /// </summary>
        public event Action SelectionChanged;

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
        public RowT Selection
            => selectedRow >= 0 && selectedRow < (sortedFilteredData?.Count ?? 0)
                ? sortedFilteredData[selectedRow]
                : default;

        /// <returns>
        /// Return the number of rows shown in the box
        /// </returns>
        public int VisibleRowCount() => sortedFilteredData?.Count ?? 0;

        /// <summary>
        /// Draw the list box
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="focused">Framework parameter not relevant to this object</param>
        public override void Draw(ConsoleTheme theme, bool focused)
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

            var remainingWidth = contentR - l - 1
                                 - columns.Select(col => col.Width ?? 0)
                                          .Sum()
                                 - (padding.Length * (columns.Count - 1));
            var autoWidthCount = columns.Count(col => !col.Width.HasValue);
            var autoWidth = autoWidthCount > 0 && remainingWidth > 0
                                ? remainingWidth / autoWidthCount
                                : 1;

            for (int y = 0, index = topRow - 1; y < h; ++y, ++index) {
                Console.SetCursorPosition(l, t + y);
                if (y == 0) {
                    Console.BackgroundColor = theme.ListBoxHeaderBg;
                    Console.ForegroundColor = theme.ListBoxHeaderFg;
                    Console.Write(" ");
                    for (int i = 0; i < columns.Count; ++i) {
                        ConsoleListBoxColumn<RowT> col = columns[i];
                        if (i > 0) {
                            Console.Write(padding);
                        }
                        // Truncate to designated size of the ListBox
                        int maxW = r - Console.CursorLeft + 1;
                        if (maxW > 0) {
                            var w = col.Width ?? autoWidth;
                            Console.Write(FmtHdr(
                                i,
                                w < maxW ? w : maxW
                            ));
                        }
                    }
                } else if (index >= 0 && index < sortedFilteredData.Count) {
                    if (topRow + y - 1 == selectedRow) {
                        Console.BackgroundColor = theme.ListBoxSelectedBg;
                        Console.ForegroundColor = theme.ListBoxSelectedFg;
                    } else {
                        Console.BackgroundColor = theme.ListBoxUnselectedBg;
                        Console.ForegroundColor = theme.ListBoxUnselectedFg;
                    }
                    Console.Write(" ");
                    for (int i = 0; i < columns.Count; ++i) {
                        ConsoleListBoxColumn<RowT> col = columns[i];
                        if (i > 0) {
                            Console.Write(padding);
                        }
                        // Truncate to designated size of the ListBox
                        int maxW = contentR - Console.CursorLeft + 1;
                        if (maxW > 0) {
                            var w = col.Width ?? autoWidth;
                            Console.Write(FormatExactWidth(
                                col.Renderer(sortedFilteredData[index]).Trim(),
                                w < maxW ? w : maxW
                            ));
                        }
                    }
                } else {
                    Console.BackgroundColor = theme.ListBoxUnselectedBg;
                    Console.ForegroundColor = theme.ListBoxUnselectedFg;
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
                    theme,
                    r, t + scrollTop, b,
                    sortedFilteredData.Count > 0
                        ? t + 1 + scrollTop + ((h - 2 - scrollTop) * selectedRow / sortedFilteredData.Count)
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
                        SelectionChanged?.Invoke();
                    }
                    break;
                case ConsoleKey.DownArrow:
                    if (selectedRow < sortedFilteredData.Count - 1) {
                        ++selectedRow;
                        SelectionChanged?.Invoke();
                    }
                    break;
                case ConsoleKey.PageUp:
                    if (selectedRow > h) {
                        selectedRow -= h;
                    } else {
                        selectedRow = 0;
                    }
                    SelectionChanged?.Invoke();
                    break;
                case ConsoleKey.PageDown:
                    if (selectedRow < sortedFilteredData.Count - 1 - h) {
                        selectedRow += h;
                    } else {
                        selectedRow = sortedFilteredData.Count - 1;
                    }
                    SelectionChanged?.Invoke();
                    break;
                case ConsoleKey.Home:
                    selectedRow = 0;
                    SelectionChanged?.Invoke();
                    break;
                case ConsoleKey.End:
                    selectedRow = sortedFilteredData.Count - 1;
                    SelectionChanged?.Invoke();
                    break;
                case ConsoleKey.Tab:
                    Blur(!k.Modifiers.HasFlag(ConsoleModifiers.Shift));
                    break;
                default:
                    // Go backwards if k.Modifiers.HasFlag(ConsoleModifiers.Shift)
                    if (!char.IsControl(k.KeyChar)
                            && (k.Modifiers | ConsoleModifiers.Shift) == ConsoleModifiers.Shift) {

                        bool forward = !k.Modifiers.HasFlag(ConsoleModifiers.Shift);
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
                                SelectionChanged?.Invoke();
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
                        Properties.Resources.Ascending, "",
                        Properties.Resources.AscendingSortTip,
                        true,
                        (ConsoleTheme theme) => {
                            SortDirection = ListSortDirection.Ascending;
                            return true;
                        },
                        () => sortDir == ListSortDirection.Ascending
                    ),
                    new ConsoleMenuOption(
                        Properties.Resources.Descending, "",
                        Properties.Resources.DescendingSortTip,
                        true,
                        (ConsoleTheme theme) => {
                            SortDirection = ListSortDirection.Descending;
                            return true;
                        },
                        () => sortDir == ListSortDirection.Descending
                    ),
                    null
                };
                for (int i = 0; i < columns.Count; ++i) {
                    // Our menus' lambas will share 'i' unless we capture it
                    int newIndex = i;
                    opts.Add(new ConsoleMenuOption(
                        string.IsNullOrEmpty(columns[i].Header)
                            ? string.Format(Properties.Resources.ColumnNumber, i + 1)
                            : columns[i].Header,
                        "",
                        string.IsNullOrEmpty(columns[i].Header)
                            ? string.Format(Properties.Resources.ColumnNumberSortTip, i + 1)
                            : string.Format(Properties.Resources.ColumnNameSortTip, columns[i].Header),
                        true,
                        (ConsoleTheme theme) => {
                            SortColumnIndex = newIndex;
                            return true;
                        },
                        () => sortColIndex == newIndex
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
        /// <param name="resetSelection">If true, select the top row after refreshing</param>
        public void SetData(IList<RowT> newData, bool resetSelection = false)
        {
            data = newData;
            filterAndSort();
            if (resetSelection) {
                selectedRow = 0;
            }
        }

        private string FmtHdr(int colIndex, int w)
            => colIndex == sortColIndex
                ? FormatExactWidth(
                    columns[colIndex].Header + " "
                        + (sortDir == ListSortDirection.Ascending ? sortUp : sortDown),
                    w)
                : FormatExactWidth(columns[colIndex].Header, w);

        private void filterAndSort()
        {
            // Keep the same row highlighted when the number of rows changes
            RowT oldSelect = Selection;

            sortedFilteredData = string.IsNullOrEmpty(filterStr) || filterCheck == null
                ? new List<RowT>(data)
                : new List<RowT>(data).FindAll(r => filterCheck(r, filterStr));
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
            => ascending
                ? col.Comparer
                    ?? ((a, b) => col.Renderer(a).Trim().CompareTo(col.Renderer(b).Trim()))
                : col.Comparer != null
                    ? (Comparison<RowT>)((RowT a, RowT b) => col.Comparer(b, a))
                    : ((RowT a, RowT b) => col.Renderer(b).Trim().CompareTo(col.Renderer(a).Trim()));

        // Sometimes type safety can be a minor hindrance;
        // this would just be "first || second" in C
        private int IntOr(Func<int> first, Func<int> second)
        {
            int a = first();
            return a != 0 ? a : second();
        }

        private          List<RowT>                 sortedFilteredData;
        private          IList<RowT>                data;
        private readonly IList<ConsoleListBoxColumn<RowT>> columns;
        private readonly Func<RowT, string, bool>   filterCheck;
        private          ConsolePopupMenu           sortMenu;

        private readonly int               defaultSortColumn = 0;
        private          int               sortColIndex;
        private          ListSortDirection sortDir;
        private          string            filterStr         = "";

        private int topRow      = 0;
        private int selectedRow = 0;

        private const int scrollTop = 1;

        private static readonly Regex nonAlphaNumPrefix = new Regex("^[^a-zA-Z0-9]*", RegexOptions.Compiled);

        private const string sortUp   = "^";
        private const string sortDown = "v";
        private const string padding  = "  ";
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
        /// Number of screen columns to use for this column.
        /// If null, take up remaining space left behind by other columns.
        /// </summary>
        public int?               Width;
    }

}
