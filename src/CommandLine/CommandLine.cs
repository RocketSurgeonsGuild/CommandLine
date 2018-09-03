using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public class CommandLine : ICommandLine
    {
        private readonly CommandLineBuilder _commandLineBuilder;
        private readonly ILogger _logger;

        internal CommandLine(CommandLineBuilder commandLineBuilder, CommandLineApplication application, ILogger logger)
        {
            _commandLineBuilder = commandLineBuilder;
            _logger = logger;
            Application = application;
        }

        public CommandLineApplication Application { get; }

        public ICommandLineExecutor Parse(params string[] args)
        {
            _logger.LogTrace("Parsing {@Args}", args);
            var result = Application.Parse(args);

            var parent = result.SelectedCommand;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }

            ApplicationState myState = null;
            if (parent is IModelAccessor ma && ma.GetModel() is ApplicationState state)
            {
                state.IsDefaultCommand = result.SelectedCommand is IModelAccessor m && m.GetModelType() == typeof(ApplicationState) || result.SelectedCommand.IsShowingInformation;

                if (state.OnParseDelegates == null) state.OnParseDelegates = new List<OnParseDelegate>();
                foreach (var d in state.OnParseDelegates)
                    d(state);
                myState = state;
            }

            _logger.LogTrace("Selected Command {@Command} {@State}",
                new
                {
                    result.SelectedCommand.FullName,
                    result.SelectedCommand.Name,
                    result.SelectedCommand.Description
                }, 
                new
                {
                    myState.RemainingArguments,
                    myState.Trace,
                    myState.Verbose,
                    myState.Log.Level
                }
            );

            var executor = new CommandLineExecutor(result.SelectedCommand, myState);
            _commandLineBuilder.LinkExecutor(executor);
            return executor;
        }

        public int Execute(IServiceProvider serviceProvider, params string[] args)
        {
            _logger.LogTrace("Executing {@Args}", args);
            return Parse(args).Execute(serviceProvider);
        }
    }
}
