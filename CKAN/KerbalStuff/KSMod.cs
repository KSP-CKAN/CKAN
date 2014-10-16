namespace CKAN.KerbalStuff {

    using Newtonsoft.Json;
    using System.Net;
    using log4net;

    public class KSMod {
        private static readonly ILog log = LogManager.GetLogger(typeof(KSMod));

        // These get filled in from JSON deserialisation.
        public KSVersion[] versions;
        public string name;
        public string license;

        public override string ToString () {
            return string.Format ("{0}", name);
        }

    }
}

