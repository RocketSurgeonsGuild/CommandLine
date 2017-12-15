using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public class CommandLineHandler
    {
        private readonly int _stopCode;
        private readonly LogLevelGetter _builder;

        internal CommandLineHandler(CommandLineApplication application, int stopCode, LogLevelGetter builder)
        {
            Application = application;
            _stopCode = stopCode;
            _builder = builder;
        }

        public CommandLineApplication Application { get; }
        public LogLevel LogLevel => _builder.LogLevel;

        public int? Execute(params string[] args)
        {
            var result = Application.Execute(args);
            if (result == _stopCode) return null;
            return result;
        }
    }
}
