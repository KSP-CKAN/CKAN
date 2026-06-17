using System;
using System.Collections.Generic;
using System.Globalization;

using CKAN;
using CKAN.Versioning;

namespace Tests.Data
{
    public class RandomModuleGenerator
    {
        public Random Generator { get; set; }

        public RandomModuleGenerator(Random generator)
        {
            Generator = generator;
        }

        public CkanModule GenerateRandomModule(
            GameVersion?                  ksp_version = null,
            List<RelationshipDescriptor>? conflicts   = null,
            List<RelationshipDescriptor>? depends     = null,
            List<RelationshipDescriptor>? recommends  = null,
            List<RelationshipDescriptor>? suggests    = null,
            List<string>?                 provides    = null,
            string?                       identifier  = null,
            ModuleVersion?                version     = null)
            => new CkanModule(new ModuleVersion(1.ToString(CultureInfo.InvariantCulture)),
                              identifier ?? Generator.Next().ToString(CultureInfo.InvariantCulture),
                              Generator.Next().ToString(CultureInfo.InvariantCulture),
                              Generator.Next().ToString(CultureInfo.InvariantCulture),
                              "",
                              new List<string> { Generator.Next().ToString(CultureInfo.InvariantCulture) },
                              new List<License> { License.UnknownLicense },
                              version ?? new ModuleVersion(Generator.Next().ToString(CultureInfo.InvariantCulture)),
                              new List<Uri> { new Uri("http://github.com/") })
            {
                ksp_version = ksp_version,
                conflicts   = conflicts,
                depends     = depends,
                recommends  = recommends,
                suggests    = suggests,
                provides    = provides,
            };
    }
}
