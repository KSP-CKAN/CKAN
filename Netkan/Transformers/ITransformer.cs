using System.Collections.Generic;
﻿using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Transformers
{
    internal class TransformOptions
    {
        public TransformOptions(int? releases)
        {
            Releases = releases;
        }

        public readonly int? Releases;
    }

    /// <summary>
    /// Represents an object that can perform transformations on NetKAN metadata.
    /// </summary>
    internal interface ITransformer
    {
        /// <summary>
        /// A unique name which identifies the transformer.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Transform the given metadata.
        /// </summary>
        /// <param name="metadata">The metadata to transform.</param>
        /// <returns>The transformed metadata.</returns>
        IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts);
    }
}
