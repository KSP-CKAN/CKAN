using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using CKAN.Versioning;

namespace CKAN
{
    using modRelList = List<Tuple<CkanModule, RelationshipDescriptor, CkanModule>>;

    /// <summary>
    /// Our application exceptions are called Krakens.
    /// </summary>
    public class Kraken : Exception
    {
        public Kraken(string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
        }
    }

    public class FileNotFoundKraken : Kraken
    {
        public readonly string file;

        public FileNotFoundKraken(string file, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.file = file;
        }
    }

    public class DirectoryNotFoundKraken : Kraken
    {
        public readonly string directory;

        public DirectoryNotFoundKraken(string directory, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.directory = directory;
        }
    }

    public class NotEnoughSpaceKraken : Kraken
    {
        public readonly DirectoryInfo destination;
        public readonly long          bytesFree;
        public readonly long          bytesToStore;

        public NotEnoughSpaceKraken(string description, DirectoryInfo destination, long bytesFree, long bytesToStore)
            : base(string.Format(Properties.Resources.KrakenNotEnoughSpace,
                                 description, destination,
                                 CkanModule.FmtSize(bytesFree),
                                 CkanModule.FmtSize(bytesToStore)))
        {
            this.destination  = destination;
            this.bytesFree    = bytesFree;
            this.bytesToStore = bytesToStore;
        }
    }

    /// <summary>
    /// A bad install location was provided.
    /// Valid locations are GameData, GameRoot, Ships, etc.
    /// </summary>
    public class BadInstallLocationKraken : Kraken
    {
        public BadInstallLocationKraken(string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
        }
    }

    public class ModuleNotFoundKraken : Kraken
    {
        public readonly string module;
        public readonly string version;

        public ModuleNotFoundKraken(string module, string version, string reason, Exception innerException = null)
            : base(reason
                   ?? string.Format(Properties.Resources.KrakenDependencyNotSatisfied, module, version),
                   innerException)
        {
            this.module  = module;
            this.version = version;
        }

        public ModuleNotFoundKraken(string module, string version = null)
            : this(module, version,
                string.Format(Properties.Resources.KrakenDependencyModuleNotFound, module, version ?? ""))
        { }

    }

    /// <summary>
    /// Exception describing a missing dependency
    /// </summary>
    public class DependencyNotSatisfiedKraken : ModuleNotFoundKraken
    {
        /// <summary>
        /// The mod with an unmet dependency
        /// </summary>
        public readonly CkanModule parent;

        /// <summary>
        /// Initialize the exceptions
        /// </summary>
        /// <param name="parentModule">The module with the unmet dependency</param>
        /// <param name="module">The name of the missing dependency</param>
        /// <param name="reason">Message parameter for base class</param>
        /// <param name="innerException">Originating exception parameter for base class</param>
        public DependencyNotSatisfiedKraken(CkanModule parentModule,
                                            string     module,
                                            string     version        = null,
                                            string     reason         = null,
                                            Exception  innerException = null)
            : base(module, version,
                   reason ?? string.Format(
                       Properties.Resources.KrakenParentDependencyNotSatisfied,
                       parentModule.identifier,
                       module,
                       version ?? Properties.Resources.KrakenAny),
                   innerException)
        {
            parent = parentModule;
        }
    }

    public class NotKSPDirKraken : Kraken
    {
        public readonly string path;

        public NotKSPDirKraken(string path, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.path = path;
        }
    }

    public class TransactionalKraken : Kraken
    {
        public TransactionalKraken(string reason = null, Exception innerException = null)
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
        public CkanModule module;

        public BadMetadataKraken(CkanModule module, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.module = module;
        }
    }

    /// <summary>
    /// Thrown if we try to load an incompatible CKAN registry.
    /// </summary>
    public class RegistryVersionNotSupportedKraken : Kraken
    {
        public readonly int requestVersion;

        public RegistryVersionNotSupportedKraken(int v, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            requestVersion = v;
        }
    }

    public class TooManyModsProvideKraken : Kraken
    {
        public readonly CkanModule requester;
        public readonly List<CkanModule> modules;
        public readonly string requested;
        public readonly string choice_help_text;

        public TooManyModsProvideKraken(CkanModule       requester,
                                        string           requested,
                                        List<CkanModule> modules,
                                        string           choice_help_text = null,
                                        Exception        innerException   = null)
            : base(choice_help_text ?? string.Format(Properties.Resources.KrakenProvidedByMoreThanOne,
                                                     requested, requester.name),
                   innerException)
        {
            this.requester        = requester;
            this.requested        = requested;
            this.modules          = modules;
            this.choice_help_text = choice_help_text;
        }
    }

