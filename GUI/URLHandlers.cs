using System;
using System.Diagnostics;
using System.IO;
using System.Text;
#if !NET5_0_OR_GREATER
using System.Reflection;
#endif
using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using log4net;

namespace CKAN.GUI
{
    [ExcludeFromCodeCoverage]
    public static class URLHandlers
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(URLHandlers));
        public  const  string UrlRegistrationArgument = "registerUrl";

        private static readonly string ApplicationsPath = ".local/share/applications/";
        private const           string HandlerFileName  = "ckan-handler.desktop";

        static URLHandlers()
        {
            if (Platform.IsUnix)
            {
                var XDGDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
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
            }
        }

        public static void RegisterURLHandler(GUIConfiguration? config, GameInstance? instance, IUser? user)
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
                        if (config == null || config.URLHandlerNoNag || instance == null)
                        {
                            return;
                        }

                        if (user != null
                            && user.RaiseYesNoDialog(Properties.Resources.URLHandlersPrompt))
                        {
                            // we need elevation to write to the registry
                            Process.Start(new ProcessStartInfo(PathToRunningExe())
                            {
                                // trigger a UAC prompt (if UAC is enabled)
                                Verb      = "runas",
                                // .NET ignores Verb without this
                                UseShellExecute = true,
                                Arguments = $"gui --asroot {UrlRegistrationArgument}"
                            });
                        }
                        config.URLHandlerNoNag = true;
                        config.Save(instance);
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

        private static string PathToRunningExe()
            #if NET5_0_OR_GREATER
            => Environment.ProcessPath ?? "";
            #else
            => Assembly.GetEntryAssembly()?.Location ?? "";
            #endif

        #if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
        #endif
        private static void RegisterURLHandler_Win32()
        {
            log.InfoFormat("Adding URL handler to registry");
            string      urlCmd = $"{PathToRunningExe()} gui %1";
            RegistryKey root   = Microsoft.Win32.Registry.ClassesRoot;
            var ckanKey = root.OpenSubKey("ckan");
            if (ckanKey != null)
            {
                try
                {
                    var path = ckanKey?.OpenSubKey("shell")
                                      ?.OpenSubKey("open")
                                      ?.OpenSubKey("command")
                                      ?.GetValue("")
                                      ?.ToString();

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
            log.InfoFormat("Trying to register URL handler");

            var handlerPath = Path.Combine(ApplicationsPath, HandlerFileName);
            var desiredExec = "mono \"" + PathToRunningExe() + "\" gui %u";

            var desiredContent = new StringBuilder()
                .AppendLine("[Desktop Entry]")
                .AppendLine("Version=1.0")
                .AppendLine("Type=Application")
                .AppendLine($"Exec={desiredExec}")
                .AppendLine("Icon=ckan")
                .AppendLine("StartupNotify=true")
                .AppendLine("NoDisplay=true")
                .AppendLine("Terminal=false")
                .AppendLine("Categories=Utility")
                .AppendLine("MimeType=x-scheme-handler/ckan")
                .AppendLine("Name=CKAN Launcher")
                .AppendLine("Comment=Launch CKAN")
                .ToString();

            var existingContent = File.Exists(handlerPath)
                ? File.ReadAllText(handlerPath)
                : null;

            if (existingContent != desiredContent)
            {
                log.InfoFormat("Writing URL handler desktop file to {0}", handlerPath);

                // Write without a Byte Order Mark. update-desktop-database errors on BOM-prefixed files.
                File.WriteAllText(handlerPath, desiredContent, new UTF8Encoding(false));
                AutoUpdate.SetExecutable(handlerPath);

                RunCommand("xdg-mime", $"default {HandlerFileName} x-scheme-handler/ckan");
                RunCommand("update-desktop-database", ApplicationsPath);
            }
            else
            {
                log.InfoFormat("URL handler desktop file is already up to date");
            }
        }

        private static void RunCommand(string command, string args)
        {
            try
            {
                log.InfoFormat("Running {0} {1}", command, args);
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                });
                process?.WaitForExit();
            }
            catch (Exception ex)
            {
                // xdg-mime and update-desktop-database are not guaranteed to be on all systems.
                log.WarnFormat("Could not run {0}: {1}", command, ex.Message);
            }
        }
    }
}