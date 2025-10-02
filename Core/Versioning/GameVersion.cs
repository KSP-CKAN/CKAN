using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;

using CKAN.Games;

namespace CKAN.Versioning
{
    /// <summary>
    /// Represents the version number of a Kerbal Space Program (KSP) installation.
    /// </summary>
    [JsonConverter(typeof(GameVersionJsonConverter))]
    public sealed partial class GameVersion
    {
        private static readonly Regex Pattern = new Regex(
            @"^(?<major>\d+)(?:\.(?<minor>\d+)(?:\.(?<patch>\d+)(?:\.(?<build>\d+))?)?)?$",
            RegexOptions.Compiled);

        private const int Undefined = -1;

        public static readonly GameVersion Any = new GameVersion();

        private readonly int _major;
        private readonly int _minor;
        private readonly int _patch;
        private readonly int _build;

        private readonly string? _string;

        /// <summary>
        /// Gets the value of the major component of the version number for the current <see cref="GameVersion"/>
        /// object.
        /// </summary>
        public int Major => _major;

        /// <summary>
        /// Gets the value of the minor component of the version number for the current <see cref="GameVersion"/>
        /// object.
        /// </summary>
        public int Minor => _minor;

        /// <summary>
        /// Gets the value of the patch component of the version number for the current <see cref="GameVersion"/>
        /// object.
        /// </summary>
        public int Patch => _patch;

        /// <summary>
        /// Gets the value of the build component of the version number for the current <see cref="GameVersion"/>
        /// object.
        /// </summary>
        public int Build => _build;

        /// <summary>
        /// Gets whether or not the major component of the version number for the current <see cref="GameVersion"/>
        /// object is defined.
        /// </summary>
        public bool IsMajorDefined => _major != Undefined;

        /// <summary>
        /// Gets whether or not the minor component of the version number for the current <see cref="GameVersion"/>
        /// object is defined.
        /// </summary>
        public bool IsMinorDefined => _minor != Undefined;

        /// <summary>
        /// Gets whether or not the patch component of the version number for the current <see cref="GameVersion"/>
        /// object is defined.
        /// </summary>
        public bool IsPatchDefined => _patch != Undefined;

        /// <summary>
        /// Gets whether or not the build component of the version number for the current <see cref="GameVersion"/>
        /// object is defined.
        /// </summary>
        public bool IsBuildDefined => _build != Undefined;

        /// <summary>
        /// Indicates whether or not all components of the current <see cref="GameVersion"/> are defined.
        /// </summary>
        public bool IsFullyDefined => IsMajorDefined && IsMinorDefined && IsPatchDefined && IsBuildDefined;

        /// <summary>
        /// Indicates wheter or not all the components of the current <see cref="GameVersion"/> are undefined.
        /// </summary>
        public bool IsAny => !IsMajorDefined && !IsMinorDefined && !IsPatchDefined && !IsBuildDefined;

        /// <summary>
        /// Provide this resource string to other DLLs outside core
        /// /// </summary>
        public static string AnyString => Properties.Resources.GameVersionYalovAny;

        /// <summary>
        /// Check whether a version is null or Any.
        /// We group them here because they mean the same thing.
        /// </summary>
        /// <param name="v">The version to check</param>
        /// <returns>
        /// True if null or Any, false otherwise
        /// </returns>

        public static bool IsNullOrAny([NotNullWhen(false)] GameVersion? v) => v == null || v.IsAny;

        /// <summary>
        /// Initialize a new instance of the <see cref="GameVersion"/> class with all components unspecified.
        /// </summary>
        public GameVersion()
        {
            _major = Undefined;
            _minor = Undefined;
            _patch = Undefined;
            _build = Undefined;

            _string = DeriveString(_major, _minor, _patch, _build);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="GameVersion"/> class using the specified major value.
        /// </summary>
        /// <param name="major">The major version number.</param>
        public GameVersion(int major)
        {
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major), major.ToString());
            }

            _major = major;
            _minor = Undefined;
            _patch = Undefined;
            _build = Undefined;

