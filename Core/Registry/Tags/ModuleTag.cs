using System.Collections.Generic;

namespace CKAN
{
    public class ModuleTag
    {
        public string          Name;
        public HashSet<string> ModuleIdentifiers = new HashSet<string>();

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
