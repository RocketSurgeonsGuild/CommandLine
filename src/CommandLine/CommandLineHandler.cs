using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public class CommandLineHandler
    {
        private readonly int _stopCode;
        private readonly CommandOption _verbose;
        private readonly CommandOption _trace;
        private readonly CommandOption _debug;

        public CommandLineHandler(CommandLineApplication application, int stopCode, CommandOption verbose, CommandOption trace, CommandOption debug)
        {
            Application = application;
            _stopCode = stopCode;
            _verbose = verbose;
            _trace = trace;
            _debug = debug;
        }

        public CommandLineApplication Application { get; }
        public LogLevel LogLevel
        {
            get
            {
                if (_verbose.HasValue() || _trace.HasValue())
                {
                    return LogLevel.Trace;
                }
                return _debug.HasValue() ? LogLevel.Debug : LogLevel.Information;
            }
        }

        public int? Execute(string[] args)
        {
            var result = Application.Execute(args);
            if (result == _stopCode) return null;
            return result;
        }
    }
}
