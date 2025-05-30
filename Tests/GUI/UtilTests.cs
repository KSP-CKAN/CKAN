using System;
using System.Drawing;

using NUnit.Framework;

using CKAN.GUI;

namespace Tests.GUI
{
    [TestFixture]
    public class UtilTests
    {
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
