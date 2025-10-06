using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
using AltCover.Cake;

namespace Build;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .ConfigureServices(services =>
            {
                services.UseToolPath(new DirectoryPath(Environment.CurrentDirectory)
                    .GetParent()
                    .Combine("_build")
                    .Combine("tools"));
            })
            .InstallTool(new Uri("nuget:?package=ILRepack&version=2.0.27"))
            .InstallTool(new Uri("nuget:?package=NUnit.ConsoleRunner&version=3.16.3"))
            .InstallTool(new Uri("nuget:?package=altcover.api&version=9.0.1"))
            .UseContext<BuildContext>()
            .UseLifetime<BuildLifetime>()
            .Run(args);
    }
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
            PackagesDirectory = context.Paths.NugetDirectory,
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
        // dotnet build won't let us compile WinForms on non-Windows,
        // fall back to mono
        if (context.IsRunningOnWindows())
        {
            context.DotNetBuild(context.Solution, new DotNetBuildSettings
            {
                Configuration = context.BuildConfiguration,
                NoRestore     = true,
            });
        }
        else
        {
            // Use dotnet to build the Core DLL to get the nupkg
            // (only created if all TargetFrameworks are built together)
            context.DotNetBuild(context.Paths.CoreProject.FullPath, new DotNetBuildSettings
            {
                Configuration = context.BuildConfiguration,
            });
            // Publish Netkan for Inflator and Metadata containers
            context.DotNetPublish(context.Paths.NetkanProject.FullPath, new DotNetPublishSettings
            {
                Configuration  = context.BuildConfiguration,
                Framework      = "net8.0",
                Runtime        = "linux-x64",
                SelfContained  = true,
            });
            // Publish Cmdline for Metadata container
            context.DotNetPublish(context.Paths.CmdlineProject.FullPath, new DotNetPublishSettings
            {
                Configuration  = context.BuildConfiguration,
                Framework      = "net8.0",
                Runtime        = "linux-x64",
                SelfContained  = true,
            });

            // Use Mono to build for net481 since dotnet can't use WinForms on Linux
            context.MSBuild(context.Solution, new MSBuildSettings
            {
                Configuration = context.BuildConfiguration,
                MaxCpuCount = 0,
                Properties = { { "TargetFramework", [context.BuildNetFramework] } },
            });
            // Use dotnet to build the stuff Mono can't build
            context.DotNetBuild(context.Solution, new DotNetBuildSettings
            {
                Configuration = "NoGUI",
                Framework     = "net8.0",
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
        context.CreateDirectory(context.Paths.RepackDirectory.Combine(context.BuildConfiguration));

        var cmdLineBinDirectory = context.Paths.OutDirectory.Combine("CKAN-CmdLine")
                                              .Combine(context.BuildConfiguration)
                                              .Combine("bin")
                                              .Combine(context.BuildNetFramework);
        var assemblyPaths = context.GetFiles($"{cmdLineBinDirectory}/*.dll");
        assemblyPaths.Add(cmdLineBinDirectory.CombineWithFilePath("CKAN-GUI.exe"));
        assemblyPaths.Add(cmdLineBinDirectory.CombineWithFilePath("CKAN-ConsoleUI.exe"));
        var cmdlinePath = context.Paths.OutDirectory.Combine("CKAN-CmdLine")
            .Combine(context.BuildConfiguration)
            .Combine("bin")
            .Combine(context.BuildNetFramework);
        assemblyPaths.Add(context.GetFiles($"{cmdlinePath}/*/*.resources.dll"));
        // Need facade to instantiate types from netstandard2.0 DLLs on Mono
        assemblyPaths.Add(context.FacadesDirectory().CombineWithFilePath("netstandard.dll"));
        var ckanLogFile = context.Paths.RepackDirectory.Combine(context.BuildConfiguration)
                                         .CombineWithFilePath("ckan.log");
        context.ReportRepacking(context.Paths.CkanFile, ckanLogFile);
        context.ILRepack(
            context.Paths.CkanFile,
            cmdLineBinDirectory.CombineWithFilePath("CKAN-CmdLine.exe"),
            assemblyPaths,
            new ILRepackSettings
            {
                Libs                 = [cmdLineBinDirectory],
                TargetPlatform       = TargetPlatformVersion.v4,
                Parallel             = true,
                Verbose              = false,
                SetupProcessSettings = BuildContext.RepackSilently,
                Log                  = ckanLogFile.FullPath,
            });

        var autoupdateBinDirectory = context.Paths.OutDirectory.Combine("CKAN-AutoUpdateHelper")
                                                 .Combine(context.BuildConfiguration)
                                                 .Combine("bin")
                                                 .Combine(context.BuildNetFramework);
        var updaterLogFile = context.Paths.RepackDirectory.Combine(context.BuildConfiguration)
                                            .CombineWithFilePath("AutoUpdater.log");
        context.ReportRepacking(context.Paths.UpdaterFile, updaterLogFile);
        context.ILRepack(
            context.Paths.UpdaterFile,
            autoupdateBinDirectory.CombineWithFilePath("CKAN-AutoUpdateHelper.exe"),
            context.GetFiles($"{autoupdateBinDirectory}/*/*.resources.dll"),
            new ILRepackSettings
            {
                Libs           = [autoupdateBinDirectory],
                TargetPlatform = TargetPlatformVersion.v4,
                Parallel       = true,
                Verbose        = false,
                SetupProcessSettings = BuildContext.RepackSilently,
                Log            = updaterLogFile.FullPath,
            });

        var finalExePath = context.Paths.BuildDirectory.CombineWithFilePath(context.Paths.CkanFile.GetFilename());
        context.CopyFile(context.Paths.CkanFile, finalExePath);
        BuildContext.ChmodExecutable(finalExePath);
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
        var netkanBinDirectory = context.Paths.OutDirectory.Combine("CKAN-NetKAN")
            .Combine(context.BuildConfiguration)
            .Combine("bin")
            .Combine(context.BuildNetFramework);
        var netkanLogFile = context.Paths.RepackDirectory.Combine(context.BuildConfiguration)
            .CombineWithFilePath("netkan.log");
        var assemblyPaths = context.GetFiles($"{netkanBinDirectory}/*.dll");
        // Need facade to instantiate types from netstandard2.0 DLLs on Mono
        assemblyPaths.Add(context.FacadesDirectory().CombineWithFilePath("netstandard.dll"));
        context.ReportRepacking(context.Paths.NetkanFile, netkanLogFile);
        context.ILRepack(
            context.Paths.NetkanFile,
            netkanBinDirectory.CombineWithFilePath("CKAN-NetKAN.exe"),
            assemblyPaths,
            new ILRepackSettings
            {
                Libs           = [netkanBinDirectory],
                TargetPlatform = TargetPlatformVersion.v4,
                Parallel       = true,
                Verbose        = false,
                SetupProcessSettings = BuildContext.RepackSilently,
                Log            = netkanLogFile.FullPath,
            }
        );

        var finalExePath = context.Paths.BuildDirectory.CombineWithFilePath(context.Paths.NetkanFile.GetFilename());
        context.CopyFile(context.Paths.NetkanFile, finalExePath);
        BuildContext.ChmodExecutable(finalExePath);
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
        context.CopyFile(context.Paths.CkanFile,    targetDir.CombineWithFilePath(context.Paths.CkanFile.GetFilename()));
        context.CopyFile(context.Paths.UpdaterFile, targetDir.CombineWithFilePath(context.Paths.UpdaterFile.GetFilename()));
        context.CopyFile(context.Paths.NupkgFile,   targetDir.CombineWithFilePath(context.Paths.NupkgFile.GetFilename()));
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
        var output = context.RunExecutable(context.Paths.CkanFile, "version").FirstOrDefault();
        if (output != $"v{context.GetVersion()}")
        {
            throw new Exception($"ckan.exe smoke test failed: {output}");
        }
    }
}

[TaskName("Test-NetkanExecutable+Only")]
[TaskDescription("Intermediate - Test netkan.exe without compiling.")]
public sealed class TestNetkanExecutableOnlyTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var output = context.RunExecutable(context.Paths.NetkanFile, "--version").FirstOrDefault();
        if (output != $"v{context.GetVersion()}")
        {
            throw new Exception($"netkan.exe smoke test failed: {output}");
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

        // Only Mono's msbuild can handle WinForms on Linux,
        // but dotnet build can handle multi-targeting on Windows
        if (context.IsRunningOnWindows())
        {
            var altcoverSettings = new CoverageSettings
            {
                PreparationPhase = new MyPrepareOptions(context),
                // CollectionPhase  = new CollectOptions(),
                CollectionPhase  = new MyCollectOptions(context),
                Options          = new TestOptions(),
            };
            var testSettings = new DotNetTestSettings
            {
                Configuration    = context.BuildConfiguration,
                NoRestore        = true,
                NoBuild          = true,
                NoLogo           = true,
                Filter           = dotNetFilter,
                ResultsDirectory = nunitOutputDirectory,
                Verbosity        = DotNetVerbosity.Minimal,
            };
            testSettings.ArgumentCustomization = altcoverSettings.Concatenate(testSettings.ArgumentCustomization);
            context.DotNetTest(context.Solution, testSettings);
        }
        else
        {
            var testDir = context.Paths.OutDirectory
                                       .Combine("CKAN.Tests")
                                       .Combine(context.BuildConfiguration)
                                       .Combine("bin")
                                       .Combine(context.BuildNetFramework);
            var instrumentedDir = testDir.GetParent()
                                         .Combine($"{context.BuildNetFramework}__Instrumented");
            var testFile = instrumentedDir.CombineWithFilePath("CKAN.Tests.dll");

            context.DotNetTest(context.Solution, new DotNetTestSettings
            {
                Configuration    = "NoGUI",
                Framework        = "net8.0",
                NoRestore        = true,
                NoBuild          = true,
                NoLogo           = true,
                Filter           = dotNetFilter,
                ResultsDirectory = nunitOutputDirectory,
                Verbosity        = DotNetVerbosity.Minimal,
            });

            // Add coverage instrumentation to our test assemblies
            context.RunAltCover("-q",
                                @"-e ""Microsoft|NUnit3|testhost|CKAN\\.Tests|IndexRange|OxyPlot""",
                                @"-t ""System|Microsoft""",
                                "-p _build",
                                "-p Tests",
                                "-p ConsoleUI",
                                "-p GUI/Dialogs",
                                "-p GUI/Controls",
                                "-p GUI/Main",
                                "-a ExcludeFromCodeCoverage",
                                $"-i {testDir}",
                                $"-o {instrumentedDir}",
                                "--save");
            // Run the tests
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
            // Transform the raw coverage data into coverage.xml and print a summary
            context.RunAltCover("runner", "--collect",
                                "--summary=O",
                                $"-r {instrumentedDir}",
                                $"-l {context.Paths.CoverageOutputDirectory.CombineWithFilePath("lcov.info")}",
                                $"-c {context.Paths.CoverageOutputDirectory.CombineWithFilePath("cobertura.xml")}");
        }
    }
}

