using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using log4net;

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public static class URLHandlers
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(URLHandlers));
        public  const  string UrlRegistrationArgument = "registerUrl";

        private static readonly string MimeAppsListPath = "mimeapps.list";
        private static readonly string ApplicationsPath = ".local/share/applications/";
        private const           string HandlerFileName  = "ckan-handler.desktop";

        static URLHandlers()
        {
            if (Platform.IsUnix)
            {
                string XDGDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (XDGDataHome != null)
                {
                    ApplicationsPath = Path.Combine(XDGDataHome, "applications");
                }
                else
                {
                    string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    ApplicationsPath = Path.Combine(home, ApplicationsPath);
                }
                Directory.CreateDirectory(ApplicationsPath);
                MimeAppsListPath = Path.Combine(ApplicationsPath, MimeAppsListPath);
            }
        }

        public static void RegisterURLHandler(GUIConfiguration config, IUser user)
        {
            try
            {
                if (Platform.IsUnix)
                {
                    RegisterURLHandler_Linux();
                }
                else if (Platform.IsWindows)
                {
                    try
                    {
                        RegisterURLHandler_Win32();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (config.URLHandlerNoNag)
                        {
                            return;
                        }

                        if (user.RaiseYesNoDialog(Properties.Resources.URLHandlersPrompt))
                        {
                            // we need elevation to write to the registry
                            Process.Start(new ProcessStartInfo(Assembly.GetEntryAssembly().Location)
                            {
                                // trigger a UAC prompt (if UAC is enabled)
                                Verb      = "runas",
                                // .NET ignores Verb without this
                                UseShellExecute = true,
                                Arguments = $"gui --asroot {UrlRegistrationArgument}"
                            });
                        }
                        config.URLHandlerNoNag = true;
                        config.Save();
                        // Don't re-throw the exception because we just dealt with it
                    }
                }
                else if (Platform.IsMac)
                {
                    //TODO
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(
                    "There was an error while registering the URL handler for ckan:// - {0}",
                    ex.Message
                );
                log.ErrorFormat("{0}", ex.StackTrace);
            }
        }

        #if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
        #endif
        private static void RegisterURLHandler_Win32()
        {
            log.InfoFormat("Adding URL handler to registry");
            string      urlCmd  = $"{Assembly.GetExecutingAssembly().Location} gui %1";
            RegistryKey root    = Microsoft.Win32.Registry.ClassesRoot;
            RegistryKey ckanKey = root.OpenSubKey("ckan");
            if (ckanKey != null)
            {
                try
                {
                    string path = ckanKey.OpenSubKey("shell")
                        .OpenSubKey("open")
                        .OpenSubKey("command")
                        .GetValue("")
                        .ToString();

                    if (path == urlCmd)
                    {
                        log.InfoFormat("URL handler already exists with the same path");
                        return;
                    }
                    // Valid key not found, delete it
                    root.DeleteSubKeyTree("ckan");
                }
                catch (Exception) { }
            }
            ckanKey = root.CreateSubKey("ckan");
            ckanKey.SetValue("", "URL: ckan Protocol");
            ckanKey.SetValue("URL Protocol", "");
            ckanKey.CreateSubKey("shell")
                .CreateSubKey("open")
                .CreateSubKey("command")
                .SetValue("", urlCmd);
        }

        #if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
        #endif
        private static void RegisterURLHandler_Linux()
        {
            var parser = new FileIniDataParser();

            // Yes, 'Assigment' is the spelling used by the library.
            parser.Parser.Configuration.AssigmentSpacer = "";
            IniData data;

            log.InfoFormat("Trying to register URL handler");

            if (!File.Exists(MimeAppsListPath))
            {
                log.InfoFormat("{0} does not exist, trying to create it", MimeAppsListPath);
                File.WriteAllLines(MimeAppsListPath, new string[] { "[Default Applications]" });
            }

            try
            {
                data = parser.ReadFile(MimeAppsListPath);
            }
            catch (DirectoryNotFoundException ex)
            {
                log.InfoFormat("Skipping URL handler: {0}", ex.Message);
                return;
            }
            catch (FileNotFoundException ex)
            {
                log.InfoFormat("Skipping URL handler: {0}", ex.Message);
                return;
            }
            catch (ParsingException ex)
            {
                log.InfoFormat("Skipping URL handler: {0}", ex.Message);
                return;
            }

            if (data["Added Associations"] == null)
            {
                data.Sections.AddSection("Added Associations");
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
            data["Desktop Entry"].AddKey("Exec", "mono \"" + Assembly.GetExecutingAssembly().Location + "\" gui %u");
            data["Desktop Entry"].AddKey("Icon", "ckan");
            data["Desktop Entry"].AddKey("StartupNotify", "true");
            data["Desktop Entry"].AddKey("NoDisplay", "true");
            data["Desktop Entry"].AddKey("Terminal", "false");
            data["Desktop Entry"].AddKey("Categories", "Utility");
            data["Desktop Entry"].AddKey("MimeType", "x-scheme-handler/ckan");
            data["Desktop Entry"].AddKey("Name", "CKAN Launcher");
            data["Desktop Entry"].AddKey("Comment", "Launch CKAN");

            parser.WriteFile(handlerPath, data);
            AutoUpdate.SetExecutable(handlerPath);
        }
    }
}
