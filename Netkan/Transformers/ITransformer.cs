using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// Represents an object that can perform transformations on NetKAN metadata.
    /// </summary>
    internal interface ITransformer
    {
        /// <summary>
        /// Transform the given metadata.
        /// </summary>
        /// <param name="metadata">The metadata to transform.</param>
        /// <returns>The transformed metadata.</returns>
        Metadata Transform(Metadata metadata);
    }
}
