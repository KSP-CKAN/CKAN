using NUnit.Framework;
using System;
using CKAN;
using System.Text.RegularExpressions;

namespace Tests
{
	[TestFixture()]
	public class KSP
	{
		[Test()]
		public void TestCase ()
		{

			string gameData = CKAN.KSP.gameData ();

			Assert.IsTrue (Regex.IsMatch (gameData, "GameData/?$", RegexOptions.IgnoreCase));
		}
	}
}

