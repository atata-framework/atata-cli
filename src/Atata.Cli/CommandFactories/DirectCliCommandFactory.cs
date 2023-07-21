namespace Atata.Cli;

/// <summary>
/// Represents the direct <see cref="CliCommand"/> factory.
/// </summary>
public class DirectCliCommandFactory : ICliCommandFactory
{
    /// <inheritdoc/>
    public CliCommand Create(string fileNameOrCommand, string arguments) =>
        new(fileNameOrCommand, arguments);
}
