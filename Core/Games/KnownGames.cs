using System.Collections.Generic;
using System.Linq;


namespace CKAN.Games
{
    public static class KnownGames
    {
        public static readonly IGame[] knownGames = new IGame[]
        {
            new KerbalSpaceProgram.KerbalSpaceProgram(),
            new KerbalSpaceProgram2.KerbalSpaceProgram2(),
        };

        /// <summary>
        /// Return a game object based on its short name
        /// </summary>
        /// <param name="shortName">The short name to find</param>
        /// <returns>A game object or null if none found</returns>
        public static IGame GameByShortName(string shortName)
            => knownGames.FirstOrDefault(g => g.ShortName == shortName);

        /// <summary>
        /// Return the short names of all known games
        /// </summary>
        /// <returns>Sequence of short name strings</returns>
        public static IEnumerable<string> AllGameShortNames()
            => knownGames.Select(g => g.ShortName);

    }
}
