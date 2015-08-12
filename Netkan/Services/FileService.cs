using System.IO;

namespace CKAN.NetKAN.Services
{
    internal sealed class FileService : IFileService
    {
        public long GetSizeBytes(string filePath)
        {
            return new FileInfo(filePath).Length;
        }
    }
}
