using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CKAN;
using NUnit.Framework;
using Tests.Core;
using Tests.Data;

namespace Tests.GUI
{
    [TestFixture]
    class Settings
    {
        private SettingsDialog _dialog;
        private DisposableKSP _instance;
        private KSPManager _manager;
        private RegistryManager _registry;

        [TestFixtureSetUp]
        public void Up()
        {
            var config = new Configuration
            {
                BuildMapUrl = SettingsDialog.DefaultBuildUrl,
                RefreshOnStartup = false,
                CheckForUpdatesOnLaunch = false
            };

            _instance = new DisposableKSP();
            _manager = new KSPManager(new NullUser(), new FakeWin32Registry(_instance.KSP)) { CurrentInstance = _instance.KSP };
            _dialog = new SettingsDialog
            {
                Configuration = config,
                Instance = _instance.KSP
            };
            _dialog.Initialize();
        }

        [TestFixtureTearDown]
        public void Down()
        {
            _dialog.Hide();
            _dialog.Close();
            _dialog = null;
            RegistryManager.Instance(_instance.KSP).ReleaseLock();
            _instance.Dispose();
        }

        [Test]
        public void OnCreation_HasBuildMapUrl()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(_dialog.BuildUrl));
        }

        [Test]
        public void OnCreation_HasRepoItems()
        {
            Assert.IsNotEmpty(_dialog.Repos);
        }
    }
}
