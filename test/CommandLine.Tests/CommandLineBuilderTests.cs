using System;
using System.Linq;
using System.Reflection;
using Autofac.Extras.FakeItEasy;
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
        public CommandLineBuilderTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

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
            Action a = () => response.Execute(command.Split(' '));
            a.Should().Throw<CommandParsingException>();
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

        [Theory]
        [InlineData("--version")]
        [InlineData("--help")]
        [InlineData("run --help")]
        [InlineData("cmd1 --help")]
        [InlineData("cmd1 a --help")]
        [InlineData("cmd1 b --help")]
        [InlineData("cmd1 c --help")]
        [InlineData("cmd1 d --help")]
        [InlineData("cmd1 e --help")]
        [InlineData("cmd2 --help")]
        [InlineData("cmd2 a --help")]
        [InlineData("cmd2 b --help")]
        [InlineData("cmd2 c --help")]
        [InlineData("cmd2 d --help")]
        [InlineData("cmd2 e --help")]
        [InlineData("cmd3 --help")]
        [InlineData("cmd3 a --help")]
        [InlineData("cmd3 b --help")]
        [InlineData("cmd3 c --help")]
        [InlineData("cmd3 d --help")]
        [InlineData("cmd3 e --help")]
        [InlineData("cmd4 --help")]
        [InlineData("cmd4 a --help")]
        [InlineData("cmd4 b --help")]
        [InlineData("cmd4 c --help")]
        [InlineData("cmd4 d --help")]
        [InlineData("cmd4 e --help")]
        [InlineData("cmd5 --help")]
        [InlineData("cmd5 a --help")]
        [InlineData("cmd5 b --help")]
        [InlineData("cmd5 c --help")]
        [InlineData("cmd5 d --help")]
        [InlineData("cmd5 e --help")]
        public void StopsForHelp(string command)
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var cla = new CommandLineApplication();
            AutoFake.Provide(cla);
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var cmd1 = cla.Command("cmd1", application => application.OnExecute(() => -1));
            cmd1.Command("a", application => application.OnExecute(() => -1));
            cmd1.Command("b", application => application.OnExecute(() => -1));
            cmd1.Command("c", application => application.OnExecute(() => -1));
            cmd1.Command("d", application => application.OnExecute(() => -1));
            cmd1.Command("e", application => application.OnExecute(() => -1));
            var cmd2 = cla.Command("cmd2", application => application.OnExecute(() => -1));
            cmd2.Command("a", application => application.OnExecute(() => -1));
            cmd2.Command("b", application => application.OnExecute(() => -1));
            cmd2.Command("c", application => application.OnExecute(() => -1));
            cmd2.Command("d", application => application.OnExecute(() => -1));
            cmd2.Command("e", application => application.OnExecute(() => -1));
            var cmd3 = cla.Command("cmd3", application => application.OnExecute(() => -1));
            cmd3.Command("a", application => application.OnExecute(() => -1));
            cmd3.Command("b", application => application.OnExecute(() => -1));
            cmd3.Command("c", application => application.OnExecute(() => -1));
            cmd3.Command("d", application => application.OnExecute(() => -1));
            cmd3.Command("e", application => application.OnExecute(() => -1));
            var cmd4 = cla.Command("cmd4", application => application.OnExecute(() => -1));
            cmd4.Command("a", application => application.OnExecute(() => -1));
            cmd4.Command("b", application => application.OnExecute(() => -1));
            cmd4.Command("c", application => application.OnExecute(() => -1));
            cmd4.Command("d", application => application.OnExecute(() => -1));
            cmd4.Command("e", application => application.OnExecute(() => -1));
            var cmd5 = cla.Command("cmd5", application => application.OnExecute(() => -1));
            cmd5.Command("a", application => application.OnExecute(() => -1));
            cmd5.Command("b", application => application.OnExecute(() => -1));
            cmd5.Command("c", application => application.OnExecute(() => -1));
            cmd5.Command("d", application => application.OnExecute(() => -1));
            cmd5.Command("e", application => application.OnExecute(() => -1));

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            cla.OnExecute(() => 0);
            var result = response.Execute(command.Split(' '));
            result.Should().Be(0);
        }
    }
}
