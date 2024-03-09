using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using CKAN.Extensions;

namespace CKAN.GUI
{
    public enum GUIModChangeType
    {
        [Display(Description  = "ChangeTypeNone",
                 ResourceType = typeof(Properties.Resources))]
        None    = 0,

        [Display(Description  = "ChangeTypeInstall",
                 ResourceType = typeof(Properties.Resources))]
        Install = 1,

        [Display(Description  = "ChangeTypeRemove",
                 ResourceType = typeof(Properties.Resources))]
        Remove  = 2,

        [Display(Description  = "ChangeTypeUpdate",
                 ResourceType = typeof(Properties.Resources))]
        Update  = 3,

        [Display(Description  = "ChangeTypeReplace",
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
        public ModChange(CkanModule mod, GUIModChangeType changeType)
            : this(mod, changeType, new SelectionReason.UserRequested())
        {
        }

        public ModChange(CkanModule mod, GUIModChangeType changeType, SelectionReason reason)
            : this(mod, changeType, Enumerable.Repeat(reason, 1))
        {
        }

        public ModChange(CkanModule mod, GUIModChangeType changeType, IEnumerable<SelectionReason> reasons)
        {
            Mod        = mod;
            ChangeType = changeType;
            Reasons    = reasons.ToArray();
            IsAutoRemoval   = Reasons.All(r => r is SelectionReason.NoLongerUsed);
            IsUserRequested = Reasons.All(r => r is SelectionReason.UserRequested);
        }

        public override bool Equals(object obj)
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

            return (obj as ModChange).Mod.Equals(Mod);
        }

        private static readonly int maxEnumVal = Enum.GetValues(typeof(GUIModChangeType)).Cast<int>().Max();

        public override int GetHashCode()
            // Distinguish between installing and removing
            => Mod == null
                ? 0
                : (((maxEnumVal + 1) * Mod.GetHashCode()) + (int)ChangeType);

        public override string ToString()
            => $"{ChangeType.Localize()} {Mod} ({Description})";

        public virtual string NameAndStatus
            => Main.Instance.Manager.Cache.DescribeAvailability(Mod);

        private string DescribeGroup(IEnumerable<SelectionReason> reasons)
            => reasons.First().DescribeWith(reasons.Skip(1));

        public virtual string Description
            => string.Join("; ",
                Reasons.GroupBy(r => r.GetType(), (t, reasons) =>
                    DescribeGroup(
                        // Avoid the reasons that throw exceptions for Parent
                        t.Equals(typeof(SelectionReason.Depends))
                            ? reasons.OrderBy(r => r.Parent.name)
                            : reasons)));
    }

    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public class ModUpgrade : ModChange
    {
        public ModUpgrade(CkanModule       mod,
                          GUIModChangeType changeType,
                          CkanModule       targetMod,
                          bool             userReinstall)
            : base(mod, changeType)
        {
            this.targetMod     = targetMod;
            this.userReinstall = userReinstall;
        }

        public override string NameAndStatus
            => Main.Instance.Manager.Cache.DescribeAvailability(targetMod);

        public override string Description
            => IsReinstall
                ? userReinstall ? Properties.Resources.MainChangesetUserReinstall
                                : Properties.Resources.MainChangesetReinstall
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
    }
}
