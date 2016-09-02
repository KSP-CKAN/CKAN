using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CKAN;
using NUnit.Framework;
using Tests.Core;
using Tests.Data;
using ModuleInstaller = CKAN.ModuleInstaller;

namespace Tests.GUI
{
    /// <summary>
    /// This test attempts to reproduce the state of GitHub issue #1866
    /// which involves sorting the GUI table by Max KSP Version and then performing a repo operation.
    /// </summary>
    [Explicit]
    [TestFixture]
    public class GH1866
    {
        private CkanModule _anyVersionModule;
        private DisposableKSP _instance;
        private KSPManager _manager;
        private Registry _registry;
        private MainModList _modList;
        private MainModListGUI _listGui;


        /*
         * an exception would be thrown at the bottom of this
         */
        /*var main = new Main(null, new GUIUser(), false);
        main.Manager = _manager;
        // first sort by name
        main.configuration.SortByColumnIndex = 2;
        // now sort by version
        main.configuration.SortByColumnIndex = 6;
        main.MarkModForInstall("kOS");

        // make sure we have one requested change
        var changeList = main.mainModList.ComputeUserChangeSet()
            .Select((change) => change.Mod.ToCkanModule()).ToList();

        // do the install
        ModuleInstaller.GetInstance(_instance.KSP, main.currentUser).InstallList(
            changeList,
            new RelationshipResolverOptions(),
            new NetAsyncModulesDownloader(main.currentUser)
        );*/

        [TestFixtureSetUp]
        public void Up()
        {
            _instance = new DisposableKSP();
            _registry = Registry.Empty();
            _manager = new KSPManager(new NullUser(), new FakeWin32Registry(_instance.KSP));

            // this module contains a ksp_version of "any" which repros our issue
            _anyVersionModule = TestData.DogeCoinFlag_101_module();

            // install it and set it as pre-installed
            _instance.KSP.Cache.Store(TestData.DogeCoinFlag_101_module().download, TestData.DogeCoinFlagZip());
            _registry.RegisterModule(_anyVersionModule, new string[] { }, _instance.KSP);
            _registry.AddAvailable(_anyVersionModule);

            ModuleInstaller.GetInstance(_instance.KSP, _manager.User).InstallList(
                new List<CkanModule> { { _anyVersionModule } },
                new RelationshipResolverOptions(),
                new NetAsyncModulesDownloader(_manager.User)
            );

            // this module is not for "any" version, to provide another to sort against
            _registry.AddAvailable(TestData.kOS_014_module());

            // test object
            _modList = new MainModList(delegate { }, delegate { return null; });
            _listGui = new MainModListGUI();

            // todo: refactor the column header code to allow mocking of the GUI without creating columns
            _listGui.Columns.Add(new DataGridViewCheckBoxColumn());
            _listGui.Columns.Add(new DataGridViewCheckBoxColumn());
            for (int i = 0; i < 10; i++)
            {
                _listGui.Columns.Add(i.ToString(), "Column" + i);
            }
        }

        [TestFixtureTearDown]
        public void Down()
        {
            _instance.Dispose();
        }

        [Test]
        public void Sanity()
        {
            Assert.IsNotNull(_instance.KSP);
            Assert.IsNotNull(_manager);
            Assert.IsNotNull(_modList);
        }

        [Test]
        public async void TestSimple()
        {
            var modules = _registry.available_modules
                .Select((mod) => new GUIMod(mod.Value.Latest(), _registry, _instance.KSP.Version()))
                .ToList();

            // varargs method signature means we must call .ToArray()
            _listGui.Rows.AddRange(_modList.ConstructModList(modules).ToArray());
            Assert.AreEqual(3, _listGui.Rows.Count);

            // sort by version, this is the fuse-lighting
            _listGui.Sort(_listGui.Columns[6], ListSortDirection.Descending);

            var anyVersionModule = modules.First((mod) => mod.Identifier.Contains("Doge"));
            var anyVersionRow = _listGui.Rows
                .Cast<DataGridViewRow>()
                .First((mod) => mod.Tag.Equals(anyVersionModule));
            anyVersionModule.SetInstallChecked(anyVersionRow, true);
            Assert.IsTrue(anyVersionModule.IsInstallChecked);
        }
    }
}
