using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using CKAN.Games;

namespace CKAN.GUI
{
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(ModuleIdentifiersRenamedConverter))]
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

        [JsonProperty("module_identifiers_by_game", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonToGamesDictionaryConverter))]
        private readonly Dictionary<string, HashSet<string>> ModuleIdentifiers =
            new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// Return the number of modules associated with this label for a given game
        /// </summary>
        /// <param name="game">Game to check</param>
        /// <returns>Number of modules</returns>
        public int ModuleCount(IGame game)
            => ModuleIdentifiers.TryGetValue(game.ShortName, out HashSet<string> identifiers)
                ? identifiers.Count
                : 0;

        /// <summary>
        /// Return whether a given identifier is associated with this label for a given game
        /// </summary>
        /// <param name="game">The game to check</param>
        /// <param name="identifier">The identifier to check</param>
        /// <returns>true if this label applies to this identifier, false otherwise</returns>
        public bool ContainsModule(IGame game, string identifier)
            => ModuleIdentifiers.TryGetValue(game.ShortName, out HashSet<string> identifiers)
               && identifiers.Contains(identifier);

        /// <summary>
        /// Check whether this label is active for a given game instance
        /// </summary>
        /// <param name="instanceName">Name of the instance</param>
        /// <returns>
        /// True if active, false otherwise
        /// </returns>
        public bool AppliesTo(string instanceName)
            => InstanceName == null || InstanceName == instanceName;

        public IEnumerable<string> IdentifiersFor(IGame game)
            => ModuleIdentifiers.TryGetValue(game.ShortName, out HashSet<string> idents)
                ? idents
                : Enumerable.Empty<string>();

        /// <summary>
        /// Add a module to this label's group
        /// </summary>
        /// <param name="identifier">The identifier of the module to add</param>
        public void Add(IGame game, string identifier)
        {
            if (ModuleIdentifiers.TryGetValue(game.ShortName, out HashSet<string> identifiers))
            {
                identifiers.Add(identifier);
            }
            else
            {
                ModuleIdentifiers.Add(game.ShortName, new HashSet<string> {identifier});
            }
        }

        /// <summary>
        /// Remove a module from this label's group
        /// </summary>
        /// <param name="identifier">The identifier of the module to remove</param>
        public void Remove(IGame game, string identifier)
        {
            if (ModuleIdentifiers.TryGetValue(game.ShortName, out HashSet<string> identifiers))
            {
                identifiers.Remove(identifier);
                if (identifiers.Count < 1)
                {
                    ModuleIdentifiers.Remove(game.ShortName);
                }
            }
        }
    }

    /// <summary>
    /// Protect old clients from trying to load a file they can't parse
    /// </summary>
    public class ModuleIdentifiersRenamedConverter : JsonPropertyNamesChangedConverter
    {
        protected override Dictionary<string, string> mapping
            => new Dictionary<string, string>
            {
                { "module_identifiers", "module_identifiers_by_game" }
            };
    }
}
