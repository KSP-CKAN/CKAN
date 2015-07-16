using System.Collections.Generic;
using System.Linq;
using CKAN.NetKAN.Model;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class PropertySortTransformer : ITransformer
    {
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
            { "download", 16 },
            { "download_size", 17 },
            { "provides", 18 },
            { "depends", 19 },
            { "recommends", 20 },
            { "suggests", 21 },
            { "supports", 22 },
            { "install", 23 },

            { "x_generated_by", int.MaxValue }
        };

        public Metadata Transform(Metadata metadata)
        {
            var json = metadata.Json();
            var sortedJson = new JObject();

            var sortedPropertyNames = json
                .Properties()
                .Select(i => i.Name)
                .OrderBy(GetPropertySortOrder)
                .ThenBy(i => i);

            foreach (var propertyName in sortedPropertyNames)
            {
                sortedJson[propertyName] = json[propertyName];
            }

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
    }
}
