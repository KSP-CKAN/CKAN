using System;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Collections;

namespace CKAN
{
    // Thanks and credit to this guy: https://stackoverflow.com/q/1952638/2422988

    class SingleAssemblyComponentResourceManager : ComponentResourceManager
    {
        public SingleAssemblyComponentResourceManager(Type t) : base(t)
        {
            contextTypeInfo = t;
        }

        protected override ResourceSet InternalGetResourceSet(CultureInfo culture,
            bool createIfNotExists, bool tryParents)
        {
            ResourceSet rs = (ResourceSet)this.ResourceSets[culture];
            if (rs == null)
            {
                // lazy-load default language (without caring about duplicate assignment in race conditions, no harm done);
                if (neutralResourcesCulture == null)
                {
                    neutralResourcesCulture = GetNeutralResourcesLanguage(this.MainAssembly);
                }

                // if we're asking for the default language, then ask for the
                // invariant (non-specific) resources.
                if (neutralResourcesCulture.Equals(culture))
                {
                    culture = CultureInfo.InvariantCulture;
                }
                string resourceFileName = GetResourceFileName(culture);

                Stream store = this.MainAssembly.GetManifestResourceStream(contextTypeInfo, resourceFileName);

                // If we found the appropriate resources in the local assembly
                if (store != null)
                {
                    rs = new ResourceSet(store);
                    // save for later.
                    AddResourceSet(this.ResourceSets, culture, ref rs);
                }
                else
                {
                    rs = base.InternalGetResourceSet(culture, createIfNotExists, tryParents);
                }
            }
            return rs;
        }

        // private method in framework, had to be re-specified here.
        private static void AddResourceSet(Hashtable localResourceSets, CultureInfo culture, ref ResourceSet rs)
        {
            lock (localResourceSets)
            {
                ResourceSet objA = (ResourceSet)localResourceSets[culture];
                if (objA != null)
                {
                    if (!object.Equals(objA, rs))
                    {
                        rs.Dispose();
                        rs = objA;
                    }
                }
                else
                {
                    localResourceSets.Add(culture, rs);
                }
            }
        }

        private Type        contextTypeInfo;
        private CultureInfo neutralResourcesCulture;
    }
}
