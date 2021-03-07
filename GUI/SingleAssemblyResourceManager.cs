using System.IO;
using System.Globalization;
using System.Resources;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace CKAN
{
    // Thanks and credit to this guy: https://stackoverflow.com/q/1952638/2422988

    class SingleAssemblyResourceManager : ResourceManager
    {
        public SingleAssemblyResourceManager(string basename, Assembly assembly) : base(basename, assembly)
        {
        }

        protected override ResourceSet InternalGetResourceSet(CultureInfo culture,
            bool createIfNotExists, bool tryParents)
        {
            ResourceSet rs;
            if (!myResourceSets.TryGetValue(culture, out rs))
            {
                // Lazy-load default language (without caring about duplicate assignment in race conditions, no harm done)
                if (neutralResourcesCulture == null)
                {
                    neutralResourcesCulture = GetNeutralResourcesLanguage(this.MainAssembly);
                }

                // If we're asking for the default language, then ask for the
                // invariant (non-specific) resources.
                if (neutralResourcesCulture.Equals(culture))
                {
                    culture = CultureInfo.InvariantCulture;
                }
                string resourceFileName = GetResourceFileName(culture);

                Stream store = this.MainAssembly.GetManifestResourceStream(resourceFileName);

                // If we found the appropriate resources in the local assembly
                if (store != null)
                {
                    rs = new ResourceSet(store);
                    // Save for later
                    myResourceSets.Add(culture, rs);
                }
                else
                {
                    rs = base.InternalGetResourceSet(culture, createIfNotExists, tryParents);
                }
            }
            return rs;
        }

        private CultureInfo neutralResourcesCulture;
        private Dictionary<CultureInfo, ResourceSet> myResourceSets = new Dictionary<CultureInfo, ResourceSet>();
    }
}
