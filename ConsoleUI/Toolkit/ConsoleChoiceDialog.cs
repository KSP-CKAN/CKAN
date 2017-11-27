using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Dialog showing a message and letting the user choose an option from a list box
    /// </summary>
    public class ConsoleChoiceDialog<ChoiceT> : ConsoleDialog {

        /// <summary>
        /// Initialize the Dialog
        /// </summary>
        /// <param name="m">Message to show</param>
        /// <param name="hdr">Text for column header of list box</param>
        /// <param name="c">List of objects to put in the list box</param>
        /// <param name="renderer">Function to generate text for each option</param>
        /// <param name="comparer">Optional function to sort the rows</param>
        public ConsoleChoiceDialog(string m, string hdr, List<ChoiceT> c, Func<ChoiceT, string> renderer, Comparison<ChoiceT> comparer = null)
            : base()
        {
            int l = GetLeft(),
                r = GetRight();
            int w = r - l + 1;

            // Resize the window to fit the content
            List<string> msgLines = Formatting.WordWrap(m, w - 4);

            int h = 2 + msgLines.Count + 1 + 1 + c.Count + 2;
            int t = (Console.WindowHeight - h) / 2;
            int b = t + h - 1;

            SetDimensions(l, t, r, b);

            // Wrapped message at top
            ConsoleTextBox tb = new ConsoleTextBox(
                l + 2, t + 2, r - 2, t + 2 + msgLines.Count - 1,
                false,
                TextAlign.Left,
                () => ConsoleTheme.Current.PopupBg,
                () => ConsoleTheme.Current.PopupFg
            );
            AddObject(tb);
            tb.AddLine(m);

            // ConsoleListBox<ChoiceT> of choices at bottom
            choices = new ConsoleListBox<ChoiceT>(
                l + 2, t + 2 + msgLines.Count + 1, r - 2, b - 2,
                c,
                new List<ConsoleListBoxColumn<ChoiceT>>() {
                    new ConsoleListBoxColumn<ChoiceT>() {
                        Header   = hdr,
                        Width    = w - 6,
                        Renderer = renderer,
                        Comparer = comparer
                    }
                },
                0, 0, ListSortDirection.Ascending
            );

            choices.AddTip("Enter", "Accept");
            choices.AddBinding(Keys.Enter, (object sender) => {
                return false;
            });

            choices.AddTip("Esc", "Cancel");
            choices.AddBinding(Keys.Escape, (object sender) => {
                cancelled = true;
                return false;
            });
            AddObject(choices);

        }

        /// <summary>
        /// Display the dialog and handle its interaction
        /// </summary>
        /// <param name="process">Function to control the dialog, default is normal user interaction</param>
        /// <returns>
        /// Row user selected
        /// </returns>
        public new ChoiceT Run(Action process = null)
        {
            base.Run(process);
            return cancelled ? default(ChoiceT) : choices.Selection;
        }

        private ConsoleListBox<ChoiceT> choices;
        private bool                    cancelled;
    }

}
