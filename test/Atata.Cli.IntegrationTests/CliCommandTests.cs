using NUnit.Framework;

namespace Atata.Cli.IntegrationTests
{
    [TestFixture]
    public class CliCommandTests
    {
        [Test]
        public void WaitForExit()
        {
            using var sut = new CliCommand("dotnet", "--version");

            sut.ToSutSubject()
                .Act(x => x.Start())
                .ResultOf(x => x.WaitForExit(null))

                .ValueOf(x => x.ExitCode).Should.Equal(0)
                .ValueOf(x => x.Output).Should.Contain(".")
                .ValueOf(x => x.Error).Should.BeEmpty();
        }

        [Test]
        public void Kill()
        {
            using var sut = new CliCommand("dotnet", "help");

            sut.ToSutSubject()
                .Act(x => x.Start())
                .ResultOf(x => x.Kill())

                .ValueOf(x => x.ExitCode).Should.Not.Equal(0)
                .ValueOf(x => x.Output).Should.BeEmpty()
                .ValueOf(x => x.Error).Should.BeEmpty();
        }
    }
}
