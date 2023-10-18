using System.IO;
using System.Linq;

using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Extensions
{
    internal static class YamlExtensions
    {
        public static YamlMappingNode[] Parse(string input)
        {
            return Parse(new StringReader(input));
        }

        public static YamlMappingNode[] Parse(TextReader input)
        {
            var stream = new YamlStream();
            stream.Load(input);
            return stream.Documents.Select(doc => doc?.RootNode as YamlMappingNode)
                                   .ToArray();
        }

        /// <summary>
        /// Convert a YAML object to a JSON object
        /// </summary>
        /// <param name="yaml">The input object</param>
        /// <returns>
        /// A JObject representation of the input data
        /// </returns>
        public static JObject ToJObject(this YamlMappingNode yaml)
        {
            var jobj = new JObject();
            foreach (var kvp in yaml)
            {
                switch (kvp.Value.NodeType)
                {
                    case YamlNodeType.Mapping:
                        jobj.Add((string)kvp.Key, (kvp.Value as YamlMappingNode).ToJObject());
                        break;
                    case YamlNodeType.Sequence:
                        jobj.Add((string)kvp.Key, (kvp.Value as YamlSequenceNode).ToJarray());
                        break;
                    case YamlNodeType.Scalar:
                        jobj.Add((string)kvp.Key, (kvp.Value as YamlScalarNode).ToJValue());
                        break;
                }
            }
            return jobj;
        }

        private static JArray ToJarray(this YamlSequenceNode yaml)
        {
            var jarr = new JArray();
            foreach (var elt in yaml)
            {
                switch (elt.NodeType)
                {
                    case YamlNodeType.Mapping:
                        jarr.Add((elt as YamlMappingNode).ToJObject());
                        break;
                    case YamlNodeType.Sequence:
                        jarr.Add((elt as YamlSequenceNode).ToJarray());
                        break;
                    case YamlNodeType.Scalar:
                        jarr.Add((elt as YamlScalarNode).ToJValue());
                        break;
                }
            }
            return jarr;
        }

        private static JValue ToJValue(this YamlScalarNode yaml)
        {
            switch (yaml.Value)
            {
                case "null":  return JValue.CreateNull();
                case "true":  return new JValue(true);
                case "false": return new JValue(false);
                // Convert unquoted integers to int type
                default:      return yaml.Style == ScalarStyle.Plain
                                     && int.TryParse(yaml.Value, out int intVal)
                                         ? new JValue(intVal)
                                         : new JValue(yaml.Value);
            }
        }
    }
}
