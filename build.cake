#addin "nuget:?package=Cake.SemVer&version=4.0.0"
#addin "nuget:?package=semver&version=2.3.0"
#addin nuget:?package=Cake.Git&version=3.0.0
#tool "nuget:?package=ILRepack&version=2.0.27"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.16.3"

using System.Text.RegularExpressions;
using Semver;

var buildNetFramework = "net48";

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Debug");
var solution = Argument<string>("solution", "CKAN.sln");

var rootDirectory = Context.Environment.WorkingDirectory;
var coreProj = rootDirectory.Combine("Core")
                            .CombineWithFilePath("CKAN-core.csproj")
                            .FullPath;
var buildDirectory = rootDirectory.Combine("_build");
var nugetDirectory = buildDirectory.Combine("lib")
                                   .Combine("nuget");
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
        DotNetRestore(solution, new DotNetRestoreSettings
        {
            PackagesDirectory    = nugetDirectory,
            EnvironmentVariables = new Dictionary<string, string> { { "Configuration", configuration } },
        });
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
            // Use dotnet to build the Core DLL to get the nupkg
            // (only created if all TargetFrameworks are built together)
            DotNetBuild(coreProj, new DotNetBuildSettings
            {
                Configuration = configuration,
            });
            // Use Mono to build for net48 since dotnet can't use WinForms on Linux
            MSBuild(solution,
                    settings => settings.SetConfiguration(configuration)
                                        .SetMaxCpuCount(0)
                                        .WithProperty("TargetFramework",
                                                      buildNetFramework));
            // Use dotnet to build the stuff Mono can't build
            DotNetBuild(solution, new DotNetBuildSettings
            {
                Configuration = "NoGUI",
                Framework     = "net7.0",
            });
        }
    });

Task("Generate-GlobalAssemblyVersionInfo")
    .Description("Intermediate - Calculate the version strings for the assembly.")
    .Does(() =>
{
    var metaDirectory = buildDirectory.Combine("meta");
    CreateDirectory(metaDirectory);

    var version = GetVersion();

    CreateAssemblyInfo(metaDirectory.CombineWithFilePath("GlobalAssemblyVersionInfo.cs"), new AssemblyInfoSettings
    {
        Version              = string.Format("{0}.{1}", version.Major, version.Minor),
        FileVersion          = string.IsNullOrEmpty(version.Metadata)
                                ? string.Format("{0}.{1}.{2}",
                                                version.Major, version.Minor, version.Patch)
                                : string.Format("{0}.{1}.{2}.{3}",
                                                version.Major, version.Minor, version.Patch,
                                                version.Metadata),
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
    // Need facade to instantiate types from netstandard2.0 DLLs on Mono
    assemblyPaths.Add(FacadesDirectory().CombineWithFilePath("netstandard.dll"));
    var ckanLogFile = repackDirectory.Combine(configuration)
                                     .CombineWithFilePath($"ckan.log");
    ReportRepacking(ckanFile, ckanLogFile);
    ILRepack(
        ckanFile,
        cmdLineBinDirectory.CombineWithFilePath("CKAN-CmdLine.exe"),
        assemblyPaths,
        new ILRepackSettings
        {
            Libs           = new List<DirectoryPath> { cmdLineBinDirectory },
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
            Libs           = new List<DirectoryPath> { autoupdateBinDirectory },
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
    var assemblyPaths = GetFiles(string.Format("{0}/*.dll", netkanBinDirectory));
    // Need facade to instantiate types from netstandard2.0 DLLs on Mono
    assemblyPaths.Add(FacadesDirectory().CombineWithFilePath("netstandard.dll"));
    ReportRepacking(netkanFile, netkanLogFile);
    ILRepack(
        netkanFile,
        netkanBinDirectory.CombineWithFilePath("CKAN-NetKAN.exe"),
        assemblyPaths,
        new ILRepackSettings
        {
            Libs           = new List<DirectoryPath> { netkanBinDirectory },
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
            NoLogo           = true,
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
            Framework        = "net7.0",
            NoBuild          = true,
            NoLogo           = true,
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
            NoHeader      = true,
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

private DirectoryPath FacadesDirectory()
    => IsRunningOnWindows()
        ? Context.Environment.GetSpecialPath(SpecialPath.ProgramFilesX86)
                             .Combine("Reference Assemblies")
                             .Combine("Microsoft")
                             .Combine("Framework")
                             .Combine(".NETFramework")
                             .Combine("v4.8")
                             .Combine("Facades")
        : new DirectoryPath("/usr").Combine("lib")
                                   .Combine("mono")
                                   .Combine("4.8-api")
                                   .Combine("Facades");

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
        var commitDate = GitLogTip(rootDirectory).Committer.When;
        version = CreateSemVer(version.Major,
                               version.Minor,
                               version.Patch,
                               version.Prerelease,
                               commitDate.ToString("yy") + commitDate.DayOfYear.ToString("000"));
    }

    return version;
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
