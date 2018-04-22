using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    [Command(ThrowOnUnexpectedArgument = false), Subcommand("run", typeof(RunApplication))]
    public class ApplicationState<T> : IApplicationStateInner
        where T : class, ICommandLineDefault
    {
        private readonly IServiceProvider _serviceProvider;

        public ApplicationState(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public Task<int> OnExecuteAsync()
        {
            return _serviceProvider.GetService<T>()?.OnExecuteAsync(this, RemainingArguments) ??
                   ActivatorUtilities.CreateInstance<T>(_serviceProvider, this as IApplicationState).OnExecuteAsync(this, RemainingArguments);
        }

        public string[] RemainingArguments { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Verbose logging", Inherited = true, ShowInHelpText = true)]
        public bool Verbose { get; }

        [Option(CommandOptionType.NoValue, Description = "Trace logging", Inherited = true, ShowInHelpText = true)]
        public bool Trace { get; }

        [Option(CommandOptionType.NoValue, Description = "Debug logging", Inherited = true, ShowInHelpText = true)]
        public bool Debug { get; }

        [Option(CommandOptionType.SingleValue, Description = "Log level", Inherited = true, ShowInHelpText = true)]
        public (bool HasValue, LogLevel Level) Log { get; }

        public LogLevel GetLogLevel()
        {
            if (Log.HasValue)
                return Log.Level;

            if (Verbose || Trace)
                return LogLevel.Trace;

            if (Debug)
                return LogLevel.Debug;

            return LogLevel.Information;
        }
    }
}
