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
        private readonly IModelAccessor _modelAccessor;
        private readonly Func<IApplicationState, IServiceProvider> _serviceProviderFactory;
        private IServiceProvider _serviceProvider;
        private readonly DefinedServices _services;

        public CommandLineServiceProvider(IModelAccessor modelAccessor, DefinedServices services, Func<IApplicationState, IServiceProvider> serviceProviderFactory)
        {
            _modelAccessor = modelAccessor ?? throw new ArgumentNullException(nameof(modelAccessor));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _serviceProviderFactory = serviceProviderFactory;
        }

        public object GetService(Type serviceType)
        {
            if (typeof(IApplicationState).IsAssignableFrom(serviceType) && serviceType?.IsAssignableFrom(_modelAccessor.GetModelType()) == true)
            {
                return _modelAccessor.GetModel();
            }

            if (typeof(IApplicationStateInner).IsAssignableFrom(serviceType) && serviceType?.IsAssignableFrom(_modelAccessor.GetModelType()) == true)
            {
                return _modelAccessor.GetModel();
            }

            if (serviceType == typeof(IConsole))
            {
                return PhysicalConsole.Singleton;
            }

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
