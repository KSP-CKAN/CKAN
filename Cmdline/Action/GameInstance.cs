using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CKAN.DLC;
using CKAN.Games;
using CKAN.Versioning;
using CommandLine;
using log4net;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing KSP installs.
    /// </summary>
    public class GameInstance : ISubCommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GameInstance));

        private GameInstanceManager _manager;
        private IUser _user;

        /// <summary>
        /// Run the 'ksp' command.
        /// </summary>
        /// <inheritdoc cref="ISubCommand.RunCommand"/>
        public int RunCommand(GameInstanceManager manager, object args)
        {
            var s = args.ToString();
            var opts = s.Replace(s.Substring(0, s.LastIndexOf('.') + 1), "").Split('+');

            CommonOptions options = new CommonOptions();
            _user = new ConsoleUser(options.Headless);
            _manager = manager ?? new GameInstanceManager(_user);
            var exitCode = options.Handle(_manager, _user);

            if (exitCode != Exit.Ok)
                return exitCode;

            switch (opts[1])
            {
                case "AddKsp":
                    exitCode = AddInstall(args);
                    break;
                case "CloneKsp":
                    exitCode = CloneInstall(args);
                    break;
                case "DefaultKsp":
                    exitCode = SetDefaultInstall(args);
                    break;
                case "FakeKsp":
                    exitCode = FakeNewKspInstall(args);
                    break;
                case "ForgetKsp":
                    exitCode = ForgetInstall(args);
                    break;
                case "ListKsp":
                    exitCode = ListInstalls();
                    break;
                case "RenameKsp":
                    exitCode = RenameInstall(args);
                    break;
                default:
                    exitCode = Exit.BadOpt;
                    break;
            }

            return exitCode;
        }

        /// <inheritdoc cref="ISubCommand.GetUsage"/>
        public string GetUsage(string prefix, string[] args)
        {
            if (args.Length == 1)
                return $"{prefix} {args[0]} <command> [options]";

            switch (args[1])
            {
                case "add":
                    return $"{prefix} {args[0]} {args[1]} [options] <name> <path>";
                case "clone":
                    return $"{prefix} {args[0]} {args[1]} [options] <nameOrPath> <newName> <newPath>";
                case "fake":
                    return $"{prefix} {args[0]} {args[1]} [options] <name> <path> <version> [--MakingHistory <version>] [--BreakingGround <version>]";
                case "default":
                case "forget":
                    return $"{prefix} {args[0]} {args[1]} [options] <name>";
                case "list":
                    return $"{prefix} {args[0]} {args[1]} [options]";
                case "rename":
                    return $"{prefix} {args[0]} {args[1]} [options] <oldName> <newName>";
                default:
                    return $"{prefix} {args[0]} <command> [options]";
            }
        }

        private int AddInstall(object args)
        {
            var opts = (KspOptions.AddKsp)args;
            if (opts.Name == null || opts.Path == null)
            {
                _user.RaiseMessage("add <name> <path> - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            if (_manager.HasInstance(opts.Name))
            {
                _user.RaiseMessage("Install with the name \"{0}\" already exists, aborting...", opts.Name);
                return Exit.BadOpt;
            }

            try
            {
                _manager.AddInstance(opts.Path, opts.Name, _user);
                _user.RaiseMessage("Added \"{0}\" with root \"{1}\" to known installs.", opts.Name, opts.Path);
            }
            catch (NotKSPDirKraken kraken)
            {
                _user.RaiseMessage("Sorry, \"{0}\" does not appear to be a KSP directory.", kraken.path);
                return Exit.Error;
            }

            return Exit.Ok;
        }

        private int CloneInstall(object args)
        {
            var opts = (KspOptions.CloneKsp)args;
            if (opts.NameOrPath == null || opts.NewName == null || opts.NewPath == null)
            {
                _user.RaiseMessage("clone <nameOrPath> <newName> <newPath> - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            // Parse all options
            var nameOrPath = opts.NameOrPath;
            var newName = opts.NewName;
            var newPath = opts.NewPath;

            Log.InfoFormat("Cloning the KSP instance \"{0}\".", nameOrPath);
            try
            {
                // Try nameOrPath as name and search the registry for it
                if (_manager.HasInstance(nameOrPath))
                {
                    var listOfInstances = _manager.Instances.Values.ToArray();
                    foreach (var instance in listOfInstances)
                    {
                        if (instance.Name == nameOrPath)
                        {
                            // Found it, now clone it
                            _manager.CloneInstance(instance, newName, newPath);
                            break;
                        }
                    }
                }
                // Try to use nameOrPath as a path and create a new game object
                else if (_manager.InstanceAt(nameOrPath, newName) is CKAN.GameInstance instance && instance.Valid)
                {
                    _manager.CloneInstance(instance, newName, newPath);
                }
                // There is no instance with this name or at this path
                else
                {
                    throw new NoGameInstanceKraken(nameOrPath);
                }
            }
            catch (NotKSPDirKraken kraken)
            {
                // There are two possible reasons:
                // First: The instance to clone is not a valid KSP instance
                // This only occurs if the user manipulated the directory and deleted
                // files/folders which CKAN searches for in the validity test

                // Second: Something went wrong with adding the new instance to the registry,
                // most likely because the newly created directory is not valid

                _user.RaiseError("The specified instance \"{0}\" is not a valid KSP instance.\r\n   {1}", nameOrPath, kraken.path);
                return Exit.Error;
            }
            catch (PathErrorKraken kraken)
            {
                // The new path is not empty
                _user.RaiseError("The directory to clone to is not empty.\r\n   {0}", kraken.path);
                return Exit.Error;
            }
            catch (IOException ex)
            {
                // Something went wrong while copying the files
                _user.RaiseError(ex.ToString());
                return Exit.Error;
            }
            catch (NoGameInstanceKraken)
            {
                // Did not find a known game instance
                _user.RaiseError("No instance found with this name or at this path: \"{0}\".\r\nSee below for a list of known instances:\r\n", nameOrPath);
                ListInstalls();
                return Exit.Error;
            }
            catch (InstanceNameTakenKraken kraken)
            {
                // Instance name already exists
                _user.RaiseError("An instance with the name \"{0}\" already exists.", kraken.instName);
                return Exit.BadOpt;
            }

            // Test if the instance was added to the registry
            // No need to test if valid, because this is done in AddInstance(),
            // so if something went wrong, HasInstance is false
            if (!_manager.HasInstance(newName))
            {
                _user.RaiseMessage("Something went wrong. Please look if the new directory has been created.\r\nTry to add the new instance manually with 'ckan ksp add'.");
                return Exit.Error;
            }

            _user.RaiseMessage("Successfully cloned the instance \"{0}\" into \"{1}\".", nameOrPath, newPath);
            return Exit.Ok;
        }

        private int SetDefaultInstall(object args)
        {
            var opts = (KspOptions.DefaultKsp)args;
            var name = opts.Name;

            if (name == null)
            {
                // Check if there is a default instance
                var defaultInstance = _manager.Configuration.AutoStartInstance;
                var defaultInstancePresent = 0;

                if (!string.IsNullOrWhiteSpace(defaultInstance))
                {
                    defaultInstancePresent = 1;
                }

                var keys = new object[_manager.Instances.Count + defaultInstancePresent];

                // Populate the list of instances
                for (var i = 0; i < _manager.Instances.Count; i++)
                {
                    var instance = _manager.Instances.ElementAt(i);

                    keys[i + defaultInstancePresent] = string.Format("\"{0}\" - {1}", instance.Key, instance.Value.GameDir());
                }

                // Mark the default instance for the user
                if (!string.IsNullOrWhiteSpace(defaultInstance))
                {
                    keys[0] = _manager.Instances.IndexOfKey(defaultInstance);
                }

                int result;
                try
                {
                    // No input argument from the user. Present a list of the possible instances
                    var message = "default <name> - argument missing, please select an instance from the list below.";
                    result = _user.RaiseSelectionDialog(message, keys);
                }
                catch (Kraken)
                {
                    return Exit.BadOpt;
                }

                if (result < 0)
                {
                    return Exit.BadOpt;
                }

                name = _manager.Instances.ElementAt(result).Key;
            }

            if (!_manager.Instances.ContainsKey(name))
            {
                _user.RaiseMessage("Couldn't find an install with the name \"{0}\", aborting...", name);
                return Exit.BadOpt;
            }

            try
            {
                _manager.SetAutoStart(name);
            }
            catch (NotKSPDirKraken kraken)
            {
                _user.RaiseMessage("Sorry, \"{0}\" does not appear to be a KSP directory.", kraken.path);
                return Exit.Error;
            }

            _user.RaiseMessage("Successfully set \"{0}\" as the default KSP installation.", name);
            return Exit.Ok;
        }

        private int FakeNewKspInstall(object args)
        {
            var opts = (KspOptions.FakeKsp)args;
            if (opts.Name == null || opts.Path == null || opts.Version == null)
            {
                _user.RaiseMessage("fake <name> <path> <version> [--MakingHistory <version>] [--BreakingGround <version>] - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            // opts.<DLC>Version is "none" if the DLC shouldn't be simulated
            var dlcs = new Dictionary<IDlcDetector, GameVersion>();
            if (opts.MakingHistory != null && opts.MakingHistory.ToLower() != "none")
            {
                if (GameVersion.TryParse(opts.MakingHistory, out GameVersion ver))
                {
                    dlcs.Add(new MakingHistoryDlcDetector(), ver);
                }
                else
                {
                    _user.RaiseError("Please check the Making History DLC version argument - Format it like Maj.Min.Patch - e.g. 1.1.0");
                    return Exit.BadOpt;
                }
            }

            if (opts.BreakingGround != null && opts.BreakingGround.ToLower() != "none")
            {
                if (GameVersion.TryParse(opts.BreakingGround, out GameVersion ver))
                {
                    dlcs.Add(new BreakingGroundDlcDetector(), ver);
                }
                else
                {
                    _user.RaiseError("Please check the Breaking Ground DLC version argument - Format it like Maj.Min.Patch - e.g. 1.1.0");
                    return Exit.BadOpt;
                }
            }

            // Parse all options
            var name = opts.Name;
            var path = opts.Path;
            var setDefault = opts.SetDefault;
            GameVersion version;

            try
            {
                version = GameVersion.Parse(opts.Version);
            }
            catch (FormatException)
            {
                // Thrown if there is anything besides numbers and points in the version string or a different syntactic error
                _user.RaiseError("Please check the version argument - Format it like Maj.Min.Patch[.Build] - e.g. 1.6.0 or 1.2.2.1622");
                return Exit.BadOpt;
            }

            // Get the full version including build number
            try
            {
                version = version.RaiseVersionSelectionDialog(new KerbalSpaceProgram(), _user);
            }
            catch (BadGameVersionKraken)
            {
                _user.RaiseError("Couldn't find a valid KSP version for your input.\r\nMake sure to enter at least the version major and minor values in the form Maj.Min - e.g. 1.5");
                return Exit.Error;
            }
            catch (CancelledActionKraken)
            {
                _user.RaiseError("Selection cancelled! Please call 'ckan ksp fake' again.");
                return Exit.Error;
            }

            _user.RaiseMessage("Creating a new fake KSP install with the name \"{0}\" at \"{1}\" with version \"{2}\".", name, path, version);
            Log.Debug("Faking instance...");

            try
            {
                // Pass all arguments to CKAN.GameInstanceManager.FakeInstance() and create a new one
                _manager.FakeInstance(new KerbalSpaceProgram(), name, path, version, dlcs);
                if (setDefault)
                {
                    _user.RaiseMessage("Setting new instance to default...");
                    _manager.SetAutoStart(name);
                }
            }
            catch (InstanceNameTakenKraken kraken)
            {
                // Instance name already exists
                _user.RaiseError("An instance with the name \"{0}\" already exists.", kraken.instName);
                return Exit.BadOpt;
            }
            catch (BadInstallLocationKraken)
            {
                // The folder exists but is not empty
                _user.RaiseError("The directory to clone to is not empty.\r\n   {0}", path);
                return Exit.Error;
            }
            catch (WrongGameVersionKraken kraken)
            {
                // Thrown because the specified KSP version is too old for one of the selected DLCs
                _user.RaiseError(kraken.Message);
                return Exit.Error;
            }
            catch (NotKSPDirKraken kraken)
            {
                // Something went wrong adding the new instance to the registry,
                // most likely because the newly created directory is somehow not valid
                _user.RaiseError("The specified instance \"{0}\" is not a valid KSP instance.\r\n   {1}", name, kraken.path);
                return Exit.Error;
            }
            catch (InvalidKSPInstanceKraken)
            {
                // Thrown by Manager.SetAutoStart() if Manager.HasInstance returns false
                // Will be checked again down below with a proper error message
            }

            // Test if the instance was added to the registry
            // No need to test if valid, because this is done in AddInstance(),
            // so if something went wrong, HasInstance is false
            if (!_manager.HasInstance(name))
            {
                _user.RaiseMessage("Something went wrong. Please look if the new directory has been created.\r\nTry to add the new instance manually with 'ckan ksp add'.");
                return Exit.Error;
            }

            _user.RaiseMessage("Successfully faked the instance \"{0}\" into \"{1}\".", name, path);
            return Exit.Ok;
        }

        private int ForgetInstall(object args)
        {
            var opts = (KspOptions.ForgetKsp)args;
            if (opts.Name == null)
            {
                _user.RaiseMessage("forget <name> - argument missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            if (!_manager.HasInstance(opts.Name))
            {
                _user.RaiseMessage("Couldn't find an install with the name \"{0}\", aborting...", opts.Name);
                return Exit.BadOpt;
            }

            _manager.RemoveInstance(opts.Name);

            _user.RaiseMessage("Successfully removed \"{0}\".", opts.Name);
            return Exit.Ok;
        }

        private int ListInstalls()
        {
            const string nameHeader = "Name";
            const string versionHeader = "Version";
            const string defaultHeader = "Default";
            const string pathHeader = "Path";

            var output = _manager.Instances
                .OrderByDescending(i => i.Value.Name == _manager.AutoStartInstance)
                .ThenByDescending(i => i.Value.Version() ?? GameVersion.Any)
                .ThenBy(i => i.Key)
                .Select(i => new
                {
                    Name = i.Key,
                    Version = i.Value.Version()?.ToString() ?? "<NONE>",
                    Default = i.Value.Name == _manager.AutoStartInstance ? "Yes" : "No",
                    Path = i.Value.GameDir()
                })
                .ToList();

            var nameWidth = Enumerable.Repeat(nameHeader, 1).Concat(output.Select(i => i.Name)).Max(i => i.Length);
            var versionWidth = Enumerable.Repeat(versionHeader, 1).Concat(output.Select(i => i.Version)).Max(i => i.Length);
            var defaultWidth = Enumerable.Repeat(defaultHeader, 1).Concat(output.Select(i => i.Default)).Max(i => i.Length);
            var pathWidth = Enumerable.Repeat(pathHeader, 1).Concat(output.Select(i => i.Path)).Max(i => i.Length);

            _user.RaiseMessage("{0}  {1}  {2}  {3}",
                nameHeader.PadRight(nameWidth),
                versionHeader.PadRight(versionWidth),
                defaultHeader.PadRight(defaultWidth),
                pathHeader.PadRight(pathWidth)
            );

            _user.RaiseMessage("{0}  {1}  {2}  {3}",
                new string('-', nameWidth),
                new string('-', versionWidth),
                new string('-', defaultWidth),
                new string('-', pathWidth)
            );

            foreach (var line in output)
            {
                _user.RaiseMessage("{0}  {1}  {2}  {3}",
                    line.Name.PadRight(nameWidth),
                    line.Version.PadRight(versionWidth),
                    line.Default.PadRight(defaultWidth),
                    line.Path.PadRight(pathWidth)
                );
            }

            return Exit.Ok;
        }

        private int RenameInstall(object args)
        {
            var opts = (KspOptions.RenameKsp)args;
            if (opts.OldName == null || opts.NewName == null)
            {
                _user.RaiseMessage("rename <oldName> <newName> - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            if (!_manager.HasInstance(opts.OldName))
            {
                _user.RaiseMessage("Couldn't find an install with the name \"{0}\", aborting...", opts.OldName);
                return Exit.BadOpt;
            }

            _manager.RenameInstance(opts.OldName, opts.NewName);

            _user.RaiseMessage("Successfully renamed \"{0}\" to \"{1}\".", opts.OldName, opts.NewName);
            return Exit.Ok;
        }
    }

    [Verb("ksp", HelpText = "Manage KSP installs")]
    [ChildVerbs(typeof(AddKsp), typeof(CloneKsp), typeof(DefaultKsp), typeof(FakeKsp), typeof(ForgetKsp), typeof(ListKsp), typeof(RenameKsp))]
    internal class KspOptions
    {
        [VerbExclude]
        [Verb("add", HelpText = "Add a KSP install")]
        internal class AddKsp : CommonOptions
        {
            [Value(0, MetaName = "Name", HelpText = "The name of the new KSP install")]
            public string Name { get; set; }

            [Value(1, MetaName = "Path", HelpText = "The path where KSP is installed")]
            public string Path { get; set; }
        }

        [VerbExclude]
        [Verb("clone", HelpText = "Clone an existing KSP install")]
        internal class CloneKsp : CommonOptions
        {
            [Value(0, MetaName = "NameOrPath", HelpText = "The name or path to clone")]
            public string NameOrPath { get; set; }

            [Value(1, MetaName = "NewName", HelpText = "The name of the new KSP install")]
            public string NewName { get; set; }

            [Value(2, MetaName = "NewPath", HelpText = "The path to clone the KSP install to")]
            public string NewPath { get; set; }
        }

        [VerbExclude]
        [Verb("default", HelpText = "Set the default KSP install")]
        internal class DefaultKsp : CommonOptions
        {
            [Value(0, MetaName = "Name", HelpText = "The name of the KSP install to set as the default")]
            public string Name { get; set; }
        }

        [VerbExclude]
        [Verb("fake", HelpText = "Fake a KSP install")]
        internal class FakeKsp : CommonOptions
        {
            [Option("MakingHistory", Default = "none", HelpText = "The version of the Making History DLC to be faked")]
            public string MakingHistory { get; set; }

            [Option("BreakingGround", Default = "none", HelpText = "The version of the Breaking Ground DLC to be faked")]
            public string BreakingGround { get; set; }

            [Option("set-default", HelpText = "Set the new instance as the default one")]
            public bool SetDefault { get; set; }

            [Value(0, MetaName = "Name", HelpText = "The name of the faked KSP install")]
            public string Name { get; set; }

            [Value(1, MetaName = "Path", HelpText = "The path to fake the KSP install to")]
            public string Path { get; set; }

            [Value(2, MetaName = "KSP version", HelpText = "The KSP version of the faked install")]
            public string Version { get; set; }
        }

        [VerbExclude]
        [Verb("forget", HelpText = "Forget a KSP install")]
        internal class ForgetKsp : CommonOptions
        {
            [Value(0, MetaName = "Name", HelpText = "The name of the KSP install to remove")]
            public string Name { get; set; }
        }

        [VerbExclude]
        [Verb("list", HelpText = "List KSP installs")]
        internal class ListKsp : CommonOptions { }

        [VerbExclude]
        [Verb("rename", HelpText = "Rename a KSP install")]
        internal class RenameKsp : CommonOptions
        {
            [Value(0, MetaName = "Old name", HelpText = "The name of the KSP install to rename")]
            public string OldName { get; set; }

            [Value(1, MetaName = "New name", HelpText = "The new name of the KSP install")]
            public string NewName { get; set; }
        }
    }
}
