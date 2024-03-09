using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace CKAN.GUI
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ModuleLabelList
    {
        [JsonProperty("labels", NullValueHandling = NullValueHandling.Ignore)]
        public ModuleLabel[] Labels = Array.Empty<ModuleLabel>();

        public IEnumerable<ModuleLabel> LabelsFor(string instanceName)
            => Labels.Where(l => l.AppliesTo(instanceName));

        public static readonly string DefaultPath =
            Path.Combine(CKANPathUtils.AppDataPath, "labels.json");

        public static ModuleLabelList GetDefaultLabels()
            => new ModuleLabelList()
            {
                Labels = new ModuleLabel[]
                {
                    new ModuleLabel()
                    {
                        Name  = Properties.Resources.ModuleLabelListFavourites,
                        Color = Color.PaleGreen,
                    },
                    new ModuleLabel()
                    {
                        Name  = Properties.Resources.ModuleLabelListHidden,
                        Hide  = true,
                        Color = Color.PaleVioletRed,
                    },
                    new ModuleLabel()
                    {
                        Name        = Properties.Resources.ModuleLabelListHeld,
                        HoldVersion = true,
                        Color       = Color.FromArgb(255, 255, 176),
                    }
                }
            };

        public static ModuleLabelList Load(string path)
        {
            try
            {
                return JsonConvert.DeserializeObject<ModuleLabelList>(File.ReadAllText(path));
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // false < true
            Labels = Labels.OrderBy(l => l.InstanceName == null)
                           .ThenBy(l => l.InstanceName)
                           .ToArray();
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

        public IEnumerable<string> HeldIdentifiers(GameInstance inst)
            => LabelsFor(inst.Name).Where(l => l.HoldVersion)
                                   .SelectMany(l => l.IdentifiersFor(inst.game))
                                   .Distinct();
    }
}
