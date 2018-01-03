using System.Transactions;

namespace CKAN
{

    public static class CkanTransaction
    {

        // as per http://blogs.msdn.com/b/dbrowne/archive/2010/05/21/using-new-transactionscope-considered-harmful.aspx

        public static TransactionScope CreateTransactionScope()
        {
            return new TransactionScope(TransactionScopeOption.Required, transOpts);
        }

        private static TransactionOptions transOpts = new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout        = TransactionManager.MaximumTimeout
        };

    }
}
