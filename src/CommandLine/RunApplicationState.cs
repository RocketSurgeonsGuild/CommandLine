using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    [Command(ThrowOnUnexpectedArgument = false)]
    class RunApplicationState : IApplicationState
    {
        public string[] RemainingArguments { get; set; }

        public bool Verbose => Parent?.Verbose ?? false;

        public bool Trace => Parent?.Trace ?? false;

        public bool Debug => Parent?.Debug ?? false;

        public ApplicationState Parent { get; set; }

        public bool IsDefaultCommand { get; internal set; }

        public LogLevel GetLogLevel()
        {
            if (Parent.Log.HasValue)
                return Parent.Log.Level;

            if (Parent.Verbose || Parent.Trace)
                return LogLevel.Trace;

            if (Parent.Debug)
                return LogLevel.Debug;

            return LogLevel.Information;
        }
    }
}