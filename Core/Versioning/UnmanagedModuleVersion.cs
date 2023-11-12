namespace CKAN.Versioning
{
    /// <summary>
    /// Represents the version of a module that is not managed by CKAN.
    /// </summary>
    public sealed class UnmanagedModuleVersion : ModuleVersion
    {
        private readonly string _string;

        public bool IsUnknownVersion { get; }

        // HACK: Hardcoding a value of "0" for autodetected DLLs preserves previous behavior.
        public UnmanagedModuleVersion(string version) : base(version ?? "0")
        {
            IsUnknownVersion = version == null;
            _string = version == null
                ? Properties.Resources.UnmanagedModuleVersionUnknown
                : string.Format(Properties.Resources.UnmanagedModuleVersionKnown, version);
        }

        public override string ToString()
            => _string;
    }
}
