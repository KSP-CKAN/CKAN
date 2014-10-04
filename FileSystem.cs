namespace CKAN {

    using System.IO;
    using System;

    public class FileSystem {

        // This implements Perl's equivalent of `-d`. Yes. This is two characters in Perl.

        public static bool IsDirectory(string path) {
            try {
                FileAttributes attr = File.GetAttributes (path);

                return ((attr & FileAttributes.Directory) == FileAttributes.Directory);
            } catch (Exception ex) {

                if (ex is System.IO.DirectoryNotFoundException || ex is System.IO.FileNotFoundException) {

                    // It's okay if the directory isn't there, we just ignore it.
                    return false;

                } else {

                    // It's not okay if something else goes wrong.
                    throw;
                }
            }
        }
    }
}