namespace Atata.Cli;

/// <summary>
/// Specifies the behavior for killing processes when disposing a CLI command.
/// </summary>
public enum CliCommandKillOnDispose
{
    /// <summary>
    /// Kills nothing.
    /// </summary>
    None,

    /// <summary>
    /// Kills only the main process.
    /// </summary>
    OnlyProcess,

    /// <summary>
    /// Kills the entire process tree.
    /// </summary>
    EntireProcessTree
}
