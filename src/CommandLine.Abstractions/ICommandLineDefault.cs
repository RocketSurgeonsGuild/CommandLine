using System.Threading.Tasks;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public delegate Task<int> CommandLineDefaultDelegate(IApplicationState state, string[] remainingArguments);
}
