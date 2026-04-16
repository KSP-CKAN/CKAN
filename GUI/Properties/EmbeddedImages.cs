using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// Provides icons after `dotnet build` completely broke non-string resources
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public static class EmbeddedImages
    {
        public static readonly Icon? AppIcon = Assembly.GetExecutingAssembly()
                                                       .GetManifestResourceStream($"CKAN.ckan.ico")
                                                   is Stream s
                                                       ? new Icon(s)
                                                       : null;

        public static Bitmap alert                => get("alert", Platform.IsWindows);
        public static Bitmap apply                => get("apply");
        public static Bitmap backward             => get("backward");
        public static Bitmap ballot               => get("ballot", Platform.IsWindows);
        public static Bitmap checkAll             => get("checkAll", true);
        public static Bitmap checkRecommendations => get("checkRecommendations", true);
        public static Bitmap checkSuggestions     => get("checkSuggestions", true);
        public static Bitmap collapseAll          => get("collapseAll", true);
        public static Bitmap delete               => get("delete", true);
        public static Bitmap expandAll            => get("expandAll", true);
        public static Bitmap file                 => get("file", Platform.IsWindows);
        public static Bitmap filter               => get("filter");
        public static Bitmap folder               => get("folder", Platform.IsWindows);
        public static Bitmap folderZip            => get("folderZip", Platform.IsWindows);
        public static Bitmap forward              => get("forward");
        public static Bitmap info                 => get("info", Platform.IsWindows);
        public static Bitmap ksp                  => get("ksp");
        public static Bitmap refresh              => get("refresh");
        public static Bitmap refreshStale         => get("refreshStale");
        public static Bitmap refreshVeryStale     => get("refreshVeryStale");
        public static Bitmap resetCollapse        => get("resetCollapse", true);
        public static Bitmap search               => get("search");
        public static Bitmap settings             => get("settings");
        public static Bitmap smile                => get("smile", Platform.IsWindows);
        public static Bitmap star                 => get("star", Platform.IsWindows);
        public static Bitmap stop                 => get("stop");
        public static Bitmap textClear            => get("textClear");
        public static Bitmap thumbup              => get("thumbup", Platform.IsWindows);
        public static Bitmap triToggleBoth        => get("triToggleBoth");
        public static Bitmap triToggleNo          => get("triToggleNo");
        public static Bitmap triToggleYes         => get("triToggleYes");
        public static Bitmap uncheckAll           => get("uncheckAll", true);
        public static Bitmap update               => get("update");

        private static Bitmap get(string what, bool invertIfDarkMode = false)
            => cache.TryGetValue(what, out Bitmap? bmp)
                ? bmp
                : add(what, invertIfDarkMode);

        private static Bitmap load(string what, bool invertIfDarkMode)
            => Assembly.GetExecutingAssembly()
                       .GetManifestResourceStream($"CKAN.GUI.Resources.{what}.png")
                   is Stream s
                       ? invertIfDarkMode && Util.DarkMode
                             ? new Bitmap(s).Inverted()
                             : new Bitmap(s)
                       : EmptyBitmap;

        private static Bitmap add(string what, bool invertIfDarkMode)
        {
            if (load(what, invertIfDarkMode) is Bitmap newBmp)
            {
                cache.Add(what, newBmp);
                return newBmp;
            }
            return EmptyBitmap;
        }

        private static readonly Dictionary<string, Bitmap> cache =
            new Dictionary<string, Bitmap>();
        private static readonly Bitmap EmptyBitmap = new Bitmap(1, 1);
    }
}
