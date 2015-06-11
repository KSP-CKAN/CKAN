using System.IO;

namespace CKAN.Exporters
{
    /// <summary>
    /// Represents an object that can export a list of mods in a machine/human readable text format.
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// Export the installed mods.
        /// </summary>
        /// <param name="registry">The registry of mods to be exported.</param>
        /// <param name="writer">The text writer to write the export to.</param>
        void Export(Registry registry, TextWriter writer);
    }
}
