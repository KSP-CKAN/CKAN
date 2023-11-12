namespace CKAN.Versioning
{
    /// <summary>
    /// Represents a virtual version that was provided by another module.
    /// </summary>
    public sealed class ProvidesModuleVersion : ModuleVersion
    {
        private readonly string _string;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProvidesModuleVersion"/> class with the specified module
        /// identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the providing module.</param>
        /// <param name="version">The version of the providing module.</param>
        public ProvidesModuleVersion(string identifier, string version) : base(version)
        {
            _string = string.Format(Properties.Resources.ProvidesModuleVersionToString, version, identifier);
        }

        /// <summary>
        /// Converts the value of the current <see cref="ProvidesModuleVersion"/> object to a <see cref="string"/>
        /// representation.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> representation of the current <see cref="ProvidesModuleVersion"/> object.>
        /// </returns>
        /// <remarks>
        /// The returned value is not a real version string and is for display purposes only.
        /// </remarks>
        public override string ToString()
            => _string;
    }
}
