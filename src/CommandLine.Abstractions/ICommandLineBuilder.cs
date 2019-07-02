using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    ///  ILoggingConvention
    /// Implements the <see cref="IConventionBuilder{ICommandLineBuilder, ICommandLineConvention, CommandLineConventionDelegate}" />
    /// </summary>
    /// <seealso cref="IConventionBuilder{ICommandLineBuilder, ICommandLineConvention, CommandLineConventionDelegate}" />
    public interface ICommandLineBuilder : IConventionBuilder<ICommandLineBuilder, ICommandLineConvention, CommandLineConventionDelegate>
    {
        /// <summary>
        /// Adds the command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="throwOnUnexpectedArg">if set to <c>true</c> [throw on unexpected argument].</param>
        /// <returns>ICommandLineBuilder.</returns>
        ICommandLineBuilder AddCommand<T>(Action<CommandLineApplication<T>> action = null,
            bool throwOnUnexpectedArg = true) where T : class;

        /// <summary>
        /// Adds the command.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <param name="throwOnUnexpectedArg">if set to <c>true</c> [throw on unexpected argument].</param>
        /// <returns>ICommandLineBuilder.</returns>
        ICommandLineBuilder AddCommand<T>(string name, Action<CommandLineApplication<T>> action = null,
            bool throwOnUnexpectedArg = true) where T : class;

        /// <summary>
        /// Called when [parse].
        /// </summary>
        /// <param name="delegate">The delegate.</param>
        /// <returns>ICommandLineBuilder.</returns>
        ICommandLineBuilder OnParse(OnParseDelegate @delegate);

        /// <summary>
        /// Called when [run].
        /// </summary>
        /// <param name="delegate">The delegate.</param>
        /// <returns>ICommandLineBuilder.</returns>
        ICommandLineBuilder OnRun(OnRunDelegate @delegate);

        /// <summary>
        /// Called when [run].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>ICommandLineBuilder.</returns>
        ICommandLineBuilder OnRun<T>() where T : IDefaultCommand;

        /// <summary>
        /// Builds the specified entry assembly.
        /// </summary>
        /// <param name="entryAssembly">The entry assembly.</param>
        /// <returns>ICommandLine.</returns>
        ICommandLine Build(Assembly entryAssembly = null);
    }
}
