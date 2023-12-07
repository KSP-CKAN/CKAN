using System;
using System.Linq;

namespace CKAN.Versioning
{
    public sealed partial class GameVersionBound
    {
        public static readonly GameVersionBound Unbounded = new GameVersionBound();

        public GameVersion Value { get; private set; }
        public bool Inclusive { get; private set; }

        private readonly string _string;

        public GameVersionBound()
            : this(GameVersion.Any, true) { }

        public GameVersionBound(GameVersion value, bool inclusive)
        {
            if (value is null)
            {
                throw new ArgumentNullException("value");
            }

            if (!value.IsAny && !value.IsFullyDefined)
            {
                throw new ArgumentException("Version must be either fully undefined or fully defined.", "value");
            }

            Value = value;
            Inclusive = inclusive;

            // Workaround an issue in old (<=3.2.x) versions of Mono that does not correctly handle null values
            // returned from ToString().
            var valueStr = value.ToString() ?? string.Empty;

            _string = inclusive ? string.Format("[{0}]", valueStr) : string.Format("({0})", valueStr);
        }

        public GameVersion AsInclusiveLower()
            => Inclusive
                // Already inclusive? Drop all trailing 0 values
                ? (Value.IsPatchDefined && Value.Patch > 0 ? new GameVersion(Value.Major, Value.Minor, Value.Patch)
                 : Value.IsMinorDefined && Value.Minor > 0 ? new GameVersion(Value.Major, Value.Minor)
                 : Value.IsMajorDefined && Value.Major > 0 ? new GameVersion(Value.Major)
                 : GameVersion.Any)
                // 1.3.1 non-inclusive => 1.3.2 inclusive lower
                : Value.IsPatchDefined ? new GameVersion(Value.Major, Value.Minor, Value.Patch + 1)
                // 1.3 non-inclusive => 1.4 inclusive lower
                : Value.IsMinorDefined ? new GameVersion(Value.Major, Value.Minor + 1)
                // 1 non-inclusive => 2 inclusive lower
                : Value.IsMajorDefined ? new GameVersion(Value.Major + 1)
                // Unbounded, I guess?
                : GameVersion.Any;

        public GameVersion AsInclusiveUpper()
            // Already inclusive?
            => Inclusive ? Value
                // 1.3.1 non-inclusive => 1.3.0 inclusive upper
                : Value.IsPatchDefined && Value.Patch > 0 ? new GameVersion(Value.Major, Value.Minor, Value.Patch - 1)
                // 1.3 non-inclusive => 1.2 inclusive upper
                : Value.IsMinorDefined && Value.Minor > 0 ? new GameVersion(Value.Major, Value.Minor - 1)
                // 2 non-inclusive => 1 inclusive lower
                : Value.IsMajorDefined && Value.Major > 0 ? new GameVersion(Value.Major - 1)
                // Unbounded, I guess?
                : GameVersion.Any;

        public override string ToString() => _string;
    }

    public sealed partial class GameVersionBound : IEquatable<GameVersionBound>
    {
        public bool Equals(GameVersionBound other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Value, other.Value) && Inclusive == other.Inclusive;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is GameVersionBound bound && Equals(bound);
        }

        public override int GetHashCode()
            => (Value, Inclusive).GetHashCode();

        public static bool operator ==(GameVersionBound left, GameVersionBound right) => Equals(left, right);
        public static bool operator !=(GameVersionBound left, GameVersionBound right) => !Equals(left, right);
    }

    public sealed partial class GameVersionBound
    {
        /// <summary>
        /// Returns the lowest of a set of <see cref="GameVersionBound"/> objects. Analagous to
        /// <see cref="GameVersion.Min(GameVersion[])"/> but does not produce a stable sort because in the event of a
        /// tie inclusive bounds are treated as both lower and higher than equivalent exclusive bounds.
        /// </summary>
        /// <param name="versionBounds">The set of <see cref="GameVersionBound"/> objects to compare.</param>
        /// <returns>The lowest value in <see cref="versionBounds"/>.</returns>
        public static GameVersionBound Lowest(params GameVersionBound[] versionBounds)
        {
            if (versionBounds == null)
            {
                throw new ArgumentNullException("versionBounds");
            }

            if (!versionBounds.Any())
            {
                throw new ArgumentException("Value cannot be empty.", "versionBounds");
            }

            if (versionBounds.Any(i => i == null))
            {
                throw new ArgumentException("Value cannot contain null.", "versionBounds");
            }

            return versionBounds
                .OrderBy(i => i == Unbounded)
                .ThenBy(i => i.Value)
                .ThenBy(i => i.Inclusive)
                .First();
        }

        /// <summary>
        /// Returns the highest of a set of <see cref="GameVersionBound"/> objects. Analagous to
        /// <see cref="GameVersion.Max(GameVersion[])"/> but does not produce a stable sort because in the event of a
        /// tie inclusive bounds are treated as both lower and higher than equivalent exclusive bounds.
        /// </summary>
        /// <param name="versionBounds">The set of <see cref="GameVersionBound"/> objects to compare.</param>
        /// <returns>The highest value in <see cref="versionBounds"/>.</returns>
        public static GameVersionBound Highest(params GameVersionBound[] versionBounds)
        {
            if (versionBounds == null)
            {
                throw new ArgumentNullException("versionBounds");
            }

            if (!versionBounds.Any())
            {
                throw new ArgumentException("Value cannot be empty.", "versionBounds");
            }

            if (versionBounds.Any(i => i == null))
            {
                throw new ArgumentException("Value cannot contain null.", "versionBounds");
            }

            return versionBounds
                .OrderBy(i => i == Unbounded)
                .ThenByDescending(i => i.Value)
                .ThenBy(i => i.Inclusive)
                .First();
        }
    }
}
