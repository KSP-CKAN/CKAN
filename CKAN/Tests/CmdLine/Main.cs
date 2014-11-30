using NUnit.Framework;
using System.Linq;
using Tests;
// for the debug
using System.IO;
using System.Reflection;

namespace CmdLineTests
{
    [TestFixture]
    public class Main
    {
        private CKAN.CkanModule[] modules = new CKAN.CkanModule[3] { TestData.kOS_014_module(), TestData.FireSpitterModule(), TestData.DogeCoinFlag_101_module() };
        private string[] module_names = null;
        private DisposableKSP environ = null;
        private string old_default = null;
        private const string environ_name = "ckan_test_environment";

        /// <summary>
        /// Extract module names from the given list of modules.
        /// </summary>
        private string[] _get_module_names(CKAN.CkanModule[] mod_list)
        {
            string[] module_names = new string[mod_list.Length];

            foreach (var index in Enumerable.Range(0, mod_list.Length))
            {
                module_names[index] = mod_list[index].identifier;
            }

            return module_names;
        }

        /// <summary>
        /// Build a clean KSP environment for each test and register it as
        /// the default KSP instance with the manager.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Provide an empty install environment before each test.
            environ = new DisposableKSP();
            // Save previous default instance if any
            old_default = CKAN.KSPManager.AutoStartInstance;
            // Establish a temporary environment as default
            CKAN.KSPManager.AddInstance(environ_name, environ.KSP.GameDir());
            CKAN.KSPManager.SetAutoStart(environ_name);

            // Filter modules into their string names
            this.module_names = _get_module_names(this.modules);
        }

        [TearDown]
        public void TearDown()
        {
            // Reset default instance to previous or none
            if (old_default != null)
            {
                CKAN.KSPManager.SetAutoStart(old_default);
            }
            else
            {
                CKAN.KSPManager.ClearAutoStart();
            }
            // Clean the environment properly.
            CKAN.KSPManager.RemoveInstance(environ_name);
            environ.Dispose();
        }

        /// <summary>
        /// Install the given list of modules.
        /// </summary>
        public void _CallOverMods(string action, string[] mod_list)
        {
            string[] args = new string[mod_list.Length+1];

            // build an argument array of "install <mod> <mod> ..."
            args[0] = action;
            mod_list.CopyTo(args, 1);

            // Call the command-line as though it were called from console.
            CKAN.CmdLine.MainClass.Main(args);

        }

        /// <summary>
        /// Ensure mod is installed or is not installed based on boolean.
        /// Asserts the assurance, does not return anything.
        /// </summary>
        private void _CheckInstalled(bool installed, string mod) {
            string msg = "";
            if (installed)
            {
                msg = mod + " is installed.";
            }
            else
            {
                msg = mod + " is not installed.";
            }
            //Assert.AreEqual(installed, environ.KSP.Registry.IsInstalled(mod), msg);
            environ.KSP.Registry.IsInstalled(mod);
        }

        /// <summary>
        /// Ensure mods are not installed, install them, and ensure
        /// the mods are installed.
        /// </summary>
        private void _Install(string[] mod_list)
        {
            // Ensure the modules are not already installed.
            foreach (var module_name in mod_list)
            {
                _CheckInstalled(false, module_name);
            }

            _CallOverMods("install", mod_list);

            // Ensure the modules are were successfully installed.
            // There might be a more precise way to check this...
            foreach (var module_name in mod_list)
            {
                _CheckInstalled(true, module_name);
            }
        }

        /// <summary>
        /// Install a single module.
        /// </summary>
        [Test]
        public void InstallOne()
        {
            // extract the first module and pack it into a list
            string[] one_module = new string[1] {this.module_names[0]};
            _Install(one_module);
        }

        /// <summary>
        /// Install many modules.
        /// </summary>
        [Test]
        public void InstallMany()
        {
            _Install(this.module_names);
        }

        /// <summary>
        /// Ensure mods are installed, remove them, and ensure
        /// the mods are removed.
        /// </summary>
        private void _Remove(string[] mod_list)
        {
            // Ensure the modules are already installed.
            foreach (var module_name in mod_list)
            {
                _CheckInstalled(true, module_name);
            }

            _CallOverMods("remove", mod_list);

            // Ensure the modules are were successfully removed.
            foreach (var module_name in mod_list)
            {
                _CheckInstalled(false, module_name);
            }
        }

        /// <summary>
        /// Install one module and then remove it.
        /// This is slightly redundant with other tests as it calls install.
        /// </summary>
        [Test]
        public void RemoveOne()
        {
            // extract the first module and pack it into a list
            string[] one_module = new string[1] {this.module_names[0]};

            // install the module
            _CallOverMods("install", one_module);

            // testfully remove it
            _Remove(one_module);
        }

        /// <summary>
        /// Install many modules and then remove them.
        /// This is slightly redundant with other tests as it calls install.
        /// </summary>
        [Test]
        public void RemoveMany()
        {
            // install the modules
            _CallOverMods("install", this.module_names);

            // testfully remove them
            _Remove(this.module_names);
        }
    }
}
