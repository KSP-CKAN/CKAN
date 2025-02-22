using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Git;

namespace Build;

public partial class BuildContext : FrostingContext
{
    public string BuildNetFramework { get; } = "net48";

    public string Target { get; }

    // Named to avoid conflict with ICakeContext.Configuration
    public string BuildConfiguration { get; set; }
    public string Solution { get; }

    public BuildPaths Paths { get; }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        var rootDir = context.Environment.WorkingDirectory.GetParent();
        
        Target = context.Argument("target", "Default");
        BuildConfiguration = context.Argument("configuration", "Debug");
        Solution = context.Argument("solution", rootDir.CombineWithFilePath("CKAN.sln").FullPath);

        Paths = new BuildPaths(rootDir, BuildConfiguration, GetVersion(false));
    }

    public SemVersion GetVersion(bool withBuild = true)
    {
        var rootDirectory = Environment.WorkingDirectory.GetParent();

        var versionMatch = System.IO.File
            .ReadAllLines(rootDirectory.CombineWithFilePath("CHANGELOG.md").FullPath)
            .Select(i => VersionRegex().Match(i))
            .FirstOrDefault(i => i.Success);

        if (!SemVersion.TryParse(versionMatch.Groups["version"].Value, out var version))
        {
            throw new System.Exception("Could not parse version from CHANGELOG.md");
        }

        if (withBuild && this.DirectoryExists(rootDirectory.Combine(".git")))
        {
            var commitDate = this.GitLogTip(rootDirectory).Committer.When;
            version = new SemVersion(version.Major,
                version.Minor,
                version.Patch,
                version.PreRelease,
                commitDate.ToString("yy") + commitDate.DayOfYear.ToString("000"));
        }

        return version;
    }

    [GeneratedRegex(@"^\s*##\s+v(?<version>\S+)\s?.*$")]
    private static partial Regex VersionRegex();

    public DirectoryPath FacadesDirectory()
    {
        if (this.IsRunningOnWindows())
        {
            return Environment.GetSpecialPath(SpecialPath.ProgramFilesX86)
                .Combine("Reference Assemblies")
                .Combine("Microsoft")
                .Combine("Framework")
                .Combine(".NETFramework")
                .Combine("v4.8")
                .Combine("Facades");
        }

        DirectoryPath monoLib;
        if (this.IsRunningOnMacOs())
        {
            monoLib = new DirectoryPath("/Library")
                .Combine("Frameworks")
                .Combine("Mono.framework")
                .Combine("Versions")
                .Combine("Current")
                .Combine("lib");
        }
        else
        {
            monoLib = new DirectoryPath("/usr").Combine("lib");
        }
        
        return monoLib
            .Combine("mono")
            .Combine("4.8-api")
            .Combine("Facades");
    }

    public void ReportRepacking(FilePath target, FilePath log)
    {
        // ILRepack is extremely noisy by default and has no options to
        // make it quieter other than shutting it up completely.
        //
        using (this.NormalVerbosity())
        {
            this.Information("Repacking {0}, logging details to {1}...",
                Paths.RootDirectory.GetRelativePath(target),
                Paths.RootDirectory.GetRelativePath(log));
        }
    }

    public static void RepackSilently(ProcessSettings settings)
        => settings.SetRedirectStandardOutput(true)
            .SetRedirectedStandardOutputHandler(s => "")
            .SetRedirectStandardError(true)
            .SetRedirectedStandardErrorHandler(s => "");

    public string GetQuote(FilePath file)
    {
        if (!this.FileExists(file)) return null;

        var quotes = System.IO.File
            .ReadAllText(file.FullPath)
            .Split("%", StringSplitOptions.RemoveEmptyEntries);

        return quotes.Length > 0 ? quotes[new Random().Next(quotes.Length)] : null;
    }
    
    public IEnumerable<string> RunExecutable(FilePath executable, string arguments)
    {
        IEnumerable<string> output;
        var exitCode = this.StartProcess(
            executable,
            new ProcessSettings { Arguments = arguments, RedirectStandardOutput = true },
            out output
        );

        if (exitCode != 0)
        {
            throw new Exception("Process failed with exit code: " + exitCode);
        }
        
        return output;
    }
}