using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    /// <summary>
    /// ICommand implementation that provides a command that will randomly select mods and install them.
    /// </summary>
    public class Roulette : ICommand
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(Roulette)) // Not currently used.

        public IUser user { get; set; }

        public Roulette(IUser user)
        {
            this.user = user;
        }

        /// <summary>
        /// Runs the Roulette command.
        /// It will generate a list of mods and ask the user if they wish to install them,
        /// it will also ask the user if they are are interested in the mods recommended mods.
        /// </summary>
        /// <remarks>This command uses an instance of the Install command to do the actual install work.</remarks>
        /// <param name="ksp"></param>
        /// <param name="raw_options"></param>
        /// <returns>An integer with the termination result as set in the Exit class.</returns>
        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            RouletteOptions options = (RouletteOptions)raw_options;

            // Make sure we have a positive number of mods selected.
            if (options.number_of_mods < 1)
            {
                user.RaiseError("The number of mods to install must be greater than 0. You entered: {0}", options.number_of_mods);
                user.RaiseError("");
                user.RaiseError("Usage: ckan roulette n");
                user.RaiseError("    Where n is a number larger than 0.");
                user.RaiseError("");
                return Exit.ERROR;
            }

            // Get the list fo available mods from the registry and shuffle it.
            List<CkanModule> available_mods = new List<CkanModule>(ksp.Registry.Available(ksp.Version()));
            RouletteHelp.Shuffle<CkanModule>(available_mods);

            // Keep choosing mods from the list until we have enough.
            int chosen = 0;
            List<CkanModule> to_install = new List<CkanModule>();
            while (chosen < options.number_of_mods) {

                // There is not enough mods left to achieve the wanted number.
                if (available_mods.Count() < 1)
                {
                    user.RaiseError("Unable to get enough mods without creating conflicts. Found {0} non conflicting mods.", to_install.Count());
                    user.RaiseError("");
                    return Exit.ERROR;
                }

                // Take the first mod in the list of available mods.
                CkanModule draw = available_mods[0];
                available_mods.RemoveRange(0, 1);

                // Check if the mods conflicts with any of the previusly selected mods.
                bool conflicts = false;
                foreach (CkanModule mod in to_install)
                {
                    conflicts = conflicts || mod.ConflictsWith(draw);
                    if (conflicts) { break; }
                }
                if (!conflicts)
                {
                    to_install.Add(draw);
                    chosen++;
                }
            }

            // Show the selected mods to the user and ask if they wish to install them.
            user.RaiseMessage("The following mods have been randomly selected:");
            foreach (CkanModule mod in to_install)
            {
                user.RaiseMessage(" * " + mod.name);
            }

            if (!user.RaiseYesNoDialog("Do you want to install these mods?"))
            {
                user.RaiseMessage("Aborting mod installation.");
                return Exit.OK;
            }

            InstallOptions install_options = new InstallOptions();
            install_options.modules = (from mod in to_install select mod.identifier).ToList<String>();

            // Get the recommended mods from the previusly selected mods.
            List<String> recommendations = new List<String>();
            foreach (CkanModule mod in to_install)
            {
                var relations = mod.recommends;
                if (relations != null)
                {
                    foreach (RelationshipDescriptor relation in relations) {
                        if (!recommendations.Contains(relation.name))
                        {
                            recommendations.Add(relation.name);
                        }
                    }
                }
            }

            // If there is any, ask the user if they want to install these as well.
            if (recommendations.Count > 0)
            {
                user.RaiseMessage("The following extra  have been recommended for the above selection:");
                foreach (string recommendation in recommendations)
                {
                    user.RaiseMessage(" * " + recommendation);
                }

                if (user.RaiseYesNoDialog("Do you want to install these mods as well?"))
                {
                    // They did, add them to the install arguments.
                    install_options.modules.AddRange(recommendations);
                }
                else
                {
                    // They didn't add the --no-recommends flag.
                    install_options.no_recommends = true;
                }
            }

            // Run the install command and return the result.
            return (new Install(user)).RunCommand(ksp, install_options);
        }
    }

    /// <summary>
    /// Helper class for the roulette function.
    /// </summary>
    public static class RouletteHelp
    {
        /// <summary>
        /// Use the Fisher-Yates shuffle to shuffle a list.
        /// Code from: http://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp
        /// </summary>
        /// <typeparam name="T">Type the list elements.</typeparam>
        /// <param name="list">List to shuffle.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
