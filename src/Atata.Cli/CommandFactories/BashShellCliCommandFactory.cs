namespace Atata.Cli
{
    /// <summary>
    /// Represents the <see cref="CliCommand"/> factory that executes the command through the Unix Bash shell program.
    /// </summary>
    public class BashShellCliCommandFactory : UnixShellCliCommandFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BashShellCliCommandFactory"/> class.
        /// </summary>
        /// <param name="shellArguments">The shell arguments.</param>
        public BashShellCliCommandFactory(string shellArguments = null)
            : base("bash", shellArguments)
        {
        }
    }
}
