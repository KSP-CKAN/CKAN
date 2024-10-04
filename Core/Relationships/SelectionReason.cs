using System;
using System.Collections.Generic;
using System.Linq;

namespace CKAN
{
    /// <summary>
    /// Used to keep track of the relationships between modules in the resolver.
    /// Intended to be used for displaying messages to the user.
    /// </summary>
    public abstract class SelectionReason : IEquatable<SelectionReason>
    {
        // Currently assumed to exist for any relationship other than UserRequested or Installed
        public virtual string DescribeWith(IEnumerable<SelectionReason> others)
            => ToString() ?? "";

        public override bool Equals(object? obj)
            => Equals(obj as SelectionReason);

        public bool Equals(SelectionReason? rsn)
            => GetType() == rsn?.GetType();

        public override int GetHashCode()
            => GetType().GetHashCode();

        public virtual SelectionReason WithIndex(int providesIndex)
            => this;

        public class Installed : SelectionReason
        {
            public override string ToString()
                => Properties.Resources.RelationshipResolverInstalledReason;
        }

        public class UserRequested : SelectionReason
        {
            public override string ToString()
                => Properties.Resources.RelationshipResolverUserReason;
        }

        public class DependencyRemoved : SelectionReason
        {
            public override string ToString()
                => Properties.Resources.RelationshipResolverDependencyRemoved;
        }

        public class NoLongerUsed : SelectionReason
        {
            public override string ToString()
                => Properties.Resources.RelationshipResolverNoLongerUsedReason;
        }

        public abstract class RelationshipReason : SelectionReason, IEquatable<RelationshipReason>
        {
            public RelationshipReason(CkanModule parent)
            {
                Parent = parent;
            }

            public CkanModule Parent;

            public bool Equals(RelationshipReason? rsn)
                => GetType() == rsn?.GetType()
                   && Parent == rsn?.Parent;

            public override bool Equals(object? obj)
                => Equals(obj as RelationshipReason);

            public override int GetHashCode()
            {
                var type = GetType();
                #if NET5_0_OR_GREATER
                return HashCode.Combine(type, Parent);
                #else
                unchecked
                {
                    return (type, Parent).GetHashCode();
                }
                #endif
            }

        }

        public class Replacement : RelationshipReason
        {
            public Replacement(CkanModule module)
                : base(module)
            {
            }

            public override string ToString()
                => string.Format(Properties.Resources.RelationshipResolverReplacementReason,
                                 Parent.name);

            public override string DescribeWith(IEnumerable<SelectionReason> others)
                => string.Format(Properties.Resources.RelationshipResolverReplacementReason,
                                 string.Join(", ",
                                             Enumerable.Repeat(this, 1)
                                                       .Concat(others)
                                                       .OfType<RelationshipReason>()
                                                       .Select(r => r.Parent.name)));
        }

        public sealed class Suggested : RelationshipReason
        {
            public Suggested(CkanModule module)
                : base(module)
            {
            }

            public override string ToString()
                => string.Format(Properties.Resources.RelationshipResolverSuggestedReason,
                                 Parent.name);
        }

        public sealed class Depends : RelationshipReason
        {
            public Depends(CkanModule module)
                : base(module)
            {
            }

            public override string ToString()
                => string.Format(Properties.Resources.RelationshipResolverDependsReason,
                                 Parent.name);

            public override string DescribeWith(IEnumerable<SelectionReason> others)
                => string.Format(Properties.Resources.RelationshipResolverDependsReason,
                                 string.Join(", ",
                                             Enumerable.Repeat(this, 1)
                                                       .Concat(others)
                                                       .OfType<RelationshipReason>()
                                                       .Select(r => r.Parent.name)));
        }

        public sealed class Recommended : RelationshipReason
        {
            public Recommended(CkanModule module, int providesIndex)
                : base(module)
            {
                ProvidesIndex = providesIndex;
            }

            public readonly int ProvidesIndex;

            public override SelectionReason WithIndex(int providesIndex)
                => new Recommended(Parent, providesIndex);

            public override string ToString()
                => string.Format(Properties.Resources.RelationshipResolverRecommendedReason,
                                 Parent.name);
        }
    }
}
