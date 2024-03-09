using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CommandLine;

namespace CKAN.CmdLine
{

    public class Prompt
    {
        public Prompt(GameInstanceManager mgr, RepositoryDataManager repoData)
        {
            manager = mgr;
            this.repoData = repoData;
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
                        int cmdExitCode = MainClass.Execute(manager, opts,
                                                            ParseTextField(command));
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

        /// <summary>
        /// Split string on spaces, unless they are between quotes.
        /// Inspired by https://stackoverflow.com/a/14655145/2422988
        /// </summary>
        /// <param name="input">The string to parse</param>
        /// <returns>Array split by strings, with quoted parts joined together</returns>
        private static string[] ParseTextField(string input)
            => quotePattern.Matches(input)
                           .Cast<Match>()
                           .Select(m => m.Value)
                           .ToArray();

        /// <summary>
        /// Look for non-quotes surrounded by quotes, or non-space-or-quotes, or end preceded by space.
        /// No attempt to allow escaped quotes within quotes.
        /// Inspired by https://stackoverflow.com/a/14655145/2422988
        /// </summary>
        private static readonly Regex quotePattern = new Regex(
            @"(?<="")[^""]*(?="")|[^ ""]+|(?<= )$", RegexOptions.Compiled);

        private static string ReadLineWithCompletion(bool headless)
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
            string[]     pieces = ParseTextField(text);
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
            return lastPiece.StartsWith("--") ? GetLongOptions(ti, lastPiece.Substring(2))
                 : lastPiece.StartsWith("-")  ? GetShortOptions(ti, lastPiece.Substring(1))
                 : HasVerbs(ti)               ? GetVerbs(ti, lastPiece, extras)
                 : WantsAvailIdentifiers(ti)  ? GetAvailIdentifiers(lastPiece)
                 : WantsInstIdentifiers(ti)   ? GetInstIdentifiers(lastPiece)
                 : WantsGameInstances(ti)     ? GetGameInstances(lastPiece)
                 :                              null;
        }

        private static string[] GetLongOptions(TypeInfo ti, string prefix)
            => AllBaseTypes(ti.AsType())
                .SelectMany(t => t.GetTypeInfo().DeclaredProperties)
                .Select(p => p.GetCustomAttribute<OptionAttribute>()?.LongName
                             ?? p.GetCustomAttribute<OptionArrayAttribute>()?.LongName
                             ?? p.GetCustomAttribute<OptionListAttribute>()?.LongName)
                .Where(o => o != null && o.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(o => o)
                .Select(o => $"--{o}")
                .ToArray();

        private static string[] GetShortOptions(TypeInfo ti, string prefix)
            => AllBaseTypes(ti.AsType())
                .SelectMany(t => t.GetTypeInfo().DeclaredProperties)
                .Select(p => p.GetCustomAttribute<OptionAttribute>()?.ShortName
                             ?? p.GetCustomAttribute<OptionArrayAttribute>()?.ShortName
                             ?? p.GetCustomAttribute<OptionListAttribute>()?.ShortName)
                .Where(o => o != null && $"{o}".StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(o => o)
                .Select(o => $"-{o}")
                .ToArray();

        private static IEnumerable<Type> AllBaseTypes(Type start)
        {
            for (Type t = start; t != null; t = t.BaseType)
            {
                yield return t;
            }
        }

        private static bool HasVerbs(TypeInfo ti)
            => ti.DeclaredProperties
                 .Any(p => p.GetCustomAttribute<VerbOptionAttribute>() != null);

        private static string[] GetVerbs(TypeInfo ti, string prefix, IEnumerable<string> extras)
            => ti.DeclaredProperties
                 .Select(p => p.GetCustomAttribute<VerbOptionAttribute>()?.LongName)
                 .Where(v => v != null)
                 .Concat(extras)
                 .Where(v => v.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                 .OrderBy(v => v)
                 .ToArray();

        private static bool WantsAvailIdentifiers(TypeInfo ti)
            => ti.DeclaredProperties
                 .Any(p => p.GetCustomAttribute<AvailableIdentifiersAttribute>() != null);

        private string[] GetAvailIdentifiers(string prefix)
        {
            CKAN.GameInstance inst = MainClass.GetGameInstance(manager);
            return RegistryManager.Instance(inst, repoData)
                                  .registry
                                  .CompatibleModules(inst.VersionCriteria())
                                  .Where(m => !m.IsDLC)
                                  .Select(m => m.identifier)
                                  .Where(ident => ident.StartsWith(prefix,
                                                                   StringComparison.InvariantCultureIgnoreCase))
                                  .ToArray();
        }

        private static bool WantsInstIdentifiers(TypeInfo ti)
            => ti.DeclaredProperties
                 .Any(p => p.GetCustomAttribute<InstalledIdentifiersAttribute>() != null);

        private string[] GetInstIdentifiers(string prefix)
        {
            CKAN.GameInstance inst = MainClass.GetGameInstance(manager);
            var registry = RegistryManager.Instance(inst, repoData).registry;
            return registry.Installed(false, false)
                           .Keys
                           .Where(ident => ident.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)
                                           && !registry.GetInstalledVersion(ident).IsDLC)
                           .ToArray();
        }

        private static bool WantsGameInstances(TypeInfo ti)
            => ti.DeclaredProperties
                 .Any(p => p.GetCustomAttribute<GameInstancesAttribute>() != null);

        private string[] GetGameInstances(string prefix)
            => manager.Instances
                      .Keys
                      .Where(ident => ident.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                      .ToArray();

        private readonly GameInstanceManager   manager;
        private readonly RepositoryDataManager repoData;
        private const    string                exitCommand = "exit";
    }

}
