using System;
using System.Collections;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CKAN.Games;

namespace CKAN
{
    /// <summary>
    /// A property converter for making an old property game-specific.
    /// Turns a String or Array value:
    ///
    ///     "myProperty": "a value",
    ///     "myOtherProperty": [ "another value" ],
    ///
    /// into a Dictionary with the game names as keys and the original
    /// value as each value:
    ///
    ///     "myProperty": {
    ///         "KSP": "a value",
    ///         "KSP2": "a value"
    ///     },
    ///     "myOtherProperty": {
    ///         "KSP": [ "another value" ],
    ///         "KSP2": [ "another value" ]
    ///     },
    ///
    /// NOTE: Do NOT use with Object values because they can't
    ///       be distinguished from an already converted value, and will
    ///       just be deserialized as-is into your Dictionary!
    ///
    /// If the value is an empty array:
    ///
    ///     "myProperty": [],
    ///
    /// the Dictionary is left empty rather than creating multiple keys
    /// with empty values:
    ///
    ///     "myProperty": {},
    /// </summary>
    public class JsonToGamesDictionaryConverter : JsonConverter
    {
        /// <summary>
        /// Turn a tree of JSON tokens into a dictionary
        /// </summary>
        /// <param name="reader">Object that provides tokens to be translated</param>
        /// <param name="objectType">The output type to be populated</param>
        /// <param name="existingValue">Not used</param>
        /// <param name="serializer">Generates output objects from tokens</param>
        /// <returns>Dictionary of type matching the property where this converter was used, containing game-specific keys and values</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.Object)
            {
                return token.ToObject(objectType);
            }
            var valueType = objectType.GetGenericArguments()[1];
            var obj = (IDictionary)Activator.CreateInstance(objectType);
            if (!IsTokenEmpty(token))
            {
                foreach (var gameName in KnownGames.AllGameShortNames())
                {
                    // Make a new copy of the value for each game
                    obj.Add(gameName, token.ToObject(valueType));
                }
            }
            return obj;
        }

        /// <summary>
        /// We don't want to make any changes during serialization
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// We don't want to make any changes during serialization
        /// </summary>
        /// <param name="writer">The object writing JSON to disk</param>
        /// <param name="value">A value to be written for this class</param>
        /// <param name="serializer">Generates output objects from tokens</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// We *only* want to be triggered for types that have explicitly
        /// set an attribute in their class saying they can be converted.
        /// By returning false here, we declare we're not interested in participating
        /// in any other conversions.
        /// </summary>
        /// <returns>
        /// false
        /// </returns>
        public override bool CanConvert(Type object_type) => false;

        private static bool IsTokenEmpty(JToken token)
            => token.Type == JTokenType.Null
                || (token.Type == JTokenType.Array && !token.HasValues);
    }
}
