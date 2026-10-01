using System;
using System.Collections.Generic;
using System.Linq;

namespace CKAN.IO
{
    public readonly struct InstallUrlPlan
    {
        public InstallUrlPlan(List<CkanModule> install, List<CkanModule> reinstall)
        {
            Install = install;
            Reinstall = reinstall;
        }

        public readonly List<CkanModule> Install;
        public readonly List<CkanModule> Reinstall;

        public bool Any => Install.Count > 0 || Reinstall.Count > 0;

        public static InstallUrlPlan Nothing()
            => new InstallUrlPlan(new List<CkanModule>(), new List<CkanModule>());
    }

    public static class InstallUrlPrompt
    {
        /// <summary>Tell the user about anything in the install URL that can't be done, and ask about potentially dangerous things.</summary>
        /// <returns>The mods to mark for install and re-install. This method doesn't do anything by itself.</returns>
        public static InstallUrlPlan Confirm(
            List<ResolvedMod> resolved,
            IUser user,
            string gameName,
            string compatibleVersions)
        {
            var install = ModulesWith(resolved, InstallOutcome.Ready, InstallOutcome.PinMissed);
            var installed = ModulesWith(resolved, InstallOutcome.AlreadyInstalled);
            var incompatible = ModulesWith(resolved, InstallOutcome.Incompatible);

            // None of the mods are in the registry. Error out.
            if (install.Count == 0 && installed.Count == 0 && incompatible.Count == 0)
            {
                user.RaiseError(Properties.Resources.InstallUrlNoneFound, Lines(resolved.Select(r => r.Query)), gameName);

                return InstallUrlPlan.Nothing();
            }

            // Tell the user what can't be done at all and ask if we should continue anyway.
            if (!AskToContinue(resolved, user, gameName))
            {
                return InstallUrlPlan.Nothing();
            }

            // There is precedent for installing incompatible mods if the user confirms it.
            if (AskAbout(user, incompatible,
                         Properties.Resources.InstallUrlIncompatible,
                         Properties.Resources.InstallUrlInstall,
                         Properties.Resources.InstallUrlSkip,
                         compatibleVersions))
            {
                install.AddRange(incompatible);
            }

            // Reinstall or skip already installed mods.
            var reinstall = AskAbout(user, installed,
                                     Properties.Resources.InstallUrlAlreadyInstalled,
                                     Properties.Resources.InstallUrlReinstall,
                                     Properties.Resources.InstallUrlSkip);

            return new InstallUrlPlan(install, reinstall ? installed : new List<CkanModule>());
        }

        // One continue/cancel question for everything the link asked for that can't be done as asked.
        private static bool AskToContinue(List<ResolvedMod> resolved, IUser user, string gameName)
        {
            var notes = new List<string>();

            // Not in the registry.
            var unknown = resolved.Where(r => r.Outcome == InstallOutcome.Unknown).Select(r => r.Query).ToList();
            if (unknown.Count > 0)
            {
                notes.Add(string.Format(Properties.Resources.InstallUrlNotFound, Lines(unknown), gameName));
            }

            // Pinned version is missing. We will use the latest instead.
            var pinMissed = ModulesWith(resolved, InstallOutcome.PinMissed);
            if (pinMissed.Count > 0)
            {
                notes.Add(string.Format(Properties.Resources.InstallUrlPinMissed, Lines(pinMissed)));
            }

            if (notes.Count == 0)
            {
                return true; // Nothing wrong, skip the dialog and continue.
            }

            return user.RaiseYesNoDialog(string.Join(Environment.NewLine + Environment.NewLine, notes),
                                         Properties.Resources.InstallUrlContinue,
                                         Properties.Resources.InstallUrlCancel);
        }

        // Ask the user about a group of mods. Skips with implicit false if there are none.
        private static bool AskAbout(IUser user, List<CkanModule> mods, string question,
                                     string yes, string no, string? versions = null)
            => mods.Count > 0 && user.RaiseYesNoDialog(string.Format(question, Lines(mods), versions), yes, no);

        // Get the list of non-null modules for the given outcomes.
        private static List<CkanModule> ModulesWith(List<ResolvedMod> resolved, params InstallOutcome[] outcomes)
            => resolved.Where(r => outcomes.Contains(r.Outcome))
                       .Select(r => r.Module)
                       .OfType<CkanModule>()
                       .ToList();

        private static string Lines<T>(IEnumerable<T> items)
            => string.Join(Environment.NewLine, items);
    }
}
