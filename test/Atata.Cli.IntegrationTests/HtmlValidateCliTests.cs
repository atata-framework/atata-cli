using System.Text;

namespace Atata.Cli.IntegrationTests;

public class HtmlValidateCliTests
{
    [Test]
    public void Execute()
    {
        ProgramCli cli = new("html-validate", useCommandShell: true)
        {
            ResultValidationRules = CliCommandResultValidationRules.NoError,
            Encoding = Encoding.UTF8
        };

        string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reddit-frontpage.html");

        var result = cli.Execute($"\"{htmlPath}\" -f json");

        int length = result.Output.Length;
        TestContext.WriteLine($"Output length: {length}");
        Assert.That(length, Is.GreaterThan(100_000));
    }
}
