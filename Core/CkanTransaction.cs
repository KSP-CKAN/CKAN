using System;
using System.Transactions;
using System.Reflection;

using log4net;

namespace CKAN
{

    public static class CkanTransaction
    {
        // as per http://blogs.msdn.com/b/dbrowne/archive/2010/05/21/using-new-transactionscope-considered-harmful.aspx

        static CkanTransaction()
        {
            // ChinhDo is incompatible with transaction timeouts on Windows; it can't
            // be aborted by another thread while the main thread is still working.
            // Disable transaction timeouts by maximizing the MaximumTimeout (49 days).
            SetMaxTimeout(maxCoretimeout);
        }

        public static TransactionScope CreateTransactionScope()
        {
            log.DebugFormat("Starting transaction with timeout {0:g}", transOpts.Timeout);
            return new TransactionScope(TransactionScopeOption.Required, transOpts);
        }

        // System.ArgumentOutOfRangeException : Time-out interval must be less than 2^32-2. (Parameter 'dueTime')
        private const double timeoutMs = Int32.MaxValue;
        private static readonly TimeSpan maxCoretimeout = TimeSpan.FromMilliseconds(timeoutMs);

        private static readonly TransactionOptions transOpts = new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout        = maxCoretimeout,
        };

        /// <summary>
        /// Set TransactionManager.MaximumTimeout with reflection
        /// </summary>
        /// <param name="timeout">New maximum transaction timeout</param>
        private static void SetMaxTimeout(TimeSpan timeout)
        {
            log.DebugFormat("Trying to set max timeout to {0:g}", timeout);
            if (TransactionManager.MaximumTimeout < timeout)
            {
                // TransactionManager.MaximumTimeout should not exist; if
                // app code tells TransactionScope's constructor that it needs
                // 2 hours to run a transaction, it's probably not wrong, and
                // the framework should listen and obey.  Instead,
                // TransactionManager reduces the requested span to 10 minutes.
                // But even worse, this limit can't be publicly changed!
                // Someone at Microsoft has arbitrarily decided that 10 minutes
                // is the maximum time for any transaction ever, without knowing
                // what those transactions need to do, and app programmers who do
                // know what they need to do can't override it no matter how dire
                // the need.
                // It can only be overridden by the end user, at the machine level,
                // and we can't ask every CKAN user to add a bunch of XML to some
                // random system file to ensure that core functionality works.
                // TransactionManager is unsuitable for use as-is, since it has
                // a built in time bomb ready to sabotage your application once you
                // hit that arbitrary limit, and you can't do anything about it.

                // To work around this design disaster, we commit our own
                // cardinal sin by using reflection to set private properties.
                // I wish TransactionManager did not force us to do this by
                // imposing incorrect behavior with no escape hatch.

                var t = typeof(TransactionManager);
                if (Platform.IsMono)
                {
                    // Mono
                    SetField(t, "machineSettings", null);
                    SetField(t, "maxTimeout",      timeout);
                }
                else
                {
                    // Framework
                    SetField(t, "_cachedMaxTimeout", true);
                    SetField(t, "_maximumTimeout",   timeout);
                    // Core
                    SetField(t, "s_cachedMaxTimeout", true);
                    SetField(t, "s_maximumTimeout",   timeout);
                }
            }
        }

        private static void SetField(Type T, string fieldName, object value)
        {
            try
            {
                T.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)
                    .SetValue(null, value);
            }
            catch
            {
                log.DebugFormat("Failed to set {0}", fieldName);
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(CkanTransaction));
    }
}
