using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// Logging Builder
    /// Implements the <see cref="Rocket.Surgery.Conventions.ConventionBuilder{Rocket.Surgery.Extensions.CommandLine.ICommandLineBuilder, Rocket.Surgery.Extensions.CommandLine.ICommandLineConvention, Rocket.Surgery.Extensions.CommandLine.CommandLineConventionDelegate}" />
    /// Implements the <see cref="Rocket.Surgery.Extensions.CommandLine.ICommandLineBuilder" />
    /// Implements the <see cref="Rocket.Surgery.Extensions.CommandLine.ICommandLineConventionContext" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Conventions.ConventionBuilder{Rocket.Surgery.Extensions.CommandLine.ICommandLineBuilder, Rocket.Surgery.Extensions.CommandLine.ICommandLineConvention, Rocket.Surgery.Extensions.CommandLine.CommandLineConventionDelegate}" />
    /// <seealso cref="Rocket.Surgery.Extensions.CommandLine.ICommandLineBuilder" />
    /// <seealso cref="Rocket.Surgery.Extensions.CommandLine.ICommandLineConventionContext" />
    public class CommandLineBuilder : ConventionBuilder<ICommandLineBuilder, ICommandLineConvention, CommandLineConventionDelegate>, ICommandLineBuilder, ICommandLineConventionContext
    {
        private readonly CommandLineApplication<ApplicationState> _application;
        private readonly DiagnosticSource _diagnosticSource;

        private readonly List<(Type serviceType, object serviceValue)> _services =
            new List<(Type serviceType, object serviceValue)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineBuilder"/> class.
        /// </summary>
        /// <param name="scanner">The scanner.</param>
        /// <param name="assemblyProvider">The assembly provider.</param>
        /// <param name="assemblyCandidateFinder">The assembly candidate finder.</param>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <param name="properties">The properties.</param>
        /// <exception cref="ArgumentNullException">diagnosticSource</exception>
        public CommandLineBuilder(
            IConventionScanner scanner,
            IAssemblyProvider assemblyProvider,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            DiagnosticSource diagnosticSource,
            IDictionary<object, object> properties) : base(scanner, assemblyProvider, assemblyCandidateFinder, properties)
        {
            _application = new CommandLineApplication<ApplicationState>()
            {
                ThrowOnUnexpectedArgument = false
            };

            _diagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
            Logger = new DiagnosticLogger(diagnosticSource);
        }

        /// <summary>
        /// Gets the command line application conventions.
        /// </summary>
        /// <value>The command line application conventions.</value>
        public IConventionBuilder CommandLineApplicationConventions => _application.Conventions;

        ICommandLineConventionContext ICommandLineConventionContext.AddCommand<T>(Action<CommandLineApplication<T>> action = null, bool throwOnUnexpectedArg = true)
        {
            AddCommand(action, throwOnUnexpectedArg);
            return this;
        }

        ICommandLineConventionContext ICommandLineConventionContext.AddCommand<T>(string name, Action<CommandLineApplication<T>> action = null, bool throwOnUnexpectedArg = true)
        {
            AddCommand(name, action, throwOnUnexpectedArg);
            return this;
        }

        ICommandLineConventionContext ICommandLineConventionContext.OnParse(OnParseDelegate @delegate)
        {
            OnParse(@delegate);
            return this;
        }

        ICommandLineConventionContext ICommandLineConventionContext.OnRun(OnRunDelegate @delegate)
        {
            OnRun(@delegate);
            return this;
        }

        ICommandLineConventionContext ICommandLineConventionContext.OnRun<T>()
        {
            OnRun<T>();
            return this;
        }

        /// <summary>
        /// A logger that is configured to work with each convention item
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; }

        /// <summary>
        /// Called when [run].
        /// </summary>
        /// <param name="delegate">The delegate.</param>
        /// <returns>ICommandLineBuilder.</returns>
        public ICommandLineBuilder OnRun(OnRunDelegate @delegate)
        {
            _application.Model.OnRunDelegate = @delegate;
            _application.Model.OnRunType = null;
            return this;
        }

        /// <summary>
        /// Called when [run].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>ICommandLineBuilder.</returns>
        public ICommandLineBuilder OnRun<T>() where T : IDefaultCommand
        {
            _application.Model.OnRunType = typeof(T);
            _application.Model.OnRunDelegate = null;
            return this;
        }

        /// <summary>
        /// Adds the command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="throwOnUnexpectedArg">if set to <c>true</c> [throw on unexpected argument].</param>
        /// <returns>ICommandLineBuilder.</returns>
        public ICommandLineBuilder AddCommand<T>(Action<CommandLineApplication<T>> action = null, bool throwOnUnexpectedArg = true)
            where T : class
        {
            if (action == null)
                action = application => { };

            var commandAttribute = (typeof(T)).GetCustomAttribute<CommandAttribute>();

            if (commandAttribute == null)
            {
                throw new ArgumentException($"You must give the command a name using {typeof(CommandAttribute).FullName} to add a command without a name.");
            }

            _application.Command(commandAttribute.Name, action, throwOnUnexpectedArg);
            return this;
        }

        /// <summary>
        /// Adds the command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <param name="throwOnUnexpectedArg">if set to <c>true</c> [throw on unexpected argument].</param>
        /// <returns>ICommandLineBuilder.</returns>
        public ICommandLineBuilder AddCommand<T>(string name, Action<CommandLineApplication<T>> action = null, bool throwOnUnexpectedArg = true)
            where T : class
        {
            if (action == null)
                action = application => { };

            _application.Command(name, action, throwOnUnexpectedArg);
            return this;
        }

        /// <summary>
        /// Called when [parse].
        /// </summary>
        /// <param name="delegate">The delegate.</param>
        /// <returns>ICommandLineBuilder.</returns>
        public ICommandLineBuilder OnParse(OnParseDelegate @delegate)
        {
            _application.Model.OnParseDelegates.Add(@delegate);
            return this;
        }

        /// <summary>
        /// Builds the specified entry assembly.
        /// </summary>
        /// <param name="entryAssembly">The entry assembly.</param>
        /// <returns>ICommandLine.</returns>
        public ICommandLine Build(Assembly entryAssembly = null)
        {
            if (entryAssembly is null) entryAssembly = Assembly.GetEntryAssembly();
            
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
                    new CommandLineServiceProvider(_application)
                ));

            return new CommandLine(this, _application, Logger);
        }
    }
}
