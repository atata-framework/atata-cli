﻿namespace Atata.Cli;

/// <summary>
/// An exception that occurs during CLI command execution.
/// </summary>
public class CliCommandException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CliCommandException"/> class.
    /// </summary>
    public CliCommandException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CliCommandException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CliCommandException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CliCommandException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public CliCommandException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    internal static CliCommandException CreateForAlreadyStartedCommand(string commandText, string workingDirectory) =>
        Create(
            commandText,
            workingDirectory,
            "The command has already been started.",
            null,
            null,
            null);

    internal static CliCommandException CreateForNotStartedCommand(string commandText, string workingDirectory) =>
        Create(
            commandText,
            workingDirectory,
            "The command was not started.",
            null,
            null,
            null);

    internal static CliCommandException CreateForTimeout(string commandText, string workingDirectory) =>
        Create(
            commandText,
            workingDirectory,
            "Timed out waiting for command to execute.",
            null,
            null,
            null);

    internal static CliCommandException CreateForErrorResult(CliCommandResult result) =>
        Create(
            result.CommandText,
            result.WorkingDirectory,
            result.Error,
            result.Output,
            result.ExitCode,
            null);

    internal static CliCommandException CreateForProcessStartException(string commandText, string workingDirectory, Exception innerException) =>
        Create(
            commandText,
            workingDirectory,
            innerException.Message,
            null,
            null,
            innerException);

    private static CliCommandException Create(
        string commandText,
        string? workingDirectory,
        string error,
        string? output,
        int? exitCode,
        Exception? innerException)
    {
        StringBuilder messageBuilder = new(error);

        if (!error.EndsWith(".", StringComparison.Ordinal))
            messageBuilder.Append('.');

        messageBuilder
            .AppendLine()
            .AppendLine()
            .Append("CLI command: ")
            .Append(commandText);

        if (!string.IsNullOrWhiteSpace(workingDirectory))
            messageBuilder
                .AppendLine()
                .Append("Working directory: ")
                .Append(workingDirectory);

        if (exitCode != null)
            messageBuilder
                .AppendLine()
                .Append("Exit code: ")
                .Append(exitCode);

        if (!string.IsNullOrWhiteSpace(output))
            messageBuilder
                .AppendLine()
                .AppendLine("Output:")
                .Append(output);

        return new CliCommandException(
            messageBuilder.ToString(),
            innerException);
    }
}
