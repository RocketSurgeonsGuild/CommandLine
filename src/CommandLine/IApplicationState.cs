using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public interface IApplicationState
    {
        IEnumerable<string> RemainingArguments { get; }
        bool Verbose { get; }
        bool Trace { get; }
        bool Debug { get; }
        LogLevel GetLogLevel();
    }
}
