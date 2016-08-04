using CKAN.Types;

namespace CKAN
{
    internal sealed class ExportOption
    {
        private readonly string _string;

        public ExportFileType ExportFileType { get; private set; }
        public string FriendlyName { get; private set; }
        public string Extension { get; private set; }

        public ExportOption(ExportFileType exportFileType, string friendlyName, string extension)
        {
            ExportFileType = exportFileType;
            FriendlyName = friendlyName;
            Extension = extension;

            _string = string.Format("{0}|{1}", FriendlyName, "*." + Extension);
        }

        public override string ToString()
        {
            return _string;
        }
    }
}