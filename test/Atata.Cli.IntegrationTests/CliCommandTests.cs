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
            using var sut = new CliCommand("dotnet", "--version");
            sut.StartInfo.WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString());

            var exception = Assert.Throws<CliCommandException>(() =>
                sut.Start());

            exception.ToSubject(nameof(exception))
                .ValueOf(x => x.Message).Should.MatchAny(
                    TermMatch.StartsWith,
                    "The directory name is invalid.",
                    "No such file or directory.")
                .ValueOf(x => x.Message).Should.EndWith(
@$"
CLI command: dotnet --version
Working directory: {sut.StartInfo.WorkingDirectory}")
                .ValueOf(x => x.InnerException).Should.Not.BeNull();
        }

        [Test]
        public void Start_WithMissingFileName()
        {
            using var sut = new CliCommand("somemissingprogram");

            var exception = Assert.Throws<CliCommandException>(() =>
                sut.Start());

            exception.ToSubject(nameof(exception))
                .ValueOf(x => x.Message).Should.MatchAny(
                    TermMatch.StartsWith,
                    "The system cannot find the file specified.",
                    "No such file or directory.")
                .ValueOf(x => x.Message).Should.EndWith(
@$"
CLI command: somemissingprogram
Working directory: {sut.StartInfo.WorkingDirectory}")
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
