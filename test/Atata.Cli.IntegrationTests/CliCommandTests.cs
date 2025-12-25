namespace Atata.Cli.IntegrationTests;

public sealed class CliCommandTests
{
    [Test]
    public void Start_WithMissingDirectory()
    {
        using CliCommand command = new("dotnet", "--version");
        command.StartInfo.WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString());

        var sut = command.ToSutSubject();

        sut.ResultOf(x => x.Start())
            .Should.Throw<CliCommandException>()
            .ValueOf(x => x.Message).Should.MatchAny(
                TermMatch.Contains,
                "The directory name is invalid.",
                "No such file or directory.")
            .ValueOf(x => x.Message).Should.EndWith(
                string.Join(
                    Environment.NewLine,
                    string.Empty,
                    "CLI command: dotnet --version",
                    $"Working directory: {command.StartInfo.WorkingDirectory}"))
            .ValueOf(x => x.InnerException).Should.Not.BeNull();
    }

    [Test]
    public void Start_WithMissingFileName()
    {
        using CliCommand command = new("somemissingprogram");
        var sut = command.ToSutSubject();

        sut.ResultOf(x => x.Start())
            .Should.Throw<CliCommandException>()
            .ValueOf(x => x.Message).Should.MatchAny(
                TermMatch.Contains,
                "The system cannot find the file specified.",
                "No such file or directory.")
            .ValueOf(x => x.Message).Should.EndWith(
                string.Join(
                    Environment.NewLine,
                    string.Empty,
                    "CLI command: somemissingprogram",
                    $"Working directory: {command.StartInfo.WorkingDirectory}"))
            .ValueOf(x => x.InnerException).Should.Not.BeNull();
    }

    [Test]
    public void Start_WithInvalidArguments()
    {
        using CliCommand sut = new("dotnet", "--unknownarg");

        sut.ToSutSubject()
            .Act(x => x.Start())
            .ResultOf(x => x.WaitForExit(null))

            .ValueOf(x => x.ExitCode).Should.Not.Be(0)
            .ValueOf(x => x.Error).Should.Not.BeNullOrWhiteSpace();
    }

    [Test]
    public void WaitForExit()
    {
        using CliCommand sut = new("dotnet", "--version");

        var subject = sut.ToSutSubject()
            .Act(x => x.Start());

        subject.ResultOf(x => x.WaitForExit(null))
            .ValueOf(x => x.ExitCode).Should.Be(0)
            .ValueOf(x => x.Output).Should.Contain(".")
            .ValueOf(x => x.Error).Should.BeEmpty();

        subject.ValueOf(x => x.Process.HasExited)
            .Should.BeTrue();
    }

    [Test]
    public void WaitForExitAsync()
    {
        using var sut = CreateDefaultFactory()
            .Create("sleep 1", null);

        var subject = sut.ToSutSubject()
            .Act(x => x.Start());

        subject.ResultOf(x => x.WaitForExitAsync(CancellationToken.None))
            .ValueOf(x => x.ExitCode).Should.Be(0);

        subject.ValueOf(x => x.Process.HasExited)
            .Should.BeTrue();
    }

    [Test]
    public void WaitForExitAsync_WithCancel()
    {
        using var sut = CreateDefaultFactory()
            .Create("sleep 10", null);

        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromMilliseconds(300));

        var subject = sut.ToSutSubject()
            .Act(x => x.Start());

        subject.Invoking(x => x.WaitForExitAsync(cancellationTokenSource.Token))
            .Should.Throw<TaskCanceledException>();

        subject.ValueOf(x => x.Process.HasExited)
            .Should.BeFalse();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Kill(bool entireProcessTree)
    {
        using var sut = CreateDefaultFactory()
            .Create("sleep 5", null);

        sut.ToSutSubject()
            .Act(x => x.Start())
            .ResultOf(x => x.Kill(entireProcessTree))

            .ValueOf(x => x.ExitCode).Should.Not.Be(0)
            .ValueOf(x => x.Output).Should.BeEmpty()
            .ValueOf(x => x.Error).Should.BeEmpty();
    }

    [Test]
    public void Dispose()
    {
        using var sut = CreateDefaultFactory()
             .Create("sleep 5", null);

        bool isExited = false;

        sut.Process.Exited += (_, _) => isExited = true;

        var subject = sut.ToSutSubject()
            .Act(x => x.Start())
            .Act(x => x.Dispose());

        Subject.ResultOf(() => isExited)
            .Should.BeTrue();
    }

    [Test]
    public void Dispose_WhenKillOnDisposeIsNone()
    {
        using var sut = CreateDefaultFactory()
             .Create("sleep 5", null);
        sut.KillOnDispose = CliCommandKillOnDispose.None;

        bool isExited = false;

        sut.Process.Exited += (_, _) => isExited = true;

        var subject = sut.ToSutSubject()
            .Act(x => x.Start())
            .Act(x => x.Dispose());

        Subject.ResultOf(() => isExited)
            .Should.BeFalse();
    }

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance")]
    private static ICliCommandFactory CreateDefaultFactory() =>
#if NETFRAMEWORK
        new CmdShellCliCommandFactory();
#else
        OSDependentShellCliCommandFactory.UseCmdForWindowsAndShForOthers();
#endif
}
