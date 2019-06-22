﻿using System;
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
    /// </summary>
    public class CommandLineBuilder : ConventionBuilder<ICommandLineBuilder, ICommandLineConvention, CommandLineConventionDelegate>, ICommandLineBuilder, ICommandLineConventionContext
    {
        private readonly CommandLineApplication<ApplicationState> _application;
        private readonly DiagnosticSource _diagnosticSource;

        private readonly List<(Type serviceType, object serviceValue)> _services =
            new List<(Type serviceType, object serviceValue)>();

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

        public IConventionBuilder CommandLineApplicationConventions => _application.Conventions;

        ICommandLineConventionContext ICommandLineConventionContext.AddCommand<T>(string name,
            Action<CommandLineApplication<T>> action = null, bool throwOnUnexpectedArg = true)
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

        ICommandLineConventionContext ICommandLineConventionContext.WithService<S>(S value)
        {
            WithService(value);
            return this;
        }

        public ILogger Logger { get; }
        //private IServiceCollection _serviceCollection;

        public ICommandLineBuilder WithService<S>(S value)
        {
            _services.Add((typeof(S), value));
            return this;
        }

        public ICommandLineBuilder OnRun(OnRunDelegate @delegate)
        {
            _application.Model.OnRunDelegate = @delegate;
            _application.Model.OnRunType = null;
            return this;
        }

        public ICommandLineBuilder OnRun<T>() where T : IDefaultCommand
        {
            _application.Model.OnRunType = typeof(T);
            _application.Model.OnRunDelegate = null;
            return this;
        }

        public ICommandLineBuilder AddCommand<T>(string name, Action<CommandLineApplication<T>> action = null, bool throwOnUnexpectedArg = true)
            where T : class
        {
            if (action == null)
                action = application => { };

            _application.Command(name, action, throwOnUnexpectedArg);
            return this;
        }

        public ICommandLineBuilder OnParse(OnParseDelegate @delegate)
        {
            _application.Model.OnParseDelegates.Add(@delegate);
            return this;
        }

        public ICommandLine Build(Assembly entryAssembly = null)
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

            return new CommandLine(this, _application, Logger);
        }
    }
}
