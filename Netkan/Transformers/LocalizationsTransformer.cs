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
            JObject    json = metadata.Json();
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            ZipFile    zip  = new ZipFile(_http.DownloadPackage(
                metadata.Download,
                metadata.Identifier,
                metadata.RemoteTimestamp
            ));

            // Extract the locale names from the ZIP's cfg files
            var locales = _moduleService.GetConfigFiles(mod, zip)
                .Select(cfg => new StreamReader(zip.GetInputStream(cfg.source)).ReadToEnd())
                .SelectMany(contents => localizationRegex.Matches(contents).Cast<Match>()
                    .Select(m => m.Groups["contents"].Value))
                .SelectMany(contents => localeRegex.Matches(contents).Cast<Match>()
                    .Select(m => m.Groups["locale"].Value));

            if (locales.Any())
            {
                json["localizations"] = new JArray(locales);
                yield return new Metadata(json);
            }
            else
            {
                yield return metadata;
            }
        }

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;

        private static readonly Regex localizationRegex = new Regex(
            @"\bLocalization\b.*?{(?<contents>.*?([-a-z]+.*?{.*?}.*?)*?)}",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
        );
        private static readonly Regex localeRegex = new Regex(
            @"(?<locale>[-a-z]+).*?{.*?}",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
        );
    }
}
