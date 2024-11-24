using CKAN.Extensions;

namespace CKAN.GUI
{
    public class ReleaseStatusItem
    {
        public ReleaseStatusItem(ReleaseStatus? value)
        {
            Value = value;
        }

        public readonly ReleaseStatus? Value;
        public override string ToString()
            => Value == null ? ""
                             : $"{Value.LocalizeName()} - {Value.LocalizeDescription()}";
    }
}
