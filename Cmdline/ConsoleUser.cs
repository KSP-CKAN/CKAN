using System;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    /// <summary>
    /// The commandline implementation of the <see cref="CKAN.IUser"/> interface.
    /// </summary>
    public class ConsoleUser : IUser
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConsoleUser));

        private int _previousPercent = -1;
        private bool _atStartOfLine = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.ConsoleUser"/> class.
        /// </summary>
        /// <param name="headless">If set to <see langword="true"/>, suppresses interactive dialogs like the Yes/No-Dialog or the SelectionDialog.</param>
        public ConsoleUser(bool headless)
        {
            Headless = headless;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CKAN.CmdLine.ConsoleUser"/> is headless.
        /// </summary>
        public bool Headless { get; }

        /// <summary>
        /// Asks the user for a yes or no input.
        /// </summary>
        /// <param name="question">The question to display.</param>
        public bool RaiseYesNoDialog(string question)
        {
            if (Headless)
            {
                return true;
            }

            Console.Write("\r\n{0} [Y/n] ", question);
            while (true)
            {
                var input = Console.In.ReadLine();

                if (input == null)
                {
                    Log.Error("No console available for input, assuming no.");
                    return false;
                }

                input = input.ToLower().Trim();

                if (input.Equals("y") || input.Equals("yes"))
                {
                    return true;
                }

                if (input.Equals("n") || input.Equals("no"))
                {
                    return false;
                }

                if (input.Equals(string.Empty))
                {
                    // User pressed enter without any text, assuming default choice
                    return true;
                }

                Console.Write("Invalid input. Please enter yes or no");
            }
        }

        /// <summary>
        /// Asks the user to select one of the elements of the array.
        /// The output is index 0 based.
        /// To supply a default option, make the first option an integer indicating the index of it.
        /// </summary>
        /// <returns>The user inputted integer of the selection dialog.</returns>
        /// <param name="message">The message to display.</param>
        /// <param name="args">The arguments to format the message.</param>
        /// <exception cref="CKAN.Kraken">Thrown if <paramref name="message"/> or <paramref name="args"/> is <see langword="null"/>.</exception>
        public int RaiseSelectionDialog(string message, params object[] args)
        {
            const int returnCancel = -1;

            // Check for the headless flag
            if (Headless)
            {
                // Return that the user cancelled the selection process
                return returnCancel;
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new Kraken("The passed message string must be non-empty.");
            }

            if (args.Length == 0)
            {
                throw new Kraken("The passed list of selection candidates must be non-empty.");
            }

            // Check if we have a default selection
            var defaultSelection = -1;

            if (args[0] is int @int)
            {
                // Check that the default selection makes sense
                defaultSelection = @int;

                if (defaultSelection < 0 || defaultSelection > args.Length - 1)
                {
                    throw new Kraken("The passed default arguments are out of range of the selection candidates.");
                }

                // Extract the relevant arguments
                var newArgs = new object[args.Length - 1];

                for (var i = 1; i < args.Length; i++)
                {
                    newArgs[i - 1] = args[i];
                }

                args = newArgs;
            }

            // Further data validation
            foreach (var argument in args)
            {
                if (string.IsNullOrWhiteSpace(argument.ToString()))
                {
                    throw new Kraken("Candidate may not be empty.");
                }
            }

            // Write passed message
            RaiseMessage(message);

            // List options
            for (var i = 0; i < args.Length; i++)
            {
                var currentRow = string.Format("{0}", i + 1);

                if (i == defaultSelection)
                {
                    currentRow += "*";
                }

                currentRow += string.Format(") {0}", args[i]);

                RaiseMessage(currentRow);
            }

            // Create message string
            var output = "\r\n";

            output += args.Length == 1
                ? string.Format("Enter the number {0} or press \"Enter\" to select {0}", 1)
                : string.Format("Enter a number between {0} and {1} to select an instance", 1, args.Length);

            output += ". To cancel, press \"c\" or \"n\".";

            if (defaultSelection >= 0)
            {
                output += string.Format(" \"Enter\" will select {0}.", defaultSelection + 1);
            }

            RaiseMessage(output);

            var valid = false;
            var result = 0;

            while (!valid)
            {
                // Wait for input from the command line
                var input = Console.In.ReadLine();

                if (input == null)
                {
                    // No console present, cancel the process
                    return returnCancel;
                }

                input = input.Trim().ToLower();

                // Check for default selection
                if (string.IsNullOrEmpty(input))
                {
                    if (defaultSelection >= 0 || args.Length == 1)
                    {
                        return defaultSelection;
                    }
                }

                // Check for cancellation characters
                if (input == "c" || input == "n")
                {
                    RaiseMessage("Selection cancelled.");
                    return returnCancel;
                }

                // Attempt to parse the input
                try
                {
                    result = Convert.ToInt32(input);
                }
                catch (FormatException)
                {
                    RaiseMessage("The input is not a number.");
                    continue;
                }
                catch (OverflowException)
                {
                    RaiseMessage("The number in the input is too large.");
                    continue;
                }

                // Check the input against the boundaries
                if (result > args.Length)
                {
                    RaiseMessage("The number in the input is too large.\r\n{0}", output);
                    continue;
                }

                if (result < 1)
                {
                    RaiseMessage("The number in the input is too small.\r\n{0}", output);
                    continue;
                }

                // The list we provide is index 1 based, but the array is index 0 based
                result--;

                // We have checked for all errors and have gotten a valid result. Stop the input loop
                valid = true;
            }

            return result;
        }

        /// <summary>
        /// Writes an error message to the console.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="args">The arguments to format the message.</param>
        public void RaiseError(string message, params object[] args)
        {
            GoToStartOfLine();
            if (Headless)
            {
                // Special GitHub Action formatting for multi-line errors
                Log.ErrorFormat(message.Replace("\r\n", "%0A"), args.Select(a => a.ToString().Replace("\r\n", "%0A")).ToArray());
            }
            else
            {
                Console.Error.WriteLine(message, args);
            }

            _atStartOfLine = true;
        }

        /// <summary>
        /// Writes a progress message including the percentage to the console.
        /// Rewrites the line, so the console is not cluttered by progress messages.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="percent">The current progress in percent.</param>
        public void RaiseProgress(string message, int percent)
        {
            if (Regex.IsMatch(message, "download", RegexOptions.IgnoreCase))
            {
                // In headless mode, only print a new message if the percent has changed,
                // to reduce clutter in Jenkins for large downloads
                if (!Headless || percent != _previousPercent)
                {
                    // The \r at the front here causes download messages to *overwrite* each other.
                    Console.Write("\r{0} - {1}%           ", message, percent);
                    _previousPercent = percent;
                }
            }
            else
            {
                // The percent looks weird on non-download messages.
                // The leading newline makes sure we don't end up with a mess from previous
                // download messages.
                GoToStartOfLine();
                Console.Write("{0}", message);
            }

            // These messages leave the cursor at the end of a line of text
            _atStartOfLine = false;
        }

        /// <summary>
        /// Writes an informative message to the console.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="args">The arguments to format the message.</param>
        public void RaiseMessage(string message, params object[] args)
        {
            GoToStartOfLine();
            Console.WriteLine(message, args);
            _atStartOfLine = true;
        }

        private void GoToStartOfLine()
        {
            if (!_atStartOfLine)
            {
                // Carriage return
                Console.WriteLine("");
                _atStartOfLine = true;
            }
        }
    }
}
