using System.Collections.Generic;

namespace CKAN
{
    public class ModuleTag
    {
        public ModuleTag(string name, HashSet<string> idents)
        {
            Name              = name;
            ModuleIdentifiers = idents;
        }

        public readonly string          Name;
        public readonly HashSet<string> ModuleIdentifiers;
    }
}
