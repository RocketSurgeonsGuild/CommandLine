using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    [Command(ThrowOnUnexpectedArgument = false), Subcommand("run", typeof(RunApplication))]
    public class ApplicationState : IApplicationStateInner
    {
        private readonly IServiceProvider _serviceProvider;

        public ApplicationState(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public CommandLineDefaultDelegate Delegate { get; set; }
        public Type Type { get; set; }

        public Task<int> OnExecuteAsync()
        {
            return Delegate?.Invoke(this, RemainingArguments) ??
                // TODO: Bring in extensions here that allow for invoke methods
                throw new NotSupportedException("Default delegate or type was not defined for command line");
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
