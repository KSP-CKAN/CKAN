using System;
using System.Threading;
using System.Drawing;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using NUnit.Framework;

using CKAN.GUI;

namespace Tests.GUI
{
    using Util = CKAN.GUI.Util;

    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [TestFixture]
    public class UtilTests
    {
        [Test]
        public void Debounce_Called_CallsBack()
        {
            // Arrange
            int startCount     = 0;
            int immediateCount = 0;
            int abortCount     = 0;
            int doneCount      = 0;
            var sut = Util.Debounce<string>((sender, e) =>
                                            {
                                                ++startCount;
                                            },
                                            (sender, e) =>
                                            {
                                                ++immediateCount;
                                                return false;
                                            },
                                            (sender, e) =>
                                            {
                                                ++abortCount;
                                                return false;
                                            },
                                            (sender, e) =>
                                            {
                                                ++doneCount;
                                            },
                                            50);

            // Act
            for (int pass = 0; pass < 3; ++pass)
            {
                for (int i = 0; i < 10; ++i)
                {
                    sut.Invoke(null, "");
                }
                Thread.Sleep(100);
            }

            // Assert
            Assert.AreEqual(30, startCount);
            Assert.AreEqual(30, immediateCount);
            Assert.AreEqual(30, abortCount);
            Assert.AreEqual(3,  doneCount);
        }

        [TestCase("https://github.com/KSP-CKAN/CKAN",        ExpectedResult = true),
         TestCase("http://status.ksp-ckan.space/",           ExpectedResult = true),
         TestCase("https://google.com/",                     ExpectedResult = true),
         TestCase("file://usr/lib/ckan/ckan.exe",            ExpectedResult = false),
         TestCase("ftp://cdrom.com/kernel/vmlinuz-1.0.0.gz", ExpectedResult = false),
         TestCase("/unix/path/without/protocol",             ExpectedResult = false),
         TestCase(@"C:\Windows\Path\Without\Protocol",       ExpectedResult = false)]
        public bool CheckURLValid_WithURL_ReturnsCorrectResult(string url)
            => Util.CheckURLValid(url);

        // https://learn.microsoft.com/en-us/dotnet/media/art-color-table.png
        [TestCase("White"),
         TestCase("OrangeRed"), TestCase("LightCoral"),
         TestCase("Yellow"), TestCase("Gold"), TestCase("Khaki"),
         TestCase("PaleGreen"), TestCase("Lime"),
         TestCase("#408080" /* Paradiso */),
         TestCase("SkyBlue"), TestCase("Aqua"), TestCase("LightBlue"), TestCase("DeepSkyBlue"),
         TestCase("Violet"),
         TestCase("Gray"),
        ]
        public void ForeColorForBackColor_LightBackColor_BlackForeColor(string colorName)
        {
            var c = ColorFromNameOrRGB(colorName);
            Assert.AreEqual(Color.Black, c.ForeColorForBackColor(),
                            $"Foreground color for {c.Name} (brightness {c.GetBrightness()}) should be Black");
        }

        [TestCase("Black"), TestCase("DarkSlateGray"),
         TestCase("DarkRed"), TestCase("DarkMagenta"), TestCase("Maroon"),
         TestCase("Green"),
         TestCase("DarkBlue"), TestCase("MidnightBlue"), TestCase("Navy"), TestCase("DarkSlateBlue"), TestCase("MediumBlue"),
         TestCase("Indigo"), TestCase("Purple"), TestCase("DarkViolet"),
         TestCase("#8000FF" /* ElectricViolet */),
         TestCase("Brown"), TestCase("Sienna"),
         TestCase("DimGray"),
        ]
        public void ForeColorForBackColor_DarkBackColor_WhiteForeColor(string colorName)
        {
            var c = ColorFromNameOrRGB(colorName);
            Assert.AreEqual(Color.White, c.ForeColorForBackColor(),
                            $"Foreground color for {c.Name} (brightness {c.GetBrightness()}) should be White");
        }

        private static Color ColorFromNameOrRGB(string nameOrRGB)
            => nameOrRGB.StartsWith("#") ? Color.FromArgb(StringToColorChannel(nameOrRGB, 1),
                                                          StringToColorChannel(nameOrRGB, 3),
                                                          StringToColorChannel(nameOrRGB, 5))
                                         : Color.FromName(nameOrRGB);

        private static int StringToColorChannel(string rgb, int start)
            => Convert.ToInt32(rgb.Substring(start, 2), 16);

    }
}
