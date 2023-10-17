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
        public static readonly JsonSchema schema =
            JsonSchema.FromJsonAsync(
                // ♫ Ooh-ooh StreamReader, I believe you can get me all the lines ♫
                // ♫ Ooh-ooh StreamReader, I believe we can reach the end of file ♫
                new StreamReader(
                    Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream(embeddedSchema)
                ).ReadToEnd()
            ).Result;

        private const string embeddedSchema = "CKAN.Core.CKAN.schema";
    }
}
