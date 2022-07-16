#addin "nuget:?package=Cake.SemVer&version=4.0.0"
#addin "nuget:?package=semver&version=2.0.6"
#addin "nuget:?package=Cake.Docker&version=0.11.0"
#tool "nuget:?package=ILRepack&version=2.0.18"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.11.1"

using System.Text.RegularExpressions;
using Semver;

var buildNetCore = "net5.0";
var buildNetFramework = "net45";

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Debug");
var buildFramework = configuration.EndsWith("NetCore") ? buildNetCore : buildNetFramework;
var solution = Argument<string>("solution", "CKAN.sln");

var rootDirectory = Context.Environment.WorkingDirectory;
var buildDirectory = rootDirectory.Combine("_build");
var outDirectory = buildDirectory.Combine("out");
var repackDirectory = buildDirectory.Combine("repack");
var ckanFile = repackDirectory.Combine(configuration).CombineWithFilePath("ckan.exe");
var netkanFile = repackDirectory.Combine(configuration).CombineWithFilePath("netkan.exe");

Task("Default")
    .Description("Build ckan.exe and netkan.exe, if not specified otherwise in Debug configuration for .NET Framework.")
    .IsDependentOn("Ckan")
    .IsDependentOn("Netkan");

Task("Debug")
    .Description("Build ckan.exe and netkan.exe in Debug configuration, if not specified otherwise for .NET Framework.")
    .IsDependentOn("Default");

Task("Release")
    .Description("Build ckan.exe and netkan.exe in Release configuration, if not specified otherwise for .NET Framework.")
    .IsDependentOn("Default");

Task("Ckan")
    .Description("Build only ckan.exe, if not specified otherwise in Debug configuration for .NET Framework.")
    .IsDependentOn("Repack-Ckan")
    .IsDependentOn("Build-DotNetCore");

Task("Netkan")
    .Description("Build only netkan.exe, if not specified otherwise in Debug configuration for .NET Framework.")
    .IsDependentOn("Repack-Netkan");

Task("DLL")
    .Description("Build only ckan.dll (CKAN-Core) for .NET Core.")
    .IsDependentOn("Build-DotNetCore");

Task("docker-inflator")
    .Description("Build the Docker image for the Inflator and push it to Dockerhub.")
    .IsDependentOn("Repack-Netkan")
    .Does(() =>
{
    var dockerDirectory   = buildDirectory.Combine("docker");
    var inflatorDirectory = dockerDirectory.Combine("inflator");
    // Versions of Docker prior to 18.03.0-ce require the Dockerfile to be within the build context
    var dockerFile        = inflatorDirectory.CombineWithFilePath("Dockerfile.netkan");
    CreateDirectory(inflatorDirectory);
    CopyFile(buildDirectory.CombineWithFilePath("netkan.exe"),
          inflatorDirectory.CombineWithFilePath("netkan.exe"));
    CopyFile(rootDirectory.CombineWithFilePath("Dockerfile.netkan"), dockerFile);

    var mainTag   = "kspckan/inflator";
    var latestTag = mainTag + ":latest";
    DockerBuild(
        new DockerImageBuildSettings()
        {
            File = dockerFile.ToString(),
            Tag  = new string[] { mainTag }
        },
        inflatorDirectory.ToString()
    );
    DockerTag(mainTag, latestTag);
    DockerPush(latestTag);

    // Restart the Inflator
    var netkanImage = "kspckan/netkan";
    DockerPull(netkanImage);
    DockerRun(new DockerContainerRunSettings()
        {
            Env = new string[]
            {
                "AWS_ACCESS_KEY_ID",
                "AWS_SECRET_ACCESS_KEY",
                "AWS_DEFAULT_REGION"
            }
        },
        netkanImage,
        "redeploy-service",
        "--cluster",      "NetKANCluster",
        "--service-name", "Inflator"
    );
});

