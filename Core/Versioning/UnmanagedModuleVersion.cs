namespace CKAN.Versioning
{
    /// <summary>
    /// Represents the version of a module that is not managed by CKAN.
    /// </summary>
    public sealed class UnmanagedModuleVersion : ModuleVersion
    {
        private readonly string _string;

        // HACK: Hardcoding a value of "0" for autodetected DLLs preserves previous behavior.
        public UnmanagedModuleVersion(string version) : base(version ?? "0")
        {
            _string = version is null ? "(unmanaged)" : $"{version} (unmanaged)";
        }

        public override string ToString()
        {
            return _string;
        }
    }
}
