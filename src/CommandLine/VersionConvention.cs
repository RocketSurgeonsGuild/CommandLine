using System.Reflection;
using McMaster.Extensions.CommandLineUtils.Conventions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    class VersionConvention : IConvention
    {
        private readonly Assembly _entryAssembly;

        public VersionConvention(Assembly entryAssembly)
        {
            _entryAssembly = entryAssembly;
        }

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
