using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Solution.Project.Properties;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Restore;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Common.Tools.DotNet.Test;
using Cake.Common.Tools.ILMerge;
using Cake.Common.Tools.ILRepack;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.NUnit;
using Cake.Core.IO;
using Cake.Frosting;
using AltCover;
using AltCover.Cake;

namespace Build;

public static class Program
{
    public static int Main(string[] args)
        => new CakeHost()
            .ConfigureServices(services =>
            {
                services.UseToolPath(new DirectoryPath(Environment.CurrentDirectory)
                    .GetParent()
                    .Combine("_build")
                    .Combine("tools"));
            })
            .InstallTool(new Uri("nuget:?package=ILRepack&version=2.0.44"))
            .InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.16.3"))
            .InstallTool(new Uri("nuget:?package=altcover&version=9.0.1"))
            .InstallTool(new Uri("nuget:?package=altcover.api&version=9.0.1"))
            .InstallTool(new Uri("nuget:?package=altcover.cake&version=9.0.1"))
            .UseContext<BuildContext>()
            .UseLifetime<BuildLifetime>()
            .Run(args);
}

[TaskName("Default")]
[TaskDescription("Build ckan.exe and netkan.exe")]
[IsDependentOn(typeof(CkanTask))]
[IsDependentOn(typeof(NetkanTask))]
public sealed class DefaultTask : FrostingTask<BuildContext>;

[TaskName("Debug")]
[TaskDescription("Build ckan.exe and netkan.exe in Debug configuration")]
[IsDependentOn(typeof(DefaultTask))]
public sealed class DebugTask : FrostingTask<BuildContext>;

[TaskName("Release")]
[TaskDescription("Build ckan.exe and netkan.exe in Release configuration")]
[IsDependentOn(typeof(DefaultTask))]
public sealed class ReleaseTask : FrostingTask<BuildContext>;

[TaskName("Netkan")]
[TaskDescription("Build only netkan.exe")]
[IsDependentOn(typeof(RepackNetkanTask))]
public sealed class NetkanTask : FrostingTask<BuildContext>;

[TaskName("Ckan")]
[TaskDescription("Build only ckan.exe")]
[IsDependentOn(typeof(RepackCkanTask))]
public sealed class CkanTask : FrostingTask<BuildContext>;

[TaskName("Restore")]
[TaskDescription("Intermediate - Download dependencies")]
public sealed class RestoreTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetRestore(new DotNetRestoreSettings
        {
            WorkingDirectory = context.Paths.RootDirectory,
            EnvironmentVariables = new Dictionary<string, string?> { { "Configuration", context.BuildConfiguration } }
        });
    }
}

[TaskName("Generate-GlobalAssemblyVersionInfo")]
[TaskDescription("Intermediate - Calculate the version strings for the assembly.")]
public sealed class GenerateGlobalAssemblyVersionInfoTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var metaDirectory = context.Paths.BuildDirectory.Combine("meta");
        context.CreateDirectory(metaDirectory);

        var version = context.GetVersion();

        context.CreateAssemblyInfo(
            metaDirectory.CombineWithFilePath("GlobalAssemblyVersionInfo.cs"), new AssemblyInfoSettings
            {
                Version = $"{version.Major}.{version.Minor}",
                FileVersion = version.HasMeta
                    ? $"{version.Major}.{version.Minor}.{version.Patch}{version.Meta}"
                    : $"{version.Major}.{version.Minor}.{version.Patch}",
                InformationalVersion = version.ToString(),
            });
    }
}

