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

        public static Bitmap alert                => get("alert");
        public static Bitmap apply                => get("apply");
        public static Bitmap backward             => get("backward");
        public static Bitmap ballot               => get("ballot");
        public static Bitmap checkAll             => get("checkAll");
        public static Bitmap checkRecommendations => get("checkRecommendations");
        public static Bitmap checkSuggestions     => get("checkSuggestions");
        public static Bitmap collapseAll          => get("collapseAll");
        public static Bitmap delete               => get("delete");
        public static Bitmap expandAll            => get("expandAll");
        public static Bitmap file                 => get("file");
        public static Bitmap filter               => get("filter");
        public static Bitmap folder               => get("folder");
        public static Bitmap folderZip            => get("folderZip");
        public static Bitmap forward              => get("forward");
        public static Bitmap info                 => get("info");
        public static Bitmap ksp                  => get("ksp");
        public static Bitmap refresh              => get("refresh");
        public static Bitmap refreshStale         => get("refreshStale");
        public static Bitmap refreshVeryStale     => get("refreshVeryStale");
        public static Bitmap resetCollapse        => get("resetCollapse");
        public static Bitmap search               => get("search");
        public static Bitmap settings             => get("settings");
        public static Bitmap smile                => get("smile");
        public static Bitmap star                 => get("star");
        public static Bitmap stop                 => get("stop");
        public static Bitmap textClear            => get("textClear");
        public static Bitmap thumbup              => get("thumbup");
        public static Bitmap triToggleBoth        => get("triToggleBoth");
        public static Bitmap triToggleNo          => get("triToggleNo");
        public static Bitmap triToggleYes         => get("triToggleYes");
        public static Bitmap uncheckAll           => get("uncheckAll");
        public static Bitmap update               => get("update");

        private static Bitmap get(string what)
            => cache.TryGetValue(what, out Bitmap? bmp)
                ? bmp
                : add(what);

        private static Bitmap load(string what)
            => Assembly.GetExecutingAssembly()
                       .GetManifestResourceStream($"CKAN.GUI.Resources.{what}.png")
                   is Stream s
                       ? new Bitmap(s)
                       : EmptyBitmap;

        private static Bitmap add(string what)
        {
            if (load(what) is Bitmap newBmp)
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
