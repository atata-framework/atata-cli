namespace Atata.Cli;

/// <summary>
/// Represents the program CLI.
/// </summary>
public class ProgramCli
{
    private static readonly ICliCommandFactory s_directCliCommandFactory = new DirectCliCommandFactory();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgramCli"/> class.
    /// </summary>
    /// <param name="fileNameOrCommand">The file name or command.</param>
    /// <param name="useCommandShell">If set to <see langword="true"/> uses command shell (cmd/bash).</param>
    public ProgramCli(string fileNameOrCommand, bool useCommandShell = false)
        : this(
            fileNameOrCommand,
            useCommandShell ? DefaultShellCliCommandFactory : s_directCliCommandFactory)
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
        CliCommandFactory = commandFactory.CheckNotNull(nameof(commandFactory));
    }

    /// <summary>
    /// Gets or sets the default shell <see cref="ICliCommandFactory"/> instance.
    /// The default value is <see cref="OSDependentShellCliCommandFactory.UseCmdForWindowsAndShForOthers"/>.
    /// </summary>
    public static ICliCommandFactory DefaultShellCliCommandFactory { get; set; } =
        OSDependentShellCliCommandFactory.UseCmdForWindowsAndShForOthers();

    /// <summary>
    /// Gets the program file name or command.
    /// </summary>
    public string FileNameOrCommand { get; }

    /// <summary>
    /// Gets or sets the CLI command factory.
    /// </summary>
    public ICliCommandFactory CliCommandFactory { get; set; }

    /// <summary>
    /// Gets a value indicating whether to use command shell (cmd, bash, etc.).
    /// </summary>
    public bool UseCommandShell =>
        CliCommandFactory is ShellCliCommandFactory;

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
    public List<Action<ProcessStartInfo>> ProcessStartInfoConfigurationActions { get; } = [];

    /// <summary>
    /// Gets or sets the command result validation rules that are performed in
    /// <see cref="Execute(string)"/> and <see cref="ExecuteAsync(string)"/>
    /// methods and produce <see cref="CliCommandException"/> throwing.
    /// The default value is <see cref="CliCommandResultValidationRules.ZeroExitCode"/>.
    /// </summary>
    public CliCommandResultValidationRules ResultValidationRules { get; set; } = CliCommandResultValidationRules.ZeroExitCode;

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
    /// Sets the CLI command factory.
    /// </summary>
    /// <param name="cliCommandFactory">The CLI command factory.</param>
    /// <returns>The same instance.</returns>
    public ProgramCli WithCliCommandFactory(ICliCommandFactory cliCommandFactory)
    {
        CliCommandFactory = cliCommandFactory.CheckNotNull(nameof(cliCommandFactory));
        return this;
    }

    /// <summary>
    /// Sets the working directory.
    /// </summary>
    /// <param name="workingDirectory">The working directory.</param>
    /// <returns>The same instance.</returns>
    public ProgramCli WithWorkingDirectory(string workingDirectory)
    {
        WorkingDirectory = workingDirectory.CheckNotNullOrWhitespace(nameof(workingDirectory));
        return this;
    }

    /// <summary>
    /// Starts the program with the specified arguments.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The started <see cref="CliCommand"/> instance.</returns>
    public CliCommand Start(string arguments = null)
    {
        CliCommand command = CliCommandFactory.Create(FileNameOrCommand, arguments);
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
    /// Throws <see cref="CliCommandException"/> if program result doesn't meet validation rules
    /// of <see cref="ResultValidationRules"/> property.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The <see cref="CliCommandResult"/> instance.</returns>
    public CliCommandResult Execute(string arguments = null)
    {
        CliCommandResult result = ExecuteRaw(arguments);
        ValidateResult(result);
        return result;
    }

    private void ValidateResult(CliCommandResult result)
    {
        if ((ResultValidationRules.HasFlag(CliCommandResultValidationRules.ZeroExitCode) && result.ExitCode != 0)
            || (ResultValidationRules.HasFlag(CliCommandResultValidationRules.NoError) && result.HasError))
            throw CliCommandException.CreateForErrorResult(result);
    }

    /// <inheritdoc cref="Execute(string)"/>
    public async Task<CliCommandResult> ExecuteAsync(string arguments = null) =>
        await Task.Run(() => Execute(arguments));

    /// <summary>
    /// Starts the program with the specified arguments and waits until it exits.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The <see cref="CliCommandResult"/> instance.</returns>
    public CliCommandResult ExecuteRaw(string arguments = null)
    {
        using var command = Start(arguments);

        return command.WaitForExit(WaitForExitTimeout);
    }

    /// <inheritdoc cref="ExecuteRaw(string)"/>
    public async Task<CliCommandResult> ExecuteRawAsync(string arguments = null) =>
        await Task.Run(() => ExecuteRaw(arguments));
}
