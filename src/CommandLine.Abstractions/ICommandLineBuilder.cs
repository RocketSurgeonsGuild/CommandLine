using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Hosting;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// Interface ILoggingConvention
    /// </summary>
    /// TODO Edit XML Comment Template for ILoggingConvention
    public interface ICommandLineBuilder : IBuilder, ICommandLineConventionContext
    {
        ICommandLineBuilder AddDelegate(CommandLineConventionDelegate @delegate);
        ICommandLineBuilder AddConvention(ICommandLineConvention convention);
    }
}
