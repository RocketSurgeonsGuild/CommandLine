using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
    class Application : ApplicationCore
    {
        private readonly IApplicationState _applicationState;

        public Application(IApplicationState applicationState)
        {
            _applicationState = applicationState;
        }

        public override async Task<int> OnExecuteAsync()
        {
            await Task.Yield();
            return (int)_applicationState.GetLogLevel();
        }
    }

    class ServiceApplication : ApplicationCore
    {
        private readonly IApplicationState _applicationState;
        private readonly IService _service;

        public ServiceApplication(IApplicationState applicationState, IService service)
        {
            _applicationState = applicationState;
            _service = service;
        }

        public override async Task<int> OnExecuteAsync()
        {
            await Task.Yield();
            return _service.ReturnCode;
        }
    }

    public interface IService { int ReturnCode { get; } }

    public class CommandLineBuilderTests : AutoTestBase
    {
        public CommandLineBuilderTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void Constructs()
        {
            var assemblyProvider = AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder<Application>>();

            builder.AssemblyProvider.Should().BeSameAs(assemblyProvider);
            builder.AssemblyCandidateFinder.Should().NotBeNull();
            Action a = () => { builder.PrependConvention(A.Fake<ICommandLineConvention>()); };
            a.Should().NotThrow();
            a = () => { builder.AppendConvention(A.Fake<ICommandLineConvention>()); };
            a.Should().NotThrow();
            a = () => { builder.PrependDelegate(delegate { }); };
            a.Should().NotThrow();
            a = () => { builder.AppendDelegate(delegate { }); };
            a.Should().NotThrow();
        }

        [Fact]
        public void BuildsALogger()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder<Application>>();

            Action a = () => builder.Build();
            a.Should().NotThrow();
        }

        [Command(), Subcommand("add", typeof(Add))]
        class Remote { public int OnExecute() => 1; }

        [Command()]
        class Add { public int OnExecute() => 1; }

        [Command(), Subcommand("origin", typeof(Origin))]
        class Fetch { public int OnExecute() => 2; }

        [Command()]
        class Origin { public int OnExecute() => 2; }

        [Fact]
        public void ShouldEnableHelpOnAllCommands()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder<Application>>();

            builder.AddCommand<Remote>("remote");
            builder.AddCommand<Fetch>("fetch");

            var response = builder.Build();

            response.Application.OptionHelp.Should().NotBeNull();

            response.Execute("remote", "add", "-v").Should().Be(1);
            Logger.LogInformation(response.Application.Commands.Find(x => x.Name == "remote").GetHelpText());
            response.Application.Commands.Find(x => x.Name == "fetch").GetHelpText().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ShouldGetVersion()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder<Application>>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            Action a = () => response.Application.ShowVersion();
            a.Should().NotThrow();
        }

        [Fact]
        public void ExecuteWorks()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder<Application>>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            response.Execute().Should().Be((int)LogLevel.Information);
        }

        [Fact]
        public void RunWorks()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder<Application>>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            response.Execute("run").Should().Be((int)LogLevel.Information);
        }

        [Theory]
        [InlineData("-v", LogLevel.Trace)]
        [InlineData("-t", LogLevel.Trace)]
        [InlineData("-d", LogLevel.Debug)]
        public void ShouldAllVerbosity(string command, LogLevel level)
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder<Application>>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            var result = (LogLevel)response.Execute(command);
            result.Should().Be(level);
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
            var builder = AutoFake.Resolve<CommandLineBuilder<Application>>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            var result = (LogLevel)response.Execute(command.Split(' '));
            result.Should().Be(level);
        }

        [Theory]
        [InlineData("-l invalid")]
        [InlineData("-l ")]
        public void ShouldDisallowInvalidLogLevels(string command)
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder<Application>>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            Action a = () => response.Execute(command.Split(' '));
            a.Should().Throw<CommandParsingException>();
        }

        [Command(),
         Subcommand("a", typeof(SubCmd))]
        class Cmd { public int OnExecute() => -1; }
        class SubCmd { public int OnExecute() => -1; }

        [Theory]
        [InlineData("--version")]
        [InlineData("--help")]
        [InlineData("run --help")]
        [InlineData("cmd1 --help")]
        [InlineData("cmd1 a --help")]
        [InlineData("cmd2 --help")]
        [InlineData("cmd2 a --help")]
        [InlineData("cmd3 --help")]
        [InlineData("cmd3 a --help")]
        [InlineData("cmd4 --help")]
        [InlineData("cmd4 a --help")]
        [InlineData("cmd5 --help")]
        [InlineData("cmd5 a --help")]
        public void StopsForHelp(string command)
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder<Application>>();

            builder
                .AddCommand<Cmd>("cmd1")
                .AddCommand<Cmd>("cmd2")
                .AddCommand<Cmd>("cmd3")
                .AddCommand<Cmd>("cmd4")
                .AddCommand<Cmd>("cmd5");

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);
            var result = response.Execute(command.Split(' '));
            result.Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void SupportsCustomDependencyInjection()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder<ServiceApplication>>();

            var service = A.Fake<IService>();
            A.CallTo(() => service.ReturnCode).Returns(1000);
            var serviceProvider = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProvider.GetService(typeof(IService))).Returns(service);
            builder.WithServiceProvider(() => serviceProvider);
            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            var result = response.Execute();

            result.Should().Be(1000);
        }
    }
}