[TaskName("Build")]
[TaskDescription("Intermediate - Build everything")]
[IsDependentOn(typeof(RestoreTask))]
[IsDependentOn(typeof(GenerateGlobalAssemblyVersionInfoTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // Build Core for all of its target frameworks at once, which creates the nupkg
        context.DotNetBuild(context.Paths.CoreProject.FullPath, new DotNetBuildSettings
        {
            Configuration = context.BuildConfiguration,
            NoRestore     = true,
        });

        foreach (var framework in context.Frameworks)
        {
            // We build Tests.csproj instead of CKAN.sln because the projects don't all support the same
            // frameworks: `dotnet build CKAN.sln -f net10.0` would fail to build GUI and AutoUpdate, and
            // `-f net10.0-windows` fails for Core, ConsoleUI and Netkan. However, Tests.csproj references every
            // other project, and its ProjectReferences (via SetTargetFramework) specify which projects and
            // frameworks each of our frameworks supports, so e.g. Netkan is built for net10.0 when building
            // net10.0-windows, and GUI is skipped for net10.0. So building it indirectly builds everything the
            // executables and tests need, and checks that the tests compile.
            // One consequence of this is that any new project must be referenced from Tests.csproj, or it won't be
            // built here.
            context.DotNetBuild(context.Paths.TestsProject.FullPath, new DotNetBuildSettings
            {
                Configuration = context.BuildConfiguration,
                Framework     = framework,
                NoRestore     = true,
            });
        }
    }
}

[TaskName("Repack-Ckan")]
[TaskDescription("Intermediate - Merge all the separate DLLs and EXEs to a single executable.")]
[IsDependentOn(typeof(BuildTask))]
public sealed class RepackCkanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var repackPath = context.Paths.RepackDirectory.Combine(context.BuildConfiguration);
        // The repack and publish can run at the same time because they access entirely different builds.
        var jobs = new List<Action>();
        if (context.BuildsNetFramework)
        {
            jobs.Add(() => context.Repack(context.Paths.Ckan.NetFrameworkPath,
                                          context.Paths.OutDirectory.Combine("CKAN-CmdLine")
                                                                    .Combine(context.BuildConfiguration)
                                                                    .Combine("bin")
                                                                    .Combine(context.BuildNetFramework),
                                          "CKAN-CmdLine.exe",
                                          TargetPlatformVersion.v4,
                                          repackPath.CombineWithFilePath("ckan.log")));
            jobs.Add(() => context.Repack(context.Paths.UpdaterFile,
                                          context.Paths.OutDirectory.Combine("CKAN-AutoUpdateHelper")
                                                                    .Combine(context.BuildConfiguration)
                                                                    .Combine("bin")
                                                                    .Combine(context.BuildNetFramework),
                                          "CKAN-AutoUpdateHelper.exe",
                                          TargetPlatformVersion.v4,
                                          repackPath.CombineWithFilePath("AutoUpdater.log")));
        }

        if (context.BuildsDotNet)
        {
            // On Windows this is the .NET 10 build with the GUI, for dark theme
            jobs.Add(() => context.PublishNative(context.Paths.CmdlineProject, "CKAN-CmdLine",
                                                 context.NativeCkanFramework, context.Paths.Ckan.NativePath));
        }

        BuildContext.RunInParallel(jobs);

        foreach (var file in context.Paths.Ckan.AllFor(context.FrameworkKinds))
        {
            context.CopyToBuildDirectory(file);
        }

        if (context.BuildConfiguration == "Release")
        {
            var pubSettings = new DotNetPublishSettings
            {
                Configuration     = context.BuildConfiguration,
                Framework         = context.BuildDotNet,
                Runtime           = "linux-x64",
                PublishSingleFile = true,
                SelfContained     = true,
            };
            // Publish Netkan for Inflator and Metadata containers
            context.DotNetPublish(context.Paths.NetkanProject.FullPath, pubSettings);
            // Publish Cmdline for Metadata container
            context.DotNetPublish(context.Paths.CmdlineProject.FullPath, pubSettings);
        }
    }
}

[TaskName("Repack-Netkan")]
[TaskDescription("Intermediate - Merge all the separate DLLs and EXEs to a single executable.")]
[IsDependentOn(typeof(BuildTask))]
public sealed class RepackNetkanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.CreateDirectory(context.Paths.RepackDirectory.Combine(context.BuildConfiguration));
        // The repack and publish can run at the same time because they access entirely different builds.
        var jobs = new List<Action>();
        if (context.BuildsNetFramework)
        {
            jobs.Add(() => context.Repack(context.Paths.Netkan.NetFrameworkPath,
                                          context.Paths.OutDirectory.Combine("CKAN-NetKAN")
                                                                    .Combine(context.BuildConfiguration)
                                                                    .Combine("bin")
                                                                    .Combine(context.BuildNetFramework),
                                          "CKAN-NetKAN.exe",
                                          TargetPlatformVersion.v4,
                                          context.Paths.RepackDirectory.Combine(context.BuildConfiguration)
                                                                       .CombineWithFilePath("netkan.log")));
        }

        if (context.BuildsDotNet)
        {
            jobs.Add(() => context.PublishNative(context.Paths.NetkanProject, "CKAN-NetKAN",
                                                 context.BuildDotNet, context.Paths.Netkan.NativePath));
        }

        BuildContext.RunInParallel(jobs);

        foreach (var file in context.Paths.Netkan.AllFor(context.FrameworkKinds))
        {
            context.CopyToBuildDirectory(file);
        }
    }
}

