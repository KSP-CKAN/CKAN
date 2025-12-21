using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;

using log4net;

using CKAN.Games;
using CKAN.Versioning;

namespace CKAN
{
    using modRelList = List<Tuple<CkanModule, RelationshipDescriptor, CkanModule?>>;

    /// <summary>
    /// Our application exceptions are called Krakens.
    /// </summary>
    public class Kraken : Exception
    {
        public Kraken(string?    reason         = null,
                      Exception? innerException = null)
            : base(reason, innerException)
        {
        }
    }

    public class FileNotFoundKraken : Kraken
    {
        public FileNotFoundKraken(string?    file,
                                  string?    reason         = null,
                                  Exception? innerException = null)
            : base(reason, innerException)
        {
            this.file = file;
        }

        public readonly string? file;
    }

    public class DirectoryNotFoundKraken : Kraken
    {
        public DirectoryNotFoundKraken(string     directory,
                                       string?    reason         = null,
                                       Exception? innerException = null)
            : base(reason, innerException)
        {
            this.directory = directory;
        }

        public readonly string directory;
    }

    public class NotEnoughSpaceKraken : Kraken
    {
        public NotEnoughSpaceKraken(string        description,
                                    DirectoryInfo destination,
                                    long          bytesFree,
                                    long          bytesToStore)
            : base(string.Format(Properties.Resources.KrakenNotEnoughSpace,
                                 description, destination,
                                 CkanModule.FmtSize(bytesFree),
                                 CkanModule.FmtSize(bytesToStore)))
        {
            this.destination  = destination;
            this.bytesFree    = bytesFree;
            this.bytesToStore = bytesToStore;
        }

        public readonly DirectoryInfo destination;
        public readonly long          bytesFree;
        public readonly long          bytesToStore;
    }

    /// <summary>
    /// A bad install location was provided.
    /// Valid locations are GameData, GameRoot, Ships, etc.
    /// </summary>
    public class BadInstallLocationKraken : Kraken
    {
        public BadInstallLocationKraken(string?    reason         = null,
                                        Exception? innerException = null)
            : base(reason, innerException)
        {
        }
    }

    public class ModuleNotFoundKraken : Kraken
    {
        public ModuleNotFoundKraken(string     identifier,
                                    string?    version        = null,
                                    string?    reason         = null,
                                    Exception? innerException = null)
            : base(reason
                   ?? (version == null
                       ? string.Format(Properties.Resources.KrakenDependencyModuleNotFound,
                                       identifier, "")
                       : string.Format(Properties.Resources.KrakenDependencyNotSatisfied,
                                       identifier, version)),
                   innerException)
        {
            this.identifier = identifier;
            this.version    = version;
        }

        public readonly string  identifier;
        public readonly string? version;
    }

    /// <summary>
    /// Exception describing a missing dependency
    /// </summary>
    public class DependenciesNotSatisfiedKraken : Kraken
    {
        /// <summary>
        /// Initialize the exception representing failed dependency resolution
        /// </summary>
        /// <param name="unsatisfied">List of chain of relationships with last one unsatisfied</param>
        /// <param name="registry">Registry to use for formatting</param>
        /// <param name="game">Game to use for formatting</param>
        /// <param name="resolved">Resolved relationships tree</param>
        /// <param name="innerException">Originating exception parameter for base class</param>
        public DependenciesNotSatisfiedKraken(IReadOnlyCollection<ResolvedRelationship[]> unsatisfied,
                                              IRegistryQuerier                            registry,
                                              IGame                                       game,
                                              ResolvedRelationshipsTree                   resolved,
                                              Exception?                                  innerException = null)
            : base(string.Join(Environment.NewLine + Environment.NewLine,
                               unsatisfied.GroupBy(rrs => rrs.Last().relationship)
                                          .OrderByDescending(grp => grp.Count())
                                          .ThenBy(grp => grp.Key.ToString())
                                          .Select(grp => string.Format(Properties.Resources.KrakenMissingDependency,
                                                                       string.Join("; ",
                                                                                   grp.DistinctBy(rrs => rrs.Last().source)
                                                                                      .Select(FormatDependsChain)),
                                                                       grp.Key.ToStringWithCompat(registry, game)))),
                   innerException)
        {
            this.unsatisfied = unsatisfied;
            log.DebugFormat("Resolved relationships tree:\r\n{0}",
                            resolved);
        }

        public DependenciesNotSatisfiedKraken(ResolvedRelationship      badOne,
                                              IRegistryQuerier          registry,
                                              IGame                     game,
                                              ResolvedRelationshipsTree resolved,
                                              Exception?                innerException = null)
            : this(new ResolvedRelationship[][] { new ResolvedRelationship[] { badOne } },
                   registry, game, resolved, innerException)
        {
        }

