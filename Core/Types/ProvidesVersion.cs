namespace CKAN
{
    /// <summary>
    /// Represents a virtual version that was provided by another package.
    /// </summary>
    public sealed class ProvidesVersion : Version
    {
        private readonly string _providedBy;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProvidesVersion"/> class with the specified package name.
        /// </summary>
        /// <param name="providedBy">The name of the providing package.</param>
        public ProvidesVersion(string providedBy) : base("0")
        {
            _providedBy = providedBy;
        }

        /// <summary>
        /// Converts the value of the current <see cref="ProvidesVersion"/> object to a <see cref="string"/>
        /// representation.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> representation of the current <see cref="ProvidesVersion"/> object.>
        /// </returns>
        /// <remarks>
        /// The returned value is not a real version string and is for display purposes only.
        /// </remarks>
        public override string ToString()
        {
            return $"provided by {_providedBy}";
        }
    }
}
