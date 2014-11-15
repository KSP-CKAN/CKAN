// Communicate with the user (status messages, yes/no questions, etc)
// This class will proxy to either the GUI or cmdline functionality.

using System;
using System.Transactions;
using log4net;

namespace CKAN
{
    public delegate bool DisplayYesNoDialog(string message);

    public delegate void DisplayMessage(string message, params object[] args);

    public delegate void DisplayError(string message, params object[] args);

    public enum FrontEndType
    {
        All,
        CommandLine,
        UI,
    }

    public class User
    {
        public static FrontEndType frontEnd = FrontEndType.CommandLine;
        public static DisplayYesNoDialog yesNoDialog = YesNoDialogConsole;
        public static DisplayMessage displayMessage = WriteLineConsole;
        public static DisplayError displayError = WriteLineConsole;

        private static readonly ILog log = LogManager.GetLogger(typeof (User));

        // Send a line to the user. On a console, this does what you expect.
        // In the GUI, this should update the status bar.
        // This is also an obvious place to do logging as well.
        public static void WriteLine(string text, params object[] args)
        {
            displayMessage(text, args);
        }

        /// <summary>
        ///     Prompts the user for a Y/N response.
        ///     Returns true for yes, false for no.
        /// </summary>
        public static bool YesNo(string text = null, FrontEndType _frontEnd = FrontEndType.All)
        {
            if (_frontEnd != FrontEndType.All && _frontEnd != frontEnd)
            {
                return true;
            }

            if (Transaction.Current != null)
            {
                log.Warn("Asking the user a question during a transaction! What are we thinking?");
            }

            return yesNoDialog(text);
        }

        public static void Error(string text, params object[] args)
        {
            displayError(text, args);
        }

        public static bool YesNoDialogConsole(string text = null)
        {
            while(true)
            {
                if (text != null)
                {
                    WriteLine("{0} [Y/N]", text);
                    
                    string input = Console.In.ReadLine().ToLower();
    
                    if (input.Equals("y") || input.Equals("yes"))
                    {
                        return true;
                    }
                    if (input.Equals("n") || input.Equals("no"))
                    {
                        return false;
                    }
                    
                }
            }
        }

        public static void WriteLineConsole(string text, params object[] args)
        {
            // Format our message.
            string message = String.Format(text, args);

            // Right now we always send to the console, but when we add extra
            // interfaces we'll switch to the appropriate one here.
            Console.WriteLine(message);
        }
    }
}
