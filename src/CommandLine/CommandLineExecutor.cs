using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    class CommandLineExecutor : ICommandLineExecutor
    {
        public CommandLineExecutor(CommandLineApplication application)
        {
            Application = application;
        }

        public CommandLineApplication Application { get; }

        public int Execute(IServiceProvider serviceProvider)
        {
            if (Application.IsShowingInformation)
            {
                return 0;
            }

            var validationResult = (ValidationResult)GetValidationResultMethod.Invoke(Application, Array.Empty<object>());
            if (validationResult != ValidationResult.Success)
            {
                return Application.ValidationErrorHandler(validationResult);
            }

            if (Application is IModelAccessor ma && ma.GetModel() is ApplicationState state)
            {
                return state.OnRunDelegate?.Invoke(state) ?? 0;
            }

            ActivatorUtilitiesConvention.AdditionalServicesProperty.SetValue(Application, serviceProvider);
            return Application.Invoke();
        }

        private readonly MethodInfo GetValidationResultMethod = typeof(CommandLineApplication)
            .GetMethod("GetValidationResult", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}
