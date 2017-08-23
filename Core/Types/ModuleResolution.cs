using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CKAN.Types
{
    public class ModuleResolution : IEnumerable<CkanModule>
    {
        public List<CkanModule> CachedModules { get; set; }
        public List<CkanModule> UncachedModules { get; set; }

        public IEnumerable<CkanModule> All => CachedModules.Concat(UncachedModules);

        public int Count => CachedModules.Count + UncachedModules.Count;

        public ModuleResolution(IEnumerable<CkanModule> modules, Func<CkanModule, bool> isCached)
        {
            CachedModules = new List<CkanModule>();
            UncachedModules = new List<CkanModule>();

            foreach (var module in modules)
            {
                if (isCached(module))
                {
                    CachedModules.Add(module);
                }
                else
                {
                    UncachedModules.Add(module);
                }
            }
        }

        public IEnumerator<CkanModule> GetEnumerator()
        {
            return All.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
