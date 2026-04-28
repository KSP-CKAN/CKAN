using System;

using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Popup to let user specify how they want to clone an instance
    /// </summary>
    public class CloneInstanceDialog : ConsoleDialog {

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        /// <param name="theme"></param>
        public CloneInstanceDialog(ConsoleTheme theme)
            : base(theme)
        {
            int t = GetTop(),
                b = t + 8;
            SetDimensions(Console.WindowWidth  / 6, t,
                          5 * Console.WindowWidth  / 6, b);
            int l = GetLeft(),
                r = GetRight();

            CenterHeader = () => Properties.Resources.CloneInstanceTitle;

            AddObject(new ConsoleLabel(l + 2, t + 2, l + 2 + labelW,
                                       () => Properties.Resources.CloneInstanceName,
                                       th => th.PopupBg,
                                       th => th.PopupFg));
            nameField = new ConsoleField(l + 2 + labelW + wPad, t + 2, r - 3)
                        {
                            GhostText = () => Properties.Resources.CloneInstanceNameGhostText,
                        };
            AddObject(nameField);

            AddObject(new ConsoleLabel(l + 2, t + 4, l + 2 + labelW,
                                       () => Properties.Resources.CloneInstancePath,
                                       th => th.PopupBg,
                                       th => th.PopupFg));
            pathField = new ConsoleField(l + 2 + labelW + wPad, t + 4, r - 3)
                        {
                            GhostText = () => Properties.Resources.CloneInstancePathGhostText,
                        };
            AddObject(pathField);

            int btnW = (2 * buttonWidth) + buttonPadding;
            int btnLeft = (Console.WindowWidth - btnW) / 2;

            AddObject(new ConsoleButton(btnLeft, t + 6, btnLeft + buttonWidth,
                                        Properties.Resources.Accept,
                                        () => {
                                            if (nameField.Value is { Length: > 0 }
                                                && pathField.Value is { Length: > 0 })
                                            {
                                                Proceed = true;
                                                Quit();
                                            }
                                        }));
            AddObject(new ConsoleButton(btnLeft + buttonWidth + buttonPadding, t + 6,
                                        btnLeft + (2 * buttonWidth) + buttonPadding,
                                        Properties.Resources.Cancel,
                                        () => { Proceed = false; Quit(); }));

            AddBinding(Keys.Escape, sender => { Proceed = false; return false; });
            AddTip(Properties.Resources.Esc, Properties.Resources.Cancel);
        }

        /// <summary>
        /// true if the user confirmed the clone, false if they cancelled
        /// </summary>
        public bool Proceed { get; private set; }

        /// <summary>
        /// Name for the newly cloned instance
        /// </summary>
        public string NewName => nameField.Value;

        /// <summary>
        /// Location to put the newly cloned instance
        /// </summary>
        public string NewPath => pathField.Value;

        private const  int buttonWidth   = 10;
        private const  int buttonPadding = 3;
        private const  int wPad          = 2;
        private static int labelW =>
            Math.Max(6, Math.Max(Properties.Resources.CloneInstanceName.Length,
                                 Properties.Resources.CloneInstancePath.Length));

        private readonly ConsoleField nameField;
        private readonly ConsoleField pathField;
    }
}
