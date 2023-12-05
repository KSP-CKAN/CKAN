using System;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Object representing an editable text field
    /// </summary>
    public class ConsoleField : ScreenObject {

        /// <summary>
        /// Initialize the text fields
        /// </summary>
        /// <param name="l">X coordinate of left edge</param>
        /// <param name="t">Y coordinate of top edge</param>
        /// <param name="r">X coordinate of right edge</param>
        /// <param name="val">Initial value of the text</param>
        public ConsoleField(int l, int t, int r, string val = "")
            : base(l, t, r, t)
        {
            if (!string.IsNullOrEmpty(val)) {
                Value = val;
                Position = Value.Length;
            }
        }

        /// <summary>
        /// Function returning text to show when field is empty
        /// </summary>
        public Func<string> GhostText = () => Properties.Resources.DefaultGhostText;
        /// <summary>
        /// Current value displayed in field
        /// </summary>
        public string       Value     = "";
        /// <summary>
        /// Position of cursor within field
        /// </summary>
        public int          Position  = 0;

        private int leftPos = 0;

        /// <summary>
        /// Type for event to notify that the text has changed
        /// </summary>
        public delegate void ChangeListener(ConsoleField sender, string newValue);
        /// <summary>
        /// Event to notify that the text has changed
        /// </summary>
        public event ChangeListener OnChange;
        private void Changed()
        {
            OnChange?.Invoke(this, Value);
        }

        /// <summary>
        /// Reset the value in the field
        /// </summary>
        public void Clear()
        {
            Position = 0;
            Value    = "";
            Changed();
        }

        /// <summary>
        /// Draw the field
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="focused">If true, draw with focus, else without focus</param>
        public override void Draw(ConsoleTheme theme, bool focused)
        {
            int w = GetRight() - GetLeft() + 1;

            if (Position > Value.Length) {
                Position = Value.Length;
            }
            if (leftPos > Position) {
                leftPos = Position;
            }
            if (leftPos < Position - w + 1) {
                leftPos = Position - w + 1;
            }

            Console.SetCursorPosition(GetLeft(), GetTop());
            Console.BackgroundColor = theme.FieldBg;
            if (string.IsNullOrEmpty(Value)) {
                Console.ForegroundColor = theme.FieldGhostFg;
                Console.Write(GhostText().PadRight(w));
            } else {
                Console.ForegroundColor = focused
                    ? theme.FieldFocusedFg
                    : theme.FieldBlurredFg;
                Console.Write(FormatExactWidth(Value.Substring(leftPos), w));
            }
        }

        /// <summary>
        /// Handle key bindings for the field.
        /// Mostly cursor movement and adding/removing characters.
        /// </summary>
        /// <param name="k">Key the user pressed</param>
        public override void OnKeyPress(ConsoleKeyInfo k)
        {
            switch (k.Key) {
                case ConsoleKey.Escape:
                    if (!string.IsNullOrEmpty(Value)) {
                        Clear();
                    }
                    break;
                case ConsoleKey.Backspace:
                    if (!k.Modifiers.HasFlag(ConsoleModifiers.Control)) {
                        if (Position > 0) {
                            --Position;
                            Value = Value.Substring(0, Position) + Value.Substring(Position + 1);
                            Changed();
                        }
                    } else if (!string.IsNullOrEmpty(Value)) {
                        Value    = Value.Substring(Position);
                        Position = 0;
                        Changed();
                    }
                    break;
                case ConsoleKey.Delete:
                    if (Position < Value.Length) {
                        Value = k.Modifiers.HasFlag(ConsoleModifiers.Control)
                            ? Value.Substring(0, Position)
                            : Value.Substring(0, Position) + Value.Substring(Position + 1);
                        Changed();
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    if (Position > 0) {
                        --Position;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (Position < Value.Length) {
                        ++Position;
                    }
                    break;
                case ConsoleKey.UpArrow:
                    Blur(false);
                    break;
                case ConsoleKey.DownArrow:
                    Blur(true);
                    break;
                case ConsoleKey.Home:
                    Position = 0;
                    break;
                case ConsoleKey.End:
                    Position = Value.Length;
                    break;
                case ConsoleKey.Tab:
                    Blur(!k.Modifiers.HasFlag(ConsoleModifiers.Shift));
                    break;
                default:
                    if (!char.IsControl(k.KeyChar)) {
                        if (Position < Value.Length) {
                            Value = Value.Substring(0, Position) + k.KeyChar + Value.Substring(Position);
                        } else {
                            Value += k.KeyChar;
                        }
                        ++Position;
                        Changed();
                    }
                    break;
            }
        }

        /// <summary>
        /// Place the screen cursor so it corresponds to Position
        /// </summary>
        public override void PlaceCursor()
        {
            Console.SetCursorPosition(GetLeft() - leftPos + Position, GetTop());
        }

    }

}
