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
            var assemblyProvider = new TestAssemblyProvider();
            var assemblyCandidateFinder = A.Fake<IAssemblyCandidateFinder>();
            var scanner = A.Fake<IConventionScanner>();
            var configuration = A.Fake<IConfiguration>();
            var builder = new CommandLineBuilder(
                scanner,
                assemblyProvider,
                assemblyCandidateFinder,
                A.Fake<IHostingEnvironment>(),
                configuration,
                new CommandLineApplication());

            builder.AssemblyProvider.Should().BeSameAs(assemblyProvider);
            builder.AssemblyCandidateFinder.Should().NotBeNull();
            builder.Configuration.Should().BeSameAs(configuration);
            builder.Environment.Should().NotBeNull();
            builder.Application.Should().NotBeNull();
            Action a = () => { builder.AddConvention(A.Fake<ICommandLineConvention>()); };
            a.Should().NotThrow();
            a = () => { builder.AddDelegate(delegate { }); };
            a.Should().NotThrow();
        }

        [Fact]
        public void BuildsALogger()
        {
            var assemblyProvider = new TestAssemblyProvider();
            var assemblyCandidateFinder = A.Fake<IAssemblyCandidateFinder>();
            var scanner = A.Fake<IConventionScanner>();
            var configuration = A.Fake<IConfiguration>();
            var builder = new CommandLineBuilder(
                scanner,
                assemblyProvider,
                assemblyCandidateFinder,
                A.Fake<IHostingEnvironment>(),
                configuration,
                new CommandLineApplication());

            Action a = () => builder.Build(Logger);
            a.Should().NotThrow();
        }

        [Fact]
        public void ShouldEnableHelpOnAllCommands()
        {
            var assemblyProvider = new TestAssemblyProvider();
            var assemblyCandidateFinder = A.Fake<IAssemblyCandidateFinder>();
            var scanner = A.Fake<IConventionScanner>();
            var configuration = A.Fake<IConfiguration>();
            var builder = new CommandLineBuilder(
                scanner,
                assemblyProvider,
                assemblyCandidateFinder,
                A.Fake<IHostingEnvironment>(),
                configuration,
                new CommandLineApplication());

            var child1 = builder.Application
                .Command("remote", c => { })
                .Command("add", c => { });
            var child2 = builder.Application
                .Command("fetch", c => { })
                .Command("origin", c => { });

            var response = builder.Build(Logger);

            response.OptionHelp.Should().NotBeNull();
            child1.OptionHelp.Should().NotBeNull();
            child2.OptionHelp.Should().NotBeNull();
        }

        [Fact]
        public void ShouldGetVersion()
        {
            var assemblyProvider = new TestAssemblyProvider();
            var assemblyCandidateFinder = A.Fake<IAssemblyCandidateFinder>();
            var scanner = A.Fake<IConventionScanner>();
            var configuration = A.Fake<IConfiguration>();
            var builder = new CommandLineBuilder(
                scanner,
                assemblyProvider,
                assemblyCandidateFinder,
                A.Fake<IHostingEnvironment>(),
                configuration,
                new CommandLineApplication());

            var response = builder.Build(Logger, typeof(CommandLineBuilderTests).GetTypeInfo().Assembly);

            Action a = () => response.ShowVersion();
            a.Should().NotThrow();
        }
    }
}
