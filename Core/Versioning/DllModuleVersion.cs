namespace CKAN.Versioning
{
    /// <summary>
    /// This class represents a DllVersion. They don't have real version numbers or anything
    /// </summary>
    public sealed class DllModuleVersion : ModuleVersion
    {
        private const string AutodetectedDllString = "autodetected dll";

        public DllModuleVersion() : base("0") { }

        public override string ToString()
        {
            return AutodetectedDllString;
        }
    }
}
