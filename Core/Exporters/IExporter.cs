using System.IO;

namespace CKAN.Exporters
{
    /// <summary>
    /// Represents an object that can export a list of mods in a machine or human readable format.
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// Export installed mods.
        /// </summary>
        /// <param name="registry">The registry of mods to be exported.</param>
        /// <param name="stream">The output stream to be written to.</param>
        void Export(Registry registry, Stream stream);
    }
}
