using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace CKAN
{
    public sealed class UnsatisfiedRelation
    {
        /// <summary>
        /// The dependency chain to reach this relationship.
        /// </summary>
        public readonly ResolvedRelationship[] depends;

        /// <summary>
        /// The reason that this relationship could not be satisfied, if any.
        /// </summary>
        public readonly RejectedProvider? rejection;

        public UnsatisfiedRelation(ResolvedRelationship[] depends,
                                   RejectedProvider?      rejection)
        {
            this.depends   = depends;
            this.rejection = rejection;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
            => string.Join(Environment.NewLine + Environment.NewLine,
                           depends.Select(d => d.ToString())
                                  .Prepend(rejection?.ToString() ?? ""));
    }
}
