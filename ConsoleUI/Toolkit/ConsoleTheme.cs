using System;
using System.Collections.Generic;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// Collection of constants defining colors for UI elements.
    /// To change default colors at compile time, edit the initializers.
    /// To change colors at run time, modify properties of ConsoleTheme.Current.
    /// </summary>
    public class ConsoleTheme {

        /// <summary>
        /// Background color for splash screen, null for transparent
        /// </summary>
        public ConsoleColor? SplashBg;

        /// <summary>
        /// Foreground color for splash screen
        /// </summary>
        public ConsoleColor SplashNormalFg;

        /// <summary>
        /// Foreground color for logo on splash screen
        /// </summary>
        public ConsoleColor SplashAccentFg;

        /// <summary>
        /// Background color for exit screen
        /// </summary>
        public ConsoleColor? ExitOuterBg;
        
        /// <summary>
        /// Background color for info pane of exit screen
        /// </summary>
        public ConsoleColor ExitInnerBg;
        
        /// <summary>
        /// Foreground color for normal text on exit screen
        /// </summary>
        public ConsoleColor ExitNormalFg;
        
        /// <summary>
        /// Foreground color for highlighted text on exit screen
        /// </summary>
        public ConsoleColor ExitHighlightFg;
        
        /// <summary>
        /// Foreground color for links on exit screen
        /// </summary>
        public ConsoleColor ExitLinkFg;

        /// <summary>
        /// Background color for normal screen
        /// </summary>
        public ConsoleColor MainBg;

        /// <summary>
        /// Background for top header row
        /// </summary>
        public ConsoleColor HeaderBg;
        /// <summary>
        /// Foreground for top header row
        /// </summary>
        public ConsoleColor HeaderFg;

        /// <summary>
        /// Background for bottom footer row
        /// </summary>
        public ConsoleColor FooterBg;
        /// <summary>
        /// Foreground for vertical separator bars between footer sections
        /// </summary>
        public ConsoleColor FooterSeparatorFg;
        /// <summary>
        /// Foreground for names of keys in footer
        /// </summary>
        public ConsoleColor FooterKeyFg;
        /// <summary>
        /// Foreground for descriptions of key functions in footer
        /// </summary>
        public ConsoleColor FooterDescriptionFg;

        /// <summary>
        /// Default background for labels
        /// </summary>
        public ConsoleColor LabelBg;
        /// <summary>
        /// Default foreground for labels
        /// </summary>
        public ConsoleColor LabelFg;
        /// <summary>
        /// Foreground for de-emphasized labels
        /// </summary>
        public ConsoleColor DimLabelFg;

        /// <summary>
        /// Background for text editing field
        /// </summary>
        public ConsoleColor FieldBg;
        /// <summary>
        /// Foreground for ghost text displayed in empty text editing field
        /// </summary>
        public ConsoleColor FieldGhostFg;
        /// <summary>
        /// Foreground for text in text editing field with focus
        /// </summary>
        public ConsoleColor FieldFocusedFg;
        /// <summary>
        /// Foreground for text in text editing field without focus
        /// </summary>
        public ConsoleColor FieldBlurredFg;

        /// <summary>
        /// Background for list box header row
        /// </summary>
        public ConsoleColor ListBoxHeaderBg;
        /// <summary>
        /// Foreground for list box header row
        /// </summary>
        public ConsoleColor ListBoxHeaderFg;
        /// <summary>
        /// Background for list box data row when not selected
        /// </summary>
        public ConsoleColor ListBoxUnselectedBg;
        /// <summary>
        /// Foreground for list box data row when not selected
        /// </summary>
        public ConsoleColor ListBoxUnselectedFg;
        /// <summary>
        /// Background for list box selected row
        /// </summary>
        public ConsoleColor ListBoxSelectedBg;
        /// <summary>
        /// Foreground for list box selected row
        /// </summary>
        public ConsoleColor ListBoxSelectedFg;

        /// <summary>
        /// Background for scroll bars
        /// </summary>
        public ConsoleColor ScrollBarBg;
        /// <summary>
        /// Foreground for scroll bars
        /// </summary>
        public ConsoleColor ScrollBarFg;

        /// <summary>
        /// Background for popup menus
        /// </summary>
        public ConsoleColor MenuBg;
        /// <summary>
        /// Foreground for popup menus
        /// </summary>
        public ConsoleColor MenuFg;
        /// <summary>
        /// Foreground for disabled popup menu options
        /// </summary>
        public ConsoleColor MenuDisabledFg;
        /// <summary>
        /// Background for selected menu option
        /// </summary>
        public ConsoleColor MenuSelectedBg;

        /// <summary>
        /// Foreground for text indicating registry was updated recently
        /// </summary>
        public ConsoleColor RegistryUpToDate;
        /// <summary>
        /// Foreground for text indicating registry was updated a while ago
        /// </summary>
        public ConsoleColor RegistryStale;
        /// <summary>
        /// Foreground for text indicating registry was updated dangerously long ago
        /// </summary>
        public ConsoleColor RegistryVeryStale;

        /// <summary>
        /// Background for popup dialogs
        /// </summary>
        public ConsoleColor PopupBg;
        /// <summary>
        /// Foreground for popup dialog text
        /// </summary>
        public ConsoleColor PopupFg;
        /// <summary>
        /// Foreground for popup dialog outlines
        /// </summary>
        public ConsoleColor PopupOutlineFg;
        /// <summary>
        /// Color for shadow drawn to bottom and right of popup dialogs
        /// </summary>
        public ConsoleColor? PopupShadow;
        /// <summary>
        /// Background for buttons
        /// </summary>
        public ConsoleColor PopupButtonBg;
        /// <summary>
        /// Foreground for buttons without focus
        /// </summary>
        public ConsoleColor PopupButtonFg;
        /// <summary>
        /// Foreground for buttons with focus
        /// </summary>
        public ConsoleColor PopupButtonSelectedFg;
        /// <summary>
        /// Color for shadow drawn to bottom and right of buttons
        /// </summary>
        public ConsoleColor? PopupButtonShadow;

        /// <summary>
        /// Default background for multi line text box
        /// </summary>
        public ConsoleColor TextBoxBg;
        /// <summary>
        /// Default foreground for multi line text box
        /// </summary>
        public ConsoleColor TextBoxFg;

        /// <summary>
        /// Background for non-completed part of progress bars
        /// </summary>
        public ConsoleColor ProgressBarBg;
        /// <summary>
        /// Foreground for non-completed part of progress bars
        /// </summary>
        public ConsoleColor ProgressBarFg;
        /// <summary>
        /// Background for completed part of progress bars
        /// </summary>
        public ConsoleColor ProgressBarHighlightBg;
        /// <summary>
        /// Foreground for completed part of progress bars
        /// </summary>
        public ConsoleColor ProgressBarHighlightFg;

        /// <summary>
        /// Foreground for normal box frames
        /// </summary>
        public ConsoleColor NormalFrameFg;
        /// <summary>
        /// Foreground for active/highlighted box frames
        /// </summary>
        public ConsoleColor ActiveFrameFg;
        /// <summary>
        /// Foreground for important/abnormal box frames
        /// </summary>
        public ConsoleColor AlertFrameFg;
        
        /// <summary>
        /// Available themes
        /// </summary>
        public static readonly Dictionary<string, ConsoleTheme> Themes = new Dictionary<string, ConsoleTheme>() {
            {
                "default",
                new ConsoleTheme() {
                    SplashBg               = null,
                    SplashNormalFg         = ConsoleColor.Gray,
                    SplashAccentFg         = ConsoleColor.Blue,
                    ExitOuterBg            = ConsoleColor.Black,
                    ExitInnerBg            = ConsoleColor.DarkRed,
                    ExitNormalFg           = ConsoleColor.Yellow,
                    ExitHighlightFg        = ConsoleColor.White,
                    ExitLinkFg             = ConsoleColor.Cyan,
                    MainBg                 = ConsoleColor.DarkBlue,
                    HeaderBg               = ConsoleColor.Gray,
                    HeaderFg               = ConsoleColor.Black,
                    FooterBg               = ConsoleColor.Gray,
                    FooterSeparatorFg      = ConsoleColor.DarkGray,
                    FooterKeyFg            = ConsoleColor.DarkRed,
                    FooterDescriptionFg    = ConsoleColor.Black,
                    LabelBg                = ConsoleColor.DarkBlue,
                    LabelFg                = ConsoleColor.Gray,
                    DimLabelFg             = ConsoleColor.DarkCyan,
                    FieldBg                = ConsoleColor.Black,
                    FieldGhostFg           = ConsoleColor.DarkGray,
                    FieldFocusedFg         = ConsoleColor.Cyan,
                    FieldBlurredFg         = ConsoleColor.DarkCyan,
                    ListBoxHeaderBg        = ConsoleColor.Gray,
                    ListBoxHeaderFg        = ConsoleColor.Black,
                    ListBoxUnselectedBg    = ConsoleColor.DarkCyan,
                    ListBoxUnselectedFg    = ConsoleColor.Black,
                    ListBoxSelectedBg      = ConsoleColor.DarkGreen,
                    ListBoxSelectedFg      = ConsoleColor.White,
                    ScrollBarBg            = ConsoleColor.DarkBlue,
                    ScrollBarFg            = ConsoleColor.DarkCyan,
                    MenuBg                 = ConsoleColor.Gray,
                    MenuFg                 = ConsoleColor.Black,
                    MenuDisabledFg         = ConsoleColor.DarkGray,
                    MenuSelectedBg         = ConsoleColor.DarkGreen,
                    RegistryUpToDate       = ConsoleColor.DarkGray,
                    RegistryStale          = ConsoleColor.Yellow,
                    RegistryVeryStale      = ConsoleColor.Red,
                    PopupBg                = ConsoleColor.Gray,
                    PopupFg                = ConsoleColor.Black,
                    PopupOutlineFg         = ConsoleColor.White,
                    PopupShadow            = ConsoleColor.Black,
                    PopupButtonBg          = ConsoleColor.DarkGreen,
                    PopupButtonFg          = ConsoleColor.Black,
                    PopupButtonSelectedFg  = ConsoleColor.Cyan,
                    PopupButtonShadow      = ConsoleColor.Black,
                    TextBoxBg              = ConsoleColor.DarkCyan,
                    TextBoxFg              = ConsoleColor.Yellow,
                    ProgressBarBg          = ConsoleColor.DarkCyan,
                    ProgressBarFg          = ConsoleColor.Black,
                    ProgressBarHighlightBg = ConsoleColor.DarkGreen,
                    ProgressBarHighlightFg = ConsoleColor.Yellow,
                    NormalFrameFg          = ConsoleColor.Gray,
                    ActiveFrameFg          = ConsoleColor.White,
                    AlertFrameFg           = ConsoleColor.Yellow,
                }
            }, {
                "dark",
                new ConsoleTheme() {
                    SplashBg               = ConsoleColor.Black,
                    SplashNormalFg         = ConsoleColor.DarkGreen,
                    SplashAccentFg         = ConsoleColor.Green,
                    ExitOuterBg            = null,
                    ExitInnerBg            = ConsoleColor.Black,
                    ExitNormalFg           = ConsoleColor.DarkGreen,
                    ExitHighlightFg        = ConsoleColor.Green,
                    ExitLinkFg             = ConsoleColor.Green,
                    MainBg                 = ConsoleColor.Black,
                    HeaderBg               = ConsoleColor.DarkGreen,
                    HeaderFg               = ConsoleColor.Black,
                    FooterBg               = ConsoleColor.DarkGreen,
                    FooterSeparatorFg      = ConsoleColor.Black,
                    FooterKeyFg            = ConsoleColor.Green,
                    FooterDescriptionFg    = ConsoleColor.Black,
                    LabelBg                = ConsoleColor.Black,
                    LabelFg                = ConsoleColor.DarkGreen,
                    DimLabelFg             = ConsoleColor.DarkGreen,
                    FieldBg                = ConsoleColor.Black,
                    FieldGhostFg           = ConsoleColor.DarkGreen,
                    FieldFocusedFg         = ConsoleColor.Green,
                    FieldBlurredFg         = ConsoleColor.DarkGreen,
                    ListBoxHeaderBg        = ConsoleColor.DarkGreen,
                    ListBoxHeaderFg        = ConsoleColor.Black,
                    ListBoxUnselectedBg    = ConsoleColor.Black,
                    ListBoxUnselectedFg    = ConsoleColor.DarkGreen,
                    ListBoxSelectedBg      = ConsoleColor.Black,
                    ListBoxSelectedFg      = ConsoleColor.Green,
                    ScrollBarBg            = ConsoleColor.Black,
                    ScrollBarFg            = ConsoleColor.DarkGreen,
                    MenuBg                 = ConsoleColor.DarkGreen,
                    MenuFg                 = ConsoleColor.Black,
                    MenuDisabledFg         = ConsoleColor.Black,
                    MenuSelectedBg         = ConsoleColor.Green,
                    RegistryUpToDate       = ConsoleColor.DarkGreen,
                    RegistryStale          = ConsoleColor.Green,
                    RegistryVeryStale      = ConsoleColor.Green,
                    PopupBg                = ConsoleColor.Black,
                    PopupFg                = ConsoleColor.Green,
                    PopupOutlineFg         = ConsoleColor.Green,
                    PopupShadow            = null,
                    PopupButtonBg          = ConsoleColor.DarkGreen,
                    PopupButtonFg          = ConsoleColor.Black,
                    PopupButtonSelectedFg  = ConsoleColor.Green,
                    PopupButtonShadow      = null,
                    TextBoxBg              = ConsoleColor.Black,
                    TextBoxFg              = ConsoleColor.DarkGreen,
                    ProgressBarBg          = ConsoleColor.Black,
                    ProgressBarFg          = ConsoleColor.DarkGreen,
                    ProgressBarHighlightBg = ConsoleColor.DarkGreen,
                    ProgressBarHighlightFg = ConsoleColor.Black,
                    NormalFrameFg          = ConsoleColor.DarkGreen,
                    ActiveFrameFg          = ConsoleColor.Green,
                    AlertFrameFg           = ConsoleColor.Green,
                }
            }
        };
    }

}
