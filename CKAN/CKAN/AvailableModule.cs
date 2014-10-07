namespace CKAN {
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Utility class to track version -> module mappings
    /// </summary>
    public class AvailableModule {
        public Dictionary<string, CkanModule> version = new Dictionary<string, CkanModule> ();

        public AvailableModule() { }

        public void Add(CkanModule module) {
            version [module.version] = module;
        }
    }
}

