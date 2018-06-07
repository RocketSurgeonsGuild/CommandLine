using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rocket.Surgery.Extensions.CommandLine
{
    interface IApplicationStateInner : IApplicationState
    {
        CommandLineDefaultDelegate Delegate { get; set; }
        string[] RemainingArguments { get; set; }
        Task<int> OnExecuteAsync();
    }
}
