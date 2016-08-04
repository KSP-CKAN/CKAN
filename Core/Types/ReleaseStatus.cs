using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CKAN
{
    /// <summary>
    /// A release status, complying to the CKAN spec.
    /// </summary>
    [JsonConverter(typeof(JsonSimpleStringConverter))]
    public class ReleaseStatus
    {
        private static readonly HashSet<string> valid_statuses = new HashSet<string> {
            "stable", "testing", "development" // Spec 1.0 statuses
        };

        private string status;

        /// <summary>
        /// Creates a ReleaseStatus object which compiles to the CKAN spec.
        /// Throws a BadMetadataKraken if passed a non-compliant string.
        /// </summary>
        /// <param name="status">Status.</param>
        public ReleaseStatus(string status)
        {
            switch (status)
            {
                // As per the spec, if the status is null, we assume stable.
                case null:
                    status = "stable";
                    break;

                // For compatibility with older metadata, we map 'alpha' and 'beta'
                // to 'development' and 'testing'.

                case "alpha":
                    status = "development";
                    break;

                case "beta":
                    status = "testing";
                    break;
            }

            if (!valid_statuses.Contains(status))
            {
                throw new BadMetadataKraken(
                    null,
                    String.Format("{0} is not a valid release status", status)
                );
            }

            this.status = status;
        }

        public override string ToString()
        {
            return status;
        }
    }
}