        public readonly IReadOnlyCollection<ResolvedRelationship[]> unsatisfied;

        private static string FormatDependsChain(ResolvedRelationship[] dependsChain)
            => dependsChain.Length == 1
                ? dependsChain.Last().source.ToString()
                : string.Format("{0} ({1})",
                                dependsChain.Last().source,
                                string.Join(", ", dependsChain.Reverse()
                                                              .Skip(1)
                                                              .Select(FormatDependsLink)));

        private static string FormatDependsLink(ResolvedRelationship rr)
            => string.Format(Properties.Resources.KrakenMissingDependencyNeededFor,
                             rr.source);

        private static readonly ILog log = LogManager.GetLogger(typeof(DependenciesNotSatisfiedKraken));
    }

    public class NotGameDirKraken : Kraken
    {
        public NotGameDirKraken(string     path,
                                string?    reason         = null,
                                Exception? innerException = null)
            : base(reason ?? string.Format(Properties.Resources.KrakenNotGameDir,
                                           Platform.FormatPath(path)),
                   innerException)
        {
            this.path = path;
        }

        public readonly string path;
    }

    public class TransactionalKraken : Kraken
    {
        public TransactionalKraken(string?    reason         = null,
                                   Exception? innerException = null)
            : base(reason, innerException)
        {
        }
    }

    /// <summary>
    /// We had bad metadata that resulted in an invalid operation occuring.
    /// For example: a file install stanza that produces no files.
    /// </summary>
    public class BadMetadataKraken : Kraken
    {
        public BadMetadataKraken(CkanModule? module,
                                 string?     reason         = null,
                                 Exception?  innerException = null)
            : base(reason,
                   innerException)
        {
            this.module = module;
        }

        public override string Message =>
            module != null ? string.Format(Properties.Resources.KrakenBadMetadata,
                                           module, base.Message)
                           : base.Message;

        public CkanModule? module;
    }

    /// <summary>
    /// Thrown if we try to load an incompatible CKAN registry.
    /// </summary>
    public class RegistryVersionNotSupportedKraken : Kraken
    {
        public RegistryVersionNotSupportedKraken(int    badVersion,
                                                 string reason)
            : base(reason)
        {
            requestVersion = badVersion;
        }

        public readonly int requestVersion;
    }

    public class TooManyModsProvideKraken : Kraken
    {
        public TooManyModsProvideKraken(CkanModule                requester,
                                        string                    requested,
                                        IReadOnlyList<CkanModule> modules,
                                        string?                   choice_help_text = null,
                                        Exception?                innerException   = null)
            : base(choice_help_text
                   ?? string.Format(Properties.Resources.KrakenProvidedByMoreThanOne,
                                    requested, requester.name),
                   innerException)
        {
            this.requester        = requester;
            this.requested        = requested;
            this.modules          = modules;
            this.choice_help_text = choice_help_text;
        }

        public readonly CkanModule                requester;
        public readonly IReadOnlyList<CkanModule> modules;
        public readonly string                    requested;
        public readonly string?                   choice_help_text;
    }

    /// <summary>
    /// Thrown if we find ourselves in an inconsistent state, such as when we have multiple modules
    /// installed which conflict with each other.
    /// </summary>
    public class InconsistentKraken : Kraken
    {
        public InconsistentKraken(IReadOnlyCollection<string> inconsistencies,
                                  Exception?                  innerException = null)
            : base(string.Join(Environment.NewLine,
                               new string[] { Properties.Resources.KrakenInconsistenciesHeader }
                                   .Concat(inconsistencies.Select(msg => $"* {msg}"))),
                   innerException)
        {
            this.inconsistencies = inconsistencies;
        }

        public InconsistentKraken(string     inconsistency,
                                  Exception? innerException = null)
            : this(new List<string> { inconsistency }, innerException)
        { }

        public string ShortDescription
            => string.Join("; ", inconsistencies);

        private readonly IReadOnlyCollection<string> inconsistencies;
    }

    public class FailedToDeleteFilesKraken : Kraken
    {
        public FailedToDeleteFilesKraken(string       identifier,
                                         List<string> undeletableFiles)
            : base(string.Format(Properties.Resources.KrakenFailedToDeleteFiles,
                                 identifier,
                                 string.Join(Environment.NewLine,
                                             undeletableFiles.Select(Platform.FormatPath))))
        {
            this.undeletableFiles = undeletableFiles;
        }

        public readonly List<string> undeletableFiles;
    }

