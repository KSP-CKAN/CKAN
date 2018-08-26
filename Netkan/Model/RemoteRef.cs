using System.Text.RegularExpressions;

namespace CKAN.NetKAN.Model
{
    internal class RemoteRef
    {
        private static readonly Regex Pattern = new Regex(
            @"^#/ckan/(?<source>[^/]+)(?:/(?<id>.+))?",
            RegexOptions.Compiled
        );

        private readonly string _string;

        public string Source { get; private set; }
        public string Id { get; private set; }

        public RemoteRef(string remoteRefToken)
            : this(ParseArguments(remoteRefToken)) { }

        public RemoteRef(RemoteRef remoteRef)
            : this(new Arguments(remoteRef.Source, remoteRef.Id)) { }

        private RemoteRef(Arguments arguments)
        {
            Source = arguments.Source;
            Id = arguments.Id;

            _string = Id == null
                ? $"#/ckan/{Source}"
                : $"#/ckan/{Source}/{Id}";
        }

        public override string ToString()
        {
            return _string;
        }

        private static Arguments ParseArguments(string refToken)
        {
            var match = Pattern.Match(refToken);

            if (match.Success)
            {
                return new Arguments(match.Groups["source"].Value, match.Groups["id"].Value);
            }
            else
            {
                throw new Kraken(string.Format(@"Could not parse reference: ""{0}""", refToken));
            }
        }

        private sealed class Arguments
        {
            public string Source { get; private set; }
            public string Id { get; private set; }

            public Arguments(string source, string id)
            {
                Source = source;
                Id = id;
            }
        }
    }
}
