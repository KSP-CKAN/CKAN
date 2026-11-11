using Cake.Common;
using Cake.Core.IO;

namespace Build;

public class BuildPaths
{
    public DirectoryPath RootDirectory { get; init; }
    public FilePath CoreProject { get; }
    public FilePath NetkanProject { get; }
    public FilePath CmdlineProject { get; }
    public FilePath TestsProject { get; }
    public DirectoryPath BuildDirectory { get; }
    public DirectoryPath OutDirectory { get; }
    public FilePath NupkgFile { get; }
    public DirectoryPath RepackDirectory { get; }
    public ExecutablePaths Ckan { get; }
    public FilePath UpdaterFile { get; }
    public ExecutablePaths Netkan { get; }
    public DirectoryPath ToolsDirectory => BuildDirectory.Combine("tools");
    public FilePath AltCoverPath => ToolsDirectory.Combine("altcover.api.9.0.1")
                                                  .Combine("lib")
                                                  .Combine("net472")
                                                  .CombineWithFilePath("AltCover.exe");
    public DirectoryPath CoverageOutputDirectory => BuildDirectory.Combine("test")
                                                                  .Combine("coverage");

    public FilePath CoverageOutputFile(string filename)
        => CoverageOutputDirectory.CombineWithFilePath(filename);

    public BuildPaths(DirectoryPath rootDirectory, string configuration, SemVersion version,
                      bool isWindows)
    {
        RootDirectory = rootDirectory;
        CoreProject = rootDirectory.Combine("Core")
                                   .CombineWithFilePath("CKAN-core.csproj");
        NetkanProject = rootDirectory.Combine("Netkan")
                                     .CombineWithFilePath("CKAN-netkan.csproj");
        CmdlineProject = rootDirectory.Combine("Cmdline")
                                      .CombineWithFilePath("CKAN-cmdline.csproj");
        TestsProject = rootDirectory.Combine("Tests")
                                    .CombineWithFilePath("Tests.csproj");
        BuildDirectory = rootDirectory.Combine("_build");
        OutDirectory = BuildDirectory.Combine("out");
        NupkgFile = OutDirectory
            .Combine("CKAN")
            .Combine(configuration)
            .Combine("bin")
            .CombineWithFilePath($"CKAN.{version}.nupkg");
        RepackDirectory = BuildDirectory.Combine("repack");
        Ckan = new ExecutablePaths(RepackDirectory.Combine(configuration), "ckan", isWindows);
        UpdaterFile = RepackDirectory
            .Combine(configuration)
            .CombineWithFilePath("AutoUpdater.exe");
        Netkan = new ExecutablePaths(RepackDirectory.Combine(configuration), "netkan", isWindows);
    }
}