Task("docker-metadata")
    .Description("Build the Docker image for the metadata testing and push it to Dockerhub.")
    .IsDependentOn("Repack-Netkan")
    .IsDependentOn("Repack-Ckan")
    .Does(() =>
{
    var dockerDirectory   = buildDirectory.Combine("docker");
    var metadataDirectory = dockerDirectory.Combine("metadata");
    // Versions of Docker prior to 18.03.0-ce require the Dockerfile to be within the build context
    var dockerFile        = metadataDirectory.CombineWithFilePath("Dockerfile.metadata");
    CreateDirectory(metadataDirectory);
    CopyFile(buildDirectory.CombineWithFilePath("netkan.exe"),
          metadataDirectory.CombineWithFilePath("netkan.exe"));
    CopyFile(buildDirectory.CombineWithFilePath("ckan.exe"),
          metadataDirectory.CombineWithFilePath("ckan.exe"));
    CopyFile(rootDirectory.CombineWithFilePath("Dockerfile.metadata"), dockerFile);

    var mainTag   = "kspckan/metadata";
    var latestTag = mainTag + ":latest";
    DockerBuild(
        new DockerImageBuildSettings()
        {
            File = dockerFile.ToString(),
            Tag  = new string[] { mainTag }
        },
        metadataDirectory.ToString()
    );
    DockerTag(mainTag, latestTag);
    DockerPush(latestTag);
});

Task("osx")
    .Description("Build the macOS(OSX) dmg package.")
    .IsDependentOn("Ckan")
    .Does(() => MakeIn("macosx"));

Task("osx-clean")
    .Description("Clean the output directory of the macOS(OSX) package.")
    .Does(() => MakeIn("macosx", "clean"));

Task("deb")
    .Description("Build the deb package for Debian-based distros.")
    .IsDependentOn("Ckan")
    .Does(() => MakeIn("debian"));

Task("deb-sign")
    .Description("Build the deb package for Debian-based distros.")
    .IsDependentOn("Ckan")
    .Does(() => MakeIn("debian", "sign"));

Task("deb-test")
    .Description("Test the deb packaging.")
    .IsDependentOn("deb")
    .Does(() => MakeIn("debian", "test"));

Task("deb-clean")
    .Description("Clean the deb output directory.")
    .Does(() => MakeIn("debian", "clean"));

Task("rpm")
    .Description("Build the rpm package for RPM-based distros.")
    .IsDependentOn("Ckan")
    .Does(() => MakeIn("rpm"));

Task("rpm-test")
    .Description("Test the rpm packaging.")
    .IsDependentOn("Ckan")
    .Does(() => MakeIn("rpm", "test"));

Task("rpm-clean")
    .Description("Clean the rpm package output directory.")
    .Does(() => MakeIn("rpm", "clean"));

private void MakeIn(string dir, string args = null)
{
    int exitCode = StartProcess("make", new ProcessSettings {
        WorkingDirectory = dir,
        Arguments = args,
        EnvironmentVariables = new Dictionary<string, string> { { "CONFIGURATION", configuration } }
    });
    if (exitCode != 0)
    {
        throw new Exception("Make failed with exit code: " + exitCode);
    }
}

Task("Restore-Nuget")
    .Description("Intermediate - Download dependencies with NuGet when building for .NET Framework.")
    .WithCriteria(() => buildFramework == buildNetFramework)
    .Does(() =>
{
    NuGetRestore(solution, new NuGetRestoreSettings
    {
        ConfigFile = "nuget.config",
        EnvironmentVariables = new Dictionary<string, string> { { "Configuration", configuration } }
    });
});

Task("Build-DotNet")
    .Description("Intermediate - Call MSBuild/XBuild to build the CKAN.sln.")
    .IsDependentOn("Restore-Nuget")
    .IsDependentOn("Generate-GlobalAssemblyVersionInfo")
    .WithCriteria(() => buildFramework == buildNetFramework)
    .Does(() =>
{
    MSBuild(solution, settings =>
    {
        settings.Configuration = configuration;
    });
});

Task("Restore-DotNetCore")
    .Description("Intermediate - Download dependencies with NuGet when building for .NET Core.")
    .WithCriteria(() => buildFramework == buildNetCore)
    .Does(() =>
{
    DotNetCoreRestore(solution, new DotNetCoreRestoreSettings
    {
        ConfigFile = "nuget.config",
        EnvironmentVariables = new Dictionary<string, string> { { "Configuration", configuration } }
    });
});

Task("Build-DotNetCore")
    .Description("Intermediate - Call .NET Core's MSBuild to build the ckan.dll.")
    .IsDependentOn("Restore-Dotnetcore")
    .IsDependentOn("Generate-GlobalAssemblyVersionInfo")
    .WithCriteria(() => buildFramework == buildNetCore)
    .Does(() =>
{
    DotNetCoreBuild(solution, new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        NoRestore = true
    });
});

