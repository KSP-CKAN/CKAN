using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CKAN
{
    public class ModuleTagList
    {
        [JsonIgnore]
        public Dictionary<string, ModuleTag> Tags = new Dictionary<string, ModuleTag>();

        [JsonIgnore]
        public HashSet<string> Untagged = new HashSet<string>();

        [JsonProperty("hidden_tags")]
        public HashSet<string> HiddenTags = new HashSet<string>();

        public static readonly string DefaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CKAN",
            "tags.json"
        );

        public void BuildTagIndexFor(AvailableModule am)
        {
            bool tagged = false;
            foreach (CkanModule m in am.AllAvailable())
            {
                if (m.Tags != null)
                {
                    tagged = true;
                    foreach (string tagName in m.Tags)
                    {
                        ModuleTag tag = null;
                        if (Tags.TryGetValue(tagName, out tag))
                            tag.Add(m.identifier);
                        else
                            Tags.Add(tagName, new ModuleTag()
                            {
                                Name = tagName,
                                Visible = !HiddenTags.Contains(tagName),
                                ModuleIdentifiers = new HashSet<string>() { m.identifier },
                            });
                    }
                }
            }
            if (!tagged)
            {
                Untagged.Add(am.AllAvailable().First().identifier);
            }
        }

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
