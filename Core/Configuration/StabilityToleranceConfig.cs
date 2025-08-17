using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

using CKAN.Extensions;

namespace CKAN.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StabilityToleranceConfig
    {
        public StabilityToleranceConfig(string path)
        {
            this.path = path;
            try
            {
                JsonConvert.PopulateObject(File.ReadAllText(this.path), this);
            }
            catch
            {
                // File doesn't exist yet, we can create it at save
            }
        }

        public bool Save()
        {
            try
            {
                JsonConvert.SerializeObject(this, Formatting.Indented)
                           .WriteThroughTo(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ReleaseStatus? ModStabilityTolerance(string identifier)
            => modStabilityTolerance.TryGetValue(identifier, out ReleaseStatus relStat)
                   ? relStat
                   : null;

        public void SetModStabilityTolerance(string identifier, ReleaseStatus? relStat)
        {
            if (relStat is ReleaseStatus rs)
            {
                modStabilityTolerance[identifier] = rs;
            }
            else
            {
                modStabilityTolerance.Remove(identifier);
            }
            Changed?.Invoke(identifier, relStat);
            Save();
        }

        public ReleaseStatus OverallStabilityTolerance
        {
            get => overallStabilityTolerance;
            set
            {
                overallStabilityTolerance = value;
                Save();
                Changed?.Invoke(null, value);
            }
        }

        public IReadOnlyCollection<string> OverriddenModIdentifiers => modStabilityTolerance.Keys;

        public event Action<string?, ReleaseStatus?>? Changed;

        [JsonProperty("overall_stability_tolerance", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(ReleaseStatus.stable)]
        private ReleaseStatus overallStabilityTolerance = ReleaseStatus.stable;

        [JsonProperty("mod_stability_tolerance", NullValueHandling = NullValueHandling.Ignore)]
        private readonly SortedDictionary<string, ReleaseStatus> modStabilityTolerance =
            new SortedDictionary<string, ReleaseStatus>();

        [JsonIgnore]
        private readonly string path;
    }
}
