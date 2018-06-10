using System;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public interface ICommandLineExecutor
    {
        int Execute(IServiceProvider serviceProvider);
        bool IsDefaultCommand { get; }
        IApplicationState ApplicationState { get; }
    }
}
