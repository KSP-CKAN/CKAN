using System;
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

    /// <summary>
    /// Everything the GUI needs to know about a change, including
    /// the mod itself, the change we're making, and the reason why.
    /// </summary>
    public class ModChange
    {
        public CkanModule       Mod        { get; private set; }
        /// <summary>
        /// For changes involving another version in addition to the main one,
        /// this is that other version.
        /// When upgrading, the target version.
        /// Otherwise not used.
        /// </summary>
        public GUIModChangeType ChangeType { get; private set; }
        public SelectionReason  Reason     { get; private set; }

        public ModChange(CkanModule mod, GUIModChangeType changeType, SelectionReason reason)
        {
            Mod        = mod;
            ChangeType = changeType;
            // If we don't have a Reason, the user probably wanted to install it
            Reason     = reason ?? new SelectionReason.UserRequested();
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
            return $"{ChangeType} {Mod} ({Reason})";
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

        public virtual string Description
        {
            get
            {
                return Reason.Reason.Trim();
            }
        }
    }

    public class ModUpgrade : ModChange
    {
        public ModUpgrade(CkanModule mod, GUIModChangeType changeType, SelectionReason reason, CkanModule targetMod)
            : base(mod, changeType, reason)
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
