using System.Threading.Tasks;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public delegate int OnRunDelegate(IApplicationState state);

    public delegate void OnParseDelegate(IApplicationState state);
}
