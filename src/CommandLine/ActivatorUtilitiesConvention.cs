using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        private Func<IApplicationState, IServiceProvider> _serviceProviderFactory;

        public ActivatorUtilitiesConvention(Func<IApplicationState, IServiceProvider> serviceProviderFactory)
        {
            _serviceProviderFactory = serviceProviderFactory;
        }

        class LazyServiceProvider : IServiceProvider
        {
            private readonly CommandLineApplication _application;
            private readonly Func<IApplicationState, IServiceProvider> _serviceProviderFactory;

            public LazyServiceProvider(CommandLineApplication application, Func<IApplicationState, IServiceProvider> serviceProviderFactory)
            {
                _application = application;
                _serviceProviderFactory = serviceProviderFactory;
            }

            public object GetService(Type serviceType)
            {
                // TODO: Get Option by attribute
                // template or long name KebabCase
                // + remaining args
                throw new NotImplementedException();
            }
        }


        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (_serviceProviderFactory != null)
            {
                AdditionalServicesProperty.SetValue(context.Application, new LazyServiceProvider(context.Application, _serviceProviderFactory));
            }

            if (context.ModelType == null)
            {
                return;
            }

            ApplyMethod.MakeGenericMethod(context.ModelType).Invoke(this, new object[] { context });

            context.Application.OnExecute(async () => await this.OnExecute(context));
        }

        private async Task<int> OnExecute(ConventionContext context)
        {
            const BindingFlags binding = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var typeInfo = context.ModelType.GetTypeInfo();
            MethodInfo method;
            MethodInfo asyncMethod;
            try
            {
                method = typeInfo.GetMethod("OnExecute", binding);
                asyncMethod = typeInfo.GetMethod("OnExecuteAsync", binding);
            }
            catch (AmbiguousMatchException ex)
            {
                throw new InvalidOperationException(AmbiguousOnExecuteMethod, ex);
            }

            if (method != null && asyncMethod != null)
            {
                throw new InvalidOperationException(AmbiguousOnExecuteMethod);
            }

            method = method ?? asyncMethod;

            if (method == null)
            {
                throw new InvalidOperationException(NoOnExecuteMethodFound);
            }

            var model = ActivatorUtilities.CreateInstance(context.Application, context.ModelType);
            var uninitializedModel = context.ModelAccessor.GetModel();

            foreach (var field in typeInfo.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                field.SetValue(model, field.GetValue(uninitializedModel));
            }
            foreach (var property in typeInfo.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite && x.CanRead))
            {
                property.SetValue(model, property.GetValue(uninitializedModel));
            }

            var arguments = (object[])BindParametersMethod.Invoke(null, new object[] { method, context.Application });

            if (method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<int>))
            {
                return await InvokeAsync(method, model, arguments);
            }
            else if (method.ReturnType == typeof(void) || method.ReturnType == typeof(int))
            {
                return Invoke(method, model, arguments);
            }

            throw new InvalidOperationException(InvalidOnExecuteReturnType(method.Name));
        }

        private async Task<int> InvokeAsync(MethodInfo method, object instance, object[] arguments)
        {
            var result = (Task)method.Invoke(instance, arguments);
            if (result is Task<int> intResult)
            {
                return await intResult;
            }

            await result;
            return 0;
        }

        private int Invoke(MethodInfo method, object instance, object[] arguments)
        {
            var result = method.Invoke(instance, arguments);
            if (method.ReturnType == typeof(int))
            {
                return (int)result;
            }

            return 0;
        }



        private static MethodInfo BindParametersMethod = typeof(ConventionContext).Assembly
            .GetType("McMaster.Extensions.CommandLineUtils.ReflectionHelper")
            .GetMethod("BindParameters", BindingFlags.Public | BindingFlags.Static);
        public const string AmbiguousOnExecuteMethod = "Could not determine which 'OnExecute' or 'OnExecuteAsync' method to use. Multiple methods with this name were found";
        public const string NoOnExecuteMethodFound = "No method named 'OnExecute' or 'OnExecuteAsync' could be found";
        public static string InvalidOnExecuteReturnType(string methodName) => methodName + " must have a return type of int or void, or if the method is async, Task<int> or Task.";

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
                    return (TModel)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(TModel));
                    // ActivatorUtilities.CreateInstance<TModel>(context.Application);
                };
        }
    }
}
