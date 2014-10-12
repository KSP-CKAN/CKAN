// Communicate with the user (status messages, yes/no questions, etc)
// This class will proxy to either the GUI or cmdline functionality.

namespace CKAN {
    using System;

    public class User {

        // Send a line to the user. On a console, this does what you expect.
        // In the GUI, this should update the status bar.
        // This is also an obvious place to do logging as well.
        public static void WriteLine(string text, params object[] args) {

            // Format our message.
            string message = String.Format (text, args);

            // Right now we always send to the console, but when we add extra
            // interfaces we'll switch to the appropriate one here.
            Console.WriteLine (message);
        }
    }
}

