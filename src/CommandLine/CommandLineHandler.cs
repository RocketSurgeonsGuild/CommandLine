using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public class CommandLineHandler<T>
        where T : ApplicationCore
    {
        public CommandLineApplication<ApplicationState<T>> Application { get; }

        internal CommandLineHandler(CommandLineApplication<ApplicationState<T>> application)
        {
            Application = application;
        }

        public int Execute(params string[] args)
        {
            return Application.Execute(args);
        }
    }
}
