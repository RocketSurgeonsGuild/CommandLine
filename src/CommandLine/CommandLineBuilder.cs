using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
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
    public class CommandLineBuilder : Builder, ICommandLineBuilder
    {
        private readonly IConventionScanner _scanner;
        public const int StopCode = -1337;
        private readonly CommandOption<bool> _verbose;
        private readonly CommandOption<bool> _trace;
        private readonly CommandOption<bool> _debug;
        private readonly CommandOption<LogLevel> _logLevel;
        private LogLevel? _userLogLevel;

        public CommandLineBuilder(
            IConventionScanner scanner,
            IAssemblyProvider assemblyProvider,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            CommandLineApplication application,
            ILogger logger)
        {
            AssemblyProvider = assemblyProvider ?? throw new ArgumentNullException(nameof(assemblyProvider));
            AssemblyCandidateFinder = assemblyCandidateFinder ?? throw new ArgumentNullException(nameof(assemblyCandidateFinder));
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            Application = application ?? throw new ArgumentNullException(nameof(application));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _verbose = Application.Option<bool>("-v | --verbose", "Verbose logging", CommandOptionType.NoValue, option =>
            {
                option.ShowInHelpText = true;
                option.Inherited = true;
            });
            _trace = Application.Option<bool>("-t | --trace", "Trace logging", CommandOptionType.NoValue, option =>
            {
                option.ShowInHelpText = true;
                option.Inherited = true;
            });
            _debug = Application.Option<bool>("-d | --debug", "Debug logging", CommandOptionType.NoValue, option =>
            {
                option.ShowInHelpText = true;
                option.Inherited = true;
            });
            _logLevel = Application.Option<LogLevel>("-l | --loglevel <LogLevel>", "Log level", CommandOptionType.SingleValue, option =>
            {
                option.ShowInHelpText = true;
                option.Description = $"The log level, valid levels are: {string.Join(",", Enum.GetValues(typeof(LogLevel)))}";
                option.Inherited = true;
                option.ValueName = "LogLevel";
                option.Validators.Add(new LogLevelValidator());
            });
        }

        public IAssemblyProvider AssemblyProvider { get; }
        public IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
        public CommandLineApplication Application { get; }
        public LogLevel LogLevel { get => _userLogLevel ?? LogLevel.Information; set => _userLogLevel = value; }
        public ILogger Logger { get; }

        public ICommandLineBuilder AddDelegate(CommandLineConventionDelegate @delegate)
        {
            _scanner.AddDelegate(@delegate);
            return this;
        }

        public ICommandLineBuilder AddConvention(ICommandLineConvention convention)
        {
            _scanner.AddConvention(convention);
            return this;
        }

        public CommandLineHandler Build(Assembly entryAssembly = null)
        {
            if (entryAssembly is null) entryAssembly = Assembly.GetEntryAssembly();

            Application.VersionOption(
                "--version",
                () => entryAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version,
                () => entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion
            );

            new ConventionComposer(_scanner)
                .Register(
                    this,
                    typeof(ICommandLineConvention),
                    typeof(CommandLineConventionDelegate)
                );

            Application.OnExecute(() => StopCode);

            var run = Application.Command("run", a =>
            {
                a.Description = "Run the application";
                a.ExtendedHelpText = "Default action if no command is given";
                a.ShowInHelpText = true;
                a.OnExecute(() => StopCode);
            });

            EnsureHelp(Application);

            return new CommandLineHandler(Application, StopCode, new LogLevelGetter(_verbose, _trace, _debug, _logLevel, _userLogLevel), run);
        }

        void EnsureHelp(CommandLineApplication application)
        {
            application.HelpOption();
            foreach (var a in application.Commands)
            {
                EnsureHelp(a);
            }
        }
    }
}
