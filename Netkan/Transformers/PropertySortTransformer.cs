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

        private static readonly Dictionary<string, int> PropertySortOrder = new string[]
        {
            "spec_version",
            "comment",
            "identifier",
            "name",
            "abstract",
            "description",
            "author",
            "$kref",
            "$vref",
            "version",
            "ksp_version",
            "ksp_version_min",
            "ksp_version_max",
            "ksp_version_strict",
            "license",
            "release_status",
            "resources",
            "tags",
            "localizations",
            "provides",
            "depends",
            "recommends",
            "suggests",
            "supports",
            "conflicts",
            "install",
            "download",
            "download_size",
            "download_hash",
            "download_content_type",
            "install_size",
            "release_date",
        }
            .Select((str, index) => new {index, str})
            .Concat(new[] { new { index = int.MaxValue, str = "x_generated_by"} })
            .ToDictionary(t => t.str, t=> t.index);

        private static readonly Dictionary<string, int> ResourcePropertySortOrder = new string[]
        {
            "homepage",
            "spacedock",
            "curse",
            "repository",
            "bugtracker",
            "ci",
            "license",
            "manual",
        }
            .Select((str, index) => new {index, str})
            .ToDictionary(t => t.str, t=> t.index);

        public string Name => "property_sort";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            var json = metadata.Json();
            var sortedJson = new JObject();

            Log.Debug("Executing property sort transformation");
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

            if (json["resources"] is JObject resources)
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

            yield return new Metadata(sortedJson);
        }

        public static Metadata SortProperties(Metadata metadata)
            => new PropertySortTransformer().Transform(metadata, null)
                                            .First();

        private static double GetPropertySortOrder(string propertyName)
            => PropertySortOrder.TryGetValue(propertyName, out int sortOrder)
                ? sortOrder
                : propertyName.StartsWith("x_")
                    ? DefaultSortOrder + 1
                    : DefaultSortOrder;

        private static double GetResourcePropertySortOrder(string propertyName)
            => ResourcePropertySortOrder.TryGetValue(propertyName, out int sortOrder)
                ? sortOrder
                : propertyName.StartsWith("x_")
                    ? DefaultSortOrder + 1
                    : DefaultSortOrder;
    }
}
