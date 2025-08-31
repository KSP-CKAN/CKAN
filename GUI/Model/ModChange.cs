using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.Configuration;
using CKAN.Extensions;

namespace CKAN.GUI
{
    public enum GUIModChangeType
    {
        [Display(Name         = "ChangeTypeNone",
                 Description  = "ChangeTypeNone",
                 ResourceType = typeof(Properties.Resources))]
        None    = 0,

        [Display(Name         = "ChangeTypeInstall",
                 Description  = "ChangeTypeInstall",
                 ResourceType = typeof(Properties.Resources))]
        Install = 1,

        [Display(Name         = "ChangeTypeRemove",
                 Description  = "ChangeTypeRemove",
                 ResourceType = typeof(Properties.Resources))]
        Remove  = 2,

        [Display(Name         = "ChangeTypeUpdate",
                 Description  = "ChangeTypeUpdate",
                 ResourceType = typeof(Properties.Resources))]
        Update  = 3,

        [Display(Name         = "ChangeTypeReplace",
                 Description  = "ChangeTypeReplace",
                 ResourceType = typeof(Properties.Resources))]
        Replace = 4,
    }

    /// <summary>
    /// Everything the GUI needs to know about a change, including
    /// the mod itself, the change we're making, and the reason why.
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class ModChange
    {
        public CkanModule        Mod        { get; private set; }
        public GUIModChangeType  ChangeType { get; private set; }
        public SelectionReason[] Reasons    { get; private set; }

        /// <summary>
        /// true if the reason for this change is that an installed dependency is no longer needed,
        /// false otherwise
        /// </summary>
        public readonly bool IsAutoRemoval;

        /// <summary>
        /// true if this change is user requested and no other changes depend on it, false otherwise.
        /// </summary>
        public readonly bool IsUserRequested;

        /// <summary>
        /// true if this change can be removed from a changeset, false otherwise
        /// </summary>
        public bool IsRemovable => IsAutoRemoval || IsUserRequested;

        // If we don't have a Reason, the user probably wanted to install it
        public ModChange(CkanModule       mod,
                         GUIModChangeType changeType,
                         IConfiguration   config)
            : this(mod, changeType, new SelectionReason.UserRequested(), config)
        {
        }

        public ModChange(CkanModule       mod,
                         GUIModChangeType changeType,
                         SelectionReason  reason,
                         IConfiguration   config)
            : this(mod, changeType, Enumerable.Repeat(reason, 1), config)
        {
        }

        public ModChange(CkanModule                   mod,
                         GUIModChangeType             changeType,
                         IEnumerable<SelectionReason> reasons,
                         IConfiguration               config)
        {
            Mod        = mod;
            ChangeType = changeType;
            Reasons    = reasons.ToArray();
            this.config = config;
            IsAutoRemoval   = Reasons.All(r => r is SelectionReason.NoLongerUsed);
            IsUserRequested = Reasons.All(r => r is SelectionReason.UserRequested);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return (obj as ModChange)?.Mod.Equals(Mod) ?? false;
        }

        private static readonly int maxEnumVal = Enum.GetValues(typeof(GUIModChangeType)).Cast<int>().Max();

        public override int GetHashCode()
            // Distinguish between installing and removing
            => Mod == null
                ? 0
                : (((maxEnumVal + 1) * Mod.GetHashCode()) + (int)ChangeType);

        public override string ToString()
            => $"{ChangeType.LocalizeDescription()} {Mod} ({Description})";

        public virtual string? NameAndStatus
            => Main.Instance?.Manager?.Cache?.DescribeAvailability(config, Mod);

        private static string DescribeGroup(IEnumerable<SelectionReason> reasons)
            => reasons.First().DescribeWith(reasons.Skip(1));

        public virtual string Description
            => string.Join("; ",
                Reasons.GroupBy(r => r.GetType(), (t, reasons) =>
                    DescribeGroup(
                        t.IsSubclassOf(typeof(SelectionReason.RelationshipReason))
                            ? reasons.OfType<SelectionReason.RelationshipReason>()
                                     .OrderBy(r => r.Parent.name)
                            : reasons)));

        protected IConfiguration config;
    }

    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class ModUpgrade : ModChange
    {
        public ModUpgrade(CkanModule       mod,
                          CkanModule       targetMod,
                          bool             userReinstall,
                          bool             metadataChanged,
                          IConfiguration   config)
            : base(mod, GUIModChangeType.Update, config)
        {
            this.targetMod       = targetMod;
            this.userReinstall   = userReinstall;
            this.metadataChanged = metadataChanged;
        }

        public override string? NameAndStatus
            => Main.Instance?.Manager?.Cache?.DescribeAvailability(config, targetMod);

        public override string Description
            => IsReinstall
                ? userReinstall ? Properties.Resources.MainChangesetReinstallUser
                                : metadataChanged ? Properties.Resources.MainChangesetReinstallMetadataChanged
                                                  : Properties.Resources.MainChangesetReinstallMissing
                : string.Format(Properties.Resources.MainChangesetUpdateSelected,
                                targetMod.version);

        /// <summary>
        /// The target version for upgrading
        /// </summary>
        public CkanModule targetMod;

        private bool IsReinstall
            => targetMod.identifier == Mod.identifier
                && targetMod.version == Mod.version;

        private readonly bool userReinstall;
        private readonly bool metadataChanged;
    }
}
