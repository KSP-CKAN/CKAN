using System;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Sources.Jenkins
{
    internal class JenkinsRef : RemoteRef
    {
        public JenkinsRef(string remoteRefToken)
            : this(new RemoteRef(remoteRefToken)) { }

        public JenkinsRef(RemoteRef remoteRef)
            : base(remoteRef)
        {
            BaseUri = new Uri(remoteRef.Id);
        }

        public readonly Uri BaseUri;
    }
}
