using System.Collections.Generic;

using NUnit.Framework;

using CKAN.IO;

namespace Tests.Core.IO
{
    [TestFixture]
    public class ProtocolRouterTests
    {
        private readonly List<string> focused = new List<string>();
        private readonly List<string> searched = new List<string>();
        private readonly List<List<(string Mod, string? Version)>> installed = new List<List<(string Mod, string? Version)>>();
        private readonly List<UrlError> errors = new List<UrlError>();

        [SetUp]
        public void SetUp()
        {
            focused.Clear();
            searched.Clear();
            installed.Clear();
            errors.Clear();
            ProtocolRouter.PendingLaunchUrl = null;
            ProtocolRouter.OnFocus += focused.Add;
            ProtocolRouter.OnSearch += searched.Add;
            ProtocolRouter.OnInstall += installed.Add;
            ProtocolRouter.OnError += errors.Add;
        }

        [TearDown]
        public void TearDown()
        {
            ProtocolRouter.OnFocus -= focused.Add;
            ProtocolRouter.OnSearch -= searched.Add;
            ProtocolRouter.OnInstall -= installed.Add;
            ProtocolRouter.OnError -= errors.Add;
        }

        [TestCase("")]
        [TestCase(" ")]
        public void Handle_EmptyOrWhitespace_NoOp(string input)
        {
            ProtocolRouter.Handle(input);

            Assert.IsEmpty(focused);
            Assert.IsEmpty(searched);
            Assert.IsEmpty(installed);
            Assert.IsEmpty(errors);
        }

        [Test]
        public void Handle_FocusUrl_OperationIsCaseInsensitive()
        {
            ProtocolRouter.Handle("ckan://FOCUS?mod=JNSQ");
            ProtocolRouter.Handle("ckan://Focus?mod=Astrogator");

            CollectionAssert.AreEqual(new[] { "JNSQ", "Astrogator" }, focused);
        }

        [Test]
        public void Handle_AcceptsUrlWithoutCkanScheme()
        {
            ProtocolRouter.Handle("focus?mod=JNSQ");

            CollectionAssert.AreEqual(new[] { "JNSQ" }, focused);
        }

        [TestCase("ckan://focus")]
        [TestCase("ckan://focus?")]
        [TestCase("ckan://focus?mod=")]
        [TestCase("ckan://focus?mod=   ")]
        [TestCase("ckan://focus?wrongkey=JNSQ")]
        public void Handle_FocusWithoutValidMod_RaisesNoValidKeys(string input)
        {
            ProtocolRouter.Handle(input);

            Assert.IsEmpty(focused);
            CollectionAssert.AreEqual(new[] { UrlError.NoValidKeys }, errors);
        }

        [Test]
        public void Handle_SearchUrl_FiresOnSearch()
        {
            ProtocolRouter.Handle("ckan://search?q=engine");

            CollectionAssert.AreEqual(new[] { "engine" }, searched);
            Assert.IsEmpty(focused);
        }

        [Test]
        public void Handle_SearchUrl_DecodesURIEncodedValue()
        {
            ProtocolRouter.Handle("ckan://search?q=%40Halban");

            CollectionAssert.AreEqual(new[] { "@Halban" }, searched);
        }

        [TestCase("ckan://search")]
        [TestCase("ckan://search?q=")]
        [TestCase("ckan://search?q=   ")]
        [TestCase("ckan://search?wrongkey=engine")]
        public void Handle_SearchWithoutValidQ_RaisesNoValidKeys(string input)
        {
            ProtocolRouter.Handle(input);

            Assert.IsEmpty(searched);
            CollectionAssert.AreEqual(new[] { UrlError.NoValidKeys }, errors);
        }

        [Test]
        public void Handle_InstallSingleMod_FiresOnInstall()
        {
            ProtocolRouter.Handle("ckan://install?mod=JNSQ");

            Assert.AreEqual(1, installed.Count);
            CollectionAssert.AreEqual(new (string, string?)[] { ("JNSQ", null) }, installed[0]);
        }

        [Test]
        public void Handle_InstallMultipleMods_AllArrive()
        {
            ProtocolRouter.Handle("ckan://install?mod=RealSolarSystem&mod=ROEngines&mod=ROTanks");

            Assert.AreEqual(1, installed.Count);
            CollectionAssert.AreEqual(
                new (string, string?)[]
                {
                    ("RealSolarSystem", null),
                    ("ROEngines", null),
                    ("ROTanks", null),
                },
                installed[0]);
        }

