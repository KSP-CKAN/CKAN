using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    public class Show : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Show));

        public IUser user { get; set; }

        public Show(IUser user)
        {
            this.user = user;
        }

        // TODO: We should have a command (probably this one) that shows
        // info about uninstalled modules.
        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            ShowOptions options = (ShowOptions) raw_options;

            if (options.Modname == null)
            {
                // empty argument
                user.RaiseMessage("show <module> - module name argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            RegistryManager registry_manager = RegistryManager.Instance(ksp);
            InstalledModule module = registry_manager.registry.InstalledModule(options.Modname);

            if (module == null)
            {
                user.RaiseMessage("{0} not installed.", options.Modname);
                user.RaiseMessage("Try `ckan list` to show installed modules");
                return Exit.BADOPT;
            }

            #region Abstract and description
            if (!string.IsNullOrEmpty(module.Module.@abstract))
                user.RaiseMessage("{0}: {1}", module.Module.name, module.Module.@abstract);
            else
                user.RaiseMessage("{0}", module.Module.name);

            if (!string.IsNullOrEmpty(module.Module.description))
                user.RaiseMessage("\n{0}\n", module.Module.description);
            #endregion

            #region General info (author, version...)
            user.RaiseMessage("\nModule info:");
            user.RaiseMessage("- version:\t{0}", module.Module.version);

            if (module.Module.author != null)
            {
                user.RaiseMessage("- authors:\t{0}", string.Join(", ", module.Module.author));
            }
            else
            {
                // Did you know that authors are optional in the spec?
                // You do now. #673.
                user.RaiseMessage("- authors:\tUNKNOWN");
            }

            user.RaiseMessage("- status:\t{0}", module.Module.release_status);
            user.RaiseMessage("- license:\t{0}", module.Module.license); 
            #endregion

            #region Relationships
            if (module.Module.depends != null && module.Module.depends.Count > 0)
            {
                user.RaiseMessage("\nDepends:");
                foreach (RelationshipDescriptor dep in module.Module.depends)
                    user.RaiseMessage("- {0}", RelationshipToPrintableString(dep));
            }

            if (module.Module.recommends != null && module.Module.recommends.Count > 0)
            {
                user.RaiseMessage("\nRecommends:");
                foreach (RelationshipDescriptor dep in module.Module.recommends)
                    user.RaiseMessage("- {0}", RelationshipToPrintableString(dep));
            }

            if (module.Module.suggests != null && module.Module.suggests.Count > 0)
            {
                user.RaiseMessage("\nSuggests:");
                foreach (RelationshipDescriptor dep in module.Module.suggests)
                    user.RaiseMessage("- {0}", RelationshipToPrintableString(dep));
            }

            if (module.Module.ProvidesList != null && module.Module.ProvidesList.Count > 0)
            {
                user.RaiseMessage("\nProvides:");
                foreach (string prov in module.Module.ProvidesList)
                    user.RaiseMessage("- {0}", prov);
            } 
            #endregion

            user.RaiseMessage("\nResources:");
            if (module.Module.resources != null)
            {
                if (module.Module.resources.bugtracker != null)
                    user.RaiseMessage("- bugtracker: {0}", module.Module.resources.bugtracker.ToString());
                if (module.Module.resources.homepage != null)
                    user.RaiseMessage("- homepage: {0}", module.Module.resources.homepage.ToString());
                if (module.Module.resources.kerbalstuff != null)
                    user.RaiseMessage("- kerbalstuff: {0}", module.Module.resources.kerbalstuff.ToString());
                if (module.Module.resources.repository != null)
                    user.RaiseMessage("- repository: {0}", module.Module.resources.repository.ToString());
            }
            

            ICollection<string> files = module.Files as ICollection<string>;
            if (files == null) throw new InvalidCastException();

            user.RaiseMessage("\nShowing {0} installed files:", files.Count);
            foreach (string file in files)
            {
                user.RaiseMessage("- {0}", file);
            }

            return Exit.OK;
        }

        /// <summary>
        /// Formats a RelationshipDescriptor into a user-readable string:
        /// Name, version: x, min: x, max: x
        /// </summary>
        private static string RelationshipToPrintableString(RelationshipDescriptor dep)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(dep.name);
            if (!string.IsNullOrEmpty(dep.version)) sb.Append(", version: " + dep.version);
            if (!string.IsNullOrEmpty(dep.min_version)) sb.Append(", min: " + dep.version);
            if (!string.IsNullOrEmpty(dep.max_version)) sb.Append(", max: " + dep.version);
            return sb.ToString();
        }
    }
}

