using System;
﻿using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using log4net;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Extensions;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class LocalizationsTransformer : ITransformer
    {

        /// <summary>
        /// Initialize the transformer for locales
        /// </summary>
        /// <param name="http">HTTP service</param>
        /// <param name="moduleService">Module service</param>
        public LocalizationsTransformer(IHttpService http, IModuleService moduleService)
        {
            _http          = http;
            _moduleService = moduleService;
        }

        /// <summary>
        /// Name of this transformer
        /// </summary>
        public string Name { get { return "localizations"; } }

        /// <summary>
        /// Apply the locale transformation to the metadata
        /// </summary>
        /// <param name="metadata">Data about the module</param>
        /// <returns>
        /// Updated metadata with the `locales` property set
        /// </returns>
        public IEnumerable<Metadata> Transform(Metadata metadata)
        {
            JObject json = metadata.Json();
            if (json.ContainsKey(localizationsProperty))
            {
                log.Debug("Localizations property already set, skipping");
                // Already set, don't override (skips a bunch of file processing)
                yield return metadata;
            }
            else
            {
                CkanModule mod  = CkanModule.FromJson(json.ToString());
                ZipFile    zip  = new ZipFile(_http.DownloadPackage(
                    metadata.Download,
                    metadata.Identifier,
                    metadata.RemoteTimestamp
                ));
    
                log.Debug("Extracting locales");
                // Extract the locale names from the ZIP's cfg files
                var locales = _moduleService.GetConfigFiles(mod, zip)
                    .Select(cfg => new StreamReader(zip.GetInputStream(cfg.source)).ReadToEnd())
                    .SelectMany(contents => localizationRegex.Matches(contents).Cast<Match>()
                        .Select(m => m.Groups["contents"].Value))
                    .SelectMany(contents => localeRegex.Matches(contents).Cast<Match>()
                        .Where(m => m.Groups["contents"].Value.Contains("="))
                        .Select(m => m.Groups["locale"].Value))
                    .Distinct();
                log.Debug("Locales extracted");
    
                if (locales.Any())
                {
                    json.SafeAdd(localizationsProperty, new JArray(locales));
                    log.Debug("Localizations property set");
                    yield return new Metadata(json);
                }
                else
                {
                    log.Debug("No localizations found");
                    yield return metadata;
                }
            }
        }

        private const string localizationsProperty = "localizations";

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;

        private static readonly ILog log = LogManager.GetLogger(typeof(LocalizationsTransformer));

        private static readonly Regex localizationRegex = new Regex(
            @"^\s*Localization\b.*?{(?<contents>.*?([-a-zA-Z]+.*?{.*?}.*?)*?)}",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
        );
        private static readonly Regex localeRegex = new Regex(
            @"^\s*(?<locale>[-a-zA-Z]+).*?{(?<contents>.*?)}",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
        );
    }
}
