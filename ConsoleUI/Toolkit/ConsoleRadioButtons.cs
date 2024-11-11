using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// A group of radio buttons that let the user choose one of several options
    /// </summary>
    /// <typeparam name="RowT">Type of object represented by each option</typeparam>
    public class ConsoleRadioButtons<RowT> : ScreenObject {

        /// <summary>
        /// Initialize a radio button group
        /// </summary>
        /// <param name="l">X coordinate of left edge</param>
        /// <param name="t">Y coordinate of top edge</param>
        /// <param name="r">X coordinate of right edge</param>
        /// <param name="b">Y coordinate of bottom edge</param>
        /// <param name="header">Label to show above the list</param>
        /// <param name="dataList">Values represented by the radio buttons</param>
        /// <param name="value">Initially selected value</param>
        public ConsoleRadioButtons(int l, int t, int r, int b,
                                   string      header,
                                   IList<RowT> dataList,
                                   RowT        value)
            : base(l, t, r, b)
        {
            rows          = dataList;
            selectedRow   = rows.IndexOf(value);
            this.header   = header;
        }

        /// <summary>
        /// Fired when the user picks a different radio button
        /// </summary>
        public event Action? SelectionChanged;

        /// <summary>
        /// Currently selected row's object
        /// </summary>
        public RowT Selection => rows[selectedRow];

        /// <summary>
        /// Handle key bindings for the list box.
        /// Mostly moving around wiht cursor keys.
        /// </summary>
        /// <param name="k">Key the user pressed</param>
        public override void OnKeyPress(ConsoleKeyInfo k)
        {
            switch (k.Key) {
                case ConsoleKey.UpArrow:
                    if (selectedRow > 0) {
                        --selectedRow;
                        SelectionChanged?.Invoke();
                    }
                    break;
                case ConsoleKey.DownArrow:
                    if (selectedRow < rows.Count - 1) {
                        ++selectedRow;
                        SelectionChanged?.Invoke();
                    }
                    break;
                case ConsoleKey.Home:
                    selectedRow = 0;
                    SelectionChanged?.Invoke();
                    break;
                case ConsoleKey.End:
                    selectedRow = rows.Count - 1;
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
                        // Find first row after current, wrap
                        int startRow = forward
                            ? selectedRow + 1
                            : selectedRow + rows.Count - 1;
                        for (int i = 0; i < rows.Count; ++i) {
                            int candidateRow = (forward
                                ? startRow + i
                                : startRow + rows.Count - i
                            ) % rows.Count;
                            if (nonAlphaNumPrefix.Replace(Renderer(rows[candidateRow]), "")
                                                 .StartsWith($"{k.KeyChar}",
                                                             StringComparison.CurrentCultureIgnoreCase)) {
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
        /// Move the screen cursor to the middle the active radio button
        /// </summary>
        public override void PlaceCursor()
        {
            Console.SetCursorPosition(GetLeft() + 2, GetTop() + selectedRow + 1);
        }

        /// <inheritdoc/>
        public override void Draw(ConsoleTheme theme, bool focused)
        {
            int l = GetLeft(), r = GetRight(),
                t = GetTop(),  b = GetBottom(),
                w = r - l + 1;

            // Prevent selection from running off the end of the list
            if (selectedRow > rows.Count - 1) {
                selectedRow = rows.Count - 1;
            }

            // Ensure selection is not before the top of the list
            if (selectedRow < 0) {
                selectedRow = 0;
            }

            Console.SetCursorPosition(l, t);
            Console.BackgroundColor = theme.MainBg;
            Console.ForegroundColor = theme.RadioButtonsHeaderFg;
            Console.Write(FormatExactWidth(header, w));

            Console.BackgroundColor = theme.RadioButtonsGroupBg;
            Console.ForegroundColor = theme.RadioButtonsGroupFg;
            for (int index = 0, y = t + 1; index < rows.Count && y <= b; ++index, ++y) {
                Console.SetCursorPosition(l, y);
                Console.Write(" ({0}) {1} ",
                              index == selectedRow ? Symbols.dot : " ",
                              FormatExactWidth(Renderer(rows[index]), w - UIWidth));
            }
        }

        /// <summary>
        /// The number of extra characters we draw per line in addition to the value strings
        /// </summary>
        protected const int UIWidth = 6;

        /// <summary>
        /// Generate a display string for a given row
        /// </summary>
        /// <param name="row">The row to display</param>
        /// <returns>A string representing the given row</returns>
        protected virtual string Renderer(RowT row) => row?.ToString() ?? "";

        private readonly IList<RowT> rows;
        private          int         selectedRow;
        private readonly string      header;

        private static readonly Regex nonAlphaNumPrefix =
            new Regex("^[^a-zA-Z0-9]*",
                      RegexOptions.Compiled);
    }
}
