using System;
using CKAN.Versioning;

namespace CKAN.GUI
{
    public abstract class IGUIPlugin
    {

        public abstract string GetName();

        public abstract ModuleVersion GetVersion();

        public abstract void Initialize();

        public abstract void Deinitialize();

        public override string ToString()
        {
            return String.Format("{0} - {1}", GetName(), GetVersion());
        }
    }

}
