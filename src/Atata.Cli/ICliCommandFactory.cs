namespace Atata.Cli;

/// <summary>
/// Provides an interface of <see cref="CliCommand"/> factory.
/// </summary>
public interface ICliCommandFactory
{
    /// <summary>
    /// Creates the <see cref="CliCommand"/> by the specified parameters.
    /// </summary>
    /// <param name="fileNameOrCommand">The file name or command.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The created <see cref="CliCommand"/> instance.</returns>
    CliCommand Create(string fileNameOrCommand, string arguments);
}
