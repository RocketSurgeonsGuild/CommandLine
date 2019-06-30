using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// CommandLineExecutor.
    /// Implements the <see cref="Rocket.Surgery.Extensions.CommandLine.ICommandLineExecutor" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Extensions.CommandLine.ICommandLineExecutor" />
    class CommandLineExecutor : ICommandLineExecutor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineExecutor"/> class.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="applicationState">State of the application.</param>
        public CommandLineExecutor(CommandLineApplication application, IApplicationState applicationState)
        {
            Application = application;
            ApplicationState = applicationState;
            IsDefaultCommand = Application is IModelAccessor m && m.GetModelType() == typeof(ApplicationState) && !Application.IsShowingInformation;
        }

        /// <summary>
        /// Gets the application.
        /// </summary>
        /// <value>The application.</value>
        public CommandLineApplication Application { get; }
        /// <summary>
        /// Gets the state of the application.
        /// </summary>
        /// <value>The state of the application.</value>
        public IApplicationState ApplicationState { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is default command.
        /// </summary>
        /// <value><c>true</c> if this instance is default command; otherwise, <c>false</c>.</value>
        public bool IsDefaultCommand { get; }

        /// <summary>
        /// Executes the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>System.Int32.</returns>
        public int Execute(IServiceProvider serviceProvider)
        {
            if (Application.IsShowingInformation)
            {
                return 0;
            }


            var validationResult = Application.GetValidationResult();
            if (validationResult != ValidationResult.Success)
            {
                return Application.ValidationErrorHandler(validationResult);
            }

            if (Application is IModelAccessor ma && ma.GetModel() is ApplicationState state)
            {
                if (state.OnRunType != null)
                {
                    var @default = ActivatorUtilities.CreateInstance(serviceProvider, state.OnRunType) as IDefaultCommand;
                    return @default.Run(state);
                }

                return state.OnRunDelegate?.Invoke(state) ?? int.MinValue;
            }

            ActivatorUtilitiesConvention.AdditionalServicesProperty.SetValue(Application, serviceProvider);
            return Application.Invoke();
        }
    }
}