    /// <summary>
    /// A mutation of InconsistentKraken that allows catching code to extract the causes of the errors.
    /// </summary>
    public class BadRelationshipsKraken : InconsistentKraken
    {
        public BadRelationshipsKraken(List<Tuple<CkanModule, RelationshipDescriptor>> depends,
                                      modRelList                                      conflicts)
            : base((depends?.Select(dep => string.Format(Properties.Resources.KrakenMissingDependency,
                                                         dep.Item1, dep.Item2))
                           ?? Array.Empty<string>())
                       .Concat(conflicts?.Select(conf => string.Format(Properties.Resources.KrakenConflictsWith,
                                                                       conf.Item1, conf.Item2))
                                        ?? Array.Empty<string>())
                       .ToArray())
        {
            Depends   = depends   ?? new List<Tuple<CkanModule, RelationshipDescriptor>>();
            Conflicts = conflicts ?? new modRelList();
        }

        public readonly List<Tuple<CkanModule, RelationshipDescriptor>> Depends;
        public readonly modRelList                                      Conflicts;
    }

    /// <summary>
    /// The terrible state when a file exists when we expect it not to be there.
    /// For example, when we install a mod, and it tries to overwrite a file from another mod.
    /// </summary>
    public class FileExistsKraken : Kraken
    {
        public FileExistsKraken(string     filename,
                                Exception? innerException = null)
            : base(null, innerException)
        {
            this.filename = filename;
        }

        public override string Message =>
            installingModule == null
                ? string.Format(Properties.Resources.KrakenFileExistsWithoutInstalling,
                                filename)
                : owningModule == null
                    ? string.Format(Properties.Resources.KrakenFileExistsWithoutOwner,
                                    filename, installingModule)
                    : string.Format(Properties.Resources.KrakenFileExistsWithOwner,
                                    filename, installingModule, owningModule.Module);

        public string filename;

        // These aren't set at construction time, but exist so that we can decorate the
        // kraken as appropriate.
        public CkanModule?      installingModule;
        public InstalledModule? owningModule;
    }

    /// <summary>
    /// The terrible state when errors occurred during downloading.
    /// Requires an IEnumerable list of exceptions on construction.
    /// </summary>
    public class DownloadErrorsKraken : Kraken
    {
        public DownloadErrorsKraken(List<KeyValuePair<NetAsyncDownloader.DownloadTarget, Exception>> errors)
            : base(string.Join(Environment.NewLine,
                               errors.Select(e => e.Value.Message)
                                     .Prepend("")
                                     .Prepend(Properties.Resources.KrakenDownloadErrorsHeader)))
        {
            Exceptions = new List<KeyValuePair<NetAsyncDownloader.DownloadTarget, Exception>>(errors);
        }

        public DownloadErrorsKraken(NetAsyncDownloader.DownloadTarget target, Exception exc)
            : base(string.Join(Environment.NewLine,
                               new string[]
                               {
                                   Properties.Resources.KrakenDownloadErrorsHeader,
                                   "",
                                   exc.Message
                               }))
        {
            Exceptions = new List<KeyValuePair<NetAsyncDownloader.DownloadTarget, Exception>>
            {
                new KeyValuePair<NetAsyncDownloader.DownloadTarget, Exception>(target, exc),
            };
        }

        public readonly List<KeyValuePair<NetAsyncDownloader.DownloadTarget, Exception>> Exceptions;
    }

    /// <summary>
    /// A download errors exception that knows about modules,
    /// to make the error message nicer.
    /// </summary>
    public class ModuleDownloadErrorsKraken : Kraken
    {
        public ModuleDownloadErrorsKraken(List<KeyValuePair<CkanModule, Exception>> errors)
            : base(string.Join(Environment.NewLine,
                               errors.Select(kvp => string.Format(Properties.Resources.KrakenModuleDownloadError,
                                                                  kvp.Key,
                                                                  kvp.Value.Message))
                                     .Prepend("")
                                     .Prepend(Properties.Resources.KrakenModuleDownloadErrorsHeader)))
        {
            Exceptions = new List<KeyValuePair<CkanModule, Exception>>(errors);
        }

        public readonly List<KeyValuePair<CkanModule, Exception>> Exceptions;
    }

    /// <summary>
    /// The terrible kraken summoned forth to indicate a user cancelled whatever
    /// we were doing.
    /// </summary>
    public class CancelledActionKraken : Kraken
    {
        public CancelledActionKraken(string?    reason         = null,
                                     Exception? innerException = null)
            : base(reason, innerException)
        {
        }
    }

