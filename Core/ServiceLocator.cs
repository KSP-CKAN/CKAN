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
        public static IContainer Container
            // NB: Totally not thread-safe.
            => _container ??= Init();

        private static IContainer? _container;

        private static IContainer Init()
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

            return builder.Build();
        }
    }
}
