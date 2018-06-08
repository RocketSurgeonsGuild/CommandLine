using System;
using System.Collections.Generic;
using System.Linq;

namespace Rocket.Surgery.Extensions.CommandLine
{
    class DefinedServices : IServiceProvider
    {
        private readonly List<(Type serviceType, object serviceValue)> _services;

        public DefinedServices(List<(Type serviceType, object serviceValue)> services)
        {
            _services = services;
        }

        public object GetService(Type serviceType)
        {
            return _services.FirstOrDefault(x => x.serviceType == serviceType).serviceValue;
        }
    }
}