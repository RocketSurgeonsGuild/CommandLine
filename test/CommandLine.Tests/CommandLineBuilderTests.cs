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
                .Command("remote", c => { })
                .Command("add", c => { });
            var child2 = builder.Application
                .Command("fetch", c => { })
                .Command("origin", c => { });

            var response = builder.Build();

            response.Application.OptionHelp.Should().NotBeNull();
            child1.OptionHelp.Should().NotBeNull();
            child2.OptionHelp.Should().NotBeNull();
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
    }
}
