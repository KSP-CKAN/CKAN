namespace CKAN.KerbalStuff {

    using Newtonsoft.Json;

    public class KSMod {
        public dynamic[] versions;
        public string name;
        public string license;

        public static KSMod FromString(string json) {
            return JsonConvert.DeserializeObject<KSMod> (json);
        }
    }
}

