using System.Diagnostics.CodeAnalysis;
using CKAN.Versioning;

namespace CKAN.GUI
{
    [ExcludeFromCodeCoverage]
    public abstract class IGUIPlugin
    {

        public abstract string GetName();

        public abstract ModuleVersion GetVersion();

        public abstract void Initialize();

        public abstract void Deinitialize();

        public override string ToString()
            => string.Format("{0} - {1}", GetName(), GetVersion());
    }

}
