using System;
using System.Collections.Generic;
using System.Linq;
using CKAN;
using NUnit.Framework;
using Tests;

namespace CKANTests
{
    [TestFixture] public class KSPManagerTests
    {
        private DisposableKSP tidy;
        private const string nameInReg = "testing";
        private FakeWin32Registry win32_reg;
        KSPManager manager;

        [SetUp]
        public void SetUp()
        {
            tidy = new DisposableKSP();
            win32_reg = GetTestWin32Reg(nameInReg);            
            manager = new KSPManager(new NullUser(), win32_reg);
        }

        [TearDown]
        public void TearDown()
        {
            tidy.Dispose();
        }

        [Test]
        public void HasInstance_ReturnsFalseIfNoInstanceByThatName()
        {
            const string anyNameNotInReg = "Games";                        
            Assert.That(manager.HasInstance(anyNameNotInReg), Is.EqualTo(false));
        }

        [Test]
        public void HasInstance_ReturnsTrueIfInstanceByThatName()
        {            
            Assert.That(manager.HasInstance(nameInReg), Is.EqualTo(true));
        }

        [Test]
        public void SetAutoStart_VaildName_SetsAutoStart()
        {         
            Assert.That(manager.AutoStartInstance, Is.EqualTo(null));

            manager.SetAutoStart(nameInReg);
            Assert.That(manager.AutoStartInstance, Is.EqualTo(nameInReg));
        }

        [Test]
        public void SetAutoStart_InvalidName_DoesNotChangeAutoStart()
        {
            manager.SetAutoStart(nameInReg);
            Assert.Throws<InvalidKSPInstanceKraken>(() => manager.SetAutoStart("invalid"));
            Assert.That(manager.AutoStartInstance, Is.EqualTo(nameInReg));
        }

        [Test]
        public void RemoveInstance_HasInstanceReturnsFalse()
        {
            manager.RemoveInstance(nameInReg);
            Assert.False(manager.HasInstance(nameInReg));
        }

        [Test]
        public void RenameInstance_HasInstanceOriginalName_ReturnsFalse()
        {
            manager.RenameInstance(nameInReg,"newname");
            Assert.False(manager.HasInstance(nameInReg));
        }
        [Test]
        public void RenameInstance_HasInstanceNewName()
        {
            const string newname = "newname";
            manager.RenameInstance(nameInReg, newname);
            Assert.True(manager.HasInstance(newname));
        }
        
        [Test]
        public void ClearAutoStart_UpdatesValueInWin32Reg()
        {

            Assert.That(win32_reg.AutoStartInstance, Is.Null.Or.Empty);
            
        }

        [Test]
        public void GetNextValidInstanceName_ManagerDoesNotHaveResult()
        {
            var name = manager.GetNextValidInstanceName(nameInReg);
            Assert.That(manager.HasInstance(name),Is.False);
            
        }

        [Test]
        public void AddInstance_ManagarHasInstance()
        {
            using (var tidy2 = new DisposableKSP())
            {                
                const string newInstance = "tidy2";
                Assert.That(manager.HasInstance(newInstance), Is.False);
                manager.AddInstance(newInstance, tidy2.KSP);
                Assert.That(manager.HasInstance(newInstance),Is.True);
            }
        }

        [Test]
        public void GetPreferredInstance_WithAutoStart_ReturnsAutoStart()
        {
            Assert.That(manager.GetPreferredInstance(),Is.EqualTo(tidy.KSP));
        }

        [Test]
        public void GetPreferredInstance_WithEmptyAutoStartAndMultipleInstances_ReturnsNull()
        {
            using (var tidy2 = new DisposableKSP())
            {
                win32_reg.Instances.Add(new Tuple<string, string>("tidy2",tidy2.KSP.GameDir()));
                manager.LoadInstancesFromRegistry();
                manager.ClearAutoStart();
                Assert.That(manager.GetPreferredInstance(), Is.Null);
            }
            
        }

        [Test]
        public void GetPreferredInstance_OnlyOneAvailable_ReturnsAvailable()
        {
            manager.ClearAutoStart();
            Assert.That(manager.GetPreferredInstance(), Is.EqualTo(tidy.KSP));
        }

        [Test]
        public void SetCurrentInstance_NameNotInRepo_Throws()
        {
            Assert.Throws<InvalidKSPInstanceKraken>(() => manager.SetCurrentInstance("invalid"));
        }

        [Test] //37a33
        public void Ctor_InvalidAutoStart_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new KSPManager(new NullUser(),new FakeWin32Registry(tidy.KSP, "invalid")
                ));
        }


        //TODO Test FindAndRegisterDefaultInstance

        private FakeWin32Registry GetTestWin32Reg(string name)
        {
            return new FakeWin32Registry(new List<Tuple<string, string>>
            {
                new Tuple<string, string>(name, tidy.KSP.GameDir())
            });
        }
    }

    public class FakeWin32Registry : IWin32Registry
    {
        public FakeWin32Registry(CKAN.KSP instance,string autostart)
        {
            Instances = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("test", instance.GameDir())
            };
            AutoStartInstance = autostart;
        }

        public FakeWin32Registry(CKAN.KSP instance):this(instance, "test")
        {
            
        }

        public FakeWin32Registry(List<Tuple<string, string>> instances, string auto_start_instance = null)
        {
            Instances = instances;
            AutoStartInstance = auto_start_instance;
        }

        public List<Tuple<string, string>> Instances { get; set; }

        public int InstanceCount
        {
            get { return Instances.Count; }
            }

        // In the Win32Registry it is not possible to get null in autostart.
        private string _AutoStartInstance;
        public string AutoStartInstance
        {
            get
            {
                if (_AutoStartInstance != null)
                {
                    return _AutoStartInstance;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                _AutoStartInstance = value;
            }
        }

        public Tuple<string, string> GetInstance(int i)
        {
            return Instances[i];
        }

        public void SetRegistryToInstances(SortedList<string, CKAN.KSP> instances, string auto_start_instance)
        {
            Instances =
                instances.Select((kvpair) => new Tuple<string, string>(kvpair.Key, kvpair.Value.GameDir())).ToList();
            AutoStartInstance = auto_start_instance;
        }

        public IEnumerable<Tuple<string, string>> GetInstances()
        {
            return Instances;
        }
    }
}