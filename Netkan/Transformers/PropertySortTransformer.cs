using System;
using System.Collections.Generic;
using System.Linq;
using CKAN.NetKAN.Model;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class PropertySortTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PropertySortTransformer));

        private const int DefaultSortOrder = 1073741823; // int.MaxValue / 2

        private static readonly Dictionary<string, int> PropertySortOrder = new Dictionary<string, int>
        {
            { "spec_version", 0 },
            { "comment", 1 },
            { "identifier", 2 },
            { "$kref", 3 },
            { "$vref", 4 },
            { "name", 5 },
            { "abstract", 6 },
            { "description", 7 },
            { "author", 8 },
            { "license", 9 },
            { "release_status", 10 },
            { "resources", 11 },
            { "version", 12 },
            { "ksp_version", 13 },
            { "ksp_version_min", 14 },
            { "ksp_version_max", 15 },
            { "ksp_version_strict", 16 },
            { "provides", 17 },
            { "depends", 18 },
            { "recommends", 19 },
            { "suggests", 20 },
            { "supports", 21 },
            { "conflicts", 22 },
            { "install", 23 },
            { "download", 24 },
            { "download_size", 25 },
            { "download_hash", 26 },
            { "download_content_type", 27 },

            { "x_generated_by", int.MaxValue }
        };

        private static readonly Dictionary<string, int> ResourcePropertySortOrder = new Dictionary<string, int>
        {
            { "homepage", 0 },
            { "spacedock", 1 },
            { "curse", 2 },
            { "repository", 3 },
            { "bugtracker", 4 },
            { "ci", 5 },
            { "license", 6 },
            { "manual", 7 }
        };

        public string Name { get { return "property_sort"; } }

        public Metadata Transform(Metadata metadata)
        {
            var json = metadata.Json();
            var sortedJson = new JObject();

            Log.InfoFormat("Executing property sort transformation");
            Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

            var sortedPropertyNames = json
                .Properties()
                .Select(i => i.Name)
                .OrderBy(GetPropertySortOrder)
                .ThenBy(i => i);

            foreach (var propertyName in sortedPropertyNames)
            {
                sortedJson[propertyName] = json[propertyName];
            }

            var resources = json["resources"] as JObject;
            if (resources != null)
            {
                var sortedResourcePropertyNames = resources
                    .Properties()
                    .Select(i => i.Name)
                    .OrderBy(GetResourcePropertySortOrder)
                    .ThenBy(i => i);

                var sortedResources = new JObject();

                foreach (var resourceProprtyName in sortedResourcePropertyNames)
                {
                    sortedResources[resourceProprtyName] = resources[resourceProprtyName];
                }

                sortedJson["resources"] = sortedResources;
            }

            Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, sortedJson);

            return new Metadata(sortedJson);
        }

        private static double GetPropertySortOrder(string propertyName)
        {
            int sortOrder;
            return PropertySortOrder.TryGetValue(propertyName, out sortOrder) ?
                sortOrder :
                propertyName.StartsWith("x_") ?
                    DefaultSortOrder + 1 :
                    DefaultSortOrder;
        }

        private static double GetResourcePropertySortOrder(string propertyName)
        {
            int sortOrder;
            return ResourcePropertySortOrder.TryGetValue(propertyName, out sortOrder) ?
                sortOrder :
                propertyName.StartsWith("x_") ?
                    DefaultSortOrder + 1 :
                    DefaultSortOrder;
        }
    }
}