    /// <summary>
    /// Thrown if we find ourselves in an inconsistent state, such as when we have multiple modules
    /// installed which conflict with each other.
    /// </summary>
    public class InconsistentKraken : Kraken
    {
        public InconsistentKraken(ICollection<string> inconsistencies, Exception innerException = null)
            : base(string.Join(Environment.NewLine,
                               new string[] { Properties.Resources.KrakenInconsistenciesHeader }
                                   .Concat(inconsistencies.Select(msg => $"* {msg}"))),
                   innerException)
        {
            this.inconsistencies = inconsistencies;
        }

        public InconsistentKraken(string inconsistency, Exception innerException = null)
            : this(new List<string> { inconsistency }, innerException)
        { }

        public string ShortDescription
            => string.Join("; ", inconsistencies);

        public override string ToString()
            => Message + Environment.NewLine + Environment.NewLine + StackTrace;

        private readonly ICollection<string> inconsistencies;
    }

    public class FailedToDeleteFilesKraken : Kraken
    {
        public FailedToDeleteFilesKraken(string identifier, List<string> undeletableFiles)
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
        public BadRelationshipsKraken(
            modRelList depends,
            modRelList conflicts
        ) : base(
            (depends?.Select(dep => string.Format(Properties.Resources.KrakenMissingDependency, dep.Item1, dep.Item2))
                ?? Array.Empty<string>()
            ).Concat(
                conflicts?.Select(conf => string.Format(Properties.Resources.KrakenConflictsWith, conf.Item1, conf.Item2))
                ?? Array.Empty<string>()
            ).ToArray()
        )
        {
            Depends   = depends   ?? new modRelList();
            Conflicts = conflicts ?? new modRelList();
        }

        public readonly modRelList Depends;
        public readonly modRelList Conflicts;
    }

    /// <summary>
    /// The terrible state when a file exists when we expect it not to be there.
    /// For example, when we install a mod, and it tries to overwrite a file from another mod.
    /// </summary>
    public class FileExistsKraken : Kraken
    {
        public string filename;

        // These aren't set at construction time, but exist so that we can decorate the
        // kraken as appropriate.
        public CkanModule      installingModule;
        public InstalledModule owningModule;

        public FileExistsKraken(string filename, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.filename = filename;
        }
    }

    /// <summary>
    /// The terrible state when errors occurred during downloading.
    /// Requires an IEnumerable list of exceptions on construction.
    /// Has a specialised ToString() that shows everything that went wrong.
    /// </summary>
    public class DownloadErrorsKraken : Kraken
    {
        public readonly List<KeyValuePair<int, Exception>> Exceptions
            = new List<KeyValuePair<int, Exception>>();

        public DownloadErrorsKraken(List<KeyValuePair<int, Exception>> errors)
            : base(string.Join(Environment.NewLine,
                               new string[] { Properties.Resources.KrakenDownloadErrorsHeader, "" }
                               .Concat(errors.Select(e => e.Value.Message))))
        {
            Exceptions = new List<KeyValuePair<int, Exception>>(errors);
        }

        public DownloadErrorsKraken(int index, Exception exc)
        {
            Exceptions = new List<KeyValuePair<int, Exception>>
            {
                new KeyValuePair<int, Exception>(index, exc),
            };
        }
    }

    /// <summary>
    /// A download errors exception that knows about modules,
    /// to make the error message nicer.
    /// </summary>
    public class ModuleDownloadErrorsKraken : Kraken
    {
        public readonly List<KeyValuePair<CkanModule, Exception>> Exceptions
            = new List<KeyValuePair<CkanModule, Exception>>();

        /// <summary>
        /// Initialize the exception.
        /// </summary>
        /// <param name="modules">List of modules that we tried to download</param>
        /// <param name="kraken">Download errors from URL-level downloader</param>
        public ModuleDownloadErrorsKraken(IList<CkanModule> modules, DownloadErrorsKraken kraken)
            : base()
        {
            foreach (var kvp in kraken.Exceptions)
            {
                Exceptions.Add(new KeyValuePair<CkanModule, Exception>(
                    modules[kvp.Key], kvp.Value.GetBaseException() ?? kvp.Value
                ));
            }
        }

        /// <summary>
        /// Generate a user friendly description of this error.
        /// </summary>
        /// <returns>
        /// One or more downloads were unsuccessful:
        ///
        /// Error downloading Astrogator v0.7.8: The remote server returned an error: (404) Not Found.
        /// Etc.
        /// </returns>
        public override string ToString()
        {
            if (builder == null)
            {
                builder = new StringBuilder();
                builder.AppendLine(Properties.Resources.KrakenModuleDownloadErrorsHeader);
                builder.AppendLine("");
                foreach (KeyValuePair<CkanModule, Exception> kvp in Exceptions)
                {
                    builder.AppendLine(string.Format(
                        Properties.Resources.KrakenModuleDownloadError, kvp.Key.ToString(), kvp.Value.Message));
                }
            }
            return builder.ToString();
        }

        private StringBuilder builder = null;
    }

