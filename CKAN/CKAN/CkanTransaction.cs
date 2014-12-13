using System;
using System.Transactions;

namespace CKAN
{

    public class CkanTransaction : IDisposable
    {

        // as per http://blogs.msdn.com/b/dbrowne/archive/2010/05/21/using-new-transactionscope-considered-harmful.aspx

        private TransactionScope m_Scope;

        public CkanTransaction()
        {
            var transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted;
            transactionOptions.Timeout = TransactionManager.MaximumTimeout;
            m_Scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }

        public void Complete()
        {
            m_Scope.Complete();
        }

        public void Dispose()
        {
            m_Scope.Dispose();
        }

    }
}
