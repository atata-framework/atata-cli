namespace Atata.Cli.IntegrationTests;

public class ProgramCliWithCommandFactoryTests
{
    [Test]
    [Platform(Include = Platforms.Windows)]
    public void CmdShellCliCommandFactory() =>
        ExecuteDotnetVersionCommmand(new CmdShellCliCommandFactory())
            .ValueOf(x => x.CommandText).Should.Be("cmd /c dotnet --version");

    [Test]
    [Platform(Exclude = Platforms.Windows)]
    public void ShShellCliCommandFactory() =>
        ExecuteDotnetVersionCommmand(new ShShellCliCommandFactory())
            .ValueOf(x => x.CommandText).Should.Be("sh -c \"dotnet --version\"");

    [Test]
    [Platform(Exclude = Platforms.Windows)]
    public void BashShellCliCommandFactory() =>
        ExecuteDotnetVersionCommmand(new BashShellCliCommandFactory())
            .ValueOf(x => x.CommandText).Should.Be("bash -c \"dotnet --version\"");

    [Test]
    [Platform(Exclude = Platforms.Windows)]
    public void SudoShellCliCommandFactory() =>
        ExecuteDotnetVersionCommmand(new SudoShellCliCommandFactory())
            .ValueOf(x => x.CommandText).Should.Be("sudo dotnet --version");

    private static Subject<CliCommandResult> ExecuteDotnetVersionCommmand(ICliCommandFactory commandFactory)
    {
        var sut = new ProgramCli("dotnet", commandFactory).ToSutSubject();

        return sut.ResultOf(x => x.Execute("--version"))
            .ValueOf(x => x.ExitCode).Should.Be(0)
            .ValueOf(x => x.Output).Should.Contain(".")
            .ValueOf(x => x.Error).Should.BeEmpty();
    }
}