        [Test]
        public void Handle_InstallWithVersion_SplitsOnFirstColon()
        {
            ProtocolRouter.Handle("ckan://install?mod=JNSQ:0.10.0&mod=Astrogator");

            Assert.AreEqual(1, installed.Count);
            CollectionAssert.AreEqual(
                new (string, string?)[]
                {
                    ("JNSQ", "0.10.0"),
                    ("Astrogator", null),
                },
                installed[0]);
        }

        [Test]
        public void Handle_VersionContainingColon_KeptWhole()
        {
            ProtocolRouter.Handle("ckan://install?mod=JNSQ:1:2");

            Assert.AreEqual(1, installed.Count);
            CollectionAssert.AreEqual(new (string, string?)[] { ("JNSQ", "1:2") },
                                      installed[0]);
        }

        [TestCase("ckan://install")]
        [TestCase("ckan://install?")]
        [TestCase("ckan://install?mod=")]
        [TestCase("ckan://install?mod=&mod=   ")]
        [TestCase("ckan://install?wrongkey=JNSQ")]
        public void Handle_InstallWithoutValidMods_RaisesNoValidKeys(string input)
        {
            ProtocolRouter.Handle(input);

            Assert.IsEmpty(installed);
            CollectionAssert.AreEqual(new[] { UrlError.NoValidKeys }, errors);
        }

        [Test]
        public void Handle_InstallMixedValidAndEmpty_KeepsOnlyValid()
        {
            ProtocolRouter.Handle("ckan://install?mod=JNSQ&mod=&mod=Astrogator");

            Assert.AreEqual(1, installed.Count);
            CollectionAssert.AreEqual(
                new (string, string?)[]
                {
                    ("JNSQ", null),
                    ("Astrogator", null),
                },
                installed[0]);
        }

        // The last two have no host, which is just as unknown as a wrong one.
        [TestCase("ckan://select?mod=JNSQ")]
        [TestCase("ckan://garbage")]
        [TestCase("ckan://JNSQ")]
        [TestCase("ckan://")]
        [TestCase("ckan:///?mod=JNSQ")]
        public void Handle_UnknownOperation_RaisesUnknownOperation(string input)
        {
            ProtocolRouter.Handle(input);

            Assert.IsEmpty(focused);
            Assert.IsEmpty(searched);
            Assert.IsEmpty(installed);
            CollectionAssert.AreEqual(new[] { UrlError.UnknownOperation }, errors);
        }

        // Uri.TryCreate should reject these.
        [TestCase("ckan://a:b")]
        [TestCase("ckan://[")]
        [TestCase("ckan://foo bar")]
        [TestCase("ckan://a%zz")]
        public void Handle_UnparseableUrl_RaisesBadSyntax(string input)
        {
            ProtocolRouter.Handle(input);

            Assert.IsEmpty(focused);
            Assert.IsEmpty(searched);
            Assert.IsEmpty(installed);
            CollectionAssert.AreEqual(new[] { UrlError.BadSyntax }, errors);
        }

        [Test]
        public void Handle_MultipleSequentialUrls_AllFire()
        {
            ProtocolRouter.Handle("ckan://focus?mod=JNSQ");
            ProtocolRouter.Handle("ckan://search?q=engine");
            ProtocolRouter.Handle("ckan://install?mod=Astrogator");
            ProtocolRouter.Handle("ckan://focus?mod=FreeIva");

            CollectionAssert.AreEqual(new[] { "JNSQ", "FreeIva" }, focused);
            CollectionAssert.AreEqual(new[] { "engine" }, searched);
            CollectionAssert.AreEqual(new (string, string?)[] { ("Astrogator", null) },
                                      installed[0]);
        }

        [TestCase("CKAN://focus?mod=JNSQ")]
        [TestCase("Ckan://focus?mod=JNSQ")]
        public void Handle_SchemeIsCaseInsensitive(string input)
        {
            ProtocolRouter.Handle(input);

            CollectionAssert.AreEqual(new[] { "JNSQ" }, focused);
        }

        [Test]
        public void HandlePendingLaunchUrl_WithUrl_HandlesItOnce()
        {
            ProtocolRouter.PendingLaunchUrl = "ckan://focus?mod=JNSQ";

            // Screen can be re-entered but the URL must not be handled again.
            ProtocolRouter.HandlePendingLaunchUrl();
            ProtocolRouter.HandlePendingLaunchUrl();

            CollectionAssert.AreEqual(new[] { "JNSQ" }, focused);
            Assert.IsNull(ProtocolRouter.PendingLaunchUrl);
        }

        [Test]
        public void HandlePendingLaunchUrl_WithoutUrl_NoOp()
        {
            ProtocolRouter.HandlePendingLaunchUrl();

            Assert.IsEmpty(focused);
            Assert.IsEmpty(searched);
            Assert.IsEmpty(installed);
        }
    }
}
