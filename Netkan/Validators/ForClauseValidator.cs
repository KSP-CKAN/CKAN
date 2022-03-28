using System.Linq;

using Newtonsoft.Json.Linq;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using CKAN.Games;

namespace CKAN.NetKAN.Validators
{
    internal sealed class ForClauseValidator : IValidator
    {
        public ForClauseValidator(IHttpService http, IModuleService moduleService, IConfigParser parser)
        {
            _http          = http;
            _moduleService = moduleService;
            _parser        = parser;
        }

        public void Validate(Metadata metadata)
        {
            Log.Info("Validating that :FOR[] clauses specify the right mod");

            JObject    json = metadata.Json();
            CkanModule mod  = CkanModule.FromJson(json.ToString());
            if (!mod.IsDLC)
            {
                var package = _http.DownloadModule(metadata);
                if (!string.IsNullOrEmpty(package))
                {
                    ZipFile      zip  = new ZipFile(package);
                    GameInstance inst = new GameInstance(new KerbalSpaceProgram(), "/", "dummy", new NullUser());

                    // Check for :FOR[identifier] in .cfg files
                    var mismatchedIdentifiers = KerbalSpaceProgram
                        .IdentifiersFromConfigNodes(
                            _parser.GetConfigNodes(mod, zip, inst)
                                   .SelectMany(kvp => kvp.Value))
                        .Where(ident => ident != mod.identifier
                                        && Identifier.ValidIdentifierPattern.IsMatch(ident))
                        .OrderBy(s => s)
                        .ToArray();
                    if (mismatchedIdentifiers.Any())
                    {
                        Log.WarnFormat("Found :FOR[] clauses with the wrong identifiers: {0}",
                                       string.Join(", ", mismatchedIdentifiers));
                    }
                }
            }
        }

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;
        private readonly IConfigParser  _parser;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ForClauseValidator));
    }
}
