using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Hosting;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public interface ICommandLineConventionContext : IConventionContext
    {
        IAssemblyProvider AssemblyProvider { get; }
        IAssemblyCandidateFinder AssemblyCandidateFinder { get; }
        IHostingEnvironment Environment { get; }
        IConfiguration Configuration { get; }
        CommandLineApplication Application { get; }
    }
}
