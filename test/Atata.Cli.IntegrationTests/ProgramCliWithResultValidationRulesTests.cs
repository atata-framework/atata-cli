using NUnit.Framework;

namespace Atata.Cli.IntegrationTests;

public class ProgramCliWithResultValidationRulesTests
{
    private Subject<ProgramCli> _sut;

    [SetUp]
    public void SetUp() =>
        _sut = new ProgramCli("dotnet").ToSutSubject();

    [TestCase(CliCommandResultValidationRules.ZeroExitCode)]
    [TestCase(CliCommandResultValidationRules.NoError)]
    [TestCase(CliCommandResultValidationRules.ZeroExitCodeAndNoError)]
    public void Execute_With(CliCommandResultValidationRules validationRules)
    {
        _sut.Object.ResultValidationRules = validationRules;

        _sut.ResultOf(x => x.Execute("--unknownflag"))
            .Should.Throw<CliCommandException>()
            .ValueOf(x => x.Message).Should.Contain("Exit code: 1");
    }

    [Test]
    public void Execute_WithNone()
    {
        _sut.Object.ResultValidationRules = CliCommandResultValidationRules.None;

        _sut.ResultOf(x => x.Execute("--unknownflag"))
            .ValueOf(x => x.ExitCode).Should.Be(1)
            .ValueOf(x => x.HasError).Should.BeTrue()
            .ValueOf(x => x.Error).Should.Not.BeNullOrEmpty();
    }
}
