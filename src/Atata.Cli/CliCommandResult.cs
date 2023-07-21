namespace Atata.Cli;

/// <summary>
/// Represents the result of CLI command.
/// </summary>
public class CliCommandResult
{
    /// <summary>
    /// Gets the text of the command that was executed.
    /// </summary>
    public string CommandText { get; internal set; }

    /// <summary>
    /// Gets the working directory.
    /// </summary>
    public string WorkingDirectory { get; internal set; }

    /// <summary>
    /// Gets the command exit code.
    /// </summary>
    public int ExitCode { get; internal set; }

    /// <summary>
    /// Gets the command output.
    /// </summary>
    public string Output { get; internal set; }

    /// <summary>
    /// Gets the command error.
    /// </summary>
    public string Error { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the result has an error.
    /// </summary>
    public bool HasError =>
        !string.IsNullOrEmpty(Error);
}