    /// <summary>
    /// The terrible kraken that awakens from the deep when we don't support something,
    /// like a metadata spec from the future.
    /// </summary>
    public class UnsupportedKraken : Kraken
    {
        public UnsupportedKraken(string     reason,
                                 Exception? innerException = null)
            : base(reason, innerException)
        {
        }
    }

    /// <summary>
    /// The mighty kraken that emerges from the depth when we have a problem with a path,
    /// such as when it cannot be converted from absolute to relative, or vice-versa.
    /// </summary>
    public class PathErrorKraken : Kraken
    {
        public PathErrorKraken(string?    path,
                               string?    reason         = null,
                               Exception? innerException = null)
            : base(reason, innerException)
        {
            this.path = path;
        }

        public readonly string? path;
    }

    /// <summary>
    /// Tremble, mortal, for ye has summoned the kraken of mods which are not installed.
    /// Thou hast tried to remove or perform actions upon a mod that is not there!
    /// </summary>
    public class ModNotInstalledKraken : Kraken
    {
        public ModNotInstalledKraken(string     mod,
                                     string?    reason         = null,
                                     Exception? innerException = null)
            : base(reason
                   ?? string.Format(Properties.Resources.KrakenNotInstalled,
                                    mod),
                   innerException)
        {
            this.mod = mod;
        }

        public readonly string mod;
    }

    /// <summary>
    /// A bad command; useful for things like command-line handling, or REST servers.
    /// </summary>
    public class BadCommandKraken : Kraken
    {
        public BadCommandKraken(string?    reason         = null,
                                Exception? innerException = null)
            : base(reason, innerException)
        {
        }
    }

    public class MissingCertificateKraken : Kraken
    {
        public MissingCertificateKraken(Uri        url,
                                        string?    reason         = null,
                                        Exception? innerException = null)
            : base(reason
                   ?? (Platform.IsMono
                          ? string.Format(Properties.Resources.KrakenMissingCertificateUnix,
                                          HelpURLs.CertificateErrors,
                                          url.OriginalString)
                          : string.Format(Properties.Resources.KrakenMissingCertificateNotUnix,
                                          url.OriginalString)),
                   innerException)
        {
            this.url = url;
        }

        public readonly Uri url;
    }

    public class RequestTimedOutKraken : Kraken
    {
        public RequestTimedOutKraken(Uri          url,
                                     WebException innerExc,
                                     string?      reason = null)
            : base(reason
                   ?? string.Format(Properties.Resources.KrakenRequestTimedOut,
                                    url.OriginalString),
                   innerExc)
        {
            this.url = url;
        }

        public readonly Uri url;
    }

    public class RequestThrottledKraken : Kraken
    {
        public RequestThrottledKraken(Uri          url,
                                      Uri          info,
                                      WebException exc,
                                      string?      reason = null)
            : this(url, info, ExceptionRetryTimes(exc).Max(), reason)
        {
        }

        private RequestThrottledKraken(Uri      url,
                                       Uri      info,
                                       DateTime retryTime,
                                       string?  reason = null)
            : base(reason
                   ?? string.Format(Properties.Resources.KrakenDownloadThrottled,
                                    url.Host))
        {
            throttledUrl   = url;
            infoUrl        = info;
            this.retryTime = retryTime;
        }

        public readonly Uri      throttledUrl;
        public readonly Uri      infoUrl;
        public readonly DateTime retryTime;

