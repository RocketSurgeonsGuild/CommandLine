using System;
using McMaster.Extensions.CommandLineUtils;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public interface ICommandLineExecutor
    {
        int Execute(IServiceProvider serviceProvider);
        bool IsDefaultCommand { get; }
        CommandLineApplication Application { get; }
        IApplicationState ApplicationState { get; }
    }
}
