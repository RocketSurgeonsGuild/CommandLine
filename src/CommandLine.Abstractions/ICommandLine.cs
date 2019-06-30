using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// Interface ICommandLine
    /// </summary>
    public interface ICommandLine
    {
        /// <summary>
        /// Gets the application.
        /// </summary>
        /// <value>The application.</value>
        CommandLineApplication Application { get; }
        /// <summary>
        /// Parses the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>ICommandLineExecutor.</returns>
        ICommandLineExecutor Parse(params string[] args);
        /// <summary>
        /// Executes the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Int32.</returns>
        int Execute(IServiceProvider serviceProvider, params string[] args);
    }
}