[TaskName("Prepare-SignPath")]
[TaskDescription("Create a folder with all artifacts to be signed")]
[IsDependentOn(typeof(RepackCkanTask))]
public sealed class PrepareSignPathTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var targetDir = context.Paths.BuildDirectory.Combine("signpath")
            .Combine(context.BuildConfiguration);
        context.CreateDirectory(targetDir);
        var files = new List<FilePath> { context.Paths.NupkgFile };
        if (context.BuildsNetFramework)
        {
            files.Add(context.Paths.Ckan.NetFrameworkPath);
            files.Add(context.Paths.UpdaterFile);
        }

        if (context.BuildsDotNet && context.IsRunningOnWindows())
        {
            files.Add(context.Paths.Ckan.NativePath);
        }

        foreach (var file in files)
        {
            context.CopyFile(file, targetDir.CombineWithFilePath(file.GetFilename()));
        }
    }
}

[TaskName("Test")]
[TaskDescription("Run tests after compilation.")]
[IsDependentOn(typeof(DefaultTask))]
[IsDependentOn(typeof(TestOnlyTask))]
public sealed class TestTask : FrostingTask<BuildContext>;

[TaskName("Test-UnitTests")]
[IsDependentOn(typeof(BuildTask))]
[IsDependentOn(typeof(TestUnitTestsOnlyTask))]
public sealed class TestUnitTestsTask : FrostingTask<BuildContext>;

[TaskName("Test+Only")]
[TaskDescription("Run tests without compiling.")]
[IsDependentOn(typeof(TestExecutablesOnlyTask))]
[IsDependentOn(typeof(TestUnitTestsOnlyTask))]
public sealed class TestOnlyTask : FrostingTask<BuildContext>;

[TaskName("Test-Executables+Only")]
[TaskDescription("Intermediate - Test executables without compiling.")]
[IsDependentOn(typeof(TestCkanExecutableOnlyTask))]
[IsDependentOn(typeof(TestNetkanExecutableOnlyTask))]
public sealed class TestExecutablesOnlyTask : FrostingTask<BuildContext>;

[TaskName("Test-CkanExecutable+Only")]
[TaskDescription("Intermediate - Test ckan.exe without compiling.")]
public sealed class TestCkanExecutableOnlyTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        foreach (var file in context.Paths.Ckan.AllFor(context.FrameworkKinds))
        {
            var output = context.RunExecutable(file, "version").FirstOrDefault();
            if (output != $"v{context.GetVersion()}")
            {
                throw new Exception($"{file.GetFilename()} smoke test failed: {output}");
            }
        }
    }
}

[TaskName("Test-NetkanExecutable+Only")]
[TaskDescription("Intermediate - Test netkan.exe without compiling.")]
public sealed class TestNetkanExecutableOnlyTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        foreach (var file in context.Paths.Netkan.AllFor(context.FrameworkKinds))
        {
            var output = context.RunExecutable(file, "--version").FirstOrDefault();
            if (output != $"v{context.GetVersion()}")
            {
                throw new Exception($"{file.GetFilename()} smoke test failed: {output}");
            }
        }
    }
}

