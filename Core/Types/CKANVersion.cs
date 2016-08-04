namespace CKAN.Types
{
    public class CKANVersion : Version
    {
        private readonly string name;

        public string Name
        {
            get { return name; }
        }

        public CKANVersion(string version, string name)
            : base(version)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return base.ToString() + " aka " + name;
        }
    }
}