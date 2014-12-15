using System;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN
{
    public class ConsoleUser:NullUser
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ConsoleUser));
        
        protected override bool DisplayYesNoDialog(string message)
        {
            Console.Write("{0} [Y/N] ", message);
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
                Console.Write("Invaild input. Please enter yes or no");
            }
        }

        protected override void DisplayMessage(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        protected override void DisplayError(string message, params object[] args)
        {
            Console.Error.WriteLine(message,args);
        }

        protected override int DisplaySelectionDialog(string message, params string[] args)
        {
            // Validate input.
            if (String.IsNullOrWhiteSpace(message))
            {
                throw new Kraken("Passed message string must be non-empty.");
            }

            if (args.Length == 0)
            {
                throw new Kraken("Passed list of selection candidates must be non-empty.");
            }

            foreach (string argument in args)
            {
                if (String.IsNullOrWhiteSpace(argument))
                {
                    throw new Kraken("Candidate may not be empty.");
                }
            }

            // List options.
            for (int i = 0; i < args.Length; i++)
            {
                string CurrentRow = String.Format("{0}) {1}", i + 1, args[i]);

                RaiseMessage(CurrentRow);
            }

            // Create message string.
            string output = String.Format("Enter a number between {0} and {1} (To cancel press \"c\" or \"n\"): ", 1, args.Length);

            RaiseMessage(output);

            bool valid = false;
            int result = 0;

            while (!valid)
            {
                // Wait for input from the command line.
                string input = Console.ReadLine().Trim().ToLower();

                // Check for cancellation characters.
                if (input == "c" || input == "n")
                {
                    RaiseMessage("Selection cancelled.");

                    return -1;
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
                 DisplayMessage(
                    // The \r at the front here causes download messages to *overwrite* each other.
                     String.Format("\r{0} - {1}%           ", format, percent)
                 );
            }
            else
            {
                // The percent looks weird on non-download messages.
                // The leading newline makes sure we don't end up with a mess from previous
                // download messages.
                DisplayMessage("\n{0}", format);
            }
        }
        protected override void ReportDownloadsComplete(Uri[] urls, string[] filenames, Exception[] errors)
        {
        }
        public override int WindowWidth
        {
            get { return Console.WindowWidth; }
        }
        
    }
}
