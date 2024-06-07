using System;
using System.Linq;
using System.Collections.Generic;

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

            // Specially formatted snippets
            var ckanPiece = new FancyLinePiece(Meta.GetProductName(), theme.ExitInnerBg, theme.ExitHighlightFg);
            var ckanVersionPiece = new FancyLinePiece($"{Meta.GetProductName()} {Meta.GetVersion()}", theme.ExitInnerBg, theme.ExitHighlightFg);
            var releaseLinkPiece = new FancyLinePiece("https://github.com/KSP-CKAN/CKAN/releases/latest", theme.ExitInnerBg, theme.ExitLinkFg);
            var issuesLinkPiece = new FancyLinePiece("https://github.com/KSP-CKAN/CKAN/issues", theme.ExitInnerBg, theme.ExitLinkFg);
            var authorsLinkPiece = new FancyLinePiece("https://github.com/KSP-CKAN/CKAN/graphs/contributors", theme.ExitInnerBg, theme.ExitLinkFg);

            FancyLinePiece[][] lines = new FancyLinePiece[][] {
                new FancyLinePiece(Properties.Resources.ExitTitle, theme.ExitInnerBg, theme.ExitNormalFg)
                    .Replace("{0}", ckanPiece).ToArray(),
                new FancyLinePiece[] {
                    new FancyLinePiece(
                        new string(Symbols.horizLine, Console.WindowWidth - 2 -(2 * horizMargin)),
                        theme.ExitInnerBg, theme.ExitNormalFg)
                },
            }.Concat(
                // Parse the single multi-line resource into array of array of FancyLinePiece
                Properties.Resources.ExitBody.Split(new string[] { "\r\n" }, StringSplitOptions.None)
                    // Each line generates one array of FancyLinePiece
                    .Select(ln => new FancyLinePiece(ln, theme.ExitInnerBg, theme.ExitNormalFg)
                        // This turns one FancyLinePiece into a sequence
                        .Replace("{0}", ckanPiece)
                        // From here on we go from sequence to sequence, flattening at each step
                        .SelectMany(flp => flp.Replace("{1}", ckanVersionPiece))
                        .SelectMany(flp => flp.Replace("{2}", releaseLinkPiece))
                        .SelectMany(flp => flp.Replace("{3}", issuesLinkPiece))
                        .SelectMany(flp => flp.Replace("{4}", authorsLinkPiece))
                        .ToArray()
                    )
                ).ToArray();

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
            int boxW = Console.WindowWidth - (2 * horizMargin);
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
        /// Replace found tokens with a FancyLinePiece
        /// </summary>
        /// <param name="tokens">Values for which to search the string</param>
        /// <param name="replacement">FancyLinePiece that should take the place of the tokens</param>
        /// <returns>
        /// FancyLinePiece array containing replacement where the tokens used to be
        /// </returns>
        public IEnumerable<FancyLinePiece> Replace(string[] tokens, FancyLinePiece replacement)
        {
            // Lambas can't yield return, and a separate method couldn't access our inputs
            IEnumerable<FancyLinePiece> InjectReplacement(string p, int i)
            {
                if (i > 0) {
                    // Return the replacement in between elements that weren't the token
                    yield return replacement;
                }
                if (p.Length > 0) {
                    // Skip empty pieces
                    yield return new FancyLinePiece(p, Background, Foreground);
                }
            }
            // We need to keep empty pieces from the split to handle tokens at the start
            var pieces = Text.Split(tokens, StringSplitOptions.None);
            return pieces.Length <= 1
                // Stop making new objects if no tokens found
                ? Enumerable.Repeat(this, 1)
                : pieces.SelectMany(InjectReplacement);
        }

        /// <summary>
        /// Replace found token with a FancyLinePiece
        /// </summary>
        /// <param name="token">Value for which to search the string</param>
        /// <param name="replacement">FancyLinePiece that should take the place of the token</param>
        /// <returns>
        /// FancyLinePiece array containing replacement where the token used to be
        /// </returns>
        public IEnumerable<FancyLinePiece> Replace(string token, FancyLinePiece replacement)
        {
            return Replace(new string[] { token }, replacement);
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
