namespace Atata.Cli.IntegrationTests;

// TODO: After Atata v4 upgrade. Remove usage of: .GetAwaiter().GetResult()
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
        using var command = new CliCommand("somemissingprogram");
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
        using var sut = new CliCommand("dotnet", "--unknownarg");

        sut.ToSutSubject()
            .Act(x => x.Start())
            .ResultOf(x => x.WaitForExit(null))

            .ValueOf(x => x.ExitCode).Should.Not.Be(0)
            .ValueOf(x => x.Error).Should.Not.BeNullOrWhiteSpace();
    }

    [Test]
    public void WaitForExit()
    {
        using var sut = new CliCommand("dotnet", "--version");

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
        using var sut = OSDependentShellCliCommandFactory.UseCmdForWindowsAndShForOthers()
            .Create("sleep 1", null);

        var subject = sut.ToSutSubject()
            .Act(x => x.Start());

        subject.ResultOf(x => x.WaitForExitAsync(CancellationToken.None).GetAwaiter().GetResult())
            .ValueOf(x => x.ExitCode).Should.Be(0);

        subject.ValueOf(x => x.Process.HasExited)
            .Should.BeTrue();
    }

    [Test]
    public void WaitForExitAsync_WithCancel()
    {
        using var sut = OSDependentShellCliCommandFactory.UseCmdForWindowsAndShForOthers()
            .Create("sleep 10", null);

        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromMilliseconds(300));

        var subject = sut.ToSutSubject()
            .Act(x => x.Start());

        subject.Invoking(x => x.WaitForExitAsync(cancellationTokenSource.Token).GetAwaiter().GetResult())
            .Should.Throw<TaskCanceledException>();

        subject.ValueOf(x => x.Process.HasExited)
            .Should.BeFalse();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Kill(bool entireProcessTree)
    {
        using var sut = OSDependentShellCliCommandFactory.UseCmdForWindowsAndShForOthers()
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
        using var sut = OSDependentShellCliCommandFactory.UseCmdForWindowsAndShForOthers()
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
        using var sut = OSDependentShellCliCommandFactory.UseCmdForWindowsAndShForOthers()
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
}
