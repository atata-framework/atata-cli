using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Atata.Cli.IntegrationTests
{
    [TestFixture(true)]
    [TestFixture(false)]
    public class ProgramCliWithUseCommandShellTests
    {
        private readonly bool _useCommandShell;

        public ProgramCliWithUseCommandShellTests(bool useCommandShell)
        {
            _useCommandShell = useCommandShell;
        }

        [Test]
        public void Execute_WithValidArguments()
        {
            var sut = new ProgramCli("dotnet", _useCommandShell).ToSutSubject();

            sut.ResultOf(x => x.Execute("--version"))
                .ValueOf(x => x.ExitCode).Should.Equal(0)
                .ValueOf(x => x.Output).Should.Contain(".")
                .ValueOf(x => x.Error).Should.BeEmpty();
        }

        [Test]
        public void Execute_WithStringArgument()
        {
            var sut = new ProgramCli("dotnet", _useCommandShell)
            {
                WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../")
            }.ToSutSubject();

            string projectName = Assembly.GetAssembly(GetType()).GetName().Name;
            string arguments = $"restore \"{projectName}.csproj\"";

            sut.ResultOf(x => x.Execute(arguments))
                .ValueOf(x => x.ExitCode).Should.Equal(0)
                .ValueOf(x => x.Output).Should.Contain("restore...")
                .ValueOf(x => x.Error).Should.BeEmpty();
        }

        [Test]
        public void Execute_WithInvalidArguments()
        {
            var sut = new ProgramCli("dotnet", _useCommandShell);

            var exception = Assert.Throws<CliCommandException>(() =>
                sut.Execute("--unknownflag"));

            exception.ToSubject(nameof(exception))
                .ValueOf(x => x.Message).Should.Contain("dotnet --unknownflag");
        }

        [Test]
        public void Execute_ForMissingCli()
        {
            var sut = new ProgramCli("somemissingprogram", _useCommandShell);

            Assert.Throws<CliCommandException>(() =>
                sut.Execute());
        }
    }
}
