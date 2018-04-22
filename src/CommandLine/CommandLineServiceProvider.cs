using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    class CommandLineServiceProvider : IServiceProvider
    {
        private readonly IModelAccessor _modelAccessor;
        private readonly Func<IApplicationState, IServiceProvider> _serviceProviderFactory;
        private IServiceProvider _serviceProvider;
        private readonly List<(Type serviceType, object serviceValue)> _services;

        public CommandLineServiceProvider(IModelAccessor modelAccessor, List<(Type serviceType, object serviceValue)> services, Func<IApplicationState, IServiceProvider> serviceProviderFactory)
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

            var givenService = _services.FirstOrDefault(x => x.serviceType == serviceType).serviceValue;
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
