using System;
using System.Collections.Generic;

using Cake.Common;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;

namespace Build;

[TaskName("osx")]
[TaskDescription("Build the macOS(OSX) dmg package.")]
[IsDependentOn(typeof(CkanTask))]
public sealed class OsxTask() : MakeTask("macosx")
{
    public override void Run(BuildContext context)
    {
        // Publish Cmdline for Mac arm64
        context.DotNetPublish(context.Paths.CmdlineProject.FullPath, new DotNetPublishSettings
        {
            Configuration  = context.BuildConfiguration,
            Framework      = "net8.0",
            Runtime        = "osx-arm64",
            SelfContained  = true,
        });
        // Publish Cmdline for Mac x64
        context.DotNetPublish(context.Paths.CmdlineProject.FullPath, new DotNetPublishSettings
        {
            Configuration  = context.BuildConfiguration,
            Framework      = "net8.0",
            Runtime        = "osx-x64",
            SelfContained  = true,
        });
        base.Run(context);
    }
}

[TaskName("osx-clean")]
[TaskDescription("Clean the output directory of the macOS(OSX) package.")]
public sealed class OsxCleanTask() : MakeTask("macosx", "clean");

[TaskName("deb")]
[TaskDescription("Build the deb package for Debian-based distros.")]
[IsDependentOn(typeof(CkanTask))]
public sealed class DebTask() : MakeTask("debian");

[TaskName("deb-sign")]
[TaskDescription("Build the deb package for Debian-based distros.")]
[IsDependentOn(typeof(DebTask))]
public sealed class DebSignTask() : MakeTask("debian", "sign");

[TaskName("deb-test")]
[TaskDescription("Test the deb packaging.")]
[IsDependentOn(typeof(DebTask))]
public sealed class DebTestTask() : MakeTask("debian", "test");

[TaskName("deb-clean")]
[TaskDescription("Clean the deb output directory.")]
public sealed class DebCleanTask() : MakeTask("debian", "clean");

[TaskName("rpm")]
[TaskDescription("Build the rpm package for RPM-based distros.")]
[IsDependentOn(typeof(CkanTask))]
public sealed class RpmTask() : MakeTask("rpm");

[TaskName("rpm-repo")]
[TaskDescription("Build the rpm repository for RPM-based distros.")]
[IsDependentOn(typeof(CkanTask))]
public sealed class RpmRepoTask() : MakeTask("rpm", "repo");

[TaskName("rpm-test")]
[TaskDescription("Test the rpm packaging.")]
public sealed class RpmTestTask() : MakeTask("rpm", "test");

[TaskName("rpm-clean")]
[TaskDescription("Clean the rpm package output directory.")]
public sealed class RpmCleanTask() : MakeTask("rpm", "clean");

public abstract class MakeTask(string location, ProcessArgumentBuilder? args = null) : FrostingTask<BuildContext>
{
    private string Location { get; } = location;
    private ProcessArgumentBuilder Args { get; } = args ?? "";

    public override void Run(BuildContext context)
    {
        var exitCode = context.StartProcess("make", new ProcessSettings() {
            WorkingDirectory = context.Paths.RootDirectory.Combine(Location),
            Arguments = Args,
            EnvironmentVariables = new Dictionary<string, string?>
            {
                { "CONFIGURATION", context.BuildConfiguration },
            }
        });
        if (exitCode != 0)
        {
            throw new Exception("Make failed with exit code: " + exitCode);
        }
    }
}
