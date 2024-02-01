using System;
using System.Linq;
using log4net;

namespace CKAN.CmdLine
{
    /// <summary>
    /// The commandline implementation of the IUser interface.
    /// </summary>
    public class ConsoleUser : IUser
    {
        /// <summary>
        /// A logger for this class.
        /// ONLY FOR INTERNAL USE!
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(ConsoleUser));

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CKAN.CmdLine.ConsoleUser"/> class.
        /// </summary>
        /// <param name="headless">If set to <c>true</c>, supress interactive dialogs like Yes/No-Dialog or SelectionDialog</param>
        public ConsoleUser(bool headless)
        {
            Headless = headless;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:CKAN.CmdLine.ConsoleUser"/> is headless
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

            Console.Write("\r\n{0} {1} ", question, Properties.Resources.UserYesNoPromptSuffix);
            while (true)
            {
                var input = Console.In.ReadLine();

                if (input == null)
                {
                    log.ErrorFormat("No console available for input, assuming no.");
                    return false;
                }

                input = input.ToLower().Trim();

                if (input.Equals(Properties.Resources.UserYesNoY) || input.Equals(Properties.Resources.UserYesNoYes))
                {
                    return true;
                }
                if (input.Equals(Properties.Resources.UserYesNoN) || input.Equals(Properties.Resources.UserYesNoNo))
                {
                    return false;
                }
                if (input.Equals(string.Empty))
                {
                    // User pressed enter without any text, assuming default choice.
                    return true;
                }

                Console.Write(Properties.Resources.UserYesNoInvalid);
            }
        }

        /// <summary>
        /// Ask the user to select one of the elements of the array.
        /// The output is index 0 based.
        /// To supply a default option, make the first option an integer indicating the index of it.
        /// </summary>
        /// <returns>The selected index or -1 if cancelled</returns>
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

            // Write passed message
            RaiseMessage(message);

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

            bool valid = false;
            int result = 0;

            while (!valid)
            {
                // Print message string
                RaiseMessage(defaultSelection >= 0
                    ? string.Format(Properties.Resources.UserSelectionPromptWithDefault, 1, args.Length, defaultSelection + 1)
                    : string.Format(Properties.Resources.UserSelectionPromptWithoutDefault,1, args.Length));

                // Wait for input from the command line.
                string input = Console.In.ReadLine();

                if (input == null)
                {
                    // No console present, cancel the process.
                    return return_cancel;
                }

                input = input.Trim().ToLower();

                // Check for default selection.
                if (string.IsNullOrEmpty(input) && defaultSelection >= 0)
                {
                    return defaultSelection;
                }

                // Check for cancellation characters.
                if (input == Properties.Resources.UserSelectionC || input == Properties.Resources.UserSelectionN)
                {
                    RaiseMessage(Properties.Resources.UserSelectionCancelled);
                    return return_cancel;
                }

                // Attempt to parse the input.
                try
                {
                    // The list we provide is index 1 based, but the array is index 0 based.
                    result = Convert.ToInt32(input) - 1;
                }
                catch (FormatException)
                {
                    RaiseMessage(Properties.Resources.UserSelectionNotNumber);
                    continue;
                }
                catch (OverflowException)
                {
                    RaiseMessage(Properties.Resources.UserSelectionTooLarge);
                    continue;
                }

                // Check the input against the boundaries.
                if (result > args.Length - 1)
                {
                    RaiseMessage(Properties.Resources.UserSelectionTooLarge);
                    continue;
                }
                else if (result < 0)
                {
                    RaiseMessage(Properties.Resources.UserSelectionTooSmall);
                    continue;
                }

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
            GoToStartOfLine();
            if (Headless)
            {
                // Special GitHub Action formatting for mutli-line errors
                log.ErrorFormat(
                    message.Replace("\r\n", "%0A"),
                    args.Select(a => a.ToString().Replace("\r\n", "%0A")).ToArray()
                );
            }
            else
            {
                Console.Error.WriteLine(message, args);
            }
            atStartOfLine = true;
        }

        /// <summary>
        /// Write a progress message including the percentage to the console.
        /// Rewrites the line, so the console is not cluttered by progress messages.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="percent">Progress in percent</param>
        public void RaiseProgress(string message, int percent)
        {
            if (message != lastProgressMessage)
            {
                // The percent looks weird on non-download messages.
                // The leading newline makes sure we don't end up with a mess from previous
                // download messages.
                GoToStartOfLine();
                Console.Write("{0}", message);
                lastProgressMessage = message;
            }

            // This message leaves the cursor at the end of a line of text
            atStartOfLine = false;
        }

        public void RaiseProgress(int percent, long bytesPerSecond, long bytesLeft)
        {
            if (!Headless || percent != previousPercent)
            {
                GoToStartOfLine();
                var fullMsg = string.Format(CKAN.Properties.Resources.NetAsyncDownloaderProgress,
                                            CkanModule.FmtSize(bytesPerSecond),
                                            CkanModule.FmtSize(bytesLeft));
                // The \r at the front here causes download messages to *overwrite* each other.
                Console.Write("\r{0} - {1}%           ", fullMsg, percent);
                previousPercent = percent;

                // This message leaves the cursor at the end of a line of text
                atStartOfLine = false;
            }
        }

        /// <summary>
        /// Needed for <see cref="RaiseProgress(string, int)"/>
        /// </summary>
        private int previousPercent = -1;

        private string lastProgressMessage = null;

        /// <summary>
        /// Writes a message to the console
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments to format the message</param>
        public void RaiseMessage(string message, params object[] args)
        {
            GoToStartOfLine();
            Console.WriteLine(message, args);
            atStartOfLine = true;
        }

        private void GoToStartOfLine()
        {
            if (!atStartOfLine)
            {
                // Carriage return
                Console.WriteLine("");
                atStartOfLine = true;
            }
        }

        private bool atStartOfLine = true;
    }
}
