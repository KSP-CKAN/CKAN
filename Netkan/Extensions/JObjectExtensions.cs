using System;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Extensions
{
    internal static class JObjectExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JObjectExtensions));

        /// <summary>
        /// Write a property to a <see cref="JObject"/> only if it does not already exist and the value is not null.
        /// </summary>
        /// <param name="jobject">The <see cref="JObject"/> to write to.</param>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="token">The value of the property to write if it does not exist.</param>
        public static void SafeAdd(this JObject jobject, string propertyName, JToken token)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                Log.Warn("Asked to set a property named null on a JSON object!");
                return;
            }

            if (token != null && (token.HasValues || token.ToObject(typeof(object)) != null))
            {
                jobject[propertyName] = jobject[propertyName] ?? token;
            }
        }

        /// <summary>
        /// Write a property to a <see cref="JObject"/> only if it does not already exist and the value is not null.
        /// The value is generated on-demand by calling tokenCallback, only when we need it.
        /// </summary>
        /// <param name="jobject">The <see cref="JObject"/> to write to</param>
        /// <param name="propertyName">The name of the property to write</param>
        /// <param name="tokenCallback">Function to generate value of the property to write if it does not exist</param>
        public static void SafeAdd(this JObject jobject, string propertyName, Func<JToken> tokenCallback)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                Log.Warn("Asked to set a property named null on a JSON object!");
                return;
            }
            if (jobject[propertyName] == null)
            {
                jobject.SafeAdd(propertyName, tokenCallback());
            }
        }

        /// <summary>
        /// Merge an object's properties into one of our child objects
        /// E.g., the "resources" object should accumulate values from all levels
        /// </summary>
        /// <param name="jobject">The object to write to</param>
        /// <param name="propertyName">The name of the property to write to</param>
        /// <param name="token">The object containing properties to merge</param>
        /// <returns>
        /// Returns
        /// </returns>
        public static void SafeMerge(this JObject jobject, string propertyName, JToken token)
        {
            JObject srcObj = token as JObject;
            // No need to do anything if source object is null or empty
            if (srcObj?.Properties().Any() ?? false)
            {
                if (!jobject.ContainsKey(propertyName))
                {
                    jobject[propertyName] = new JObject();
                }
                if (jobject[propertyName] is JObject targetJson)
                {
                    foreach (JProperty property in srcObj.Properties())
                    {
                        targetJson.SafeAdd(property.Name, property.Value);
                    }
                }
            }
        }

    }
}
