using System.IO;

namespace CKAN.Exporters
{
    /// <summary>
    /// Represents an object that can 
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="writer"></param>
        void Export(Registry registry, TextWriter writer);
    }
}