[TaskName("Test-UnitTests+Only")]
[TaskDescription("Intermediate - Run unit tests without compiling.")]
public sealed class TestUnitTestsOnlyTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var where  = context.Argument<string?>("where", null);
        var labels = context.Argument("labels", "Off");
        var withCoverage = context.Argument("coverage", "On") != "Off";
        var nunitOutputDirectory = context.Paths.BuildDirectory
                                                .Combine("test")
                                                .Combine("nunit");
        context.CreateDirectory(nunitOutputDirectory);
        context.CreateDirectory(context.Paths.CoverageOutputDirectory);
        var dotNetFilter = where?.Replace("class=", "FullyQualifiedName=",
                                          StringComparison.CurrentCultureIgnoreCase)
                                 .Replace("category=", "TestCategory=",
                                          StringComparison.CurrentCultureIgnoreCase)
                                 .Replace("category!=", "TestCategory!=",
                                          StringComparison.CurrentCultureIgnoreCase)
                                 .Replace("namespace=", "FullyQualifiedName~",
                                          StringComparison.CurrentCultureIgnoreCase)
                                 .Replace("name=", "Name~",
                                          StringComparison.CurrentCultureIgnoreCase);

        var altcoverSettings = new CoverageSettings
        {
            PreparationPhase = new MyPrepareOptions(context),
            CollectionPhase  = new MyCollectOptions(context),
            Options          = new MyTestOptions(),
        };
        // We don't enable coverage in dotnet test on Non-Windows platforms because it doesn't support running net481.
        // Instead, test coverage just comes entirely from NUnit in Mono.
        var dotNetCoverage = withCoverage && context.IsRunningOnWindows();

        // Run each framework's tests at the same time, like a single multi-targeted `dotnet test` would.
        // Each gets its own coverage output files so they don't overwrite each other.
        var jobs = new List<Action>();
        var dotNetFrameworks = new List<string>();
        foreach (var framework in context.Frameworks)
        {
            if (framework == context.BuildNetFramework && !context.IsRunningOnWindows())
            {
                // dotnet test can't run net481 outside Windows
                jobs.Add(() => RunNUnitOnMono(context, where, labels, withCoverage,
                                              nunitOutputDirectory, altcoverSettings));
            }
            else
            {
                dotNetFrameworks.Add(framework);
                jobs.Add(() => RunDotNetTest(context, framework, dotNetFilter,
                                             dotNetCoverage, nunitOutputDirectory));
            }
        }
        BuildContext.RunInParallel(jobs);

        if (dotNetCoverage)
        {
            // Now combine the target-specific coverage data into one file
            OpenCover.Merge(dotNetFrameworks.Select(f => $"coverage.{f}.xml")
                                            .Select(context.Paths.CoverageOutputFile)
                                            .Select(f => f.FullPath)
                                            .Select(XDocument.Load))
                     .Save(context.Paths.CoverageOutputFile("coverage.xml")
                                        .FullPath);
            foreach (var framework in dotNetFrameworks)
            {
                // Remove spurious duplicate entries with '\' paths from cobertura.xml
                var path = context.Paths.CoverageOutputFile($"cobertura.{framework}.xml").FullPath;
                var cobertura = XDocument.Load(path);
                cobertura.Descendants().Where(elt => elt.Attribute("filename")
                                                        ?.Value.Contains('\\')
                                                        ?? false)
                                       .Remove();
                cobertura.Save(path);
            }
        }
    }

    private static void RunDotNetTest(BuildContext  context,
                                      string        framework,
                                      string?       filter,
                                      bool          withCoverage,
                                      DirectoryPath resultsDirectory)
    {
        var testSettings = new DotNetTestSettings
        {
            Configuration    = context.BuildConfiguration,
            Framework        = framework,
            NoRestore        = true,
            NoBuild          = true,
            NoLogo           = true,
            Filter           = filter,
            ResultsDirectory = resultsDirectory,
            Verbosity        = DotNetVerbosity.Minimal,
        };
        if (withCoverage)
        {
            testSettings.ArgumentCustomization = new CoverageSettings
            {
                PreparationPhase = new MyPrepareOptions(context),
                CollectionPhase  = new MyCollectOptions(context, framework),
                Options          = new MyTestOptions(),
            }.Concatenate(testSettings.ArgumentCustomization);
        }
        context.DotNetTest(context.Paths.TestsProject.FullPath, testSettings);
    }

    // Run the net481 tests in the NUnit console runner on Mono
    private static void RunNUnitOnMono(BuildContext     context,
                                       string?          where,
                                       string           labels,
                                       bool             withCoverage,
                                       DirectoryPath    nunitOutputDirectory,
                                       CoverageSettings altcoverSettings)
    {
        var testDir = context.Paths.OutDirectory
                                   .Combine("CKAN.Tests")
                                   .Combine(context.BuildConfiguration)
                                   .Combine("bin")
                                   .Combine(context.BuildNetFramework);
        var instrumentedDir = testDir.GetParent()
                                     .Combine($"{context.BuildNetFramework}__Instrumented");
        // The instrumented copy only exists with coverage on
        var testFile = (withCoverage ? instrumentedDir : testDir).CombineWithFilePath("CKAN.Tests.dll");

        if (withCoverage)
        {
            // Add coverage instrumentation to our test assemblies
            context.RunAltCover(altcoverSettings.PreparationPhase
                                                .ToArguments(testDir, instrumentedDir));
        }

        context.NUnit3(testFile.FullPath, new NUnit3Settings
        {
            Configuration = context.BuildConfiguration,
            Where         = where,
            Labels        = Enum.Parse<NUnit3Labels>(labels),
            Work          = nunitOutputDirectory,
            NoHeader      = true,
            // Work around System.Runtime.Remoting.RemotingException : Tcp transport error.
            Process       = NUnit3ProcessOption.InProcess,
        });

        if (withCoverage)
        {
            // Transform the raw coverage data into coverage.xml and print a summary
            context.RunAltCover(altcoverSettings.CollectionPhase
                                                .ToArguments(instrumentedDir));
        }
    }
}

