using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CKAN
{
    public class ModuleTagList
    {
        [JsonProperty("hidden_tags")]
        public HashSet<string> HiddenTags = new HashSet<string>();

        public static readonly string DefaultPath =
            Path.Combine(CKANPathUtils.AppDataPath, "tags.json");

        public static ModuleTagList Load(string path)
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

        public bool Save(string path)
        {
            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
