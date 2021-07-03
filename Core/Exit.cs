namespace CKAN
{
    /// <summary>
    /// Exit codes for the command line interfaces.
    /// </summary>
    public static class Exit
    {
        /// <summary>
        /// No errors. The program executed successfully.
        /// </summary>
        public static readonly int Ok = 0;

        /// <summary>
        /// The program returned an error.
        /// </summary>
        public static readonly int Error = 1;

        /// <summary>
        /// The command line parameters could not be parsed.
        /// </summary>
        public static readonly int BadOpt = 2;
    }
}
