using CKAN.Configuration;

namespace CKAN
{
    // TODO: It would be lovely to get rid of the `without` fields,
    // and replace them with `with` fields. Humans suck at inverting
    // cases in their heads.
    public class RelationshipResolverOptions
    {
        public RelationshipResolverOptions(StabilityToleranceConfig stabTolCfg)
        {
            stability_tolerance = stabTolCfg;
        }

        /// <summary>
        /// Default options for relationship resolution.
        /// </summary>
        public static RelationshipResolverOptions DefaultOpts(StabilityToleranceConfig stabTolCfg)
            => new RelationshipResolverOptions(stabTolCfg);

        /// <summary>
        /// Options to install without recommendations.
        /// </summary>
        public static RelationshipResolverOptions DependsOnlyOpts(StabilityToleranceConfig stabTolCfg)
            => new RelationshipResolverOptions(stabTolCfg)
            {
                with_recommends   = false,
                with_suggests     = false,
                with_all_suggests = false,
            };

        /// <summary>
        /// Options to find all dependencies, recommendations, and suggestions
        /// of anything in the changeset (except when suppress_recommendations==true),
        /// without throwing exceptions, so the calling code can decide what to do about conflicts
        /// </summary>
        public static RelationshipResolverOptions KitchenSinkOpts(StabilityToleranceConfig stabTolCfg)
            => new RelationshipResolverOptions(stabTolCfg)
            {
                with_recommends                = true,
                with_suggests                  = true,
                without_toomanyprovides_kraken = true,
                without_enforce_consistency    = true,
                proceed_with_inconsistencies   = true,
                get_recommenders               = true,
            };

        public static RelationshipResolverOptions ConflictsOpts(StabilityToleranceConfig stabTolCfg)
            => new RelationshipResolverOptions(stabTolCfg)
            {
                without_toomanyprovides_kraken = true,
                proceed_with_inconsistencies   = true,
                without_enforce_consistency    = true,
                with_recommends                = false,
            };

        /// <summary>
        /// If true, add recommended mods, and their recommendations.
        /// </summary>
        public bool with_recommends = true;

        /// <summary>
        /// If true, add suggests, but not suggested suggests. :)
        /// </summary>
        public bool with_suggests = false;

        /// <summary>
        /// If true, add suggested modules, and *their* suggested modules, too!
        /// </summary>
        public bool with_all_suggests = false;

        /// <summary>
        /// If true, surpresses the TooManyProvides kraken when resolving
        /// relationships. Otherwise, we just pick the first.
        /// </summary>
        public bool without_toomanyprovides_kraken = false;

        /// <summary>
        /// If true, we skip our sanity check at the end of our relationship
        /// resolution. Note that non-sane resolutions can't actually be
        /// installed, so this is mostly useful for giving the user feedback
        /// on failed resolutions.
        /// </summary>
        public bool without_enforce_consistency = false;

        /// <summary>
        /// If true, we'll populate the `conflicts` field, rather than immediately
        /// throwing a kraken when inconsistencies are detected. Again, these
        /// solutions are non-installable, so mostly of use to provide user
        /// feedback when things go wrong.
        /// </summary>
        public bool proceed_with_inconsistencies = false;

        /// <summary>
        /// If true, then if a module has no versions that are compatible with
        /// the current game version, then we will consider incompatible versions
        /// of that module.
        /// This replaces the former behavior of ignoring compatibility for
        /// `install identifier=version` commands.
        /// </summary>
        public bool allow_incompatible = false;

        /// <summary>
        /// If true, get the list of mods that should be checked for
        /// recommendations and suggestions.
        /// Differs from normal resolution in that it stops when
        /// ModuleRelationshipDescriptor.suppress_recommendations==true
        /// </summary>
        public bool get_recommenders = false;

        /// <summary>
        /// The least stable category of mods to allow
        /// </summary>
        public StabilityToleranceConfig? stability_tolerance;

        public RelationshipResolverOptions OptionsFor(RelationshipDescriptor descr)
            => descr.suppress_recommendations ? WithoutRecommendations() : this;

        public RelationshipResolverOptions WithoutRecommendations()
        {
            if (with_recommends || with_all_suggests || with_suggests)
            {
                var newOptions = (RelationshipResolverOptions)MemberwiseClone();
                newOptions.with_recommends   = false;
                newOptions.with_all_suggests = false;
                newOptions.with_suggests     = false;
                return newOptions;
            }
            return this;
        }

        public RelationshipResolverOptions WithoutSuggestions()
        {
            if (with_suggests)
            {
                var newOptions = (RelationshipResolverOptions)MemberwiseClone();
                newOptions.with_suggests = false;
                return newOptions;
            }
            return this;
        }

        public OptionalRelationships OptionalHandling()
            => (with_all_suggests ? OptionalRelationships.AllSuggestions | OptionalRelationships.Suggestions
                                  : OptionalRelationships.None)
               | (with_suggests   ? OptionalRelationships.Suggestions
                                  : OptionalRelationships.None)
               | (with_recommends ? OptionalRelationships.Recommendations
                                  : OptionalRelationships.None);

    }

}
