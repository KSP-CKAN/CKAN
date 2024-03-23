using System;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// A list of keys for key bindings in a console UI.
    /// </summary>
    public static class Keys {

        private const char LinuxEnter   = (char)10;
        private const char WindowsEnter = (char)13;

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
            (char)27, ConsoleKey.Escape, false, false, false
        );

        /// <summary>
        /// Representation of F1 for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo F1 = new ConsoleKeyInfo(
            (char)0, ConsoleKey.F1, false, false, false
        );

        /// <summary>
        /// Representation of F2 for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo F2 = new ConsoleKeyInfo(
            (char)0, ConsoleKey.F2, false, false, false
        );

        /// <summary>
        /// Representation of F5 for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo F5 = new ConsoleKeyInfo(
            (char)0, ConsoleKey.F5, false, false, false
        );

        /// <summary>
        /// Representation of F8 for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo F8 = new ConsoleKeyInfo(
            (char)0, ConsoleKey.F8, false, false, false
        );

        /// <summary>
        /// Representation of F9 for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo F9 = new ConsoleKeyInfo(
            (char)0, ConsoleKey.F9, false, false, false
        );

        /// <summary>
        /// Representation of F10 for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo F10 = new ConsoleKeyInfo(
            (char)0, ConsoleKey.F10, false, false, false
        );

        /// <summary>
        /// Representation of Apps for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo Apps = new ConsoleKeyInfo(
            (char)0, ConsoleKey.Applications, false, false, false
        );

        /// <summary>
        /// Representation of down arrow for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo DownArrow = new ConsoleKeyInfo(
            (char)0, ConsoleKey.DownArrow, false, false, false
        );

        /// <summary>
        /// Representation of up arrow for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo UpArrow = new ConsoleKeyInfo(
            (char)0, ConsoleKey.UpArrow, false, false, false
        );

        /// <summary>
        /// Representation of home for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo Home = new ConsoleKeyInfo(
            (char)0, ConsoleKey.Home, false, false, false
        );

        /// <summary>
        /// Representation of end for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo End = new ConsoleKeyInfo(
            (char)0, ConsoleKey.End, false, false, false
        );

        /// <summary>
        /// Representation of page up for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo PageUp = new ConsoleKeyInfo(
            (char)0, ConsoleKey.PageUp, false, false, false
        );

        /// <summary>
        /// Representation of page down for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo PageDown = new ConsoleKeyInfo(
            (char)0, ConsoleKey.PageDown, false, false, false
        );

        /// <summary>
        /// Representation of plus key for key bindings
        /// Both with and without Shift for those pesky Germans
        /// </summary>
        public static readonly ConsoleKeyInfo[] Plus = new ConsoleKeyInfo[] {
            new ConsoleKeyInfo(
                '+',
                Platform.IsWindows ? ConsoleKey.OemPlus : ConsoleKey.Add,
                false, false, false
            ), new ConsoleKeyInfo(
                '+',
                Platform.IsWindows ? ConsoleKey.OemPlus : ConsoleKey.Add,
                true, false, false
            )
        };

        /// <summary>
        /// Representation of minus key for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo Minus = new ConsoleKeyInfo(
            '-',
            Platform.IsWindows ? ConsoleKey.OemMinus : ConsoleKey.Subtract,
            false, false, false
        );

        /// <summary>
        /// Representation of Ctrl+A for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlA = new ConsoleKeyInfo(
            (char)1, ConsoleKey.A, false, false, true
        );

        /// <summary>
        /// Representation of Ctrl+D for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlD = new ConsoleKeyInfo(
            (char)4, ConsoleKey.D, false, false, true
        );

        /// <summary>
        /// Representation of Ctrl+F for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlF = new ConsoleKeyInfo(
            (char)6, ConsoleKey.F, false, false, true
        );

        /// <summary>
        /// Representation of Ctrl+L for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlL = new ConsoleKeyInfo(
            (char)12, ConsoleKey.Clear, false, false, false
        );

        /// <summary>
        /// Representation of Ctrl+P for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlP = new ConsoleKeyInfo(
            (char)16, ConsoleKey.P, false, false, true
        );

        /// <summary>
        /// Representation of Ctrl+Q for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlQ = new ConsoleKeyInfo(
            (char)17, ConsoleKey.Q, false, false, true
        );

        /// <summary>
        /// Representation of Ctrl+R for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlR = new ConsoleKeyInfo(
            (char)18, ConsoleKey.R, false, false, true
        );

        /// <summary>
        /// Representation of Ctrl+U for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo CtrlU = new ConsoleKeyInfo(
            (char)21, ConsoleKey.U, false, false, true
        );

        /// <summary>
        /// Representation of Alt+H for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo AltH = new ConsoleKeyInfo(
            'h', ConsoleKey.H, false, true, false
        );

        /// <summary>
        /// Representation of Alt+X for key bindings
        /// Does not work on MacOSX, so we
        /// </summary>
        public static readonly ConsoleKeyInfo AltX = new ConsoleKeyInfo(
            'x', ConsoleKey.X, false, true, false
        );

        /// <summary>
        /// Representation of letter 'a' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo A = new ConsoleKeyInfo(
            'a', ConsoleKey.A, false, false, false
        );

        /// <summary>
        /// Representation of letter 'd' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo D = new ConsoleKeyInfo(
            'd', ConsoleKey.D, false, false, false
        );

        /// <summary>
        /// Representation of letter 'e' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo E = new ConsoleKeyInfo(
            'e', ConsoleKey.E, false, false, false
        );

        /// <summary>
        /// Representation of letter 'k' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo K = new ConsoleKeyInfo(
            'k', ConsoleKey.K, false, false, false
        );

        /// <summary>
        /// Representation of letter 'n' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo N = new ConsoleKeyInfo(
            'n', ConsoleKey.N, false, false, false
        );

        /// <summary>
        /// Representation of letter 'r' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo R = new ConsoleKeyInfo(
            'r', ConsoleKey.R, false, false, false
        );

        /// <summary>
        /// Representation of letter 'y' for key bindings
        /// </summary>
        public static readonly ConsoleKeyInfo Y = new ConsoleKeyInfo(
            'y', ConsoleKey.Y, false, false, false
        );

    }

}
