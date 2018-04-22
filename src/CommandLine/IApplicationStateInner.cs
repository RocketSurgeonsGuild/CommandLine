using System.Threading.Tasks;

namespace Rocket.Surgery.Extensions.CommandLine
{
    interface IApplicationStateInner : IApplicationState
    {
        Task<int> OnExecuteAsync();
    }
}