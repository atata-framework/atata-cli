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
                .ValueOf(x => x.Output).Should.Not.BeNullOrWhiteSpace()
                .ValueOf(x => x.Error).Should.BeEmpty();
        }

        [Test]
        public void Execute_WithInvalidArguments()
        {
            var sut = new ProgramCli("dotnet", _useCommandShell);

            var exception = Assert.Throws<CliCommandException>(() =>
                sut.Execute("--unknownflag"));

            exception.ToResultSubject()
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
