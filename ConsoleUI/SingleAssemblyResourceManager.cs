using System.IO;
using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Collections.Generic;

namespace CKAN.ConsoleUI
{
    // Thanks and credit to this guy: https://stackoverflow.com/q/1952638/2422988

    /// <summary>
    /// Wrapper around ResourceManager that retrieves strings from the assembly
    /// rather than external files
    /// </summary>
    public class SingleAssemblyResourceManager : ResourceManager
    {
        /// <summary>
        /// Initialize the resource manager
        /// </summary>
        /// <param name="basename">To be passed to ResourceManager</param>
        /// <param name="assembly">To be passed to ResourceManager</param>
        public SingleAssemblyResourceManager(string basename, Assembly assembly)
            : base(basename, assembly)
        {
        }

        /// <summary>
        /// Provides resources from the assembly to ResourceManager
        /// </summary>
        /// <param name="culture">The language to get</param>
        /// <param name="createIfNotExists">Set to false to avoid loading if not already cached</param>
        /// <param name="tryParents">Just gets passed to base class implementation</param>
        /// <returns></returns>
        protected override ResourceSet InternalGetResourceSet(CultureInfo culture,
            bool createIfNotExists, bool tryParents)
        {
            if (!myResourceSets.TryGetValue(culture, out ResourceSet rs) && createIfNotExists)
            {
                // Lazy-load default language (without caring about duplicate assignment in race conditions, no harm done)
                if (neutralResourcesCulture == null)
                {
                    neutralResourcesCulture = GetNeutralResourcesLanguage(MainAssembly);
                }

                // If we're asking for the default language, then ask for the
                // invariant (non-specific) resources.
                if (neutralResourcesCulture.Equals(culture))
                {
                    culture = CultureInfo.InvariantCulture;
                }
                string resourceFileName = GetResourceFileName(culture);

                Stream store = MainAssembly.GetManifestResourceStream(resourceFileName);

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
        private readonly Dictionary<CultureInfo, ResourceSet> myResourceSets = new Dictionary<CultureInfo, ResourceSet>();
    }
}
