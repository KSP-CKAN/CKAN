using System.Collections.Generic;
using Newtonsoft.Json;

namespace CKAN
{
    /// <summary>
    /// A Spec compliment license string.
    /// </summary>
    [JsonConverter(typeof(JsonSimpleStringConverter))]
    public class License
    {
        // TODO: It would be lovely for our build system to write these for us.
        private static readonly HashSet<string> valid_licenses = new HashSet<string> {
            "public-domain",
            "Apache", "Apache-1.0", "Apache-2.0",
            "Artistic", "Artistic-1.0", "Artistic-2.0",
            "BSD-2-clause", "BSD-3-clause", "BSD-4-clause",
            "ISC",
            "CC-BY", "CC-BY-1.0", "CC-BY-2.0", "CC-BY-2.5", "CC-BY-3.0", "CC-BY-4.0",
            "CC-BY-SA", "CC-BY-SA-1.0", "CC-BY-SA-2.0", "CC-BY-SA-2.5", "CC-BY-SA-3.0", "CC-BY-SA-4.0",
            "CC-BY-NC", "CC-BY-NC-1.0", "CC-BY-NC-2.0", "CC-BY-NC-2.5", "CC-BY-NC-3.0", "CC-BY-NC-4.0",
            "CC-BY-NC-SA", "CC-BY-NC-SA-1.0", "CC-BY-NC-SA-2.0", "CC-BY-NC-SA-2.5", "CC-BY-NC-SA-3.0", "CC-BY-NC-SA-4.0",
            "CC-BY-NC-ND", "CC-BY-NC-ND-1.0", "CC-BY-NC-ND-2.0", "CC-BY-NC-ND-2.5", "CC-BY-NC-ND-3.0", "CC-BY-NC-ND-4.0",
            "CC0",
            "CDDL", "CPL",
            "EFL-1.0", "EFL-2.0",
            "Expat", "MIT",
            "GPL-1.0", "GPL-2.0", "GPL-3.0",
            "LGPL-2.0", "LGPL-2.1", "LGPL-3.0",
            "GFDL-1.0", "GFDL-1.1", "GFDL-1.2", "GFDL-1.3",
            "GFDL-NIV-1.0", "GFDL-NIV-1.1", "GFDL-NIV-1.2", "GFDL-NIV-1.3",
            "LPPL-1.0", "LPPL-1.1", "LPPL-1.2", "LPPL-1.3c",
            "MPL-1.1",
            "Perl",
            "Python-2.0",
            "QPL-1.0",
            "W3C",
            "Zlib",
            "Zope",
            "open-source", "restricted", "unrestricted", "unknown"
        };

        // TODO: It would be nice to generate these as well
        /// <summary>
        /// Contains a mapping between non-valid license name that can be converted and valid one for sanitization.
        /// </summary>
        private static readonly Dictionary<string, string> license_synonyms = new Dictionary<string, string> {
            { "GPLv3", "GPL-3.0" }
        };

        private string license;

        /// <summary>
        /// Takes a string and returns a license object.
        /// Throws a BadMetadataKraken if not a spec-confirming license.
        /// </summary>
        /// <param name="license">License.</param>
        public License(string license)
        {
            license = GetSynonymIfAvaliable(license);

            if (!valid_licenses.Contains(license))
            {
                throw new BadMetadataKraken(
                    null,
                    string.Format("The license {0} is invalid", license)
                );
            }

            this.license = license;
        }

        /// <summary>
        /// If a license has a valid synonym, this method returns it. If not, <paramref name="license"/> is returned.
        /// </summary>
        /// <param name="license">License to check for a valid synonym</param>
        /// <returns>Valid synonym or <paramref name="license"/> if not found</returns>
        private string GetSynonymIfAvaliable(string license)
        {
            if (license_synonyms.ContainsKey(license))
            {
                return license_synonyms[license];
            }

            return license;
        }

        /// <summary>
        /// Returns the license as a string.
        /// </summary>
        public override string ToString()
        {
            return license;
        }
    }
}

