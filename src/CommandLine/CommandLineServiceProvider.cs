using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// CommandLineServiceProvider.
    /// Implements the <see cref="System.IServiceProvider" />
    /// </summary>
    /// <seealso cref="System.IServiceProvider" />
    class CommandLineServiceProvider : IServiceProvider
    {
        private readonly IModelAccessor _modelAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineServiceProvider"/> class.
        /// </summary>
        /// <param name="modelAccessor">The model accessor.</param>
        /// <param name="services">The services.</param>
        /// <exception cref="ArgumentNullException">services</exception>
        public CommandLineServiceProvider(IModelAccessor modelAccessor)
        {
            _modelAccessor = modelAccessor;
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type <paramref name="serviceType">serviceType</paramref>.   -or-  null if there is no service object of type <paramref name="serviceType">serviceType</paramref>.</returns>
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

            return null;
        }
    }
}