        // https://docs.github.com/en/rest/using-the-rest-api/best-practices-for-using-the-rest-api?apiVersion=2022-11-28#handle-rate-limit-errors-appropriately
        private static IEnumerable<DateTime> ExceptionRetryTimes(WebException exc)
        {
            // If the retry-after response header is present, you should not retry your request
            // until after that many seconds has elapsed.
            if (exc.Response?.Headers["Retry-After"] is string waitString
                && int.TryParse(waitString, out int waitSeconds))
            {
                yield return DateTime.UtcNow + TimeSpan.FromSeconds(waitSeconds);
            }
            // If the x-ratelimit-remaining header is 0, you should not make another request
            // until after the time specified by the x-ratelimit-reset header.
            // The x-ratelimit-reset header is in UTC epoch seconds.
            if (exc.Response?.Headers["X-RateLimit-Reset"] is string epochString
                && int.TryParse(epochString, out int epochSeconds))
            {
                yield return UnixEpoch + TimeSpan.FromSeconds(epochSeconds);
            }
            // Otherwise, wait for at least one minute before retrying.
            yield return DateTime.UtcNow + TimeSpan.FromMinutes(1);
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    public class RegistryInUseKraken : Kraken
    {
        public RegistryInUseKraken(string     path,
                                   string?    reason          = null,
                                   Exception? inner_exception = null)
            : base(reason
                   ?? string.Format(Properties.Resources.KrakenAlreadyRunning,
                                    Platform.FormatPath(path)),
                   inner_exception)
        {
            lockfilePath = path;
        }

        public readonly string lockfilePath;
    }

    /// <summary>
    /// Exception thrown when a downloaded file isn't valid for a module.
    /// Happens if:
    ///   1. Size doesn't match download_size
    ///   2. Not a valid ZIP file
    ///   3. SHA1 doesn't match download_hash.sha1
    ///   4. SHA256 doesn't match download_hash.sha256
    /// </summary>
    public class InvalidModuleFileKraken : Kraken
    {
        /// <summary>
        /// Release the kraken
        /// </summary>
        /// <param name="module">Module to check against path</param>
        /// <param name="path">Path to the file to check against module</param>
        /// <param name="reason">Human-readable description of the problem</param>
        public InvalidModuleFileKraken(CkanModule module,
                                       string     path,
                                       string?    reason = null)
            : base(reason)
        {
            this.module = module;
            this.path   = path;
        }

        /// <summary>
        /// The module that doesn't match the file
        /// </summary>
        public readonly CkanModule module;

        /// <summary>
        /// Path to the file that doesn't match the module
        /// </summary>
        public readonly string     path;
    }

    /// <summary>
    /// The version is either not in the build map, or incomplete or something like this.
    /// </summary>
    public class BadGameVersionKraken : Kraken
    {
        public BadGameVersionKraken(string?    reason          = null,
                                    Exception? inner_exception = null)
            : base(reason, inner_exception)
        {
        }
    }

    /// <summary>
    /// The version is a known and per se a valid Game version, but is not allowed to be used for an action.
    /// For example the given base game version is too low to fake a DLC in instance faking.
    /// </summary>
    public class WrongGameVersionKraken : Kraken
    {
        public WrongGameVersionKraken(GameVersion version,
                                      string?     reason          = null,
                                      Exception?  inner_exception = null)
            : base(reason, inner_exception)
        {
            this.version = version;
        }

        public readonly GameVersion version;
    }

    /// <summary>
    /// The instance name is already in use.
    /// </summary>
    public class InstanceNameTakenKraken : Kraken
    {
        public InstanceNameTakenKraken(string  name,
                                       string? reason = null)
            : base(reason ?? string.Format(Properties.Resources.KrakenInstanceNameTaken, name))
        {
            instName = name;
        }

        public readonly string instName;
    }

    public class ModuleIsDLCKraken : Kraken
    {
        public ModuleIsDLCKraken(CkanModule module,
                                 string?    reason = null)
            : base(reason
                   ?? (new Uri?[] {
                          module.resources?.store,
                          module.resources?.steamstore,
                          module.resources?.gogstore,
                          module.resources?.epicstore,
                       }
                           .OfType<Uri>()
                           .ToArray()
                       is Uri[] { Length: > 0 } urls
                           ? Properties.Resources.KrakenIsDLC
                             + string.Format(Properties.Resources.KrakenIsDLCStorePage,
                                             string.Join(Environment.NewLine,
                                                         urls.Select(u => $"- {u}")))
                           : Properties.Resources.KrakenIsDLC))
        {
            this.module = module;
        }

        /// <summary>
        /// The DLC module that can't be operated upon
        /// </summary>
        public readonly CkanModule module;
    }

    /// <summary>
    /// A manually installed mod is installed somewhere other than
    /// where CKAN would install it, so we can't safely overwrite it.
    /// </summary>
    public class DllLocationMismatchKraken : Kraken
    {
        public DllLocationMismatchKraken(string  path,
                                         string? reason = null)
            : base(reason)
        {
            this.path = path;
        }

        public readonly string path;
    }

    public class GameManagerKraken : Kraken
    {
        public GameManagerKraken(string?    reason         = null,
                                 Exception? innerException = null)
            : base(reason, innerException)
        {
        }
    }

    public class InvalidGameInstanceKraken : Kraken
    {
        public InvalidGameInstanceKraken(string     instance,
                                         string?    reason         = null,
                                         Exception? innerException = null)
            : base(reason, innerException)
        {
            this.instance = instance;
        }

        public readonly string instance;
    }

    public class InvalidModuleAttributesKraken : Kraken
    {
        public InvalidModuleAttributesKraken(string      why,
                                             CkanModule? module = null)
            : base(string.Format("[InvalidModuleAttributesKraken] {0} in {1}",
                                 why, module?.identifier ?? "unknown"))
        {
            this.module = module;
        }

        public readonly CkanModule? module;
    }
}
