using NUnit.Framework;

namespace Atata.Cli.IntegrationTests
{
    [TestFixture]
    public class ProgramCliTests
    {
        [Test]
        public void Execute_WithValidArguments()
        {
            var sut = new ProgramCli("dotnet").ToSutSubject();

            sut.ResultOf(x => x.Execute("--version"))
                .ValueOf(x => x.ExitCode).Should.Equal(0)
                .ValueOf(x => x.Output).Should.Not.BeNullOrWhiteSpace()
                .ValueOf(x => x.Error).Should.BeEmpty();
        }

        [Test]
        public void Execute_WithInvalidArguments()
        {
            var sut = new ProgramCli("dotnet");

            var exception = Assert.Throws<CliCommandException>(() =>
                sut.Execute("--unknownflag"));

            exception.ToResultSubject()
                .ValueOf(x => x.Message).Should.StartWith("CLI command failure: dotnet --unknownflag");
        }

        [Test]
        public void Execute_ForMissingCli_UseCommandShell(
            [Values(true, false)] bool useCommandShell)
        {
            var sut = new ProgramCli("somemissingprogram", useCommandShell);

            Assert.Throws<CliCommandException>(() =>
                sut.Execute());
        }
    }
}
