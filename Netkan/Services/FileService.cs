using System.IO;

namespace CKAN.NetKAN.Services
{
    internal sealed class FileService : IFileService
    {
        public FileService(NetFileCache cache)
        {
            this.cache = cache;
        }

        public long GetSizeBytes(string filePath)
            => new FileInfo(filePath).Length;

        public string GetFileHashSha1(string filePath)
            // Use shared implementation from Core.
            // Also needs to be an instance method so it can be Moq'd for testing.
            => cache.GetFileHashSha1(filePath, null);

        public string GetFileHashSha256(string filePath)
            // Use shared implementation from Core.
            // Also needs to be an instance method so it can be Moq'd for testing.
            => cache.GetFileHashSha256(filePath, null);

        public string GetMimetype(string filePath)
            => FileIdentifier.IdentifyFile(filePath) switch
            {
                FileType.ASCII   => "text/plain",
                FileType.GZip    => "application/x-gzip",
                FileType.Tar     => "application/x-tar",
                FileType.TarGz   => "application/x-compressed-tar",
                FileType.Zip     => "application/zip",
                FileType.Unknown => "application/octet-stream",
                _                => "application/octet-stream",
            };

        private readonly NetFileCache cache;
    }
}
