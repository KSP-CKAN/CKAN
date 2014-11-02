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
    /// Thrown if our registry was going to contain conflicting modules or un-met depdendencies.
    /// Our last defence against having a totally out-of-whack install.
    /// </summary>
    public class RegistryInsaneKraken : Kraken
    {
        public RegistryInsaneKraken(string reason = null, Exception inner_exception = null)
            :base(reason, inner_exception)
        {
        }
    }
}
