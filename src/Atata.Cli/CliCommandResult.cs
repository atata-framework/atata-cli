namespace Atata.Cli;

/// <summary>
/// Represents the result of CLI command.
/// </summary>
public sealed class CliCommandResult
{
    internal CliCommandResult(
        string commandText,
        string workingDirectory,
        int exitCode,
        string output,
        string error)
    {
        CommandText = commandText;
        WorkingDirectory = workingDirectory;
        ExitCode = exitCode;
        Output = output;
        Error = error;
    }

    /// <summary>
    /// Gets the text of the command that was executed.
    /// </summary>
    public string CommandText { get; }

    /// <summary>
    /// Gets the working directory.
    /// </summary>
    public string WorkingDirectory { get; }

    /// <summary>
    /// Gets the command exit code.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Gets the command output.
    /// </summary>
    public string Output { get; }

    /// <summary>
    /// Gets the command error.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Gets a value indicating whether the result has an error.
    /// </summary>
    public bool HasError =>
        Error?.Length > 0;
}