// AltCover.cake regrettably requires defining new types rather than just filling in a struct

public class MyPrepareOptions(BuildContext context) : PrepareOptions
{
    public override TraceLevel Verbosity => TraceLevel.Info;

    public override IEnumerable<string> AssemblyFilter =>
    [
        "Microsoft", "NUnit3", "testhost",
        "IndexRange", "OxyPlot", "Humanizer", @"AltCover\.Monitor",
        "CKAN-ConsoleUI", @"CKAN\.Tests",
    ];

    public override IEnumerable<string> TypeFilter =>
    [
        "System", "Microsoft",
    ];

    public override IEnumerable<string> PathFilter =>
    [
        "_build",
        "GUI/Dialogs", "GUI/Controls", "GUI/Main",
        @"GUI\\Dialogs", @"GUI\\Controls", @"GUI\\Main",
    ];

    public override IEnumerable<string> AttributeFilter =>
    [
        "ExcludeFromCodeCoverage",
    ];

    public override bool LocalSource => true;

    public override string Report => OutputPath("coverage.xml");

    private string OutputPath(string filename)
        => context.Paths.CoverageOutputFile(filename).FullPath;

    private readonly BuildContext context = context;
}

// If framework is set, it is added to the file names (e.g. cobertura.net481.xml),
// so test runs for different frameworks can write their output at the same time
public class MyCollectOptions(BuildContext context, string? framework = null) : CollectOptions
{
    public override TraceLevel Verbosity => TraceLevel.Info;
    public override string OutputFile    => OutputPath("output", "xml");
    public override string Cobertura     => OutputPath("cobertura", "xml");
    public override string LcovReport    => OutputPath("lcov", "info");

    private string OutputPath(string name, string extension)
        => context.Paths.CoverageOutputFile(framework == null
                                                ? $"{name}.{extension}"
                                                : $"{name}.{framework}.{extension}")
                        .FullPath;

    private readonly BuildContext context = context;
    private readonly string? framework = framework;
}

public class MyTestOptions : TestOptions
{
    public override bool ForceDelete => true;
    public override bool FailFast    => true;
}

public static class CoverageExtensions
{
    public static string[] ToArguments(this Abstract.IPrepareOptions opts,
                                       DirectoryPath                 inDir,
                                       DirectoryPath                 outDir)
        =>
        [
            .. Verbosity(opts.Verbosity),
            .. opts.AssemblyFilter.Select(f => $"-s {f}"),
            .. opts.AssemblyExcludeFilter.Select(f => $"-e {f}"),
            .. opts.TypeFilter.Select(f => $"-t {f}"),
            .. opts.PathFilter.Select(f => $"-p {f}"),
            .. opts.AttributeFilter.Select(f => $"-a {f}"),
            .. opts.LocalSource ? Enumerable.Repeat("-l", 1) : [],
            $"-i {inDir}",
            $"-o {outDir}",
            "--save",
        ];

    public static string[] ToArguments(this Abstract.ICollectOptions opts,
                                       DirectoryPath                 dir)
        =>
        [
            "runner",
            "--collect",
            .. Verbosity(opts.Verbosity),
            opts.SummaryFormat is { Length: > 0 } sf
                ? $"--summary={sf}"
                : "",
            $"-r {dir}",
            $"-l {opts.LcovReport}",
            $"-c {opts.Cobertura}",
            $"-o {opts.OutputFile}",
        ];

    private static IEnumerable<string> Verbosity(TraceLevel level)
        => level switch
           {
               TraceLevel.Off     => Enumerable.Repeat("-q", 3),
               TraceLevel.Error   => Enumerable.Repeat("-q", 2),
               TraceLevel.Warning => Enumerable.Repeat("-q", 1),
               TraceLevel.Info    => [],
               TraceLevel.Verbose => Enumerable.Repeat("--verbose", 1),
               _                  => [],
           };
}

[TaskName("Version")]
[TaskDescription("Print the current CKAN version.")]
public sealed class VersionTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        using (context.NormalVerbosity())
        {
            context.Information(context.GetVersion().ToString());
        }
    }
}
