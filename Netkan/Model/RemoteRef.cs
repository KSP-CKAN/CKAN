using System.Text.RegularExpressions;

namespace CKAN.NetKAN.Model
{
    internal class RemoteRef
    {
         public RemoteRef(string remoteRefToken)
        {
            if (Pattern.Match(remoteRefToken) is Match { Success: true } match)
            {
                Source  = match.Groups["source"].Value;
                Id      = match.Groups["id"].Value;
                _string = remoteRefToken;
            }
            else
            {
                throw new Kraken(string.Format(@"Could not parse reference: ""{0}""",
                                               remoteRefToken));
            }
        }

        public RemoteRef(RemoteRef remoteRef)
        {
            Source  = remoteRef.Source;
            Id      = remoteRef.Id;
            _string = remoteRef.ToString();
        }

        public override string ToString()
            => _string;

        public static explicit operator RemoteRef(string rref)
        {
            return new RemoteRef(rref);
        }

        public string  Source { get; private set; }
        public string? Id     { get; private set; }

        private static readonly Regex Pattern = new Regex(
            @"^#/ckan/(?<source>[^/]+)(?:/(?<id>.+))?",
            RegexOptions.Compiled);

        private readonly string _string;
    }
}
