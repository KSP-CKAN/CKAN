using System.Linq;
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
         TestCase("SkyBlue"), TestCase("Aqua"), TestCase("LightBlue"), TestCase("DeepSkyBlue"),
         TestCase("Violet"),
         TestCase("Gray"),
        ]
        public void ForeColorForBackColor_LightBackColor_BlackForeColor(string colorName)
        {
            var c = Color.FromName(colorName);
            Assert.AreEqual(Color.Black, c.ForeColorForBackColor(),
                            $"Foreground color for {c.Name} (brightness {c.GetBrightness()}) should be Black");
        }

        [TestCase("Black"), TestCase("DarkSlateGray"),
         TestCase("DarkRed"), TestCase("DarkMagenta"), TestCase("Maroon"),
         TestCase("Green"),
         TestCase("DarkBlue"), TestCase("MidnightBlue"), TestCase("Navy"), TestCase("DarkSlateBlue"), TestCase("MediumBlue"),
         TestCase("Indigo"), TestCase("Purple"), TestCase("DarkViolet"),
         TestCase("Brown"), TestCase("Sienna"),
         TestCase("DimGray"),
        ]
        public void ForeColorForBackColor_DarkBackColor_WhiteForeColor(string colorName)
        {
            var c = Color.FromName(colorName);
            Assert.AreEqual(Color.White, c.ForeColorForBackColor(),
                            $"Foreground color for {c.Name} (brightness {c.GetBrightness()}) should be White");
        }
    }
}
