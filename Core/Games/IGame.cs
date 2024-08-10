using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using CKAN.DLC;
using CKAN.Versioning;

namespace CKAN.Games
{
    public interface IGame
    {
        // Identification, used for display and saved/loaded in settings JSON
        // Must be unique!
        string   ShortName        { get; }
        DateTime FirstReleaseDate { get; }

        // Where are we?
        bool          GameInFolder(DirectoryInfo where);
        DirectoryInfo MacPath();

        // What do we contain?
        string         PrimaryModDirectoryRelative     { get; }
        string[]       AlternateModDirectoriesRelative { get; }
        string         PrimaryModDirectory(GameInstance inst);
        string[]       StockFolders       { get; }
        string[]       LeaveEmptyInClones { get; }
        string[]       ReservedPaths      { get; }
        string[]       CreateableDirs     { get; }
        string[]       AutoRemovableDirs  { get; }
        bool           IsReservedDirectory(GameInstance inst, string path);
        bool           AllowInstallationIn(string name, out string path);
        void           RebuildSubdirectories(string absGameRoot);
        string[]       DefaultCommandLines(SteamLibrary steamLib, DirectoryInfo path);
        string[]       AdjustCommandLine(string[] args, GameVersion installedVersion);
        IDlcDetector[] DlcDetectors { get; }

        // Which versions exist and which is present?
        void              RefreshVersions();
        List<GameVersion> KnownVersions { get; }
        GameVersion[]     EmbeddedGameVersions { get; }
        GameVersion[]     ParseBuildsJson(JToken json);
        GameVersion       DetectVersion(DirectoryInfo where);
        string            CompatibleVersionsFile { get; }
        string[]          InstanceAnchorFiles { get; }

        // How to get metadata
        Uri DefaultRepositoryURL  { get; }
        Uri RepositoryListURL     { get; }
        Uri MetadataBugtrackerURL { get; }
        Uri ModSupportURL         { get; }
    }
}
