using System.Threading.Tasks;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public delegate int OnRunDelegate(IApplicationState state);

    public delegate void OnParseDelegate(IApplicationState state);

    public interface IDefaultCommand
    {
        int Run(IApplicationState state);
    }

    class OnRunDefaultCommand : IDefaultCommand
    {
        private readonly OnRunDelegate @delegate;

        public OnRunDefaultCommand(OnRunDelegate @delegate)
        {
            this.@delegate = @delegate;
        }

        public int Run(IApplicationState state)
        {
            return @delegate(state);
        }
    }
}
