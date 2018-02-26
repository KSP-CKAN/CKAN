namespace CKAN.Versioning
{
    /// <summary>
    /// Represents the version of a module that is not managed by CKAN.
    /// </summary>
    public sealed class UnmanagedModuleVersion : ModuleVersion
    {
        private readonly string _string;

        public UnmanagedModuleVersion(string version) : base(version)
        {
            _string = $"{version} (unmanaged)";
        }

        public override string ToString()
        {
            return _string;
        }
    }
}
