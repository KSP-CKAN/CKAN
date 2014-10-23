using System;

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
        public FileNotFoundKraken(string reason = null, Exception inner_exception = null) : base(reason, inner_exception)
        {
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

}

