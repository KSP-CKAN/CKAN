using System;
using System.Collections.Generic;
using System.Linq;

namespace CKAN.GUI
{
    public enum GUIModChangeType
    {
        None    = 0,
        Install = 1,
        Remove  = 2,
        Update  = 3,
        Replace = 4
    }

    public static class GUIModChangeTypeExtensions
    {
        public static string ToI18nString(this GUIModChangeType val)
        {
            switch (val)
            {
                case GUIModChangeType.None:    return Properties.Resources.ChangeTypeNone;
                case GUIModChangeType.Install: return Properties.Resources.ChangeTypeInstall;
                case GUIModChangeType.Remove:  return Properties.Resources.ChangeTypeRemove;
                case GUIModChangeType.Update:  return Properties.Resources.ChangeTypeUpdate;
                case GUIModChangeType.Replace: return Properties.Resources.ChangeTypeReplace;
            }
            throw new NotImplementedException(val.ToString());
        }
    }

    /// <summary>
    /// Everything the GUI needs to know about a change, including
    /// the mod itself, the change we're making, and the reason why.
    /// </summary>
    public class ModChange
    {
        public CkanModule        Mod        { get; private set; }
        /// <summary>
        /// For changes involving another version in addition to the main one,
        /// this is that other version.
        /// When upgrading, the target version.
        /// Otherwise not used.
        /// </summary>
        public GUIModChangeType  ChangeType { get; private set; }
        public SelectionReason[] Reasons    { get; private set; }

        // If we don't have a Reason, the user probably wanted to install it
        public ModChange(CkanModule mod, GUIModChangeType changeType)
            : this(mod, changeType, new SelectionReason.UserRequested())
        {
        }

        public ModChange(CkanModule mod, GUIModChangeType changeType, SelectionReason reason)
            : this(mod, changeType, new SelectionReason[] { reason })
        {
        }

        public ModChange(CkanModule mod, GUIModChangeType changeType, IEnumerable<SelectionReason> reasons)
        {
            Mod        = mod;
            ChangeType = changeType;
            Reasons    = reasons.ToArray();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return (obj as ModChange).Mod.Equals(Mod);
        }

        private static int maxEnumVal = Enum.GetValues(typeof(GUIModChangeType)).Cast<int>().Max();

        public override int GetHashCode()
        {
            // Distinguish between installing and removing
            return Mod == null
                ? 0
                : ((maxEnumVal + 1) * Mod.GetHashCode() + (int)ChangeType);
        }

        public override string ToString()
        {
            return $"{ChangeType.ToI18nString()} {Mod} ({Description})";
        }

        protected string modNameAndStatus(CkanModule m)
        {
            return m.IsMetapackage
                ? string.Format(Properties.Resources.MainChangesetMetapackage, m.name, m.version)
                : Main.Instance.Manager.Cache.IsMaybeCachedZip(m)
                    ? string.Format(Properties.Resources.MainChangesetCached, m.name, m.version)
                    : string.Format(Properties.Resources.MainChangesetHostSize,
                        m.name, m.version, m.download.Host ?? "", CkanModule.FmtSize(m.download_size));
        }

        public virtual string NameAndStatus
        {
            get
            {
                return modNameAndStatus(Mod);
            }
        }

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

    public class ModUpgrade : ModChange
    {
        public ModUpgrade(CkanModule mod, GUIModChangeType changeType, IEnumerable<SelectionReason> reasons, CkanModule targetMod)
            : base(mod, changeType, reasons)
        {
            this.targetMod = targetMod;
        }

        public override string NameAndStatus
        {
            get
            {
                return modNameAndStatus(targetMod);
            }
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    Properties.Resources.MainChangesetUpdateSelected,
                    targetMod.version
                );
            }
        }

        public readonly CkanModule targetMod;
    }
}
