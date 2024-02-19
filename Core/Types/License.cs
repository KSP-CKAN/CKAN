using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

#if NETSTANDARD2_0
using CKAN.Extensions;
#endif

namespace CKAN
{
    /// <summary>
    /// A spec complement license string
    /// </summary>
    [JsonConverter(typeof(JsonSimpleStringConverter))]
    public class License
    {
        public static readonly HashSet<string> valid_licenses =
            CKANSchema.schema.Definitions["license"]
                .Enumeration
                .Select(obj => obj.ToString())
                .ToHashSet();

        private static readonly HashSet<string> redistributable_licenses =
            CKANSchema.schema.Definitions["redistributable_license"]
                .Enumeration
                .Select(obj => obj.ToString())
                .ToHashSet();

        // Make sure this is the last static field so the others will be ready for the instance constructor!
        public static readonly License UnknownLicense = new License("unknown");

        private readonly string license;

        /// <summary>
        /// Takes a string and returns a license object.
        /// Throws a BadMetadataKraken if not a spec-confirming license.
        /// </summary>
        /// <param name="license">License.</param>
        public License(string license)
        {
            if (!valid_licenses.Contains(license))
            {
                throw new BadMetadataKraken(
                    null,
                    string.Format(Properties.Resources.LicenceInvalid, license)
                );
            }

            this.license = license;
        }

        /// <summary>
        /// Return whether this license permits CKAN and others to redistribute the module.
        /// We automatically upload such mods to https://archive.org/details/kspckanmods
        /// </summary>
        /// <returns>
        /// True if redistributable, false otherwise.
        /// </returns>
        public bool Redistributable => redistributable_licenses.Contains(license);

        /// <summary>
        /// Returns the license as a string.
        /// </summary>
        public override string ToString()
            => license;
    }
}
