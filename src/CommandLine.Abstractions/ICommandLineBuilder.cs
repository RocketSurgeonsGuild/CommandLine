using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// Interface ILoggingConvention
    /// </summary>
    /// TODO Edit XML Comment Template for ILoggingConvention
    public interface ICommandLineBuilder : IConventionBuilder<ICommandLineBuilder, ICommandLineConvention,
        CommandLineConventionDelegate>
    {
        ICommandLineBuilder AddCommand<T>(string name, Action<CommandLineApplication<T>> action = null,
            bool throwOnUnexpectedArg = true) where T : class;
        ICommandLineBuilder OnParse(OnParseDelegate @delegate);
        ICommandLineBuilder OnRun(OnRunDelegate @delegate);
        ICommandLineBuilder WithService<S>(S value);
        ICommandLine Build(Assembly entryAssembly = null);
    }
}
