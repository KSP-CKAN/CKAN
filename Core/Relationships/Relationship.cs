namespace CKAN
{
    public class Relationship
    {
        public Relationship(CkanModule             source,
                            RelationshipType       type,
                            RelationshipDescriptor descr)
        {
            Source     = source;
            Type       = type;
            Descriptor = descr;
        }

        public readonly CkanModule             Source;
        public readonly RelationshipType       Type;
        public readonly RelationshipDescriptor Descriptor;
    }
}
