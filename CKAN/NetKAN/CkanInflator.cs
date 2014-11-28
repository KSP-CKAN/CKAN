using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN
{
    // Assurances that given the metadata, the mod version, and the filename,
    // we will *modify* the metadata with whatever we've picked up from our
    // API.

    public abstract class CkanInflator
    {
        public abstract void InflateMetadata(JObject metadata, string filename, object context);

        // Inflate will add a value, but only if that key is not
        // already filled with something else.

        public static void Inflate(JObject metadata, string key, string value)
        {
            if (metadata[key] == null)
            {
                metadata[key] = value;
            }
        }

        public static void Inflate(JObject metadata, string key, long value)
        {
            if (metadata[key] == null)
            {
                metadata[key] = value;
            }
        }

    }    
}

