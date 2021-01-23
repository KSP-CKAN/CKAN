using System;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Not inheriting from ConsoleScreen because we don't
    /// want the standard header/footer/background.
    /// </summary>
    public class ExitScreen {

        /// <summary>
        /// Initialize the screen.
        /// </summary>
        public ExitScreen() { }

        /// <summary>
        /// Show the screen.
        /// Luckily we don't have any interaction to do.
        /// </summary>
        public void Run(ConsoleTheme theme)
        {
            Draw(theme);

            // Try to return the terminal to normal
            Console.SetCursorPosition(0, Console.WindowHeight - 2);
            Console.ResetColor();
            Console.CursorVisible = true;
        }

        /// <summary>
        /// Draw a cool retro exit screen like Id used to do.
        /// This is a composite of the exit screens of ULTIMATE DOOM and DOOM II.
        /// </summary>
        private void Draw(ConsoleTheme theme)
        {
            Console.CursorVisible   = false;
            if (theme.ExitOuterBg.HasValue)
            {
                Console.BackgroundColor = theme.ExitOuterBg.Value;
            }
            else
            {
                Console.ResetColor();                
            }
            Console.Clear();

            FancyLinePiece ckanPiece = new FancyLinePiece("CKAN", theme.ExitInnerBg, theme.ExitHighlightFg);
    
            FancyLinePiece[][] lines = new FancyLinePiece[][] {
                new FancyLinePiece[] {
                    ckanPiece,
                    new FancyLinePiece(", the Comprehensive Kerbal Archive Network", theme.ExitInnerBg, theme.ExitNormalFg)
                }, new FancyLinePiece[] {
                    new FancyLinePiece(
                        new string(Symbols.horizLine, Console.WindowWidth - 2 -2 * horizMargin),
                        theme.ExitInnerBg, theme.ExitNormalFg)
                }, new FancyLinePiece[] {
                    new FancyLinePiece("YOU ARE USING ", theme.ExitInnerBg, theme.ExitNormalFg),
                    new FancyLinePiece($"CKAN {Meta.GetVersion()}", theme.ExitInnerBg, theme.ExitHighlightFg),
                    new FancyLinePiece(".", theme.ExitInnerBg, theme.ExitNormalFg)
                }, new FancyLinePiece[] {
                }, new FancyLinePiece[] {
                    new FancyLinePiece("Thanks for downloading ", theme.ExitInnerBg, theme.ExitNormalFg),
                    ckanPiece,
                    new FancyLinePiece(". We hope you have as", theme.ExitInnerBg, theme.ExitNormalFg)
                }, new FancyLinePiece[] {
                    new FancyLinePiece("much fun using it as we had (and have) making it.", theme.ExitInnerBg, theme.ExitNormalFg)
                }, new FancyLinePiece[] {
                }, new FancyLinePiece[] {
                    new FancyLinePiece("If you have paid for ", theme.ExitInnerBg, theme.ExitNormalFg),
                    ckanPiece,
                    new FancyLinePiece(", try to get your money back,", theme.ExitInnerBg, theme.ExitNormalFg)
                }, new FancyLinePiece[] {
                    new FancyLinePiece("because you can download ", theme.ExitInnerBg, theme.ExitNormalFg),
                    ckanPiece,
                    new FancyLinePiece(" for free from", theme.ExitInnerBg, theme.ExitNormalFg)
                }, new FancyLinePiece[] {
                    new FancyLinePiece("https://github.com/KSP-CKAN/CKAN/releases/latest", theme.ExitInnerBg, theme.ExitLinkFg)
                }, new FancyLinePiece[] {
                }, new FancyLinePiece[] {
                    new FancyLinePiece("If you have any problems using ", theme.ExitInnerBg, theme.ExitNormalFg),
                    ckanPiece,
                    new FancyLinePiece(", please send us an issue at", theme.ExitInnerBg, theme.ExitNormalFg)
                }, new FancyLinePiece[] {
                    new FancyLinePiece("https://github.com/KSP-CKAN/CKAN/issues", theme.ExitInnerBg, theme.ExitLinkFg)
                }, new FancyLinePiece[] {
                }, new FancyLinePiece[] {
                    ckanPiece,
                    new FancyLinePiece(" WAS CREATED BY THE ", theme.ExitInnerBg, theme.ExitNormalFg),
                    ckanPiece,
                    new FancyLinePiece(" AUTHORS:", theme.ExitInnerBg, theme.ExitNormalFg)
                }, new FancyLinePiece[] {
                    new FancyLinePiece("https://github.com/KSP-CKAN/CKAN/graphs/contributors", theme.ExitInnerBg, theme.ExitLinkFg)
                }, new FancyLinePiece[] {
                }
            };

            for (int i = 0; i < lines.Length; ++i) {
                drawLine(theme, i, lines[i]);
            }
        }

        private void drawLine(ConsoleTheme theme, int y, FancyLinePiece[] pieces)
        {
            // First we need to know how long the text is for centering
            int textLen = 0;
            foreach (FancyLinePiece p in pieces) {
                textLen += p.Text.Length;
            }
            int boxW = Console.WindowWidth - 2 * horizMargin;
            int leftPad = (boxW - textLen) / 2;
            if (leftPad < 0) {
                leftPad = 0;
            }
            int rightPad = boxW - textLen - leftPad;
            if (rightPad < 0) {
                rightPad = 0;
            }

            // Left padding
            Console.SetCursorPosition(horizMargin, y);
            Console.BackgroundColor = theme.ExitInnerBg;
            Console.Write(new string(' ', leftPad));
            foreach (FancyLinePiece p in pieces) {
                p.Draw();
            }
            Console.Write(new string(' ', rightPad));
        }

        private const  int          horizMargin = 6;
    }

    /// <summary>
    /// An object representing a segment of a colorful exit screen line
    /// </summary>
    public class FancyLinePiece {

        /// <summary>
        /// Initialize a piece.
        /// </summary>
        /// <param name="text">The text that this piece contains</param>
        /// <param name="bg">Background color of this piece</param>
        /// <param name="fg">Foreground color of this piece</param>
        public FancyLinePiece(string text, ConsoleColor bg, ConsoleColor fg)
        {
            Text       = text;
            Background = bg;
            Foreground = fg;
        }

        /// <summary>
        /// Draw this piece at the current cursor location.
        /// </summary>
        public void Draw()
        {
            Console.BackgroundColor = Background;
            Console.ForegroundColor = Foreground;
            Console.Write(Text);
        }

        /// <summary>
        /// The text that this piece contains.
        /// </summary>
        public  readonly string       Text;
        private readonly ConsoleColor Background;
        private readonly ConsoleColor Foreground;
    }

}
