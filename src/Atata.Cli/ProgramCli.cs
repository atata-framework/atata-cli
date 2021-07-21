using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Atata.Cli
{
    /// <summary>
    /// Represents the program CLI.
    /// </summary>
    public class ProgramCli
    {
        private static readonly ICliCommandFactory s_shellCliCommandFactory = new ShellCliCommandFactory();

        private static readonly ICliCommandFactory s_directCliCommandFactory = new DirectCliCommandFactory();

        private readonly ICliCommandFactory _commandFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramCli"/> class.
        /// </summary>
        /// <param name="fileNameOrCommand">The file name or command.</param>
        /// <param name="useCommandShell">If set to <see langword="true"/> uses command shell (cmd/bash).</param>
        public ProgramCli(string fileNameOrCommand, bool useCommandShell = false)
            : this(
                fileNameOrCommand,
                useCommandShell ? s_shellCliCommandFactory : s_directCliCommandFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramCli"/> class.
        /// </summary>
        /// <param name="fileNameOrCommand">The file name or command.</param>
        /// <param name="commandFactory">The command factory.</param>
        public ProgramCli(string fileNameOrCommand, ICliCommandFactory commandFactory)
        {
            FileNameOrCommand = fileNameOrCommand.CheckNotNullOrWhitespace(nameof(fileNameOrCommand));
            _commandFactory = commandFactory.CheckNotNull(nameof(commandFactory));
        }

        /// <summary>
        /// Gets the program file name or command.
        /// </summary>
        public string FileNameOrCommand { get; }

        /// <summary>
        /// Gets a value indicating whether to use command shell (cmd/bash).
        /// </summary>
        public bool UseCommandShell { get; }

        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        public string WorkingDirectory { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Gets or sets the encoding to use.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets the wait for exit timeout.
        /// </summary>
        public TimeSpan? WaitForExitTimeout { get; set; }

        /// <summary>
        /// Gets the list of configuration actions of process <see cref="Process.StartInfo"/>.
        /// </summary>
        public List<Action<ProcessStartInfo>> ProcessStartInfoConfigurationActions { get; } = new List<Action<ProcessStartInfo>>();

        /// <summary>
        /// Adds the process <see cref="Process.StartInfo"/> configuration.
        /// </summary>
        /// <param name="configurationAction">The configuration action.</param>
        /// <returns>The same instance.</returns>
        public ProgramCli AddProcessStartInfoConfiguration(Action<ProcessStartInfo> configurationAction)
        {
            configurationAction.CheckNotNull(nameof(configurationAction));

            ProcessStartInfoConfigurationActions.Add(configurationAction);

            return this;
        }

        /// <summary>
        /// Starts the program with the specified arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The started <see cref="CliCommand"/> instance.</returns>
        public CliCommand Start(string arguments = null)
        {
            CliCommand command = _commandFactory.Create(FileNameOrCommand, arguments);
            FillStartInfo(command.StartInfo);

            return command.Start();
        }

        private void FillStartInfo(ProcessStartInfo startInfo)
        {
            startInfo.WorkingDirectory = WorkingDirectory;

            if (Encoding != null)
            {
                startInfo.StandardOutputEncoding = Encoding;
                startInfo.StandardErrorEncoding = Encoding;
            }

            foreach (var configurationAction in ProcessStartInfoConfigurationActions)
                configurationAction.Invoke(startInfo);
        }

        /// <summary>
        /// Starts the program with the specified arguments and waits until it exits.
        /// Throws <see cref="CliCommandException"/> if program result contains an error.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The <see cref="CliCommandResult"/> instance.</returns>
        public CliCommandResult Execute(string arguments = null)
        {
            CliCommandResult result = ExecuteRaw(arguments);

            if (result.HasError)
                throw CliCommandException.Create(result.CommandText, result.Error, result.Output);
            else
                return result;
        }

        /// <inheritdoc cref="Execute(string)"/>
        public async Task<CliCommandResult> ExecuteAsync(string arguments = null) =>
            await Task.Run(() => Execute(arguments));

        /// <summary>
        /// Starts the program with the specified arguments and waits until it exits.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The <see cref="CliCommandResult"/> instance.</returns>
        protected CliCommandResult ExecuteRaw(string arguments = null)
        {
            using (var command = Start(arguments))
            {
                return command.WaitForExit(WaitForExitTimeout);
            }
        }
    }
}
