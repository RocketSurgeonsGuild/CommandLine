using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public interface IApplicationState
    {
        bool Verbose { get; }
        bool Trace { get; }
        bool Debug { get; }
        LogLevel GetLogLevel();
    }
}