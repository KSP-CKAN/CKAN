namespace CKAN
{
    public class ModuleReplacement
    {
        public ModuleReplacement(CkanModule toReplace, CkanModule replaceWith)
        {
            ToReplace   = toReplace;
            ReplaceWith = replaceWith;
        }

        public readonly CkanModule ToReplace;
        public readonly CkanModule ReplaceWith;
    }
}
