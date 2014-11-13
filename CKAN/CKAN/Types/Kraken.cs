using System;
using System.Collections.Generic;

namespace CKAN
{
    /// <summary>
    /// Our application exceptions are called Krakens.
    /// </summary>
    public class Kraken : Exception
    {
        public Kraken(string reason = null, Exception inner_exception = null) : base(reason, inner_exception)
        {
        }
    }

    public class FileNotFoundKraken : Kraken
    {
        public string file;

        public FileNotFoundKraken(string file, string reason = null, Exception inner_exception = null) 
            :base(reason, inner_exception)
        {
            this.file = file;
        }
    }

    public class DirectoryNotFoundKraken : Kraken
    {
        public string directory;

        public DirectoryNotFoundKraken(string directory, string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
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

        public BadInstallLocationKraken(string reason = null, Exception inner_exception = null) : base(reason, inner_exception)
        {
        }
    }

    public class ModuleNotFoundKraken : Kraken
    {
        public string module;
        public string version;

        // TODO: Is there a way to set the stringify version of this?
        public ModuleNotFoundKraken(string module, string version = null, string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
        {
            this.module = module;
            this.version = version;
        }
    }

    public class NotKSPDirKraken : Kraken
    {
        public string path;

        public NotKSPDirKraken(string path, string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
        {
            this.path = path;
        }
    }

    public class TransactionalKraken : Kraken
    {
        public TransactionalKraken(string reason = null, Exception inner_exception = null)
            :base(reason,inner_exception)
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

        public BadMetadataKraken(CkanModule module, string reason = null, Exception inner_exception = null)
            :base(reason,inner_exception)
        {
            this.module = module;
        }
    }

    /// <summary>
    /// Thrown if we try to load an incompatible CKAN registry.
    /// </summary>
    public class RegistryVersionNotSupportedKraken : Kraken
    {
        public int requested_version;

        public RegistryVersionNotSupportedKraken(int v, string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
        {
            requested_version = v;
        }
    }

    public class TooManyModsProvideKraken : Kraken
    {
        public List<CkanModule> modules;
        public string requested;

        public TooManyModsProvideKraken(string requested, List<CkanModule> modules, Exception inner_exception = null)
            :base(FormatMessage(requested, modules), inner_exception)
        {
            this.modules = modules;
            this.requested = requested;
        }

        internal static string FormatMessage(string requested, List<CkanModule> modules)
        {
            string oops = string.Format("Too many mods provide {0}:\n\n", requested);
            foreach (var mod in modules)
            {
                oops += "* " + mod + "\n";
            }
            return oops;
        }
    }

    /// <summary>
    /// Thrown if we find ourselves in an inconsistent state, such as when we have multiple modules
    /// installed which conflict with each other.
    /// </summary>
    public class InconsistentKraken : Kraken
    {
        public List<string> inconsistencies;

        public string InconsistenciesPretty
        {
            get {
                string message = "The following inconsistecies were found:\n\n";

                foreach (string issue in inconsistencies)
                {
                    message += " * " + issue + "\n";
                }

                return message;
            }
        }

        public InconsistentKraken(List<string> inconsistencies, Exception inner_exception = null)
            :base(null, inner_exception)
        {
            this.inconsistencies = inconsistencies;
        }

        public InconsistentKraken(string inconsistency, Exception inner_exception = null)
            :base(null, inner_exception)
        {
            this.inconsistencies = new List<string> { inconsistency };
        }

        public override string ToString()
        {
            return this.InconsistenciesPretty + this.StackTrace;
        }
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
        public CkanModule installing_module;
        public string owning_module;

        public FileExistsKraken(string filename, string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
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
        public List<Exception> exceptions;

        public DownloadErrorsKraken(IEnumerable<Exception> errors, string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
        {
            this.exceptions = new List<Exception> (errors);
        }

        public override string ToString()
        {
            return "Uh oh, the following things went wrong when downloading...\n\n" + String.Join("\n", exceptions);
        }
    }

    /// <summary>
    /// The terrible kraken summoned forth to indicate a user cancelled whatever
    /// we were doing.
    /// </summary>
    public class CancelledActionKraken : Kraken
    {
        public CancelledActionKraken(string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
        {
        }
    }

    /// <summary>
    /// The terrible kraken that awakens from the deep when we don't support something,
    /// like a metadata spec from the future.
    /// </summary>
    public class UnsupportedKraken : Kraken
    {
        public UnsupportedKraken(string reason, Exception inner_exception = null)
            :base(reason, inner_exception)
        {
        }
    }

    public class MissingCertificateKraken : Kraken
    {
        public MissingCertificateKraken(string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
        {
        }

        public override string ToString()
        {
            return
                "\nOh no! Our download failed with a certificate error!\n\n" +
                "If you're on Linux, try running:\n" +
                "\tmozroots --import --ask-remove\n" +
                "on the command-line to update your certificate store, and try again.\n\n"
            ;
        }
    }
}
