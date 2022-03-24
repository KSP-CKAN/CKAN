using log4net;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class TagsValidator : IValidator
    {
        public TagsValidator() { }

        public void Validate(Metadata metadata)
        {
            Log.Debug("Validating that metadata has tags");

            JObject json = metadata.Json();
            JArray tags = !json.ContainsKey("tags") ? null : (JArray)json["tags"];
            if (tags == null || tags.Count < 1)
            {
                Log.Warn("Tags not found, see https://github.com/KSP-CKAN/CKAN/wiki/Suggested-Tags");
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(TagsValidator));
    }
}
