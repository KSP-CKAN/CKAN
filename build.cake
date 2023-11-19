#addin "nuget:?package=Cake.SemVer&version=4.0.0"
#addin "nuget:?package=semver&version=2.3.0"
#addin "nuget:?package=Cake.Docker&version=0.11.1"
#tool "nuget:?package=ILRepack&version=2.0.18"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.16.3"

using System.Text.RegularExpressions;
using Semver;

var buildNetFramework = "net48";

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Debug");
var solution = Argument<string>("solution", "CKAN.sln");

var rootDirectory = Context.Environment.WorkingDirectory;
var buildDirectory = rootDirectory.Combine("_build");
var outDirectory = buildDirectory.Combine("out");
var repackDirectory = buildDirectory.Combine("repack");
var ckanFile = repackDirectory.Combine(configuration)
                              .CombineWithFilePath("ckan.exe");
var netkanFile = repackDirectory.Combine(configuration)
                                .CombineWithFilePath("netkan.exe");

Task("Default")
    .Description("Build ckan.exe and netkan.exe")
    .IsDependentOn("Ckan")
    .IsDependentOn("Netkan");

Task("Debug")
    .Description("Build ckan.exe and netkan.exe in Debug configuration")
    .IsDependentOn("Default");

Task("Release")
    .Description("Build ckan.exe and netkan.exe in Release configuration")
    .IsDependentOn("Default");

Task("Ckan")
    .Description("Build only ckan.exe")
    .IsDependentOn("Repack-Ckan");

Task("Netkan")
    .Description("Build only netkan.exe")
    .IsDependentOn("Repack-Netkan");

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
            File = dockerFile.FullPath,
            Tag  = new string[] { mainTag }
        },
        inflatorDirectory.FullPath
    );
    DockerTag(mainTag, latestTag);
    DockerPush(latestTag);

    // Restart the Inflator
    var netkanImage = "kspckan/netkan";
    DockerPull(netkanImage);
    var runSettings = new DockerContainerRunSettings()
    {
        Env = new string[]
        {
            "AWS_ACCESS_KEY_ID",
            "AWS_SECRET_ACCESS_KEY",
            "AWS_DEFAULT_REGION"
        }
    };
    DockerRun(runSettings, netkanImage,
        "redeploy-service",
        "--cluster",      "NetKANCluster",
        "--service-name", "InflatorKsp");
    DockerRun(runSettings, netkanImage,
        "redeploy-service",
        "--cluster",      "NetKANCluster",
        "--service-name", "InflatorKsp2");
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
            File = dockerFile.FullPath,
            Tag  = new string[] { mainTag }
        },
        metadataDirectory.FullPath
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

Task("rpm-repo")
    .Description("Build the rpm repository for RPM-based distros.")
    .IsDependentOn("Ckan")
    .Does(() => MakeIn("rpm", "repo"));

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

Task("Restore")
    .Description("Intermediate - Download dependencies")
    .Does(() =>
    {
        var nugetDirectory = buildDirectory.Combine("lib")
                                           .Combine("nuget");
        if (IsRunningOnWindows())
        {
            DotNetRestore(solution, new DotNetRestoreSettings
            {
                PackagesDirectory    = nugetDirectory,
                EnvironmentVariables = new Dictionary<string, string> { { "Configuration", configuration } },
            });
        }
        else
        {
            // NuGet is confused by multi-targeting, and
            // Mono has no idea what "net7.0" is. Only restore for buildNetFramework.
            DotNetRestore(solution, new DotNetRestoreSettings
            {
                PackagesDirectory    = nugetDirectory,
                EnvironmentVariables = new Dictionary<string, string> { { "Configuration", configuration } },
                MSBuildSettings      = new DotNetMSBuildSettings()
                                           .SetConfiguration(configuration)
                                           .WithProperty("TargetFramework",
                                                         buildNetFramework),
            });
            DotNetRestore(solution, new DotNetRestoreSettings
            {
                PackagesDirectory    = nugetDirectory,
                EnvironmentVariables = new Dictionary<string, string> { { "Configuration", "NoGUI" } },
                MSBuildSettings      = new DotNetMSBuildSettings()
                                           .SetConfiguration("NoGUI")
                                           .WithProperty("TargetFramework",
                                                         "net7.0"),
            });
        }
    });

