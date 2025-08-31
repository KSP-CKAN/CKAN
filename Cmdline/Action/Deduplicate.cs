using log4net;

using CKAN.IO;

namespace CKAN.CmdLine
{
    // Not an ICommand because it doesn't need a game instance
    public class Deduplicate
    {
        public Deduplicate(GameInstanceManager mgr, RepositoryDataManager repoData, IUser user)
        {
            manager       = mgr;
            this.repoData = repoData;
            this.user     = user;
        }

        public int RunCommand()
        {
            log.Debug("Deduplicating...");
            user.RaiseMessage(Properties.Resources.ScanningForDuplicates);
            var deduper = new InstalledFilesDeduplicator(manager.Instances.Values, repoData);
            try
            {
                deduper.DeduplicateAll(user);
                log.Debug("Deduplication done.");
            }
            catch (CancelledActionKraken)
            {
                // Cancelled by user, do nothing.
                log.Debug("Deduplication cancelled.");
            }
            return Exit.OK;
        }

        private readonly GameInstanceManager   manager;
        private readonly RepositoryDataManager repoData;
        private readonly IUser                 user;

        private static readonly ILog log = LogManager.GetLogger(typeof(Deduplicate));
    }

    internal class DeduplicateOptions : CommonOptions { }
}