// AltCover.cake regrettably requires defining new types rather than just filling in a struct

public class MyPrepareOptions(BuildContext context) : PrepareOptions
{
     public override TraceLevel Verbosity => TraceLevel.Info;

    public override IEnumerable<string> AssemblyExcludeFilter => [
        "Microsoft", "NUnit3", "testhost", "IndexRange", "OxyPlot",
        "CKAN\\.ConsoleUI", "CKAN\\.Tests",
    ];

    public override IEnumerable<string> TypeFilter => [
        "System", "Microsoft",
    ];

    public override IEnumerable<string> PathFilter => [
        "_build", "Tests", "ConsoleUI",
        "GUI/Dialogs", "GUI/Controls", "GUI/Main",
    ];

    public override IEnumerable<string> AttributeFilter => [
        "ExcludeFromCodeCoverage",
    ];

    public override string Report => OutputPath("coverage.xml");

    private string OutputPath(string filename)
        => context.Paths.CoverageOutputDirectory
                        .CombineWithFilePath(filename)
                        .ToString();

    private readonly BuildContext context = context;
}

public class MyCollectOptions(BuildContext context) : CollectOptions
{
    public override TraceLevel Verbosity => TraceLevel.Info;
    public override string SummaryFormat => "O";
    public override string OutputFile    => OutputPath("coverage.xml");
    public override string Cobertura     => OutputPath("cobertura.xml");
    public override string LcovReport    => OutputPath("lcov.info");

    private string OutputPath(string filename)
        => context.Paths.CoverageOutputDirectory
                        .CombineWithFilePath(filename)
                        .ToString();

    private readonly BuildContext context = context;
}

public class MyTestOptions : TestOptions
{
    public override string ShowSummary => "O";
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
