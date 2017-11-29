using System.Text;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Glyphs for drawing text UIs
    /// Relies on code page 437, see:
    ///   https://int10h.org/oldschool-pc-fonts/readme/
    /// </summary>
    public static class Symbols {

        // This needs to be first so it's set when the other properties are initialized
        private static readonly Encoding dosCodePage = Encoding.GetEncoding(437);

        /// <summary>
        /// The "three horizontal lines" menu symbol in the latest web apps
        /// AND Turbo Vision from 1990 (see the upper left corner of the screenshots).
        /// </summary>
        public static readonly string hamburger        = cp437s(0xf0);

        /// <summary>
        /// Check mark or square root symbol
        /// </summary>
        public static readonly string checkmark        = cp437s(0xfb);
        /// <summary>
        /// >= symbol
        /// </summary>
        public static readonly string greaterEquals    = cp437s(0xf2);

        /// <summary>
        /// Hashed square box for drawing scrollbars
        /// </summary>
        public static readonly string hashBox          = cp437s(0xb1);
        /// <summary>
        /// Vertically centered round dot symbol for radio buttons
        /// </summary>
        public static readonly string dot              = cp437s(0xf9);

        /// <summary>
        /// Upper left corner line drawing symbol
        /// </summary>
        public static readonly string upperLeftCorner  = cp437s(0xda);
        /// <summary>
        /// Upper right corner line drawing symbol
        /// </summary>
        public static readonly string upperRightCorner = cp437s(0xbf);
        /// <summary>
        /// Horizontal line symbol
        /// </summary>
        public static readonly char   horizLine        = cp437c(0xc4);
        /// <summary>
        /// Vertical line symbol
        /// </summary>
        public static readonly string vertLine         = cp437s(0xb3);
        /// <summary>
        /// Lower left corner line drawing symbol
        /// </summary>
        public static readonly string lowerLeftCorner  = cp437s(0xc0);
        /// <summary>
        /// Lower right corner line drawing symbol
        /// </summary>
        public static readonly string lowerRightCorner = cp437s(0xd9);
        /// <summary>
        /// Left tee line drawing symbol (connects up, down, and right)
        /// </summary>
        public static readonly string leftTee          = cp437s(0xc3);
        /// <summary>
        /// Right tee line drawing symbol (connects up, down, and left)
        /// </summary>
        public static readonly string rightTee         = cp437s(0xb4);

        /// <summary>
        /// Upper left corner double line drawing symbol
        /// </summary>
        public static readonly string upperLeftCornerDouble  = cp437s(0xc9);
        /// <summary>
        /// Upper right corner double line drawing symobl
        /// </summary>
        public static readonly string upperRightCornerDouble = cp437s(0xbb);
        /// <summary>
        /// Double horizontal line symbol
        /// </summary>
        public static readonly char   horizLineDouble        = cp437c(0xcd);
        /// <summary>
        /// Double vertical line symbol
        /// </summary>
        public static readonly string vertLineDouble         = cp437s(0xba);
        /// <summary>
        /// Lower left corner double line drawing symbol
        /// </summary>
        public static readonly string lowerLeftCornerDouble  = cp437s(0xc8);
        /// <summary>
        /// Lower right corner double line drawing symbol
        /// </summary>
        public static readonly string lowerRightCornerDouble = cp437s(0xbc);
        /// <summary>
        /// Left tee double line drawing symbol (connects up, down, and right)
        /// </summary>
        public static readonly string leftTeeDouble          = cp437s(0xcc);
        /// <summary>
        /// Right tee double line drawing symbol (connects up, down, and left)
        /// </summary>
        public static readonly string rightTeeDouble         = cp437s(0xb9);

        /// <summary>
        /// Upper half block used for the small shadow under buttons
        /// </summary>
        public static readonly char upperHalfBlock = cp437c(0xdf);
        /// <summary>
        /// Lower half block used for the shadow to the right of buttons
        /// and the letters in the splash screen ASCII art
        /// </summary>
        public static readonly char lowerHalfBlock = cp437c(0xdc);

        private static char   cp437c(byte num) { return dosCodePage.GetChars(new byte[] {num})[0]; }
        private static string cp437s(byte num) { return $"{cp437c(num)}";                          }
    }

}
