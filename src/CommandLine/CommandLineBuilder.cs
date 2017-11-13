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
    public class CommandLineBuilder : Builder, ICommandLineBuilder, ICommandLineConventionContext
    {
        private readonly IConventionScanner _scanner;

        public CommandLineBuilder(
            IConventionScanner scanner,
            IAssemblyProvider assemblyProvider,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IHostingEnvironment envionment,
            IConfiguration configuration,
            CommandLineApplication application)
        {
            AssemblyProvider = assemblyProvider;
            AssemblyCandidateFinder = assemblyCandidateFinder;
            _scanner = scanner;
            Environment = envionment;
            Configuration = configuration;
            Application = application;
        }

        public IAssemblyProvider AssemblyProvider { get; }
        public IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public CommandLineApplication Application { get; }

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

        public CommandLineApplication Build(ILogger logger, Assembly entryAssembly = null)
        {
            if (entryAssembly is null) entryAssembly = Assembly.GetEntryAssembly();
            new ConventionComposer(_scanner, logger)
                .Register(
                    this,
                    typeof(ICommandLineConventionContext),
                    typeof(CommandLineConventionDelegate)
                );

            Application.HelpOption();
            Application.VersionOption(
                "--version",
                () => entryAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version,
                () => entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion
            );

            foreach (var command in Application.Commands)
            {
                EnsureOptions(command);
            }

            return Application;
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
