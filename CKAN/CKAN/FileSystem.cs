namespace CKAN {

    using System.IO;

    public class FileSystem {

        public static bool IsDirectory(string path) {
            FileAttributes attr = File.GetAttributes (path);

            return ((attr & FileAttributes.Directory) == FileAttributes.Directory);
        }
    }
}