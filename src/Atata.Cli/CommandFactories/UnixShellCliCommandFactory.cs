namespace Atata.Cli;

/// <summary>
/// Represents the <see cref="CliCommand"/> factory that executes the command through the specified Unix shell program.
/// </summary>
public class UnixShellCliCommandFactory : ShellCliCommandFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnixShellCliCommandFactory"/> class.
    /// </summary>
    /// <param name="shellFileName">Name of the shell file.</param>
    /// <param name="shellArguments">The shell arguments.</param>
    public UnixShellCliCommandFactory(string shellFileName, string? shellArguments = null)
        : base(shellFileName, shellArguments)
    {
    }

    /// <summary>
    /// Escapes the double quotes.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The escaped string.</returns>
    protected static string EscapeDoubleQuotes(string value) =>
        value.Replace("\"", "\\\"");

    /// <inheritdoc/>
    protected override string BuildShellCommandArgument(string command, string? commandArguments)
    {
        string escapedCommandArguments = commandArguments?.Length > 0
            ? EscapeCommandArguments(commandArguments)
            : string.Empty;

        return $"-c \"{command} {escapedCommandArguments}\"";
    }

    /// <summary>
    /// Escapes the command arguments.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The escaped string.</returns>
    protected virtual string EscapeCommandArguments(string value) =>
        EscapeDoubleQuotes(value);
}
