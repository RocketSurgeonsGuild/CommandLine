using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// ApplicationState.
    /// Implements the <see cref="IApplicationState" />
    /// </summary>
    /// <seealso cref="IApplicationState" />
    [Command(ThrowOnUnexpectedArgument = false)]
    class ApplicationState : IApplicationState
    {
        /// <summary>
        /// Gets or sets the on run delegate.
        /// </summary>
        /// <value>The on run delegate.</value>
        public OnRunDelegate OnRunDelegate { get; set; }

        /// <summary>
        /// Gets or sets the type of the on run.
        /// </summary>
        /// <value>The type of the on run.</value>
        public Type OnRunType { get; set; }

        /// <summary>
        /// Gets or sets the on parse delegates.
        /// </summary>
        /// <value>The on parse delegates.</value>
        public List<OnParseDelegate> OnParseDelegates { get; internal set; } = new List<OnParseDelegate>();

        /// <summary>
        /// Gets the remaining arguments.
        /// </summary>
        /// <value>The remaining arguments.</value>
        public string[] RemainingArguments { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Rocket.Surgery.Extensions.CommandLine.IApplicationState" /> is verbose.
        /// </summary>
        /// <value><c>true</c> if verbose; otherwise, <c>false</c>.</value>
        [Option(CommandOptionType.NoValue, Description = "Verbose logging", Inherited = true, ShowInHelpText = true)]
        public bool Verbose { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Rocket.Surgery.Extensions.CommandLine.IApplicationState" /> is trace.
        /// </summary>
        /// <value><c>true</c> if trace; otherwise, <c>false</c>.</value>
        [Option(CommandOptionType.NoValue, Description = "Trace logging", Inherited = true, ShowInHelpText = true)]
        public bool Trace { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Rocket.Surgery.Extensions.CommandLine.IApplicationState" /> is debug.
        /// </summary>
        /// <value><c>true</c> if debug; otherwise, <c>false</c>.</value>
        [Option(CommandOptionType.NoValue, Description = "Debug logging", Inherited = true, ShowInHelpText = true)]
        public bool Debug { get; }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <value>The log.</value>
        [Option(CommandOptionType.SingleValue, Description = "Log level", Inherited = true, ShowInHelpText = true)]
        public (bool HasValue, LogLevel Level) Log { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is default command.
        /// </summary>
        /// <value><c>true</c> if this instance is default command; otherwise, <c>false</c>.</value>
        public bool IsDefaultCommand { get; internal set; }

        /// <summary>
        /// Gets the log level.
        /// </summary>
        /// <returns>LogLevel.</returns>
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
