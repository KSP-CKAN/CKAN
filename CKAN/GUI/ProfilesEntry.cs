using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CKAN
{
    [Serializable]
    public struct ProfilesEntry : IEquatable<ProfilesEntry>
    {
        public string Name;
        public List<string> ModIdentifiers;

        public override bool Equals(object obj)
        {
            if (obj is ProfilesEntry)
            {
                return this.Equals((ProfilesEntry)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public bool Equals(ProfilesEntry other)
        {
            return Name == other.Name;
        }

        public static ProfilesEntry Create(string name)
        {
            return new ProfilesEntry
            {
                Name = name,
                ModIdentifiers = new List<string>()
            };
        }

        public static ProfilesEntry Create(string name, List<string> modIdentifiers)
        {
            return new ProfilesEntry
            {
                Name = name,
                ModIdentifiers = new List<string>(modIdentifiers)
            };
        }

        public static ProfilesEntry Create(string name, ProfilesEntry other)
        {
            return new ProfilesEntry
            {
                Name = name,
                ModIdentifiers = new List<string>(other.ModIdentifiers)
            };
        }
    }
}