Task("Generate-GlobalAssemblyVersionInfo")
    .Description("Intermediate - Calculate the version strings for the assembly.")
    .Does(() =>
{
    var version = GetVersion();
    var versionStr2 = string.Format("{0}.{1}", version.Major, version.Minor);
    var versionStr3 = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Patch);

    var metaDirectory = buildDirectory.Combine("meta");

    CreateDirectory(metaDirectory);

    CreateAssemblyInfo(metaDirectory.CombineWithFilePath("GlobalAssemblyVersionInfo.cs"), new AssemblyInfoSettings
    {
        Version = versionStr2,
        FileVersion = versionStr3,
        InformationalVersion = version.ToString()
    });
});

Task("Repack-Ckan")
    .Description("Intermediate - Merge all the separate DLLs and EXEs to a single executable.")
    .WithCriteria(() => buildFramework == buildNetFramework)
    .IsDependentOn("Build-DotNet")
    .Does(() =>
{
    var cmdLineBinDirectory = outDirectory.Combine("CmdLine").Combine(configuration).Combine("bin");
    var assemblyPaths = GetFiles(string.Format("{0}/*.dll", cmdLineBinDirectory));
    assemblyPaths.Add(cmdLineBinDirectory.CombineWithFilePath("CKAN-GUI.exe"));
    assemblyPaths.Add(cmdLineBinDirectory.CombineWithFilePath("CKAN-ConsoleUI.exe"));
    assemblyPaths.Add(GetFiles(string.Format(
        "{0}/*/*.resources.dll",
        outDirectory.Combine("CKAN-GUI").Combine(configuration).Combine("bin")
    )));

    ILRepack(ckanFile, cmdLineBinDirectory.CombineWithFilePath("CmdLine.exe"), assemblyPaths,
        new ILRepackSettings
        {
            Libs = new List<DirectoryPath> { cmdLineBinDirectory.ToString() },
            TargetPlatform = TargetPlatformVersion.v4
        }
    );

    CopyFile(ckanFile, buildDirectory.CombineWithFilePath("ckan.exe"));
});

Task("Repack-Netkan")
    .Description("Intermediate - Merge all the separate DLLs and EXEs to a single executable.")
    .WithCriteria(() => buildFramework == buildNetFramework)
    .IsDependentOn("Build-DotNet")
    .Does(() =>
{
    var netkanBinDirectory = outDirectory.Combine("NetKAN").Combine(configuration).Combine("bin");
    var assemblyPaths = GetFiles(string.Format("{0}/*.dll", netkanBinDirectory));

    ILRepack(netkanFile, netkanBinDirectory.CombineWithFilePath("NetKAN.exe"), assemblyPaths,
        new ILRepackSettings
        {
            Libs = new List<DirectoryPath> { netkanBinDirectory.ToString() },
        }
    );

    CopyFile(netkanFile, buildDirectory.CombineWithFilePath("netkan.exe"));
});

Task("Test")
    .Description("Run CKANs tests after compilation.")
    .IsDependentOn("Default")
    .IsDependentOn("Test+Only");

Task("Test+Only")
    .Description("Only run CKANs tests, without compiling beforehand.")
    .IsDependentOn("Test-UnitTests+Only")
    .IsDependentOn("Test-Executables+Only");

Task("Test-UnitTests+Only")
    .Description("Intermediate - Only run CKANs unit tests, without compiling beforehand.")
    .IsDependentOn("Test-UnitTests+Only-DotNetCore")
    .WithCriteria(() => buildFramework == buildNetFramework)
    .Does(() =>
{
    var where = Argument<string>("where", null);

    var testFile = outDirectory
        .Combine("CKAN.Tests")
        .Combine(configuration)
        .Combine("bin")
        .Combine(buildFramework)
        .CombineWithFilePath("CKAN.Tests.dll");

    if (!FileExists(testFile))
        throw new Exception("Test assembly not found: " + testFile);

    var nunitOutputDirectory = buildDirectory.Combine("test/nunit");

    CreateDirectory(nunitOutputDirectory);

    NUnit3(testFile.FullPath, new NUnit3Settings {
        Where = where,
        Work = nunitOutputDirectory
    });
});

Task("Test-UnitTests+Only-DotNetCore")
    .Description("Intermediate - Only run CKANs unit tests using DotNetCoreTest, without compiling beforehand.")
    .WithCriteria(() => buildFramework == buildNetCore)
    .Does(() =>
{
    var where = Argument<string>("where", null);

    var nunitOutputDirectory = buildDirectory.Combine("test/nunit");

    CreateDirectory(nunitOutputDirectory);

    DotNetCoreTest(solution, new DotNetCoreTestSettings {
        NoBuild = true,
        Configuration= configuration,
        ResultsDirectory = nunitOutputDirectory,
        Filter = where
    });
});

