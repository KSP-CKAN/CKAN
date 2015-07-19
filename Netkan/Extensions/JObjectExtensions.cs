using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Extensions
{
    internal static class JObjectExtensions
    {
        /// <summary>
        /// Write a property to a <see cref="JObject"/> only if it does not already exist and the value is not null.
        /// </summary>
        /// <param name="jobject">The <see cref="JObject"/> to write to.</param>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="token">The value of the property to write if it does not exist.</param>
        public static void SafeAdd(this JObject jobject, string propertyName, JToken token)
        {
            if (token != null && token.ToObject<object>() != null)
            {
                jobject[propertyName] = jobject[propertyName] ?? token;
            }
        }
    }
}
