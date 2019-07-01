
namespace CKAN
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
        public GUIModChangeType ChangeType { get; private set; }
        public SelectionReason  Reason     { get; private set; }

        public ModChange(CkanModule mod, GUIModChangeType changeType, SelectionReason reason)
        {
            Mod        = mod;
            ChangeType = changeType;
            Reason     = reason;

            if (Reason == null)
            {
                // Hey, we don't have a Reason
                // Most likely the user wanted to install it
                Reason = new SelectionReason.UserRequested();
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return (obj as ModChange).Mod.Equals(Mod);
        }

        public override int GetHashCode()
        {
            // Distinguish between installing and removing
            return Mod == null
                ? 0
                : (4 * Mod.GetHashCode() + (int)ChangeType);
        }

        public override string ToString()
        {
            return $"{ChangeType} {Mod} ({Reason})";
        }
    }
}
