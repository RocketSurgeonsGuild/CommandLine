using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

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

    class CommandLineServiceProvider : IServiceProvider
    {
        private readonly Func<IApplicationState, IServiceProvider> _serviceProviderFactory;
        private IServiceProvider _serviceProvider;
        private readonly CommandLineApplication _application;
        private readonly DefinedServices _services;

        public CommandLineServiceProvider(DefinedServices services, Func<IApplicationState, IServiceProvider> serviceProviderFactory)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _serviceProviderFactory = serviceProviderFactory;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(DefinedServices))
            {
                return _services;
            }

            var givenService = _services.GetService(serviceType);
            if (givenService != null) return givenService;

            if (_serviceProviderFactory == null) return null;

            if (_serviceProvider == null)
            {
                _serviceProvider = _serviceProviderFactory(_modelAccessor.GetModel() as IApplicationState);
            }

            return _serviceProvider.GetService(serviceType);
        }
    }
}
