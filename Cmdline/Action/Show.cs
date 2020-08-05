using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for showing information about a mod.
    /// </summary>
    public class Show : ICommand
    {
        private readonly IUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.Action.Show"/> class.
        /// </summary>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        public Show(IUser user)
        {
            _user = user;
        }

        /// <summary>
        /// Run the 'show' command.
        /// </summary>
        /// <inheritdoc cref="ICommand.RunCommand"/>
        public int RunCommand(CKAN.GameInstance inst, object args)
        {
            var opts = (ShowOptions)args;
            if (string.IsNullOrWhiteSpace(opts.ModName))
            {
                _user.RaiseMessage("show <mod> - argument missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            // Check installed modules for an exact match
            var registry = RegistryManager.Instance(inst).registry;
            var installedModuleToShow = registry.InstalledModule(opts.ModName);

            if (installedModuleToShow != null)
            {
                // Show the installed module
                return ShowMod(installedModuleToShow);
            }

            // Module was not installed, look for an exact match in the available modules,
            // either by "name" (the user-friendly display name) or by identifier
            var moduleToShow = registry
                .CompatibleModules(inst.VersionCriteria())
                .SingleOrDefault(
                    mod => mod.name == opts.ModName
                           || mod.identifier == opts.ModName
                );

            if (moduleToShow == null)
            {
                // No exact match found. Try to look for a close match for this game version
                _user.RaiseMessage("\"{0}\" was not found or installed.\r\nLooking for close matches in mods compatible with {1} {2}.", opts.ModName, inst.game.ShortName, inst.Version());

                var search = new Search(_user);
                var matches = search.PerformSearch(inst, opts.ModName);

                // Display the results of the search
                if (!matches.Any())
                {
                    // No matches found
                    _user.RaiseMessage("No close matches found.");
                    return Exit.BadOpt;
                }

                if (matches.Count == 1)
                {
                    // If there is only 1 match, display it
                    _user.RaiseMessage("Found 1 close match: \"{0}\".\r\n", matches[0].name);
                    moduleToShow = matches[0];
                }
                else
                {
                    // Display the found close matches
                    var stringsMatches = new string[matches.Count];

                    for (var i = 0; i < matches.Count; i++)
                    {
                        stringsMatches[i] = matches[i].name;
                    }

                    var selection = _user.RaiseSelectionDialog("Close matches", stringsMatches);

                    if (selection < 0)
                    {
                        return Exit.BadOpt;
                    }

                    // Mark the selection as the one to show
                    moduleToShow = matches[selection];
                }
            }

            return ShowMod(moduleToShow);
        }

        private int ShowMod(InstalledModule module)
        {
            // Display the basic info
            var returnValue = ShowMod(module.Module);

            // Display InstalledModule specific information
            if (!(module.Files is ICollection<string> files))
            {
                throw new InvalidCastException();
            }

            if (!module.Module.IsDLC)
            {
                _user.RaiseMessage("\r\nShowing {0} installed files:", files.Count);
                foreach (var file in files)
                {
                    _user.RaiseMessage("- {0}", file);
                }
            }

            return returnValue;
        }

        private int ShowMod(CkanModule module)
        {
            // Abstract and description

            if (!string.IsNullOrEmpty(module.@abstract))
            {
                _user.RaiseMessage("{0}: {1}", module.name, module.@abstract);
            }
            else
            {
                _user.RaiseMessage("{0}", module.name);
            }

            if (!string.IsNullOrEmpty(module.description))
            {
                _user.RaiseMessage("\r\n{0}\r\n", module.description);
            }

            // General info (author, version...)

            _user.RaiseMessage("\r\nModule info:\r\n- version:\t{0}", module.version);

            if (module.author != null)
            {
                _user.RaiseMessage("- authors:\t{0}", string.Join(", ", module.author));
            }
            else
            {
                // Did you know that authors are optional in the spec?
                // You do now. #673
                _user.RaiseMessage("- authors:\tUNKNOWN");
            }

            _user.RaiseMessage("- status:\t{0}", module.release_status);
            _user.RaiseMessage("- license:\t{0}", string.Join(", ", module.license));

            // Relationships

            if (module.depends != null && module.depends.Count > 0)
            {
                _user.RaiseMessage("\r\nDepends:");
                foreach (var dep in module.depends)
                {
                    _user.RaiseMessage("- {0}", RelationshipToPrintableString(dep));
                }
            }

            if (module.recommends != null && module.recommends.Count > 0)
            {
                _user.RaiseMessage("\r\nRecommends:");
                foreach (var dep in module.recommends)
                {
                    _user.RaiseMessage("- {0}", RelationshipToPrintableString(dep));
                }
            }

            if (module.suggests != null && module.suggests.Count > 0)
            {
                _user.RaiseMessage("\r\nSuggests:");
                foreach (var dep in module.suggests)
                {
                    _user.RaiseMessage("- {0}", RelationshipToPrintableString(dep));
                }
            }

            if (module.ProvidesList != null && module.ProvidesList.Count > 0)
            {
                _user.RaiseMessage("\r\nProvides:");
                foreach (var prov in module.ProvidesList)
                {
                    _user.RaiseMessage("- {0}", prov);
                }
            }

            _user.RaiseMessage("\r\nResources:");

            if (module.resources != null)
            {
                if (module.resources.bugtracker != null)
                {
                    _user.RaiseMessage("- bugtracker: {0}", Uri.EscapeUriString(module.resources.bugtracker.ToString()));
                }

                if (module.resources.homepage != null)
                {
                    _user.RaiseMessage("- homepage: {0}", Uri.EscapeUriString(module.resources.homepage.ToString()));
                }

                if (module.resources.spacedock != null)
                {
                    _user.RaiseMessage("- spacedock: {0}", Uri.EscapeUriString(module.resources.spacedock.ToString()));
                }

                if (module.resources.repository != null)
                {
                    _user.RaiseMessage("- repository: {0}", Uri.EscapeUriString(module.resources.repository.ToString()));
                }

                if (module.resources.curse != null)
                {
                    _user.RaiseMessage("- curse: {0}", Uri.EscapeUriString(module.resources.curse.ToString()));
                }

                if (module.resources.store != null)
                {
                    _user.RaiseMessage("- store: {0}", Uri.EscapeUriString(module.resources.store.ToString()));
                }

                if (module.resources.steamstore != null)
                {
                    _user.RaiseMessage("- steamstore: {0}", Uri.EscapeUriString(module.resources.steamstore.ToString()));
                }
            }

            if (!module.IsDLC)
            {
                // Compute the CKAN filename
                var fileUriHash = NetFileCache.CreateURLHash(module.download);
                var fileName = CkanModule.StandardName(module.identifier, module.version);

                _user.RaiseMessage("\r\nFilename: {0}", fileUriHash + "-" + fileName);
            }

            return Exit.Ok;
        }

        private static string RelationshipToPrintableString(RelationshipDescriptor dep)
        {
            var sb = new StringBuilder();
            sb.Append(dep);
            return sb.ToString();
        }
    }

    [Verb("show", HelpText = "Show information about a mod")]
    internal class ShowOptions : InstanceSpecificOptions
    {
        [Value(0, MetaName = "Mod name", HelpText = "The mod name to show information about")]
        public string ModName { get; set; }
    }
}
