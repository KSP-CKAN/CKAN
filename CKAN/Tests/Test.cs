using NUnit.Framework;
using System;
using CKAN;
using System.Text.RegularExpressions;

namespace Tests
{
    [TestFixture()]
    public class KSP
    {



        // Disabled, because Travis machines don't have a KSP intall.
        // TODO: How do we mark tests as 'TODO' in Nunit? Does it even have that?
        // [Test()]
        public void TestCase ()
        {

            string gameData = CKAN.KSP.gameData ();

            Assert.IsTrue (Regex.IsMatch (gameData, "GameData/?$", RegexOptions.IgnoreCase));
        }
    }
}

