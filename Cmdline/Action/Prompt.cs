using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using CommandLine;
using CommandLine.Text;
using log4net;

namespace CKAN.CmdLine
{

    public class Prompt
    {
        public Prompt(GameInstanceManager mgr)
        {
            manager = mgr;
        }

        public int RunCommand(object raw_options)
        {
            CommonOptions opts = raw_options as CommonOptions;
            bool headless = opts?.Headless ?? false;
            // Print an intro if not in headless mode
            if (!headless)
            {
                Console.WriteLine(Properties.Resources.PromptWelcome, exitCommand);
            }
            ReadLine.AutoCompletionHandler = GetSuggestions;
            bool done = false;
            while (!done)
            {
                // Prompt if not in headless mode
                if (!headless)
                {
                    Console.Write(
                        manager.CurrentInstance != null
                            ? string.Format(Properties.Resources.PromptWithInstance,
                                Meta.GetVersion(), manager.CurrentInstance.game.ShortName, manager.CurrentInstance.Version(), manager.CurrentInstance.Name)
                            : string.Format(Properties.Resources.PromptWithoutInstance, Meta.GetVersion())
                    );
                }
                try
                {
                    // Get input
                    string command = ReadLineWithCompletion(headless);
                    if (command == null || command == exitCommand)
                    {
                        done = true;
                    }
                    else if (command != "")
                    {
                        // Parse input as if it was a normal command line,
                        // but with a persistent GameInstanceManager object.
                        int cmdExitCode = MainClass.Execute(manager, opts, command.Split(' '));
                        // Clear the command if no exception was thrown
                        if (headless && cmdExitCode != Exit.OK)
                        {
                            // Pass failure codes to calling process in headless mode
                            // (in interactive mode the user can see the error and try again)
                            return cmdExitCode;
                        }
                    }
                }
                catch (NoGameInstanceKraken)
                {
                    Console.WriteLine(Properties.Resources.CompletionNotAvailable);
                }
            }
            return Exit.OK;
        }

        private string ReadLineWithCompletion(bool headless)
        {
            try
            {
                // ReadLine.Read can't read from pipes
                return headless ? Console.ReadLine()
                                : ReadLine.Read("");
            }
            catch (InvalidOperationException)
            {
                // InvalidOperationException is thrown in a mintty on Windows
                return Console.ReadLine();
            }
        }

        private string[] GetSuggestions(string text, int index)
        {
            string[]     pieces = text.Split(new char[] { ' ' });
            TypeInfo     ti     = typeof(Actions).GetTypeInfo();
            List<string> extras = new List<string> { exitCommand, "help" };
            foreach (string piece in pieces.Take(pieces.Length - 1))
            {
                PropertyInfo pi = ti.DeclaredProperties
                    .FirstOrDefault(p => p?.GetCustomAttribute<VerbOptionAttribute>()?.LongName == piece);
                if (pi == null)
                {
                    // Couldn't find it, no suggestions
                    return null;
                }
                ti = pi.PropertyType.GetTypeInfo();
                extras.Clear();
            }
            var lastPiece = pieces.LastOrDefault() ?? "";
            return lastPiece.StartsWith("--") ? GetOptions(ti, lastPiece.Substring(2))
                : HasVerbs(ti)                ? GetVerbs(ti, lastPiece, extras)
                : WantsAvailIdentifiers(ti)   ? GetAvailIdentifiers(lastPiece)
                : WantsInstIdentifiers(ti)    ? GetInstIdentifiers(lastPiece)
                : WantsGameInstances(ti)      ? GetGameInstances(lastPiece)
                :                               null;
        }

        private string[] GetOptions(System.Reflection.TypeInfo ti, string prefix)
        {
            return ti.DeclaredProperties
                .Select(p => p.GetCustomAttribute<OptionAttribute>()?.LongName)
                .Where(o => o != null && o.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(o => o)
                .Select(o => $"--{o}")
                .ToArray();
        }

        private bool HasVerbs(TypeInfo ti)
        {
            return ti.DeclaredProperties
                .Any(p => p.GetCustomAttribute<VerbOptionAttribute>() != null);
        }

        private string[] GetVerbs(TypeInfo ti, string prefix, IEnumerable<string> extras)
        {
            return ti.DeclaredProperties
                .Select(p => p.GetCustomAttribute<VerbOptionAttribute>()?.LongName)
                .Where(v => v != null)
                .Concat(extras)
                .Where(v => v.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(v => v)
                .ToArray();
        }

        private bool WantsAvailIdentifiers(TypeInfo ti)
        {
            return ti.DeclaredProperties
                .Any(p => p.GetCustomAttribute<AvailableIdentifiersAttribute>() != null);
        }

        private string[] GetAvailIdentifiers(string prefix)
        {
            CKAN.GameInstance inst = MainClass.GetGameInstance(manager);
            return RegistryManager.Instance(inst).registry
                .CompatibleModules(inst.VersionCriteria())
                .Where(m => !m.IsDLC)
                .Select(m => m.identifier)
                .Where(ident => ident.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
        }

        private bool WantsInstIdentifiers(TypeInfo ti)
        {
            return ti.DeclaredProperties
                .Any(p => p.GetCustomAttribute<InstalledIdentifiersAttribute>() != null);
        }

        private string[] GetInstIdentifiers(string prefix)
        {
            CKAN.GameInstance inst = MainClass.GetGameInstance(manager);
            var registry = RegistryManager.Instance(inst).registry;
            return registry.Installed(false, false)
                .Select(kvp => kvp.Key)
                .Where(ident => ident.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)
                    && !registry.GetInstalledVersion(ident).IsDLC)
                .ToArray();
        }

        private bool WantsGameInstances(TypeInfo ti)
        {
            return ti.DeclaredProperties
                .Any(p => p.GetCustomAttribute<GameInstancesAttribute>() != null);
        }

        private string[] GetGameInstances(string prefix)
        {
            return manager.Instances
                .Select(kvp => kvp.Key)
                .Where(ident => ident.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
        }

        private readonly GameInstanceManager manager;
        private const string exitCommand = "exit";
    }

}
