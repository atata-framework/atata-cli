﻿namespace Atata.Cli
{
    /// <summary>
    /// Represents the <see cref="CliCommand"/> factory that executes the command through the Unix sudo shell program.
    /// </summary>
    public class SudoShellCliCommandFactory : UnixShellCliCommandFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SudoShellCliCommandFactory"/> class.
        /// </summary>
        /// <param name="shellArguments">The shell arguments.</param>
        public SudoShellCliCommandFactory(string shellArguments = null)
            : base("sudo", shellArguments)
        {
        }
    }
}
