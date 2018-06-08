using System;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public interface ICommandLineExecutor
    {
        int Execute(IServiceProvider serviceProvider);
    }
}