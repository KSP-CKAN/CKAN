using System.Drawing;
using Autofac;
using CKAN.Configuration;
using CKAN.GUI.Styling;

namespace CKAN.GUI
{
    namespace Styling
    {
        // Set of fonts used accross GUI
        public static class Fonts
        {
            private static readonly float FontScale;
            public static Font Regular;
            public static Font Bold;
            public static Font Italic;
            public static Font Strikeout;
            static Fonts()
            {
                var config = ServiceLocator.Container.Resolve<IConfiguration>();
                FontScale = config.FontScale;

                Regular = new Font(SystemFonts.DefaultFont.FontFamily, SystemFonts.DefaultFont.Size * FontScale, SystemFonts.DefaultFont.Style);
                Bold = new Font(Regular, FontStyle.Bold);
                Italic = new Font(Regular, FontStyle.Italic);
                Strikeout = new Font(Regular, FontStyle.Strikeout);
            }
        }
    }
    // Wrppers to make setting scaled default font easier
    namespace Forms
    {
        public class Form : System.Windows.Forms.Form
        {
            public Form() : base()
            {
                Font = Fonts.Regular;
            }
        }
        public class MenuStrip : System.Windows.Forms.MenuStrip
        {
            public MenuStrip() : base()
            {
                Font = Fonts.Regular;
            }
        }
        public class ToolStripMenuItem : System.Windows.Forms.ToolStripMenuItem
        {
            public ToolStripMenuItem() : base()
            {
                Font = Fonts.Regular;
            }
        }
    }
}
