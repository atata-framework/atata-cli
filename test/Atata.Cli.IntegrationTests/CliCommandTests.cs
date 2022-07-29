using System;
using System.IO;
using NUnit.Framework;

namespace Atata.Cli.IntegrationTests
{
    [TestFixture]
    public class CliCommandTests
    {
        [Test]
        public void Start_WithMissingDirectory()
        {
            using var command = new CliCommand("dotnet", "--version");
            command.StartInfo.WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString());

            var sut = command.ToSutSubject();

            sut.ResultOf(x => x.Start())
                .Should.Throw<CliCommandException>()
                .ValueOf(x => x.Message).Should.MatchAny(
                    TermMatch.Contains,
                    "The directory name is invalid.",
                    "No such file or directory.")
                .ValueOf(x => x.Message).Should.EndWith(
@$"
CLI command: dotnet --version
Working directory: {command.StartInfo.WorkingDirectory}")
                .ValueOf(x => x.InnerException).Should.Not.BeNull();
        }

        [Test]
        public void Start_WithMissingFileName()
        {
            using var command = new CliCommand("somemissingprogram");
            var sut = command.ToSutSubject();

            sut.ResultOf(x => x.Start())
                .Should.Throw<CliCommandException>()
                .ValueOf(x => x.Message).Should.MatchAny(
                    TermMatch.Contains,
                    "The system cannot find the file specified.",
                    "No such file or directory.")
                .ValueOf(x => x.Message).Should.EndWith(
@$"
CLI command: somemissingprogram
Working directory: {command.StartInfo.WorkingDirectory}")
                .ValueOf(x => x.InnerException).Should.Not.BeNull();
        }

        [Test]
        public void Start_WithInvalidArguments()
        {
            using var sut = new CliCommand("dotnet", "--unknownarg");

            sut.ToSutSubject()
                .Act(x => x.Start())
                .ResultOf(x => x.WaitForExit(null))

                .ValueOf(x => x.ExitCode).Should.Not.Equal(0)
                .ValueOf(x => x.Error).Should.Not.BeNullOrWhiteSpace();
        }

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

        [TestCase(true)]
        [TestCase(false)]
        public void Kill(bool entireProcessTree)
        {
            using var sut = new CliCommand("dotnet", "help");

            sut.ToSutSubject()
                .Act(x => x.Start())
                .ResultOf(x => x.Kill(entireProcessTree))

                .ValueOf(x => x.ExitCode).Should.Not.Equal(0)
                .ValueOf(x => x.Output).Should.BeEmpty()
                .ValueOf(x => x.Error).Should.BeEmpty();
        }
    }
}
