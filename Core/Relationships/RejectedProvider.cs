using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace CKAN
{
    public abstract class RejectedProvider
    {
        public readonly CkanModule provider;

        protected RejectedProvider(CkanModule provider)
        {
            this.provider = provider;
        }
    }

    public sealed class RejectedByRelationship : RejectedProvider
    {
        public readonly Relationship violation;

        public RejectedByRelationship(CkanModule provider, Relationship violation)
            : base(provider)
        {
            this.violation = violation;
        }

        public static IEnumerable<RejectedByRelationship> WrapMany(
                CkanModule                candidate,
                IEnumerable<Relationship> violations)
            => violations.Select(r => new RejectedByRelationship(candidate, r));

        [ExcludeFromCodeCoverage]
        public override bool Equals(object? other)
            => other is RejectedByRelationship r
               && provider.Equals(r.provider)
               && violation.Equals(r.violation);

        public override int GetHashCode()
            => (provider, violation).GetHashCode();
    }

    public sealed class RejectedByConflict : RejectedProvider
    {
        public readonly string?    sharedProvidesId;
        public readonly CkanModule blockingMod;
        public readonly bool       blockerIsInstalled;

        public RejectedByConflict(CkanModule provider,
                                  string?    sharedProvidesId,
                                  CkanModule blockingMod,
                                  bool       blockerIsInstalled)
            : base(provider)
        {
            this.sharedProvidesId   = sharedProvidesId;
            this.blockingMod        = blockingMod;
            this.blockerIsInstalled = blockerIsInstalled;
        }

        [ExcludeFromCodeCoverage]
        public override bool Equals(object? other)
            => other is RejectedByConflict r
               && provider.Equals(r.provider)
               && sharedProvidesId == r.sharedProvidesId
               && blockingMod.Equals(r.blockingMod)
               && blockerIsInstalled == r.blockerIsInstalled;

        public override int GetHashCode()
            => (provider, sharedProvidesId, blockingMod, blockerIsInstalled).GetHashCode();

        [ExcludeFromCodeCoverage]
        public override string ToString()
            => $"{provider}, {sharedProvidesId}, {blockingMod}, {blockerIsInstalled}";
    }

    public sealed class RejectedByVersionMismatch : RejectedProvider
    {
        public readonly CkanModule             blockingMod;
        public readonly ResolvedRelationship[] blockerChain;

        public RejectedByVersionMismatch(CkanModule              provider,
                                         CkanModule              blockingMod,
                                         ResolvedRelationship[]? blockerChain = null)
            : base(provider)
        {
            this.blockingMod  = blockingMod;
            this.blockerChain = blockerChain ?? Array.Empty<ResolvedRelationship>();
        }

        [ExcludeFromCodeCoverage]
        public override bool Equals(object? other)
            => other is RejectedByVersionMismatch r
               && provider.Equals(r.provider)
               && blockingMod.Equals(r.blockingMod);

        public override int GetHashCode()
            => (provider, blockingMod).GetHashCode();
    }

}
