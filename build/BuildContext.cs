using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Common.Tools.ILMerge;
using Cake.Common.Tools.ILRepack;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Git;

namespace Build;

public partial class BuildContext : FrostingContext
{
    public string BuildNetFramework => "net481";
    public string BuildDotNet       => "net10.0";

    public string Target { get; }

    // Named to avoid conflict with ICakeContext.Configuration
    public string? BuildConfiguration { get; set; }

    public BuildPaths Paths { get; }

    /// <summary>
    /// The target framework used for DotNet builds.
    /// </summary>
    public string NativeCkanFramework
        => this.IsRunningOnWindows() ? $"{BuildDotNet}-windows" : BuildDotNet;

    /// <summary>
    /// Which target frameworks to build executables and run tests for
    /// </summary>
    public BuildFrameworks FrameworkKinds { get; }

    public bool BuildsNetFramework => FrameworkKinds.IncludesNetFramework();
    public bool BuildsDotNet => FrameworkKinds.IncludesDotNet();

    /// <summary>
    /// The target frameworks of the Tests project to build and test for the selected frameworks.
    /// net10.0-windows can only be built on Windows.
    /// </summary>
    public IEnumerable<string> Frameworks
    {
        get
        {
            if (BuildsNetFramework)
            {
                yield return BuildNetFramework;
            }

            if (BuildsDotNet)
            {
                yield return BuildDotNet;
                if (this.IsRunningOnWindows())
                {
                    // This target lets us use WinForms under dotnet (otherwise it would be .NET Framework only).
                    yield return $"{BuildDotNet}-windows";
                }
            }
        }
    }

    /// <summary>
    /// DotNet runtime identifier of the host, for native self-contained builds
    /// </summary>
    public string HostRuntime
    {
        get
        {
            var os = this.IsRunningOnWindows() ? "win" : this.IsRunningOnMacOs() ? "osx" : "linux";
            var arch = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
            return $"{os}-{arch}";
        }
    }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        var rootDir = context.Environment.WorkingDirectory.GetParent();

        Target = context.Argument("target", "Default");
        BuildConfiguration = context.Argument<string?>("configuration", null);

        if (string.Equals(Target, "Release", StringComparison.OrdinalIgnoreCase))
        {
            if (BuildConfiguration != null)
            {
                context.Warning($"Ignoring configuration argument: '{BuildConfiguration}'");
            }

            BuildConfiguration = "Release";
        }
        else if (string.Equals(Target, "Debug", StringComparison.OrdinalIgnoreCase))
        {
            if (BuildConfiguration != null)
            {
                context.Warning($"Ignoring configuration argument: '{BuildConfiguration}'");
            }

            BuildConfiguration = "Debug";
        }

        BuildConfiguration ??= "Debug";

        // Default to net481, which runs everywhere (including outside Windows via Mono)
        var framework = context.Argument("framework", "netframework");
        FrameworkKinds = framework.ToLowerInvariant() switch
        {
            "netframework" => BuildFrameworks.NetFramework,
            "dotnet" => BuildFrameworks.DotNet,
            "all" => BuildFrameworks.All,
            _ => throw new Exception($"Unknown framework '{framework}', expected netframework, dotnet or all"),
        };

        Paths = new BuildPaths(rootDir, BuildConfiguration, GetVersion(false),
            context.IsRunningOnWindows());

