using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using CKAN.Extensions;

namespace CKAN
{
    public partial class Registry
    {
        // IgnoreAndPopulate also clears these fields when rolling back creation of
        // the first profile from a snapshot without profile data.
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        private Dictionary<string, string[]>? profiles;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        private string? active_profile;

        [JsonIgnore] public string? ActiveProfile => active_profile;
        [JsonIgnore] public string[] ProfileNames => profiles?.Keys.Order().ToArray()
                                                     ?? Array.Empty<string>();
        [JsonIgnore] internal bool SwitchingProfile { get; set; }

        public string[] ProfileRoots(string name)
            => profiles != null && profiles.TryGetValue(name, out var roots)
                ? roots.ToArray()
                : throw new ArgumentException(Properties.Resources.ProfileNotFound, nameof(name));

        public void CreateProfile(string name)
        {
            ValidateProfileName(name);
            EnlistWithTransaction();
            profiles ??= new Dictionary<string, string[]>();
            profiles.Add(name, CurrentProfileRoots());
            active_profile ??= name;
        }

        public void RenameProfile(string name, string newName)
        {
            var roots = ProfileRoots(name);
            if (name == newName)
            {
                return;
            }
            ValidateProfileName(newName);
            EnlistWithTransaction();
            profiles!.Remove(name);
            profiles.Add(newName, roots);
            if (active_profile == name)
            {
                active_profile = newName;
            }
        }

        public void DeleteProfile(string name)
        {
            ProfileRoots(name);
            if (active_profile == name)
            {
                throw new ArgumentException(Properties.Resources.ProfileDeleteActive, nameof(name));
            }
            EnlistWithTransaction();
            profiles!.Remove(name);
        }

        internal void CaptureActiveProfile()
        {
            if (!SwitchingProfile && active_profile != null && profiles != null)
            {
                var roots = CurrentProfileRoots();
                if (!roots.SequenceEqual(profiles[active_profile]))
                {
                    EnlistWithTransaction();
                    profiles[active_profile] = roots;
                }
            }
        }

        internal void ActivateProfile(string name)
        {
            var roots = ProfileRoots(name).ToHashSet();
            EnlistWithTransaction();
            foreach (var mod in InstalledModules.Where(im => !im.Module.IsDLC))
            {
                mod.AutoInstalled = !roots.Contains(mod.identifier);
            }
            active_profile = name;
        }

        private string[] CurrentProfileRoots()
            => InstalledModules.Where(im => !im.AutoInstalled && !im.Module.IsDLC)
                               .Select(im => im.identifier).Order().ToArray();

        private void ValidateProfileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name != name.Trim()
                || name.Any(char.IsControl)
                || ProfileNames.Any(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(Properties.Resources.ProfileInvalidName, nameof(name));
            }
        }
    }
}
