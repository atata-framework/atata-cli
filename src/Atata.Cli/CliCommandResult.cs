namespace Atata.Cli;

/// <summary>
/// Represents the result of CLI command.
/// </summary>
public sealed class CliCommandResult
{
    private readonly Func<string> _outputGetter;

    private readonly Func<string> _errorGetter;

    private readonly Func<string> _mergedOutputGetter;

    internal CliCommandResult(
        string commandText,
        string workingDirectory,
        int exitCode,
        Func<string> outputGetter,
        Func<string> errorGetter,
        Func<string> mergedOutputGetter)
    {
        CommandText = commandText;
        WorkingDirectory = workingDirectory;
        ExitCode = exitCode;

        _outputGetter = outputGetter;
        _errorGetter = errorGetter;
        _mergedOutputGetter = mergedOutputGetter;
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
    /// Gets the command standard output (stdout).
    /// </summary>
    public string Output =>
        _outputGetter.Invoke();

    /// <summary>
    /// Gets the command standard error (stderr).
    /// </summary>
    public string Error =>
        _errorGetter.Invoke();

    /// <summary>
    /// Gets the command merged output: <see cref="Output"/> + <see cref="Error"/>.
    /// </summary>
    public string MergedOutput =>
        _mergedOutputGetter.Invoke();

    /// <summary>
    /// Gets a value indicating whether the result has an error.
    /// </summary>
    public bool HasError =>
        Error?.Length > 0;
}
