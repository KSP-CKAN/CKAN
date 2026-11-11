using System.Collections.Generic;
using Cake.Core.IO;

namespace Build;

/// <summary>
/// The paths of one executable as built for each of the <see cref="BuildFrameworks"/>.
/// </summary>
public class ExecutablePaths
{
    /// <param name="directory">The directory the executables are built into</param>
    /// <param name="stem">The base file name of the executable</param>
    /// <param name="isWindows">Whether the native version of the executable is for Windows</param>
    public ExecutablePaths(DirectoryPath directory, string stem, bool isWindows)
    {
        NetFrameworkPath = directory.CombineWithFilePath($"{stem}.exe");
        // On Windows the native build needs a different name from the net481 one
        // so we don't end up with two files called e.g. "ckan.exe" that end up uploaded to
        // release assets.
        NativePath = directory.CombineWithFilePath(isWindows ? $"{stem}-windows.exe" : stem);
    }

    /// <summary>
    /// The .NET Framework executable, such as <c>ckan.exe</c>
    /// </summary>
    public FilePath NetFrameworkPath { get; }

    /// <summary>
    /// The self-contained executable for the host,
    /// such as <c>ckan-windows.exe</c> on Windows or <c>ckan</c> elsewhere
    /// </summary>
    public FilePath NativePath { get; }

    /// <summary>
    /// The paths built for the given frameworks
    /// </summary>
    public IEnumerable<FilePath> AllFor(BuildFrameworks frameworks)
    {
        if (frameworks.IncludesNetFramework())
        {
            yield return NetFrameworkPath;
        }

        if (frameworks.IncludesDotNet())
        {
            yield return NativePath;
        }
    }
}