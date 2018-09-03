using System;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public interface ICommandLineConventionContext : IConventionContext
    {
        //IAssemblyProvider AssemblyProvider { get; }
        //IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
        IConventionBuilder CommandLineApplicationConventions { get; }
        ICommandLineConventionContext AddCommand<T>(string name, Action<CommandLineApplication<T>> action = null, bool throwOnUnexpectedArg = true) where T : class;
        ICommandLineConventionContext OnParse(OnParseDelegate @delegate);
        ICommandLineConventionContext OnRun(OnRunDelegate @delegate);
        ICommandLineConventionContext WithService<S>(S value);
    }
}
