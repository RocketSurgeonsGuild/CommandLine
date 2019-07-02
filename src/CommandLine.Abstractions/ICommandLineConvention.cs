using Rocket.Surgery.Conventions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    ///  ILoggingConvention
    /// Implements the <see cref="IConvention{ICommandLineConventionContext}" />
    /// </summary>
    /// <seealso cref="IConvention{ICommandLineConventionContext}" />
    public interface ICommandLineConvention : IConvention<ICommandLineConventionContext> { }
}
