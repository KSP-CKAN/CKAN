using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace CKAN
{
    /// <summary>
    /// Our application exceptions are called Krakens.
    /// </summary>
    public class Kraken : Exception
    {
        public Kraken(string reason = null, Exception innerException = null) : base(reason, innerException)
        {
        }
    }

    public class FileNotFoundKraken : Kraken
    {
        public string file;

        public FileNotFoundKraken(string file, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.file = file;
        }
    }

    public class DirectoryNotFoundKraken : Kraken
    {
        public string directory;

        public DirectoryNotFoundKraken(string directory, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.directory = directory;
        }
    }

    /// <summary>
    /// A bad install location was provided.
    /// Valid locations are GameData, GameRoot, Ships, etc.
    /// </summary>
    public class BadInstallLocationKraken : Kraken
    {
        // Okay C#, you really need a keyword in your class declaration that says we call our
        // parent constructors by default. This sort of thing is unacceptable in a modern
        // programming langauge.

        public BadInstallLocationKraken(string reason = null, Exception innerException = null) : base(reason, innerException)
        {
        }
    }

    public class ModuleNotFoundKraken : Kraken
    {
        public string module;
        public string version;

        // TODO: Is there a way to set the stringify version of this?
        public ModuleNotFoundKraken(string module, string version, string reason, Exception innerException = null)
            : base(reason ?? $"Dependency on {module} version {version} not satisfied", innerException)
        {
            this.module  = module;
            this.version = version;
        }

        public ModuleNotFoundKraken(string module, string version = null)
            : this(module, version, $"Module not found: {module} {version ?? ""}")
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
            string module, string version = null, string reason = null, Exception innerException = null)
            : base(module, version,
                reason ?? $"{parentModule.identifier} dependency on {module} version {version ?? "(any)"} not satisfied",
                innerException)
        {
            parent = parentModule;
        }
    }

    public class NotKSPDirKraken : Kraken
    {
        public string path;

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
        public int requestVersion;

        public RegistryVersionNotSupportedKraken(int v, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            requestVersion = v;
        }
    }

    public class TooManyModsProvideKraken : Kraken
    {
        public List<CkanModule> modules;
        public string requested;

        public TooManyModsProvideKraken(string requested, List<CkanModule> modules, Exception innerException = null)
            : base(FormatMessage(requested, modules), innerException)
        {
            this.modules = modules;
            this.requested = requested;
        }

        internal static string FormatMessage(string requested, List<CkanModule> modules)
        {
            string oops = string.Format("Too many mods provide {0}:\r\n", requested);
            return oops + String.Join("\r\n", modules.Select(m => $"* {m}"));
        }
    }

    /// <summary>
    /// Thrown if we find ourselves in an inconsistent state, such as when we have multiple modules
    /// installed which conflict with each other.
    /// </summary>
    public class InconsistentKraken : Kraken
    {
        public ICollection<string> inconsistencies;

        public string InconsistenciesPretty
        {
            get
            {
                return header + String.Join("\r\n", inconsistencies.Select(msg => $"* {msg}"));
            }
        }

        public string ShortDescription
        {
            get
            {
                return String.Join("; ", inconsistencies);
            }
        }

        public InconsistentKraken(ICollection<string> inconsistencies, Exception innerException = null)
            : base(null, innerException)
        {
            this.inconsistencies = inconsistencies;
        }

        public InconsistentKraken(string inconsistency, Exception innerException = null)
            : base(null, innerException)
        {
            inconsistencies = new List<string> { inconsistency };
        }

        public override string ToString()
        {
            return InconsistenciesPretty + "\r\n\r\n" + StackTrace;
        }

        private const string header = "The following inconsistencies were found:\r\n";
    }

    /// <summary>
    /// A mutation of InconsistentKraken that allows catching code to extract the causes of the errors.
    /// </summary>
    public class BadRelationshipsKraken : InconsistentKraken
    {
        public BadRelationshipsKraken(
            ICollection<KeyValuePair<CkanModule, RelationshipDescriptor>> depends,
            ICollection<KeyValuePair<CkanModule, RelationshipDescriptor>> conflicts
        ) : base(
            (depends?.Select(dep => $"{dep.Key} missing dependency {dep.Value}")
                ?? new string[] {}
            ).Concat(
                conflicts?.Select(conf => $"{conf.Key} conflicts with {conf.Value}")
                ?? new string[] {}
            ).ToArray()
        )
        {
            Depends   = depends?.ToList()
                ?? new List<KeyValuePair<CkanModule, RelationshipDescriptor>>();
            Conflicts = conflicts?.ToList()
                ?? new List<KeyValuePair<CkanModule, RelationshipDescriptor>>();
        }

        public List<KeyValuePair<CkanModule, RelationshipDescriptor>> Depends   { get; private set; }
        public List<KeyValuePair<CkanModule, RelationshipDescriptor>> Conflicts { get; private set; }
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
        public CkanModule installingModule;
        public string owningModule;

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
        public readonly List<KeyValuePair<int, Exception>> exceptions
            = new List<KeyValuePair<int, Exception>>();

        public DownloadErrorsKraken(List<KeyValuePair<int, Exception>> errors) : base()
        {
            exceptions = new List<KeyValuePair<int, Exception>>(errors);
        }

        public override string ToString()
        {
            return "Uh oh, the following things went wrong when downloading...\r\n\r\n" + String.Join("\r\n", exceptions);
        }
    }

    /// <summary>
    /// A download errors exception that knows about modules,
    /// to make the error message nicer.
    /// </summary>
    public class ModuleDownloadErrorsKraken : Kraken
    {
        /// <summary>
        /// Initialize the exception.
        /// </summary>
        /// <param name="modules">List of modules that we tried to download</param>
        /// <param name="kraken">Download errors from URL-level downloader</param>
        public ModuleDownloadErrorsKraken(IList<CkanModule> modules, DownloadErrorsKraken kraken)
            : base()
        {
            foreach (var kvp in kraken.exceptions)
            {
                exceptions.Add(new KeyValuePair<CkanModule, Exception>(
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
                builder.AppendLine("One or more downloads were unsuccessful:");
                builder.AppendLine("");
                foreach (KeyValuePair<CkanModule, Exception> kvp in exceptions)
                {
                    builder.AppendLine(
                        $"Error downloading {kvp.Key.ToString()}: {kvp.Value.Message}"
                    );
                }
            }
            return builder.ToString();
        }

        private readonly List<KeyValuePair<CkanModule, Exception>> exceptions
            = new List<KeyValuePair<CkanModule, Exception>>();
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
        public string path;

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
        public string mod;

        public override string Message
        {
            get { return string.Format("Module {0} is not installed!", mod); }
        }

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
            if (Platform.IsUnix)
            {
                return "Oh no! Our download failed with a certificate error!\r\n\r\n"
                    + "Consult this page for help:\r\n"
                    + "\thttps://github.com/KSP-CKAN/CKAN/wiki/SSL-certificate-errors";
            }
            else
            {
                return "Oh no! Our download failed with a certificate error!";
            }
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
            return $"Download from {throttledUrl.Host} was throttled.\r\nConsider adding an authentication token to increase the throtting limit.";
        }
    }

    public class RegistryInUseKraken : Kraken
    {
        public readonly string lockfilePath;

        public RegistryInUseKraken(string path, string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
        {
            this.lockfilePath = path;
        }

        public override string ToString()
        {
            return String.Format("CKAN is already running for this instance!\n\nIf you're certain this is not the case, then delete:\n\"{0}\"\n", lockfilePath);
        }
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
        public readonly Versioning.GameVersion version;

        public WrongGameVersionKraken(Versioning.GameVersion version, string reason = null, Exception inner_exception = null)
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
            this.instName = name;
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
        public string instance;

        public InvalidKSPInstanceKraken(string instance, string reason = null, Exception innerException = null)
            : base(reason, innerException)
        {
            this.instance = instance;
        }
    }

    public class ReinstallModuleKraken : Kraken
    {
        public readonly List<CkanModule> Modules;

        public ReinstallModuleKraken(List<CkanModule> modules)
            : base(string.Format("Metadata changed, reinstallation recommended: {0}",
                string.Join(", ", modules)))
        {
            Modules = modules;
        }
    }

}
