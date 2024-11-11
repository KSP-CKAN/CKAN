using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace CKAN
{
    [JsonConverter(typeof(JsonReleaseStatusConverter))]
    public enum ReleaseStatus
    {
        [Display(Name         = "ReleaseStatusStableName",
                 Description  = "ReleaseStatusStableDescription",
                 ResourceType = typeof(Properties.Resources))]
        stable      = 0,
        [Display(Name         = "ReleaseStatusTestingName",
                 Description  = "ReleaseStatusTestingDescription",
                 ResourceType = typeof(Properties.Resources))]
        testing     = 1,
        [Display(Name         = "ReleaseStatusDevelopmentName",
                 Description  = "ReleaseStatusDevelopmentDescription",
                 ResourceType = typeof(Properties.Resources))]
        development = 2,
    }
}
