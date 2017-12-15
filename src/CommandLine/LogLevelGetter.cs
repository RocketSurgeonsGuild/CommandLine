using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    class LogLevelGetter
    {
        private readonly LogLevel? _userLogLevel;
        private readonly CommandOption _verbose;
        private readonly CommandOption _trace;
        private readonly CommandOption _debug;
        private readonly CommandOption _logLevel;

        public LogLevelGetter(CommandOption verbose, CommandOption trace, CommandOption debug, CommandOption logLevel, LogLevel? userLogLevel)
        {
            _userLogLevel = userLogLevel;
            _verbose = verbose ?? throw new ArgumentNullException(nameof(verbose));
            _trace = trace ?? throw new ArgumentNullException(nameof(trace));
            _debug = debug ?? throw new ArgumentNullException(nameof(debug));
            _logLevel = logLevel ?? throw new ArgumentNullException(nameof(logLevel));
        }

        public LogLevel LogLevel
        {
            get
            {
                if (_logLevel.HasValue())
                {
                    if (_logLevel.Value().Equals("verbose", StringComparison.OrdinalIgnoreCase))
                    {
                        return LogLevel.Trace;
                    }

                    if (Enum.TryParse<LogLevel>(_logLevel.Value(), true, out var logLevel))
                    {
                        return logLevel;
                    }
                }
                if (_verbose.HasValue() || _trace.HasValue())
                    return LogLevel.Trace;
                if (_debug.HasValue())
                    return LogLevel.Debug;
                if (_userLogLevel.HasValue) return _userLogLevel.Value;
                return LogLevel.Information;
            }
        }
    }
}