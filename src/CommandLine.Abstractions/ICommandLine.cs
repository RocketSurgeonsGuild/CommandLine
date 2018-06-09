using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public interface ICommandLine
    {
        CommandLineApplication Application { get; }

        ICommandLineExecutor Parse(params string[] args);
        int Execute(IServiceProvider serviceProvider, params string[] args);
    }
}
