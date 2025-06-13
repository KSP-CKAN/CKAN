using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

using CKAN.IO;
using CKAN.Extensions;

namespace CKAN
{
    public class ModuleTagList
    {
        [JsonProperty("hidden_tags")]
        public HashSet<string> HiddenTags = new HashSet<string>();

        public static readonly string DefaultPath =
            Path.Combine(CKANPathUtils.AppDataPath, "tags.json");

        public static ModuleTagList? Load(string path)
        {
            try
            {
                return JsonConvert.DeserializeObject<ModuleTagList>(File.ReadAllText(path));
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public static readonly ModuleTagList ModuleTags = Load(DefaultPath) ?? new ModuleTagList();

        public bool Save(string path)
        {
            try
            {
                JsonConvert.SerializeObject(this, Formatting.Indented)
                           .WriteThroughTo(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