Task("Test-Executables+Only")
    .Description("Intermediate - Only test CKANs executables, without compiling them beforhand.")
    .IsDependentOn("Test-CkanExecutable+Only")
    .IsDependentOn("Test-NetkanExecutable+Only");

Task("Test-CkanExecutable+Only")
    .Description("Intermediate - Only test the ckan.exe, without compiling beforhand.")
    .WithCriteria(() => buildFramework == buildNetFramework)
    .Does(() =>
{
    if (RunExecutable(ckanFile, "version").FirstOrDefault() != string.Format("v{0}", GetVersion()))
        throw new Exception("ckan.exe smoke test failed.");
});

Task("Test-NetkanExecutable+Only")
    .Description("Intermediate - Only test the netkan.exe, without compiling beforhand.")
    .WithCriteria(() => buildFramework == buildNetFramework)
    .Does(() =>
{
    if (RunExecutable(netkanFile, "--version").FirstOrDefault() != string.Format("v{0}", GetVersion()))
        throw new Exception("netkan.exe smoke test failed.");
});

Task("Version")
    .Description("Print the current CKAN version.")
    .Does(() =>
{
    Information(GetVersion().ToString());
});

Setup(context =>
{
    var argConfiguration = Argument<string>("configuration", null);

    if (string.Equals(target, "Release", StringComparison.OrdinalIgnoreCase))
    {
        if (argConfiguration != null)
            Warning($"Ignoring configuration argument: '{argConfiguration}'");

        configuration = "Release";
        buildFramework = buildNetFramework;
    }
    else if (string.Equals(target, "Debug", StringComparison.OrdinalIgnoreCase))
    {
        if (argConfiguration != null)
            Warning($"Ignoring configuration argument: '{argConfiguration}'");

        configuration = "Debug";
        buildFramework = buildNetFramework;
    }
    else if (string.Equals(target, "DLL", StringComparison.OrdinalIgnoreCase))
    {
        if (argConfiguration == null || argConfiguration.StartsWith("Debug"))
            configuration = "Debug_NetCore";
        else if (argConfiguration.StartsWith("Release"))
            configuration = "Release_NetCore";

        buildFramework = buildNetCore;
    }
});

Teardown(context =>
{
    var quote = GetQuote(rootDirectory.CombineWithFilePath("quotes.txt"));

    if (quote != null)
    {
        Information(quote);
    }
});

RunTarget(target);

private SemVersion GetVersion()
{
    var pattern = new Regex(@"^\s*##\s+v(?<version>\S+)\s?.*$");
    var rootDirectory = Context.Environment.WorkingDirectory;

    var versionMatch = System.IO.File
        .ReadAllLines(rootDirectory.CombineWithFilePath("CHANGELOG.md").FullPath)
        .Select(i => pattern.Match(i))
        .FirstOrDefault(i => i.Success);

    var version = ParseSemVer(versionMatch.Groups["version"].Value);

    if (DirectoryExists(rootDirectory.Combine(".git")))
    {
        var hash = GetGitCommitHash();

        version = CreateSemVer(
            version.Major,
            version.Minor,
            version.Patch,
            version.Prerelease,
            hash == null ? null : hash.Substring(0, 12)
        );
    }

    return version;
}

private string GetGitCommitHash()
{
    IEnumerable<string> output;
    try
    {
        var exitCode = StartProcess(
            "git",
            new ProcessSettings { Arguments = "rev-parse HEAD", RedirectStandardOutput = true },
            out output
        );

        return exitCode == 0 ? output.FirstOrDefault() : null;
    }
    catch(Exception)
    {
        return null;
    }
}

private IEnumerable<string> RunExecutable(FilePath executable, string arguments)
{
    IEnumerable<string> output;
    var exitCode = StartProcess(
        executable,
        new ProcessSettings { Arguments = arguments, RedirectStandardOutput = true },
        out output
    );

    if (exitCode == 0)
        return output;
    else
        throw new Exception("Process failed with exit code: " + exitCode);
}

private string GetQuote(FilePath file)
{
    if (FileExists(file))
    {
        var quotes = System.IO.File
            .ReadAllText(file.FullPath)
            .Split(new [] { '%' }, StringSplitOptions.RemoveEmptyEntries);

        if (quotes.Length > 0)
            return quotes[(new Random()).Next(0, quotes.Length)];
    }

    return null;
}
