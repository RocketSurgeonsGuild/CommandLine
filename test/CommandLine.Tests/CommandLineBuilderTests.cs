using System;
using System.Reflection;
using FakeItEasy;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.CommandLine.Tests
{
    public class CommandLineBuilderTests : AutoTestBase
    {
        public CommandLineBuilderTests(ITestOutputHelper outputHelper) : base(outputHelper){}

        [Fact]
        public void Constructs()
        {
            var assemblyProvider = AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            AutoFake.Provide(new CommandLineApplication());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            builder.AssemblyProvider.Should().BeSameAs(assemblyProvider);
            builder.AssemblyCandidateFinder.Should().NotBeNull();
            builder.Application.Should().NotBeNull();
            Action a = () => { builder.AddConvention(A.Fake<ICommandLineConvention>()); };
            a.Should().NotThrow();
            a = () => { builder.AddDelegate(delegate { }); };
            a.Should().NotThrow();
        }

        [Fact]
        public void BuildsALogger()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            AutoFake.Provide(new CommandLineApplication());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            Action a = () => builder.Build();
            a.Should().NotThrow();
        }

        [Fact]
        public void ShouldEnableHelpOnAllCommands()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            AutoFake.Provide(new CommandLineApplication());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var child1 = builder.Application
                .Command("remote", c => c.OnExecute(() => 1))
                .Command("add", c => c.OnExecute(() => 1));
            var child2 = builder.Application
                .Command("fetch", c => c.OnExecute(() => 2))
                .Command("origin", c => c.OnExecute(() => 2));

            var response = builder.Build();

            response.Application.OptionHelp.Should().NotBeNull();

            response.Execute("remote", "add", "-v").Should().Be(1);
            Logger.LogInformation(child2.GetHelpText());
            child2.GetHelpText().Should().NotBeNullOrEmpty();
            response.LogLevel.Should().Be(LogLevel.Trace);
        }

        [Fact]
        public void ShouldGetVersion()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            AutoFake.Provide(new CommandLineApplication());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            Action a = () => response.Application.ShowVersion();
            a.Should().NotThrow();
        }

        [Fact]
        public void ExecuteWorks()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            AutoFake.Provide(new CommandLineApplication());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            response.Execute().Should().Be(null);
        }

        [Fact]
        public void RunWorks()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            AutoFake.Provide(new CommandLineApplication());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            response.Execute("run").Should().Be(null);
        }

        [Theory]
        [InlineData("-v", LogLevel.Trace)]
        [InlineData("-t", LogLevel.Trace)]
        [InlineData("-d", LogLevel.Debug)]
        public void ShouldAllVerbosity(string command, LogLevel level)
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var cla = new CommandLineApplication();
            AutoFake.Provide(cla);
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            cla.OnExecute(() => 0);
            var result = response.Execute(command);
            result.Should().Be(0);

            response.LogLevel.Should().Be(level);
        }

        [Theory]
        [InlineData("-l debug", LogLevel.Debug)]
        [InlineData("-l verbose", LogLevel.Trace)]
        [InlineData("-l nonE", LogLevel.None)]
        [InlineData("-l Information", LogLevel.Information)]
        [InlineData("-l Error", LogLevel.Error)]
        [InlineData("-l WARNING", LogLevel.Warning)]
        [InlineData("-l critical", LogLevel.Critical)]
        public void ShouldAllowLogLevelIn(string command, LogLevel level)
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var cla = new CommandLineApplication();
            AutoFake.Provide(cla);
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            cla.OnExecute(() => 0);
            var result = response.Execute(command.Split(' '));
            result.Should().Be(0);

            response.LogLevel.Should().Be(level);
        }

        [Theory]
        [InlineData("-l invalid")]
        [InlineData("-l ")]
        public void ShouldDisallowInvalidLogLevels(string command)
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var cla = new CommandLineApplication();
            AutoFake.Provide(cla);
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            cla.OnExecute(() => 0);
            response.Execute(command.Split(' ')).Should().Be(1);
        }

        [Fact]
        public void DefaultToGivenLogLevel()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var cla = new CommandLineApplication();
            AutoFake.Provide(cla);
            var builder = AutoFake.Resolve<CommandLineBuilder>();
            builder.LogLevel = LogLevel.None;

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            cla.OnExecute(() => 0);
            var result = response.Execute();
            result.Should().Be(0);

            response.LogLevel.Should().Be(LogLevel.None);
        }
    }
}
