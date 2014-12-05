using System;
using System.Transactions;
using log4net;

namespace CKAN
{

    public class TransactionScopeAlreadyDisposed : Kraken
    {
        public TransactionScopeAlreadyDisposed(string msg)
            : base(msg)
        {
        }

    }

    public class TransactionScope : IDisposable
    {
        // as per http://blogs.msdn.com/b/dbrowne/archive/2010/05/21/using-new-transactionscope-considered-harmful.aspx

        private static readonly ILog log = LogManager.GetLogger(typeof(ModuleInstaller));

        private System.Transactions.TransactionScope m_Scope = null;

        public TransactionScope()
        {
            var transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted;
            transactionOptions.Timeout = TransactionManager.MaximumTimeout;
            m_Scope = new System.Transactions.TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }

        public void Complete()
        {
            if (m_Scope == null)
            {
                log.ErrorFormat("Calling Complete() on a scope that has already been disposed!");
                throw new TransactionScopeAlreadyDisposed("Calling Complete() on a scope that has already been disposed!");
            }

            m_Scope.Complete();
        }

        public void Dispose()
        {
            if (m_Scope == null)
            {
                log.ErrorFormat("Calling Dispose() on a scope that has already been disposed!");
                throw new TransactionScopeAlreadyDisposed("Calling Dispose() on a scope that has already been disposed!");
            }

            m_Scope.Dispose();
            m_Scope = null;
        }
    }
}
