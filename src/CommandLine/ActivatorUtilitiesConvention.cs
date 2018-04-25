using System;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// Uses an instance of <see cref="IServiceProvider" /> to call constructors
    /// when creating models.
    /// </summary>
    public class ActivatorUtilitiesConvention : IConvention
    {
        private readonly IServiceProvider _additionalServices;

        /// <summary>
        /// Initializes an instance of <see cref="ConstructorInjectionConvention" />.
        /// </summary>
        /// <param name="additionalServices">Additional services use to inject the constructor of the model</param>
        public ActivatorUtilitiesConvention(IServiceProvider additionalServices)
        {
            _additionalServices = additionalServices;
        }

        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (_additionalServices != null)
            {
                AdditionalServicesProperty.SetValue(context.Application, _additionalServices);
            }

            if (context.ModelType == null)
            {
                return;
            }

            ApplyMethod.MakeGenericMethod(context.ModelType).Invoke(this, new object[] { context });
        }

        private static readonly PropertyInfo AdditionalServicesProperty =
            typeof(CommandLineApplication)
                .GetRuntimeProperties()
                .Single(m => m.Name == "AdditionalServices");

        private static readonly MethodInfo ApplyMethod =
            typeof(ActivatorUtilitiesConvention)
                .GetRuntimeMethods()
                .Single(m => m.Name == nameof(ApplyImpl));

        private void ApplyImpl<TModel>(ConventionContext context)
            where TModel : class
        {
            (context.Application as CommandLineApplication<TModel>).ModelFactory =
                () =>
                {
                    return ActivatorUtilities.CreateInstance<TModel>(context.Application);
                };
        }
    }
}
