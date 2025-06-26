using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.ExceptionServices;

namespace CKAN.Extensions
{
    public static class ExceptionExtensions
    {
        public static void RethrowInner(this AggregateException agExc)
        {
            var inner = agExc switch
            {
                { InnerException:  Exception                     exc  } => exc,
                { InnerExceptions: ReadOnlyCollection<Exception> excs } => excs.First(),
                _ => agExc,
            };
            ExceptionDispatchInfo.Capture(inner).Throw();
        }
    }
}
