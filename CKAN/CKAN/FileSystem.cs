namespace CKAN {

    using System.IO;

    public class FileSystem {

        public static bool IsDirectory(string path) {
            try {
                FileAttributes attr = File.GetAttributes (path);

                return ((attr & FileAttributes.Directory) == FileAttributes.Directory);
            } catch (System.IO.DirectoryNotFoundException) {
                return false;
            }
        }
    }
}