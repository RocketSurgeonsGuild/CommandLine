using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public class CommandLineHandler
    {
        private readonly int _stopCode;
        private readonly LogLevelGetter _builder;
        private readonly CommandLineApplication _run;

        internal CommandLineHandler(CommandLineApplication application, int stopCode, LogLevelGetter builder, CommandLineApplication run)
        {
            Application = application;
            _stopCode = stopCode;
            _builder = builder;
            _run = run;
        }

        public CommandLineApplication Application { get; }
        public LogLevel LogLevel => _builder.LogLevel;

        public int? Execute(params string[] args)
        {
            var result = Application.Execute(args);
            if (result == _stopCode || _run.IsShowingInformation || Application.IsShowingInformation) return null;
            return result;
        }
    }
}
