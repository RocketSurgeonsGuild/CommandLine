using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Conventions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// VersionConvention.
    /// Implements the <see cref="IConvention" />
    /// </summary>
    /// <seealso cref="IConvention" />
    class VersionConvention : IConvention
    {
        private readonly Assembly _entryAssembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionConvention"/> class.
        /// </summary>
        /// <param name="entryAssembly">The entry assembly.</param>
        public VersionConvention(Assembly entryAssembly)
        {
            _entryAssembly = entryAssembly;
        }

        /// <summary>
        /// Applies the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            // TODO: All tagged assembly versions
            context.Application.VersionOption(
                "--version",
                () => _entryAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version,
                () => _entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion
            );
        }
    }
}
