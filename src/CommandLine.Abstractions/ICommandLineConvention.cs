using Rocket.Surgery.Conventions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// Interface ILoggingConvention
    /// Implements the <see cref="Rocket.Surgery.Conventions.IConvention{Rocket.Surgery.Extensions.CommandLine.ICommandLineConventionContext}" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Conventions.IConvention{Rocket.Surgery.Extensions.CommandLine.ICommandLineConventionContext}" />
    public interface ICommandLineConvention : IConvention<ICommandLineConventionContext> { }
}
