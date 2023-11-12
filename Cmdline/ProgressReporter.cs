using System.Text.RegularExpressions;

namespace CKAN.CmdLine
{
    /// <summary>
    /// A simple class that manages progress report events for the CmdLine.
    /// This will almost certainly need extra functionality if we deprecate the User class.
    /// </summary>
    public static class ProgressReporter
    {
        /// <summary>
        /// Only shows download report messages, and nothing else.
        /// </summary>
        public static void FormattedDownloads(string message, int progress, IUser user)
        {
            if (Regex.IsMatch(message, "download", RegexOptions.IgnoreCase))
            {
                // The \r at the front causes download messages to *overwrite* each other.
                user.RaiseMessage("\r{0} - {1}%           ", message, progress);
            }
            else
            {
                // The percent looks weird on non-download messages.
                // The leading newline makes sure we don't end up with a mess from previous
                // download messages.
                user.RaiseMessage("\r\n{0}", message);
            }
        }
    }
}
