namespace CKAN
{
    /// <summary>
    /// Everything the GUI needs to know about a change, including
    /// the mod itself, the change we're making, and the reason why.
    /// </summary>
    public class ModChange
    {
        public GUIMod Mod { get; private set; }
        public GUIModChangeType ChangeType { get; private set; }
        public SelectionReason Reason { get; private set; }

        public ModChange(GUIMod mod, GUIModChangeType changeType, SelectionReason reason)
        {
            Mod = mod;
            ChangeType = changeType;
            Reason = reason;

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
            return (obj as ModChange).Mod.Identifier == Mod.Identifier;
        }

        public override int GetHashCode()
        {
            return Mod != null ? Mod.Identifier.GetHashCode() : 0;
        }
    }
}