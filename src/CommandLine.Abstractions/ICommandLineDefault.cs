using System.Threading.Tasks;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public interface ICommandLineDefault
    {   
        Task<int> OnExecuteAsync(IApplicationState state, string[] remainingArguments);
    }
}
