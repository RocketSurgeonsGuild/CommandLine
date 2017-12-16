using System.Linq;
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
            if (IsShowingInformation(Application)) return 0;
            if (result == _stopCode) return null;
            return result;
        }

        private bool IsShowingInformation(CommandLineApplication application)
        {
            return application.IsShowingInformation || application.Commands.Any(IsShowingInformation);
        }
    }
}
