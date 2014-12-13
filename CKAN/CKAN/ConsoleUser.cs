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
