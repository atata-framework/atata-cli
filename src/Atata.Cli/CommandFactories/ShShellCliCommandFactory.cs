namespace Atata.Cli;

/// <summary>
/// Represents the <see cref="CliCommand"/> factory that executes the command through the Unix sh shell program.
/// </summary>
public class ShShellCliCommandFactory : UnixShellCliCommandFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShShellCliCommandFactory"/> class.
    /// </summary>
    /// <param name="shellArguments">The shell arguments.</param>
    public ShShellCliCommandFactory(string shellArguments = null)
        : base("sh", shellArguments)
    {
    }
}