        referenceAssemblyDirectories = new Lazy<IReadOnlyList<DirectoryPath>>(GetReferenceAssemblyDirectories);
    }

    public SemVersion GetVersion(bool withBuild = true)
    {
        var rootDirectory = Environment.WorkingDirectory.GetParent();

        var versionMatch = File
            .ReadAllLines(rootDirectory.CombineWithFilePath("CHANGELOG.md").FullPath)
            .Select(i => VersionRegex().Match(i))
            .First(i => i.Success);

        if (!SemVersion.TryParse(versionMatch.Groups["version"].Value, out var version))
        {
            throw new Exception("Could not parse version from CHANGELOG.md");
        }

        if (withBuild && this.DirectoryExists(rootDirectory.Combine(".git")))
        {
            var commitDate = this.GitLogTip(rootDirectory).Committer.When;
            version = new SemVersion(version.Major,
                version.Minor,
                version.Patch,
                version.PreRelease,
                "." + commitDate.ToString("yy") + commitDate.DayOfYear.ToString("000"));
        }

        return version;
    }

    /// <summary>
    /// Publish a self-contained single-file executable for the host platform.
    /// </summary>
    /// <param name="project">The path of the .csproj to publish.</param>
    /// <param name="assemblyName">The name of the built executable in <c>out</c>, without any .exe suffix</param>
    /// <param name="framework">The target framework, such as <see cref="BuildDotNet"/></param>
    /// <param name="target">The destination path of the built executable</param>
    public void PublishNative(FilePath project, string assemblyName, string framework, FilePath target)
    {
        this.DotNetPublish(project.FullPath, new DotNetPublishSettings
        {
            Configuration = BuildConfiguration,
            Framework = framework,
            Runtime = HostRuntime,
            PublishSingleFile = true,
            SelfContained = true,
            // At the time of writing there aren't any native libraries, but this would make them be bundled within
            // the executable so the built binary can be moved around independent of its dylibs.
            IncludeNativeLibrariesForSelfExtract = true,
        });
        this.CreateDirectory(target.GetDirectory());
        this.CopyFile(Paths.OutDirectory.Combine(assemblyName)
                .Combine(BuildConfiguration)
                .Combine("bin")
                .Combine(framework)
                .Combine(HostRuntime)
                .Combine("publish")
                .CombineWithFilePath(this.IsRunningOnWindows()
                    ? $"{assemblyName}.exe"
                    : assemblyName),
            target);
    }

    // Copy an executable to the top of _build
    public void CopyToBuildDirectory(FilePath file)
    {
        var finalExePath = Paths.BuildDirectory.CombineWithFilePath(file.GetFilename());
        this.CopyFile(file, finalExePath);
        ChmodExecutable(finalExePath);
    }

    public void ReportRepacking(FilePath target, FilePath log)
    {
        // Repacks can run in parallel, and NormalVerbosity changes the verbosity for every thread
        lock (logLock)
        {
            using (this.NormalVerbosity())
            {
                this.Information("Repacking {0}, logging details to {1}...",
                    Paths.RootDirectory.GetRelativePath(target),
                    Paths.RootDirectory.GetRelativePath(log));
            }
        }
    }

    private readonly object logLock = new();

    // Run actions on the thread pool and wait for all of them,
    // throwing the first exception if any fail
    public static void RunInParallel(IEnumerable<Action> actions)
        => Task.WhenAll(actions.Select(Task.Run))
            .GetAwaiter()
            .GetResult();

    public void Repack(FilePath               target,
                       DirectoryPath          assembliesPath,
                       string                 sourceFilename,
                       TargetPlatformVersion? targetPlatform,
                       FilePath               logFile)
    {
        this.CreateDirectory(target.GetDirectory());
        this.CreateDirectory(logFile.GetDirectory());
        var source     = assembliesPath.CombineWithFilePath(sourceFilename);
        var assemblyPaths = this.GetFiles($"{assembliesPath}/*.dll");
        // Need facade to instantiate types from netstandard2.0 DLLs on Mono
        assemblyPaths.Add(ReferenceAssemblyDirectories.Select(d => d.CombineWithFilePath("netstandard.dll"))
                                                      .FirstOrDefault(this.FileExists)
                          ?? throw new Exception("netstandard.dll not found in reference assembly directories: "
                                                 + string.Join(", ", ReferenceAssemblyDirectories)));
        assemblyPaths.Add(this.GetFiles($"{assembliesPath}/*/*.resources.dll"));
        if (target.FullPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            assemblyPaths.Add(this.GetFiles($"{assembliesPath}/*.exe"));
        }
        assemblyPaths.Remove(source);
        // ILRepack is extremely noisy by default and has no options to
        // make it quieter other than shutting it up completely.
        ReportRepacking(target, logFile);
        this.ILRepack(target, source, assemblyPaths,
                      new ILRepackSettings
                      {
                          Libs                 = [assembliesPath, .. ReferenceAssemblyDirectories],
                          TargetPlatform       = targetPlatform,
                          Parallel             = true,
                          Verbose              = false,
                          SetupProcessSettings = RepackSilently,
                          Log                  = logFile.FullPath,
                      });
    }

    public static void RepackSilently(ProcessSettings settings)
        => settings.SetRedirectStandardOutput(true)
                   .SetRedirectedStandardOutputHandler(s => "")
                   .SetRedirectStandardError(true)
                   .SetRedirectedStandardErrorHandler(s => "");

    public static void ChmodExecutable(FilePath path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var proc = Process.Start(new ProcessStartInfo("chmod",
                                                          $"+x \"{path}\"")
            {
                UseShellExecute = false,
            });
            proc?.WaitForExit();
        }
    }

    public string? GetQuote(FilePath file)
    {
        if (!this.FileExists(file))
        {
            return null;
        }

        var quotes = File.ReadAllText(file.FullPath)
                         .Split("%", StringSplitOptions.RemoveEmptyEntries);

        return quotes.Length > 0 ? quotes[new Random().Next(quotes.Length)] : null;
    }

    public IEnumerable<string> RunExecutable(FilePath executable, string arguments)
    {
        var exitCode = this.StartProcess(
            executable,
            new ProcessSettings { Arguments = arguments, RedirectStandardOutput = true },
            out IEnumerable<string> output
        );

        if (exitCode != 0)
        {
            throw new Exception("Process failed with exit code: " + exitCode);
        }

        return output;
    }

    public void RunAltCover(params string[] args)
    {
        if (this.StartProcess(Paths.AltCoverPath,
                              new ProcessSettings
                              {
                                  WorkingDirectory = Paths.CoverageOutputDirectory,
                                  Arguments        = string.Join(" ", args),
                              })
            is int exitCode and not 0)
        {
            throw new Exception($"AltCover failed with exit code: {exitCode}");
        }
    }

    [GeneratedRegex(@"^\s*##\s+v(?<version>\S+)\s?.*$")]
    private static partial Regex VersionRegex();

    // .NET Framework reference assemblies from NuGet, including Facades. These are made available
    // during the restore step because of a PackageReference in `Directory.Build.props`.
    private IReadOnlyList<DirectoryPath> ReferenceAssemblyDirectories
        => referenceAssemblyDirectories.Value;

    // Lazy so parallel repacks share one msbuild query instead of each starting their own
    private readonly Lazy<IReadOnlyList<DirectoryPath>> referenceAssemblyDirectories;

    /// <summary>
    /// Asks MSBuild which reference assembly directories are used by the compiler.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">If msbuild could not be queried, or there are no directories found.</exception>
    private List<DirectoryPath> GetReferenceAssemblyDirectories()
    {
        var process = this.StartAndReturnProcess("dotnet", new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                .Append("msbuild")
                .AppendQuoted(Paths.CmdlineProject.FullPath)
                .Append("-nologo")
                .Append($"-p:Configuration={BuildConfiguration}")
                .Append($"-p:TargetFramework={BuildNetFramework}")
                .Append("-t:GetReferenceAssemblyPaths")
                .Append("-getProperty:TargetFrameworkDirectory"),
            RedirectStandardOutput = true,
        });

        process.WaitForExit();
        if (process.GetExitCode() is var exitCode and not 0)
        {
            throw new Exception($"Finding reference assemblies failed with exit code: {exitCode}");
        }

        var output = process.GetStandardOutput().FirstOrDefault() ?? "";
        var dirs = output.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => new DirectoryPath(d))
            .ToList();
        return dirs.Count > 0
            ? dirs
            : throw new Exception("Reference assemblies not found, run the Restore task first");
    }
}

public enum BuildFrameworks
{
    /// <summary>
    /// Both.
    /// </summary>
    All,

    /// <summary>
    /// net481 exe (.NET Framework / Mono), with GUI support
    /// </summary>
    NetFramework,

    /// <summary>
    /// Self-contained native executables for the host, with GUI support only on Windows
    /// </summary>
    DotNet,
}

public static class BuildFrameworksExtensions
{
    public static bool IncludesNetFramework(this BuildFrameworks frameworks)
        => frameworks != BuildFrameworks.DotNet;

    public static bool IncludesDotNet(this BuildFrameworks frameworks)
        => frameworks != BuildFrameworks.NetFramework;
}
