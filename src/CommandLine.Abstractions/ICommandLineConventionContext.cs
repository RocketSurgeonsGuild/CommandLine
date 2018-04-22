using System;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Hosting;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public interface ICommandLineConventionContext : IConventionContext
    {
        IAssemblyProvider AssemblyProvider { get; }
        IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
        IConventionBuilder CommandLineApplicationConventions { get; }
        ICommandLineConventionContext AddCommand<T>(string name, Action<CommandLineApplication<T>> action = null, bool throwOnUnexpectedArg = true) where T : class;
    }
}
