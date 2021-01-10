using System;
using System.IO;
using System.Collections.Generic;
using CKAN.Versioning;

namespace CKAN.Games
{
    public interface IGame
    {
        // Identification, used for display and saved/loaded in settings JSON
        // Must be unique!
        string ShortName { get; }

        // Where are we?
        bool   GameInFolder(DirectoryInfo where);
        string SteamPath();
        string MacPath();

        // What do we contain?
        string   PrimaryModDirectoryRelative { get; }
        string   PrimaryModDirectory(GameInstance inst);
        string[] StockFolders { get; }
        bool     IsReservedDirectory(GameInstance inst, string path);
        bool     AllowInstallationIn(string name, out string path);
        void     RebuildSubdirectories(GameInstance inst);
        string   DefaultCommandLine { get; }
        string[] AdjustCommandLine(string[] args, GameVersion installedVersion);

        // Which versions exist and which is present?
        List<GameVersion> KnownVersions { get; }
        GameVersion       DetectVersion(DirectoryInfo where);
        string            CompatibleVersionsFile { get; }
        string[]          BuildIDFiles { get; }

        // How to get metadata
        Uri DefaultRepositoryURL { get; }
        Uri RepositoryListURL    { get; }
    }

}
