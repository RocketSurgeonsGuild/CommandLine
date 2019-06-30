using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// Interface IApplicationState
    /// </summary>
    public interface IApplicationState
    {
        /// <summary>
        /// Gets the remaining arguments.
        /// </summary>
        /// <value>The remaining arguments.</value>
        string[] RemainingArguments { get; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="IApplicationState"/> is verbose.
        /// </summary>
        /// <value><c>true</c> if verbose; otherwise, <c>false</c>.</value>
        bool Verbose { get; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="IApplicationState"/> is trace.
        /// </summary>
        /// <value><c>true</c> if trace; otherwise, <c>false</c>.</value>
        bool Trace { get; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="IApplicationState"/> is debug.
        /// </summary>
        /// <value><c>true</c> if debug; otherwise, <c>false</c>.</value>
        bool Debug { get; }
        /// <summary>
        /// Gets the log level.
        /// </summary>
        /// <returns>LogLevel.</returns>
        LogLevel GetLogLevel();
        /// <summary>
        /// Gets a value indicating whether this instance is default command.
        /// </summary>
        /// <value><c>true</c> if this instance is default command; otherwise, <c>false</c>.</value>
        bool IsDefaultCommand { get; }
    }
}
