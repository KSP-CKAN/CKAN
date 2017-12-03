#addin "nuget:?package=Cake.SemVer&version=2.0.0"
#addin "nuget:?package=semver&version=2.0.4"
#tool "nuget:?package=ILRepack&version=2.0.13"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.7.0"

using System.Text.RegularExpressions;
using Semver;

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Debug");
var solution = Argument<string>("solution", "CKAN.sln");

var rootDirectory = Context.Environment.WorkingDirectory;
var buildDirectory = rootDirectory.Combine("_build");
var outDirectory = buildDirectory.Combine("out");
var repackDirectory = buildDirectory.Combine("repack");
var ckanFile = repackDirectory.Combine(configuration).CombineWithFilePath("ckan.exe");
var netkanFile = repackDirectory.Combine(configuration).CombineWithFilePath("netkan.exe");
var autoUpdateFile = repackDirectory.Combine(configuration).CombineWithFilePath("AutoUpdater.exe");

Task("Default")
    .IsDependentOn("Ckan")
    .IsDependentOn("Netkan")
    .IsDependentOn("AutoUpdater");


Task("Debug")
    .IsDependentOn("Default");

Task("Release")
    .IsDependentOn("Default");

Task("Ckan")
    .IsDependentOn("Repack-Ckan");

Task("Netkan")
    .IsDependentOn("Repack-Netkan");

Task("AutoUpdater")
    .IsDependentOn("Repack-AutoUpdater");

Task("Restore-Nuget")
    .Does(() =>
{
    NuGetRestore(solution, new NuGetRestoreSettings
    {
        ConfigFile = "nuget.config"
    });
});

Task("Build-DotNet")
    .IsDependentOn("Restore-Nuget")
    .IsDependentOn("Generate-GlobalAssemblyVersionInfo")
    .Does(() =>
{
    DotNetBuild(solution, settings =>
    {
        settings.Configuration = configuration;
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
    .IsDependentOn("Build-DotNet")
    .Does(() =>
{
    var cmdLineBinDirectory = outDirectory.Combine("CmdLine").Combine(configuration).Combine("bin");
    var assemblyPaths = GetFiles(string.Format("{0}/*.dll", cmdLineBinDirectory));
    assemblyPaths.Add(cmdLineBinDirectory.CombineWithFilePath("CKAN-GUI.exe"));

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

Task("Repack-AutoUpdater")
    .IsDependentOn("Build-DotNet")
    .Does(() =>
{
    CopyFile(outDirectory.Combine("AutoUpdater").Combine(configuration).Combine("bin").CombineWithFilePath("AutoUpdater.exe"), autoUpdateFile);
});

Task("Test")
    .IsDependentOn("Default")
    .IsDependentOn("Test+Only");

Task("Test+Only")
    .IsDependentOn("Test-UnitTests+Only")
    .IsDependentOn("Test-Executables+Only");

Task("Test-UnitTests+Only")
    .Does(() =>
{
    var where = Argument<string>("where", null);

    var testFile = outDirectory
        .Combine("CKAN.Tests")
        .Combine(configuration)
        .Combine("bin")
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

Task("Test-Executables+Only")
    .IsDependentOn("Test-CkanExecutable+Only")
    .IsDependentOn("Test-NetkanExecutable+Only");

Task("Test-CkanExecutable+Only")
    .Does(() =>
{
    if (RunExecutable(ckanFile, "version").FirstOrDefault() != string.Format("v{0}", GetVersion()))
        throw new Exception("ckan.exe smoke test failed.");
});

Task("Test-NetkanExecutable+Only")
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
        Information(quote);
    }
});

RunTarget(target);

private SemVersion GetVersion()
{
    var pattern = new Regex(@"^\s*##\s+v(?<version>\S+)\s*$");
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
