using System;
using Autofac;
using CQRSlite.Config;

namespace Televic.Mycro.Bus
{
    public class DependencyResolver : IServiceLocator
    {
        private readonly IContainer _container;

        public DependencyResolver(IContainer container)
        {
            _container = container;
        }

        public object GetService(Type type)
        {
            return _container.Resolve(type);
        }

        public T GetService<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
