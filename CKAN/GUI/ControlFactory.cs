using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace CKAN
{

    // this class ensures that all controls are created from the same thread
    // this is a mono limitation described here - http://www.mono-project.com/docs/faq/winforms/


    public class ControlFactory
    {

        private int m_MainThreadID = 0;

        public ControlFactory()
        {
            m_MainThreadID = Thread.CurrentThread.ManagedThreadId;
        }

        public T CreateControl<T>() where T : new()
        {
            if (Thread.CurrentThread.ManagedThreadId != m_MainThreadID)
            {
                Debugger.Break();
            }

            return new T();
        }
        

    }

}
