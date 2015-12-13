using Autofac;

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

            set
            {
                _container = value;
            }
        }

        private static void Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<GrasGameComparator>().As<IGameComparator>();

            _container = builder.Build();
        }
    }
}
