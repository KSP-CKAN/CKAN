namespace CKAN.Versioning
{
    /// <summary>
    /// This class represents a DllVersion. They don't have real version numbers or anything
    /// </summary>
    public sealed class DllVersion : Version
    {
        public DllVersion() : base("0") { }

        public override string ToString()
        {
            return AutodetectedDllString;
        }
    }
}
