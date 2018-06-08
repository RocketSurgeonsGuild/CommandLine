using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    class CommandLineServiceProvider : IServiceProvider
    {
        private readonly IModelAccessor _modelAccessor;
        private readonly DefinedServices _services;

        public CommandLineServiceProvider(IModelAccessor modelAccessor, DefinedServices services)
        {
            _modelAccessor = modelAccessor;
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public object GetService(Type serviceType)
        {
            if (typeof(IApplicationState).IsAssignableFrom(serviceType))
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

            return _services.GetService(serviceType);
        }
    }
}
