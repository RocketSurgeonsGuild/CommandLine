﻿using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Rocket.Surgery.Extensions.CommandLine
{
    public class CommandLine
    {
        private readonly CommandLineBuilder _commandLineBuilder;

        internal CommandLine(CommandLineBuilder commandLineBuilder, CommandLineApplication application)
        {
            _commandLineBuilder = commandLineBuilder;
            Application = application;
        }

        public CommandLineApplication Application { get; }

        public ICommandLineExecutor Parse(params string[] args)
        {
            var result = Application.Parse(args);

            var parent = result.SelectedCommand;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }

            if (parent is IModelAccessor ma && ma.GetModel() is ApplicationState state)
            {
                if (state.OnParseDelegates == null) state.OnParseDelegates = new List<OnParseDelegate>();
                foreach (var d in state.OnParseDelegates)
                    d(state);
            }

            var executor = new CommandLineExecutor(result.SelectedCommand);
            _commandLineBuilder.LinkExecutor(executor);
            return executor;
        }

        public int Execute(IServiceProvider serviceProvider, params string[] args)
        {
            return Parse(args).Execute(serviceProvider);
        }
    }
}