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
using CKAN.NetKAN.Transformers;
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

        /// <param name="overwrite">Ignore saved entries (but still save new ones),
        /// same as the download cache's overwrite flag</param>
        public CacheService(string cacheDir, IHttpService http, bool overwrite = false)
        {
            _cacheDir = cacheDir;
            _http = http;
            _overwrite = overwrite;
        }

        public IEnumerable<Metadata> ExecuteCached(Metadata metadata,
                                                   TransformOptions options,
                                                   Type caller,
                                                   Func<string?, Metadata> action)
        {
            return ExecuteCached(metadata,
                                 options,
                                 caller,
                                 contents => new Metadata[]{action(contents)});
        }

        public IEnumerable<Metadata> ExecuteCached(Metadata metadata,
                                                   TransformOptions options,
                                                   Type caller,
                                                   Func<string?, IEnumerable<Metadata>> action)
        {
            if (metadata.Download is null)
            {
                throw new InvalidOperationException("Called ExecuteWithModel but metadata does not have a download URL");
            }

            var key = new CacheKey(metadata, options, caller);
            var entry = _overwrite ? null : GetCacheEntry(metadata, key);
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
                log.Debug($"Using cache entry for {metadata.Identifier}");
                foreach (var warning in entry.Warnings)
                {
                    log.Warn(warning);
                }

                return entry.Metadata;
            }

            List<Metadata> items;
            List<string>   warnings;

            using (var collector = new WarningCollector())
            {
                items = action(response.Path).ToList();
                warnings = collector.Warnings;
            }

            entry = new CacheEntry(items, warnings)
            {
                ETag = response.ETag,
                LastModified = response.LastModified
            };

            SaveCacheEntry(metadata, key, entry);

            return items;
        }

        private CacheEntry? GetCacheEntry(Metadata metadata, CacheKey key)
        {
            var path = CachePath(metadata, key);

            try
            {
                return CacheEntry.FromJson(ParseJson(File.ReadAllText(path)));
            }
            catch (FileNotFoundException) {}
            catch (Exception e)
            {
                // We can just count any exception as a cache miss and this is okay
                log.Debug($"Ignoring unreadable cache entry {path}: {e.Message}");
            }

            return null;
        }

        private void SaveCacheEntry(Metadata metadata, CacheKey key, CacheEntry entry)
        {
            var path = CachePath(metadata, key);

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

        private string CachePath(Metadata metadata, CacheKey key)
            => Path.Combine(_cacheDir, $"netkan-{key.GetSha256Hash()}-{metadata.Identifier}.json");

        /// <summary>
        /// Parse without turning date-like strings into DateTime tokens, so the
        /// metadata comes back exactly as it was saved
        /// </summary>
        private static JObject ParseJson(string json)
        {
            using var reader = new JsonTextReader(new StringReader(json))
            {
                DateParseHandling = DateParseHandling.None,
            };
            return JObject.Load(reader);
        }

        private readonly struct CacheKey
        {
            public readonly Metadata Metadata;
            public readonly TransformOptions Options;
            public readonly string Transformer;

            public CacheKey(Metadata metadata,
                            TransformOptions options,
                            string transformer)
            {
                Metadata = metadata;
                Options = options;
                Transformer = transformer;
            }

            public CacheKey(Metadata metadata,
                            TransformOptions options,
                            Type transformer)
                : this(metadata, options, transformer.FullName!)
            { }

            public string GetSha256Hash()
            {
                var json = new JArray(CacheVersion,
                                      Metadata.AllJson,
                                      JToken.FromObject(Options),
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

        private sealed class CacheEntry
        {
            public string? ETag;
            public DateTime? LastModified;
            public List<Metadata> Metadata;
            public List<string> Warnings;

            public CacheEntry(List<Metadata> metadata, List<string> warnings)
            {
                Metadata = metadata;
                Warnings = warnings;
            }

            public JObject ToJson()
                => new JObject
                   {
                       ["etag"]          = ETag,
                       ["last_modified"] = LastModified?.ToUniversalTime()
                                                        .ToString("o", CultureInfo.InvariantCulture),
                       ["metadata"]      = new JArray(Metadata.Select(m => m.AllJson)),
                       ["warnings"]      = new JArray(Warnings),
                   };

            public static CacheEntry FromJson(JObject json)
                => new CacheEntry(json["metadata"]!.Children<JObject>()
                                                   .Select(j => new Metadata(j))
                                                   .ToList(),
                                  json["warnings"]!.Values<string>()
                                                   .OfType<string>()
                                                   .ToList())
                   {
                       ETag         = (string?)json["etag"],
                       LastModified = (string?)json["last_modified"] is string lm
                                          ? DateTime.Parse(lm, CultureInfo.InvariantCulture,
                                                           DateTimeStyles.RoundtripKind)
                                          : null,
                   };
        }
    }
}
