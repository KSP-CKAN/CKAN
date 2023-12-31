using System;
using System.Linq;
using log4net;

namespace CKAN.NetKAN
{
    /// <summary>
    /// The commandline implementation of the IUser interface.
    /// It is exactly the same as the one of the CKAN-cmdline.
    /// At least at the time of this commit (git blame is your friend).
    /// </summary>
    public class ConsoleUser : IUser
    {
        /// <summary>
        /// A logger for this class.
        /// ONLY FOR INTERNAL USE!
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(ConsoleUser));

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CKAN.CmdLine.ConsoleUser"/> class
        /// </summary>
        /// <param name="headless">If set to <c>true</c>, suppress interactive dialogs like Yes/No-Dialog or SelectionDialog</param>
        public ConsoleUser(bool headless)
        {
            Headless = headless;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:CKAN.CmdLine.ConsoleUser"/> is headless.
        /// </summary>
        /// <value><c>true</c> if headless; otherwise, <c>false</c></value>
        public bool Headless { get; }

        /// <summary>
        /// Ask the user for a yes or no input
        /// </summary>
        /// <param name="question">Question</param>
        public bool RaiseYesNoDialog(string question)
        {
            if (Headless)
            {
                return true;
            }

            Console.Write("{0} [Y/n] ", question);
            while (true)
            {
                var input = Console.In.ReadLine();

                if (input == null)
                {
                    log.ErrorFormat("No console available for input, assuming no.");
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
                    // User pressed enter without any text, assuming default choice.
                    return true;
                }

                Console.Write("Invalid input. Please enter yes or no");
            }
        }

        /// <summary>
        /// Ask the user to select one of the elements of the array.
        /// The output is index 0 based.
        /// To supply a default option, make the first option an integer indicating the index of it.
        /// </summary>
        /// <returns>The selection dialog</returns>
        /// <param name="message">Message</param>
        /// <param name="args">Array of available options</param>
        public int RaiseSelectionDialog(string message, params object[] args)
        {
            const int return_cancel = -1;

            // Check for the headless flag.
            if (Headless)
            {
                // Return that the user cancelled the selection process.
                return return_cancel;
            }

            // Validate input.
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new Kraken("Passed message string must be non-empty.");
            }

            if (args.Length == 0)
            {
                throw new Kraken("Passed list of selection candidates must be non-empty.");
            }

            // Check if we have a default selection.
            int defaultSelection = -1;

            if (args[0] is int v)
            {
                // Check that the default selection makes sense.
                defaultSelection = v;

                if (defaultSelection < 0 || defaultSelection > args.Length - 1)
                {
                    throw new Kraken("Passed default arguments is out of range of the selection candidates.");
                }

                // Extract the relevant arguments.
                object[] newArgs = new object[args.Length - 1];

                for (int i = 1; i < args.Length; i++)
                {
                    newArgs[i - 1] = args[i];
                }

                args = newArgs;
            }

            // Further data validation.
            foreach (object argument in args)
            {
                if (string.IsNullOrWhiteSpace(argument.ToString()))
                {
                    throw new Kraken("Candidate may not be empty.");
                }
            }

            // List options.
            for (int i = 0; i < args.Length; i++)
            {
                string CurrentRow = string.Format("{0}", i + 1);

                if (i == defaultSelection)
                {
                    CurrentRow += "*";
                }

                CurrentRow += string.Format(") {0}", args[i]);

                RaiseMessage(CurrentRow);
            }

            // Create message string.
            string output = string.Format("Enter a number between {0} and {1} (To cancel press \"c\" or \"n\".", 1, args.Length);

            if (defaultSelection >= 0)
            {
                output += string.Format(" \"Enter\" will select {0}.", defaultSelection + 1);
            }

            output += "): ";

            RaiseMessage(output);

            bool valid = false;
            int result = 0;

            while (!valid)
            {
                // Wait for input from the command line.
                string input = Console.In.ReadLine();

                if (input == null)
                {
                    // No console present, cancel the process.
                    return return_cancel;
                }

                input = input.Trim().ToLower();

                // Check for default selection.
                if (string.IsNullOrEmpty(input))
                {
                    if (defaultSelection >= 0)
                    {
                        return defaultSelection;
                    }
                }

                // Check for cancellation characters.
                if (input == "c" || input == "n")
                {
                    RaiseMessage("Selection cancelled.");

                    return return_cancel;
                }

                // Attempt to parse the input.
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

                // Check the input against the boundaries.
                if (result > args.Length)
                {
                    RaiseMessage("The number in the input is too large.");
                    RaiseMessage(output);

                    continue;
                }
                else if (result < 1)
                {
                    RaiseMessage("The number in the input is too small.");
                    RaiseMessage(output);

                    continue;
                }

                // The list we provide is index 1 based, but the array is index 0 based.
                result--;

                // We have checked for all errors and have gotten a valid result. Stop the input loop.
                valid = true;
            }

            return result;
        }

        /// <summary>
        /// Write an error to the console
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Possible arguments to format the message</param>
        public void RaiseError(string message, params object[] args)
        {
            if (Headless)
            {
                log.ErrorFormat(
                    message.Replace("\r\n", "%0A"),
                    args.Select(a => a.ToString().Replace("\r\n", "%0A")).ToArray()
                );
            }
            else
            {
                Console.Error.WriteLine(message, args);
            }
        }

        /// <summary>
        /// Write a progress message including the percentage to the console.
        /// Rewrites the line, so the console is not cluttered by progress messages.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="percent">Progress in percent</param>
        public void RaiseProgress(string message, int percent)
        {
            // The percent looks weird on non-download messages.
            // The leading newline makes sure we don't end up with a mess from previous
            // download messages.
            Console.Write("\r\n{0}", message);
        }

        public void RaiseProgress(int percent, long bytesPerSecond, long bytesLeft)
        {
            // In headless mode, only print a new message if the percent has changed,
            // to reduce clutter in logs for large downloads
            if (!Headless || percent != previousPercent)
            {
                var fullMsg = string.Format(Properties.Resources.NetAsyncDownloaderProgress,
                                            CkanModule.FmtSize(bytesPerSecond),
                                            CkanModule.FmtSize(bytesLeft));
                // The \r at the front here causes download messages to *overwrite* each other.
                Console.Write("\r{0} - {1}%           ", fullMsg, percent);
                previousPercent = percent;
            }
        }

        /// <summary>
        /// Needed for <see cref="RaiseProgress(string, int)"/>
        /// </summary>
        private int previousPercent = -1;

        /// <summary>
        /// Writes a message to the console
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments to format the message</param>
        public void RaiseMessage(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }
    }
}
