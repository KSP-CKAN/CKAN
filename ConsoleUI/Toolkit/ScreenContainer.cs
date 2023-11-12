using System;
using System.Collections.Generic;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Common base class of ConsoleScreen and ConsoleDialog.
    /// Encapsulates the logic for managing a group of ScreenObjects.
    /// </summary>
    public abstract class ScreenContainer {

        /// <summary>
        /// Initialize the container
        /// </summary>
        protected ScreenContainer()
        {
            AddBinding(Keys.CtrlL, (object sender, ConsoleTheme theme) => {
                // Just redraw everything and keep running
                DrawBackground(theme);
                return true;
            });
        }

        /// <summary>
        /// Draw the contained screen objects and manage their interaction
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="process">Logic to drive the screen, default is normal user interaction</param>
        public virtual void Run(ConsoleTheme theme, Action<ConsoleTheme> process = null)
        {
            DrawBackground(theme);

            if (process == null) {
                // This should be a simple default parameter, but C# has trouble
                // with that for instance delegates.
                process = Interact;
            } else {
                // Other classes can't call Draw directly, so do it for them once.
                // Would be nice to make this cleaner somehow.
                Draw(theme);
            }

            // Run the actual logic for the container
            process(theme);

            ClearBackground();
        }

        /// <summary>
        /// Add a ScreenObject for inclusion in this display
        /// </summary>
        /// <param name="so">ScreenObject to Add</param>
        protected void AddObject(ScreenObject so)
        {
            objects.Add(so);
            so.OnBlur += Blur;
        }

        /// <summary>
        /// Delegate type for key bindings
        /// </summary>
        public delegate bool KeyAction(object sender, ConsoleTheme theme);

        /// <summary>
        /// Bind an action to a key
        /// </summary>
        /// <param name="k">Key to bind</param>
        /// <param name="a">Action to bind to the key</param>
        public void AddBinding(ConsoleKeyInfo k, KeyAction a)
        {
            if (bindings.ContainsKey(k)) {
                bindings[k] = a;
            } else {
                bindings.Add(k, a);
            }
        }

        /// <summary>
        /// Add custom key bindings
        /// </summary>
        /// <param name="keys">Keys to bind</param>
        /// <param name="a">Action to bind to key</param>
        public void AddBinding(IEnumerable<ConsoleKeyInfo> keys, KeyAction a)
        {
            foreach (ConsoleKeyInfo k in keys) {
                AddBinding(k, a);
            }
        }

        /// <summary>
        /// Add a screen tip to show in the FooterBg
        /// </summary>
        /// <param name="key">User readable description of the key</param>
        /// <param name="descrip">Description of the action</param>
        /// <param name="displayIf">Function returning true to show the tip or false to hide it</param>
        public void AddTip(string key, string descrip, Func<bool> displayIf = null)
        {
            if (displayIf == null) {
                displayIf = () => true;
            }
            tips.Add(new ScreenTip(key, descrip, displayIf));
        }

        /// <summary>
        /// Draw the basic background elements of the display.
        /// Called once at the beginning and then again later if we need to reset the display.
        /// NOT called every tick, to reduce flickering.
        /// </summary>
        protected virtual void DrawBackground(ConsoleTheme theme)  { }

        /// <summary>
        /// Reset the display, called when closing it.
        /// </summary>
        protected virtual void ClearBackground() { }

        /// <summary>
        /// Draw all the contained ScreenObjects.
        /// Also places the cursor where it should be.
        /// </summary>
        protected void Draw(ConsoleTheme theme)
        {
            lock (screenLock) {
                Console.CursorVisible = false;

                DrawFooter(theme);

                for (int i = 0; i < objects.Count; ++i) {
                    objects[i].Draw(theme, i == focusIndex);
                }

                if (objects.Count > 0
                        && focusIndex >= 0
                        && focusIndex < objects.Count
                        && objects[focusIndex].Focusable()) {
                    objects[focusIndex].PlaceCursor();
                    Console.CursorVisible = true;
                } else {
                    Console.CursorVisible = false;
                }
            }
        }

        /// <summary>
        /// Standard driver function for normal screen interaction.
        /// Draws the screen and reads keys till done.
        /// Each key is checked against the local bindings,
        /// then the bindings of the focused ScreenObject.
        /// Stops when 'done' is true.
        /// </summary>
        protected void Interact(ConsoleTheme theme)
        {
            focusIndex = -1;
            Blur(null, true);

            do {
                Draw(theme);
                ConsoleKeyInfo k = Console.ReadKey(true);
                if (bindings.ContainsKey(k)) {
                    done = !bindings[k](this, theme);
                } else if (objects.Count > 0) {
                    if (objects[focusIndex].Bindings.ContainsKey(k)) {
                        done = !objects[focusIndex].Bindings[k](this, theme);
                    } else {
                        objects[focusIndex].OnKeyPress(k);
                    }
                }
            } while (!done);
        }

        /// <summary>
        /// Set our private 'done' flag to indicate that Interact should stop.
        /// </summary>
        protected void Quit() { done = true; }

        /// <returns>
        /// Currently focused ScreenObject
        /// </returns>
        protected ScreenObject Focused()
            => focusIndex >= 0 && focusIndex < objects.Count
                ? objects[focusIndex]
                : null;

        /// <summary>
        /// Set the focus to a given ScreenObject
        /// </summary>
        /// <param name="so">ScreenObject to focus</param>
        protected void SetFocus(ScreenObject so)
        {
            int index = objects.IndexOf(so);
            if (index >= 0) {
                focusIndex = index;
            }
        }

        private void DrawFooter(ConsoleTheme theme)
        {
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.BackgroundColor = theme.FooterBg;
            Console.Write("  ");
            var tipLists = new List<List<ScreenTip>>() { tips };
            if (objects.Count > 0) {
                tipLists.Add(objects[focusIndex].Tips);
            }
            bool first = true;
            foreach (var tipList in tipLists) {
                for (int i = 0; i < tipList.Count; ++i) {
                    if (tipList[i].DisplayIf()) {
                        if (Console.CursorLeft + tipSeparator.Length + tipList[i].Key.Length + 5 > Console.WindowWidth) {
                            // Stop drawing if not even enough room for the key
                            break;
                        }
                        if (first) {
                            first = false;
                        } else {
                            Console.ForegroundColor = theme.FooterSeparatorFg;
                            Console.Write(tipSeparator);
                        }
                        Console.ForegroundColor = theme.FooterKeyFg;
                        Console.Write(tipList[i].Key);
                        Console.ForegroundColor = theme.FooterDescriptionFg;
                        string remainder = tipList[i].Key == tipList[i].Description.Substring(0, 1)
                            ? tipList[i].Description.Substring(1)
                            : $" - {tipList[i].Description}";
                        int maxW = Console.WindowWidth - Console.CursorLeft - 1;
                        if (remainder.Length > maxW && maxW > 0) {
                            Console.Write(remainder.Substring(0, maxW));
                        } else {
                            Console.Write(remainder);
                        }
                    }
                }
            }
            // Windows cmd.exe auto-scrolls the whole window if you draw a
            // character in the bottom right corner :(
            Console.Write("".PadLeft(Console.WindowWidth - Console.CursorLeft - 1));
        }

        private void Blur(ScreenObject source, bool forward)
        {
            if (objects.Count > 0) {
                int loops = 0;
                do {
                    if (++loops > objects.Count) {
                        focusIndex = 0;
                        break;
                    }
                    focusIndex = forward
                        ? (focusIndex + 1) % objects.Count
                        : (focusIndex + objects.Count - 1) % objects.Count;
                } while (!objects[focusIndex].Focusable());
            }
        }

        private bool done = false;

        private readonly List<ScreenObject> objects    = new List<ScreenObject>();
        private int                focusIndex = 0;

        private readonly Dictionary<ConsoleKeyInfo, KeyAction> bindings   = new Dictionary<ConsoleKeyInfo, KeyAction>();
        private readonly List<ScreenTip>                       tips       = new List<ScreenTip>();
        private readonly object                                screenLock = new object();

        private static readonly string tipSeparator = $" {Symbols.vertLine} ";
    }

    /// <summary>
    /// Object representing a tip to be shown in the footer
    /// </summary>
    public class ScreenTip {

        /// <summary>
        /// Initialize the object
        /// </summary>
        /// <param name="key">Description of the keypress</param>
        /// <param name="descrip">Description of the bound action</param>
        /// <param name="dispIf">Function that returns true to display the tip or false to hide it</param>
        public ScreenTip(string key, string descrip, Func<bool> dispIf = null)
        {
            Key         = key;
            Description = descrip;
            DisplayIf   = dispIf ?? (() => true);
        }

        /// <summary>
        /// Description of the keypress
        /// </summary>
        public readonly string     Key;
        /// <summary>
        /// Description of the bound action
        /// </summary>
        public readonly string     Description;
        /// <summary>
        /// Function that returns true to display the tip or false to hide it
        /// </summary>
        public readonly Func<bool> DisplayIf;
    }

}
