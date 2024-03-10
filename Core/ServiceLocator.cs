using Autofac;

using CKAN.Games.KerbalSpaceProgram.GameVersionProviders;
using CKAN.Configuration;

namespace CKAN
{
    /// <summary>
    /// This class exists as a really obvious place for our service locator (ie: Autofac container)
    /// to live.
    /// </summary>
    public static class ServiceLocator
    {
        private static IContainer _container;
        public static IContainer Container
        {
            // NB: Totally not thread-safe.
            get
            {
                if (_container == null)
                {
                    Init();
                }

                return _container;
            }

            #pragma warning disable IDE0027
            set
            {
                _container = value;
            }
            #pragma warning restore IDE0027
        }

        private static void Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<StrictGameComparator>()
                .As<IGameComparator>();

            builder.RegisterType<JsonConfiguration>()
                .As<IConfiguration>()
                // Technically not needed, but makes things easier
                .SingleInstance();

            builder.RegisterType<KspBuildMap>()
                .As<IKspBuildMap>()
                // Since it stores cached data we want to keep it around
                .SingleInstance();

            builder.RegisterType<KspBuildIdVersionProvider>()
                .As<IGameVersionProvider>()
                .Keyed<IGameVersionProvider>(GameVersionSource.BuildId);

            builder.RegisterType<KspReadmeVersionProvider>()
                .As<IGameVersionProvider>()
                .Keyed<IGameVersionProvider>(GameVersionSource.Readme);

            builder.RegisterType<RepositoryDataManager>()
                   .SingleInstance();

            _container = builder.Build();
        }
    }
}
