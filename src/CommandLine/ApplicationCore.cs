using System.Threading.Tasks;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public abstract class ApplicationCore
    {
        public abstract Task<int> OnExecuteAsync();
    }
}
