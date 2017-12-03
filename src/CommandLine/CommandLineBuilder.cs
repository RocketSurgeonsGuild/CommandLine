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
        private readonly CommandOption _verbose;
        private readonly CommandOption _trace;
        private readonly CommandOption _debug;

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

            Application.HelpOption();
            _verbose = Application.Option("-v | --verbose", "Verbose logging", CommandOptionType.NoValue, option =>
            {
                option.ShowInHelpText = true;
            });
            _trace = Application.Option("-t | --trace", "Trace logging", CommandOptionType.NoValue, option =>
            {
                option.ShowInHelpText = true;
            });
            _debug = Application.Option("-d | --debug", "Debug logging", CommandOptionType.NoValue, option =>
            {
                option.ShowInHelpText = true;
            });
            Application.OnExecute(() => StopCode);
        }

        public IAssemblyProvider AssemblyProvider { get; }
        public IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
        public CommandLineApplication Application { get; }
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

            Application.Command("run", application =>
            {
                application.Description = "Run the application";
                application.ExtendedHelpText = "Default action if no command is given";
                application.OnExecute(() => StopCode);
            });

            new ConventionComposer(_scanner)
                .Register(
                    this,
                    typeof(ICommandLineConventionContext),
                    typeof(CommandLineConventionDelegate)
                );

            foreach (var command in Application.Commands)
            {
                EnsureOptions(command);
            }

            return new CommandLineHandler(Application, StopCode, _verbose, _trace, _debug);
        }

        private static void EnsureOptions(CommandLineApplication command)
        {
            if (command.OptionHelp == null) command.HelpOption();
            command.ShowInHelpText = true;

            foreach (var subcommand in command.Commands)
            {
                EnsureOptions(subcommand);
            }
        }
    }
}
