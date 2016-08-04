using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal interface IValidator
    {
        void Validate(Metadata metadata);
    }
}