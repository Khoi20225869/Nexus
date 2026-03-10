using System;
using System.Collections.Generic;

namespace Game.Core.Services
{
    public interface IServiceRegistry
    {
        void Register<TService>(TService service) where TService : class;
        TService Resolve<TService>() where TService : class;
    }

    public sealed class ServiceRegistry : IServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<TService>(TService service) where TService : class
        {
            _services[typeof(TService)] = service;
        }

        public TService Resolve<TService>() where TService : class
        {
            if (_services.TryGetValue(typeof(TService), out var service))
            {
                return service as TService;
            }

            throw new InvalidOperationException($"Service not found: {typeof(TService).Name}");
        }
    }
}
