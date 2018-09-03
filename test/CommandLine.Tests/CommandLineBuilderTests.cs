using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.FakeItEasy;
using FakeItEasy;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.CommandLine.Tests
{
    public interface IService { int ReturnCode { get; } }

    public class CommandLineBuilderTests : AutoTestBase
    {
        public CommandLineBuilderTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void Constructs()
        {
            var assemblyProvider = AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

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
            var builder = AutoFake.Resolve<CommandLineBuilder>();

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
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            builder.AddCommand<Remote>("remote");
            builder.AddCommand<Fetch>("fetch");

            var response = builder.Build();

            response.Application.OptionHelp.Should().NotBeNull();

            response.Execute(AutoFake.Resolve<IServiceProvider>(), "remote", "add", "-v").Should().Be(1);
            Logger.LogInformation(response.Application.Commands.Find(x => x.Name == "remote").GetHelpText());
            response.Application.Commands.Find(x => x.Name == "fetch").GetHelpText().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ShouldGetVersion()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            Action a = () => response.Application.ShowVersion();
            a.Should().NotThrow();
        }

        [Fact]
        public void ExecuteWorks()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder
                .OnRun((state) => (int)state.GetLogLevel())
                .Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            response.Execute(AutoFake.Resolve<IServiceProvider>()).Should().Be((int)LogLevel.Information);
        }

        [Fact]
        public void RunWorks()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder
                .OnRun((state) => (int)state.GetLogLevel())
                .Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            response.Execute(AutoFake.Resolve<IServiceProvider>(), "run").Should().Be((int)LogLevel.Information);
        }

        [Theory]
        [InlineData("-v", LogLevel.Trace)]
        [InlineData("-t", LogLevel.Trace)]
        [InlineData("-d", LogLevel.Debug)]
        public void ShouldAllVerbosity(string command, LogLevel level)
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder
                .OnRun((state) => (int)state.GetLogLevel())
                .Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            var result = (LogLevel)response.Execute(AutoFake.Resolve<IServiceProvider>(), command);
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
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder
                .OnRun((state) => (int)state.GetLogLevel())
                .Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            var result = (LogLevel)response.Execute(AutoFake.Resolve<IServiceProvider>(), command.Split(' '));
            result.Should().Be(level);
        }

        [Theory]
        [InlineData("-l invalid")]
        [InlineData("-l ")]
        public void ShouldDisallowInvalidLogLevels(string command)
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            Action a = () => response.Execute(AutoFake.Resolve<IServiceProvider>(), command.Split(' '));
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
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            builder
                .AddCommand<Cmd>("cmd1")
                .AddCommand<Cmd>("cmd2")
                .AddCommand<Cmd>("cmd3")
                .AddCommand<Cmd>("cmd4")
                .AddCommand<Cmd>("cmd5");

            var response = builder
                .Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);
            var result = response.Execute(AutoFake.Resolve<IServiceProvider>(), command.Split(' '));
            result.Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void SupportsAppllicationStateWithCustomDependencyInjection()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var service = A.Fake<IService>();
            A.CallTo(() => service.ReturnCode).Returns(1000);

            var serviceProvider = A.Fake<IServiceProvider>();

            A.CallTo(() => serviceProvider.GetService(A<Type>.Ignored)).Returns(null);
            A.CallTo(() => serviceProvider.GetService(typeof(IService))).Returns(service).NumberOfTimes(2);
            builder.OnRun((state) => 
            {
                state.GetLogLevel().Should().Be(LogLevel.Error);
                return 1000;
            });
            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            var result = response.Execute(AutoFake.Resolve<IServiceProvider>(), "--log", "error");

            result.Should().Be(1000);
        }

        [Command()]

        public class InjectionConstructor
        {
            private readonly IService _service;

            public InjectionConstructor(IService service)
            {
                _service = service;
            }

            public async Task<int> OnExecuteAsync()
            {
                await Task.Yield();
                return _service.ReturnCode;
            }
        }

        [Command()]

        public class InjectionExecute
        {
            public InjectionExecute()
            {
            }

            public async Task<int> OnExecuteAsync(IService service)
            {
                await Task.Yield();
                return service.ReturnCode;
            }
        }

        [Fact]
        public void SupportsInjection_Without_Creating_The_SubContainer()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            var sp = AutoFake.Resolve<IServiceProvider>();

            builder
                .AddCommand<InjectionConstructor>("constructor")
                .AddCommand<InjectionExecute>("execute");

            builder.OnParse((state) => { state.IsDefaultCommand.Should().BeFalse(); });

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            var result = response.Parse("constructor");
            A.CallTo(() => sp.GetService(A<Type>._)).MustNotHaveHappened();
        }

        [Fact]
        public void SupportsInjection_Creating_On_Construction()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            builder
                .AddCommand<InjectionConstructor>("constructor")
                .AddCommand<InjectionExecute>("execute");

            builder.OnParse((state) => { state.IsDefaultCommand.Should().BeFalse(); });

            var service = AutoFake.Resolve<IService>();
            A.CallTo(() => service.ReturnCode).Returns(1000);

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            var result = response.Execute(new AutofacServiceProvider(AutoFake.Container), "constructor");
            result.Should().Be(1000);
            A.CallTo(() => service.ReturnCode).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void SupportsInjection_Creating_On_Execute()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder>();

            builder
                .AddCommand<InjectionConstructor>("constructor")
                .AddCommand<InjectionExecute>("execute");

            var service = AutoFake.Resolve<IService>();
            A.CallTo(() => service.ReturnCode).Returns(1000);

            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            var result = response.Execute(new AutofacServiceProvider(AutoFake.Container), "constructor");
            result.Should().Be(1000);
            A.CallTo(() => service.ReturnCode).MustHaveHappened(1, Times.Exactly);
        }

        [Command()]
        class CommandWithValues
        {
            [Option("--api-domain", CommandOptionType.SingleValue, Description = "The auth0 Domain")]
            public string ApiDomain { get; }

            [Option("--client-name", CommandOptionType.SingleValue, Description = "The client name to create or update")]
            public string ClientName { get; }

            [Option("--origin", CommandOptionType.MultipleValue, Description =
                "The origins that are allowed to access the client")]
            public IEnumerable<string> Origins { get; set; } = Enumerable.Empty<string>();

            public int OnExecute()
            {
                ApiDomain.Should().Be("mydomain.com");
                ClientName.Should().Be("client1");
                Origins.Should().Contain("origin1");
                Origins.Should().Contain("origin2");
                return -1;
            }
        }

        [Fact]
        public void Sets_Values_In_Commands()
        {
            AutoFake.Provide<IAssemblyProvider>(new TestAssemblyProvider());
            var builder = AutoFake.Resolve<CommandLineBuilder>();
            builder.AddCommand<CommandWithValues>("cwv");
            var response = builder.Build(typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);
            response.Execute(new AutofacServiceProvider(AutoFake.Container), "cwv", "--api-domain", "mydomain.com",
                "--origin", "origin1", "--origin", "origin2", "--client-name", "client1");
        }
    }
}
