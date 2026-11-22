using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Utils;

namespace CKAN.NetKAN.Services
{
    internal sealed class CacheService : ICacheService
    {
        // Change this to invalidate all existing caches.
        public const int CacheVersion = 0;

        private static readonly ILog log = LogManager.GetLogger(typeof(CacheService));

        private readonly string _cacheDir;
        private readonly IHttpService _http;
        private readonly bool _overwrite;

        public CacheService(string cacheDir, IHttpService http, bool overwrite = false)
        {
            _cacheDir  = cacheDir;
            _http      = http;
            _overwrite = overwrite;
        }

        public IEnumerable<Metadata> ExecuteCached(Metadata metadata,
                                                   Type caller,
                                                   Func<string?, Metadata> action)
            => new Metadata[] { ExecuteCached<Metadata>(metadata, caller, action) };

        public IEnumerable<Metadata> ExecuteCached(Metadata metadata,
                                                   Type caller,
                                                   Func<string?, IEnumerable<Metadata>> action)
            => ExecuteCached(metadata, caller, contents => action(contents).ToList());

        public void ExecuteCached(Metadata metadata,
                                  Type caller,
                                  Action<string?> action)
        {
            ExecuteCached<byte>(metadata, caller, contents =>
            {
                action(contents);
                return 0;
            });
        }

        public T ExecuteCached<T>(Metadata metadata,
                                  Type caller,
                                  Func<string?, T> action)
        {
            if (metadata.Download is null)
            {
                return action(null);
            }

            var path  = CachePath<T>(metadata, new CacheKey(metadata, caller));
            var entry = _overwrite ? null : GetCacheEntry<T>(path);
            var httpOpts = entry is null
                ? null
                : new DownloadOptions(entry.ETag, entry.LastModified);

            var response = _http.DownloadModule(metadata, httpOpts);
            if (response is null)
            {
                return action(null);
            }

            if (entry is not null && response.NotModified)
            {
                log.Info($"Using cache entry for {metadata.Identifier}");
                foreach (var message in entry.Log)
                {
                    message.Replay(typeof(CacheService));
                }

                return entry.Item;
            }

            T                item;
            List<LogMessage> messages;

            using (var collector = new LogCollector())
            {
                item     = action(response.Path);
                messages = collector.Messages;
            }

            SaveCacheEntry(metadata, path, new CacheEntry<T>(item, messages)
            {
                ETag         = response.ETag,
                LastModified = response.LastModified,
            });

            return item;
        }

        private static CacheEntry<T>? GetCacheEntry<T>(string path)
        {
            try
            {
                return CacheEntry<T>.FromJson(ParseJson(File.ReadAllText(path)));
            }
            catch (FileNotFoundException) {}
            catch (Exception e)
            {
                // We can just count any exception as a cache miss and this is okay
                log.Debug($"Ignoring unreadable cache entry {path}: {e.Message}");
            }

            return null;
        }

        private void SaveCacheEntry<T>(Metadata metadata, string path, CacheEntry<T> entry)
        {
            try
            {
                Directory.CreateDirectory(_cacheDir);
                File.WriteAllText(path, entry.ToJson().ToString(Formatting.None));
            }
            catch (Exception e)
            {
                // If we fail to save the cache then that should not break
                // anything higher up. We should log the error though.
                log.Debug($"Failed to save cache entry for {metadata.Identifier}: {e}");
            }
        }

        private string CachePath<T>(Metadata metadata, CacheKey key)
            => Path.Combine(_cacheDir, $"netkan-{key.GetSha256Hash(typeof(T))}-{metadata.Identifier}.json");

          private static JObject ParseJson(string json)
        {
            using var reader = new JsonTextReader(new StringReader(json))
            {
                // We want the metadata to come back exactly as saved so avoid
                // doing any processing on date parsing.
                DateParseHandling = DateParseHandling.None,
            };
            return JObject.Load(reader);
        }

        private readonly struct CacheKey
        {
            public readonly Metadata Metadata;
            public readonly string Transformer;

            public CacheKey(Metadata metadata, string transformer)
            {
                Metadata = metadata;
                Transformer = transformer;
            }

            public CacheKey(Metadata metadata, Type transformer)
                : this(metadata, transformer.FullName!)
            { }

            public string GetSha256Hash(Type value)
            {
                var json = new JArray(CacheVersion,
                                      value.FullName!,
                                      Metadata.AllJson,
                                      Transformer)
                               .ToString(Formatting.None);
                using var sha = SHA256.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
                var sb   = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private sealed class CacheEntry<T>
        {
            public string?          ETag;
            public DateTime?        LastModified;
            public T                Item;
            public List<LogMessage> Log;

            public CacheEntry(T item, List<LogMessage> log)
            {
                Item = item;
                Log  = log;
            }

            public JObject ToJson()
                => new JObject
                   {
                       ["etag"]          = ETag,
                       ["last_modified"] = LastModified?.ToUniversalTime()
                                                        .ToString("o", CultureInfo.InvariantCulture),
                       ["item"]          = Item is null ? JValue.CreateNull()
                                                        : JToken.FromObject(Item, serializer),
                       ["log"]           = JToken.FromObject(Log, serializer),
                   };

            public static CacheEntry<T> FromJson(JObject json)
                => new CacheEntry<T>(json["item"]!.ToObject<T>(serializer)!,
                                     json["log"]!.ToObject<List<LogMessage>>(serializer)!)
                   {
                       ETag         = (string?)json["etag"],
                       LastModified = (string?)json["last_modified"] is string lm
                                          ? DateTime.Parse(lm, CultureInfo.InvariantCulture,
                                                           DateTimeStyles.RoundtripKind)
                                          : null,
                   };
        }

        private static readonly JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None,
            Converters        = { new MetadataConverter() },
        });

        /// <summary>
        /// Metadata only serializes its typed fields, so store the full JSON instead
        /// </summary>
        private sealed class MetadataConverter : JsonConverter<Metadata>
        {
            public override void WriteJson(JsonWriter writer, Metadata? value, JsonSerializer serializer)
            {
                if (value is null)
                {
                    writer.WriteNull();
                }
                else
                {
                    value.AllJson.WriteTo(writer);
                }
            }

            public override Metadata? ReadJson(JsonReader     reader,
                                               Type           objectType,
                                               Metadata?      existingValue,
                                               bool           hasExistingValue,
                                               JsonSerializer serializer)
                => reader.TokenType == JsonToken.Null
                       ? null
                       : new Metadata(JObject.Load(reader));
        }
    }
}
