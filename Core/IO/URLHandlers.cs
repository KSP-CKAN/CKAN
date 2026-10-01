using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using log4net;

namespace CKAN.IO
{
    [ExcludeFromCodeCoverage]
    public static class URLHandlers
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(URLHandlers));

        private static readonly string ApplicationsPath = ".local/share/applications/";
        private const string LinuxHandlerFilename = "ckan-handler.desktop";
        private const string LinuxConsoleUIFilename = "ckan-url-consoleui.desktop";

        // WindowsHandlerResName must match the LogicalName in CKAN-cmdline.csproj.
        private const string WindowsHandlerResName = "CKAN.CmdLine.ckan-urlhandler.exe";
        private const string WindowsHandlerExeName = "ckan-urlhandler.exe";

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

        // uiCommand is the verb of the calling UI (gui or consoleui). A cold ckan:// click
        // launches that UI, so whichever one registered last is the one that links open.
        public static void RegisterURLHandler(string uiCommand)
        {
            try
            {
                if (Platform.IsUnix)
                {
                    RegisterURLHandler_Linux(uiCommand);
                }
                else if (Platform.IsWindows)
                {
                    RegisterURLHandler_Win32(uiCommand);
                }

                // todo: macOS URL handler is defined in macosx/Info.plist.in but commented out until we can receive the apple event.
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
        private static void RegisterURLHandler_Win32(string uiCommand)
        {
            log.InfoFormat("Adding URL handler to registry");

            var stub = ExtractURLHandlerStub();
            if (stub == null)
            {
                return;
            }

            // `ckan-urlhandler.exe <path to ckan.exe> <verb> <pipe name> <url>`
            var urlCmd = $"\"{stub}\" \"{PathToRunningExe()}\" {uiCommand} \"{URLPipe.Name}\" \"%1\"";

            // Register per user so no admin rights are needed.
            // Windows automatically gives this precedence over the old handler we used to register for all users.
            using var classes = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes");
            var existing = classes.OpenSubKey(@"ckan\shell\open\command")?.GetValue("")?.ToString();
            if (existing == urlCmd)
            {
                log.InfoFormat("URL handler already registered with the same command");
                return;
            }

            using var ckanKey = classes.CreateSubKey("ckan");
            ckanKey.SetValue("", "URL: ckan Protocol");
            ckanKey.SetValue("URL Protocol", "");

            using var commandKey = ckanKey.CreateSubKey(@"shell\open\command");
            commandKey.SetValue("", urlCmd);
        }

        // Extract the embedded URL handler to %LOCALAPPDATA%\CKAN and return its path.
        // Returns null if the stub is not embedded.
        private static string? ExtractURLHandlerStub()
        {
            using var resource = Assembly.GetEntryAssembly()?.GetManifestResourceStream(WindowsHandlerResName);
            if (resource == null)
            {
                log.InfoFormat("URL handler stub '{0}' not embedded. Skipping URL handler registration",
                               WindowsHandlerResName);
                return null;
            }

            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CKAN");
            Directory.CreateDirectory(dir);
            var stubPath = Path.Combine(dir, WindowsHandlerExeName);

            using var ms = new MemoryStream();
            resource.CopyTo(ms);
            var wanted = ms.ToArray();

            // Write when non-existent or the bytes differ.
            if (!File.Exists(stubPath) || !File.ReadAllBytes(stubPath).SequenceEqual(wanted))
            {
                File.WriteAllBytes(stubPath, wanted);
            }

            return stubPath;
        }

        #if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
        #endif
        private static void RegisterURLHandler_Linux(string uiCommand)
        {
            log.InfoFormat("Trying to register URL handler");

            string handlerExec = $"mono \"{PathToRunningExe()}\" {uiCommand} --url %u";
            string handlerPath = Path.Combine(ApplicationsPath, LinuxHandlerFilename);

            // Terminal=false even for the console UI because a Terminal=true handler would flash a terminal on warm handoff.
            // Console UI cold start gets relaunched instead. See RelaunchConsoleUIInTerminal.
            if (WriteDesktopEntry(handlerPath, DesktopEntry("CKAN Launcher", handlerExec, terminal: false, scheme: true)))
            {
                RunCommand("xdg-mime", $"default {LinuxHandlerFilename} x-scheme-handler/ckan");
                RunCommand("update-desktop-database", ApplicationsPath);
            }
        }

        private static string DesktopEntry(string name, string exec, bool terminal, bool scheme)
        {
            var sb = new StringBuilder()
                .AppendLine("[Desktop Entry]")
                .AppendLine("Version=1.0")
                .AppendLine("Type=Application")
                .AppendLine($"Exec={exec}")
                .AppendLine("Icon=ckan")
                .AppendLine("StartupNotify=true")
                .AppendLine("NoDisplay=true")
                .AppendLine($"Terminal={(terminal ? "true" : "false")}")
                .AppendLine("Categories=Utility");

            if (scheme)
            {
                sb.AppendLine("MimeType=x-scheme-handler/ckan");
            }

            return sb.AppendLine($"Name={name}")
                     .AppendLine("Comment=Launch CKAN")
                     .ToString();
        }

        private static bool WriteDesktopEntry(string path, string content)
        {
            if (File.Exists(path) && File.ReadAllText(path) == content)
            {
                log.InfoFormat("Desktop file {0} is already up to date", path);

                return false;
            }

            log.InfoFormat("Writing desktop file to {0}", path);

            // Write without a Byte Order Mark. update-desktop-database errors on BOM-prefixed files.
            File.WriteAllText(path, content, new UTF8Encoding(false));
            AutoUpdate.SetExecutable(path);

            return true;
        }

        // We have no terminal to draw in, so write an entry that does have one and launch that.
        public static bool RelaunchConsoleUIInTerminal(string url)
        {
            // We know this is the cold case. --no-handoff skips retrying the pipe.
            string command = $"mono \"{PathToRunningExe()}\" consoleui --url %u --no-handoff";
            string entry = DesktopEntry("CKAN Console UI", command, terminal: true, scheme: false);
            string path = Path.Combine(ApplicationsPath, LinuxConsoleUIFilename);
            WriteDesktopEntry(path, entry);

            // gio launch opens the user's terminal without us hardcoding anything.
            // gio is from glib2 so it is practically always available.
            if (RunCommand("gio", $"launch \"{path}\" \"{url}\"", redirect: false))
            {
                return true;
            }

            log.Error("Could not relaunch the console UI in a terminal. Is gio (glib2) installed?");

            return false;
        }

        // Returns true if the command ran and succeeded.
        private static bool RunCommand(string command, string args, bool redirect = true)
        {
            try
            {
                log.InfoFormat("Running {0} {1}", command, args);
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName               = command,
                    Arguments              = args,
                    UseShellExecute        = false,
                    RedirectStandardError  = redirect,
                    RedirectStandardOutput = redirect,
                });
                if (process != null)
                {
                    var stderr = redirect ? process.StandardError.ReadToEnd() : "";
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        log.WarnFormat("{0} exited with code {1}: {2}",
                                       command, process.ExitCode, stderr);

                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                // xdg-mime and update-desktop-database are not guaranteed to be on all systems.
                log.WarnFormat("Could not run {0}: {1}", command, ex.Message);
            }

            return false;
        }
    }
}
