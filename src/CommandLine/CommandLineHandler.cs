using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public class CommandLineHandler
    {
        public CommandLineApplication Application { get; }

        internal CommandLineHandler(CommandLineApplication application)
        {
            Application = application;
        }

        public int Execute(params string[] args)
        {
            return Application.Execute(args);
        }
    }
}