    /// <summary>
    /// The terrible kraken summoned forth to indicate a user cancelled whatever
    /// we were doing.
    /// </summary>
    public class CancelledActionKraken : Kraken
    {
        public CancelledActionKraken(string reason = null, Exception innerException = null)
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
        public UnsupportedKraken(string reason, Exception innerException = null)
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
        public readonly string path;

        public PathErrorKraken(string path, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.path = path;
        }
    }

    /// <summary>
    /// Tremble, mortal, for ye has summoned the kraken of mods which are not installed.
    /// Thou hast tried to remove or perform actions upon a mod that is not there!
    /// This kraken provides a custom Message
    /// </summary>
    public class ModNotInstalledKraken : Kraken
    {
        public readonly string mod;

        public override string Message => string.Format(Properties.Resources.KrakenNotInstalled, mod);

        // TODO: Since we override message, should we really allow users to pass in a reason
        // here? Is there a way we can check if that was set, and then access it directly from
        // our base class?

        public ModNotInstalledKraken(string mod, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.mod = mod;
        }
    }

    /// <summary>
    /// A bad command; useful for things like command-line handling, or REST servers.
    /// </summary>
    public class BadCommandKraken : Kraken
    {
        public BadCommandKraken(string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
        }
    }

    public class MissingCertificateKraken : Kraken
    {
        public MissingCertificateKraken(string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
        }

        public override string ToString()
        {
            return Platform.IsUnix
                ? string.Format(Properties.Resources.KrakenMissingCertificateUnix, HelpURLs.CertificateErrors)
                : Properties.Resources.KrakenMissingCertificateNotUnix;
        }
    }

    public class DownloadThrottledKraken : Kraken
    {
        public readonly Uri throttledUrl;
        public readonly Uri infoUrl;

        public DownloadThrottledKraken(Uri url, Uri info) : base()
        {
            throttledUrl = url;
            infoUrl      = info;
        }

        public override string ToString()
        {
            return string.Format(Properties.Resources.KrakenDownloadThrottled, throttledUrl.Host);
        }
    }

    public class RegistryInUseKraken : Kraken
    {
        public readonly string lockfilePath;

        public RegistryInUseKraken(string path, string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
        {
            lockfilePath = path;
        }

        public override string ToString()
            => string.Format(Properties.Resources.KrakenAlreadyRunning,
                             Platform.FormatPath(lockfilePath));
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
        /// The module that doesn't match the file
        /// </summary>
        public readonly CkanModule module;

        /// <summary>
        /// Path to the file that doesn't match the module
        /// </summary>
        public readonly string     path;

        /// <summary>
        /// Release the kraken
        /// </summary>
        /// <param name="module">Module to check against path</param>
        /// <param name="Path">Path to the file to check against module</param>
        /// <param name="reason">Human-readable description of the problem</param>
        public InvalidModuleFileKraken(CkanModule module, string path, string reason = null)
            : base(reason)
        {
            this.module = module;
            this.path   = path;
        }
    }

    /// <summary>
    /// The version is either not in the build map, or incomplete or something like this.
    /// </summary>
    public class BadGameVersionKraken : Kraken
    {
        public BadGameVersionKraken(string reason = null, Exception inner_exception = null)
            : base(reason, inner_exception)
        {
        }
    }

    /// <summary>
    /// The version is a known and per se a valid KSP version, but is not allowed to be used for an action.
    /// For example the given base game version is too low to fake a DLC in instance faking.
    /// </summary>
    public class WrongGameVersionKraken : Kraken
    {
        public readonly GameVersion version;

        public WrongGameVersionKraken(GameVersion version, string reason = null, Exception inner_exception = null)
            : base(reason, inner_exception)
        {
            this.version = version;
        }
    }

    /// <summary>
    /// The instance name is already in use.
    /// </summary>
    public class InstanceNameTakenKraken : Kraken
    {
        public readonly string instName;

        public InstanceNameTakenKraken(string name, string reason = null)
            : base(reason)
        {
            instName = name;
        }
    }

    public class ModuleIsDLCKraken : Kraken
    {
        /// <summary>
        /// The DLC module that can't be operated upon
        /// </summary>
        public readonly CkanModule module;

        public ModuleIsDLCKraken(CkanModule module, string reason = null)
            : base(reason)
        {
            this.module = module;
        }
    }

    /// <summary>
    /// A manually installed mod is installed somewhere other than
    /// where CKAN would install it, so we can't safely overwrite it.
    /// </summary>
    public class DllLocationMismatchKraken : Kraken
    {
        public readonly string path;

        public DllLocationMismatchKraken(string path, string reason = null)
            : base(reason)
        {
            this.path = path;
        }
    }

    public class KSPManagerKraken : Kraken
    {
        public KSPManagerKraken(string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
        }
    }

    public class InvalidKSPInstanceKraken : Kraken
    {
        public readonly string instance;

        public InvalidKSPInstanceKraken(string instance, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.instance = instance;
        }
    }

}
