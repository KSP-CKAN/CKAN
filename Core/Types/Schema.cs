using System.IO;
using System.Reflection;

using NJsonSchema;

namespace CKAN
{
    /// <summary>
    /// A static container for a JsonSchema object representing CKAN.schema
    /// </summary>
    public static class CKANSchema
    {
        /// <summary>
        /// Parsed representation of our embedded CKAN.schema resource
        /// </summary>
        public static readonly JsonSchema? schema =
            Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(embeddedSchema)
                is Stream s
                ? JsonSchema.FromJsonAsync(
                    // ♫ Ooh-ooh StreamReader, I believe you can get me all the lines ♫
                    // ♫ Ooh-ooh StreamReader, I believe we can reach the end of file ♫
                    new StreamReader(s).ReadToEnd()).Result
                : null;

        private const string embeddedSchema = "CKAN.Core.CKAN.schema";
    }
}
