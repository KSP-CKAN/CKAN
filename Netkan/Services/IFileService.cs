namespace CKAN.NetKAN.Services
{
    internal interface IFileService
    {
        long GetSizeBytes(string filePath);

        string GetFileHashSha1(string filePath);

        string GetFileHashSha256(string filePath);

        string GetMimetype(string filePath);
    }
}