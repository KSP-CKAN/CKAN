using Cake.Common;
using Cake.Core.IO;

namespace Build;

public class BuildPaths
{
    public DirectoryPath RootDirectory { get; init; }
    public FilePath CoreProject { get; } 
    public DirectoryPath BuildDirectory { get; }
    public DirectoryPath NugetDirectory { get; }
    public DirectoryPath OutDirectory { get; }
    public FilePath NupkgFile { get; }
    public DirectoryPath RepackDirectory { get; }
    public FilePath CkanFile { get; }
    public FilePath UpdaterFile { get; }
    public FilePath NetkanFile { get; }

    public BuildPaths(DirectoryPath rootDirectory, string configuration, SemVersion version)
    {
        RootDirectory = rootDirectory;
        CoreProject = rootDirectory.Combine("Core")
            .CombineWithFilePath("CKAN-core.csproj");
        BuildDirectory = rootDirectory.Combine("_build");
        NugetDirectory = BuildDirectory.Combine("lib").Combine("nuget");
        OutDirectory = BuildDirectory.Combine("out");
        NupkgFile = OutDirectory
            .Combine("CKAN")
            .Combine(configuration)
            .Combine("bin")
            .CombineWithFilePath($"CKAN.{version}.nupkg");
        RepackDirectory = BuildDirectory.Combine("repack");
        CkanFile = RepackDirectory
            .Combine(configuration)
            .CombineWithFilePath("ckan.exe");
        UpdaterFile = RepackDirectory
            .Combine(configuration)
            .CombineWithFilePath("AutoUpdater.exe");
        NetkanFile = RepackDirectory
            .Combine(configuration)
            .CombineWithFilePath("netkan.exe");
    }
}