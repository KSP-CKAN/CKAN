using System;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    public class ConsoleUser : NullUser
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ConsoleUser));

        private bool m_Headless = false;
        public ConsoleUser(bool headless)
        {
            m_Headless = headless;
        }

        public override bool Headless
        {
            get
            {
                return m_Headless;
            }
        }

        protected override bool DisplayYesNoDialog(string message)
        {
            if (m_Headless)
            {
                return true;
            }

            Console.Write("{0} [Y/n] ", message);
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

        protected override void DisplayMessage(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        protected override void DisplayError(string message, params object[] args)
        {
            Console.Error.WriteLine(message, args);
        }

        protected override int DisplaySelectionDialog(string message, params object[] args)
        {
            const int return_cancel = -1;

            // Check for the headless flag.
            if (m_Headless)
            {
                // Return that the user cancelled the selection process.
                return return_cancel;
            }

            // Validate input.
            if (String.IsNullOrWhiteSpace(message))
            {
                throw new Kraken("Passed message string must be non-empty.");
            }

            if (args.Length == 0)
            {
                throw new Kraken("Passed list of selection candidates must be non-empty.");
            }

            // Check if we have a default selection.
            int defaultSelection = -1;

            if (args[0] is int)
            {
                // Check that the default selection makes sense.
                defaultSelection = (int)args[0];

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
                if (String.IsNullOrWhiteSpace(argument.ToString()))
                {
                    throw new Kraken("Candidate may not be empty.");
                }
            }

            // List options.
            for (int i = 0; i < args.Length; i++)
            {
                string CurrentRow = String.Format("{0}", i + 1);

                if (i == defaultSelection)
                {
                    CurrentRow += "*";
                }

                CurrentRow += String.Format(") {0}", args[i]);

                RaiseMessage(CurrentRow);
            }

            // Create message string.
            string output = String.Format("Enter a number between {0} and {1} (To cancel press \"c\" or \"n\".", 1, args.Length);

            if (defaultSelection >= 0)
            {
                output += String.Format(" \"Enter\" will select {0}.", defaultSelection + 1);
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
                if (String.IsNullOrEmpty(input))
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

        protected override void ReportProgress(string format, int percent)
        {
            if (Regex.IsMatch(format, "download", RegexOptions.IgnoreCase))
            {
                // In headless mode, only print a new message if the percent has changed,
                // to reduce clutter in Jenkins for large downloads
                if (!m_Headless || percent != previousPercent)
                {
                    // The \r at the front here causes download messages to *overwrite* each other.
                    Console.Write(
                        "\r{0} - {1}%           ", format, percent);
                    previousPercent = percent;
                }
            }
            else
            {
                // The percent looks weird on non-download messages.
                // The leading newline makes sure we don't end up with a mess from previous
                // download messages.
                Console.Write("\r\n{0}", format);
            }
        }

        private int previousPercent = -1;
    }
}
