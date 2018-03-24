using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    class LogLevelGetter
    {
        private readonly LogLevel? _userLogLevel;
        private readonly CommandOption<bool> _verbose;
        private readonly CommandOption<bool> _trace;
        private readonly CommandOption<bool> _debug;
        private readonly CommandOption<LogLevel> _logLevel;

        public LogLevelGetter(CommandOption<bool> verbose, CommandOption<bool> trace, CommandOption<bool> debug, CommandOption<LogLevel> logLevel, LogLevel? userLogLevel)
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
                    return _logLevel.ParsedValue;
                }
                if (_verbose.HasValue() || _trace.HasValue())
                    return LogLevel.Trace;
                if (_debug.HasValue())
                    return LogLevel.Debug;
                return _userLogLevel ?? LogLevel.Information;
            }
        }
    }
}
