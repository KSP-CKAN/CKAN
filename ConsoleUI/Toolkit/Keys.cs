using System;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// A list of keys for key bindings in a console UI.
    /// </summary>
    public static class Keys {

        private const char LinuxEnter   = (System.Char)10;
        private const char WindowsEnter = (System.Char)13;

        /// <summary>
        /// Representation of enter key for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo Enter = new ConsoleKeyInfo(
            Platform.IsWindows ? WindowsEnter : LinuxEnter,
            ConsoleKey.Enter, false, false, false
        );

        /// <summary>
        /// Representation of enter key for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo Space = new ConsoleKeyInfo(
            ' ', ConsoleKey.Spacebar, false, false, false
        );

        /// <summary>
        /// Representation of escape key for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo Escape = new ConsoleKeyInfo(
            (System.Char)27, ConsoleKey.Escape, false, false, false
        );

        /// <summary>
        /// Representation of F1 for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo F1 = new ConsoleKeyInfo(
            (System.Char)0, ConsoleKey.F1, false, false, false
        );

        /// <summary>
        /// Representation of F2 for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo F2 = new ConsoleKeyInfo(
            (System.Char)0, ConsoleKey.F2, false, false, false
        );

        /// <summary>
        /// Representation of F5 for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo F5 = new ConsoleKeyInfo(
            (System.Char)0, ConsoleKey.F5, false, false, false
        );

        /// <summary>
        /// Representation of F9 for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo F9 = new ConsoleKeyInfo(
            (System.Char)0, ConsoleKey.F9, false, false, false
        );

        /// <summary>
        /// Representation of F10 for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo F10 = new ConsoleKeyInfo(
            (System.Char)0, ConsoleKey.F10, false, false, false
        );

        /// <summary>
        /// Representation of down arrow for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo DownArrow = new ConsoleKeyInfo(
            (System.Char)0, ConsoleKey.DownArrow, false, false, false
        );

        /// <summary>
        /// Representation of up arrow for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo UpArrow = new ConsoleKeyInfo(
            (System.Char)0, ConsoleKey.UpArrow, false, false, false
        );

        /// <summary>
        /// Representation of home for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo Home = new ConsoleKeyInfo(
            (System.Char)0, ConsoleKey.Home, false, false, false
        );

        /// <summary>
        /// Representation of end for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo End = new ConsoleKeyInfo(
            (System.Char)0, ConsoleKey.End, false, false, false
        );

        /// <summary>
        /// Representation of page up for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo PageUp = new ConsoleKeyInfo(
            (System.Char)0, ConsoleKey.PageUp, false, false, false
        );

        /// <summary>
        /// Representation of page down for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo PageDown = new ConsoleKeyInfo(
            (System.Char)0, ConsoleKey.PageDown, false, false, false
        );

        /// <summary>
        /// Representation of plus key for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo Plus = new ConsoleKeyInfo(
            (System.Char)'+',
            Platform.IsWindows ? ConsoleKey.OemPlus : ConsoleKey.Add,
            Platform.IsWindows ? true : false, false, false
        );

        /// <summary>
        /// Representation of minus key for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo Minus = new ConsoleKeyInfo(
            (System.Char)'-',
            Platform.IsWindows ? ConsoleKey.OemMinus : ConsoleKey.Subtract,
            false, false, false
        );

        /// <summary>
        /// Representation of Ctrl+A for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlA = new ConsoleKeyInfo(
            (System.Char)1, ConsoleKey.A, false, false, true
        );

        /// <summary>
        /// Representation of Ctrl+D for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlD = new ConsoleKeyInfo(
            (System.Char)4, ConsoleKey.D, false, false, true
        );

        /// <summary>
        /// Representation of Ctrl+F for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlF = new ConsoleKeyInfo(
            (System.Char)6, ConsoleKey.F, false, false, true
        );

        /// <summary>
        /// Representation of Ctrl+L for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlL = new ConsoleKeyInfo(
            (System.Char)12, ConsoleKey.Clear, false, false, false
        );

        /// <summary>
        /// Representation of Ctrl+Q for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlQ = new ConsoleKeyInfo(
            (System.Char)17, ConsoleKey.Q, false, false, true
        );

        /// <summary>
        /// Representation of Ctrl+R for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlR = new ConsoleKeyInfo(
            (System.Char)18, ConsoleKey.R, false, false, true
        );

        /// <summary>
        /// Representation of Ctrl+U for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlU = new ConsoleKeyInfo(
            (System.Char)21, ConsoleKey.U, false, false, true
        );

        /// <summary>
        /// Representation of Alt+H for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo AltH = new ConsoleKeyInfo(
            (System.Char)'h', ConsoleKey.H, false, true, false
        );

        /// <summary>
        /// Representation of Alt+X for key bindings
        /// Does not work on MacOSX, so we
        /// </summary>
        public static readonly ConsoleKeyInfo AltX = new ConsoleKeyInfo(
            (System.Char)'x', ConsoleKey.X, false, true, false
        );

        /// <summary>
        /// Representation of letter 'a' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo A = new ConsoleKeyInfo(
            (System.Char)'a', ConsoleKey.A, false, false, false
        );

        /// <summary>
        /// Representation of letter 'd' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo D = new ConsoleKeyInfo(
            (System.Char)'d', ConsoleKey.D, false, false, false
        );

        /// <summary>
        /// Representation of letter 'e' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo E = new ConsoleKeyInfo(
            (System.Char)'e', ConsoleKey.E, false, false, false
        );

        /// <summary>
        /// Representation of letter 'n' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo N = new ConsoleKeyInfo(
            (System.Char)'n', ConsoleKey.N, false, false, false
        );

        /// <summary>
        /// Representation of letter 'r' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo R = new ConsoleKeyInfo(
            (System.Char)'r', ConsoleKey.R, false, false, false
        );

        /// <summary>
        /// Representation of letter 'y' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo Y = new ConsoleKeyInfo(
            (System.Char)'y', ConsoleKey.Y, false, false, false
        );

    }

}
