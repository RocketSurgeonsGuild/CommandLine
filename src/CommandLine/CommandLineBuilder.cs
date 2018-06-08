using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// Logging Builder
    /// </summary>
    public class CommandLineBuilder : ConventionBuilder<ICommandLineBuilder, ICommandLineConvention, CommandLineConventionDelegate>, ICommandLineBuilder
    {
        private readonly CommandLineApplication<ApplicationState> _application;
        private readonly CommandLineApplication<ApplicationState> _run;

        private readonly List<(Type serviceType, object serviceValue)> _services =
            new List<(Type serviceType, object serviceValue)>();
        private Func<IApplicationState, IServiceProvider> _serviceProviderFactory;

        public CommandLineBuilder(
            IConventionScanner scanner,
            IAssemblyProvider assemblyProvider,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger logger,
            IDictionary<object, object> properties) : base(scanner, assemblyProvider, assemblyCandidateFinder, properties)
        {
            _application = new CommandLineApplication<ApplicationState>()
            {
                ThrowOnUnexpectedArgument = false
            };
            _run = _application.Command<ApplicationState>("run", application =>
            {
                application.ThrowOnUnexpectedArgument = false;
                application.Description = "Run the application";
                application.ExtendedHelpText = "Default action if no command is given";
                application.ShowInHelpText = true;
            });

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
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

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public ILogger Logger { get; }

        public CommandLineBuilder WithService<S>(S value)
        {
            _services.Add((typeof(S), value));
            return this;
        }

        public CommandLineBuilder OnRun(OnRunDelegate @delegate)
        {
            _application.Model.OnRunDelegate = _run.Model.OnRunDelegate = @delegate;
            return this;
        }

        public CommandLineBuilder OnParse(OnParseDelegate @delegate)
        {
            _application.Model.OnParseDelegate = _run.Model.OnParseDelegate = @delegate;
            return this;
        }

        public CommandLine Build(Assembly entryAssembly = null)
        {
            if (entryAssembly is null) entryAssembly = Assembly.GetCallingAssembly();

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
                //.UseOnExecuteMethodFromModel()
                .UseOnValidateMethodFromModel()
                .UseOnValidationErrorMethodFromModel()
                .AddConvention(new DefaultHelpOptionConvention())
                .AddConvention(new VersionConvention(entryAssembly))
                .AddConvention(new ActivatorUtilitiesConvention(
                    new CommandLineServiceProvider(_application, new DefinedServices(_services))
                ));

            return new CommandLine(_application);
        }
    }
}
