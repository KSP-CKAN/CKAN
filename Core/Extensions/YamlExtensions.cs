using System.IO;
using System.Linq;

using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using Newtonsoft.Json.Linq;

namespace CKAN.Extensions
{
    public static class YamlExtensions
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
                                   .OfType<YamlMappingNode>()
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
                if ((string?)kvp.Key is string k)
                {
                    switch (kvp.Value)
                    {
                        case YamlMappingNode obj:
                            jobj.Add(k, obj.ToJObject());
                            break;
                        case YamlSequenceNode array:
                            jobj.Add(k, array.ToJarray());
                            break;
                        case YamlScalarNode scalar:
                            jobj.Add(k, scalar.ToJValue());
                            break;
                    }
                }
            }
            return jobj;
        }

        private static JArray ToJarray(this YamlSequenceNode yaml)
        {
            var jarr = new JArray();
            foreach (var elt in yaml)
            {
                switch (elt)
                {
                    case YamlMappingNode obj:
                        jarr.Add(obj.ToJObject());
                        break;
                    case YamlSequenceNode array:
                        jarr.Add(array.ToJarray());
                        break;
                    case YamlScalarNode scalar:
                        jarr.Add(scalar.ToJValue());
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
