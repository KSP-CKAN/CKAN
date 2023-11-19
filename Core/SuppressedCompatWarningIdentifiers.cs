using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using log4net;

using CKAN.Versioning;

namespace CKAN
{
    public class SuppressedCompatWarningIdentifiers
    {
        public GameVersion GameVersionWhenWritten;
        public HashSet<string> Identifiers = new HashSet<string>();

        public static SuppressedCompatWarningIdentifiers LoadFrom(GameVersion gameVer, string filename)
        {
            try
            {
                var saved = JsonConvert.DeserializeObject<SuppressedCompatWarningIdentifiers>(File.ReadAllText(filename));
                // Reset warnings if e.g. Steam auto-updates the game
                if (saved.GameVersionWhenWritten == gameVer)
                {
                    return saved;
                }
            }
            catch (Exception exc)
            {
                log.Debug("Failed to load", exc);
            }
            return new SuppressedCompatWarningIdentifiers()
            {
                GameVersionWhenWritten = gameVer
            };
        }

        public void SaveTo(string filename)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(this));
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(SuppressedCompatWarningIdentifiers));
    }
}
