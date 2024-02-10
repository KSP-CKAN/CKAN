using Newtonsoft.Json;
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
                    if (metadata.SpecVersion < v1p2 && relName.Equals("supports"))
                    {
                        throw new Kraken("spec_version v1.2+ required for 'supports'");
                    }
                    foreach (var relToken in json[relName].Children())
                    {
                        switch (relToken)
                        {
                            case JObject rel:
                                if (rel.ContainsKey("any_of"))
                                {
                                    if (metadata.SpecVersion < v1p26)
                                    {
                                        throw new Kraken("spec_version v1.26+ required for 'any_of'");
                                    }
                                    foreach (string forbiddenPropertyName in AnyOfRelationshipDescriptor.ForbiddenPropertyNames)
                                    {
                                        if (rel.ContainsKey(forbiddenPropertyName))
                                        {
                                            throw new Kraken($"{forbiddenPropertyName} is not valid in the same relationship as 'any_of'");
                                        }
                                    }
                                    if (rel.ContainsKey("choice_help_text") && metadata.SpecVersion < v1p31)
                                    {
                                        throw new Kraken("spec_version v1.31+ required for choice_help_text in same relationship as 'any_of'");
                                    }
                                    foreach (var optToken in rel["any_of"].Children())
                                    {
                                        switch (optToken)
                                        {
                                            case JObject opt:
                                                string name = (string)opt["name"];
                                                if (!Identifier.ValidIdentifierPattern.IsMatch(name))
                                                {
                                                    throw new Kraken($"{name} in {relName} 'any_of' is not a valid CKAN identifier");
                                                }
                                                break;
                                            default:
                                                throw new Kraken($"{relName} 'any_of' elements should be {JTokenType.Object}, found {optToken.Type}: {optToken.ToString(Formatting.None)}");
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
                                    if (rel.ContainsKey("version_min"))
                                    {
                                        throw new Kraken($"'version_min' found in relationship, the correct form is 'min_version'");
                                    }
                                    if (rel.ContainsKey("version_max"))
                                    {
                                        throw new Kraken($"'version_max' found in relationship, the correct form is 'max_version'");
                                    }
                                }
                                break;

                            default:
                                throw new Kraken($"{relName} elements should be {JTokenType.Object}, found {relToken.Type}: {relToken.ToString(Formatting.None)}");
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
        private static readonly ModuleVersion v1p2  = new ModuleVersion("v1.2");
        private static readonly ModuleVersion v1p26 = new ModuleVersion("v1.26");
        private static readonly ModuleVersion v1p31 = new ModuleVersion("v1.31");
    }
}
