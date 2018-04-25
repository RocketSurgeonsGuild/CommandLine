using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Hosting;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// Logging Builder
    /// </summary>
    public class CommandLineBuilder<T> : ConventionBuilder<ICommandLineBuilder, ICommandLineConvention, CommandLineConventionDelegate>, ICommandLineBuilder
        where T : class, ICommandLineDefault
    {
        private readonly CommandLineApplication<ApplicationState<T>> _application;

        private readonly List<(Type serviceType, object serviceValue)> _services =
            new List<(Type serviceType, object serviceValue)>();
        private Func<IApplicationState, IServiceProvider> _serviceProviderFactory;

        public CommandLineBuilder(
            IConventionScanner scanner,
            IAssemblyProvider assemblyProvider,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            ILogger logger) : base(scanner, assemblyProvider, assemblyCandidateFinder)
        {
            _application = new CommandLineApplication<ApplicationState<T>>();
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override ICommandLineBuilder GetBuilder() => this;

        public IConventionBuilder CommandLineApplicationConventions => _application.Conventions;

        public ICommandLineConventionContext AddCommand<TSubCommand>(string name, Action<CommandLineApplication<TSubCommand>> action = null, bool throwOnUnexpectedArg = true)
            where TSubCommand : class
        {
            if (action == null)
                action = application => { };

            _application.Command(name, action, throwOnUnexpectedArg);
            return this;
        }

        public ILogger Logger { get; }

        public CommandLineBuilder<T> WithServiceProvider(Func<IApplicationState, IServiceProvider> serviceProviderFactory)
        {
            _serviceProviderFactory = serviceProviderFactory;
            return this;
        }

        public CommandLineBuilder<T> WithService<S>(S value)
        {
            _services.Add((typeof(S), value));
            return this;
        }

        public CommandLineBuilder<T> WithDefaultCommand(T value)
        {
            _services.Add((typeof(T), value));
            return this;
        }

        public CommandLineHandler<T> Build(Assembly entryAssembly = null)
        {
            if (entryAssembly is null) entryAssembly = typeof(T).GetTypeInfo().Assembly;

            new ConventionComposer(Scanner)
                .Register(
                    this,
                    typeof(ICommandLineConvention),
                    typeof(CommandLineConventionDelegate)
                );

            _application.Conventions
                .UseAttributes()
                .SetAppNameFromEntryAssembly()
                .SetRemainingArgsPropertyOnModel()
                .SetSubcommandPropertyOnModel()
                .SetParentPropertyOnModel()
                .UseOnExecuteMethodFromModel()
                .UseOnValidateMethodFromModel()
                .UseOnValidationErrorMethodFromModel()
                .AddConvention(new DefaultHelpOptionConvention())
                .AddConvention(new VersionConvention(entryAssembly))
                .AddConvention(new ActivatorUtilitiesConvention(
                    new CommandLineServiceProvider(_application, new DefinedServices(_services), _serviceProviderFactory)
                ));

            return new CommandLineHandler<T>(_application);
        }
    }
}
