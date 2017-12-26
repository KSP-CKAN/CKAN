using System;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Collection of constants defining colors for UI elements.
    /// To change default colors at compile time, edit the initializers.
    /// To change colors at run time, modify properties of ConsoleTheme.Current.
    /// </summary>
    public class ConsoleTheme {

        /// <summary>
        /// Background color for normal screen
        /// </summary>
        public readonly ConsoleColor MainBg   = ConsoleColor.DarkBlue;

        /// <summary>
        /// Background for top header row
        /// </summary>
        public readonly ConsoleColor HeaderBg = ConsoleColor.Gray;
        /// <summary>
        /// Foreground for top header row
        /// </summary>
        public readonly ConsoleColor HeaderFg = ConsoleColor.Black;

        /// <summary>
        /// Background for bottom footer row
        /// </summary>
        public readonly ConsoleColor FooterBg            = ConsoleColor.Gray;
        /// <summary>
        /// Foreground for vertical separator bars between footer sections
        /// </summary>
        public readonly ConsoleColor FooterSeparatorFg   = ConsoleColor.DarkGray;
        /// <summary>
        /// Foreground for names of keys in footer
        /// </summary>
        public readonly ConsoleColor FooterKeyFg         = ConsoleColor.DarkRed;
        /// <summary>
        /// Foreground for descriptions of key functions in footer
        /// </summary>
        public readonly ConsoleColor FooterDescriptionFg = ConsoleColor.Black;

        /// <summary>
        /// Default background for labels
        /// </summary>
        public readonly ConsoleColor LabelBg    = ConsoleColor.DarkBlue;
        /// <summary>
        /// Default foreground for labels
        /// </summary>
        public readonly ConsoleColor LabelFg    = ConsoleColor.Gray;
        /// <summary>
        /// Foreground for de-emphasized labels
        /// </summary>
        public readonly ConsoleColor DimLabelFg = ConsoleColor.DarkCyan;

        /// <summary>
        /// Background for text editing field
        /// </summary>
        public readonly ConsoleColor FieldBg        = ConsoleColor.Black;
        /// <summary>
        /// Foreground for ghost text displayed in empty text editing field
        /// </summary>
        public readonly ConsoleColor FieldGhostFg   = ConsoleColor.DarkGray;
        /// <summary>
        /// Foreground for text in text editing field with focus
        /// </summary>
        public readonly ConsoleColor FieldFocusedFg = ConsoleColor.Cyan;
        /// <summary>
        /// Foreground for text in text editing field without focus
        /// </summary>
        public readonly ConsoleColor FieldBlurredFg = ConsoleColor.DarkCyan;

        /// <summary>
        /// Background for list box header row
        /// </summary>
        public readonly ConsoleColor ListBoxHeaderBg     = ConsoleColor.Gray;
        /// <summary>
        /// Foreground for list box header row
        /// </summary>
        public readonly ConsoleColor ListBoxHeaderFg     = ConsoleColor.Black;
        /// <summary>
        /// Background for list box data row when not selected
        /// </summary>
        public readonly ConsoleColor ListBoxUnselectedBg = ConsoleColor.DarkCyan;
        /// <summary>
        /// Foreground for list box data row when not selected
        /// </summary>
        public readonly ConsoleColor ListBoxUnselectedFg = ConsoleColor.Black;
        /// <summary>
        /// Background for list box selected row
        /// </summary>
        public readonly ConsoleColor ListBoxSelectedBg   = ConsoleColor.DarkGreen;
        /// <summary>
        /// Foreground for list box selected row
        /// </summary>
        public readonly ConsoleColor ListBoxSelectedFg   = ConsoleColor.White;

        /// <summary>
        /// Background for scroll bars
        /// </summary>
        public readonly ConsoleColor ScrollBarBg = ConsoleColor.DarkBlue;
        /// <summary>
        /// Foreground for scroll bars
        /// </summary>
        public readonly ConsoleColor ScrollBarFg = ConsoleColor.DarkCyan;

        /// <summary>
        /// Background for popup menus
        /// </summary>
        public readonly ConsoleColor MenuBg         = ConsoleColor.Gray;
        /// <summary>
        /// Foreground for popup menus
        /// </summary>
        public readonly ConsoleColor MenuFg         = ConsoleColor.Black;
        /// <summary>
        /// Background for selected menu option
        /// </summary>
        public readonly ConsoleColor MenuSelectedBg = ConsoleColor.DarkGreen;

        /// <summary>
        /// Foreground for text indicating registry was updated recently
        /// </summary>
        public readonly ConsoleColor RegistryUpToDate  = ConsoleColor.DarkGray;
        /// <summary>
        /// Foreground for text indicating registry was updated a while ago
        /// </summary>
        public readonly ConsoleColor RegistryStale     = ConsoleColor.Yellow;
        /// <summary>
        /// Foreground for text indicating registry was updated dangerously long ago
        /// </summary>
        public readonly ConsoleColor RegistryVeryStale = ConsoleColor.Red;

        /// <summary>
        /// Background for popup dialogs
        /// </summary>
        public readonly ConsoleColor PopupBg               = ConsoleColor.Gray;
        /// <summary>
        /// Foreground for popup dialog text
        /// </summary>
        public readonly ConsoleColor PopupFg               = ConsoleColor.Black;
        /// <summary>
        /// Foreground for popup dialog outlines
        /// </summary>
        public readonly ConsoleColor PopupOutlineFg        = ConsoleColor.White;
        /// <summary>
        /// Color for shadow drawn to bottom and right of popup dialogs
        /// </summary>
        public readonly ConsoleColor PopupShadow           = ConsoleColor.Black;
        /// <summary>
        /// Background for buttons
        /// </summary>
        public readonly ConsoleColor PopupButtonBg         = ConsoleColor.DarkGreen;
        /// <summary>
        /// Foreground for buttons without focus
        /// </summary>
        public readonly ConsoleColor PopupButtonFg         = ConsoleColor.Black;
        /// <summary>
        /// Foreground for buttons with focus
        /// </summary>
        public readonly ConsoleColor PopupButtonSelectedFg = ConsoleColor.Cyan;
        /// <summary>
        /// Color for shadow drawn to bottom and right of buttons
        /// </summary>
        public readonly ConsoleColor PopupButtonShadow     = ConsoleColor.Black;

        /// <summary>
        /// Default background for multi line text box
        /// </summary>
        public readonly ConsoleColor TextBoxBg = ConsoleColor.DarkCyan;
        /// <summary>
        /// Default foreground for multi line text box
        /// </summary>
        public readonly ConsoleColor TextBoxFg = ConsoleColor.Yellow;

        /// <summary>
        /// Background for non-completed part of progress bars
        /// </summary>
        public readonly ConsoleColor ProgressBarBg          = ConsoleColor.DarkCyan;
        /// <summary>
        /// Foreground for non-completed part of progress bars
        /// </summary>
        public readonly ConsoleColor ProgressBarFg          = ConsoleColor.Black;
        /// <summary>
        /// Background for completed part of progress bars
        /// </summary>
        public readonly ConsoleColor ProgressBarHighlightBg = ConsoleColor.DarkGreen;
        /// <summary>
        /// Foreground for completed part of progress bars
        /// </summary>
        public readonly ConsoleColor ProgressBarHighlightFg = ConsoleColor.Yellow;

        /// <summary>
        /// Foreground for normal box frames
        /// </summary>
        public readonly ConsoleColor NormalFrameFg = ConsoleColor.Gray;
        /// <summary>
        /// Foreground for active/highlighted box frames
        /// </summary>
        public readonly ConsoleColor ActiveFrameFg = ConsoleColor.White;
        /// <summary>
        /// Foreground for important/abnormal box frames
        /// </summary>
        public readonly ConsoleColor AlertFrameFg  = ConsoleColor.Yellow;

        /// <summary>
        /// Singleton instance for current theme.
        /// Default values are as per above.
        /// </summary>
        public static   ConsoleTheme Current = new ConsoleTheme();

        private ConsoleTheme() { }
    }

}
