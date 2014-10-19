using System.Reflection;

namespace CKAN
{
    public static class Meta
    {
        /// <summary>
        ///     Returns the version of the CKAN.dll used.
        /// </summary>
        public static string Version()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // SeriouslyLongestClassNamesEverThanksMicrosoft
            var attr =
                (AssemblyInformationalVersionAttribute[])
                    assembly.GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false);

            if (attr.Length == 0 || attr[0].InformationalVersion == null)
            {
                // Dunno the version. Some dev probably built it. 
                return "development";
            }
            return attr[0].InformationalVersion;
        }
    }
}