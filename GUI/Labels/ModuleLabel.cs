using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CKAN.GUI
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ModuleLabel
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string  Name;

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public Color   Color;

        [JsonProperty("instance_name", NullValueHandling = NullValueHandling.Ignore)]
        public string  InstanceName;

        [JsonProperty("hide", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool    Hide;

        [JsonProperty("notify_on_change", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool    NotifyOnChange;

        [JsonProperty("remove_on_change", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool    RemoveOnChange;

        [JsonProperty("alert_on_install", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool    AlertOnInstall;

        [JsonProperty("remove_on_install", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool    RemoveOnInstall;

        [JsonProperty("hold_version", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool    HoldVersion;

        [JsonProperty("module_identifiers", NullValueHandling = NullValueHandling.Ignore)]
        public HashSet<string> ModuleIdentifiers = new HashSet<string>();

        /// <summary>
        /// Check whether this label is active for a given game instance
        /// </summary>
        /// <param name="instanceName">Name of the instance</param>
        /// <returns>
        /// True if active, false otherwise
        /// </returns>
        public bool AppliesTo(string instanceName)
        {
            return InstanceName == null || InstanceName == instanceName;
        }

        /// <summary>
        /// Add a module to this label's group
        /// </summary>
        /// <param name="identifier">The identifier of the module to add</param>
        public void Add(string identifier)
        {
            ModuleIdentifiers.Add(identifier);
        }

        /// <summary>
        /// Remove a module from this label's group
        /// </summary>
        /// <param name="identifier">The identifier of the module to remove</param>
        public void Remove(string identifier)
        {
            ModuleIdentifiers.Remove(identifier);
        }
    }
}
