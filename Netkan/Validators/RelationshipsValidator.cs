using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using CKAN.Versioning;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class RelationshipsValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json = metadata.Json();
            foreach (string relName in relProps)
            {
                if (json.ContainsKey(relName))
                {
                    foreach (JObject rel in json[relName])
                    {
                        if (rel.ContainsKey("any_of"))
                        {
                            if (metadata.SpecVersion < v1p26)
                            {
                                throw new Kraken("spec_version v1.26+ required for 'any_of'");
                            }
                            foreach (JObject opt in rel["any_of"])
                            {
                                string name = (string)opt["name"];
                                if (!Identifier.ValidIdentifierPattern.IsMatch(name))
                                {
                                    throw new Kraken($"{name} in {relName} any_of is not a valid CKAN identifier");
                                }
                            }
                        }
                        else
                        {
                            string name = (string)rel["name"];
                            if (!Identifier.ValidIdentifierPattern.IsMatch(name))
                            {
                                throw new Kraken($"{name} in {relName} is not a valid CKAN identifier");
                            }
                        }
                    }
                }
            }

        }

        private static readonly string[] relProps = new string[]
        {
            "depends",
            "recommends",
            "suggests",
            "conflicts",
            "supports"
        };
        private static readonly ModuleVersion v1p26 = new ModuleVersion("v1.26");
    }
}
