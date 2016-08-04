using System;

namespace CKAN
{
    public abstract class IGUIPlugin
    {
        public abstract string GetName();

        public abstract Version GetVersion();

        public abstract void Initialize();

        public abstract void Deinitialize();

        public override string ToString()
        {
            return String.Format("{0} - {1}", GetName(), GetVersion());
        }
    }
}