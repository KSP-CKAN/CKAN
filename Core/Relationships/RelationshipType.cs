using System.ComponentModel.DataAnnotations;

namespace CKAN
{
    public enum RelationshipType
    {
        [Display(Name         = "RelationshipTypeProvides",
                 Description  = "RelationshipTypeProvides",
                 ResourceType = typeof(Properties.Resources))]
        Provides   = 0,

        [Display(Name         = "RelationshipTypeDepends",
                 Description  = "RelationshipTypeDepends",
                 ResourceType = typeof(Properties.Resources))]
        Depends    = 1,

        [Display(Name         = "RelationshipTypeRecommends",
                 Description  = "RelationshipTypeRecommends",
                 ResourceType = typeof(Properties.Resources))]
        Recommends = 2,

        [Display(Name         = "RelationshipTypeSuggests",
                 Description  = "RelationshipTypeSuggests",
                 ResourceType = typeof(Properties.Resources))]
        Suggests   = 3,

        [Display(Name         = "RelationshipTypeSupports",
                 Description  = "RelationshipTypeSupports",
                 ResourceType = typeof(Properties.Resources))]
        Supports   = 4,

        [Display(Name         = "RelationshipTypeConflicts",
                 Description  = "RelationshipTypeConflicts",
                 ResourceType = typeof(Properties.Resources))]
        Conflicts  = 5,
    }
}
