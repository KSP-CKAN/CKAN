using System;
using System.Reflection;

namespace CKAN
{
    public static class Meta
    {
        public static string GetVersion(VersionFormat format = VersionFormat.Normal)
        {
            var version = ((AssemblyInformationalVersionAttribute)
                Assembly
                    .GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0]
            ).InformationalVersion;

            var dashIndex = version.IndexOf('-');
            var plusIndex = version.IndexOf('+');

            switch (format)
            {
                case VersionFormat.Short:
                    if (dashIndex >= 0)
                        version = version.Substring(0, dashIndex);
                    else if (plusIndex >= 0)
                        version = version.Substring(0, plusIndex);

                    break;
                case VersionFormat.Normal:
                    if (plusIndex >= 0)
                        version = version.Substring(0, plusIndex);

                    break;
                case VersionFormat.Full:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            return "v" + version;
        }
    }
}