Task("Build")
    .Description("Intermediate - Build everything")
    .IsDependentOn("Restore")
    .IsDependentOn("Generate-GlobalAssemblyVersionInfo")
    .Does(() =>
    {
        // dotnet build won't let us compile WinForms on non-Windows,
        // fall back to mono
        if (IsRunningOnWindows())
        {
            DotNetBuild(solution, new DotNetBuildSettings
            {
                Configuration = configuration,
                NoRestore     = true,
            });
        }
        else
        {
            // Mono has no idea what "net7.0" is
            MSBuild(solution,
                    settings => settings.SetConfiguration(configuration)
                                        .WithProperty("TargetFramework",
                                                      buildNetFramework));
            DotNetBuild(solution, new DotNetBuildSettings
            {
                Configuration = "NoGUI",
                NoRestore     = true,
                Framework     = "net7.0",
            });
        }
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
    .IsDependentOn("Build")
    .Does(() =>
{
    CreateDirectory(repackDirectory.Combine(configuration));
    var cmdLineBinDirectory = outDirectory.Combine("CKAN-CmdLine")
                                          .Combine(configuration)
                                          .Combine("bin")
                                          .Combine(buildNetFramework);
    var assemblyPaths = GetFiles(string.Format("{0}/*.dll", cmdLineBinDirectory));
    assemblyPaths.Add(cmdLineBinDirectory.CombineWithFilePath("CKAN-GUI.exe"));
    assemblyPaths.Add(cmdLineBinDirectory.CombineWithFilePath("CKAN-ConsoleUI.exe"));
    assemblyPaths.Add(GetFiles(string.Format(
        "{0}/*/*.resources.dll",
        outDirectory.Combine("CKAN-CmdLine")
                    .Combine(configuration)
                    .Combine("bin")
                    .Combine(buildNetFramework))));
    var ckanLogFile = repackDirectory.Combine(configuration)
                                     .CombineWithFilePath($"ckan.log");
    ReportRepacking(ckanFile, ckanLogFile);
    ILRepack(
        ckanFile,
        cmdLineBinDirectory.CombineWithFilePath("CKAN-CmdLine.exe"),
        assemblyPaths,
        new ILRepackSettings
        {
            Libs           = new List<DirectoryPath> { cmdLineBinDirectory.FullPath },
            TargetPlatform = TargetPlatformVersion.v4,
            Parallel       = true,
            Verbose        = false,
            SetupProcessSettings = RepackSilently,
            Log            = ckanLogFile.FullPath,
        });

    var autoupdateBinDirectory = outDirectory.Combine("CKAN-AutoUpdateHelper")
                                             .Combine(configuration)
                                             .Combine("bin")
                                             .Combine(buildNetFramework);
    var updaterFile = repackDirectory.Combine(configuration)
                                     .CombineWithFilePath("AutoUpdater.exe");
    var updaterLogFile = repackDirectory.Combine(configuration)
                                        .CombineWithFilePath($"AutoUpdater.log");
    ReportRepacking(updaterFile, updaterLogFile);
    ILRepack(
        updaterFile,
        autoupdateBinDirectory.CombineWithFilePath("CKAN-AutoUpdateHelper.exe"),
        GetFiles(string.Format("{0}/*/*.resources.dll",
                               autoupdateBinDirectory)),
        new ILRepackSettings
        {
            Libs           = new List<DirectoryPath> { autoupdateBinDirectory.FullPath },
            TargetPlatform = TargetPlatformVersion.v4,
            Parallel       = true,
            Verbose        = false,
            SetupProcessSettings = RepackSilently,
            Log            = updaterLogFile.FullPath,
        });

    CopyFile(ckanFile, buildDirectory.CombineWithFilePath("ckan.exe"));
});

Task("Repack-Netkan")
    .Description("Intermediate - Merge all the separate DLLs and EXEs to a single executable.")
    .IsDependentOn("Build")
    .Does(() =>
{
    CreateDirectory(repackDirectory.Combine(configuration));
    var netkanBinDirectory = outDirectory.Combine("CKAN-NetKAN")
                                         .Combine(configuration)
                                         .Combine("bin")
                                         .Combine(buildNetFramework);
    var netkanLogFile = repackDirectory.Combine(configuration)
                                       .CombineWithFilePath($"netkan.log");
    ReportRepacking(netkanFile, netkanLogFile);
    ILRepack(
        netkanFile,
        netkanBinDirectory.CombineWithFilePath("CKAN-NetKAN.exe"),
        GetFiles(string.Format("{0}/*.dll",
                 netkanBinDirectory)),
        new ILRepackSettings
        {
            Libs           = new List<DirectoryPath> { netkanBinDirectory.FullPath },
            TargetPlatform = TargetPlatformVersion.v4,
            Parallel       = true,
            Verbose        = false,
            SetupProcessSettings = RepackSilently,
            Log            = netkanLogFile.FullPath,
        }
    );

    CopyFile(netkanFile, buildDirectory.CombineWithFilePath("netkan.exe"));
});

private void ReportRepacking(FilePath target, FilePath log)
{
    // ILRepack is extremly noisy by default and has no options to
    // make it quieter other than shutting it up completely.
    //
    using (NormalVerbosity())
    {
        Information("Repacking {0}, logging details to {1}...",
                    rootDirectory.GetRelativePath(target),
                    rootDirectory.GetRelativePath(log));
    }
}

private void RepackSilently(ProcessSettings settings)
    => settings.SetRedirectStandardOutput(true)
               .SetRedirectedStandardOutputHandler(s => "")
               .SetRedirectStandardError(true)
               .SetRedirectedStandardErrorHandler(s => "");

Task("Test")
    .Description("Run tests after compilation.")
    .IsDependentOn("Default")
    .IsDependentOn("Test+Only");

Task("Test+Only")
    .Description("Run tests without compiling.")
    .IsDependentOn("Test-Executables+Only")
    .IsDependentOn("Test-UnitTests+Only");

Task("Test-Executables+Only")
    .Description("Intermediate - Test executables without compiling.")
    .IsDependentOn("Test-CkanExecutable+Only")
    .IsDependentOn("Test-NetkanExecutable+Only");

Task("Test-CkanExecutable+Only")
    .Description("Intermediate - Test ckan.exe without compiling.")
    .Does(() =>
{
    var output = RunExecutable(ckanFile, "version").FirstOrDefault();
    if (output != string.Format("v{0}", GetVersion()))
    {
        throw new Exception($"ckan.exe smoke test failed: {output}");
    }
});

Task("Test-NetkanExecutable+Only")
    .Description("Intermediate - Test netkan.exe without compiling.")
    .Does(() =>
{
    var output = RunExecutable(netkanFile, "--version").FirstOrDefault();
    if (output != string.Format("v{0}", GetVersion()))
    {
        throw new Exception($"netkan.exe smoke test failed: {output}");
    }
});

Task("Test-UnitTests+Only")
    .Description("Intermediate - Run unit tests without compiling.")
    .Does(() =>
{
    var where = Argument<string>("where", null);
    var nunitOutputDirectory = buildDirectory.Combine("test")
                                             .Combine("nunit");
    CreateDirectory(nunitOutputDirectory);
    // Only Mono's msbuild can handle WinForms on Linux,
    // but dotnet build can handle multi-targeting on Windows
    if (IsRunningOnWindows())
    {
        DotNetTest(solution, new DotNetTestSettings
        {
            Configuration    = configuration,
            NoBuild          = true,
            Filter           = where,
            ResultsDirectory = nunitOutputDirectory,
            Verbosity        = DotNetVerbosity.Minimal,
        });
    }
    else
    {
        DotNetTest(solution, new DotNetTestSettings
        {
            Configuration    = "NoGUI",
            NoBuild          = true,
            Filter           = where,
            ResultsDirectory = nunitOutputDirectory,
            Verbosity        = DotNetVerbosity.Minimal,
        });
        var testFile = outDirectory.Combine("CKAN.Tests")
                                   .Combine(configuration)
                                   .Combine("bin")
                                   .Combine(buildNetFramework)
                                   .CombineWithFilePath("CKAN.Tests.dll");
        NUnit3(testFile.FullPath, new NUnit3Settings
        {
            Configuration = configuration,
            Where         = where,
            Work          = nunitOutputDirectory,
            // Work around System.Runtime.Remoting.RemotingException : Tcp transport error.
            Process       = NUnit3ProcessOption.InProcess,
        });
    }
});

Task("Version")
    .Description("Print the current CKAN version.")
    .Does(() =>
{
    using (NormalVerbosity())
    {
        Information(GetVersion().ToString());
    }
});

Setup(context =>
{
    var argConfiguration = Argument<string>("configuration", null);

    if (string.Equals(target, "Release", StringComparison.OrdinalIgnoreCase))
    {
        if (argConfiguration != null)
            Warning($"Ignoring configuration argument: '{argConfiguration}'");

        configuration = "Release";
    }
    else if (string.Equals(target, "Debug", StringComparison.OrdinalIgnoreCase))
    {
        if (argConfiguration != null)
            Warning($"Ignoring configuration argument: '{argConfiguration}'");

        configuration = "Debug";
    }
});

Teardown(context =>
{
    var quote = GetQuote(rootDirectory.CombineWithFilePath("quotes.txt"));

    if (quote != null)
    {
        using (NormalVerbosity())
        {
            Information(quote);
        }
    }
});

RunTarget(target);

private Semver.SemVersion GetVersion()
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
    {
        return output;
    }
    else
    {
        throw new Exception("Process failed with exit code: " + exitCode);
    }
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
