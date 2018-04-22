using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    class CommandLineServiceProvider : IServiceProvider
    {
        private readonly IModelAccessor _modelAccessor;
        private readonly Lazy<IServiceProvider> _serviceProvider;

        public CommandLineServiceProvider(IModelAccessor modelAccessor, Func<IServiceProvider> serviceProviderFactory)
        {
            _modelAccessor = modelAccessor ?? throw new ArgumentNullException(nameof(modelAccessor));
            if (serviceProviderFactory != null) _serviceProvider = new Lazy<IServiceProvider>(serviceProviderFactory);
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

            return _serviceProvider?.Value.GetService(serviceType);
        }
    }
}