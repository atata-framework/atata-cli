namespace Atata.Cli
{
    /// <summary>
    /// Represents the <see cref="CliCommand"/> base factory class that executes the command through the specified shell program.
    /// </summary>
    public abstract class ShellCliCommandFactory : ICliCommandFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellCliCommandFactory"/> class.
        /// </summary>
        /// <param name="shellFileName">Name of the shell file.</param>
        /// <param name="shellArguments">The shell arguments.</param>
        protected ShellCliCommandFactory(string shellFileName, string shellArguments = null)
        {
            ShellFileName = shellFileName.CheckNotNullOrWhitespace(nameof(shellFileName));
            ShellArguments = shellArguments;
        }

        /// <summary>
        /// Gets the name of the shell file.
        /// </summary>
        public string ShellFileName { get; }

        /// <summary>
        /// Gets the shell arguments.
        /// </summary>
        public string ShellArguments { get; }

        /// <inheritdoc/>
        public CliCommand Create(string fileNameOrCommand, string arguments)
        {
            string shellCommandArgument = BuildShellCommandArgument(fileNameOrCommand, arguments);
            string shellFullArguments = ShellArguments != null
                ? ConcatShellArguments(ShellArguments, shellCommandArgument)
                : shellCommandArgument;

            return new CliCommand(ShellFileName, shellFullArguments);
        }

        /// <summary>
        /// Concatenates the shell arguments.
        /// </summary>
        /// <param name="shellArguments">The shell arguments.</param>
        /// <param name="shellCommandArgument">The shell command argument.</param>
        /// <returns>The full shell arguments string.</returns>
        protected virtual string ConcatShellArguments(string shellArguments, string shellCommandArgument) =>
            $"{shellArguments} {shellCommandArgument}";

        /// <summary>
        /// Builds the shell command argument.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="commandArguments">The command arguments.</param>
        /// <returns>The shell command argument.</returns>
        protected abstract string BuildShellCommandArgument(string command, string commandArguments);
    }
}