            _string = DeriveString(_major, _minor, _patch, _build);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="GameVersion"/> class using the specified major and minor
        /// values.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        public GameVersion(int major, int minor)
        {
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major), major.ToString());
            }

            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minor), minor.ToString());
            }

            _major = major;
            _minor = minor;
            _patch = Undefined;
            _build = Undefined;

            _string = DeriveString(_major, _minor, _patch, _build);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="GameVersion"/> class using the specified major, minor, and
        /// patch values.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        public GameVersion(int major, int minor, int patch)
        {
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major), major.ToString());
            }

            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minor), minor.ToString());
            }

            if (patch < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(patch), patch.ToString());
            }

            _major = major;
            _minor = minor;
            _patch = patch;
            _build = Undefined;

            _string = DeriveString(_major, _minor, _patch, _build);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="GameVersion"/> class using the specified major, minor, patch,
        /// and build values.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        /// <param name="build">The build verison number.</param>
        public GameVersion(int major, int minor, int patch, int build)
        {
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major), major, $"{major}");
            }

            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minor), minor, $"{minor}");
            }

            if (patch < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(patch), patch, $"{patch}");
            }

            if (build < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(build), build, $"{build}");
            }

            _major = major;
            _minor = minor;
            _patch = patch;
            _build = build;

            _string = DeriveString(_major, _minor, _patch, _build);
        }

        /// <summary>
        /// Converts the value of the current <see cref="GameVersion"/> to its equivalent <see cref="string"/>
        /// representation.
        /// </summary>
        /// <returns>
        /// <para>
        /// The <see cref="string"/> representation of the values of the major, minor, patch, and build components of
        /// the current <see cref="GameVersion"/> object as depicted in the following format. Each component is
        /// separated by a period character ('.'). Square brackets ('[' and ']') indicate a component that will not
        /// appear in the return value if the component is not defined:
        /// </para>
        /// <para>
        /// [<i>major</i>[.<i>minor</i>[.<i>patch</i>[.<i>build</i>]]]]
        /// </para>
        /// <para>
        /// For example, if you create a <see cref="GameVersion"/> object using the constructor <c>GameVersion(1,1)</c>,
        /// the returned string is "1.1". If you create a <see cref="GameVersion"/> using the constructor (1,3,4,2),
        /// the returned string is "1.3.4.2".
        /// </para>
        /// <para>
        /// If the current <see cref="GameVersion"/> is totally undefined the return value will be <c>null</c>.
        /// </para>
        /// </returns>
        public override string? ToString() => _string;

        /// <summary>
        /// Strip off the build number if it's defined
        /// </summary>
        /// <returns>A GameVersion equal to this but without a build number</returns>
        public GameVersion WithoutBuild => IsBuildDefined ? new GameVersion(_major, _minor, _patch)
                                                          : this;

        /// <summary>
        /// Converts the value of the current <see cref="GameVersion"/> to its equivalent
        /// <see cref="GameVersionRange"/>.
        /// </summary>
        /// <returns>
        /// <para>
        /// A <see cref="GameVersionRange"/> which specifies a set of versions equivalent to the current
        /// <see cref="GameVersion"/>.
        /// </para>
        /// <para>
        /// For example, the version "1.0.0.0" would be equivalent to the range ["1.0.0.0", "1.0.0.0"], while the
        /// version "1.0" would be equivalent to the range ["1.0.0.0", "1.1.0.0"). Where '[' and ']' represent
        /// inclusive bounds and '(' and ')' represent exclusive bounds.
        /// </para>
        /// </returns>
        public GameVersionRange ToVersionRange()
        {
            GameVersionBound lower;
            GameVersionBound upper;

            if (IsBuildDefined)
            {
                lower = new GameVersionBound(this, inclusive: true);
                upper = new GameVersionBound(this, inclusive: true);
            }
            else if (IsPatchDefined)
            {
                lower = new GameVersionBound(new GameVersion(Major, Minor, Patch, 0), inclusive: true);
                upper = new GameVersionBound(new GameVersion(Major, Minor, Patch + 1, 0), inclusive: false);
            }
            else if (IsMinorDefined)
            {
                lower = new GameVersionBound(new GameVersion(Major, Minor, 0, 0), inclusive: true);
                upper = new GameVersionBound(new GameVersion(Major, Minor + 1, 0, 0), inclusive: false);
            }
            else if (IsMajorDefined)
            {
                lower = new GameVersionBound(new GameVersion(Major, 0, 0, 0), inclusive: true);
                upper = new GameVersionBound(new GameVersion(Major + 1, 0, 0, 0), inclusive: false);
            }
            else
            {
                lower = GameVersionBound.Unbounded;
                upper = GameVersionBound.Unbounded;
            }

            return new GameVersionRange(lower, upper);
        }

        /// <summary>
        /// Converts the string representation of a version number to an equivalent <see cref="GameVersion"/> object.
        /// </summary>
        /// <param name="input">A string that contains a version number to convert.</param>
        /// <returns>
        /// A <see cref="GameVersion"/> object that is equivalent to the version number specified in the
        /// input parameter.
        /// </returns>
        public static GameVersion Parse(string input)
        {
            if (TryParse(input, out GameVersion? result) && result is not null)
            {
                return result;
            }
            else
            {
                throw new FormatException();
            }
        }

        /// <summary>
        /// Tries to convert the string representation of a version number to an equivalent <see cref="GameVersion"/>
        /// object and returns a value that indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">
        /// A string that contains a version number to convert.
        /// </param>
        /// <param name="result">
        /// When this method returns <c>true</c>, contains the <see cref="GameVersion"/> equivalent of the number that
        /// is contained in input. When this method returns <c>false</c>, the value is unspecified.
        /// </param>
        /// <returns>
        /// <c>true</c> if the input parameter was converted successfully; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryParse(string? input,
                                    [NotNullWhen(returnValue: true)] out GameVersion? result)
        {
            result = null;

            if (input is null)
            {
                return false;
            }

            if (input == "any")
            {
                result = Any;
                return true;
            }

            var major = Undefined;
            var minor = Undefined;
            var patch = Undefined;
            var build = Undefined;

            var match = Pattern.Match(input.Trim());

            if (match.Success)
            {
                var majorGroup = match.Groups["major"];
                var minorGroup = match.Groups["minor"];
                var patchGroup = match.Groups["patch"];
                var buildGroup = match.Groups["build"];

                if (majorGroup.Success)
                {
                    if (!int.TryParse(majorGroup.Value, out major))
                    {
                        return false;
                    }

                    if (major is < 0 or int.MaxValue)
                    {
                        major = Undefined;
                    }
                }

                if (minorGroup.Success)
                {
                    if (!int.TryParse(minorGroup.Value, out minor))
                    {
                        return false;
                    }

                    if (minor is < 0 or int.MaxValue)
                    {
                        minor = Undefined;
                    }
                }

                if (patchGroup.Success)
                {
                    if (!int.TryParse(patchGroup.Value, out patch))
                    {
                        return false;
                    }

                    if (patch is < 0 or int.MaxValue)
                    {
                        patch = Undefined;
                    }
                }

                if (buildGroup.Success)
                {
                    if (!int.TryParse(buildGroup.Value, out build))
                    {
                        return false;
                    }

                    if (build is < 0 or int.MaxValue)
                    {
                        build = Undefined;
                    }
                }

                if (minor == Undefined)
                {
                    result = new GameVersion(major);
                }
                else if (patch == Undefined)
                {
                    result = new GameVersion(major, minor);
                }
                else if (build == Undefined)
                {
                    result = new GameVersion(major, minor, patch);
                }
                else
                {
                    result = new GameVersion(major, minor, patch, build);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Searches the build map if the version is a valid, known KSP version.
        /// </summary>
        /// <returns><c>true</c>, if version is in the build map, <c>false</c> otherwise.</returns>
        public bool InBuildMap(IGame game)
        {
            List<GameVersion> knownVersions = game.KnownVersions;

            foreach (GameVersion ver in knownVersions)
            {
                if (ver.Major == Major && ver.Minor == Minor && ver.Patch == Patch)
                {
                    // If it found a matching maj, min and patch,
                    // test if the build numbers are the same too, but ignore if the
                    // version is NOT build defined.
                    if (ver.Build == Build || !IsBuildDefined)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Raises a selection dialog for choosing a specific KSP version, if it is not fully defined yet.
        /// If a build number is specified but not known, it presents a list of all builds
        /// of the patch range.
        /// Needs at least a Major and Minor (doesn't make sense else).
        /// </summary>
        /// <returns>A complete GameVersion object</returns>
        /// <param name="game">The game whose versions are to be selected</param>
        /// <param name="user">A IUser instance, to raise the corresponding dialog</param>
        public GameVersion RaiseVersionSelectionDialog(IGame game, IUser? user)
        {
            if (IsFullyDefined && InBuildMap(game))
            {
                // The specified version is complete and known :hooray:. Return this instance.
                return this;
            }
            else if (!IsMajorDefined || !IsMinorDefined)
            {
                throw new BadGameVersionKraken(Properties.Resources.GameVersionSelectNeedOne);
            }
            else
            {
                // Get all known versions out of the build map.
                var knownVersions = game.KnownVersions;
                List<GameVersion> possibleVersions = new List<GameVersion>();

                // Default message passed to RaiseSelectionDialog.
                string message = Properties.Resources.GameVersionSelectHeader;

                // Find the versions which are part of the range.
                foreach (GameVersion ver in knownVersions)
                {
                    // If we only have Major and Minor -> compare these two.
                    if (!IsPatchDefined)
                    {
                        if (Major == ver.Major && Minor == ver.Minor)
                        {
                            possibleVersions.Add(ver);
                        }
                    }
                    // If we also have Patch -> compare it too.
                    else if (!IsBuildDefined)
                    {
                        if (Major == ver.Major && Minor == ver.Minor && Patch == ver.Patch)
                        {
                            possibleVersions.Add(ver);
                        }
                    }
                    // And if we are here, there's a build number not known in the build map.
                    // Only compare Major, Minor, Patch and adjust the message.
                    else
                    {
                        message = Properties.Resources.GameVersionSelectBuildHeader;
                        if (Major == ver.Major && Minor == ver.Minor && Patch == ver.Patch)
                        {
                            possibleVersions.Add(ver);
                        }
                    }
                }

                // Now do some checks and raise the selection dialog.
                if (possibleVersions.Count == 0)
                {
                    // No version found in the map. Happens for future or other unknown versions.
                    throw new BadGameVersionKraken(Properties.Resources.GameVersionNotKnown);
                }
                else if (possibleVersions.Count == 1)
                {
                    // Lucky, there's only one possible version. Happens f.e. if there's only one build per patch (especially the case for newer versions).
                    return possibleVersions.ElementAt(0);
                }
                else if (user == null || user.Headless)
                {
                    return possibleVersions.Last();
                }
                else
                {
                    int choosen = user.RaiseSelectionDialog(message, possibleVersions.ToArray());
                    if (choosen >= 0 && choosen < possibleVersions.Count)
                    {
                        return possibleVersions.ElementAt(choosen);
                    }
                    else
                    {
                        throw new CancelledActionKraken();
                    }
                }
            }
        }

        private static string? DeriveString(int major, int minor, int patch, int build)
        {
            var sb = new StringBuilder();

            if (major != Undefined)
            {
                sb.Append(major);
            }

            if (minor != Undefined)
            {
                sb.Append(".");
                sb.Append(minor);
            }

            if (patch != Undefined)
            {
                sb.Append(".");
                sb.Append(patch);
            }

            if (build != Undefined)
            {
                sb.Append(".");
                sb.Append(build);
            }

            var s = sb.ToString();

            return s.Equals("") ? null : s;
        }

        /// <summary>
        /// Update the game versions of a module.
        /// Final range will be the union of the previous and new ranges.
        /// Note that this means we always increase, never decrease, compatibility.
        /// </summary>
        /// <param name="json">The module being inflated</param>
        /// <param name="ver">The single game version</param>
        /// <param name="minVer">The minimum game version</param>
        /// <param name="maxVer">The maximum game version</param>
        public static void SetJsonCompatibility(JObject      json,
                                                GameVersion? ver,
                                                GameVersion? minVer,
                                                GameVersion? maxVer)
        {
            // Get the minimum and maximum game versions that already exist in the metadata.
            // Use specific game version if min/max don't exist.
            var existingMinStr = json.Value<string>("ksp_version_min") ?? json.Value<string>("ksp_version");
            var existingMaxStr = json.Value<string>("ksp_version_max") ?? json.Value<string>("ksp_version");

            var existingMin = existingMinStr == null ? null : Parse(existingMinStr);
            var existingMax = existingMaxStr == null ? null : Parse(existingMaxStr);

            GameVersion? avcMin, avcMax;
            if (minVer == null && maxVer == null)
            {
                // Use specific game version if min/max don't exist
                avcMin = avcMax = ver;
            }
            else
            {
                avcMin = minVer;
                avcMax = maxVer;
            }

            // Now calculate the minimum and maximum KSP versions between both the existing metadata and the
            // AVC file.
            var gameVerMins  = new List<GameVersion?>();
            var gameVerMaxes = new List<GameVersion?>();

            if (!IsNullOrAny(existingMin))
            {
                gameVerMins.Add(existingMin);
            }

            if (!IsNullOrAny(avcMin))
            {
                gameVerMins.Add(avcMin);
            }

            if (!IsNullOrAny(existingMax))
            {
                gameVerMaxes.Add(existingMax);
            }

            if (!IsNullOrAny(avcMax))
            {
                gameVerMaxes.Add(avcMax);
            }

            var gameVerMin = gameVerMins.DefaultIfEmpty(null).Min();
            var gameVerMax = gameVerMaxes.DefaultIfEmpty(null).Max();

            if (gameVerMin != null || gameVerMax != null)
            {
                // If we have either a minimum or maximum game version, remove all existing game version
                // information from the metadata.
                json.Remove("ksp_version");
                json.Remove("ksp_version_min");
                json.Remove("ksp_version_max");

                if (gameVerMin != null && gameVerMax != null)
                {
                    // If we have both a minimum and maximum game version...
                    if (gameVerMin.Equals(gameVerMax))
                    {
                        // ...and they are equal, then just set ksp_version
                        log.DebugFormat("Min and max game versions are same, setting ksp_version");
                        json["ksp_version"] = gameVerMin.ToString();
                    }
                    else
                    {
                        // ...otherwise set both ksp_version_min and ksp_version_max
                        log.DebugFormat("Min and max game versions are different, setting both");
                        json["ksp_version_min"] = gameVerMin.ToString();
                        json["ksp_version_max"] = gameVerMax.ToString();
                    }
                }
                else
                {
                    // If we have only one or the other then set which ever is applicable
                    if (gameVerMin != null)
                    {
                        log.DebugFormat("Only min game version is set");
                        json["ksp_version_min"] = gameVerMin.ToString();
                    }
                    if (gameVerMax != null)
                    {
                        log.DebugFormat("Only max game version is set");
                        json["ksp_version_max"] = gameVerMax.ToString();
                    }
                }
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(GameVersion));
    }

    public sealed partial class GameVersion : IEquatable<GameVersion>
    {
        /// <summary>
        /// Returns a value indicating whether the current <see cref="GameVersion"/> object and specified
        /// <see cref="GameVersion"/> object represent the same value.
        /// </summary>
        /// <param name="obj">
        /// A <see cref="GameVersion"/> object to compare to the current <see cref="GameVersion"/> object, or
        /// <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if every component of the current <see cref="GameVersion"/> matches the corresponding component
        /// of the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(GameVersion? obj)
            => obj is not null
                && (ReferenceEquals(obj, this)
                    || (_major == obj._major
                        && _minor == obj._minor
                        && _patch == obj._patch
                        && _build == obj._build));

        /// <summary>
        /// Returns a value indicating whether the current <see cref="GameVersion"/> object is equal to a specified
        /// object.
        /// </summary>
        /// <param name="obj">
        /// An object to compare with the current <see cref="GameVersion"/> object, or <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current <see cref="GameVersion"/> object and obj are both
        /// <see cref="GameVersion"/> objects and every component of the current <see cref="GameVersion"/> object
        /// matches the corresponding component of obj; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
            => obj is not null
                && (ReferenceEquals(obj, this)
                    || (obj is GameVersion gv && Equals(gv)));

        /// <summary>
        /// Returns a hash code for the current <see cref="GameVersion"/> object.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        #if NET5_0_OR_GREATER
            => HashCode.Combine(_major, _minor, _patch, _build);
        #else
            => (_major, _minor, _patch, _build).GetHashCode();
        #endif

        /// <summary>
        /// Determines whether two specified <see cref="GameVersion"/> objects are equal.
        /// </summary>
        /// <param name="v1">The first <see cref="GameVersion"/> object.</param>
        /// <param name="v2">The second <see cref="GameVersion"/> object.</param>
        /// <returns><c>true</c> if v1 equals v2; otherwise, <c>false</c>.</returns>
        public static bool operator ==(GameVersion? v1, GameVersion? v2)
            => Equals(v1, v2);

        /// <summary>
        /// Determines whether two specified <see cref="GameVersion"/> objects are not equal.
        /// </summary>
        /// <param name="v1">The first <see cref="GameVersion"/> object.</param>
        /// <param name="v2">The second <see cref="GameVersion"/> object.</param>
        /// <returns>
        /// <c>true</c> if v1 does not equal v2; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(GameVersion? v1, GameVersion? v2)
            => !Equals(v1, v2);
    }

    public sealed partial class GameVersion : IComparable, IComparable<GameVersion>
    {
        /// <summary>
        /// Compares the current <see cref="GameVersion"/> object to a specified object and returns an indication of
        /// their relative values.
        /// </summary>
        /// <param name="obj">An object to compare, or <c>null</c>.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of the two objects, as shown in the following table.
        /// <list type="table">
        /// <listheader>
        /// <term>Return value</term>
        /// <description>Meaning</description>
        /// </listheader>
        /// <item>
        /// <term>Less than zero</term>
        /// <description>
        /// The current <see cref="GameVersion"/> object is a version before obj.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>
        /// The current <see cref="GameVersion"/> object is the same version as obj.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>
        /// <para>
        /// The current <see cref="GameVersion"/> object is a version subsequent to obj.
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(object? obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var objGameVersion = obj as GameVersion;

            if (objGameVersion != null)
            {
                return CompareTo(objGameVersion);
            }
            else
            {
                throw new ArgumentException("Object must be of type GameVersion.");
            }
        }

        /// <summary>
        /// Compares the current <see cref="GameVersion"/> object to a specified object and returns an indication of
        /// their relative values.
        /// </summary>
        /// <param name="other">An object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of the two objects, as shown in the following table.
        /// <list type="table">
        /// <listheader>
        /// <term>Return value</term>
        /// <description>Meaning</description>
        /// </listheader>
        /// <item>
        /// <term>Less than zero</term>
        /// <description>
        /// The current <see cref="GameVersion"/> object is a version before other.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>
        /// The current <see cref="GameVersion"/> object is the same version as other.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>
        /// <para>
        /// The current <see cref="GameVersion"/> object is a version subsequent to other.
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(GameVersion? other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (Equals(this, other))
            {
                return 0;
            }

            var majorCompare = _major.CompareTo(other._major);

            if (majorCompare == 0)
            {
                var minorCompare = _minor.CompareTo(other._minor);

                if (minorCompare == 0)
                {
                    var patchCompare = _patch.CompareTo(other._patch);

                    return patchCompare == 0 ? _build.CompareTo(other._build) : patchCompare;
                }
                else
                {
                    return minorCompare;
                }
            }
            else
            {
                return majorCompare;
            }
        }

        /// <summary>
        /// Determines whether the first specified <see cref="GameVersion"/> object is less than the second specified
        /// <see cref="GameVersion"/> object.
        /// </summary>
        /// <param name="left">The first <see cref="GameVersion"/> object.</param>
        /// <param name="right">The second <see cref="GameVersion"/> object.</param>
        /// <returns>
        /// <c>true</c> if left is less than right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator <(GameVersion left, GameVersion right)
            => left.CompareTo(right) < 0;

        /// <summary>
        /// Determines whether the first specified <see cref="GameVersion"/> object is greater than the second
        /// specified <see cref="ModuleVersion"/> object.
        /// </summary>
        /// <param name="left">The first <see cref="GameVersion"/> object.</param>
        /// <param name="right">The second <see cref="GameVersion"/> object.</param>
        /// <returns>
        /// <c>true</c> if left is greater than right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator >(GameVersion left, GameVersion right)
            => left.CompareTo(right) > 0;

        /// <summary>
        /// Determines whether the first specified <see cref="GameVersion"/> object is less than or equal to the second
        /// specified <see cref="GameVersion"/> object.
        /// </summary>
        /// <param name="left">The first <see cref="GameVersion"/> object.</param>
        /// <param name="right">The second <see cref="GameVersion"/> object.</param>
        /// <returns>
        /// <c>true</c> if left is less than or equal to right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator <=(GameVersion left, GameVersion right)
            => left.CompareTo(right) <= 0;

        /// <summary>
        /// Determines whether the first specified <see cref="GameVersion"/> object is greater than or equal to the
        /// second specified <see cref="GameVersion"/> object.
        /// </summary>
        /// <param name="left">The first <see cref="GameVersion"/> object.</param>
        /// <param name="right">The second <see cref="GameVersion"/> object.</param>
        /// <returns>
        /// <c>true</c> if left is greater than or equal to right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator >=(GameVersion left, GameVersion right)
            => left.CompareTo(right) >= 0;
    }

    public sealed class GameVersionJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString();

            switch (value)
            {
                case null:
                    return null;

                default:
                    // For a little while, AVC files which didn't specify a full three-part
                    // version number could result in versions like `1.1.`, which cause our
                    // code to fail. Here we strip any trailing dot from the version number,
                    // which makes them valid again before parsing. CKAN#1780

                    value = Regex.Replace(value, @"\.$", "");

                    if (GameVersion.TryParse(value, out GameVersion? result))
                    {
                        return result;
                    }
                    else
                    {
                        throw new JsonException(string.Format("Could not parse game version: {0}", value));
                    }
            }
        }

        public override bool CanConvert(Type objectType)
            => objectType == typeof(GameVersion);
    }
}
