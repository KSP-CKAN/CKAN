#addin "nuget:?package=Cake.SemVer&version=3.0.0"
#addin "nuget:?package=semver&version=2.0.4"
#addin "nuget:?package=Cake.Docker&version=0.10.0"
#tool "nuget:?package=ILRepack&version=2.0.17"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.10.0"

using System.Text.RegularExpressions;
using Semver;

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Debug");
var buildFramework = configuration.EndsWith("NetCore") ? "netcoreapp2.1" : "net45";
var solution = Argument<string>("solution", "CKAN.sln");

var rootDirectory = Context.Environment.WorkingDirectory;
var buildDirectory = rootDirectory.Combine("_build");
var outDirectory = buildDirectory.Combine("out");
var repackDirectory = buildDirectory.Combine("repack");
var ckanFile = repackDirectory.Combine(configuration).CombineWithFilePath("ckan.exe");
var netkanFile = repackDirectory.Combine(configuration).CombineWithFilePath("netkan.exe");

Task("Default")
    .IsDependentOn("Ckan")
    .IsDependentOn("Netkan");

Task("Debug")
    .IsDependentOn("Default");

Task("Release")
    .IsDependentOn("Default");

Task("Ckan")
    .IsDependentOn("Repack-Ckan")
    .IsDependentOn("Build-DotNetCore");

Task("Netkan")
    .IsDependentOn("Repack-Netkan");

Task("docker-inflator")
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

Task("osx")
    .IsDependentOn("Ckan")
    .Does(() => StartProcess("make",
        new ProcessSettings { WorkingDirectory = "macosx" }));

Task("osx-clean")
    .Does(() => StartProcess("make",
        new ProcessSettings { Arguments = "clean", WorkingDirectory = "macosx" }));

Task("deb")
    .IsDependentOn("Ckan")
    .Does(() => StartProcess("make",
        new ProcessSettings { WorkingDirectory = "debian" }));

Task("deb-test")
    .IsDependentOn("deb")
    .Does(() => StartProcess("make",
        new ProcessSettings { Arguments = "test", WorkingDirectory = "debian" }));

Task("deb-clean")
    .Does(() => StartProcess("make",
        new ProcessSettings { Arguments = "clean", WorkingDirectory = "debian" }));

Task("rpm")
    .IsDependentOn("Ckan")
    .Does(() => StartProcess("make",
        new ProcessSettings { WorkingDirectory = "rpm" }));

Task("rpm-test")
    .IsDependentOn("Ckan")
    .Does(() => StartProcess("make",
        new ProcessSettings { Arguments = "test", WorkingDirectory = "rpm" }));

Task("rpm-clean")
    .Does(() => StartProcess("make",
        new ProcessSettings { Arguments = "clean", WorkingDirectory = "rpm" }));

Task("Restore-Nuget")
    .WithCriteria(buildFramework == "net45")
    .Does(() =>
{
    NuGetRestore(solution, new NuGetRestoreSettings
    {
        ConfigFile = "nuget.config",
        EnvironmentVariables = new Dictionary<string, string> { { "Configuration", configuration } }
    });
});

Task("Build-DotNet")
    .IsDependentOn("Restore-Nuget")
    .IsDependentOn("Generate-GlobalAssemblyVersionInfo")
    .WithCriteria(buildFramework == "net45")
    .Does(() =>
{
    MSBuild(solution, settings =>
    {
        settings.Configuration = configuration;
    });
});

Task("Restore-DotNetCore")
    .WithCriteria(buildFramework == "netcoreapp2.1")
    .Does(() =>
{
    DotNetCoreRestore(solution, new DotNetCoreRestoreSettings
    {
        ConfigFile = "nuget.config",
        EnvironmentVariables = new Dictionary<string, string> { { "Configuration", configuration } }
    });
});

Task("Build-DotNetCore")
    .IsDependentOn("Restore-Dotnetcore")
    .IsDependentOn("Generate-GlobalAssemblyVersionInfo")
    .WithCriteria(buildFramework == "netcoreapp2.1")
    .Does(() =>
{
    DotNetCoreBuild(solution, new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        NoRestore = true
    });
});

Task("Generate-GlobalAssemblyVersionInfo")
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
    .WithCriteria(buildFramework == "net45")
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
    .WithCriteria(buildFramework == "net45")
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
    .IsDependentOn("Default")
    .IsDependentOn("Test+Only");

Task("Test+Only")
    .IsDependentOn("Test-UnitTests+Only")
    .IsDependentOn("Test-Executables+Only");

Task("Test-UnitTests+Only")
    .IsDependentOn("Test-UnitTests+Only-DotNetCore")
    .WithCriteria(buildFramework == "net45")
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
    .WithCriteria(buildFramework == "netcoreapp2.1")
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
    .IsDependentOn("Test-CkanExecutable+Only")
    .IsDependentOn("Test-NetkanExecutable+Only");

Task("Test-CkanExecutable+Only")
    .WithCriteria(buildFramework == "net45")
    .Does(() =>
{
    if (RunExecutable(ckanFile, "version").FirstOrDefault() != string.Format("v{0}", GetVersion()))
        throw new Exception("ckan.exe smoke test failed.");
});

Task("Test-NetkanExecutable+Only")
    .WithCriteria(buildFramework == "net45")
    .Does(() =>
{
    if (RunExecutable(netkanFile, "--version").FirstOrDefault() != string.Format("v{0}", GetVersion()))
        throw new Exception("netkan.exe smoke test failed.");
});

Task("Version")
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
        buildFramework = "net45";
    }
    else if (string.Equals(target, "Debug", StringComparison.OrdinalIgnoreCase))
    {
        if (argConfiguration != null)
            Warning($"Ignoring configuration argument: '{argConfiguration}'");

        configuration = "Debug";
        buildFramework = "net45";
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
