namespace CKAN.Versioning
{
    /// <summary>
    /// Represents the version of a CKAN client.
    /// </summary>
    public sealed class CkanVersion : Version
    {
        private readonly string _string;

        /// <summary>
        /// The human friendly name of the CKAN client release.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CkanVersion"/> class using the specified version string and
        /// human friendly name.
        /// </summary>
        /// <param name="version">A <see cref="string"/> in the appropriate format.</param>
        /// <param name="name">A human friendly name for the CKAN client release.</param>
        public CkanVersion(string version, string name)
            : base(version)
        {
            Name = name;
            _string = $"{base.ToString()} aka {Name}";
        }

        /// <summary>
        /// Converts the value of the current <see cref="CkanVersion"/> object to a <see cref="string"/>
        /// representation.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> representation of the current <see cref="CkanVersion"/> object.>
        /// </returns>
        /// <remarks>
        /// The returned value is not a real version string and is for display purposes only.
        /// </remarks>
        public override string ToString()
        {
            return _string;
        }
    }
}
