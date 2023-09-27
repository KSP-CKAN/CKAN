using System;

namespace CKAN
{
    public class ProgressImmediate<T> : IProgress<T>
    {
        public ProgressImmediate(Action<T> action)
        {
            this.action = action;
        }

        public void Report(T val)
        {
            action(val);
        }

        private readonly Action<T> action;
    }
}
