namespace Atata.Cli;

/// <summary>
/// Represents the <see cref="CliCommand"/> factory that executes the command through the Windows cmd shell program.
/// </summary>
public class CmdShellCliCommandFactory : ShellCliCommandFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CmdShellCliCommandFactory"/> class.
    /// </summary>
    /// <param name="shellArguments">The shell arguments.</param>
    public CmdShellCliCommandFactory(string shellArguments = null)
        : base("cmd", shellArguments)
    {
    }

    /// <inheritdoc/>
    protected override string BuildShellCommandArgument(string command, string commandArguments) =>
        $"/c {command} {commandArguments}";
}
