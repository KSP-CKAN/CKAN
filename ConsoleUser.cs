using System;
using log4net;

namespace CKAN
{
    public class ConsoleUser:IUser
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ConsoleUser));
        public bool DisplayYesNoDialog(string message)
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

        public void DisplayMessage(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        public void DisplayError(string message, params object[] args)
        {
            Console.Error.WriteLine(message,args);
        }

        public int WindowWidth
        {
            get { return Console.WindowWidth; }
        }
    }
}
