using System.Transactions;

namespace CKAN
{
    public static class CkanTransaction
    {
        // as per http://blogs.msdn.com/b/dbrowne/archive/2010/05/21/using-new-transactionscope-considered-harmful.aspx

        public static TransactionScope CreateTransactionScope()
        {
            var transactionOptions = new TransactionOptions
            {
                Timeout = TransactionManager.MaximumTimeout
            };
            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }
    }
}