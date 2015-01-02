using System;
using System.IO;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;

namespace CKAN
{
    public static class URLHandlers
    {

        public static void RegisterURLHandler()
        {
            if (Util.IsLinux)
            {
                RegisterURLHandler_Linux();
            }
            else
            {
                RegisterURLHandler_Win32();
            }
        }

        private static void RegisterURLHandler_Win32()
        {
            var root = Microsoft.Win32.Registry.ClassesRoot;

            if (root.OpenSubKey("ckan") != null)
            {
                root.DeleteSubKeyTree("ckan");
            }

            var key = root.CreateSubKey("ckan");
            key.SetValue("", "URL: ckan Protocol");
            key.SetValue("URL Protocol", "");
            key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private const string MimeAppsListPath = "~/.local/share/applications/mimeapps.list";
        private const string ApplicationsPath = "~/.local/share/applications/";
        private const string HandlerFileName = "ckan-handler.desktop";

        private static void RegisterURLHandler_Linux()
        {
            var parser = new FileIniDataParser();
            IniData data = null;

            try
            {
                data = parser.ReadFile(MimeAppsListPath); //();
            }
            catch (DirectoryNotFoundException)
            {
                return;
            }
            catch (FileNotFoundException)
            {
                return;
            }
            catch (ParsingException)
            {
                return;
            }

            if (data == null)
            {
                return;
            }

            data["Added Associations"].RemoveKey("x-scheme-handler/ckan");
            data["Added Associations"].AddKey("x-scheme-handler/ckan", HandlerFileName);

            parser.WriteFile(MimeAppsListPath, data);

            var handlerPath = Path.Combine(ApplicationsPath, HandlerFileName);

            if (File.Exists(handlerPath))
            {
                File.Delete(handlerPath);
            }

            File.WriteAllText(handlerPath, "");
            data = parser.ReadFile(handlerPath);
            data.Sections.AddSection("Desktop Entry");
            data["Desktop Entry"].AddKey("Version", "1.0");
            data["Desktop Entry"].AddKey("Type", "Application");
            data["Desktop Entry"].AddKey("Exec", System.Reflection.Assembly.GetExecutingAssembly().Location + " gui %u");
            data["Desktop Entry"].AddKey("Icon", "ckan");
            data["Desktop Entry"].AddKey("StartupNotify", "true");
            data["Desktop Entry"].AddKey("Terminal", "false");
            data["Desktop Entry"].AddKey("Categories", "Utility");
            data["Desktop Entry"].AddKey("MimeType", "x-scheme-handler/ckan");
            data["Desktop Entry"].AddKey("Name", "CKAN Launcher");
            data["Desktop Entry"].AddKey("Comment", "Launch CKAN");

            parser.WriteFile(handlerPath, data);
        }

    }

}


