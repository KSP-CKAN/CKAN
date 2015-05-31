﻿using System;
using System.Diagnostics;
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
        public const string UrlRegistrationArgument = "registerUrl";

        public static void RegisterURLHandler(Configuration config, IUser user)
        {
            try
            {
                if (Platform.IsUnix)
                {
                    RegisterURLHandler_Linux();
                }
                else if(Platform.IsWindows)
                {
                    try
                    {
                       RegisterURLHandler_Win32();
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        if (config.URLHandlerNoNag)
                        {
                            return;
                        }

                        if (user.RaiseYesNoDialog(@"CKAN requires permission to add a handler for ckan:// URLs.
Do you want to allow CKAN to do this? If you click no you won't see this message again."))
                        {
                            // we need elevation to write to the registry
                            ProcessStartInfo startInfo = new ProcessStartInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                            startInfo.Verb = "runas"; // trigger a UAC prompt (if UAC is enabled)
                            startInfo.Arguments = "gui " + UrlRegistrationArgument;
                            Process.Start(startInfo);
                        }
                        else
                        {
                            config.URLHandlerNoNag = true;
                            config.Save();
                        }
                        
                        throw;
                    }
                } else if (Platform.IsMac) {
                    //TODO
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("There was an error while registering the URL handler for ckan:// - {0}", ex.Message);
            }
        }

        private static void RegisterURLHandler_Win32()
        {
            log.InfoFormat("Adding URL handler to registry");

            var root = Microsoft.Win32.Registry.ClassesRoot;

            if (root.OpenSubKey("ckan") != null)
            {
                try
                {
                    var path =
                        (string)root.OpenSubKey("ckan")
                            .OpenSubKey("shell")
                            .OpenSubKey("open")
                            .OpenSubKey("command")
                            .GetValue("");

                    if (path == (System.Reflection.Assembly.GetExecutingAssembly().Location + " gui %1"))
                    {
                        log.InfoFormat("URL handler already exists with the same path");
                        return;
                    }
                }
                catch (Exception)
                {
                }

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
                log.WarnFormat("Skipping URL handler: {0}", ex.Message);
                return;
            }
            catch (FileNotFoundException ex)
            {
                log.WarnFormat("Skipping URL handler: {0}", ex.Message);
                return;
            }
            catch (ParsingException ex)
            {
                log.WarnFormat("Skipping URL handler: {0}", ex.Message);
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


