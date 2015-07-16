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
            { "provides", 16 },
            { "depends", 17 },
            { "recommends", 18 },
            { "suggests", 19 },
            { "supports", 20 },
            { "install", 21 },
            { "download", 22 },
            { "download_size", 23 },

            { "x_generated_by", int.MaxValue }
        };

        private static readonly Dictionary<string, int> ResourcePropertySortOrder = new Dictionary<string, int>
        {
            { "homepage", 0 },
            { "kerbalstuff", 1 },
            { "repository", 2 },
            { "bugtracker", 3 },
            { "ci", 4 },
            { "license", 5 },
            { "manual", 6 }
        };

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
