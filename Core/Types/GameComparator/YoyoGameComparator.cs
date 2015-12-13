using System;

namespace CKAN
{
    /// <summary>
    /// You're On Your Own (YOYO) game compatibility comparison.
    /// This claims everything is compatible with everything.
    /// </summary>
    public class YoyoGameComparator : IGameComparator
    {
        public bool Compatible(KSPVersion gameVersion, CkanModule module)
        {
            return true;
        }
    }
}