using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public static class ApplicationStateExtensions
    {
        public static IConfigurationBuilder AddApplicationState(this IConfigurationBuilder  builder, IApplicationState state)
        {
            builder.AddInMemoryCollection(new Dictionary<string, string>
            {
                [$"{nameof(ApplicationState)}:{nameof(ApplicationState.Debug)}"] = state.Debug.ToString(),
                [$"{nameof(ApplicationState)}:{nameof(ApplicationState.Trace)}"] = state.Trace.ToString(),
                [$"{nameof(ApplicationState)}:{nameof(ApplicationState.Verbose)}"] = state.Verbose.ToString(),
                [$"{nameof(ApplicationState)}:{nameof(ApplicationState.IsDefaultCommand)}"] = state.IsDefaultCommand.ToString(),
                [$"{nameof(ApplicationState)}:LogLevel"] = state.GetLogLevel().ToString(),
            });
            return builder;
        }
    }
}
