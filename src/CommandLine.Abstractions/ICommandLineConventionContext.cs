using System;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    ///  ICommandLineConventionContext
    /// Implements the <see cref="IConventionContext" />
    /// </summary>
    /// <seealso cref="IConventionContext" />
    public interface ICommandLineConventionContext : IConventionContext
    {
        /// <summary>
        /// Gets the assembly provider.
        /// </summary>
        /// <value>The assembly provider.</value>
        IAssemblyProvider AssemblyProvider { get; }

        /// <summary>
        /// Gets the assembly candidate finder.
        /// </summary>
        /// <value>The assembly candidate finder.</value>
        IAssemblyCandidateFinder AssemblyCandidateFinder { get; }

        /// <summary>
        /// Gets the command line application conventions.
        /// </summary>
        /// <value>The command line application conventions.</value>
        IConventionBuilder CommandLineApplicationConventions { get; }

        /// <summary>
        /// Adds the command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="throwOnUnexpectedArg">if set to <c>true</c> [throw on unexpected argument].</param>
        /// <returns>ICommandLineBuilder.</returns>
        ICommandLineConventionContext AddCommand<T>(Action<CommandLineApplication<T>> action = null, bool throwOnUnexpectedArg = true) where T : class;

        /// <summary>
        /// Adds the command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <param name="throwOnUnexpectedArg">if set to <c>true</c> [throw on unexpected argument].</param>
        /// <returns>ICommandLineConventionContext.</returns>
        ICommandLineConventionContext AddCommand<T>(string name, Action<CommandLineApplication<T>> action = null, bool throwOnUnexpectedArg = true) where T : class;

        /// <summary>
        /// Called when [parse].
        /// </summary>
        /// <param name="delegate">The delegate.</param>
        /// <returns>ICommandLineConventionContext.</returns>
        ICommandLineConventionContext OnParse(OnParseDelegate @delegate);

        /// <summary>
        /// Called when [run].
        /// </summary>
        /// <param name="delegate">The delegate.</param>
        /// <returns>ICommandLineConventionContext.</returns>
        ICommandLineConventionContext OnRun(OnRunDelegate @delegate);

        /// <summary>
        /// Called when [run].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>ICommandLineConventionContext.</returns>
        ICommandLineConventionContext OnRun<T>() where T : IDefaultCommand;
    }
}
