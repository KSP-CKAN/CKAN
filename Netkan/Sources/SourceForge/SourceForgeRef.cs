using System.Text.RegularExpressions;

using CKAN.Extensions;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Sources.SourceForge
{
    /// <summary>
    /// Represents a SourceForge $kref
    /// </summary>
    internal sealed class SourceForgeRef : RemoteRef
    {
        /// <summary>
        /// Initialize the SourceForge reference
        /// </summary>
        /// <param name="reference">The base $kref object from a netkan</param>
        public SourceForgeRef(RemoteRef reference)
            : base(reference)
        {
            if (reference.Id != null
                && Pattern.TryMatch(reference.Id, out Match? match))
            {
                Name = match.Groups["name"].Value;
            }
            else
            {
                throw new Kraken(string.Format(@"Could not parse reference: ""{0}""",
                                 reference));
            }
        }

        /// <summary>
        /// The name of the project on SourceForge
        /// </summary>
        public readonly string Name;

        private static readonly Regex Pattern =
            new Regex(@"^(?<name>[^/]+)$",
                      RegexOptions.Compiled);
    }
}
