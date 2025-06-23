namespace CKAN.Versioning
{
    /// <summary>
    /// Represents the version of a CKAN client.
    /// </summary>
    public sealed class CkanModuleVersion : ModuleVersion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CkanModuleVersion"/> class using the specified version string
        /// and human friendly name.
        /// </summary>
        /// <param name="version">A <see cref="string"/> in the appropriate format.</param>
        /// <param name="name">A human friendly name for the CKAN client release.</param>
        public CkanModuleVersion(string version, string name)
            : base(version)
        {
            Name = name;
            _string = string.Format(Properties.Resources.CkanModuleVersionToString, base.ToString(), Name);
        }

        /// <summary>
        /// The human friendly name of the CKAN client release.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Converts the value of the current <see cref="CkanModuleVersion"/> object to a <see cref="string"/>
        /// representation.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> representation of the current <see cref="CkanModuleVersion"/> object.>
        /// </returns>
        /// <remarks>
        /// The returned value is not a real version string and is for display purposes only.
        /// </remarks>
        public override string ToString()
            => _string;

        public bool SameClientVersion(ModuleVersion other)
            => other.Equals(this)
               || other.ToString().StartsWith($"{base.ToString()}.");

        private readonly string _string;
    }
}
