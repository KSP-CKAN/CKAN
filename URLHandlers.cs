using System;
using System.IO;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using log4net;

namespace CKAN
{
    public static class URLHandlers
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(URLHandlers));

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
            log.InfoFormat("Adding URL handler to registry");

            var root = Microsoft.Win32.Registry.ClassesRoot;

            if (root.OpenSubKey("ckan") != null)
            {
                root.DeleteSubKeyTree("ckan");
            }

            var key = root.CreateSubKey("ckan");
            key.SetValue("", "URL: ckan Protocol");
            key.SetValue("URL Protocol", "");
            key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue
                ("", System.Reflection.Assembly.GetExecutingAssembly().Location + " gui %1");
        }

        private const string MimeAppsListPath = "~/.local/share/applications/mimeapps.list";
        private const string ApplicationsPath = "~/.local/share/applications/";
        private const string HandlerFileName = "ckan-handler.desktop";

        private static void RegisterURLHandler_Linux()
        {
            var parser = new FileIniDataParser();
            IniData data = null;

            log.InfoFormat("Trying to register URL handler");

            try
            {
                data = parser.ReadFile(MimeAppsListPath); //();
            }
            catch (DirectoryNotFoundException ex)
            {
                log.ErrorFormat("Error: {0}", ex.Message);
                return;
            }
            catch (FileNotFoundException ex)
            {
                log.ErrorFormat("Error: {0}", ex.Message);
                return;
            }
            catch (ParsingException ex)
            {
                log.ErrorFormat("Error: {0}", ex.Message);
                return;
            }

            data["Added Associations"].RemoveKey("x-scheme-handler/ckan");
            data["Added Associations"].AddKey("x-scheme-handler/ckan", HandlerFileName);

            parser.WriteFile(MimeAppsListPath, data);

            var handlerPath = Path.Combine(ApplicationsPath, HandlerFileName);
            var handlerDirectory = Path.GetDirectoryName(handlerPath);

            if (handlerDirectory == null || !Directory.Exists(handlerDirectory))
            {
                log.ErrorFormat("Error: {0} doesn't exist", handlerDirectory);
                return;
            }

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


