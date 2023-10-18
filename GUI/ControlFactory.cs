using System.Diagnostics;
using System.Threading;

namespace CKAN.GUI
{
    /// <summary>
    /// This class ensures that all controls are created from the same thread.
    /// This is a mono limitation described here - http://www.mono-project.com/docs/faq/winforms/
    /// </summary>
    public class ControlFactory
    {
        private readonly int m_MainThreadID;

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
