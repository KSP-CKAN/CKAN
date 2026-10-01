using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Tests.Data;

using CKAN;
using CKAN.IO;
using CKAN.Versioning;

namespace Tests.Core.IO
{
    [TestFixture]
    public class InstallUrlPromptTests
    {
        private readonly RandomModuleGenerator gen = new RandomModuleGenerator(new Random(0451));

        [Test]
        public void Confirm_NothingUsable_RaisesErrorAndReturnsNothing()
        {
            var user = User(true);

            var plan = InstallUrlPrompt.Confirm(Resolved(("Foo", null, InstallOutcome.Unknown)),
                                                user, "KSP", "1.12.5");

            Assert.IsFalse(plan.Any);
            Assert.AreEqual(1, user.RaisedErrors.Count);
        }

        [Test]
        public void Confirm_AllReady_InstallsWithoutPrompting()
        {
            var mod = Mod("JNSQ");
            var user = User(false);

            var plan = InstallUrlPrompt.Confirm(Resolved(("JNSQ", mod, InstallOutcome.Ready)),
                                                user, "KSP", "1.12.5");

            CollectionAssert.AreEqual(new[] { mod }, plan.Install);
            Assert.IsEmpty(user.RaisedYesNoDialogQuestions);
        }

        [Test]
        public void Confirm_UserCancelsContinue_ReturnsNothing()
        {
            // A known mod so it isn't the none-found case, plus an unknown one to raise the continue dialog.
            var plan = InstallUrlPrompt.Confirm(Resolved(("JNSQ", Mod("JNSQ"), InstallOutcome.Ready),
                                                         ("Foo", null, InstallOutcome.Unknown)),
                                                User(false), "KSP", "1.12.5");

            Assert.IsFalse(plan.Any);
        }

        [Test]
        public void Confirm_PinMissed_ContinueThenInstalls()
        {
            var mod = Mod("JNSQ");
            var user = User(true);

            var plan = InstallUrlPrompt.Confirm(Resolved(("JNSQ", mod, InstallOutcome.PinMissed)),
                                                user, "KSP", "1.12.5");

            // The missed pin is noted in a continue dialog, then the latest is installed.
            Assert.AreEqual(1, user.RaisedYesNoDialogQuestions.Count);
            CollectionAssert.Contains(plan.Install, mod);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Confirm_Incompatible_InstalledOnlyIfConfirmed(bool accept)
        {
            var mod = Mod("JNSQ");

            var plan = InstallUrlPrompt.Confirm(Resolved(("JNSQ", mod, InstallOutcome.Incompatible)),
                                                User(accept), "KSP", "1.12.5");

            Assert.AreEqual(accept, plan.Install.Contains(mod));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Confirm_AlreadyInstalled_ReinstalledOnlyIfConfirmed(bool accept)
        {
            var mod = Mod("JNSQ");

            var plan = InstallUrlPrompt.Confirm(Resolved(("JNSQ", mod, InstallOutcome.AlreadyInstalled)),
                                                User(accept), "KSP", "1.12.5");

            Assert.AreEqual(accept, plan.Reinstall.Contains(mod));
        }

        [Test]
        public void Confirm_MixedOutcomes_AsksInOrderAndSplitsThePlan()
        {
            var ready = Mod("JNSQ");
            var incompatible = Mod("RealChute");
            var installed = Mod("Astrogator");

            // Continue past the unknown mod, take the incompatible one, don't reinstall.
            var user = User(true, true, false);

            var plan = InstallUrlPrompt.Confirm(
                Resolved(("JNSQ", ready, InstallOutcome.Ready),
                         ("NoSuchMod", null, InstallOutcome.Unknown),
                         ("RealChute", incompatible, InstallOutcome.Incompatible),
                         ("Astrogator", installed, InstallOutcome.AlreadyInstalled)),
                user, "KSP", "1.12.5");

            Assert.AreEqual(3, user.RaisedYesNoDialogQuestions.Count);
            CollectionAssert.AreEqual(new[] { ready, incompatible }, plan.Install);
            Assert.IsEmpty(plan.Reinstall);
        }

        private static List<ResolvedMod> Resolved(params (string Query, CkanModule? Module, InstallOutcome Outcome)[] mods)
            => mods.Select(m => new ResolvedMod(m.Query, m.Module, m.Outcome)).ToList();

        // Answers the yes/no dialogs in the order they are raised.
        private static CapturingUser User(params bool[] answers)
        {
            int asked = 0;
            return new CapturingUser(false, _ => answers[asked++], (_, _) => 0);
        }

        private CkanModule Mod(string identifier)
            => gen.GenerateRandomModule(identifier: identifier, version: new ModuleVersion("1.0"));
    }
}
