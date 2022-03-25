using System;
using System.Diagnostics;
using System.IO;

namespace Atata.Cli
{
    /// <summary>
    /// Represents the base class of specific program CLI.
    /// </summary>
    /// <typeparam name="TCli">The type of the specific CLI class that inherits <see cref="ProgramCli{TCli}"/>.</typeparam>
    public abstract class ProgramCli<TCli> : ProgramCli
        where TCli : ProgramCli<TCli>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramCli{TCli}"/> class.
        /// </summary>
        /// <param name="fileNameOrCommand">The file name or command.</param>
        /// <param name="useCommandShell">If set to <see langword="true"/> uses command shell (cmd/bash).</param>
        protected ProgramCli(string fileNameOrCommand, bool useCommandShell = false)
            : base(fileNameOrCommand, useCommandShell)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramCli{TCli}"/> class.
        /// </summary>
        /// <param name="fileNameOrCommand">The file name or command.</param>
        /// <param name="commandFactory">The command factory.</param>
        protected ProgramCli(string fileNameOrCommand, ICliCommandFactory commandFactory)
            : base(fileNameOrCommand, commandFactory)
        {
        }

        /// <summary>
        /// Creates the <typeparamref name="TCli"/> instance
        /// with <see cref="ProgramCli.WorkingDirectory"/> set to <see cref="AppDomain.BaseDirectory"/> of <see cref="AppDomain.CurrentDomain"/>.
        /// </summary>
        /// <returns>The created <typeparamref name="TCli"/> instance.</returns>
        public static TCli InBaseDirectory() =>
            new TCli
            {
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

        /// <summary>
        /// Creates the <typeparamref name="TCli"/> instance
        /// with <see cref="ProgramCli.WorkingDirectory"/> set to the specified value.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns>The created <typeparamref name="TCli"/> instance.</returns>
        public static TCli InDirectory(string directory)
        {
            directory.CheckNotNullOrWhitespace(nameof(directory));

            return new TCli
            {
                WorkingDirectory = Path.IsPathRooted(directory)
                    ? directory
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directory)
            };
        }

        /// <inheritdoc cref="ProgramCli.AddProcessStartInfoConfiguration(Action{ProcessStartInfo})"/>
        public new TCli AddProcessStartInfoConfiguration(Action<ProcessStartInfo> configurationAction) =>
            (TCli)base.AddProcessStartInfoConfiguration(configurationAction);

        /// <inheritdoc cref="ProgramCli.WithCliCommandFactory(ICliCommandFactory)"/>
        public new TCli WithCliCommandFactory(ICliCommandFactory cliCommandFactory) =>
            (TCli)base.WithCliCommandFactory(cliCommandFactory);

        /// <inheritdoc cref="ProgramCli.WithWorkingDirectory(string)"/>
        public new TCli WithWorkingDirectory(string workingDirectory) =>
            (TCli)base.WithWorkingDirectory(workingDirectory);
    }
}
