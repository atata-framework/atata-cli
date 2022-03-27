using System;
using System.Runtime.InteropServices;

namespace Atata.Cli
{
    /// <summary>
    /// Represents the <see cref="CliCommand"/> base factory class that executes the command through the specified shell program.
    /// </summary>
    // TODO: v2. Should be abstract.
    public class ShellCliCommandFactory : ICliCommandFactory
    {
        [Obsolete("Use " + nameof(OSDependentShellCliCommandFactory) + " instead.")] // Obsolete since v1.4.0.
        public ShellCliCommandFactory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellCliCommandFactory"/> class.
        /// </summary>
        /// <param name="shellFileName">Name of the shell file.</param>
        /// <param name="shellArguments">The shell arguments.</param>
        public ShellCliCommandFactory(string shellFileName, string shellArguments = null)
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
            // TODO: v2. Remove 2 below lines.
            if (ShellFileName == null)
                return CreateUsingOldBehavior(fileNameOrCommand, arguments);

            string shellCommandArgument = BuildShellCommandArgument(fileNameOrCommand, arguments);
            string shellFullArguments = ShellArguments != null
                ? ConcatShellArguments(ShellArguments, shellCommandArgument)
                : shellCommandArgument;

            return new CliCommand(ShellFileName, shellFullArguments);
        }

        private static CliCommand CreateUsingOldBehavior(string fileNameOrCommand, string arguments)
        {
            string argumentsPart = string.IsNullOrEmpty(arguments)
                ? string.Empty
                : $" {arguments}";

            (string actualFileName, string actualArguments) = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? ("cmd", $"/c {fileNameOrCommand}{argumentsPart}")
                : ("bash", $"-c \"{fileNameOrCommand}{EscapeDoubleQuotes(argumentsPart)}\"");

            return new CliCommand(actualFileName, actualArguments);
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
        // TODO: v2. Should be abstract.
        protected virtual string BuildShellCommandArgument(string command, string commandArguments) =>
            throw new NotSupportedException();

        // TODO: v2. Should be deleted.
        private static string EscapeDoubleQuotes(string value) =>
            value.Replace("\"", "\\\"");
    }
}
